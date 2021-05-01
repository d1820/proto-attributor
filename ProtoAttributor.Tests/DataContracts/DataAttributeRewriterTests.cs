using System;
using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using ProtoAttributor.Parsers.DataContracts;
using Xunit;

namespace ProtoAttributor.Tests.DataContracts
{
    public class DataAttributeRewriterTests: IClassFixture<TestFixure>
    {
        private readonly TestFixure _fixture;

        public DataAttributeRewriterTests(TestFixure fixture)
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
                                    [DataMember(Order = 10, Name=""Test"")]
                                    public int MyProperty { get; set; }
                                    [DataMember(Order = 20)]
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
            var rewriter = new DataAttributeRewriter();

            var rewrittenRoot = rewriter.Visit(tree.GetRoot());

            var output = rewrittenRoot.GetText().ToString();

            output.Should().Contain("System.Runtime.Serialization");
            output.Should().Contain("[DataContract]");
            output.Should().Contain(@"[DataMember(Order = 1, Name=""Test"")]");
            output.Should().Contain("[DataMember(Order = 2)]");
            output.Should().Contain("[DataMember(Order = 3)]");
            output.Should().Contain("[DataMember(Order = 4)]");
        }

        [Fact]
        public void RewritesAttributesWithCorrectOrderWhenFileHasProtoIgnores()
        {
            var tree = CSharpSyntaxTree.ParseText(_fixture.LoadTestFile(@"./Mocks/TestDataIgnore.cs"));
            var rewriter = new DataAttributeRewriter();
            var rewrittenRoot = rewriter.Visit(tree.GetRoot());

            var output = rewrittenRoot.GetText().ToString();
            var source = output.Split(new string[] { " ", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            output.Should().Contain("System.Runtime.Serialization");
            output.Should().Contain("[DataContract]");
            output.Should().Contain("[DataMember(Order = 1)]");
            output.Should().Contain("[DataMember(Order = 2)]");
            output.Should().Contain("[DataMember(Order = 3)]");
            output.Should().Contain("[DataMember(Order = 4)]");
            _fixture.AssertOutputContainsCount(source, "[IgnoreDataMember]", 2);
        }

        [Fact]
        public void RewritesAttributesWithCorrectOrderWhenAttributeExistsWithoutOrderProperty()
        {
            var tree = CSharpSyntaxTree.ParseText(_fixture.LoadTestFile(@"./Mocks/TestMissingOrderPropertyAndWeirdSpacing.cs"));
            var rewriter = new DataAttributeRewriter();
            var rewrittenRoot = rewriter.Visit(tree.GetRoot());

            var output = rewrittenRoot.GetText().ToString();
            var source = output.Split(new string[] { " ", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            output.Should().Contain("System.Runtime.Serialization");
            output.Should().Contain("[DataContract]");
            output.Should().Contain("[DataMember(Order = 1)]");
            output.Should().Contain("[DataMember(Order = 2)]");
            output.Should().Contain(@"[DataMember(Name =""test"",Order = 3)]");
            output.Should().Contain("[DataMember(Order = 4)]");
            _fixture.AssertOutputContainsCount(source, "[IgnoreDataMember]", 1);
        }

        [Fact]
        public void RewritesEnumAttributesWhenFileHasDataMemeberIgnores()
        {
            var tree = CSharpSyntaxTree.ParseText(_fixture.LoadTestFile(@"./Mocks/TestEnumWithDataMemberIgnore.cs"));
            var rewriter = new DataAttributeRewriter();
            var rewrittenRoot = rewriter.Visit(tree.GetRoot());

            var output = rewrittenRoot.GetText().ToString();
            var source = output.Split(new string[] { " ", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            output.Should().Contain("System.Runtime.Serialization");
            output.Should().Contain("[DataContract]");
            _fixture.AssertOutputContainsCount(source, "[IgnoreDataMember]", 2);
            _fixture.AssertOutputContainsCount(source, "[EnumMember]", 3);
        }

        [Fact]
        public void RewritesAttributesWithOrderPropertyWhenMissing()
        {
            var tree = CSharpSyntaxTree.ParseText(_fixture.LoadTestFile(@"./Mocks/TestCodeWithDataMemberAttributesMissingOrderProperties.cs"));
            var rewriter = new DataAttributeRewriter();
            var rewrittenRoot = rewriter.Visit(tree.GetRoot());

            var output = rewrittenRoot.GetText().ToString();
            var source = output.Split(new string[] { " ", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            output.Should().Contain("System.Runtime.Serialization");
            output.Should().Contain("[DataContract]");
            _fixture.AssertOutputContainsCount(source, "Order", 5); //includes 1 for the name of class
        }
    }
}
