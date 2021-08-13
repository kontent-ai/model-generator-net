using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kentico.Kontent.ModelGenerator.Core
{
    public class DeliveryClassCodeGenerator : ClassCodeGeneratorBase
    {
        public DeliveryClassCodeGenerator(ClassDefinition classDefinition, string classFilename, string @namespace = DEFAULT_NAMESPACE, bool customPartial = false)
            : base(classDefinition, classFilename, @namespace, customPartial)
        {
        }

        protected override UsingDirectiveSyntax[] GetApiUsings() => new[]
        {
            SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(typeof(Kontent.Delivery.Abstractions.IApiResponse).Namespace!))
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
                        GetAccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                    )).ToArray();

            MemberDeclarationSyntax[] propertyCodenameConstants = ClassDefinition.PropertyCodenameConstants
                .OrderBy(p => p.Codename)
                .Select(element =>
                    GetFieldDeclaration("string", $"{TextHelpers.GetValidPascalCaseIdentifierName(element.Codename)}Codename", element.Codename))
                .ToArray();

            if (!CustomPartial)
            {
                var classCodenameConstant = GetFieldDeclaration("string", "Codename", ClassDefinition.Codename);

                classDeclaration = classDeclaration.AddMembers(classCodenameConstant);
            }

            classDeclaration = classDeclaration.AddMembers(propertyCodenameConstants);
            classDeclaration = classDeclaration.AddMembers(properties);

            return classDeclaration;
        }

        private static FieldDeclarationSyntax GetFieldDeclaration(string typeName, string identifier, string literal) =>
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
