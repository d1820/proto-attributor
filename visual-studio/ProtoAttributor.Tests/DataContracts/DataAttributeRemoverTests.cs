using Shouldly;
using Microsoft.CodeAnalysis.CSharp;
using ProtoAttributor.Parsers.DataContracts;
using Xunit;

namespace ProtoAttributor.Tests.DataContracts
{
    public class DataAttributeRemoverTests: IClassFixture<TestFixure>
    {
        private readonly TestFixure _fixture;

        public DataAttributeRemoverTests(TestFixure fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void AddsAttributesWithCorrectOrderWhenAttributesAlreadyExists()
        {
            var tree = CSharpSyntaxTree.ParseText(_fixture.LoadTestFile(@"./Mocks/TestRemoveDataAttributes.cs"));
            var rewriter = new DataAttributeRemover();

            var rewrittenRoot = rewriter.Visit(tree.GetRoot());

            var output = rewrittenRoot.GetText().ToString();

            output.ShouldNotContain("System.Runtime.Serialization");
            output.ShouldNotContain("[DataContract]");
            output.ShouldNotContain("[KnownType");
            output.ShouldNotContain("[IgnoreDataMember]");
            output.ShouldNotContain("[EnumMember]");
            output.ShouldNotContain(@"[DataMember(Order = 1, Name=""Test"")]");
            output.ShouldNotContain("[DataMember(Order = 2)]");
            output.ShouldNotContain(@"DataMember(Name = ""test12"")");

            output.ShouldContain("[Required]");

            output.ShouldContain("[Serializable]");
        }
    }
}
