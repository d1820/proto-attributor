using Shouldly;
using Microsoft.CodeAnalysis.CSharp;
using ProtoAttributor.Parsers.DataContracts;
using Xunit;

namespace ProtoAttributor.Tests.DataContracts
{
    public class DataAttributeReaderBugTests
    {
        // Bug: DataAttributeReader.VisitPropertyDeclaration calls
        // item.ArgumentList.Arguments.FirstOrDefault(f => f.NameEquals.Name...)
        // without null-guarding ArgumentList. A bare [DataMember] (no parentheses)
        // has ArgumentList == null, causing NullReferenceException.
        [Fact]
        public void Add_DoesNotThrow_WhenExistingDataMemberHasNoArguments()
        {
            var code = @"
using System.Runtime.Serialization;
namespace Test
{
    [DataContract]
    public class Foo
    {
        [DataMember]
        public int Bar { get; set; }
        public int Baz { get; set; }
    }
}";
            var tree = CSharpSyntaxTree.ParseText(code);
            var rewriter = new DataAttributeAdder();

            var exception = Record.Exception(() => rewriter.Visit(tree.GetRoot()));

            exception.ShouldBeNull();
        }

        // Bug: DataAttributeReader's predicate f => f.NameEquals.Name.Identifier.ValueText.Equals("Order")
        // dereferences NameEquals without a null check. A positional argument like [DataMember(1)]
        // has NameEquals == null, causing NullReferenceException when the predicate is evaluated.
        [Fact]
        public void Add_DoesNotThrow_WhenExistingDataMemberHasPositionalArgument()
        {
            var code = @"
using System.Runtime.Serialization;
namespace Test
{
    [DataContract]
    public class Foo
    {
        [DataMember(1)]
        public int Bar { get; set; }
        public int Baz { get; set; }
    }
}";
            var tree = CSharpSyntaxTree.ParseText(code);
            var rewriter = new DataAttributeAdder();

            var exception = Record.Exception(() => rewriter.Visit(tree.GetRoot()));

            exception.ShouldBeNull();
        }

        // Bug: DataAttributeAdder.VisitPropertyDeclaration does not filter static properties.
        // Static properties cannot be serialized by DataContractSerializer,
        // but they receive [DataMember] anyway.
        [Fact]
        public void Add_DoesNotAddAttribute_ToStaticProperty()
        {
            var code = @"
namespace Test
{
    public class Foo
    {
        public static string Version { get; } = ""1.0"";
        public int RegularProp { get; set; }
    }
}";
            var tree = CSharpSyntaxTree.ParseText(code);
            var rewriter = new DataAttributeAdder();
            var rewrittenRoot = rewriter.Visit(tree.GetRoot());
            var output = rewrittenRoot.GetText().ToString();
            var source = output.Split(new string[] { " ", "\r\n" }, System.StringSplitOptions.RemoveEmptyEntries);

            // Only RegularProp should be attributed. Bug: Version also gets [DataMember].
            new TestFixure().AssertOutputContainsCount(source, "DataMember", 1);
        }
    }
}
