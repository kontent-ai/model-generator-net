using System.Collections.Generic;
using System.Linq;
using Kontent.Ai.ModelGenerator.Core.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kontent.Ai.ModelGenerator.Core.Generators.Class;

/// <summary>
/// Emits a content type as a <c>sealed partial record</c> implementing <c>IContentItem</c>,
/// with <c>[KontentContentType]</c> at the type level and <c>[KontentElement]</c> + constraint
/// attributes per property. Constraint attributes come from each <see cref="ManagementProperty"/>'s
/// <see cref="ManagementProperty.Attributes"/> list.
/// </summary>
public sealed class ManagementClassCodeGenerator(
    ClassDefinition classDefinition,
    string classFilename,
    string @namespace = ClassCodeGenerator.DefaultNamespace)
    : ClassCodeGenerator(classDefinition, classFilename, @namespace)
{
    private const string ContentItemInterfaceName = "IContentItem";
    private const string KontentContentTypeAttribute = "KontentContentType";

    protected override bool IsRecord => true;

    protected override bool UseFileScopedNamespace => true;

    protected override AttributeListSyntax[] BuildPropertyAttributes(Property property)
    {
        if (property is not ManagementProperty managementProperty)
        {
            return [];
        }

        return managementProperty.Attributes
            .Select(BuildAttributeList)
            .ToArray();
    }

    protected override TypeDeclarationSyntax GetClassDeclaration()
    {
        var declaration = (RecordDeclarationSyntax)base.GetClassDeclaration();

        // C# requires 'partial' immediately before the type keyword. Base emits 'public partial';
        // 'sealed' has to slot between them, hence WithModifiers (not AddModifiers, which appends).
        declaration = declaration
            .WithModifiers(SyntaxFactory.TokenList(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                SyntaxFactory.Token(SyntaxKind.SealedKeyword),
                SyntaxFactory.Token(SyntaxKind.PartialKeyword)))
            .AddAttributeLists(BuildAttributeList(
                new AttributeSpec(KontentContentTypeAttribute,
                [
                    AttributeArg.Named("Codename", ClassDefinition.Codename),
                ])))
            .WithBaseList(SyntaxFactory.BaseList(
                SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                    SyntaxFactory.SimpleBaseType(
                        SyntaxFactory.IdentifierName(ContentItemInterfaceName)))));

        declaration = declaration.AddMembers(Properties);

        return declaration;
    }

    protected override UsingDirectiveSyntax[] GetApiUsings() =>
    [
        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")),
        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Collections.Generic")),
        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.ComponentModel.DataAnnotations")),
        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Kontent.Ai.Management.Annotations")),
        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Kontent.Ai.Management.Models")),
    ];

    protected override MemberDeclarationSyntax[] GetAdditionalNamespaceMembers() =>
        ClassDefinition.Enums.Select(BuildEnumDeclaration).ToArray<MemberDeclarationSyntax>();

    private static EnumDeclarationSyntax BuildEnumDeclaration(EnumDefinition definition)
    {
        var enumDecl = SyntaxFactory.EnumDeclaration(definition.Name)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

        foreach (var member in definition.Members)
        {
            var memberDecl = SyntaxFactory.EnumMemberDeclaration(member.Identifier);

            if (member.Attributes.Count > 0)
            {
                memberDecl = memberDecl.AddAttributeLists(
                    member.Attributes.Select(BuildAttributeList).ToArray());
            }

            enumDecl = enumDecl.AddMembers(memberDecl);
        }

        return enumDecl;
    }

    private static AttributeListSyntax BuildAttributeList(AttributeSpec spec)
    {
        var attribute = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(spec.Name));

        if (spec.Arguments.Count > 0)
        {
            attribute = attribute.WithArgumentList(
                SyntaxFactory.AttributeArgumentList(
                    SyntaxFactory.SeparatedList(spec.Arguments.Select(BuildArgument))));
        }

        return SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attribute));
    }

    private static AttributeArgumentSyntax BuildArgument(AttributeArg arg)
    {
        var expression = BuildArgumentExpression(arg.Value);

        return arg.Name is null
            ? SyntaxFactory.AttributeArgument(expression)
            : SyntaxFactory.AttributeArgument(
                SyntaxFactory.NameEquals(arg.Name),
                nameColon: null,
                expression);
    }

    private static ExpressionSyntax BuildArgumentExpression(object value) =>
        value switch
        {
            string s => SyntaxFactory.LiteralExpression(
                SyntaxKind.StringLiteralExpression,
                SyntaxFactory.Literal(s)),
            bool b => SyntaxFactory.LiteralExpression(
                b ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression),
            int i => SyntaxFactory.LiteralExpression(
                SyntaxKind.NumericLiteralExpression,
                SyntaxFactory.Literal(i)),
            long l => SyntaxFactory.LiteralExpression(
                SyntaxKind.NumericLiteralExpression,
                SyntaxFactory.Literal(l)),
            // Strings of comma-separated values, enum members ("AssetFileType.Image"), etc.
            // are rendered as raw C# expressions via ToString().
            _ => SyntaxFactory.ParseExpression(value.ToString()),
        };
}
