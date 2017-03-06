using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace CloudModelGenerator
{
    public class ClassCodeGenerator
    {
        public const string DEFAULT_NAMESPACE = "KenticoCloudModels";

        public ClassDefinition ClassDefinition { get; }
        public string Namespace { get; }

        public ClassCodeGenerator(ClassDefinition classDefinition, string @namespace = DEFAULT_NAMESPACE)
        {
            if (classDefinition == null)
            {
                throw new ArgumentNullException(nameof(classDefinition));
            }

            ClassDefinition = classDefinition;
            Namespace = @namespace ?? DEFAULT_NAMESPACE;
        }

        public string GenerateCode()
        {
            var usings = new[]
            {
                SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("System")),
                SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("System.Collections.Generic")),
                SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("KenticoCloud.Delivery"))
            };

            var properties = ClassDefinition.Properties.Select(element =>
                SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName(element.TypeName), element.Identifier)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddAccessorListAccessors(
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                    )
            ).ToArray();

            var classDeclaration = SyntaxFactory.ClassDeclaration(ClassDefinition.ClassName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddMembers(properties);

            CompilationUnitSyntax cu = SyntaxFactory.CompilationUnit()
                .AddUsings(usings)
                .AddMembers(
                    SyntaxFactory.NamespaceDeclaration(SyntaxFactory.IdentifierName(Namespace))
                        .AddMembers(classDeclaration)
                );

            AdhocWorkspace cw = new AdhocWorkspace();
            return Formatter.Format(cu, cw).ToFullString();
        }
    }
}
