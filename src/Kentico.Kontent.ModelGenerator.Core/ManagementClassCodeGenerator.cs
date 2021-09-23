using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Kentico.Kontent.Management.Modules.ModelBuilders;
using Newtonsoft.Json.Serialization;

namespace Kentico.Kontent.ModelGenerator.Core
{
    public class ManagementClassCodeGenerator : ClassCodeGenerator
    {
        private static readonly string KontentElementIdAttributeName = new string(nameof(KontentElementIdAttribute).SkipLast(9).ToArray());

        public ManagementClassCodeGenerator(ClassDefinition classDefinition, string classFilename, string @namespace = DefaultNamespace)
            : base(classDefinition, classFilename, @namespace)
        {
        }

        protected override UsingDirectiveSyntax[] GetApiUsings() =>
            new[]
            {
                SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(typeof(Management.Models.LanguageVariants.Elements.BaseElement).Namespace!)),
                SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(typeof(KontentElementIdAttribute).Namespace!)),
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
                            GetAttributeList(KontentElementIdAttributeName, element.Id)))
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
