using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using ProtoAttributor.Services;
using System;
using System.IO;
using Xunit;

namespace ProtoAttributor.Tests
{
    public class AttributeReaderTests
    {

        public string LoadTestFile(string relativePath)
        {
            return File.ReadAllText(relativePath);
        }

        [Fact]
        public void GetsNextIdForProtoMember()
        {
            var tree = CSharpSyntaxTree.ParseText(LoadTestFile(@"./Mocks/TestCodeWithAttributes.cs"));
            var protoReader = new ProtoAttributeReader("ProtoMember");

            var output = protoReader.GetProtoNextId(tree.GetRoot());

            output.Should().Be(3);
        }
    }

}
