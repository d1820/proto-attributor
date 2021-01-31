using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ProtoAttributor.Services
{
    public class AttributeAdder : CSharpSyntaxRewriter
    {
        private readonly string _propertyAttributeName;
        private int _startIndex;
        private readonly string _classAttributeName;
        private readonly string _usingStatement;

        public AttributeAdder(string attributeName, string classAttributeName, string usingStatement)
        {
            _propertyAttributeName = attributeName;
            _classAttributeName = classAttributeName;
            _usingStatement = usingStatement;
        }

        public SyntaxNode Visit(SyntaxNode node, int startIndex)
        {
            _startIndex = startIndex;
            return base.Visit(node);
        }

        public override SyntaxNode VisitCompilationUnit(CompilationUnitSyntax node)
        {
            var hasUsing = node.Usings.Any(a => a.Name.ToString() == _usingStatement);

            if (!hasUsing)
            {
                var name = SyntaxFactory.ParseName($" {_usingStatement}");
                var usingDir = SyntaxFactory.UsingDirective(name);
                node = node.AddUsings(usingDir);
            }

            return base.VisitCompilationUnit(node);
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var hasMatches = NodeHelper.HasMatch(node.AttributeLists, _classAttributeName);

            if (!hasMatches)
            {
                var name = SyntaxFactory.ParseName(_classAttributeName);
                var attribute = SyntaxFactory.Attribute(name);
                var newAttributes = BuildAttribute(attribute, node.AttributeLists);
                //Get the leading trivia (the newlines and comments)
                var leadTriv = node.GetLeadingTrivia();
                node = node.WithAttributeLists(newAttributes);

                //Append the leading trivia to the method
                node = node.WithLeadingTrivia(leadTriv);
            }

            return base.VisitClassDeclaration(node);
        }

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            var hasMatches = NodeHelper.HasMatch(node.AttributeLists, _propertyAttributeName);

            if (!hasMatches)
            {
                var name = SyntaxFactory.ParseName(_propertyAttributeName);
                var arguments = SyntaxFactory.ParseAttributeArgumentList($"({_startIndex})");
                var attribute = SyntaxFactory.Attribute(name, arguments); //ProtoMember("1")
                var newAttributes = BuildAttribute(attribute, node.AttributeLists);
                //Get the leading trivia (the newlines and comments)
                var leadTriv = node.GetLeadingTrivia();
                node = node.WithAttributeLists(newAttributes);

                //Append the leading trivia to the method
                node = node.WithLeadingTrivia(leadTriv);
                _startIndex++;
            }

            return base.VisitPropertyDeclaration(node);
        }

        private SyntaxList<AttributeListSyntax> BuildAttribute(AttributeSyntax attribute, SyntaxList<AttributeListSyntax> attributeLists)
        {
            var attributeList = new SeparatedSyntaxList<AttributeSyntax>();
            attributeList = attributeList.Add(attribute);

            var asl = SyntaxFactory.AttributeList(attributeList);

            var newAttribute = (AttributeListSyntax)VisitAttributeList(asl);

            return attributeLists.Add(newAttribute);
        }
    }
}
