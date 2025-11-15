using Shouldly;
using Microsoft.CodeAnalysis.CSharp;
using ProtoAttributor.Parsers.ProtoContracts;
using Xunit;

namespace ProtoAttributor.Tests.ProtoContracts
{
    public class ProtoAttributeRemoverTests: IClassFixture<TestFixure>
    {
        private readonly TestFixure _fixture;

        public ProtoAttributeRemoverTests(TestFixure fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void RemovesProtoAttributesWhenAttributesAlreadyExists()
        {
            var tree = CSharpSyntaxTree.ParseText(_fixture.LoadTestFile(@"./Mocks/TestRemoveAttributes.cs"));
            var rewriter = new ProtoAttributeRemover();

            var rewrittenRoot = rewriter.Visit(tree.GetRoot());

            var output = rewrittenRoot.GetText().ToString();

            output.ShouldNotContain("ProtoBuf");
            output.ShouldNotContain("[ProtoContract]");
            output.ShouldNotContain("[ProtoInclude]");
            output.ShouldNotContain("[ProtoEnum]");
            output.ShouldNotContain("[ProtoIgnore]");
            output.ShouldNotContain(@"[ProtoMember(1, Name=""Test"")]");
            output.ShouldNotContain("[ProtoMember(2)]");

            output.ShouldContain("[Required]");

            output.ShouldContain("[Serializable]");
        }
    }
}
