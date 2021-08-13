using System;
using Kentico.Kontent.ModelGenerator.Core.Configuration;

namespace Kentico.Kontent.ModelGenerator.Core
{
    public static class ClassCodeGeneratorFactory
    {
        public static ClassCodeGeneratorBase CreateClassCodeGenerator(CodeGeneratorOptions options, ClassDefinition classDefinition, string classFilename, bool customPartial = false)
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

            return options.ContentManagementApi
                ? new ManagementClassCodeGenerator(classDefinition, classFilename, options.Namespace)
                : new DeliveryClassCodeGenerator(classDefinition, classFilename, options.Namespace, customPartial);
        }

    }
}
