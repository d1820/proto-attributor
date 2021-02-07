using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace ProtoAttributor.Services
{
    public class ProtoAttributeAdder : BaseProtoRewriter
    {
        public SyntaxNode Visit(SyntaxNode node, int startIndex)
        {
            _startIndex = startIndex;
            return base.Visit(node);
        }

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            var hasMatch = NodeHelper.HasMatch(node.AttributeLists, Constants.Proto.PROPERTY_ATTRIBUTE_NAME);
            var hasMatchIgnore = NodeHelper.HasMatch(node.AttributeLists, "ProtoIgnore");

            if (!hasMatch && !hasMatchIgnore)
            {
                var name = SyntaxFactory.ParseName(Constants.Proto.PROPERTY_ATTRIBUTE_NAME);
                var arguments = SyntaxFactory.ParseAttributeArgumentList($"({_startIndex})");
                var attribute = SyntaxFactory.Attribute(name, arguments); //ProtoMember("1")

                node = TriviaMaintainer.Apply(node, (innerNode, wp) =>
                {
                    var newAttributes = BuildAttribute(attribute, innerNode.AttributeLists, wp);

                    innerNode = innerNode.WithAttributeLists(newAttributes).WithAdditionalAnnotations(Formatter.Annotation);

                    return innerNode;
                });
                _startIndex++;
            }

            return base.VisitPropertyDeclaration(node);
        }
    }
}
