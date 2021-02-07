using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using ProtoAttributor.Services;
using ProtoBuf;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace ProtoAttributor.Tests
{
    public class ProtoAttributeAdderTests: IClassFixture<TestFixure>
    {
        private readonly TestFixure _fixture;

        public ProtoAttributeAdderTests(TestFixure fixture)
        {
            _fixture = fixture;
        }
        [Fact]
        public void AddsAttributesForProtBuf()
        {
			var tree = CSharpSyntaxTree.ParseText(_fixture.LoadTestFile(@"./Mocks/TestClassPlain.cs"));
            var rewriter = new ProtoAttributeAdder();

            var rewrittenRoot = rewriter.Visit(tree.GetRoot(), 1);

            var output = rewrittenRoot.GetText().ToString();

            var source = output.Split(new string[] { " " , "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            output.Should().Contain("ProtoBuf");
            output.Should().Contain("[ProtoContract]");
            output.Should().Contain("[Required]");
            output.Should().Contain("[ProtoMember(1)]");
            output.Should().Contain("[ProtoMember(2)]");

            _fixture.AssertOutputContainsCount(source, "ProtoMember", 2);
            _fixture.AssertOutputContainsCount(source, "Required", 1);

        }

        [Fact]
        public void AddsAttributesForProtBufWherePropDontHaveProtoIgnore()
        {
            var tree = CSharpSyntaxTree.ParseText(_fixture.LoadTestFile(@"./Mocks/TestCodeWithAttributesAndProtoIgnore.cs"));
            var rewriter = new ProtoAttributeAdder();
            var protoReader = new ProtoAttributeReader();
            var rewrittenRoot = rewriter.Visit(tree.GetRoot(), protoReader.GetProtoNextId(tree.GetRoot()));

            var output = rewrittenRoot.GetText().ToString();

            var source = output.Split(new string[] { " ", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            output.Should().Contain("ProtoBuf");
            output.Should().Contain("[ProtoContract]");
            output.Should().Contain("[Required]");
            output.Should().Contain("[ProtoMember(1)]");
            output.Should().Contain("[ProtoMember(2)]");
            output.Should().Contain("[ProtoMember(3)]");

            _fixture.AssertOutputContainsCount(source, "[ProtoMember", 3);
            _fixture.AssertOutputContainsCount(source, "[Required]", 1);
            _fixture.AssertOutputContainsCount(source, "[ProtoIgnore]", 1);

        }

        [Fact]
        public void AddsAttributesAndKeepCommentsInTack()
        {
            var tree = CSharpSyntaxTree.ParseText(_fixture.LoadTestFile(@"./Mocks/TestClassWithComments.cs"));
            var rewriter = new ProtoAttributeAdder();

            var rewrittenRoot = rewriter.Visit(tree.GetRoot(), 1);

            var output = rewrittenRoot.GetText().ToString();

            output.Should().Contain("ProtoBuf");
            output.Should().Contain("[ProtoContract]");
            output.Should().Contain("[ProtoMember(1)]");
            output.Should().Contain("[ProtoMember(2)]");

            output.Should().Contain("        /// </value>");
            output.Should().Contain("        [ProtoMember(1)]");
            output.Should().Contain("        public int MyProperty { get; set; }");

            //This verifies spacing is correct
            output.Should().Contain(@"
        /// <summary>
        /// Gets or sets my property.
        /// </summary>
        /// <value>
        /// My property.
        /// </value>
        [ProtoMember(1)]
        public int MyProperty { get; set; }");

        }

        [Fact]
        public void AddsUsingWhenNoneExist()
        {
            var tree = CSharpSyntaxTree.ParseText(_fixture.LoadTestFile(@"./Mocks/TestClassNoUsings.cs"));
            var rewriter = new ProtoAttributeAdder();

            var rewrittenRoot = rewriter.Visit(tree.GetRoot(), 1);

            var output = rewrittenRoot.GetText().ToString();

            output.Should().Contain("ProtoBuf");
            output.Should().Contain("[ProtoContract]");
            output.Should().Contain("[ProtoMember(1)]");
            output.Should().Contain("[ProtoMember(2)]");
        }

        [Fact]
        public void AddsAttributesWithCorrectOrderWhenAttributesAlreadyExists()
        {
            var tree = CSharpSyntaxTree.ParseText(_fixture.LoadTestFile(@"./Mocks/TestCodeWithAttributes.cs"));
            var rewriter = new ProtoAttributeAdder();
            var protoReader = new ProtoAttributeReader();

            var rewrittenRoot = rewriter.Visit(tree.GetRoot(), protoReader.GetProtoNextId(tree.GetRoot()));

            var output = rewrittenRoot.GetText().ToString();

            output.Should().Contain("ProtoBuf");
            output.Should().Contain("[ProtoContract]");
            output.Should().Contain("[ProtoMember(1)]");
            output.Should().Contain("[ProtoMember(2)]");
            output.Should().Contain("[ProtoMember(3)]");
            output.Should().Contain("[ProtoMember(4)]");
        }
    }

}
