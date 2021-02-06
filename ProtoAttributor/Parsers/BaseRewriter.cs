using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;

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

        public SyntaxList<AttributeListSyntax> BuildAttribute(AttributeSyntax attribute, SyntaxList<AttributeListSyntax> attributeLists)
        {
            var asl = NodeHelper.BuildAttributeList(attribute);
            var newAttribute = (AttributeListSyntax)VisitAttributeList(asl);
            newAttribute = newAttribute.WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);
            newAttribute = newAttribute.WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation);
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

                var newAttributes = BuildAttribute(attribute, node.AttributeLists);
                //Get the leading trivia (the newlines and comments)
                var leadTriv = node.GetLeadingTrivia();
                //var trailTriv = node.GetTrailingTrivia();
                node = node.WithAttributeLists(newAttributes).WithAdditionalAnnotations(Formatter.Annotation);

                //Append the leading trivia to the method
                node = node.WithLeadingTrivia(leadTriv);
                //node = node.WithTrailingTrivia(trailTriv);

            }

            return base.VisitClassDeclaration(node);
        }
    }
}
