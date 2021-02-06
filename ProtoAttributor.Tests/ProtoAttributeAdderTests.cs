using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using ProtoAttributor.Services;
using ProtoBuf;
using System;
using System.IO;
using Xunit;

namespace ProtoAttributor.Tests
{
    public class ProtoAttributeAdderTests
    {
        private string code = @"
using System;
using Xunit;
namespace ProtoAttributor.Tests
{
    public class Test
    {
        [Required]
        public int MyProperty { get; set; }
        public int MyProperty2 { get; set; }
    }
}
";




        private string codeWithAttributes = @"
using System;
using Xunit;
namespace ProtoAttributor.Tests
{
    [ProtoContract]
    public class Test
        {
            [Required]
            [ProtoMember(1)]
            public int MyProperty { get; set; }
            [ProtoMember(2)]
            public int MyProperty2 { get; set; }
            public int MyProperty3 { get; set; }
            public int MyProperty4 { get; set; }
        }
}
";
        private string _location;
        private string _dirPath;

        public string LoadTestFile(string relativePath)
        {
            return File.ReadAllText(relativePath);
        }
        [Fact]
        public void AddsAttributesForProtBuf()
        {
			var tree = CSharpSyntaxTree.ParseText(code);
            var rewriter = new ProtoAttributeAdder("ProtoMember", "ProtoContract", "ProtoBuf");

            var rewrittenRoot = rewriter.Visit(tree.GetRoot(), 1);

            var output = rewrittenRoot.GetText().ToString();

            output.Should().Contain("ProtoBuf");
            output.Should().Contain("[ProtoContract]");
            output.Should().Contain("[ProtoMember(1)]");
            output.Should().Contain("[ProtoMember(2)]");
        }

        [Fact]
        public void AddsAttributesAndKeepCommentsInTack()
        {
            var tree = CSharpSyntaxTree.ParseText(LoadTestFile(@"./Mocks/TestClassWithComments.cs"));
            var rewriter = new ProtoAttributeAdder("ProtoMember", "ProtoContract", "ProtoBuf");

            var rewrittenRoot = rewriter.Visit(tree.GetRoot(), 1);

            var output = rewrittenRoot.GetText().ToString();

            output.Should().Contain("ProtoBuf");
            output.Should().Contain("[ProtoContract]");
            output.Should().Contain("[ProtoMember(1)]");
            output.Should().Contain("[ProtoMember(2)]");
        }

        [Fact]
        public void AddsUsingWhenNoneExist()
        {
            var tree = CSharpSyntaxTree.ParseText(LoadTestFile(@"./Mocks/TestClassNoUsings.cs"));
            var rewriter = new ProtoAttributeAdder("ProtoMember", "ProtoContract", "ProtoBuf");

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
            var tree = CSharpSyntaxTree.ParseText(codeWithAttributes);
            var rewriter = new ProtoAttributeAdder("ProtoMember", "ProtoContract", "ProtoBuf");
            var protoReader = new ProtoAttributeReader("ProtoMember");

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
