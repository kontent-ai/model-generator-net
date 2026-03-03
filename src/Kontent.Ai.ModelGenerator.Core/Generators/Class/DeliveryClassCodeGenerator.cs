using Kontent.Ai.ModelGenerator.Core.Common;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kontent.Ai.ModelGenerator.Core.Generators.Class;

public class DeliveryClassCodeGenerator(ClassDefinition classDefinition, string classFilename, string @namespace = ClassCodeGenerator.DefaultNamespace) : DeliveryClassCodeGeneratorBase(classDefinition, classFilename, @namespace)
{
    protected override bool IsRecord => true;

    protected override bool UseFileScopedNamespace => true;

    protected override TypeDeclarationSyntax GetClassDeclaration()
    {
        var recordDeclaration = base.GetClassDeclaration();

        // Add ContentTypeCodename attribute
        recordDeclaration = recordDeclaration.AddAttributeLists(
            SyntaxFactory.AttributeList(
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Attribute(
                        SyntaxFactory.IdentifierName("ContentTypeCodename"),
                        SyntaxFactory.AttributeArgumentList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.AttributeArgument(
                                    SyntaxFactory.LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        SyntaxFactory.Literal(ClassDefinition.Codename)))))))));

        // Add element codename constants and properties
        recordDeclaration = recordDeclaration.AddMembers(PropertyCodenameConstants);
        recordDeclaration = recordDeclaration.AddMembers(Properties);

        return recordDeclaration;
    }

    protected override UsingDirectiveSyntax[] GetApiUsings() =>
    [
        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Collections.Generic")),
        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Text.Json.Serialization")),
        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Kontent.Ai.Delivery.Abstractions")),
        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Kontent.Ai.Delivery.Attributes")),
        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Kontent.Ai.Delivery.ContentItems")),
        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Kontent.Ai.Delivery.ContentItems.RichText")),
        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Kontent.Ai.Delivery.SharedModels"))
    ];
}
