using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.Management.Configuration;
using Kontent.Ai.ModelGenerator.Core.Configuration;

namespace Kontent.Ai.ModelGenerator.Options;

internal static class ArgHelpers
{
    private const char NameAndValueSeparator = '=';
    private const char NamePrefix = '-';

    private static readonly ProgramOptionsData<ManagementOptions> ManagementProgramOptionsData =
        new ProgramOptionsData<ManagementOptions>(typeof(ManagementOptions), "management-sdk-net");

    private static readonly ProgramOptionsData<DeliveryOptions> DeliveryProgramOptionsData =
        new ProgramOptionsData<DeliveryOptions>(typeof(DeliveryOptions), "delivery-sdk-net");

    private static readonly ProgramOptionsData<ManagementOptions> ExtendedDeliveryProgramOptionsData =
        new ProgramOptionsData<ManagementOptions>(typeof(ManagementOptions), typeof(DeliveryOptions), "delivery-sdk-net");

    public static IDictionary<string, string> GetSwitchMappings(string[] args) => ArgMappingsRegister.GeneralMappings
        .Union(GetSpecificSwitchMappings(args))
        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

    public static ArgValidationResult ContainsValidArgs(string[] args)
    {
        var codeGeneratorOptionsProperties = typeof(CodeGeneratorOptions).GetProperties()
            .Where(p =>
                p.PropertyType != ManagementProgramOptionsData.Type &&
                p.PropertyType != DeliveryProgramOptionsData.Type &&
                p.PropertyType != ExtendedDeliveryProgramOptionsData.Type)
            .Select(p => p.Name.ToLower())
            .ToList();

        var unsupportedArgs = args.Where(a =>
        {
            if (!StartsWithArgumentName(a)) return false;

            var argumentName = SplitArgument(a).FirstOrDefault();
            return !ArgMappingsRegister.AllMappingsKeys.Contains(argumentName) &&
                   !IsOptionPropertyValid(ManagementProgramOptionsData, argumentName) &&
                   !IsOptionPropertyValid(DeliveryProgramOptionsData, argumentName) &&
                   !IsOptionPropertyValid(ExtendedDeliveryProgramOptionsData, argumentName) &&
                   !IsOptionPropertyValid(codeGeneratorOptionsProperties, argumentName);
        });

        return new ArgValidationResult(unsupportedArgs);
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
        var managementDecidingArgs = new DecidingArgs("m", GetPrefixedMappingName(nameof(CodeGeneratorOptions.ManagementApi)));
        var extendedDeliverDecidingArgs = new DecidingArgs("e", GetPrefixedMappingName(nameof(CodeGeneratorOptions.ExtendedDeliveryModels)));

        for (var i = 0; i < args.Length; i++)
        {
            if (!StartsWithArgumentName(args[i])) continue;

            string argValue, argName;
            if (args[i].Contains(NameAndValueSeparator))
            {
                var argPair = SplitArgument(args[i]);

                argName = argPair[0];
                argValue = argPair[1];
            }
            else
            {
                argName = args[i];
                argValue = args[i + 1];
            }

            if (!bool.TrueString.Equals(argValue, StringComparison.OrdinalIgnoreCase))
                continue;

            if (argName == managementDecidingArgs.ShorthandedArgName ||
                argName == managementDecidingArgs.FullArgName ||
                argName == extendedDeliverDecidingArgs.ShorthandedArgName ||
                argName == extendedDeliverDecidingArgs.FullArgName)
            {
                return ArgMappingsRegister.ManagementProjectIdMappings;
            }
        }

        return ArgMappingsRegister.DeliveryProjectIdMappings;
    }

    private static bool IsOptionPropertyValid<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>
        (ProgramOptionsData<T> programOptionsData, string arg) =>
            IsOptionPropertyValid(programOptionsData.OptionProperties.Select(prop => $"{programOptionsData.OptionsName}:{prop.Name}"), arg);

    private static bool IsOptionPropertyValid(IEnumerable<string> optionProperties, string arg) =>
        optionProperties.Any(prop => GetPrefixedMappingName(prop, false) == arg);

    private static string GetPrefixedMappingName(string mappingName, bool toLower = true) => $"--{(toLower ? mappingName.ToLower() : mappingName)}";

    private static string[] SplitArgument(string arg) => arg.Split(NameAndValueSeparator);

    private static bool StartsWithArgumentName(string arg) => arg.StartsWith(NamePrefix);

    private class ProgramOptionsData<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>
    {
        public IEnumerable<PropertyInfo> OptionProperties { get; }
        public string OptionsName { get; }
        public UsedSdkInfo UsedSdkInfo { get; set; }
        public Type Type { get; }

        public ProgramOptionsData(
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
            Type type,
            string sdkName)
        {
            UsedSdkInfo = new UsedSdkInfo(type, sdkName);
            Type = type;
            OptionProperties = type.GetProperties();
            OptionsName = type.Name;
        }

        public ProgramOptionsData(
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
            Type optionsType,
            Type sdkType,
            string sdkName)
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
            ShorthandedArgName = $"{NamePrefix}{shorthandedArgName}";
            FullArgName = fullArgName;
        }
    }
}
