using System.Collections.Generic;
using System.Linq;
using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.Management.Configuration;
using Kontent.Ai.ModelGenerator.Core.Configuration;

namespace Kontent.Ai.ModelGenerator.Options;

internal class ArgMappingsRegister
{
    public static readonly IDictionary<string, string> GeneralMappings = new Dictionary<string, string>
    {
        { "-n", nameof(CodeGeneratorOptions.Namespace) },
        { "-o", nameof(CodeGeneratorOptions.OutputDir) },
        { "-f", nameof(CodeGeneratorOptions.FileNameSuffix) },
        { "-g", nameof(CodeGeneratorOptions.GeneratePartials) },
        { "-b", nameof(CodeGeneratorOptions.BaseClass) },
        { "-k", $"{nameof(ManagementOptions)}:{nameof(ManagementOptions.ApiKey)}" },
        { "--apikey", $"{nameof(ManagementOptions)}:{nameof(ManagementOptions.ApiKey)}" }, // Backwards compatibility
        { "-e", nameof(CodeGeneratorOptions.ExtendedDeliveryModels) },
        { "-s", nameof(CodeGeneratorOptions.StructuredModel) },
        { "-t", nameof(CodeGeneratorOptions.WithTypeProvider) },
        { "-m", nameof(CodeGeneratorOptions.ManagementApi) }
    };

    public static readonly IDictionary<string, string> DeliveryProjectIdMappings = new Dictionary<string, string>
    {
        { "-p", $"{nameof(DeliveryOptions)}:{nameof(DeliveryOptions.ProjectId)}" },
        {"--projectid", $"{nameof(DeliveryOptions)}:{nameof(DeliveryOptions.ProjectId)}" } // Backwards compatibility
    };

    public static readonly IDictionary<string, string> ManagementProjectIdMappings = new Dictionary<string, string>
    {
        { "-p", $"{nameof(ManagementOptions)}:{nameof(ManagementOptions.ProjectId)}" },
        {"--projectid", $"{nameof(ManagementOptions)}:{nameof(ManagementOptions.ProjectId)}" } // Backwards compatibility
    };

    public static readonly IEnumerable<string> AllMappingsKeys =
        GeneralMappings.Keys
            .Union(DeliveryProjectIdMappings.Keys)
            .Union(ManagementProjectIdMappings.Keys);
}
