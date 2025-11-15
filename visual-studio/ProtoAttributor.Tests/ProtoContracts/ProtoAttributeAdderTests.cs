using System;
using Shouldly;
using Microsoft.CodeAnalysis.CSharp;
using ProtoAttributor.Parsers.ProtoContracts;
using Xunit;

namespace ProtoAttributor.Tests.ProtoContracts
{
    public class ProtoAttributeAdderTests: IClassFixture<TestFixure>
    {
        private readonly TestFixure _fixture;

        public ProtoAttributeAdderTests(TestFixure fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void AddsAttributesForProtBuf_WhenClassWithNoBracketNamespace()
        {
            var tree = CSharpSyntaxTree.ParseText(_fixture.LoadTestFile(@"./Mocks/TestClassWithNoBracketNamespace.cs"));
            var rewriter = new ProtoAttributeAdder();

            var rewrittenRoot = rewriter.Visit(tree.GetRoot());

            var output = rewrittenRoot.GetText().ToString();

            var source = output.Split(new string[] { " ", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            output.ShouldContain("using ProtoBuf;");
            output.ShouldContain("[ProtoContract]");
            output.ShouldContain("[ProtoMember(1)]");
            output.ShouldContain("[ProtoMember(2)]");

            _fixture.AssertOutputContainsCount(source, "ProtoMember", 2);
        }

        [Fact]
        public void AddsAttributesForProtBuf_WhenClassWithNoUsings()
        {
            var tree = CSharpSyntaxTree.ParseText(_fixture.LoadTestFile(@"./Mocks/TestClassNoUsings.cs"));
            var rewriter = new ProtoAttributeAdder();

            var rewrittenRoot = rewriter.Visit(tree.GetRoot());

            var output = rewrittenRoot.GetText().ToString();

            var source = output.Split(new string[] { " ", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            output.ShouldContain("using ProtoBuf;");
            output.ShouldContain("[ProtoContract]");
            output.ShouldContain("[ProtoMember(1)]");
            output.ShouldContain("[ProtoMember(2)]");

            _fixture.AssertOutputContainsCount(source, "ProtoMember", 2);
        }

        [Fact]
        public void AddsAttributesForProtBuf()
        {
            var tree = CSharpSyntaxTree.ParseText(_fixture.LoadTestFile(@"./Mocks/TestClassPlain.cs"));
            var rewriter = new ProtoAttributeAdder();

            var rewrittenRoot = rewriter.Visit(tree.GetRoot());

            var output = rewrittenRoot.GetText().ToString();

            var source = output.Split(new string[] { " ", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            output.ShouldContain("ProtoBuf");
            output.ShouldContain("[ProtoContract]");
            output.ShouldContain("[Required]");
            output.ShouldContain("[ProtoMember(1)]");
            output.ShouldContain("[ProtoMember(2)]");

            _fixture.AssertOutputContainsCount(source, "ProtoMember", 2);
            _fixture.AssertOutputContainsCount(source, "Required", 1);
        }

        [Fact]
        public void AddsAttributesForProtBufWherePropDontHaveProtoIgnore()
        {
            var tree = CSharpSyntaxTree.ParseText(_fixture.LoadTestFile(@"./Mocks/TestCodeWithAttributesAndProtoIgnore.cs"));
            var rewriter = new ProtoAttributeAdder();
            var rewrittenRoot = rewriter.Visit(tree.GetRoot());

            var output = rewrittenRoot.GetText().ToString();

            var source = output.Split(new string[] { " ", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            output.ShouldContain("ProtoBuf");
            output.ShouldContain("[ProtoContract]");
            output.ShouldContain("[Required]");
            output.ShouldContain("[ProtoMember(1)]");
            output.ShouldContain("[ProtoMember(2)]");
            output.ShouldContain("[ProtoMember(3)]");

            _fixture.AssertOutputContainsCount(source, "[ProtoMember", 3);
            _fixture.AssertOutputContainsCount(source, "[Required]", 1);
            _fixture.AssertOutputContainsCount(source, "[ProtoIgnore]", 1);
        }

        [Fact]
        public void AddsAttributesAndKeepCommentsInTack()
        {
            var tree = CSharpSyntaxTree.ParseText(_fixture.LoadTestFile(@"./Mocks/TestClassWithXmlComments.cs"));
            var rewriter = new ProtoAttributeAdder();

            var rewrittenRoot = rewriter.Visit(tree.GetRoot());

            var output = rewrittenRoot.GetText().ToString();

            output.ShouldContain("ProtoBuf");
            output.ShouldContain("[ProtoContract]");
            output.ShouldContain("[ProtoMember(1)]");
            output.ShouldContain("[ProtoMember(2)]");

            output.ShouldContain("        /// </value>");
            output.ShouldContain("        [ProtoMember(1)]");
            output.ShouldContain("        public int MyProperty { get; set; }");

            //This verifies spacing is correct
            output.ShouldContain(@"
        /// <summary> Comments not wrapped </summary>
        /// <value> My property. </value>
        [ProtoMember(1)]
        public int MyProperty { get; set; }");
        }

        [Fact]
        public void AddsUsingWhenNoneExist()
        {
            var tree = CSharpSyntaxTree.ParseText(_fixture.LoadTestFile(@"./Mocks/TestClassNoUsings.cs"));
            var rewriter = new ProtoAttributeAdder();

            var rewrittenRoot = rewriter.Visit(tree.GetRoot());

            var output = rewrittenRoot.GetText().ToString();

            output.ShouldContain("ProtoBuf");
            output.ShouldContain("[ProtoContract]");
            output.ShouldContain("[ProtoMember(1)]");
            output.ShouldContain("[ProtoMember(2)]");
        }

        [Fact]
        public void AddsAttributesWithCorrectOrderWhenAttributesAlreadyExists()
        {
            var tree = CSharpSyntaxTree.ParseText(_fixture.LoadTestFile(@"./Mocks/TestCodeWithProtoAttributes.cs"));
            var rewriter = new ProtoAttributeAdder();

            var rewrittenRoot = rewriter.Visit(tree.GetRoot());

            var output = rewrittenRoot.GetText().ToString();

            output.ShouldContain("ProtoBuf");
            output.ShouldContain("[ProtoContract]");
            output.ShouldContain("[ProtoMember(1)]");
            output.ShouldContain("[ProtoMember(2)]");
            output.ShouldContain("[ProtoMember(3)]");
            output.ShouldContain("[ProtoMember(4)]");
        }

        [Fact]
        public void AddsAttributesWithCorrectOrderWhenFileHasWierdFormatting()
        {
            var tree = CSharpSyntaxTree.ParseText(_fixture.LoadTestFile(@"./Mocks/TestWierdFormatting.cs"));
            var rewriter = new ProtoAttributeAdder();
            var rewrittenRoot = rewriter.Visit(tree.GetRoot());

            var output = rewrittenRoot.GetText().ToString();

            output.ShouldContain("ProtoBuf");
            output.ShouldContain("[ProtoContract]");
            output.ShouldContain("[ProtoMember(1)]");
            output.ShouldContain("[ProtoMember(2)]");
        }

        [Fact]
        public void AddsAttributesWithCorrectOrderWhenFileHasProtoIgnores()
        {
            var tree = CSharpSyntaxTree.ParseText(_fixture.LoadTestFile(@"./Mocks/TestProtoIgnore.cs"));
            var rewriter = new ProtoAttributeAdder();
            var rewrittenRoot = rewriter.Visit(tree.GetRoot());

            var output = rewrittenRoot.GetText().ToString();
            var source = output.Split(new string[] { " ", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            output.ShouldContain("ProtoBuf");
            output.ShouldContain("[ProtoContract]");
            output.ShouldContain("[ProtoMember(1)]");
            output.ShouldContain("[ProtoMember(2)]");
            output.ShouldContain("[ProtoMember(14)]");
            output.ShouldContain("[ProtoMember(16)]");
            _fixture.AssertOutputContainsCount(source, "[ProtoIgnore]", 2);
        }

        [Fact]
        public void AddsAttributesWhenFileIsEnum()
        {
            var tree = CSharpSyntaxTree.ParseText(_fixture.LoadTestFile(@"./Mocks/TestEnum.cs"));
            var rewriter = new ProtoAttributeAdder();
            var rewrittenRoot = rewriter.Visit(tree.GetRoot());

            var output = rewrittenRoot.GetText().ToString();
            var source = output.Split(new string[] { " ", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            output.ShouldContain("ProtoBuf");
            output.ShouldContain("[ProtoContract]");
            output.ShouldContain("[ProtoEnum]");
            _fixture.AssertOutputContainsCount(source, "[ProtoEnum]", 5);
        }

        [Fact]
        public void AddsAttributesWhenFileIsEnumWithExistingAttributes()
        {
            var tree = CSharpSyntaxTree.ParseText(_fixture.LoadTestFile(@"./Mocks/TestEnumWithProtoAttributes.cs"));
            var rewriter = new ProtoAttributeAdder();
            var rewrittenRoot = rewriter.Visit(tree.GetRoot());

            var output = rewrittenRoot.GetText().ToString();
            var source = output.Split(new string[] { " ", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            output.ShouldContain("ProtoBuf");
            output.ShouldContain("[ProtoContract]");
            output.ShouldContain("[ProtoEnum]");
            _fixture.AssertOutputContainsCount(source, "[ProtoEnum]", 5);
        }

        [Fact]
        public void AddsAttributesWhenFileIsEnumWithIgnoreAttributes()
        {
            var tree = CSharpSyntaxTree.ParseText(_fixture.LoadTestFile(@"./Mocks/TestEnumWithProtoIgnore.cs"));
            var rewriter = new ProtoAttributeAdder();
            var rewrittenRoot = rewriter.Visit(tree.GetRoot());

            var output = rewrittenRoot.GetText().ToString();
            var source = output.Split(new string[] { " ", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            output.ShouldContain("ProtoBuf");
            output.ShouldContain("[ProtoContract]");
            output.ShouldContain("[ProtoEnum]");
            _fixture.AssertOutputContainsCount(source, "[ProtoEnum]", 3);
            _fixture.AssertOutputContainsCount(source, "[ProtoIgnore]", 2);
        }
    }
}
