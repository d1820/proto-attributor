using Microsoft.CodeAnalysis.CSharp;
using ProtoAttributor.Parsers.DataContracts;
using ProtoAttributor.Services;

namespace DataAttributor.Services
{

    public class DataAnnoAttributeService: IDataAnnoAttributeService
    {
        private readonly Microsoft.VisualStudio.OLE.Interop.IServiceProvider _serviceProvider;
        private readonly DataAttributeAdder _adder;
        private readonly DataAttributeRemover _remover;
        private readonly DataAttributeRewriter _rewriter;

        public DataAnnoAttributeService(Microsoft.VisualStudio.OLE.Interop.IServiceProvider sp,
            DataAttributeAdder adder, DataAttributeRemover remover, DataAttributeRewriter rewriter)
        {
            _serviceProvider = sp;
            //TODO: move to an injection
            _adder = adder;
            _remover = remover;
            _rewriter = rewriter;
        }

        public string AddAttributes(string fileContents)
        {
            var tree = CSharpSyntaxTree.ParseText(fileContents);

            var root = tree.GetRoot();
            var rewrittenRoot = _adder.Visit(root);
            return rewrittenRoot.GetText().ToString();
        }

        public string RemoveAttributes(string fileContents)
        {
            var tree = CSharpSyntaxTree.ParseText(fileContents);
            var rewrittenRoot = _remover.Visit(tree.GetRoot());
            return rewrittenRoot.GetText().ToString();
        }

        public string ReorderAttributes(string fileContents)
        {
            var tree = CSharpSyntaxTree.ParseText(fileContents);

            var rewrittenRoot = _rewriter.Visit(tree.GetRoot());
            return rewrittenRoot.GetText().ToString();
        }
    }
}
