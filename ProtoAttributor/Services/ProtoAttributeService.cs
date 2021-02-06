using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ProtoAttributor.Services
{
    public class ProtoAttributeService : IAttributeService
    {
        private Microsoft.VisualStudio.OLE.Interop.IServiceProvider _serviceProvider;

        private const string ATTRIBUTE_NAME = "ProtoMember";
        private const string CLASS_NAME = "ProtoContract";
        private const string USING_STATEMENT = "ProtoBuf";

        public ProtoAttributeService(Microsoft.VisualStudio.OLE.Interop.IServiceProvider sp)
        {
            _serviceProvider = sp;
        }

        public string AddAttributes(string fileContents)
        {
            var tree = CSharpSyntaxTree.ParseText(fileContents);
            var rewriter = new ProtoAttributeAdder(ATTRIBUTE_NAME, CLASS_NAME, USING_STATEMENT);
            var protoReader = new ProtoAttributeReader(ATTRIBUTE_NAME);
            var root = tree.GetRoot();
            var rewrittenRoot = rewriter.Visit(root, protoReader.GetProtoNextId(root));
            return rewrittenRoot.GetText().ToString();
        }

        public string RemoveAttributes(string fileContents)
        {
            var tree = CSharpSyntaxTree.ParseText(fileContents);
            var rewriter = new ProtoAttributeRemover(ATTRIBUTE_NAME, CLASS_NAME, USING_STATEMENT);

            var rewrittenRoot = rewriter.Visit(tree.GetRoot());
            return rewrittenRoot.GetText().ToString();
        }

        public string ReorderAttributes(string fileContents, int startingIndex = 1)
        {
            var tree = CSharpSyntaxTree.ParseText(fileContents);
            var rewriter = new ProtoAttributeRewriter(ATTRIBUTE_NAME, CLASS_NAME, USING_STATEMENT);

            var rewrittenRoot = rewriter.Visit(tree.GetRoot(), startingIndex);
            return rewrittenRoot.GetText().ToString();
        }
    }
}
