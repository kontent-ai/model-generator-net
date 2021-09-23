using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kentico.Kontent.ModelGenerator.Core
{
    public abstract class DeliveryClassCodeGeneratorBase : ClassCodeGenerator
    {
        protected DeliveryClassCodeGeneratorBase(ClassDefinition classDefinition, string classFilename, string @namespace = DefaultNamespace)
            : base(classDefinition, classFilename, @namespace)
        {
        }

        protected MemberDeclarationSyntax[] Properties
            => ClassDefinition.Properties.OrderBy(p => p.Identifier).Select(element => SyntaxFactory
                .PropertyDeclaration(SyntaxFactory.ParseTypeName(element.TypeName), element.Identifier)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                    GetAccessorDeclaration(SyntaxKind.GetAccessorDeclaration),
                    GetAccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                )).ToArray<MemberDeclarationSyntax>();

        protected MemberDeclarationSyntax[] PropertyCodenameConstants
            => ClassDefinition.PropertyCodenameConstants
                .OrderBy(p => p.Codename)
                .Select(element =>
                    GetFieldDeclaration("string", $"{TextHelpers.GetValidPascalCaseIdentifierName(element.Codename)}Codename", element.Codename))
                .ToArray<MemberDeclarationSyntax>();

        protected static FieldDeclarationSyntax GetFieldDeclaration(string typeName, string identifier, string literal) =>
            SyntaxFactory.FieldDeclaration(
                    SyntaxFactory.VariableDeclaration(
                        SyntaxFactory.ParseTypeName(typeName),
                        SyntaxFactory.SeparatedList(new[]
                        {
                            SyntaxFactory.VariableDeclarator(
                                SyntaxFactory.Identifier(
                                    identifier),
                                null,
                                SyntaxFactory.EqualsValueClause(SyntaxFactory.LiteralExpression(
                                    SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(literal)))
                            )
                        })
                    )
                )
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.ConstKeyword));
    }
}
