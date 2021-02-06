using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

            if (!hasMatch)
            {
                var name = SyntaxFactory.ParseName(_propertyAttributeName);
                var arguments = SyntaxFactory.ParseAttributeArgumentList($"({_startIndex})");
                var attribute = SyntaxFactory.Attribute(name, arguments); //ProtoMember("1")

                var newAttributes = BuildAttribute(attribute, node.AttributeLists);
                node = NodeHelper.AddNewPropertyAttribute(newAttributes, node);
                _startIndex++;
            }

            return base.VisitPropertyDeclaration(node);
        }

    }
}
