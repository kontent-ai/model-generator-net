using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json.Serialization;

namespace Kentico.Kontent.ModelGenerator.Core
{
    public class ManagementClassCodeGenerator : ClassCodeGeneratorBase
    {
        private const string ElementIdPropertyName = "KontentElementId";//todo replace with name from MAPI 

        public ManagementClassCodeGenerator(ClassDefinition classDefinition, string classFilename, string @namespace = DEFAULT_NAMESPACE) : base(classDefinition, classFilename, @namespace)
        {
        }

        protected override UsingDirectiveSyntax[] GetApiUsings() =>
            new[]
            {
                SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(typeof(Kontent.Management.Models.Items.ContentItemModel).Namespace!)),
                SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(typeof(Kontent.Management.Models.Assets.AssetModel).Namespace!)),
                SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(typeof(Kontent.Management.Modules.ModelBuilders.IModelProvider).Namespace!)),//todo replace with KontentElementId
                SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName($"{nameof(Newtonsoft)}.{nameof(Newtonsoft.Json)}"))
            };

        protected override ClassDeclarationSyntax GetClassDeclaration()
        {
            var classDeclaration = base.GetClassDeclaration();

            MemberDeclarationSyntax[] properties = ClassDefinition.Properties.OrderBy(p => p.Identifier).Select(element =>
                SyntaxFactory
                    .PropertyDeclaration(SyntaxFactory.ParseTypeName(element.TypeName), element.Identifier)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddAccessorListAccessors(
                        GetAccessorDeclaration(SyntaxKind.GetAccessorDeclaration),
                        GetAccessorDeclaration(SyntaxKind.SetAccessorDeclaration))
                    .AddAttributeLists(
                        GetAttributeList(nameof(JsonProperty), element.Codename),
                        GetAttributeList(ElementIdPropertyName, element.Id)))
                .ToArray();

            classDeclaration = classDeclaration.AddMembers(properties);

            return classDeclaration;
        }

        private static AttributeListSyntax GetAttributeList(string identifier, string literal) =>
            SyntaxFactory.AttributeList(
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(identifier))
                        .WithArgumentList(
                            SyntaxFactory.AttributeArgumentList(
                                SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.AttributeArgument(
                                        SyntaxFactory.LiteralExpression(
                                            SyntaxKind.StringLiteralExpression,
                                            SyntaxFactory.Literal(literal))))))));
    }
}
