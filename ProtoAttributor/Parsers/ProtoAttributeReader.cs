using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ProtoAttributor.Services
{
    public class ProtoAttributeReader : CSharpSyntaxWalker
    {
        private readonly string _propertyAttributeName;
        private int highestOrder;

        public ProtoAttributeReader(string attributeName)
        {
            _propertyAttributeName = attributeName;
        }

        public int GetProtoNextId(SyntaxNode node)
        {
            highestOrder = 0;
            base.Visit(node);
            return highestOrder + 1;
        }

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            if (node.AttributeLists.Count > 0)
            {
                foreach (var attributeList in node.AttributeLists)
                {
                    var attrs =
                        attributeList
                        .Attributes
                        .Where(
                            attribute =>
                                NodeHelper.AttributeNameMatches(attribute, _propertyAttributeName)
                                )
                        .ToArray();

                    foreach (var item in attrs)
                    {
                        var value = item.ArgumentList.Arguments.FirstOrDefault();
                        int.TryParse(value.GetText().ToString(), out var order);
                        if (order > highestOrder)
                        {
                            highestOrder = order;
                        }
                    }
                }
            }
            base.VisitPropertyDeclaration(node);
        }
    }
}
