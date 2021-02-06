using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;

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
                    if (attributeList.Attributes.Any(attribute => AttributeNameMatches(attribute, matchName)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static AttributeListSyntax BuildAttributeList(AttributeSyntax attribute)
        {
            var attributeList = new SeparatedSyntaxList<AttributeSyntax>();
            attributeList = attributeList.Add(attribute);

            return SyntaxFactory.AttributeList(attributeList);
        }

        public static CompilationUnitSyntax AddUsing(CompilationUnitSyntax node, string usingStatement)
        {
            var hasUsing = node.Usings.Any(a => a.Name.ToString() == usingStatement);

            if (!hasUsing)
            {
                var name = SyntaxFactory.ParseName($" {usingStatement}");
                var usingDir = SyntaxFactory.UsingDirective(name);
                usingDir = usingDir.WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation);
                usingDir = usingDir.WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);
                node = node.AddUsings(usingDir);
            }
            return node;
        }

        public static PropertyDeclarationSyntax AddNewPropertyAttribute(SyntaxList<AttributeListSyntax> newAttributes, PropertyDeclarationSyntax node)
        {
            //Get the leading trivia (the newlines and comments)
            var leadTriv = node.GetLeadingTrivia();
            var trailTriv = node.GetTrailingTrivia();

            node = node.WithAttributeLists(newAttributes);

            var leadsToKeep = leadTriv.Where(w => w.Kind() != SyntaxKind.EndOfLineTrivia);
            node = node.WithLeadingTrivia(SyntaxFactory.TriviaList(leadsToKeep));

            node = node.WithTrailingTrivia(trailTriv);
            return node;

        }


    }
}
