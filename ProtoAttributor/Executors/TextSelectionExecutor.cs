using System;
using System.Threading;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace ProtoAttributor.Executors
{
    public class TextSelectionExecutor
    {
        public void Execute(TextSelection textSelection, Func<string, string> seletionCallback)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            textSelection.GotoLine(1, true);
            textSelection.SelectAll();
            var contents = textSelection.Text;
            var changedTxt = seletionCallback.Invoke(contents);
            textSelection.Insert(changedTxt);
            textSelection.SmartFormat();
            textSelection.GotoLine(1, false);
        }
    }
}
