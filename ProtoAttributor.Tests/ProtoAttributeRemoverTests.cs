using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using ProtoAttributor.Services;
using System;
using Xunit;

namespace ProtoAttributor.Tests
{

    public class ProtoAttributeRemoverTests: IClassFixture<TestFixure>
    {
        private readonly TestFixure _fixture;

        public ProtoAttributeRemoverTests(TestFixure fixture)
        {
            _fixture = fixture;
        }


        [Fact]
        public void AddsAttributesWithCorrectOrderWhenAttributesAlreadyExists()
        {
            var tree = CSharpSyntaxTree.ParseText(_fixture.LoadTestFile(@"./Mocks/TestRemoveAttributes.cs"));
            var rewriter = new ProtoAttributeRemover();

            var rewrittenRoot = rewriter.Visit(tree.GetRoot());

            var output = rewrittenRoot.GetText().ToString();

            output.Should().NotContain("ProtoBuf");
            output.Should().NotContain("[ProtoContract]");
            output.Should().NotContain("[ProtoInclude]");
            output.Should().NotContain("[ProtoIgnore]");
            output.Should().NotContain(@"[ProtoMember(1, Name=""Test"")]");
            output.Should().NotContain("[ProtoMember(2)]");

            output.Should().Contain("[Required]");

            output.Should().Contain("[Serializable]");
        }
    }

}
