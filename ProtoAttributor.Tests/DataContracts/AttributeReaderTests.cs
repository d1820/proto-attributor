using System.IO;
using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using ProtoAttributor.Parsers.DataContracts;
using Xunit;

namespace ProtoAttributor.Tests.DataContracts
{
    public class AttributeReaderTests
    {
        public string LoadTestFile(string relativePath)
        {
            return File.ReadAllText(relativePath);
        }

        [Fact]
        public void GetsNextIdForDataMember()
        {
            var tree = CSharpSyntaxTree.ParseText(LoadTestFile(@"./Mocks/TestCodeWithDataMemberAttributes.cs"));
            var protoReader = new DataAttributeReader();

            var output = protoReader.GetDataMemberNextId(tree.GetRoot());

            output.Should().Be(3);
        }
    }
}
