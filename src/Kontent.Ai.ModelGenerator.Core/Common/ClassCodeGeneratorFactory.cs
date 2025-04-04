﻿using System;
using Kontent.Ai.ModelGenerator.Core.Configuration;
using Kontent.Ai.ModelGenerator.Core.Contract;
using Kontent.Ai.ModelGenerator.Core.Generators.Class;

namespace Kontent.Ai.ModelGenerator.Core.Common;

public class ClassCodeGeneratorFactory : IClassCodeGeneratorFactory
{
    public ClassCodeGenerator CreateClassCodeGenerator(
        CodeGeneratorOptions options,
        ClassDefinition classDefinition,
        string classFilename,
        IUserMessageLogger logger,
        bool customPartial = false)
    {
        ArgumentNullException.ThrowIfNull(options);

        ArgumentNullException.ThrowIfNull(classDefinition);

        ArgumentNullException.ThrowIfNull(classFilename);

        ArgumentNullException.ThrowIfNull(logger);

        if (customPartial)
        {
            return new PartialClassCodeGenerator(classDefinition, classFilename, options.Namespace);
        }

        if (options.ManagementApi())
        {
            return new ManagementClassCodeGenerator(classDefinition, classFilename, options.Namespace);
        }

        return options.ExtendedDeliveryModels()
            ? new ExtendedDeliveryClassCodeGenerator(classDefinition, classFilename, options.IsStructuredModelModularContent(), logger, options.Namespace)
            : new DeliveryClassCodeGenerator(classDefinition, classFilename, options.Namespace);
    }
}
