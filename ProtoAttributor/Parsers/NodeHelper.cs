using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ProtoAttributor.Services
{
    public static class NodeHelper
    {
        public static SimpleNameSyntax GetSimpleNameFromNode(AttributeSyntax node)
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

        public static bool AttributeNameMatches(AttributeSyntax attribute, string startsWith)
        {
            return
                GetSimpleNameFromNode(attribute)
                .Identifier
                .Text
                .StartsWith(startsWith);
        }

        public static bool HasMatch(SyntaxList<AttributeListSyntax> attributeLists, string matchName)
        {
            if (attributeLists.Count > 0)
            {
                foreach (var attributeList in attributeLists)
                {
                    var matches =
                        attributeList
                        .Attributes
                        .Where(
                            attribute =>
                                AttributeNameMatches(attribute, matchName)
                                )
                        .ToArray();

                    if (matches.Length > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
