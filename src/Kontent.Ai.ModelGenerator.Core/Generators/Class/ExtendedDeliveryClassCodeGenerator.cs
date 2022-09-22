using Kontent.Ai.ModelGenerator.Core.Common;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kontent.Ai.ModelGenerator.Core.Generators.Class;

public class ExtendedDeliveryClassCodeGenerator : DeliveryClassCodeGenerator
{
    public ExtendedDeliveryClassCodeGenerator(ClassDefinition classDefinition, string classFilename, string @namespace = DefaultNamespace)
        : base(classDefinition, classFilename, @namespace)
    {
    }

    protected override TypeDeclarationSyntax GetClassDeclaration()
    {
        ClassDefinition.TryAddSystemProperty();

        var classDeclaration = base.GetClassDeclaration();

        var baseType = SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(ContentItemClassCodeGenerator.DefaultContentItemClassName));
        classDeclaration = (TypeDeclarationSyntax)classDeclaration.AddBaseListTypes(baseType);

        return classDeclaration;
    }
}
