using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Newtonsoft.Json.Serialization;

namespace Kentico.Kontent.ModelGenerator.Core
{
    public class ClassCodeGenerator
    {
        public const string DEFAULT_NAMESPACE = "KenticoKontentModels";

        public ClassDefinition ClassDefinition { get; }

        public string ClassFilename { get; }

        public bool CustomPartial { get; }

        public string Namespace { get; }

        public bool OverwriteExisting { get; }

        public ClassCodeGenerator(ClassDefinition classDefinition, string classFilename, string @namespace = DEFAULT_NAMESPACE, bool customPartial = false)
        {
            ClassDefinition = classDefinition ?? throw new ArgumentNullException(nameof(classDefinition));
            ClassFilename = string.IsNullOrEmpty(classFilename) ? ClassDefinition.ClassName : classFilename;
            CustomPartial = customPartial;
            Namespace = string.IsNullOrEmpty(@namespace) ? DEFAULT_NAMESPACE : @namespace;
            OverwriteExisting = !CustomPartial;
        }

        public string GenerateCode(bool cmApi = false)
        {
            var cmApiUsings = new[]
            {
                SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName($"{nameof(Newtonsoft)}.{nameof(Newtonsoft.Json)}")),
                SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(typeof(Management.Models.Items.ContentItemModel).Namespace!)),
                SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(typeof(Management.Models.Assets.AssetModel).Namespace!))
            };

            var deliveryUsings = new[]
            {
                SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(typeof(Delivery.Abstractions.IApiResponse).Namespace!))
            };

            var usings = new[]
            {
                SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(nameof(System))),
                SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(typeof(System.Collections.Generic.IEnumerable<>).Namespace!)),
            }.Concat(cmApi ? cmApiUsings : deliveryUsings).ToArray();

            MemberDeclarationSyntax[] properties = ClassDefinition.Properties.OrderBy(p => p.Identifier).Select((element) =>
                {
                    var property = SyntaxFactory
                        .PropertyDeclaration(SyntaxFactory.ParseTypeName(element.TypeName), element.Identifier)
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .AddAccessorListAccessors(
                            SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                            SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                        );

                    if (cmApi)
                    {
                        property = property.AddAttributeLists(
                                SyntaxFactory.AttributeList(
                                    SyntaxFactory.SingletonSeparatedList(
                                        SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(nameof(JsonProperty)))
                                            .WithArgumentList(
                                                SyntaxFactory.AttributeArgumentList(
                                                    SyntaxFactory.SingletonSeparatedList(
                                                        SyntaxFactory.AttributeArgument(
                                                            SyntaxFactory.LiteralExpression(
                                                                SyntaxKind.StringLiteralExpression,
                                                                SyntaxFactory.Literal(element.Codename)))))))));
                    }

                    return property;
                }
            ).ToArray();

            MemberDeclarationSyntax[] propertyCodenameConstants = ClassDefinition.PropertyCodenameConstants.OrderBy(p => p.Codename).Select(element =>
                      SyntaxFactory.FieldDeclaration(
                              SyntaxFactory.VariableDeclaration(
                                      SyntaxFactory.ParseTypeName("string"),
                                      SyntaxFactory.SeparatedList(new[] {
                                        SyntaxFactory.VariableDeclarator(
                                            SyntaxFactory.Identifier($"{TextHelpers.GetValidPascalCaseIdentifierName(element.Codename)}Codename"),
                                            null,
                                            SyntaxFactory.EqualsValueClause(SyntaxFactory.LiteralExpression( SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(element.Codename)))
                                        )
                                      })
                                  )
                              )
                          .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                          .AddModifiers(SyntaxFactory.Token(SyntaxKind.ConstKeyword))
            ).ToArray();

            var classDeclaration = SyntaxFactory.ClassDeclaration(ClassDefinition.ClassName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword));

            if (!CustomPartial && !cmApi)
            {
                var classCodenameConstant = SyntaxFactory.FieldDeclaration(
                                SyntaxFactory.VariableDeclaration(
                                        SyntaxFactory.ParseTypeName("string"),
                                        SyntaxFactory.SeparatedList(new[] {
                                            SyntaxFactory.VariableDeclarator(
                                                SyntaxFactory.Identifier("Codename"),
                                                null,
                                                SyntaxFactory.EqualsValueClause(SyntaxFactory.LiteralExpression( SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(ClassDefinition.Codename)))
                                            )
                                        })
                                    )
                                )
                            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                            .AddModifiers(SyntaxFactory.Token(SyntaxKind.ConstKeyword));

                classDeclaration = classDeclaration.AddMembers(classCodenameConstant);
            }

            if (!cmApi)
            {
                classDeclaration = classDeclaration.AddMembers(propertyCodenameConstants);
            }

            classDeclaration = classDeclaration.AddMembers(properties);

            var description = SyntaxFactory.Comment(
@"// This code was generated by a kontent-generators-net tool 
// (see https://github.com/Kentico/kontent-generators-net).
// 
// Changes to this file may cause incorrect behavior and will be lost if the code is regenerated. 
// For further modifications of the class, create a separate file with the partial class." + Environment.NewLine + Environment.NewLine
);

            CompilationUnitSyntax cu = SyntaxFactory.CompilationUnit()
                .AddUsings(usings)
                .AddMembers(
                    SyntaxFactory.NamespaceDeclaration(SyntaxFactory.IdentifierName(Namespace))
                        .AddMembers(classDeclaration)
                );

            if (!CustomPartial)
            {
                cu = cu.WithLeadingTrivia(description);
            }

            AdhocWorkspace cw = new AdhocWorkspace();
            return Formatter.Format(cu, cw).ToFullString().NormalizeLineEndings();
        }
    }
}
