using Microsoft.CodeAnalysis.CSharp;

namespace ProtoAttributor.Services
{
    public class ProtoAttributeService: IAttributeService
    {
        private readonly Microsoft.VisualStudio.OLE.Interop.IServiceProvider _serviceProvider;
        private readonly ProtoAttributeAdder _adder;
        private readonly ProtoAttributeReader _protoReader;
        private readonly ProtoAttributeRemover _remover;
        private readonly ProtoAttributeRewriter _rewriter;

        public ProtoAttributeService(Microsoft.VisualStudio.OLE.Interop.IServiceProvider sp,
            ProtoAttributeAdder adder, ProtoAttributeReader reader, ProtoAttributeRemover remover, ProtoAttributeRewriter rewriter)
        {
            _serviceProvider = sp;
            //TODO: move to an injection
            _adder = adder;
            _protoReader = reader;
            _remover = remover;
            _rewriter = rewriter;
        }

        public string AddAttributes(string fileContents)
        {
            var tree = CSharpSyntaxTree.ParseText(fileContents);

            var root = tree.GetRoot();
            var rewrittenRoot = _adder.Visit(root, _protoReader.GetProtoNextId(root));
            return rewrittenRoot.GetText().ToString();
        }

        public string RemoveAttributes(string fileContents)
        {
            var tree = CSharpSyntaxTree.ParseText(fileContents);
            var rewrittenRoot = _remover.Visit(tree.GetRoot());
            return rewrittenRoot.GetText().ToString();
        }

        public string ReorderAttributes(string fileContents, int startingIndex = 1)
        {
            var tree = CSharpSyntaxTree.ParseText(fileContents);

            var rewrittenRoot = _rewriter.Visit(tree.GetRoot(), startingIndex);
            return rewrittenRoot.GetText().ToString();
        }
    }
}
