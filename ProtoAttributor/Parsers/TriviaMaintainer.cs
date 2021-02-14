using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ProtoAttributor.Parsers
{
    public static class TriviaMaintainer
    {
        public static PropertyDeclarationSyntax Apply(PropertyDeclarationSyntax node, Func<PropertyDeclarationSyntax, SyntaxTrivia, PropertyDeclarationSyntax> builder)
        {
            var leadingTrivia = node.GetLeadingTrivia();
            var trailingTrivia = node.GetTrailingTrivia();

            node = node.WithoutLeadingTrivia();
            node = node.WithoutTrailingTrivia();

            var wp = leadingTrivia.FirstOrDefault(w => w.Kind() == SyntaxKind.WhitespaceTrivia);

            node = builder?.Invoke(node, wp);

            node = node.WithLeadingTrivia(leadingTrivia);

            node = node.WithTrailingTrivia(trailingTrivia);

            return node;
        }

        public static ClassDeclarationSyntax Apply(ClassDeclarationSyntax node, Func<ClassDeclarationSyntax, SyntaxTrivia, ClassDeclarationSyntax> builder)
        {
            var leadingTrivia = node.GetLeadingTrivia();
            var trailingTrivia = node.GetTrailingTrivia();

            node = node.WithoutLeadingTrivia();
            node = node.WithoutTrailingTrivia();

            var wp = leadingTrivia.FirstOrDefault(w => w.Kind() == SyntaxKind.WhitespaceTrivia);

            node = builder?.Invoke(node, wp);

            node = node.WithLeadingTrivia(leadingTrivia);

            node = node.WithTrailingTrivia(trailingTrivia);

            return node;
        }
    }
}
