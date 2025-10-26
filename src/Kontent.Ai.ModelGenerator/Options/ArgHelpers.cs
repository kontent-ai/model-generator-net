using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.ModelGenerator.Core.Configuration;

namespace Kontent.Ai.ModelGenerator.Options;

/// <summary>
/// Argument helpers for the CLI tool.
/// Modern beta version only supports Delivery SDK models.
/// </summary>
internal static class ArgHelpers
{
    private const char NameAndValueSeparator = '=';
    private const char NamePrefix = '-';

    private static readonly ProgramOptionsData<DeliveryOptions> DeliveryProgramOptionsData =
        new(typeof(DeliveryOptions), "delivery-sdk-net");

    public static IDictionary<string, string> GetSwitchMappings(string[] args) => ArgMappingsRegister.GeneralMappings
        .Union(ArgMappingsRegister.DeliveryEnvironmentIdMappings)
        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

    public static bool ContainsValidArgs(string[] args)
    {
        var containsValidArgs = true;
        var codeGeneratorOptionsProperties = typeof(CodeGeneratorOptions).GetProperties()
            .Where(p => p.PropertyType != DeliveryProgramOptionsData.Type)
            .Select(p => p.Name.ToLower())
            .ToList();

        var brokenArgs = args.Where(a =>
        {
            if (!StartsWithArgumentName(a)) return false;

            var argumentName = SplitArgument(a).FirstOrDefault();
            return !ArgMappingsRegister.AllMappingsKeys.Contains(argumentName) &&
                   !IsOptionPropertyValid(DeliveryProgramOptionsData, argumentName) &&
                   !IsOptionPropertyValid(codeGeneratorOptionsProperties, argumentName);
        });

        foreach (var arg in brokenArgs)
        {
            Console.Error.WriteLine($"Unsupported parameter: {arg}");
            containsValidArgs = false;
        }

        return containsValidArgs;
    }

    public static UsedSdkInfo GetUsedSdkInfo() => DeliveryProgramOptionsData.UsedSdkInfo;

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

    private class DecidingArgs(string shorthandedArgName, string fullArgName)
    {
        public string ShorthandedArgName { get; } = $"{NamePrefix}{shorthandedArgName}";
        public string FullArgName { get; } = fullArgName;
    }
}
