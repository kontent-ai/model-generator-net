using System;
using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Configuration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;

namespace Kontent.Ai.ModelGenerator.Core.Helpers;

public static class DeliveryElementHelper
{
    public static string GetElementType(CodeGeneratorOptions options, string elementType)
    {
        Validate(options, elementType);

        if (options.StructuredModel && Property.IsContentTypeSupported(elementType + Property.StructuredSuffix))
        {
            elementType += Property.StructuredSuffix;
        }

        return elementType;
    }

    internal static PropertyDeclarationSyntax EnsureAttributesForDisplayTimezones(this PropertyDeclarationSyntax syntax, Property element)
    {
        if (element is not DisplayTimezoneProperty displayTimezone)
            return syntax;

        var converterName = SyntaxFactory.ParseName(nameof(Delivery.Abstractions.DisplayTimzoneConverterAttribute));
        var converterAttribute = SyntaxFactory.Attribute(converterName);

        var jsonPropertyName = SyntaxFactory.ParseName(nameof(JsonPropertyAttribute));
        var jsonPropertyArguments = SyntaxFactory.ParseAttributeArgumentList($"(\"{displayTimezone.DateTimeElementCodename}\")");
        var jsonPropertyAttribute = SyntaxFactory.Attribute(jsonPropertyName, jsonPropertyArguments);

        var attributeList = new SeparatedSyntaxList<AttributeSyntax>();
        attributeList = attributeList.Add(converterAttribute);
        attributeList = attributeList.Add(jsonPropertyAttribute);
        var list = SyntaxFactory.AttributeList(attributeList);

        return syntax.AddAttributeLists(list);
    }

    private static void Validate(CodeGeneratorOptions options, string elementType)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (options.ManagementApi)
        {
            throw new InvalidOperationException("Cannot generate structured model for the Management models.");
        }

        if (elementType == null)
        {
            throw new ArgumentNullException(nameof(elementType));
        }
    }
}
