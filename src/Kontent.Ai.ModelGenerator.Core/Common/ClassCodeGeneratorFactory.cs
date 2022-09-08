using System;
using Kontent.Ai.ModelGenerator.Core.Configuration;
using Kontent.Ai.ModelGenerator.Core.Generators.Class;

namespace Kontent.Ai.ModelGenerator.Core.Common;

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

        if (customPartial)
        {
            return new PartialClassCodeGenerator(classDefinition, classFilename, options.Namespace);
        }

        return options.ManagementApi
            ? new ManagementClassCodeGenerator(classDefinition, classFilename, options.Namespace)
            : new DeliveryClassCodeGenerator(classDefinition, classFilename, options.Namespace);
    }
}
