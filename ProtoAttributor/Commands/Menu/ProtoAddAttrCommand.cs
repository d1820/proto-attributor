using System;
using System.ComponentModel.Design;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using ProtoAttributor.Services;
using Task = System.Threading.Tasks.Task;

namespace ProtoAttributor.Commands.Menu
{
    /// <summary> Command handler </summary>
    internal sealed class ProtoAddAttrCommand
    {
        /// <summary> Command ID. </summary>
        public const int CommandId = 13;

        /// <summary> Command menu group (command set GUID). </summary>
        public static readonly Guid CommandSet = new Guid("389ac0f4-15c7-4b06-b5be-ab2039d45ef2");

        /// <summary> VS Package that provides this command, not null. </summary>
        private readonly AsyncPackage package;

        private readonly SDTE _SDTEService;
        private readonly IAttributeService _attributeService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProtoAddAttrCommand" /> class. Adds our command handlers
        ///     for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package"> Owner package, not null. </param>
        /// <param name="commandService"> Command service to add command to, not null. </param>
        private ProtoAddAttrCommand(AsyncPackage package, OleMenuCommandService commandService, SDTE SDTEService, IAttributeService attributeService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            _SDTEService = SDTEService;

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(Execute, menuCommandID);
            commandService.AddCommand(menuItem);
            _attributeService = attributeService;
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
                return package;
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
            var SDTE = await package.GetServiceAsync(typeof(SDTE)) as SDTE;
            Instance = new ProtoAddAttrCommand(package, commandService, SDTE, attributeService);
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

            //https://github.com/GregTrevellick/AutoFindReplace/blob/master/AutoFindReplace/VSPackage.cs

            //var formattedRoot = Formatter.Format(newRoot, Formatter.Annotation, document.Project.Workspace);

            //_attributeService.Hello();

            var dte = _SDTEService as DTE;
            if (dte.ActiveDocument != null)
            {
                var textSelection = (TextSelection)dte.ActiveDocument.Selection;
                textSelection.GotoLine(1, true);
                textSelection.SelectAll();
                var contents = textSelection.Text;
                var changedTxt = _attributeService.AddAttributes(contents);
                textSelection.Insert(changedTxt);
                //textSelection.Text = changedTxt;
                textSelection.SmartFormat();
                // var formattedRoot = Formatter.Format(newRoot, Formatter.Annotation, document.Project.Workspace);
            }
        }
    }
}
