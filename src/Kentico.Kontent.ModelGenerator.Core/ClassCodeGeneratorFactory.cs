using System;
using Kentico.Kontent.ModelGenerator.Core.Configuration;

namespace Kentico.Kontent.ModelGenerator.Core
{
    public static class ClassCodeGeneratorFactory
    {
        public static ClassCodeGenerator CreateClassCodeGenerator(CodeGeneratorOptions options, ClassDefinition classDefinition, string classFilename, bool customPartial = false)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (classDefinition == null)
            {
                throw new ArgumentNullException(nameof(classDefinition));
            }

            if (classFilename == null)
            {
                throw new ArgumentNullException(nameof(classFilename));
            }

            if (options.ContentManagementApi)
            {
                return new ManagementClassCodeGenerator(classDefinition, classFilename, options.Namespace);
            }

            return customPartial
                ? new PartialClassCodeGenerator(classDefinition, classFilename, options.Namespace)
                : new DeliveryClassCodeGenerator(classDefinition, classFilename, options.Namespace);
        }
    }
}
