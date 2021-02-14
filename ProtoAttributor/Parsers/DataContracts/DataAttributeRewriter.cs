using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace ProtoAttributor.Parsers.DataContracts
{
    public class DataAttributeRewriter: BaseDataRewriter
    {
        public override int CalculateStartingIndex(SyntaxNode node)
        {
            return 1;
        }

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            var hasMatch = NodeHelper.HasMatch(node.AttributeLists, Constants.Data.PROPERTY_ATTRIBUTE_NAME, Constants.Data.PROPERTY_IGNORE_ATTRIBUTE_NAME);

            if (!hasMatch)
            {
                var name = SyntaxFactory.ParseName(Constants.Data.PROPERTY_ATTRIBUTE_NAME);
                var arguments = SyntaxFactory.ParseAttributeArgumentList($"(Order = {_startIndex})");
                var attribute = SyntaxFactory.Attribute(name, arguments); //DataMember(Order = 1)

                node = TriviaMaintainer.Apply(node, (innerNode, wp) =>
                {
                    var newAttributes = BuildAttribute(attribute, innerNode.AttributeLists, wp);

                    return innerNode.WithAttributeLists(newAttributes).WithAdditionalAnnotations(Formatter.Annotation);
                });

                _startIndex++;
            }
            else
            {
                //renumber
                if (node.AttributeLists.Count > 0)
                {
                    var newAttributeLists = new SyntaxList<AttributeListSyntax>();
                    foreach (var attributeList in node.AttributeLists)
                    {
                        var attributeSyntaxes = attributeList.Attributes.Where(attribute => NodeHelper.AttributeNameMatches(attribute, Constants.Data.PROPERTY_ATTRIBUTE_NAME)).ToArray();

                        var modifiedAttributeList = attributeList.Attributes;
                        foreach (var attributeSyntax in attributeSyntaxes)
                        {
                            var attributeArguementSyntax = attributeSyntax.ArgumentList.Arguments.FirstOrDefault(f => f.NameEquals.Name.Identifier.ValueText.Equals("Order"));
                            if (attributeArguementSyntax != null && attributeArguementSyntax.Expression.Kind() == SyntaxKind.NumericLiteralExpression)
                            {
                                var oldToken = attributeArguementSyntax.Expression.ChildTokens().FirstOrDefault(f => f.Kind() == SyntaxKind.NumericLiteralToken);
                                if (oldToken == default)
                                {
                                    continue;
                                }
                                var newToken = SyntaxFactory.Literal(_startIndex);

                                var newExpression = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, newToken);
                                var newAttributeArguementSyntax = attributeArguementSyntax.Update(attributeArguementSyntax.NameEquals, null, newExpression);

                                var newSeparatedArgList = attributeSyntax.ArgumentList.Arguments.Replace(attributeArguementSyntax, newAttributeArguementSyntax);

                                var newAttributeArgumentListSyntax = attributeSyntax.ArgumentList.WithArguments(newSeparatedArgList);
                                var newAttributeSyntax = attributeSyntax.Update(attributeSyntax.Name, newAttributeArgumentListSyntax);
                                modifiedAttributeList = modifiedAttributeList.Replace(attributeSyntax, newAttributeSyntax);
                                _startIndex++;
                            }
                        }
                        newAttributeLists = newAttributeLists.Add(attributeList.WithAttributes(modifiedAttributeList));
                    }
                    var leadTriv = node.GetLeadingTrivia();
                    node = node.WithAttributeLists(newAttributeLists);
                    node = node.WithLeadingTrivia(leadTriv);
                }
            }

            return base.VisitPropertyDeclaration(node);
        }
    }
}
