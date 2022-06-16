using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kentico.Kontent.Management.Extensions;
using Kentico.Kontent.Management.Models.Types;
using Kentico.Kontent.Management.Models.Types.Elements;
using Kentico.Kontent.ModelGenerator.Core.Common;
using Kentico.Kontent.ModelGenerator.Core.Configuration;
using Kentico.Kontent.ModelGenerator.Core.Generators.Class;

namespace Kentico.Kontent.ModelGenerator.Core.Helpers
{
    public static class ContentTypeToDeliverTypeNameMapper
    {
        public static IEnumerable<Property> Map(ElementMetadataBase el, List<ContentTypeModel> contentTypes, CodeGeneratorOptions options)
        {
            var linkedItemsElement = el.ToElement<LinkedItemsElementMetadataModel>();
            if (linkedItemsElement == null)
            {
                throw new ArgumentNullException();
            }

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

            if (!linkedItemsElement.AllowedTypes.Any() || options.ExtendedDeliverPreviewModels)
            {

                yield return Property.FromContentTypeElement(el, GetEnumerablePropertyName(ContentItemClassCodeGenerator.DefaultContentItemClassName));
                yield break;
            }

            var elementCodename = TextHelpers.GetValidPascalCaseIdentifierName(el.Codename);
            if (linkedItemsElement.AllowedTypes.Count() > 1)
            {
                foreach (var allowedType in linkedItemsElement.AllowedTypes)
                {
                    var allowedTypeName = TextHelpers.GetValidPascalCaseIdentifierName(GetAllowedContentType(allowedType.Id.Value, contentTypes).Codename);
                    if (linkedItemsElement.ItemCountLimit is { Condition: LimitType.Exactly, Value: 1 } or { Condition: LimitType.AtMost, Value: 1 })
                    {
                        continue;
                    }

                    yield return Property.FromContentTypeElement(el, GetEnumerablePropertyName(allowedTypeName), GetCompoundPropertyName(elementCodename, allowedTypeName));
                }
                yield break;
            }

            var typeName = TextHelpers.GetValidPascalCaseIdentifierName(GetAllowedContentType(linkedItemsElement.AllowedTypes.First().Id.Value, contentTypes).Codename);
            if (linkedItemsElement.ItemCountLimit.Condition == LimitType.Exactly && linkedItemsElement.ItemCountLimit.Value == 1)
            {
                yield return Property.FromContentTypeElement(el, typeName);
                yield break;
            }

            yield return Property.FromContentTypeElement(el, GetEnumerablePropertyName(typeName), GetCompoundPropertyName(elementCodename, typeName));
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

        private static string GetEnumerablePropertyName(string typeName) => $"{nameof(IEnumerable)}<{typeName}>";

        private static string GetCompoundPropertyName(string codename, string typeName) => $"{codename}_{typeName}";
    }
}