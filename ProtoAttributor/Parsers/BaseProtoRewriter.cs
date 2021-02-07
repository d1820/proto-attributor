using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace ProtoAttributor.Services
{
    public class BaseProtoRewriter: CSharpSyntaxRewriter
    {
        internal int _startIndex;

        public SyntaxList<AttributeListSyntax> BuildAttribute(AttributeSyntax attribute,
                                                                SyntaxList<AttributeListSyntax> attributeLists,
                                                                SyntaxTrivia trailingWhitspace)
        {
            var newAttribute = NodeHelper.BuildAttributeList(attribute);

            newAttribute = (AttributeListSyntax)VisitAttributeList(newAttribute);

            if (attributeLists.Count > 0)
            {
                //Existing attribute cause the alignment to be off, this is the adjustment
                newAttribute = newAttribute.WithLeadingTrivia(trailingWhitspace);
                newAttribute = newAttribute.WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);
            }
            else
            {
                newAttribute = newAttribute.WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed, trailingWhitspace);
            }

            newAttribute = newAttribute.WithAdditionalAnnotations(Formatter.Annotation);
            return attributeLists.Add(newAttribute);
        }

        public override SyntaxNode VisitCompilationUnit(CompilationUnitSyntax node)
        {
            node = NodeHelper.AddUsing(node, Constants.Proto.USING_STATEMENT);
            return base.VisitCompilationUnit(node);
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            _startIndex = 1;
            var hasMatch = NodeHelper.HasMatch(node.AttributeLists, Constants.Proto.CLASS_ATTRIBUTE_NAME);

            if (!hasMatch)
            {
                var name = SyntaxFactory.ParseName(Constants.Proto.CLASS_ATTRIBUTE_NAME);
                var attribute = SyntaxFactory.Attribute(name);

                node = TriviaMaintainer.Apply(node, (innerNode, wp) =>
                {
                    var newAttributes = BuildAttribute(attribute, innerNode.AttributeLists, wp);

                    return innerNode.WithAttributeLists(newAttributes).WithAdditionalAnnotations(Formatter.Annotation);
                });
            }

            return base.VisitClassDeclaration(node);
        }
    }
}
