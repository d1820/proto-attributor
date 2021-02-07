using System;
using System.ComponentModel.Design;
using System.Globalization;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using ProtoAttributor.Executors;
using ProtoAttributor.Services;
using Task = System.Threading.Tasks.Task;

namespace ProtoAttributor.Commands.Context
{
    /// <summary> Command handler </summary>
    internal sealed class ProtoRenumberAttrCommand
    {
        /// <summary> Command ID. </summary>
        public const int CommandId = 24;

        /// <summary> Command menu group (command set GUID). </summary>
        public static readonly Guid _commandSet = new Guid("389ac0f4-15c7-4b06-b5be-ab2039d45ef2");

        /// <summary> VS Package that provides this command, not null. </summary>
        private readonly AsyncPackage _package;
        private readonly SDTE _sdteService;
        private readonly IAttributeService _attributeService;
        private readonly TextSelectionExecutor _textSelectionExecutor;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProtoRenumberAttrCommand" /> class. Adds our command
        ///     handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package"> Owner package, not null. </param>
        /// <param name="commandService"> Command service to add command to, not null. </param>
        private ProtoRenumberAttrCommand(AsyncPackage package, OleMenuCommandService commandService, SDTE SDTEService,
            IAttributeService attributeService, TextSelectionExecutor textSelectionExecutor)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
            _sdteService = SDTEService;
            _attributeService = attributeService;
            _textSelectionExecutor = textSelectionExecutor;
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(_commandSet, CommandId);
            var menuItem = new MenuCommand(Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary> Gets the instance of the command. </summary>
        public static ProtoRenumberAttrCommand Instance
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
            var SDTE = await package.GetServiceAsync(typeof(SDTE)) as SDTE;
            var textSelectionExecutor = new TextSelectionExecutor();
            Instance = new ProtoRenumberAttrCommand(package, commandService, SDTE, attributeService, textSelectionExecutor);
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
            if (dte.ActiveDocument != null)
            {
                _textSelectionExecutor.Execute((TextSelection)dte.ActiveDocument.Selection, (contents) => _attributeService.ReorderAttributes(contents));
            }
        }
    }
}
