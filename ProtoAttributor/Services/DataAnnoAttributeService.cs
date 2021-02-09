using Microsoft.CodeAnalysis.CSharp;

namespace ProtoAttributor.Services
{

    public class DataAnnoAttributeService: IDataAnnoAttributeService
    {
        private readonly Microsoft.VisualStudio.OLE.Interop.IServiceProvider _serviceProvider;
        private readonly ProtoAttributeAdder _adder;
        private readonly ProtoAttributeReader _protoReader;
        private readonly ProtoAttributeRemover _remover;
        private readonly ProtoAttributeRewriter _rewriter;

        public DataAnnoAttributeService(Microsoft.VisualStudio.OLE.Interop.IServiceProvider sp,
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
