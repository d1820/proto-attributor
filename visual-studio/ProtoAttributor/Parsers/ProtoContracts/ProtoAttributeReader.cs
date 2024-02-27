using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ProtoAttributor.Parsers.ProtoContracts
{
    public class ProtoAttributeReader: CSharpSyntaxWalker
    {
        private int _highestOrder;

        public int GetProtoNextId(SyntaxNode node)
        {
            _highestOrder = 0;
            base.Visit(node);
            return _highestOrder + 1;
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
                                NodeHelper.AttributeNameMatches(attribute, Constants.Proto.PROPERTY_ATTRIBUTE_NAME)
                                )
                        .ToArray();

                    foreach (var item in attrs)
                    {
                        var value = item.ArgumentList.Arguments.FirstOrDefault();
                        int.TryParse(value.GetText().ToString(), out var order);
                        if (order > _highestOrder)
                        {
                            _highestOrder = order;
                        }
                    }
                }
            }
            base.VisitPropertyDeclaration(node);
        }
    }
}
