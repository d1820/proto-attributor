using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using ProtoAttributor.Parsers;
using ProtoAttributor.Parsers.ProtoContracts;

namespace ProtoAttributor.Parsers.ProtoContracts
{
    public class ProtoAttributeRewriter: BaseProtoRewriter
    {
        public override int CalculateStartingIndex(SyntaxNode node)
        {
            return 1;
        }

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            var hasMatch = NodeHelper.HasMatch(node.AttributeLists, Constants.Proto.PROPERTY_ATTRIBUTE_NAME, Constants.Proto.PROPERTY_IGNORE_ATTRIBUTE_NAME);

            if (!hasMatch)
            {
                var name = SyntaxFactory.ParseName(Constants.Proto.PROPERTY_ATTRIBUTE_NAME);
                var arguments = SyntaxFactory.ParseAttributeArgumentList($"({_startIndex})");
                var attribute = SyntaxFactory.Attribute(name, arguments); //ProtoMember("1")

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
                        var attributeSyntaxes = attributeList.Attributes.Where(attribute => NodeHelper.AttributeNameMatches(attribute, Constants.Proto.PROPERTY_ATTRIBUTE_NAME)).ToArray();

                        var modifiedAttributeList = attributeList.Attributes;
                        foreach (var attributeSyntax in attributeSyntaxes)
                        {
                            //The first is always the order value with a protoMember
                            var value = attributeSyntax.ArgumentList?.Arguments.FirstOrDefault();
                            if (value == null)
                            {
                                continue;
                            }
                            var newValueExpression = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(_startIndex));
                            var newvalue = value.Update(null, null, newValueExpression);
                            var newSeparatedArgList = attributeSyntax.ArgumentList.Arguments.Replace(value, newvalue);

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
