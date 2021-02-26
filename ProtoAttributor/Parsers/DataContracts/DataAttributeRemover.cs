using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ProtoAttributor.Parsers.DataContracts
{
    public class DataAttributeRemover: CSharpSyntaxRewriter
    {
        public DataAttributeRemover()
        {
        }

        public override SyntaxNode VisitCompilationUnit(CompilationUnitSyntax node)
        {
            if (node.Usings.Count > 0)
            {
                var newUsingDirectives = new SyntaxList<UsingDirectiveSyntax>();
                var nodesToKeep = node.Usings.Where(directive => directive.Name.ToString() != Constants.Data.USING_STATEMENT).ToArray();
                newUsingDirectives = newUsingDirectives.AddRange(nodesToKeep);
                var leadTriv = node.GetLeadingTrivia();
                node = node.WithUsings(newUsingDirectives);
                node = node.WithLeadingTrivia(leadTriv);
            }

            return base.VisitCompilationUnit(node);
        }

        public override SyntaxNode VisitEnumDeclaration(EnumDeclarationSyntax node)
        {
            if (node.AttributeLists.Count > 0)
            {
                var newAttributeLists = new SyntaxList<AttributeListSyntax>();
                foreach (var attributeList in node.AttributeLists)
                {
                    var nodesToRemove = attributeList.Attributes.Where(attribute => NodeHelper.AttributeNameContains(attribute, Constants.Data.BASE_PROP_NAME, Constants.Data.BASE_KNOWN_TYPE_NAME)).ToArray();

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

            return base.VisitEnumDeclaration(node);
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            if (node.AttributeLists.Count > 0)
            {
                var newAttributeLists = new SyntaxList<AttributeListSyntax>();
                foreach (var attributeList in node.AttributeLists)
                {
                    var nodesToRemove = attributeList.Attributes.Where(attribute => NodeHelper.AttributeNameContains(attribute, Constants.Data.BASE_PROP_NAME, Constants.Data.BASE_KNOWN_TYPE_NAME)).ToArray();

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
                    var nodesToRemove = attributeList.Attributes.Where(attribute => NodeHelper.AttributeNameContains(attribute, Constants.Data.BASE_PROP_NAME, Constants.Data.BASE_PROPIGNORE_NAME)).ToArray();

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

            return base.VisitPropertyDeclaration(node);
        }

        public override SyntaxNode VisitEnumMemberDeclaration(EnumMemberDeclarationSyntax node)
        {
            if (node.AttributeLists.Count > 0)
            {
                var newAttributeLists = new SyntaxList<AttributeListSyntax>();
                foreach (var attributeList in node.AttributeLists)
                {
                    var nodesToRemove = attributeList.Attributes.Where(attribute => NodeHelper.AttributeNameContains(attribute, Constants.Data.BASE_PROP_NAME, Constants.Data.BASE_PROPIGNORE_NAME, Constants.Data.ENUM_MEMBER_NAME)).ToArray();

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

            return base.VisitEnumMemberDeclaration(node);
        }
    }
}
