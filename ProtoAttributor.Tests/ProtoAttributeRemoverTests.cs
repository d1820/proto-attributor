using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using ProtoAttributor.Services;
using System;
using Xunit;

namespace ProtoAttributor.Tests
{

    public class ProtoAttributeRemoverTests
    {
        private string codeWithAttributes = @"
                        using System;
                        using Xunit;
                        using ProtoBuf
                        namespace ProtoAttributor.Tests
                        {
                            [Required]
                            [ProtoContract]
                            [ProtoInclude]
                            [ProtoInclude]
                            public class Test
                                {
                                    [Required]
                                    [ProtoMember(10, Name=""Test"")]
                                    public int MyProperty { get; set; }
                                    [ProtoMember(20)]
                                    public int MyProperty2 { get; set; }

                                    [Required]
                                    public int MyProperty3 { get; set; }
                                    public int MyProperty4 { get; set; }
                                }
                        }
                        ";


        [Fact]
        public void AddsAttributesWithCorrectOrderWhenAttributesAlreadyExists()
        {
            var tree = CSharpSyntaxTree.ParseText(codeWithAttributes);
            var rewriter = new ProtoAttributeRemover("ProtoMember", "ProtoContract", "ProtoBuf");

            var rewrittenRoot = rewriter.Visit(tree.GetRoot());

            var output = rewrittenRoot.GetText().ToString();

            output.Should().NotContain("ProtoBuf");
            output.Should().NotContain("[ProtoContract]");
            output.Should().NotContain("[ProtoInclude]");
            output.Should().NotContain(@"[ProtoMember(1, Name=""Test"")]");
            output.Should().NotContain("[ProtoMember(2)]");
        }
    }

}
