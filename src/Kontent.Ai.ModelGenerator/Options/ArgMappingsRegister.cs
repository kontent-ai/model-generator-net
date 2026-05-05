using System;
using System.Collections.Generic;
using System.Linq;
using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.ModelGenerator.Core.Configuration;

namespace Kontent.Ai.ModelGenerator.Options;

/// <summary>
/// Argument mappings for the CLI tool.
/// </summary>
internal class ArgMappingsRegister
{
    public static readonly IDictionary<string, string> GeneralMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "-n", nameof(CodeGeneratorOptions.Namespace) },
        { "-o", nameof(CodeGeneratorOptions.OutputDir) },
        { "-b", nameof(CodeGeneratorOptions.BaseRecord) },
        { "-r", nameof(CodeGeneratorOptions.BaseRecord) },
        { "--nullability", nameof(CodeGeneratorOptions.Nullability) },
#pragma warning disable CS0618 // CodeGeneratorOptions.WithTypeProvider is intentionally kept as [Obsolete] for one more major cycle.
        { "-t", nameof(CodeGeneratorOptions.WithTypeProvider) }
#pragma warning restore CS0618
    };

    public static readonly IDictionary<string, string> DeliveryEnvironmentIdMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "-i", $"{nameof(DeliveryOptions)}:{nameof(DeliveryOptions.EnvironmentId)}" },
        { "--environmentId", $"{nameof(DeliveryOptions)}:{nameof(DeliveryOptions.EnvironmentId)}" },
        { "-p", $"{nameof(DeliveryOptions)}:{nameof(DeliveryOptions.EnvironmentId)}" }, // Backwards compatibility
        {"--projectid", $"{nameof(DeliveryOptions)}:{nameof(DeliveryOptions.EnvironmentId)}" } // Backwards compatibility
    };

    public static readonly ISet<string> AllMappingsKeys = new HashSet<string>(
        GeneralMappings.Keys
            .Union(DeliveryEnvironmentIdMappings.Keys),
        StringComparer.OrdinalIgnoreCase);
}
