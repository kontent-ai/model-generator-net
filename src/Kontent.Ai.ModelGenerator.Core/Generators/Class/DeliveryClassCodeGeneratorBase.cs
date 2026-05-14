using System.Linq;
using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Helpers;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kontent.Ai.ModelGenerator.Core.Generators.Class;

public abstract class DeliveryClassCodeGeneratorBase(ClassDefinition classDefinition, string classFilename, string @namespace = ClassCodeGenerator.DefaultNamespace) : ClassCodeGenerator(classDefinition, classFilename, @namespace)
{
    protected override AttributeListSyntax[] BuildPropertyAttributes(Property property) =>
    [
        SyntaxFactory.AttributeList(
            SyntaxFactory.SingletonSeparatedList(
                SyntaxFactory.Attribute(
                    SyntaxFactory.IdentifierName("JsonPropertyName"),
                    SyntaxFactory.AttributeArgumentList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.AttributeArgument(
                                SyntaxFactory.LiteralExpression(
                                    SyntaxKind.StringLiteralExpression,
                                    SyntaxFactory.Literal(property.Codename))))))))
    ];

    protected MemberDeclarationSyntax[] PropertyCodenameConstants
        => ClassDefinition.PropertyCodenameConstants
            .OrderBy(p => p)
            .Select(codename =>
            {
                var identifier = $"{TextHelpers.GetValidPascalCaseIdentifierName(codename)}Codename";
                if (ClassDefinition.RenamedCodenameConstants.Contains(codename))
                {
                    identifier = "_" + identifier;
                }

                return GetFieldDeclaration("string", identifier, codename);
            })
            .ToArray<MemberDeclarationSyntax>();

    protected static FieldDeclarationSyntax GetFieldDeclaration(string typeName, string identifier, string literal) =>
        SyntaxFactory.FieldDeclaration(
                SyntaxFactory.VariableDeclaration(
                    SyntaxFactory.ParseTypeName(typeName),
                    SyntaxFactory.SeparatedList(
                    [
                        SyntaxFactory.VariableDeclarator(
                            SyntaxFactory.Identifier(
                                identifier),
                            null,
                            SyntaxFactory.EqualsValueClause(SyntaxFactory.LiteralExpression(
                                SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(literal)))
                        )
                    ])
                )
            )
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.ConstKeyword));
}
