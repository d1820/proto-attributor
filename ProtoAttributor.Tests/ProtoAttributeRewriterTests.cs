using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using ProtoAttributor.Services;
using System;
using Xunit;

namespace ProtoAttributor.Tests
{


    public class ProtoAttributeRewriterTests
    {
        private string codeWithAttributes = @"
                        using System;
                        using Xunit;
                        namespace ProtoAttributor.Tests
                        {
                            [Required]
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
        public void RewritesAttributesWithCorrectOrderWhenAttributesAlreadyExists()
        {
            var tree = CSharpSyntaxTree.ParseText(codeWithAttributes);
            var rewriter = new ProtoAttributeRewriter("ProtoMember", "ProtoContract", "ProtoBuf");

            var rewrittenRoot = rewriter.Visit(tree.GetRoot());

            var output = rewrittenRoot.GetText().ToString();

            output.Should().Contain("ProtoBuf");
            output.Should().Contain("[ProtoContract]");
            output.Should().Contain(@"[ProtoMember(1, Name=""Test"")]");
            output.Should().Contain("[ProtoMember(2)]");
            output.Should().Contain("[ProtoMember(3)]");
            output.Should().Contain("[ProtoMember(4)]");
        }

        [Fact]
        public void RewritesAttributesWithCorrectOrderStartingAtProvidedIndexWhenAttributesAlreadyExists()
        {
            var tree = CSharpSyntaxTree.ParseText(codeWithAttributes);
            var rewriter = new ProtoAttributeRewriter("ProtoMember", "ProtoContract", "ProtoBuf");

            var rewrittenRoot = rewriter.Visit(tree.GetRoot(), 100);

            var output = rewrittenRoot.GetText().ToString();

            output.Should().Contain("ProtoBuf");
            output.Should().Contain("[ProtoContract]");
            output.Should().Contain(@"[ProtoMember(100, Name=""Test"")]");
            output.Should().Contain("[ProtoMember(101)]");
            output.Should().Contain("[ProtoMember(102)]");
            output.Should().Contain("[ProtoMember(103)]");
        }
    }

}
