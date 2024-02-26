using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace ProtoAttributor.Parsers.DataContracts
{
    public class DataAttributeAdder: BaseDataRewriter
    {
        private DataAttributeReader _dataReader;

        public DataAttributeAdder()
        {
            _dataReader = new DataAttributeReader();
        }
        public override int CalculateStartingIndex(SyntaxNode node)
        {
            return _dataReader.GetDataMemberNextId(node);
        }
        public override SyntaxNode VisitEnumMemberDeclaration(EnumMemberDeclarationSyntax node)
        {
            var hasMatch = NodeHelper.HasMatch(node.AttributeLists, Constants.Data.ENUM_MEMBER_NAME, Constants.Data.PROPERTY_IGNORE_ATTRIBUTE_NAME);

            if (!hasMatch)
            {
                var name = SyntaxFactory.ParseName(Constants.Data.ENUM_MEMBER_NAME);
                var attribute = SyntaxFactory.Attribute(name); //EnumMember

                node = TriviaMaintainer.Apply(node, (innerNode, wp) =>
                {
                    var newAttributes = BuildAttribute(attribute, innerNode.AttributeLists, wp);

                    return innerNode.WithAttributeLists(newAttributes).WithAdditionalAnnotations(Formatter.Annotation);
                });
                StartIndex++;

            }
            return base.VisitEnumMemberDeclaration(node);
        }

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            var hasMatch = NodeHelper.HasMatch(node.AttributeLists, Constants.Data.PROPERTY_ATTRIBUTE_NAME, Constants.Data.PROPERTY_IGNORE_ATTRIBUTE_NAME);

            if (!hasMatch)
            {
                var name = SyntaxFactory.ParseName(Constants.Data.PROPERTY_ATTRIBUTE_NAME);
                var arguments = SyntaxFactory.ParseAttributeArgumentList($"(Order = {StartIndex})");
                var attribute = SyntaxFactory.Attribute(name, arguments); //DataMember(Order = 1)

                node = TriviaMaintainer.Apply(node, (innerNode, wp) =>
                {
                    var newAttributes = BuildAttribute(attribute, innerNode.AttributeLists, wp);

                    return innerNode.WithAttributeLists(newAttributes).WithAdditionalAnnotations(Formatter.Annotation);
                });
                StartIndex++;
            }

            return base.VisitPropertyDeclaration(node);
        }
    }
}
