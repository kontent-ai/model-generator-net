using Kontent.Ai.ModelGenerator.Core.Common;

namespace Kontent.Ai.ModelGenerator.Core.Contract;

public interface IClassDefinitionFactory
{
    ClassDefinition CreateClassDefinition(string codename);
}
