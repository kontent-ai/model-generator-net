using System;
using Kontent.Ai.ModelGenerator.Core.Contract;

namespace Kontent.Ai.ModelGenerator.Core.Common;

public class ClassDefinitionFactory : IClassDefinitionFactory
{
    public ClassDefinition CreateClassDefinition(string codename)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codename, nameof(codename));

        return new ClassDefinition(codename);
    }
}
