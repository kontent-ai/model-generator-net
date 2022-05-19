using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kentico.Kontent.Management.Extensions;
using Kentico.Kontent.Management.Models.Types;
using Kentico.Kontent.Management.Models.Types.Elements;
using Kentico.Kontent.ModelGenerator.Core.Configuration;
using Kentico.Kontent.ModelGenerator.Core.Generators.Class;

namespace Kentico.Kontent.ModelGenerator.Core.Helpers
{
    public static class ContentTypeToDeliverTypeNameMapper
    {
        public static string Map(ElementMetadataBase el, List<ContentTypeModel> contentTypes, CodeGeneratorOptions options)
        {
            var linkedItemsElement = el.ToElement<LinkedItemsElementMetadataModel>();
            var linkedTypeCodename = linkedItemsElement.AllowedTypes.Count() == 1 && !options.ExtendedDeliverPreviewModels
                ? contentTypes.FirstOrDefault(type => linkedItemsElement.AllowedTypes.First().Id == type.Id).Codename
                : null;

            return linkedTypeCodename == null
                ? $"{nameof(IEnumerable)}<{ContentItemClassCodeGenerator.DefaultContentItemClassName}>"
                : $"{nameof(IEnumerable)}<{TextHelpers.GetValidPascalCaseIdentifierName(linkedTypeCodename)}>";
        }
    }
}
