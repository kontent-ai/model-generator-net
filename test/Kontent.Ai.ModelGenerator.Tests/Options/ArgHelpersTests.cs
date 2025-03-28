using System.Reflection;
using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.Management.Configuration;
using Kontent.Ai.ModelGenerator.Core.Configuration;
using Kontent.Ai.ModelGenerator.Options;

namespace Kontent.Ai.ModelGenerator.Tests.Options;

public class ArgHelpersTests
{
    private static IEnumerable<string> GeneralOptionArgs => ToLower(new List<string>
        {
            nameof(CodeGeneratorOptions.Namespace),
            nameof(CodeGeneratorOptions.OutputDir),
            nameof(CodeGeneratorOptions.FileNameSuffix),
            nameof(CodeGeneratorOptions.GeneratePartials),
            nameof(CodeGeneratorOptions.BaseClass)
        });

    private static IDictionary<string, string> ExpectedManagementMappings => new Dictionary<string, string>
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
            { "-m", nameof(CodeGeneratorOptions.ManagementApi) },
            { "-i", $"{nameof(ManagementOptions)}:{nameof(ManagementOptions.EnvironmentId)}" },
            { "--environmentid", $"{nameof(ManagementOptions)}:{nameof(ManagementOptions.EnvironmentId)}" },
            { "-p", $"{nameof(ManagementOptions)}:{nameof(ManagementOptions.EnvironmentId)}" }, // Backwards compatibility
            { "--projectid", $"{nameof(ManagementOptions)}:{nameof(ManagementOptions.EnvironmentId)}" } // Backwards compatibility
};

    private static IDictionary<string, string> ExpectedDeliveryMappings => new Dictionary<string, string>
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
        { "-m", nameof(CodeGeneratorOptions.ManagementApi) },
        { "-i", $"{nameof(DeliveryOptions)}:{nameof(DeliveryOptions.EnvironmentId)}" },
        { "--environmentid", $"{nameof(DeliveryOptions)}:{nameof(DeliveryOptions.EnvironmentId)}" },
        { "-p", $"{nameof(DeliveryOptions)}:{nameof(DeliveryOptions.EnvironmentId)}" }, // Backwards compatibility
        { "--projectid", $"{nameof(DeliveryOptions)}:{nameof(DeliveryOptions.EnvironmentId)}" } // Backwards compatibility
    };

    [Theory]
    [MemberData(nameof(DeliveryApiSwitchOptions))]
    public void GetSwitchMappings_DeliveryApiSwitchOptions_ReturnsDeliveryMappings(string[] args)
    {
        var result = ArgHelpers.GetSwitchMappings(args);

        result.Should().BeEquivalentTo(ExpectedDeliveryMappings);
    }

    [Theory]
    [MemberData(nameof(MapiSwitchOptions))]
    public void GetSwitchMappings_MapiSwitchOptions_ReturnsManagementMappings(string[] args)
    {
        var result = ArgHelpers.GetSwitchMappings(args);

        result.Should().BeEquivalentTo(ExpectedManagementMappings);
    }

    [Theory]
    [MemberData(nameof(ExtendedDeliveryApiSwitchOptions))]
    public void GetSwitchMappings_ExtendedDeliveryApiSwitchOptions_ReturnsManagementMappings(string[] args)
    {
        var result = ArgHelpers.GetSwitchMappings(args);

        result.Should().BeEquivalentTo(ExpectedManagementMappings);
    }

    [Theory]
    [MemberData(nameof(SupportedDeliveryOptions))]
    public void ContainsContainsValidArgs_SupportedDeliveryOptions_ReturnsTrue(string[] args)
    {
        var result = ArgHelpers.ContainsValidArgs(args);

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("-x")]
    [InlineData("--projectidX")]
    [InlineData("--DeliveryOptionsX:UseSecureAccess")]
    [InlineData("--DeliveryOptions:UseSecureAccessX")]
    public void ContainsContainsValidArgs_UnsupportedDeliveryOptions_ReturnsFalse(string arg)
    {
        var args = new[]
        {
            arg,
            "arg_value"
        };
        var result = ArgHelpers.ContainsValidArgs(args);

        result.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(SupportedManagementOptions))]
    public void ContainsContainsValidArgs_SupportedManagementOptions_ReturnsTrue(string[] args)
    {
        var result = ArgHelpers.ContainsValidArgs(args);

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("-x")]
    [InlineData("--contentmanagementapi")]
    [InlineData("--managementapiX")]
    [InlineData("--ManagementOptions:ApiKeyX")]
    [InlineData("--ManagementOptionsX:ApiKey")]
    public void ContainsContainsValidArgs_UnsupportedManagementOptions_ReturnsFalse(string arg)
    {
        var args = new[]
        {
            arg,
            "arg_value"
        };
        var result = ArgHelpers.ContainsValidArgs(args);

        result.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(SupportedExtendedDeliveryOptions))]
    public void ContainsContainsValidArgs_SupportedExtendedDeliveryOptions_ReturnsTrue(string[] args)
    {
        var result = ArgHelpers.ContainsValidArgs(args);

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("-x")]
    [InlineData("--contentmanagementapi")]
    [InlineData("--managementapix")]
    [InlineData("--ManagementOptions:ApiKeyX")]
    [InlineData("--ManagementOptionsX:ApiKey")]
    [InlineData("--DeliveryOptionsX:UseSecureAccess")]
    [InlineData("--DeliveryOptions:UseSecureAccessX")]
    public void ContainsContainsValidArgs_UnsupportedExtendedDeliveryOptions_ReturnsFalse(string arg)
    {
        var args = new[]
        {
            arg,
            "arg_value"
        };

        var result = ArgHelpers.ContainsValidArgs(args);

        result.Should().BeFalse();
    }

    [Fact]
    public void GetProgramOptionsData_ManagementApi_ReturnsManagementProgramOptionsData()
    {
        var result = ArgHelpers.GetUsedSdkInfo(DesiredModelsType.Management);

        AssertUsedSdkInfoResult(result, "management-sdk-net", typeof(ManagementOptions));
    }

    [Fact]
    public void GetProgramOptionsData_DeliveryApi_ReturnsDeliveryProgramOptionsData()
    {
        var result = ArgHelpers.GetUsedSdkInfo(DesiredModelsType.Delivery);

        AssertUsedSdkInfoResult(result, "delivery-sdk-net", typeof(DeliveryOptions));
    }

    [Fact]
    public void GetProgramOptionsData_ExtendedDeliveryApi_ReturnsExtendedDeliveryProgramOptionsData()
    {
        var result = ArgHelpers.GetUsedSdkInfo(DesiredModelsType.ExtendedDelivery);

        AssertUsedSdkInfoResult(result, "delivery-sdk-net", typeof(DeliveryOptions));
    }

    public static IEnumerable<object[]> DeliveryApiSwitchOptions()
    {
        var environmentId = Guid.NewGuid().ToString();

        yield return [new [] { "-p", environmentId, "-m", "False" }];
        yield return [new [] { "-p", environmentId, "-m", "false" }];
        yield return [new [] { "-p", environmentId, "-m=false" }];
        yield return [new [] { "-p", environmentId, "-m=False" }];
        yield return [new [] { "-p", environmentId }];
    }

    public static IEnumerable<object[]> MapiSwitchOptions()
    {
        var environmentId = Guid.NewGuid().ToString();

        yield return [new [] { "-p", environmentId, "-m", "True" }];
        yield return [new [] { "-p", environmentId, "-m", "true" }];
        yield return [new [] { "-p", environmentId, "-m=true" }];
        yield return [new [] { "-p", environmentId, "-m=True" }];
    }

    public static IEnumerable<object[]> ExtendedDeliveryApiSwitchOptions()
    {
        var environmentId = Guid.NewGuid().ToString();

        yield return [new [] { "-p", environmentId, "-e", "True" }];
        yield return [new [] { "-p", environmentId, "-e", "true" }];
        yield return [new [] { "-p", environmentId, "-e=true" }];
        yield return [new [] { "-p", environmentId, "-e=True" }];
    }

    public static IEnumerable<object[]> SupportedManagementOptions()
    {
        var args = AppendValuesToArgs(ExpectedManagementMappings)
            .Concat(AppendValuesToArgs(ToLower(new List<string>
            {
                nameof(CodeGeneratorOptions.ManagementApi)
            })))
            .Concat(AppendValuesToArgs(GeneralOptionArgs))
            .Concat(AppendValuesToArgs(typeof(ManagementOptions)))
            .ToArray();

        foreach (var arg in args)
        {
            yield return [arg];
        }
    }

    public static IEnumerable<object[]> SupportedDeliveryOptions()
    {
        var args = AppendValuesToArgs(ExpectedDeliveryMappings)
            .Concat(AppendValuesToArgs(ToLower(new List<string>
            {
                nameof(CodeGeneratorOptions.StructuredModel),
                nameof(CodeGeneratorOptions.WithTypeProvider)
            })))
            .Concat(AppendValuesToArgs(GeneralOptionArgs))
            .Concat(AppendValuesToArgs(typeof(DeliveryOptions)))
            .ToArray();

        foreach (var arg in args)
        {
            yield return [arg];
        }
    }

    public static IEnumerable<object[]> SupportedExtendedDeliveryOptions()
    {
        var args = AppendValuesToArgs(ExpectedManagementMappings)
            .Concat(AppendValuesToArgs(ToLower(new List<string>
            {
                nameof(CodeGeneratorOptions.StructuredModel),
                nameof(CodeGeneratorOptions.WithTypeProvider)
            })))
            .Concat(AppendValuesToArgs(GeneralOptionArgs))
            .Concat(AppendValuesToArgs(typeof(ManagementOptions)))
            .ToArray();

        foreach (var arg in args)
        {
            yield return [arg];
        }
    }

    private static IEnumerable<IEnumerable<string>> AppendValuesToArgs(IDictionary<string, string> mappings) => AppendValuesToArgs(mappings.Keys);

    private static IEnumerable<IEnumerable<string>> AppendValuesToArgs(Type type) =>
        AppendValuesToArgs(type.GetProperties().Select(p => $"{type.Name}:{p.Name}"));

    private static IEnumerable<string> ToLower(IEnumerable<string> args) => args.Select(a => a.ToLower());

    private static IEnumerable<IEnumerable<string>> AppendValuesToArgs(IEnumerable<string> args)
    {
        var argValue = "arg_value";
        foreach (var arg in args)
        {
            var argName = arg.StartsWith('-') ? arg : $"--{arg}";

            yield return new List<string> { argName, argValue };
            yield return new List<string> { $"{argName}={argValue}" };
        }
    }

    private static void AssertUsedSdkInfoResult(UsedSdkInfo result, string expectedName, Type expectedType)
    {
        result.Name.Should().Be(expectedName);
        result.Version.Should().Be(Assembly.GetAssembly(expectedType).GetName().Version.ToString(3));
    }
}

