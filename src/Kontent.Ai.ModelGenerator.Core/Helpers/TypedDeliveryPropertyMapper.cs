using Kontent.Ai.Management.Models.Types.Elements;
using Kontent.Ai.Management.Models.Types;
using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Configuration;
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

        if (!IsAllowedContentTypesOptionSupported(elementOptions.AllowedTypes))
        {
            typedProperty = null;
            return false;
        }

        var allowedContentType = GetAllowedContentType(elementOptions.AllowedTypes.First().Id.Value, contentTypes);
        var allowedContentTypeCodename = TextHelpers.GetValidPascalCaseIdentifierName(allowedContentType.Codename);

        if (IsItemCountLimitOptionSupported(elementOptions.ItemCountLimit))
        {
            typedProperty = Property.FromContentTypeElement(el, allowedContentTypeCodename);
            return true;
        }

        typedProperty = CreateProperty(el, allowedContentTypeCodename);
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

    private static bool IsAllowedContentTypesOptionSupported(IEnumerable<Reference> allowedTypes) =>
        allowedTypes != null && allowedTypes.Count() == 1;

    private static bool IsItemCountLimitOptionSupported(LimitModel itemCountLimit) => itemCountLimit is
    {
        Value: 1,
        Condition: LimitType.Exactly or LimitType.AtMost
    };

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

    private static Property CreateProperty(ElementMetadataBase element, string typeName) => Property.FromContentTypeElement(
        element,
        TextHelpers.GetEnumerableType(typeName),
        GetCompoundPropertyName(TextHelpers.GetUpperSnakeCasedIdentifierName(element.Codename), typeName));

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
