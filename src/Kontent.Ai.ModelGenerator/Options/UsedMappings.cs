using System.Collections.Generic;
using System.Linq;
using Kontent.Ai.ModelGenerator.Core.Configuration;

namespace Kontent.Ai.ModelGenerator.Options;

internal class UsedMappings
{
    public IDictionary<string, string> Mappings { get; }
    public UsedMappingsType UsedMappingsType { get; }

    public UsedMappings(IDictionary<string, string> mappings, UsedMappingsType usedMappingsType)
    {
        Mappings = ArgMappingsRegister.GeneralMappings
            .Union(mappings)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        UsedMappingsType = usedMappingsType;
    }
}
