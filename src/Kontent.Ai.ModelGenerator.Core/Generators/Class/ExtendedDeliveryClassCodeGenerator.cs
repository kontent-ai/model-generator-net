using System;
using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Contract;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kontent.Ai.ModelGenerator.Core.Generators.Class;

public class ExtendedDeliveryClassCodeGenerator : DeliveryClassCodeGenerator
{
    private readonly IUserMessageLogger _userMessageLogger;
    private readonly bool _generateStructuredModularContent;

    public ExtendedDeliveryClassCodeGenerator(
        ClassDefinition classDefinition,
        string classFilename,
        bool generateStructuredModularContent,
        IUserMessageLogger userMessageLogger,
        string @namespace = DefaultNamespace)
        : base(classDefinition, classFilename, @namespace)
    {
        _generateStructuredModularContent = generateStructuredModularContent;
        _userMessageLogger = userMessageLogger;
    }

    protected override TypeDeclarationSyntax GetClassDeclaration()
    {
        try
        {
            ClassDefinition.AddSystemProperty();
        }
        catch (InvalidOperationException)
        {
            _userMessageLogger.LogWarning(
                $"Can't add 'System' property. It's in collision with existing element in Content Type '{ClassDefinition.ClassName}'.");
        }

        var classDeclaration = base.GetClassDeclaration();

        if (_generateStructuredModularContent)
        {
            var baseType = SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(nameof(IContentItem)));
            classDeclaration = (TypeDeclarationSyntax)classDeclaration.AddBaseListTypes(baseType);
        }

        return classDeclaration;
    }
}
