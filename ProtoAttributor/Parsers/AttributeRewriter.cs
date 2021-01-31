using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ProtoAttributor.Services
{
    public class AttributeRewriter : CSharpSyntaxRewriter
    {
        public AttributeRewriter(
            string attributeName)
        {
            _attributeName = attributeName;
        }

        private readonly string _attributeName;

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            var newAttributes = new SyntaxList<AttributeListSyntax>();

            foreach (var attributeList in node.AttributeLists)
            {
                var nodesToRemove =
                    attributeList
                    .Attributes
                    .Where(
                        attribute =>
                            AttributeNameMatches(attribute)
                            )
                    .ToArray();

                if (nodesToRemove.Length != attributeList.Attributes.Count)
                {
                    //We want to remove only some of the attributes
                    var newAttribute =
                        (AttributeListSyntax)VisitAttributeList(
                            attributeList.RemoveNodes(nodesToRemove, SyntaxRemoveOptions.KeepNoTrivia));

                    newAttributes = newAttributes.Add(newAttribute);
                }
            }

            //Get the leading trivia (the newlines and comments)
            var leadTriv = node.GetLeadingTrivia();
            node = node.WithAttributeLists(newAttributes);

            //Append the leading trivia to the method
            node = node.WithLeadingTrivia(leadTriv);
            return node;
        }

        private static SimpleNameSyntax GetSimpleNameFromNode(AttributeSyntax node)
        {
            var identifierNameSyntax = node.Name as IdentifierNameSyntax;
            var qualifiedNameSyntax = node.Name as QualifiedNameSyntax;

            return
                identifierNameSyntax
                ??
                qualifiedNameSyntax?.Right
                ??
                (node.Name as AliasQualifiedNameSyntax)?.Name;
        }

        private bool AttributeNameMatches(AttributeSyntax attribute)
        {
            return
                GetSimpleNameFromNode(attribute)
                .Identifier
                .Text
                .StartsWith(_attributeName);
        }
    }
}
