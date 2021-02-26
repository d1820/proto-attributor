using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using ProtoAttributor.Parsers;
using ProtoAttributor.Parsers.ProtoContracts;

namespace ProtoAttributor.Parsers.ProtoContracts
{
    public class ProtoAttributeAdder: BaseProtoRewriter
    {
        private ProtoAttributeReader _protoReader;

        public ProtoAttributeAdder()
        {
            _protoReader = new ProtoAttributeReader();
        }
        public override int CalculateStartingIndex(SyntaxNode node)
        {
            return _protoReader.GetProtoNextId(node);
        }

        public override SyntaxNode VisitEnumMemberDeclaration(EnumMemberDeclarationSyntax node)
        {
            var hasMatch = NodeHelper.HasMatch(node.AttributeLists, Constants.Proto.ENUM_MEMBER_NAME, Constants.Proto.PROPERTY_IGNORE_ATTRIBUTE_NAME);

            if (!hasMatch)
            {
                var name = SyntaxFactory.ParseName(Constants.Proto.ENUM_MEMBER_NAME);
                var attribute = SyntaxFactory.Attribute(name); //ProtoEnum()

                node = TriviaMaintainer.Apply(node, (innerNode, wp) =>
                {
                    var newAttributes = BuildAttribute(attribute, innerNode.AttributeLists, wp);

                    return innerNode.WithAttributeLists(newAttributes).WithAdditionalAnnotations(Formatter.Annotation);
                });
                _startIndex++;
            }

            return base.VisitEnumMemberDeclaration(node);
        }

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            var hasMatch = NodeHelper.HasMatch(node.AttributeLists, Constants.Proto.PROPERTY_ATTRIBUTE_NAME, Constants.Proto.PROPERTY_IGNORE_ATTRIBUTE_NAME);

            if (!hasMatch)
            {
                var name = SyntaxFactory.ParseName(Constants.Proto.PROPERTY_ATTRIBUTE_NAME);
                var arguments = SyntaxFactory.ParseAttributeArgumentList($"({_startIndex})");
                var attribute = SyntaxFactory.Attribute(name, arguments); //ProtoMember("1")

                node = TriviaMaintainer.Apply(node, (innerNode, wp) =>
                {
                    var newAttributes = BuildAttribute(attribute, innerNode.AttributeLists, wp);

                    return innerNode.WithAttributeLists(newAttributes).WithAdditionalAnnotations(Formatter.Annotation);
                });
                _startIndex++;
            }

            return base.VisitPropertyDeclaration(node);
        }
    }
}
