using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ProtoAttributor.Services
{

    public class ProtoAttributeRemover : CSharpSyntaxRewriter
    {
        internal readonly string _usingStatement;
        private readonly string PARTIAL_SEARCH_TERM = "Proto";

        public ProtoAttributeRemover(string usingStatement)
        {
            _usingStatement = usingStatement;
        }


        public override SyntaxNode VisitCompilationUnit(CompilationUnitSyntax node)
        {
            if (node.Usings.Count > 0)
            {
                var newUsingDirectives = new SyntaxList<UsingDirectiveSyntax>();
                var nodesToKeep = node.Usings.Where(directive => directive.Name.ToString() != _usingStatement).ToArray();
                newUsingDirectives = newUsingDirectives.AddRange(nodesToKeep);
                var leadTriv = node.GetLeadingTrivia();
                node = node.WithUsings(newUsingDirectives);
                node = node.WithLeadingTrivia(leadTriv);
            }

            return base.VisitCompilationUnit(node);
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            if (node.AttributeLists.Count > 0)
            {
                var newAttributeLists = new SyntaxList<AttributeListSyntax>();
                foreach (var attributeList in node.AttributeLists)
                {
                    var nodesToRemove = attributeList.Attributes.Where(attribute => NodeHelper.AttributeNameMatches(attribute, PARTIAL_SEARCH_TERM)).ToArray();

                    // If the lists are the same length, we are removing all attributes and can just avoid populating newAttributes.
                    if (nodesToRemove.Length != attributeList.Attributes.Count)
                    {
                        var newAttribute = (AttributeListSyntax)VisitAttributeList(attributeList.RemoveNodes(nodesToRemove, SyntaxRemoveOptions.KeepNoTrivia));
                        newAttributeLists = newAttributeLists.Add(newAttribute);
                    }
                }
                var leadTriv = node.GetLeadingTrivia();
                node = node.WithAttributeLists(newAttributeLists);
                node = node.WithLeadingTrivia(leadTriv);
            }

            return base.VisitClassDeclaration(node);
        }

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            if (node.AttributeLists.Count > 0)
            {
                var newAttributeLists = new SyntaxList<AttributeListSyntax>();
                foreach (var attributeList in node.AttributeLists)
                {
                    var nodesToRemove = attributeList.Attributes.Where(attribute => NodeHelper.AttributeNameMatches(attribute, PARTIAL_SEARCH_TERM)).ToArray();

                    // If the lists are the same length, we are removing all attributes and can just avoid populating newAttributes.
                    if (nodesToRemove.Length != attributeList.Attributes.Count)
                    {
                        var newAttribute = (AttributeListSyntax)VisitAttributeList( attributeList.RemoveNodes(nodesToRemove, SyntaxRemoveOptions.KeepNoTrivia));
                        newAttributeLists = newAttributeLists.Add(newAttribute);
                    }
                }
                var leadTriv = node.GetLeadingTrivia();
                node = node.WithAttributeLists(newAttributeLists);
                node = node.WithLeadingTrivia(leadTriv);
            }

            return base.VisitPropertyDeclaration(node);
        }
    }
}
