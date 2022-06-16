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
        public static Property Map(ElementMetadataBase el, List<ContentTypeModel> contentTypes, CodeGeneratorOptions options)
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


            if (linkedItemsElement.AllowedTypes.Count() != 1)
            {
                return null;
            }

            var typeName = TextHelpers.GetValidPascalCaseIdentifierName(GetAllowedContentType(linkedItemsElement.AllowedTypes.First().Id.Value, contentTypes).Codename);
            if (linkedItemsElement.ItemCountLimit.Condition == LimitType.Exactly && linkedItemsElement.ItemCountLimit.Value == 1)
            {
                return Property.FromContentTypeElement(el, typeName);
            }

            if (linkedItemsElement.ItemCountLimit == null || !(linkedItemsElement.ItemCountLimit.Condition == LimitType.AtMost && linkedItemsElement.ItemCountLimit.Value == 1))
            {
                return Property.FromContentTypeElement(el, GetEnumerablePropertyTypeName(typeName));
            }

            return Property.FromContentTypeElement(el, GetEnumerablePropertyTypeName(ContentItemClassCodeGenerator.DefaultContentItemClassName));
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
    }
}