using System;
using Shouldly;
using Microsoft.CodeAnalysis.CSharp;
using ProtoAttributor.Parsers.ProtoContracts;
using Xunit;

namespace ProtoAttributor.Tests.ProtoContracts
{
    public class ProtoAttributeRewriterTests: IClassFixture<TestFixure>
    {
        private readonly TestFixure _fixture;

        public ProtoAttributeRewriterTests(TestFixure fixture)
        {
            _fixture = fixture;
        }

        private readonly string _codeWithAttributes = @"
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
            var tree = CSharpSyntaxTree.ParseText(_codeWithAttributes);
            var rewriter = new ProtoAttributeRewriter();

            var rewrittenRoot = rewriter.Visit(tree.GetRoot());

            var output = rewrittenRoot.GetText().ToString();

            output.ShouldContain("ProtoBuf");
            output.ShouldContain("[ProtoContract]");
            output.ShouldContain(@"[ProtoMember(1, Name=""Test"")]");
            output.ShouldContain("[ProtoMember(2)]");
            output.ShouldContain("[ProtoMember(3)]");
            output.ShouldContain("[ProtoMember(4)]");
        }

        [Fact]
        public void RewritesAttributesWithCorrectOrderWhenFileHasProtoIgnores()
        {
            var tree = CSharpSyntaxTree.ParseText(_fixture.LoadTestFile(@"./Mocks/TestProtoIgnore.cs"));
            var rewriter = new ProtoAttributeRewriter();
            var rewrittenRoot = rewriter.Visit(tree.GetRoot());

            var output = rewrittenRoot.GetText().ToString();
            var source = output.Split(new string[] { " ", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            output.ShouldContain("ProtoBuf");
            output.ShouldContain("[ProtoContract]");
            output.ShouldContain("[ProtoMember(1)]");
            output.ShouldContain("[ProtoMember(2)]");
            output.ShouldContain("[ProtoMember(3)]");
            output.ShouldContain("[ProtoMember(4)]");
            _fixture.AssertOutputContainsCount(source, "[ProtoIgnore]", 2);
        }

        [Fact]
        public void RewritesEnumAttributesFileHasProtoIgnores()
        {
            var tree = CSharpSyntaxTree.ParseText(_fixture.LoadTestFile(@"./Mocks/TestEnumWithProtoIgnore.cs"));
            var rewriter = new ProtoAttributeRewriter();
            var rewrittenRoot = rewriter.Visit(tree.GetRoot());

            var output = rewrittenRoot.GetText().ToString();
            var source = output.Split(new string[] { " ", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            output.ShouldContain("ProtoBuf");
            output.ShouldContain("[ProtoContract]");
            _fixture.AssertOutputContainsCount(source, "[ProtoIgnore]", 2);
            _fixture.AssertOutputContainsCount(source, "[ProtoEnum]", 3);
        }
    }
}
