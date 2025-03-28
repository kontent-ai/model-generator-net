using Kontent.Ai.ModelGenerator.Core.Common;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kontent.Ai.ModelGenerator.Core.Generators.Class;

public class DeliveryClassCodeGenerator(ClassDefinition classDefinition, string classFilename, string @namespace = ClassCodeGenerator.DefaultNamespace) : DeliveryClassCodeGeneratorBase(classDefinition, classFilename, @namespace)
{
    protected override TypeDeclarationSyntax GetClassDeclaration()
    {
        var classDeclaration = base.GetClassDeclaration();

        classDeclaration = classDeclaration.AddMembers(ClassCodenameConstant);
        classDeclaration = classDeclaration.AddMembers(PropertyCodenameConstants);
        classDeclaration = classDeclaration.AddMembers(Properties);

        return classDeclaration;
    }

    protected override UsingDirectiveSyntax[] GetApiUsings() =>
    [
        SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(nameof(System))),
        SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(typeof(System.Collections.Generic.IEnumerable<>).Namespace!)),
        SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(typeof(Delivery.Abstractions.IApiResponse).Namespace!))
    ];

    private FieldDeclarationSyntax ClassCodenameConstant => GetFieldDeclaration("string", "Codename", ClassDefinition.Codename);
}
