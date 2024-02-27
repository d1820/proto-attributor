using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ProtoAttributor.Parsers.DataContracts
{
    public class DataAttributeReader: CSharpSyntaxWalker
    {
        private int _highestOrder;

        public int GetDataMemberNextId(SyntaxNode node)
        {
            _highestOrder = 0;
            base.Visit(node);
            return _highestOrder + 1;
        }

        /// <summary>
        /// Called when the visitor visits a PropertyDeclarationSyntax node.
        /// </summary>
        /// <param name="node"></param>
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
                                NodeHelper.AttributeNameMatches(attribute, Constants.Data.PROPERTY_ATTRIBUTE_NAME)
                                )
                        .ToArray();

                    foreach (var item in attrs)
                    {
                        var argument = item.ArgumentList.Arguments.FirstOrDefault(f=>f.NameEquals.Name.Identifier.ValueText.Equals("Order"));
                        if(argument != null && argument.Expression.Kind() == SyntaxKind.NumericLiteralExpression)
                        {
                            var tokenValue = argument.Expression.ChildTokens().FirstOrDefault(f => f.Kind() == SyntaxKind.NumericLiteralToken);
                            if(tokenValue != null)
                            {
                                var order = Convert.ToInt32( tokenValue.Value);
                                if (order > _highestOrder)
                                {
                                    _highestOrder = order;
                                }
                            }
                        }
                    }
                }
            }
            base.VisitPropertyDeclaration(node);
        }
    }
}
