using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.Management.Configuration;
using Kontent.Ai.ModelGenerator.Core.Configuration;

namespace Kontent.Ai.ModelGenerator.Options;

internal static class ArgHelpers
{
    private static readonly ProgramOptionsData ManagementProgramOptionsData = new ProgramOptionsData(typeof(ManagementOptions), "management-sdk-net");
    private static readonly ProgramOptionsData DeliveryProgramOptionsData = new ProgramOptionsData(typeof(DeliveryOptions), "delivery-sdk-net");
    private static readonly ProgramOptionsData ExtendedDeliveryProgramOptionsData = new ProgramOptionsData(typeof(ManagementOptions), typeof(DeliveryOptions), "delivery-sdk-net");

    public static IDictionary<string, string> GetSwitchMappings(string[] args) => ArgMappingsRegister.GeneralMappings
        .Union(GetSpecificSwitchMappings(args))
        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

    public static bool ContainsValidArgs(string[] args)
    {
        var containsValidArgs = true;
        var codeGeneratorOptionsProperties = typeof(CodeGeneratorOptions).GetProperties()
            .Where(p =>
                p.PropertyType != ManagementProgramOptionsData.Type &&
                p.PropertyType != DeliveryProgramOptionsData.Type &&
                p.PropertyType != ExtendedDeliveryProgramOptionsData.Type)
            .Select(p => p.Name.ToLower())
            .ToList();

        foreach (var arg in args.Where(a =>
            a.StartsWith('-') &&
            !ArgMappingsRegister.AllMappingsKeys.Contains(a) &&
            !IsOptionPropertyValid(ManagementProgramOptionsData, a) &&
            !IsOptionPropertyValid(DeliveryProgramOptionsData, a) &&
            !IsOptionPropertyValid(ExtendedDeliveryProgramOptionsData, a) &&
            !IsOptionPropertyValid(codeGeneratorOptionsProperties, a)))
        {
            Console.Error.WriteLine($"Unsupported parameter: {arg}");
            containsValidArgs = false;
        }

        return containsValidArgs;
    }

    public static UsedSdkInfo GetUsedSdkInfo(DesiredModelsType desiredModelsType) => desiredModelsType switch
    {
        DesiredModelsType.Delivery => DeliveryProgramOptionsData.UsedSdkInfo,
        DesiredModelsType.Management => ManagementProgramOptionsData.UsedSdkInfo,
        DesiredModelsType.ExtendedDelivery => ExtendedDeliveryProgramOptionsData.UsedSdkInfo,
        _ => throw new ArgumentOutOfRangeException(nameof(desiredModelsType))
    };

    private static IDictionary<string, string> GetSpecificSwitchMappings(string[] args)
    {
        var managementDecidingArgs = new DecidingArgs("-m", GetPrefixedMappingName(nameof(CodeGeneratorOptions.ManagementApi)));
        var extendedDeliverDecidingArgs = new DecidingArgs("-e", GetPrefixedMappingName(nameof(CodeGeneratorOptions.ExtendedDeliverModels)));
        var extendedDeliverPreviewDecidingArgs = new DecidingArgs("-r", GetPrefixedMappingName(nameof(CodeGeneratorOptions.ExtendedDeliverPreviewModels)));

        for (var i = 0; i < args.Length; i++)
        {
            if (i + 1 >= args.Length || args[i + 1] != "true") continue;

            if (args[i] == managementDecidingArgs.ShorthandedArgName || args[i] == managementDecidingArgs.FullArgName)
            {
                return ArgMappingsRegister.ManagementMappings;
            }

            if (args[i] == extendedDeliverDecidingArgs.ShorthandedArgName || args[i] == extendedDeliverDecidingArgs.FullArgName ||
                args[i] == extendedDeliverPreviewDecidingArgs.ShorthandedArgName || args[i] == extendedDeliverPreviewDecidingArgs.FullArgName)
            {
                return ArgMappingsRegister.ExtendedDeliveryMappings;
            }
        }

        return ArgMappingsRegister.DeliveryMappings;
    }

    private static bool IsOptionPropertyValid(ProgramOptionsData programOptionsData, string arg) =>
        IsOptionPropertyValid(programOptionsData.OptionProperties.Select(prop => $"{programOptionsData.OptionsName}:{prop.Name}"), arg);

    private static bool IsOptionPropertyValid(IEnumerable<string> optionProperties, string arg) =>
        optionProperties.Any(prop => GetPrefixedMappingName(prop, false) == arg);

    private static string GetPrefixedMappingName(string mappingName, bool toLower = true) => $"--{(toLower ? mappingName.ToLower() : mappingName)}";

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

        public ProgramOptionsData(Type optionsType, Type sdkType, string sdkName)
        {
            UsedSdkInfo = new UsedSdkInfo(sdkType, sdkName);
            Type = optionsType;
            OptionProperties = optionsType.GetProperties();
            OptionsName = optionsType.Name;
        }
    }

    private class DecidingArgs
    {
        public string ShorthandedArgName { get; }
        public string FullArgName { get; }

        public DecidingArgs(string shorthandedArgName, string fullArgName)
        {
            ShorthandedArgName = shorthandedArgName;
            FullArgName = fullArgName;
        }
    }
}
