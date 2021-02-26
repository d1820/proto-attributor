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

        public override SyntaxNode VisitEnumMemberDeclaration(EnumMemberDeclarationSyntax node)
        {
            var hasMatch = NodeHelper.HasMatch(node.AttributeLists, Constants.Data.ENUM_MEMBER_NAME, Constants.Data.PROPERTY_IGNORE_ATTRIBUTE_NAME);

            if (!hasMatch)
            {
                var name = SyntaxFactory.ParseName(Constants.Data.ENUM_MEMBER_NAME);
                var attribute = SyntaxFactory.Attribute(name); //EnumMember

                node = TriviaMaintainer.Apply(node, (innerNode, wp) =>
                {
                    var newAttributes = BuildAttribute(attribute, innerNode.AttributeLists, wp);

                    return innerNode.WithAttributeLists(newAttributes).WithAdditionalAnnotations(Formatter.Annotation);
                });

                _startIndex++;
            }
            return base.VisitEnumMemberDeclaration(node);
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

                            var newToken = SyntaxFactory.Literal(_startIndex);
                            var spaceTrivia = SyntaxFactory.TriviaList(SyntaxFactory.Space);
                            var newExpression = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, newToken);
                            var equalsToken = SyntaxFactory.Token(SyntaxFactory.TriviaList(), SyntaxKind.EqualsToken, spaceTrivia);
                            SeparatedSyntaxList<AttributeArgumentSyntax> newSeparatedArgList;
                            if (attributeArguementSyntax?.Expression.Kind() == SyntaxKind.NumericLiteralExpression)
                            {
                                var oldToken = attributeArguementSyntax.Expression.ChildTokens().FirstOrDefault(f => f.Kind() == SyntaxKind.NumericLiteralToken);
                                if (oldToken == default)
                                {
                                    continue;
                                }
                                var newIdentifier = attributeArguementSyntax.NameEquals.Name.Identifier.WithLeadingTrivia().WithTrailingTrivia(spaceTrivia);
                                var newIdentifierNameSyntax = attributeArguementSyntax.NameEquals.Name.Update(newIdentifier);
                                var newName = attributeArguementSyntax.NameEquals.Update(newIdentifierNameSyntax, equalsToken);
                                var newAttributeArguementSyntax = attributeArguementSyntax.Update(newName, null, newExpression);
                                newSeparatedArgList = attributeSyntax.ArgumentList.Arguments.Replace(attributeArguementSyntax, newAttributeArguementSyntax);
                            }
                            else
                            {
                                //No order attribute add it
                                var newIdentifierNameSyntax = SyntaxFactory.IdentifierName("Order").WithTrailingTrivia(spaceTrivia);
                                var newName = SyntaxFactory.NameEquals(newIdentifierNameSyntax, equalsToken);
                                var newAttributeArguementSyntax = SyntaxFactory.AttributeArgument(newName, null, newExpression);
                                newSeparatedArgList = attributeSyntax.ArgumentList.Arguments.Add(newAttributeArguementSyntax);
                            }
                            var newAttributeArgumentListSyntax = attributeSyntax.ArgumentList.WithArguments(newSeparatedArgList);
                            var newAttributeSyntax = attributeSyntax.Update(attributeSyntax.Name, newAttributeArgumentListSyntax);
                            modifiedAttributeList = modifiedAttributeList.Replace(attributeSyntax, newAttributeSyntax);
                            _startIndex++;
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
