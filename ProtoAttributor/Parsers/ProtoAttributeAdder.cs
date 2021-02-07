using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace ProtoAttributor.Services
{
    public class ProtoAttributeAdder : BaseRewriter
    {
        public ProtoAttributeAdder(string attributeName, string classAttributeName, string usingStatement)
            :base(attributeName, classAttributeName, usingStatement)
        {

        }

        public SyntaxNode Visit(SyntaxNode node, int startIndex)
        {
            _startIndex = startIndex;
            return base.Visit(node);
        }

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            var hasMatch = NodeHelper.HasMatch(node.AttributeLists, _propertyAttributeName);
            var hasMatchIgnore = NodeHelper.HasMatch(node.AttributeLists, "ProtoIgnore");

            if (!hasMatch && !hasMatchIgnore)
            {
                var name = SyntaxFactory.ParseName(_propertyAttributeName);
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
