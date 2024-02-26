using System.IO;
using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using ProtoAttributor.Parsers.ProtoContracts;
using Xunit;

namespace ProtoAttributor.Tests.ProtoContracts
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
            var tree = CSharpSyntaxTree.ParseText(LoadTestFile(@"./Mocks/TestCodeWithProtoAttributes.cs"));
            var protoReader = new ProtoAttributeReader();

            var output = protoReader.GetProtoNextId(tree.GetRoot());

            output.Should().Be(3);
        }
    }
}
