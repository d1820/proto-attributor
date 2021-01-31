using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using ProtoAttributor.Services;
using System;
using Xunit;

namespace ProtoAttributor.Tests
{
    public class AttributeReaderTests
    {
        private string code = @"
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
                                }
                        }
                        ";


        [Fact]
        public void GetsNextIdForProtoMember()
        {
            var tree = CSharpSyntaxTree.ParseText(code);
            var protoReader = new ProtoAttributeReader("ProtoMember");

            var output = protoReader.GetProtoNextId(tree.GetRoot());

            output.Should().Be(2);
        }
    }

}
