﻿using Kontent.Ai.ModelGenerator.Core.Configuration;
using Kontent.Ai.ModelGenerator.Core.Generators.Class;

namespace Kontent.Ai.ModelGenerator.Core.Common;

public interface IClassCodeGeneratorFactory
{
    ClassCodeGenerator CreateClassCodeGenerator(CodeGeneratorOptions options, ClassDefinition classDefinition, string classFilename, bool customPartial = false);
}
