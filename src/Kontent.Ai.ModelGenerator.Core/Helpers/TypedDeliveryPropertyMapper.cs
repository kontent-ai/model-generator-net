using Kontent.Ai.Management.Models.Types.Elements;
using Kontent.Ai.Management.Models.Types;
using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Configuration;
using Kontent.Ai.ModelGenerator.Core.Generators.Class;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kontent.Ai.Management.Extensions;

namespace Kontent.Ai.ModelGenerator.Core.Helpers;

public static class TypedDeliveryPropertyMapper
{
    public static ICollection<Property> Map(ElementMetadataBase el, List<ContentTypeModel> contentTypes, CodeGeneratorOptions options)
    {
        Validate(contentTypes, options);

        var linkedItemsElement = el.ToElement<LinkedItemsElementMetadataModel>();
        if (linkedItemsElement == null)
        {
            throw new ArgumentNullException();
        }

        if (!linkedItemsElement.AllowedTypes.Any() ||
            linkedItemsElement.AllowedTypes.Count() > 1)
        {
            return new List<Property>();
        }

        var allowedContentType = GetAllowedContentType(linkedItemsElement.AllowedTypes.First().Id.Value, contentTypes);
        var allowedContentTypeCodename = TextHelpers.GetValidPascalCaseIdentifierName(allowedContentType.Codename);

        if (linkedItemsElement.ItemCountLimit is { Condition: LimitType.Exactly, Value: 1 })
        {
            var singleAllowedContentTypeCodename = options.ExtendedDeliverPreviewModels
                ? GetEnumerablePropertyTypeName(ContentItemClassCodeGenerator.DefaultContentItemClassName)
                : allowedContentTypeCodename;

            return CreateEnumerableProperties(el, singleAllowedContentTypeCodename);
        }

        var multipleAllowedContentTypeCodename = options.ExtendedDeliverPreviewModels
            ? ContentItemClassCodeGenerator.DefaultContentItemClassName
            : allowedContentTypeCodename;

        return CreateEnumerableProperties(el, multipleAllowedContentTypeCodename, allowedContentTypeCodename);
    }

    private static void Validate(List<ContentTypeModel> contentTypes, CodeGeneratorOptions options)
    {
        if (contentTypes == null)
        {
            throw new ArgumentNullException(nameof(contentTypes));
        }

        if (!contentTypes.Any())
        {
            throw new ArgumentException($"{nameof(contentTypes)} cannot be empty");
        }

        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (!options.ExtendedDeliveryModels())
        {
            throw new ArgumentException("Can be used only for extended delivery models.");
        }
    }

    private static ContentTypeModel GetAllowedContentType(Guid allowedTypeId, List<ContentTypeModel> contentTypes)
    {
        var allowedType = contentTypes.FirstOrDefault(type => allowedTypeId == type.Id);

        if (allowedType == null)
        {
            throw new ArgumentException("Could not find allowed type.");
        }

        return allowedType;
    }

    private static string GetEnumerablePropertyTypeName(string typeName) => $"{nameof(IEnumerable)}<{typeName}>";

    private static string GetCompoundPropertyName(string codename, string typeName) => $"{codename}_{typeName}";

    private static List<Property> GetPropertyAsList(Property property) => new List<Property> { property };

    private static List<Property> CreateEnumerableProperties(ElementMetadataBase element, string elementType, string propertyName) => new List<Property>
    {
        Property.FromContentTypeElement(
            element,
            GetEnumerablePropertyTypeName(elementType),
            GetCompoundPropertyName(TextHelpers.GetValidPascalCaseIdentifierName(element.Codename), propertyName))
    };

    private static List<Property> CreateEnumerableProperties(ElementMetadataBase element, string elementType) => new List<Property>
    {
        Property.FromContentTypeElement(element, elementType)
    };
}
