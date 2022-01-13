using System;
using Kentico.Kontent.Management.Models.Types.Elements;
using Kentico.Kontent.ModelGenerator.Core.Common;
using Kentico.Kontent.ModelGenerator.Core.Configuration;

namespace Kentico.Kontent.ModelGenerator.Core.Helpers
{
    public static class ElementTypeHelper
    {
        public static string GetElementType(CodeGeneratorOptions options, string elementType, ElementMetadataBase managementElement)
        {
            Validate(options, elementType, managementElement);

            if (options.ContentManagementApi && elementType == "modular_content" && managementElement.Type == ElementMetadataType.Subpages)
            {
                return "subpages";
            }

            if (options.StructuredModel && Property.IsContentTypeSupported(elementType + Property.StructuredSuffix, options.ContentManagementApi))
            {
                elementType += Property.StructuredSuffix;
            }

            return elementType;
        }

        private static void Validate(CodeGeneratorOptions options, string elementType, ElementMetadataBase managementElement)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (elementType == null)
            {
                throw new ArgumentNullException(nameof(elementType));
            }

            if (options.ContentManagementApi && managementElement == null)
            {
                throw new ArgumentNullException(nameof(managementElement));
            }
        }
    }
}
