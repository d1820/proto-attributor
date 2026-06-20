using Shouldly;
using Microsoft.CodeAnalysis.CSharp;
using ProtoAttributor.Parsers.ProtoContracts;
using Xunit;

namespace ProtoAttributor.Tests.ProtoContracts
{
    public class ProtoAttributeAdderBugTests
    {
        // Bug: ProtoAttributeReader.VisitPropertyDeclaration dereferences
        // item.ArgumentList.Arguments.FirstOrDefault() without null-guarding ArgumentList.
        // A bare [ProtoMember] (no parentheses) has ArgumentList == null, causing NullReferenceException.
        [Fact]
        public void Add_DoesNotThrow_WhenExistingProtoMemberHasNoArguments()
        {
            var code = @"
using ProtoBuf;
namespace Test
{
    [ProtoContract]
    public class Foo
    {
        [ProtoMember]
        public int Bar { get; set; }
        public int Baz { get; set; }
    }
}";
            var tree = CSharpSyntaxTree.ParseText(code);
            var rewriter = new ProtoAttributeAdder();

            var exception = Record.Exception(() => rewriter.Visit(tree.GetRoot()));

            exception.ShouldBeNull();
        }

        // Bug: ProtoAttributeReader.VisitPropertyDeclaration calls value.GetText()
        // where value is the result of FirstOrDefault() on an empty argument list.
        // [ProtoMember()] has empty ArgumentList.Arguments, so FirstOrDefault returns null -> NRE.
        [Fact]
        public void Add_DoesNotThrow_WhenExistingProtoMemberHasEmptyArguments()
        {
            var code = @"
using ProtoBuf;
namespace Test
{
    [ProtoContract]
    public class Foo
    {
        [ProtoMember()]
        public int Bar { get; set; }
        public int Baz { get; set; }
    }
}";
            var tree = CSharpSyntaxTree.ParseText(code);
            var rewriter = new ProtoAttributeAdder();

            var exception = Record.Exception(() => rewriter.Visit(tree.GetRoot()));

            exception.ShouldBeNull();
        }

        // Bug: BaseProtoRewriter.VisitClassDeclaration resets StartIndex for every class it visits,
        // including nested ones. After the inner class is processed, StartIndex is left at the inner
        // class's final value. The outer class's remaining properties continue from that value
        // instead of the outer class's own next index.
        //
        // With 2 inner properties (X, Y), StartIndex after inner = 3.
        // Outer's PropB incorrectly gets [ProtoMember(3)] instead of [ProtoMember(2)].
        [Fact]
        public void Add_AssignsCorrectIndices_WhenClassHasNestedClass()
        {
            var code = @"
namespace Test
{
    public class Outer
    {
        public int PropA { get; set; }
        public class Inner
        {
            public int PropX { get; set; }
            public int PropY { get; set; }
        }
        public int PropB { get; set; }
    }
}";
            var tree = CSharpSyntaxTree.ParseText(code);
            var rewriter = new ProtoAttributeAdder();
            var rewrittenRoot = rewriter.Visit(tree.GetRoot());
            var output = rewrittenRoot.GetText().ToString();

            // Outer class: PropA=1, PropB=2. Inner class: PropX=1, PropY=2.
            // Bug: PropB gets [ProtoMember(3)] because inner's counter bleeds into outer.
            output.ShouldNotContain("[ProtoMember(3)]");
        }

        // Bug: ProtoAttributeReader walks the entire syntax subtree including nested classes
        // when computing the next available index for a class. Existing [ProtoMember] attributes
        // in nested classes inflate the computed max and cause the outer class to start too high.
        //
        // Inner has [ProtoMember(5)]. Reader finds it when scanning Outer, returns 6.
        // Outer's unattributed PropA incorrectly gets [ProtoMember(6)] instead of [ProtoMember(1)].
        [Fact]
        public void Add_DoesNotInflateIndex_FromNestedClassAttributes()
        {
            var code = @"
using ProtoBuf;
namespace Test
{
    public class Outer
    {
        public int PropA { get; set; }
        public class Inner
        {
            [ProtoMember(5)]
            public int PropX { get; set; }
        }
        public int PropB { get; set; }
    }
}";
            var tree = CSharpSyntaxTree.ParseText(code);
            var rewriter = new ProtoAttributeAdder();
            var rewrittenRoot = rewriter.Visit(tree.GetRoot());
            var output = rewrittenRoot.GetText().ToString();

            // Outer's PropA and PropB have no existing attributes so should start at 1.
            // Bug: reader sees Inner's [ProtoMember(5)] and starts Outer at 6.
            output.ShouldContain("[ProtoMember(1)]");
            output.ShouldNotContain("[ProtoMember(6)]");
        }

        // Bug: ProtoAttributeAdder.VisitPropertyDeclaration does not filter static properties.
        // Static properties cannot be serialized by protobuf-net (no instance to read/write),
        // but they receive [ProtoMember] anyway.
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
            var rewriter = new ProtoAttributeAdder();
            var rewrittenRoot = rewriter.Visit(tree.GetRoot());
            var output = rewrittenRoot.GetText().ToString();
            var source = output.Split(new string[] { " ", "\r\n" }, System.StringSplitOptions.RemoveEmptyEntries);

            // Only RegularProp should be attributed. Bug: Version also gets [ProtoMember].
            new TestFixure().AssertOutputContainsCount(source, "ProtoMember", 1);
        }

        // Bug: ProtoAttributeAdder.VisitPropertyDeclaration does not filter expression-bodied
        // (lambda/computed) properties. These have no setter and protobuf-net cannot deserialize them,
        // but they receive [ProtoMember] anyway.
        [Fact]
        public void Add_DoesNotAddAttribute_ToExpressionBodiedProperty()
        {
            var code = @"
namespace Test
{
    public class Foo
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => FirstName + "" "" + LastName;
    }
}";
            var tree = CSharpSyntaxTree.ParseText(code);
            var rewriter = new ProtoAttributeAdder();
            var rewrittenRoot = rewriter.Visit(tree.GetRoot());
            var output = rewrittenRoot.GetText().ToString();
            var source = output.Split(new string[] { " ", "\r\n" }, System.StringSplitOptions.RemoveEmptyEntries);

            // Only FirstName and LastName should be attributed (2 total).
            // Bug: FullName also gets [ProtoMember], producing 3.
            new TestFixure().AssertOutputContainsCount(source, "ProtoMember", 2);
        }
    }
}
