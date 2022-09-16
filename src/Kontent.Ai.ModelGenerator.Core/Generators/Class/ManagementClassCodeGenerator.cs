using System.Linq;
using Kontent.Ai.Management.Modules.ModelBuilders;
using Kontent.Ai.ModelGenerator.Core.Common;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json.Serialization;

namespace Kontent.Ai.ModelGenerator.Core.Generators.Class;

public class ManagementClassCodeGenerator : ClassCodeGenerator
{
    private static readonly string KontentElementIdAttributeName = new string
    (
        nameof(KontentElementIdAttribute)
            .Substring(0, nameof(KontentElementIdAttribute).Length - "Attribute".Length)
            .ToArray()
    );

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

    protected override TypeDeclarationSyntax GetClassDeclaration()
    {
        var classDeclaration = base.GetClassDeclaration();

        classDeclaration = classDeclaration.AddMembers(Properties);

        return classDeclaration;
    }

    private MemberDeclarationSyntax[] Properties => ClassDefinition.Properties.OrderBy(p => p.Identifier).Select(element =>
            SyntaxFactory
                .PropertyDeclaration(SyntaxFactory.ParseTypeName(element.TypeName), element.Identifier)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                    GetAccessorDeclaration(SyntaxKind.GetAccessorDeclaration),
                    GetAccessorDeclaration(SyntaxKind.SetAccessorDeclaration))
                .AddAttributeLists(
                    GetAttributeList(nameof(JsonProperty), element.Codename),
                    GetAttributeList(KontentElementIdAttributeName, element.Id)))
        .ToArray<MemberDeclarationSyntax>();

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
