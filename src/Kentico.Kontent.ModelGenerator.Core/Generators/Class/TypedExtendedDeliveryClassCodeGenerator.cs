using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using Kentico.Kontent.ModelGenerator.Core.Common;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kentico.Kontent.ModelGenerator.Core.Generators.Class
{
    public class TypedExtendedDeliveryClassCodeGenerator : ClassCodeGenerator
    {
        private const string OfTypeName = "OfType";
        private const string FirstOrDefaultName = "FirstOrDefault";
        private const string SingleSuffix = "Single";
        private const string TypedSuffix = "Typed";

        public TypedExtendedDeliveryClassCodeGenerator(ClassDefinition classDefinition, string classFilename, string @namespace = DefaultNamespace)
            : base(classDefinition, classFilename, @namespace)
        {
        }

        protected override MemberDeclarationSyntax[] Properties => GetProperties();

        protected override UsingDirectiveSyntax[] GetApiUsings() => new[]
        {
            SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(nameof(System))),
            SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(typeof(Enumerable).Namespace!)),
            SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(typeof(System.Collections.Generic.IEnumerable<>).Namespace!)),
            SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(typeof(Delivery.Abstractions.IApiResponse).Namespace!))
        };

        protected override TypeDeclarationSyntax GetClassDeclaration()
        {
            var classDeclaration = base.GetClassDeclaration();

            classDeclaration = classDeclaration.AddMembers(Properties);

            return classDeclaration;
        }

        private MemberDeclarationSyntax[] GetProperties()
        {
            var singleProperties = ClassDefinition.Properties
                .Where(x => !x.TypeName.Contains(nameof(IEnumerable)))
                .Select(element => SyntaxFactory
                    .PropertyDeclaration(SyntaxFactory.ParseTypeName(element.TypeName), element.Identifier + SingleSuffix)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .WithExpressionBody(
                        SyntaxFactory.ArrowExpressionClause(
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.IdentifierName(element.Identifier),
                                            SyntaxFactory.GenericName(SyntaxFactory.Identifier(OfTypeName))
                                                .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(
                                                    SyntaxFactory.SingletonSeparatedList<TypeSyntax>(SyntaxFactory.IdentifierName(element.TypeName)))))),
                                    SyntaxFactory.IdentifierName(FirstOrDefaultName)))))
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));

            var enumeratedProperties = ClassDefinition.Properties
                .Where(x => x.TypeName.Contains(nameof(IEnumerable)))
                .Select(element =>
                {
                    var typeName = GetNonEnumerableTypeName(element.TypeName);
                    return SyntaxFactory
                        .PropertyDeclaration(SyntaxFactory.ParseTypeName(element.TypeName), element.Identifier + TypedSuffix)
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .WithExpressionBody(
                            SyntaxFactory.ArrowExpressionClause(
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.IdentifierName(element.Identifier),
                                        SyntaxFactory.GenericName(SyntaxFactory.Identifier(OfTypeName))
                                            .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(
                                                SyntaxFactory.SingletonSeparatedList<TypeSyntax>(SyntaxFactory.IdentifierName(typeName))))))))
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
                });

            return singleProperties.Union(enumeratedProperties).ToArray<MemberDeclarationSyntax>();
        }

        private static string GetNonEnumerableTypeName(string typeName) => Regex.Match(typeName, "[^\\>|\\<]\\w+\\b(?<!\\bIEnumerable)").Value;
    }
}
