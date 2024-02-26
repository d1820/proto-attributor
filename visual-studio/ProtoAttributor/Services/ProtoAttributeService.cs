using Microsoft.CodeAnalysis.CSharp;
using ProtoAttributor.Parsers.ProtoContracts;

namespace ProtoAttributor.Services
{
    public class ProtoAttributeService: IProtoAttributeService
    {
        private readonly Microsoft.VisualStudio.OLE.Interop.IServiceProvider _serviceProvider;
        private readonly ProtoAttributeAdder _adder;
        private readonly ProtoAttributeRemover _remover;
        private readonly ProtoAttributeRewriter _rewriter;

        public ProtoAttributeService(Microsoft.VisualStudio.OLE.Interop.IServiceProvider sp,
            ProtoAttributeAdder adder, ProtoAttributeRemover remover, ProtoAttributeRewriter rewriter)
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
