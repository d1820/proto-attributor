using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace ProtoAttributor.Services
{
    public class BaseRewriter : CSharpSyntaxRewriter
    {
        internal readonly string _propertyAttributeName;
        internal int _startIndex;
        internal readonly string _classAttributeName;
        internal readonly string _usingStatement;

        public BaseRewriter(string attributeName, string classAttributeName, string usingStatement)
        {
            _propertyAttributeName = attributeName;
            _classAttributeName = classAttributeName;
            _usingStatement = usingStatement;
        }

        public SyntaxList<AttributeListSyntax> BuildAttribute(AttributeSyntax attribute,
                                                                SyntaxList<AttributeListSyntax> attributeLists,
                                                                SyntaxTrivia trailingWhitspace)
        {
            var newAttribute = NodeHelper.BuildAttributeList(attribute);

            newAttribute = (AttributeListSyntax)VisitAttributeList(newAttribute);

            newAttribute = newAttribute.WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed, trailingWhitspace);
            newAttribute = newAttribute.WithAdditionalAnnotations(Formatter.Annotation);
            return attributeLists.Add(newAttribute);
        }

        public override SyntaxNode VisitCompilationUnit(CompilationUnitSyntax node)
        {
            node = NodeHelper.AddUsing(node, _usingStatement);
            return base.VisitCompilationUnit(node);
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var hasMatch = NodeHelper.HasMatch(node.AttributeLists, _classAttributeName);

            if (!hasMatch)
            {
                var name = SyntaxFactory.ParseName(_classAttributeName);
                var attribute = SyntaxFactory.Attribute(name);

                node = TriviaMaintainer.Apply(node, (innerNode, wp) =>
                {
                    var newAttributes = BuildAttribute(attribute, innerNode.AttributeLists, wp);

                    innerNode = innerNode.WithAttributeLists(newAttributes).WithAdditionalAnnotations(Formatter.Annotation);

                    return innerNode;
                });
            }

            return base.VisitClassDeclaration(node);
        }
    }
}
