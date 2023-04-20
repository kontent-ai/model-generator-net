using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.ModelGenerator.Core.Common;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kontent.Ai.ModelGenerator.Core.Generators.Class;

public class ExtendedDeliveryClassCodeGenerator : DeliveryClassCodeGenerator
{
    private readonly bool _generateStructuredModularContent;

    public ExtendedDeliveryClassCodeGenerator(ClassDefinition classDefinition, string classFilename, bool generateStructuredModularContent, string @namespace = DefaultNamespace)
        : base(classDefinition, classFilename, @namespace)
    {
        _generateStructuredModularContent = generateStructuredModularContent;
    }

    protected override TypeDeclarationSyntax GetClassDeclaration()
    {
        ClassDefinition.TryAddSystemProperty();

        var classDeclaration = base.GetClassDeclaration();

        if (_generateStructuredModularContent)
        {
            var baseType = SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(nameof(IContentItem)));
            classDeclaration = (TypeDeclarationSyntax)classDeclaration.AddBaseListTypes(baseType);
        }

        return classDeclaration;
    }
}
