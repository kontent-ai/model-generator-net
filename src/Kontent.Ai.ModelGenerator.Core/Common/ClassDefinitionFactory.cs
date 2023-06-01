using System;
using Kontent.Ai.ModelGenerator.Core.Contract;

namespace Kontent.Ai.ModelGenerator.Core.Common;

public class ClassDefinitionFactory : IClassDefinitionFactory
{
    public ClassDefinition CreateClassDefinition(string codename)
    {
        if (string.IsNullOrWhiteSpace(codename))
        {
            throw new ArgumentException("Class codeName must be a non null and not white space string.", nameof(codename));
        }

        return new ClassDefinition(codename);
    }
}
