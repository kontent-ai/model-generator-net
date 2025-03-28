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

    public static readonly IDictionary<string, string> DeliveryEnvironmentIdMappings = new Dictionary<string, string>
    {
        { "-i", $"{nameof(DeliveryOptions)}:{nameof(DeliveryOptions.EnvironmentId)}" },
        { "--environmentid", $"{nameof(DeliveryOptions)}:{nameof(DeliveryOptions.EnvironmentId)}" },
        { "-p", $"{nameof(DeliveryOptions)}:{nameof(DeliveryOptions.EnvironmentId)}" }, // Backwards compatibility
        {"--environmentid", $"{nameof(DeliveryOptions)}:{nameof(DeliveryOptions.EnvironmentId)}" } // Backwards compatibility
    };

    public static readonly IDictionary<string, string> ManagementEnvironmentIdMappings = new Dictionary<string, string>
    {
        { "-i", $"{nameof(ManagementOptions)}:{nameof(ManagementOptions.EnvironmentId)}" },
        { "--environmentid", $"{nameof(ManagementOptions)}:{nameof(ManagementOptions.EnvironmentId)}" },
        { "-p", $"{nameof(ManagementOptions)}:{nameof(ManagementOptions.EnvironmentId)}" }, // Backwards compatibility
        {"--environmentid", $"{nameof(ManagementOptions)}:{nameof(ManagementOptions.EnvironmentId)}" } // Backwards compatibility
    };

    public static readonly IEnumerable<string> AllMappingsKeys =
        GeneralMappings.Keys
            .Union(DeliveryEnvironmentIdMappings.Keys)
            .Union(ManagementEnvironmentIdMappings.Keys);
}
