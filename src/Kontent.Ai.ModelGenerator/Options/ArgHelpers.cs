using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.Management.Configuration;
using Kontent.Ai.ModelGenerator.Core.Configuration;

namespace Kontent.Ai.ModelGenerator.Options;

/// <summary>
/// Argument helpers for the CLI tool. Supports both Delivery and Management modes; the latter
/// is opted into with the <c>-m</c> / <c>--management</c> flag.
/// </summary>
internal static class ArgHelpers
{
    private const char NameAndValueSeparator = '=';
    private const char NamePrefix = '-';

    private static readonly ProgramOptionsData<DeliveryOptions> DeliveryProgramOptionsData =
        new(typeof(DeliveryOptions), "delivery-sdk-net");

    private static readonly ProgramOptionsData<ManagementOptions> ManagementProgramOptionsData =
        new(typeof(ManagementOptions), "management-sdk-net");

    /// <summary>
    /// Returns true if <c>-m</c> or <c>--management</c> appears anywhere in <paramref name="args"/>.
    /// </summary>
    public static bool IsManagementMode(string[] args) => args.Any(IsModeSwitch);

    /// <summary>
    /// Strips mode-switch flags (<c>-m</c> / <c>--management</c>) from <paramref name="args"/>
    /// so the remaining list can be fed to the configuration system (which would otherwise
    /// reject them since they don't bind to a config property).
    /// </summary>
    public static string[] StripModeSwitches(string[] args) =>
        args.Where(a => !IsModeSwitch(a)).ToArray();

    private static bool IsModeSwitch(string arg) =>
        string.Equals(arg, ArgMappingsRegister.ManagementShortFlag, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(arg, ArgMappingsRegister.ManagementLongFlag, StringComparison.OrdinalIgnoreCase);

    public static IDictionary<string, string> GetSwitchMappings(string[] args)
    {
        var modeMappings = IsManagementMode(args)
            ? ArgMappingsRegister.ManagementMappings
            : ArgMappingsRegister.DeliveryEnvironmentIdMappings;

        return ArgMappingsRegister.GeneralMappings
            .Union(modeMappings)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.OrdinalIgnoreCase);
    }

    public static bool ContainsValidArgs(string[] args)
    {
        var containsValidArgs = true;
        var codeGeneratorOptionsProperties = typeof(CodeGeneratorOptions).GetProperties()
            .Where(p => p.PropertyType != DeliveryProgramOptionsData.Type
                        && p.PropertyType != ManagementProgramOptionsData.Type)
            .Select(p => p.Name)
            .ToList();

        var brokenArgs = args.Where(a =>
        {
            if (!StartsWithArgumentName(a)) return false;

            var argumentName = SplitArgument(a).FirstOrDefault();
            return !ArgMappingsRegister.AllMappingsKeys.Contains(argumentName) &&
                   !IsOptionPropertyValid(DeliveryProgramOptionsData, argumentName) &&
                   !IsOptionPropertyValid(ManagementProgramOptionsData, argumentName) &&
                   !IsOptionPropertyValid(codeGeneratorOptionsProperties, argumentName);
        });

        foreach (var arg in brokenArgs)
        {
            Console.Error.WriteLine($"Unsupported parameter: {arg}");
            containsValidArgs = false;
        }

        foreach (var error in ValidateEnumArgValues(args))
        {
            Console.Error.WriteLine(error);
            containsValidArgs = false;
        }

        return containsValidArgs;
    }

    private static IEnumerable<string> ValidateEnumArgValues(string[] args)
    {
        var enumKeys = BuildEnumArgKeyMap();
        if (enumKeys.Count == 0)
        {
            yield break;
        }

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (!StartsWithArgumentName(arg)) continue;

            string key;
            string value;
            var inlineSeparator = arg.IndexOf(NameAndValueSeparator);
            if (inlineSeparator >= 0)
            {
                key = arg[..inlineSeparator];
                value = arg[(inlineSeparator + 1)..];
            }
            else
            {
                key = arg;
                if (i + 1 < args.Length && !StartsWithArgumentName(args[i + 1]))
                {
                    value = args[++i];
                }
                else
                {
                    value = null;
                }
            }

            if (!enumKeys.TryGetValue(key, out var enumType)) continue;

            if (string.IsNullOrEmpty(value) || !Enum.TryParse(enumType, value, ignoreCase: true, out _))
            {
                var allowed = string.Join(", ", Enum.GetNames(enumType).Select(n => n.ToLowerInvariant()));
                yield return $"Invalid value for {key}: '{value ?? string.Empty}'. Allowed values: {allowed}.";
            }
        }
    }

    private static Dictionary<string, Type> BuildEnumArgKeyMap()
    {
        var enumProperties = typeof(CodeGeneratorOptions).GetProperties()
            .Where(p => p.PropertyType.IsEnum)
            .ToDictionary(p => p.Name, p => p.PropertyType, StringComparer.Ordinal);

        var keys = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        foreach (var (name, type) in enumProperties)
        {
            keys[$"--{name}"] = type;
        }
        foreach (var (cliKey, target) in ArgMappingsRegister.GeneralMappings)
        {
            if (enumProperties.TryGetValue(target, out var type))
            {
                keys[cliKey] = type;
            }
        }
        return keys;
    }

    public static UsedSdkInfo GetUsedSdkInfo(bool managementMode = false) =>
        managementMode ? ManagementProgramOptionsData.UsedSdkInfo : DeliveryProgramOptionsData.UsedSdkInfo;

    private static bool IsOptionPropertyValid<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>
        (ProgramOptionsData<T> programOptionsData, string arg) =>
            IsOptionPropertyValid(programOptionsData.OptionProperties.Select(prop => $"{programOptionsData.OptionsName}:{prop.Name}"), arg);

    private static bool IsOptionPropertyValid(IEnumerable<string> optionProperties, string arg) =>
        optionProperties.Any(prop =>
            string.Equals(GetPrefixedMappingName(prop), arg, StringComparison.OrdinalIgnoreCase));

    private static string GetPrefixedMappingName(string mappingName) => $"--{mappingName}";

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
