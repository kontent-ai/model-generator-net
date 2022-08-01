using System;
using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Configuration;

namespace Kontent.Ai.ModelGenerator.Core.Helpers
{
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
}
