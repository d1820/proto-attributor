using FluentAssertions;
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

            output.Should().NotContain("System.Runtime.Serialization");
            output.Should().NotContain("[DataContract]");
            output.Should().NotContain("[KnownType");
            output.Should().NotContain("[IgnoreDataMember]");
            output.Should().NotContain("[EnumMember]");
            output.Should().NotContain(@"[DataMember(Order = 1, Name=""Test"")]");
            output.Should().NotContain("[DataMember(Order = 2)]");
            output.Should().NotContain(@"DataMember(Name = ""test12"")");

            output.Should().Contain("[Required]");

            output.Should().Contain("[Serializable]");
        }
    }
}
