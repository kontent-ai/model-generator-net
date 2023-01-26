using Kontent.Ai.Management.Models.Types.Elements;
using Kontent.Ai.Management.Models.Types;
using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Configuration;
using Kontent.Ai.ModelGenerator.Core.Generators.Class;
using System;
using System.Collections.Generic;
using System.Linq;
using Kontent.Ai.Management.Extensions;
using Kontent.Ai.Management.Models.Shared;

namespace Kontent.Ai.ModelGenerator.Core.Helpers;

public static class TypedDeliveryPropertyMapper
{
    public static bool TryMap(
        ElementMetadataBase el,
        List<ContentTypeModel> contentTypes,
        CodeGeneratorOptions options,
        out Property typedProperty)
    {
        Validate(contentTypes, el, options);

        var elementOptions = GetElementOptions(el);

        if (!elementOptions.AllowedTypes.Any() ||
            elementOptions.AllowedTypes.Count() > 1)
        {
            typedProperty = null;
            return false;
        }

        var allowedContentType = GetAllowedContentType(elementOptions.AllowedTypes.First().Id.Value, contentTypes);
        var allowedContentTypeCodename = TextHelpers.GetValidPascalCaseIdentifierName(allowedContentType.Codename);

        if (elementOptions.ItemCountLimit is { Condition: LimitType.Exactly, Value: 1 })
        {
            var singleAllowedContentTypeCodename = options.ExtendedDeliverPreviewModels
                ? TextHelpers.GetEnumerableType(ContentItemClassCodeGenerator.DefaultContentItemClassName)
                : allowedContentTypeCodename;

            typedProperty = Property.FromContentTypeElement(el, singleAllowedContentTypeCodename);
            return true;
        }

        var multipleAllowedContentTypeCodename = options.ExtendedDeliverPreviewModels
            ? ContentItemClassCodeGenerator.DefaultContentItemClassName
            : allowedContentTypeCodename;

        typedProperty = CreateProperty(el, multipleAllowedContentTypeCodename, allowedContentTypeCodename);
        return true;
    }

    private static void Validate(List<ContentTypeModel> contentTypes, ElementMetadataBase element, CodeGeneratorOptions options)
    {
        if (contentTypes == null)
        {
            throw new ArgumentNullException(nameof(contentTypes));
        }

        if (!contentTypes.Any())
        {
            throw new ArgumentException($"{nameof(contentTypes)} cannot be empty");
        }

        if (element == null)
        {
            throw new ArgumentNullException(nameof(element));
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

    private static string GetCompoundPropertyName(string codename, string typeName) => $"{codename}_{typeName}";

    private static Property CreateProperty(ElementMetadataBase element, string elementType, string propertyName) => Property.FromContentTypeElement(
        element,
        TextHelpers.GetEnumerableType(elementType),
        GetCompoundPropertyName(TextHelpers.GetValidPascalCaseIdentifierName(element.Codename), propertyName));

    private static (IEnumerable<Reference> AllowedTypes, LimitModel ItemCountLimit) GetElementOptions(ElementMetadataBase el)
    {
        if (el.Type == ElementMetadataType.LinkedItems)
        {
            var linkedItemsElement = el.ToElement<LinkedItemsElementMetadataModel>();
            ValidateTypedElement(linkedItemsElement);

            return (linkedItemsElement.AllowedTypes, linkedItemsElement.ItemCountLimit);
        }

        if (el.Type == ElementMetadataType.Subpages)
        {
            var subpagesElement = el.ToElement<SubpagesElementMetadataModel>();
            ValidateTypedElement(subpagesElement);

            return (subpagesElement.AllowedContentTypes, subpagesElement.ItemCountLimit);
        }

        throw new ArgumentException();

        static void ValidateTypedElement(ElementMetadataBase element)
        {
            if (element == null)
            {
                throw new ArgumentNullException();
            }
        }
    }
}
