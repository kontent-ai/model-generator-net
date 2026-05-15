using System;
using System.Collections.Generic;
using System.Linq;
using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.Management.Configuration;
using Kontent.Ai.ModelGenerator.Core.Configuration;

namespace Kontent.Ai.ModelGenerator.Options;

/// <summary>
/// Argument mappings for the CLI tool. Two short-flag sets coexist — Delivery and Management —
/// because <c>-i</c> / <c>--environmentId</c> target different config sections depending on mode.
/// The mode is detected by <see cref="ArgHelpers"/> from the args list itself
/// (presence of <c>-m</c> / <c>--management</c>) before configuration binding.
/// </summary>
internal class ArgMappingsRegister
{
    public const string ManagementShortFlag = "-m";
    public const string ManagementLongFlag = "--management";

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

    public static readonly IDictionary<string, string> ManagementMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "-i", $"{nameof(ManagementOptions)}:{nameof(ManagementOptions.EnvironmentId)}" },
        { "--environmentId", $"{nameof(ManagementOptions)}:{nameof(ManagementOptions.EnvironmentId)}" },
        { "-k", $"{nameof(ManagementOptions)}:{nameof(ManagementOptions.ApiKey)}" },
        { "--apiKey", $"{nameof(ManagementOptions)}:{nameof(ManagementOptions.ApiKey)}" },
    };

    /// <summary>
    /// All recognized argument keys across both modes plus the mode-switch flags themselves
    /// (which don't bind to a config property but are still legal). Used by
    /// <see cref="ArgHelpers.ContainsValidArgs"/> to reject typos early.
    /// </summary>
    public static readonly ISet<string> AllMappingsKeys = new HashSet<string>(
        GeneralMappings.Keys
            .Union(DeliveryEnvironmentIdMappings.Keys)
            .Union(ManagementMappings.Keys)
            .Union([ManagementShortFlag, ManagementLongFlag]),
        StringComparer.OrdinalIgnoreCase);
}
