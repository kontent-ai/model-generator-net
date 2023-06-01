using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Configuration;
using Kontent.Ai.ModelGenerator.Core.Generators.Class;

namespace Kontent.Ai.ModelGenerator.Core.Contract;

public interface IClassCodeGeneratorFactory
{
    ClassCodeGenerator CreateClassCodeGenerator(
        CodeGeneratorOptions options,
        ClassDefinition classDefinition,
        string classFilename,
        IUserMessageLogger logger,
        bool customPartial = false);
}
