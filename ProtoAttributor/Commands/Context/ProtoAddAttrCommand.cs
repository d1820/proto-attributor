using System;
using System.ComponentModel.Design;
using System.Threading;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using ProtoAttributor.Executors;
using ProtoAttributor.Services;
using Task = System.Threading.Tasks.Task;

namespace ProtoAttributor.Commands.Context
{
    /// <summary> Command handler </summary>
    internal sealed class ProtoAddAttrCommand
    {
        /// <summary> Command ID. </summary>
        public const int CommandId = 23;

        /// <summary> Command menu group (command set GUID). </summary>
        public static readonly Guid _commandSet = new Guid("389ac0f4-15c7-4b06-b5be-ab2039d45ef2");

        /// <summary> VS Package that provides this command, not null. </summary>
        private readonly AsyncPackage _package;

        private readonly SDTE _sdteService;
        private readonly IAttributeService _attributeService;
        private readonly TextSelectionExecutor _textSelectionExecutor;
        private readonly IVsThreadedWaitDialogFactory _dialogFactory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProtoAddAttrCommand" /> class. Adds our command handlers
        ///     for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package"> Owner package, not null. </param>
        /// <param name="commandService"> Command service to add command to, not null. </param>
        private ProtoAddAttrCommand(AsyncPackage package, OleMenuCommandService commandService, SDTE SDTEService,
            IAttributeService attributeService, TextSelectionExecutor textSelectionExecutor,
            IVsThreadedWaitDialogFactory dialogFactory)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            _sdteService = SDTEService;
            _attributeService = attributeService;
            _textSelectionExecutor = textSelectionExecutor;
            _dialogFactory = dialogFactory;
            var menuCommandID = new CommandID(_commandSet, CommandId);
            var menuItem = new MenuCommand(Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary> Gets the instance of the command. </summary>
        public static ProtoAddAttrCommand Instance
        {
            get;
            private set;
        }

        /// <summary> Gets the service provider from the owner package. </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return _package;
            }
        }

        /// <summary> Initializes the singleton instance of the command. </summary>
        /// <param name="package"> Owner package, not null. </param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in ProtoCommand's constructor requires the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            var attributeService = await package.GetServiceAsync(typeof(IAttributeService)) as IAttributeService;
            var dialogFactory = await package.GetServiceAsync(typeof(SVsThreadedWaitDialogFactory)) as IVsThreadedWaitDialogFactory;
            var SDTE = await package.GetServiceAsync(typeof(SDTE)) as SDTE;
            var textSelectionExecutor = new TextSelectionExecutor();
            Instance = new ProtoAddAttrCommand(package, commandService, SDTE, attributeService, textSelectionExecutor, dialogFactory);
        }

        /// <summary>
        ///     This function is the callback used to execute the command when the menu item is clicked. See the
        ///     constructor to see how the menu item is associated with this function using OleMenuCommandService
        ///     service and MenuCommand class.
        /// </summary>
        /// <param name="sender"> Event sender. </param>
        /// <param name="e"> Event args. </param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var dte = _sdteService as DTE;

            if (dte.SelectedItems.Count <= 0) return;

            var totalCount = 0;
            foreach (SelectedItem selectedItem in dte.SelectedItems)
            {
                if (selectedItem.ProjectItem == null)
                {
                    continue;
                }
                GetTotalItemCount(selectedItem.ProjectItem, ref totalCount);
            }

            IVsThreadedWaitDialog2 dialog = null;
            if (totalCount > 1 && _dialogFactory != null)
            {
                //https://www.visualstudiogeeks.com/extensions/visualstudio/using-progress-dialog-in-visual-studio-extensions
                _dialogFactory.CreateInstance(out dialog);
            }

            var currentCount = 0;
            bool cancelProcessing = false;
            var cts = new CancellationTokenSource();

            if (dialog == null ||
                dialog.StartWaitDialogWithPercentageProgress("Proto Attributor: Attributing Progress", "", $"{currentCount} of {totalCount} Processed",
                 null, "Attributing", true, 0, totalCount, currentCount) != VSConstants.S_OK)
            {
                dialog = null;
            }

            foreach (SelectedItem selectedItem in dte.SelectedItems)
            {
                dialog?.HasCanceled(out cancelProcessing);
                if (cancelProcessing)
                {
                    cts.Cancel();
                    break;
                }
                if (selectedItem.ProjectItem == null)
                {
                    continue;
                }
                ProcessProjectItem(selectedItem.ProjectItem, cts.Token, (fileName) =>
                {
                    ThreadHelper.ThrowIfNotOnUIThread();
                    currentCount++;
                    dialog?.UpdateProgress($"Annotating: {fileName}", $"{currentCount} of {totalCount} Processed", "Attributing", currentCount, totalCount, false, out cancelProcessing);
                    if (cancelProcessing)
                    {
                        cts.Cancel();
                    }
                }, (fileName) =>
                {
                    ThreadHelper.ThrowIfNotOnUIThread();
                    dialog?.UpdateProgress($"Annotating: {fileName}", $"{currentCount} of {totalCount} Processed", "Attributing", currentCount, totalCount, false, out cancelProcessing);
                    if (cancelProcessing)
                    {
                        cts.Cancel();
                    }
                });
            }

            dialog?.EndWaitDialog(out var usercancel);
        }

        private void GetTotalItemCount(ProjectItem projectItem, ref int count)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (projectItem.Kind == EnvDTE.Constants.vsProjectItemKindPhysicalFile)
            {
                var fullPath = projectItem.Properties.Item("FullPath")?.Value?.ToString();
                if (fullPath?.EndsWith(".cs") == true)
                {
                    count++;
                }
            }
            if (projectItem.Kind == EnvDTE.Constants.vsProjectItemKindPhysicalFolder && projectItem.ProjectItems.Count > 0)
            {
                foreach (ProjectItem item in projectItem.ProjectItems)
                {
                    GetTotalItemCount(item, ref count);
                }
            }
        }

        private void ProcessProjectItem(ProjectItem projectItem, CancellationToken token, Action<string> progressCallback, Action<string> processItemCallback)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (token.IsCancellationRequested)
            {
                return;
            }
            if (projectItem.Kind == EnvDTE.Constants.vsProjectItemKindPhysicalFolder)
            {
                if (projectItem.ProjectItems.Count > 0)
                {
                    foreach (ProjectItem item in projectItem.ProjectItems)
                    {
                        ProcessProjectItem(item, token, progressCallback, processItemCallback);
                    }
                }
                return;
            }
            if (projectItem.Kind == EnvDTE.Constants.vsProjectItemKindPhysicalFile)
            {
                var fullPath = projectItem.Properties.Item("FullPath")?.Value?.ToString();
                var name = projectItem.Name;
                processItemCallback?.Invoke(name);
                var isOpen = projectItem.IsOpen[EnvDTE.Constants.vsViewKindTextView];
                if (!isOpen)
                {
                    if (fullPath?.EndsWith(".cs") == true)
                    {
                        var window = projectItem.Open(EnvDTE.Constants.vsViewKindTextView);
                        window.Activate();
                        //process file
                        if (projectItem.Document != null)
                        {
                            projectItem.Document.Activate();
                            _textSelectionExecutor.Execute((TextSelection)projectItem.Document.Selection, (contents) => _attributeService.AddAttributes(contents));
                        }
                        progressCallback?.Invoke(name);
                    }
                }
                else if (fullPath?.EndsWith(".cs") == true)
                {
                    //process file
                    if (projectItem.Document != null)
                    {
                        projectItem.Document.Activate();
                        _textSelectionExecutor.Execute((TextSelection)projectItem.Document.Selection, (contents) => _attributeService.AddAttributes(contents));
                    }
                    progressCallback?.Invoke(name);
                }
            }
        }

        //Used in testing
        private string DecodeProjectItemKind(string sProjectItemKind)
        {
            string sResult;
            switch (sProjectItemKind ?? "")
            {
                case var @case when @case == EnvDTE.Constants.vsProjectItemKindMisc:
                    {
                        sResult = "EnvDTE.Constants.vsProjectItemKindMisc";
                        break;
                    }

                case var case1 when case1 == EnvDTE.Constants.vsProjectItemKindPhysicalFile:
                    {
                        sResult = "EnvDTE.Constants.vsProjectItemKindPhysicalFile";
                        break;
                    }

                case var case2 when case2 == EnvDTE.Constants.vsProjectItemKindPhysicalFolder:
                    {
                        sResult = "EnvDTE.Constants.vsProjectItemKindPhysicalFolder";
                        break;
                    }

                case var case3 when case3 == EnvDTE.Constants.vsProjectItemKindSolutionItems:
                    {
                        sResult = "EnvDTE.Constants.vsProjectItemKindSolutionItems";
                        break;
                    }

                case var case4 when case4 == EnvDTE.Constants.vsProjectItemKindSubProject:
                    {
                        sResult = "EnvDTE.Constants.vsProjectItemKindSubProject";
                        break;
                    }

                case var case5 when case5 == EnvDTE.Constants.vsProjectItemKindVirtualFolder:
                    {
                        sResult = "EnvDTE.Constants.vsProjectItemKindVirtualFolder";
                        break;
                    }

                default:
                    {
                        sResult = "";
                        break;
                    }
            }

            return sResult;
        }
    }
}
