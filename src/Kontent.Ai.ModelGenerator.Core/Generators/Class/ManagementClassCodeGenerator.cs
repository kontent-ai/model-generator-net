using System;
using System.Collections.Generic;
using System.Linq;
using Kontent.Ai.Management.Modules.ModelBuilders;
using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Configuration;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json.Serialization;

namespace Kontent.Ai.ModelGenerator.Core.Generators.Class;

public class ManagementClassCodeGenerator : ClassCodeGenerator
{
    private readonly ElementReferenceType _elementReference;

    private static readonly string KontentElementIdAttributeName = new string
    (
        nameof(KontentElementIdAttribute)
            .Substring(0, nameof(KontentElementIdAttribute).Length - "Attribute".Length)
            .ToArray()
    );

    public ManagementClassCodeGenerator(
        ClassDefinition classDefinition,
        string classFilename,
        ElementReferenceType elementReference,
        string @namespace = DefaultNamespace) : base(classDefinition, classFilename, @namespace)
    {
        _elementReference = elementReference;
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
                .AddAttributeLists(GetAttributeLists(element).ToArray()))
        .ToArray<MemberDeclarationSyntax>();

    private IEnumerable<AttributeListSyntax> GetAttributeLists(Property element)
    {
        yield return GetAttributeList(nameof(JsonProperty), element.Codename);

        if (_elementReference.HasFlag(ElementReferenceType.Id))
        {
            yield return GetAttributeList(KontentElementIdAttributeName, element.Id);
        }

        if (_elementReference.HasFlag(ElementReferenceType.ExternalId))
        {
            throw new NotImplementedException();
        }
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
