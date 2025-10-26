using Kontent.Ai.ModelGenerator.Core.Common;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kontent.Ai.ModelGenerator.Core.Generators.Class;

public class DeliveryClassCodeGenerator(ClassDefinition classDefinition, string classFilename, string @namespace = ClassCodeGenerator.DefaultNamespace) : ClassCodeGenerator(classDefinition, classFilename, @namespace)
{
    protected override bool IsRecord => true;

    protected override bool UseFileScopedNamespace => true;

    protected override TypeDeclarationSyntax GetClassDeclaration()
    {
        var recordDeclaration = base.GetClassDeclaration();

        // Add IElementsModel interface
        recordDeclaration = (TypeDeclarationSyntax)recordDeclaration.AddBaseListTypes(
            SyntaxFactory.SimpleBaseType(SyntaxFactory.IdentifierName("IElementsModel")));

        // Add properties
        recordDeclaration = recordDeclaration.AddMembers(Properties);

        return recordDeclaration;
    }

    protected override UsingDirectiveSyntax[] GetApiUsings() =>
    [
        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Collections.Generic")),
        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Text.Json.Serialization")),
        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Kontent.Ai.Delivery.Abstractions")),
        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Kontent.Ai.Delivery.ContentItems")),
        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Kontent.Ai.Delivery.ContentItems.RichText")),
        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Kontent.Ai.Delivery.SharedModels"))
    ];
}
