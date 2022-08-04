using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.Management.Configuration;
using Kontent.Ai.ModelGenerator.Core.Configuration;

namespace Kontent.Ai.ModelGenerator;

internal static class ArgHelpers
{
    private static readonly ProgramOptionsData ManagementProgramOptionsData = new ProgramOptionsData(typeof(ManagementOptions), "management-sdk-net");
    private static readonly ProgramOptionsData DeliveryProgramOptionsData = new ProgramOptionsData(typeof(DeliveryOptions), "delivery-sdk-net");

    public static IDictionary<string, string> GetSwitchMappings(string[] args)
    {
        var generalMappings = new Dictionary<string, string>
            {
                { "-n", nameof(CodeGeneratorOptions.Namespace) },
                { "-o", nameof(CodeGeneratorOptions.OutputDir) },
                { "-f", nameof(CodeGeneratorOptions.FileNameSuffix) },
                { "-g", nameof(CodeGeneratorOptions.GeneratePartials) },
                { "-b", nameof(CodeGeneratorOptions.BaseClass) }
            };

        var deliveryMappings = new Dictionary<string, string>
            {
                { "-p", $"{DeliveryProgramOptionsData.OptionsName}:{nameof(DeliveryOptions.ProjectId)}" },
                {"--projectid", $"{DeliveryProgramOptionsData.OptionsName}:{nameof(DeliveryOptions.ProjectId)}" }, // Backwards compatibility
                { "-s", nameof(CodeGeneratorOptions.StructuredModel) },
                { "-t", nameof(CodeGeneratorOptions.WithTypeProvider) }
            };

        var managementMappings = new Dictionary<string, string>
            {
                { "-p", $"{ManagementProgramOptionsData.OptionsName}:{nameof(ManagementOptions.ProjectId)}" },
                {"--projectid", $"{ManagementProgramOptionsData.OptionsName}:{nameof(ManagementOptions.ProjectId)}" }, // Backwards compatibility
                { "-m", nameof(CodeGeneratorOptions.ManagementApi) },
                { "-k", $"{ManagementProgramOptionsData.OptionsName}:{nameof(ManagementOptions.ApiKey)}" },
                { "--apikey", $"{ManagementProgramOptionsData.OptionsName}:{nameof(ManagementOptions.ApiKey)}" } // Backwards compatibility
            };

        return generalMappings
            .Union(ContainsManageApiArg() ? managementMappings : deliveryMappings)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        bool ContainsManageApiArg() =>
            args.Where((value, index) => (value is "-m" or "--managementapi") && index + 1 < args.Length && args[index + 1] == "true").Any();
    }

    public static bool ContainsUnsupportedArg(string[] args, IDictionary<string, string> usedMappings)
    {
        var containsUnsupportedArg = false;
        var codeGeneratorOptionsProperties = typeof(CodeGeneratorOptions).GetProperties()
            .Where(p => p.PropertyType != ManagementProgramOptionsData.Type && p.PropertyType != DeliveryProgramOptionsData.Type)
            .Select(p => p.Name.ToLower())
            .ToList();

        foreach (var arg in args.Where(a => a.StartsWith('-')))
        {
            if (!usedMappings.ContainsKey(arg) &&
                IsOptionPropertyUnsupported(ManagementProgramOptionsData, arg) &&
                IsOptionPropertyUnsupported(DeliveryProgramOptionsData, arg) &&
                IsOptionPropertyUnsupported(codeGeneratorOptionsProperties, arg))
            {
                Console.WriteLine($"Unsupported parameter: {arg}");
                containsUnsupportedArg = true;
            }
        }

        return containsUnsupportedArg;
    }

    public static UsedSdkInfo GetUsedSdkInfo(bool managementApi) =>
        managementApi ? ManagementProgramOptionsData.UsedSdkInfo : DeliveryProgramOptionsData.UsedSdkInfo;

    private static bool IsOptionPropertyUnsupported(ProgramOptionsData programOptionsData, string arg) =>
        IsOptionPropertyUnsupported(programOptionsData.OptionProperties.Select(prop => $"{programOptionsData.OptionsName}:{prop.Name}"), arg);

    private static bool IsOptionPropertyUnsupported(IEnumerable<string> optionProperties, string arg) =>
        optionProperties.All(prop => $"--{prop}" != arg);

    private class ProgramOptionsData
    {
        public IEnumerable<PropertyInfo> OptionProperties { get; }
        public string OptionsName { get; }
        public UsedSdkInfo UsedSdkInfo { get; set; }
        public Type Type { get; }

        public ProgramOptionsData(Type type, string sdkName)
        {
            UsedSdkInfo = new UsedSdkInfo(type, sdkName);
            Type = type;
            OptionProperties = type.GetProperties();
            OptionsName = type.Name;
        }
    }
}
