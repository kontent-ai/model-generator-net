using System.Collections.Generic;
using System.Linq;
using Kontent.Ai.ModelGenerator.Core.Configuration;

namespace Kontent.Ai.ModelGenerator.Options;

internal class UsedMappings
{
    public IDictionary<string, string> Mappings { get; }
    public DesiredModelsType DesiredModelsType { get; }

    public UsedMappings(IDictionary<string, string> mappings, DesiredModelsType desiredModelsType)
    {
        Mappings = ArgMappingsRegister.GeneralMappings
            .Union(mappings)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        DesiredModelsType = desiredModelsType;
    }
}
