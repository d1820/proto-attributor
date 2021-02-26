using System;
using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using ProtoAttributor.Parsers.DataContracts;
using Xunit;

namespace ProtoAttributor.Tests.DataContracts
{
    public class DataAttributeAdderTests: IClassFixture<TestFixure>
    {
        private readonly TestFixure _fixture;

        public DataAttributeAdderTests(TestFixure fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void AddsAttributesForProtoBuf()
        {
            var tree = CSharpSyntaxTree.ParseText(_fixture.LoadTestFile(@"./Mocks/TestClassPlain.cs"));
            var rewriter = new DataAttributeAdder();

            var rewrittenRoot = rewriter.Visit(tree.GetRoot());

            var output = rewrittenRoot.GetText().ToString();

            var source = output.Split(new string[] { " ", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            output.Should().Contain("System.Runtime.Serialization");
            output.Should().Contain("[DataContract]");
            output.Should().Contain("[Required]");
            output.Should().Contain("[DataMember(Order = 1)]");
            output.Should().Contain("[DataMember(Order = 2)]");

            _fixture.AssertOutputContainsCount(source, "DataMember", 2);
            _fixture.AssertOutputContainsCount(source, "Required", 1);
        }

        [Fact]
        public void AddsAttributesForProtBufWherePropDontHaveIgnoreDataMember()
        {
            var tree = CSharpSyntaxTree.ParseText(_fixture.LoadTestFile(@"./Mocks/TestCodeWithAttributesAndDataMemberIgnore.cs"));
            var rewriter = new DataAttributeAdder();
            var rewrittenRoot = rewriter.Visit(tree.GetRoot());

            var output = rewrittenRoot.GetText().ToString();

            var source = output.Split(new string[] { " ", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            output.Should().Contain("System.Runtime.Serialization");
            output.Should().Contain("[DataContract]");
            output.Should().Contain("[Required]");
            output.Should().Contain("[DataMember(Order = 1)]");
            output.Should().Contain("[DataMember(Order = 2)]");
            output.Should().Contain("[DataMember(Order = 3)]");

            _fixture.AssertOutputContainsCount(source, "[DataMember", 3);
            _fixture.AssertOutputContainsCount(source, "[Required]", 1);
            _fixture.AssertOutputContainsCount(source, "[IgnoreDataMember]", 1);
        }

        [Fact]
        public void AddsAttributesAndKeepCommentsInTack()
        {
            var tree = CSharpSyntaxTree.ParseText(_fixture.LoadTestFile(@"./Mocks/TestClassWithXmlComments.cs"));
            var rewriter = new DataAttributeAdder();

            var rewrittenRoot = rewriter.Visit(tree.GetRoot());

            var output = rewrittenRoot.GetText().ToString();

            output.Should().Contain("System.Runtime.Serialization");
            output.Should().Contain("[DataContract]");
            output.Should().Contain("[DataMember(Order = 1)]");
            output.Should().Contain("[DataMember(Order = 2)]");

            output.Should().Contain("        /// </value>");
            output.Should().Contain("        [DataMember(Order = 1)]");
            output.Should().Contain("        public int MyProperty { get; set; }");

            //This verifies spacing is correct
            output.Should().Contain(@"
        /// <summary> Comments not wrapped </summary>
        /// <value> My property. </value>
        [DataMember(Order = 1)]
        public int MyProperty { get; set; }");
        }

        [Fact]
        public void AddsUsingWhenNoneExist()
        {
            var tree = CSharpSyntaxTree.ParseText(_fixture.LoadTestFile(@"./Mocks/TestClassNoUsings.cs"));
            var rewriter = new DataAttributeAdder();

            var rewrittenRoot = rewriter.Visit(tree.GetRoot());

            var output = rewrittenRoot.GetText().ToString();

            output.Should().Contain("System.Runtime.Serialization");
            output.Should().Contain("[DataContract]");
            output.Should().Contain("[DataMember(Order = 1)]");
            output.Should().Contain("[DataMember(Order = 2)]");
        }

        [Fact]
        public void AddsAttributesWithCorrectOrderWhenAttributesAlreadyExists()
        {
            var tree = CSharpSyntaxTree.ParseText(_fixture.LoadTestFile(@"./Mocks/TestCodeWithDataMemberAttributes.cs"));
            var rewriter = new DataAttributeAdder();

            var rewrittenRoot = rewriter.Visit(tree.GetRoot());

            var output = rewrittenRoot.GetText().ToString();

            output.Should().Contain("System.Runtime.Serialization");
            output.Should().Contain("[DataContract]");
            output.Should().Contain("[DataMember(Order = 1)]");
            output.Should().Contain("[DataMember(Order = 2)]");
            output.Should().Contain("[DataMember(Order = 3)]");
            output.Should().Contain("[DataMember(Name = \"Test\")]");
        }

        [Fact]
        public void AddsAttributesWithCorrectOrderWhenFileHasWierdFormatting()
        {
            var tree = CSharpSyntaxTree.ParseText(_fixture.LoadTestFile(@"./Mocks/TestWierdFormatting.cs"));
            var rewriter = new DataAttributeAdder();
            var rewrittenRoot = rewriter.Visit(tree.GetRoot());

            var output = rewrittenRoot.GetText().ToString();

            output.Should().Contain("System.Runtime.Serialization");
            output.Should().Contain("[DataContract]");
            output.Should().Contain("[DataMember(Order = 1)]");
            output.Should().Contain("[DataMember(Order = 2)]");
        }

        [Fact]
        public void AddsAttributesWithCorrectOrderWhenFileHasProtoIgnores()
        {
            var tree = CSharpSyntaxTree.ParseText(_fixture.LoadTestFile(@"./Mocks/TestIgnoreDataMember.cs"));
            var rewriter = new DataAttributeAdder();
            var rewrittenRoot = rewriter.Visit(tree.GetRoot());

            var output = rewrittenRoot.GetText().ToString();
            var source = output.Split(new string[] { " ", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            output.Should().Contain("System.Runtime.Serialization");
            output.Should().Contain("[DataContract]");
            output.Should().Contain("[DataMember(Order = 1)]");
            output.Should().Contain("[DataMember(Order = 2)]");
            output.Should().Contain("[DataMember(Order = 14)]");
            output.Should().Contain("[DataMember(Order = 16)]");
            _fixture.AssertOutputContainsCount(source, "[IgnoreDataMember]", 2);
        }

        [Fact]
        public void AddsAttributesWhenFileIsEnum()
        {
            var tree = CSharpSyntaxTree.ParseText(_fixture.LoadTestFile(@"./Mocks/TestEnum.cs"));
            var rewriter = new DataAttributeAdder();
            var rewrittenRoot = rewriter.Visit(tree.GetRoot());

            var output = rewrittenRoot.GetText().ToString();
            var source = output.Split(new string[] { " ", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            output.Should().Contain("System.Runtime.Serialization");
            output.Should().Contain("[DataContract]");
            output.Should().Contain("[EnumMember]");
            _fixture.AssertOutputContainsCount(source, "[EnumMember]", 5);

        }

        [Fact]
        public void AddsAttributesWhenFileIsEnumWithExistingAttributes()
        {
            var tree = CSharpSyntaxTree.ParseText(_fixture.LoadTestFile(@"./Mocks/TestEnumWithDataAttributes.cs"));
            var rewriter = new DataAttributeAdder();
            var rewrittenRoot = rewriter.Visit(tree.GetRoot());

            var output = rewrittenRoot.GetText().ToString();
            var source = output.Split(new string[] { " ", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            output.Should().Contain("System.Runtime.Serialization");
            output.Should().Contain("[DataContract]");
            output.Should().Contain("[EnumMember]");
            _fixture.AssertOutputContainsCount(source, "[EnumMember]", 5);
        }

        [Fact]
        public void AddsAttributesWhenFileIsEnumWithIgnoreAttributes()
        {
            var tree = CSharpSyntaxTree.ParseText(_fixture.LoadTestFile(@"./Mocks/TestEnumWithDataMemberIgnore.cs"));
            var rewriter = new DataAttributeAdder();
            var rewrittenRoot = rewriter.Visit(tree.GetRoot());

            var output = rewrittenRoot.GetText().ToString();
            var source = output.Split(new string[] { " ", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            output.Should().Contain("System.Runtime.Serialization");
            output.Should().Contain("[DataContract]");
            output.Should().Contain("[EnumMember]");
            _fixture.AssertOutputContainsCount(source, "[EnumMember]", 3);
            _fixture.AssertOutputContainsCount(source, "[IgnoreDataMember]", 2);
        }
    }
}
