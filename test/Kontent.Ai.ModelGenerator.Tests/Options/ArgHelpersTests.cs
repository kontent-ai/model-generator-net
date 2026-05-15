using Kontent.Ai.ModelGenerator.Core.Configuration;
using Kontent.Ai.ModelGenerator.Options;
using Microsoft.Extensions.Configuration;

namespace Kontent.Ai.ModelGenerator.Tests.Options;

public class ArgHelpersTests
{
    [Theory]
    [InlineData("--environmentid=123")]
    [InlineData("--environmentId=123")]
    [InlineData("--ENVIRONMENTID=123")]
    public void ContainsValidArgs_EnvironmentIdCasingVariants_ReturnsTrue(string argument)
    {
        var result = ArgHelpers.ContainsValidArgs([argument]);

        result.Should().BeTrue();
    }

    [Fact]
    public void GetSwitchMappings_EnvironmentIdMapping_IsCaseInsensitive()
    {
        var mappings = ArgHelpers.GetSwitchMappings([]);

        mappings.ContainsKey("--environmentid").Should().BeTrue();
        mappings.ContainsKey("--environmentId").Should().BeTrue();
        mappings.Keys.Count(key => key.Equals("--environmentid", StringComparison.OrdinalIgnoreCase)).Should().Be(1);
    }

    public static TheoryData<string[]> ValidNullabilityArgs => new()
    {
        new[] { "--nullability=strict" },
        new[] { "--nullability=semantic" },
        new[] { "--Nullability=Semantic" },
        new[] { "--nullability", "strict" },
        new[] { "--nullability", "semantic" },
    };

    [Theory]
    [MemberData(nameof(ValidNullabilityArgs))]
    public void ContainsValidArgs_NullabilityArg_ReturnsTrue(string[] args)
    {
        var result = ArgHelpers.ContainsValidArgs(args);

        result.Should().BeTrue();
    }

    public static TheoryData<string[]> InvalidNullabilityArgs => new()
    {
        new[] { "--nullability=bogus" },
        new[] { "--nullability=" },
        new[] { "--nullability", "bogus" },
        new[] { "--nullability" },
        new[] { "--nullability", "--baseRecord", "Foo" },
    };

    [Theory]
    [MemberData(nameof(InvalidNullabilityArgs))]
    public void ContainsValidArgs_InvalidNullabilityValue_ReturnsFalse(string[] args)
    {
        var result = ArgHelpers.ContainsValidArgs(args);

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("strict", NullabilityMode.Strict)]
    [InlineData("semantic", NullabilityMode.Semantic)]
    [InlineData("Semantic", NullabilityMode.Semantic)]
    [InlineData("SEMANTIC", NullabilityMode.Semantic)]
    public void ConfigurationBinding_NullabilityArg_BindsCodeGeneratorOptions(string value, NullabilityMode expected)
    {
        var args = new[] { $"--nullability={value}" };
        var configuration = new ConfigurationBuilder()
            .AddCommandLine(args, ArgHelpers.GetSwitchMappings(args))
            .Build();

        var options = configuration.Get<CodeGeneratorOptions>();

        options.Nullability.Should().Be(expected);
    }

    [Fact]
    public void ConfigurationBinding_NullabilityArgOmitted_DefaultsToStrict()
    {
        var configuration = new ConfigurationBuilder()
            .AddCommandLine([], ArgHelpers.GetSwitchMappings([]))
            .Build();

        var options = configuration.Get<CodeGeneratorOptions>() ?? new CodeGeneratorOptions();

        options.Nullability.Should().Be(NullabilityMode.Strict);
    }

    [Theory]
    [InlineData("-m")]
    [InlineData("--management")]
    [InlineData("--MANAGEMENT")]
    [InlineData("-M")]
    public void IsManagementMode_FlagPresent_ReturnsTrue(string flag)
    {
        ArgHelpers.IsManagementMode([flag]).Should().BeTrue();
    }

    [Fact]
    public void IsManagementMode_NoFlag_ReturnsFalse()
    {
        ArgHelpers.IsManagementMode(["-i", "abc"]).Should().BeFalse();
    }

    [Fact]
    public void StripModeSwitches_RemovesModeFlagsOnly()
    {
        var result = ArgHelpers.StripModeSwitches(["-m", "-i", "abc", "--management", "-k", "xyz"]);

        result.Should().Equal("-i", "abc", "-k", "xyz");
    }

    [Fact]
    public void GetSwitchMappings_ManagementMode_MapsIToManagementEnvironmentId()
    {
        var mappings = ArgHelpers.GetSwitchMappings(["-m"]);

        mappings["-i"].Should().Be("ManagementOptions:EnvironmentId");
        mappings["--environmentId"].Should().Be("ManagementOptions:EnvironmentId");
        mappings["-k"].Should().Be("ManagementOptions:ApiKey");
        mappings["--apiKey"].Should().Be("ManagementOptions:ApiKey");
    }

    [Fact]
    public void GetSwitchMappings_DeliveryMode_MapsIToDeliveryEnvironmentId()
    {
        var mappings = ArgHelpers.GetSwitchMappings([]);

        mappings["-i"].Should().Be("DeliveryOptions:EnvironmentId");
        mappings.Should().NotContainKey("-k");
        mappings.Should().NotContainKey("--apiKey");
    }

    public static TheoryData<string[]> ManagementSwitchArgs => new()
    {
        new[] { "-m" },
        new[] { "--management" },
        new[] { "-m", "--apiKey=xxx" },
    };

    [Theory]
    [MemberData(nameof(ManagementSwitchArgs))]
    public void ContainsValidArgs_ManagementSwitch_Accepted(string[] args)
    {
        ArgHelpers.ContainsValidArgs(args).Should().BeTrue();
    }

    [Fact]
    public void ContainsValidArgs_ManagementOptionsLongForm_Accepted()
    {
        ArgHelpers.ContainsValidArgs(["--ManagementOptions:EnvironmentId=abc"]).Should().BeTrue();
        ArgHelpers.ContainsValidArgs(["--ManagementOptions:ApiKey=xyz"]).Should().BeTrue();
    }

    [Fact]
    public void ConfigurationBinding_ManagementMode_PopulatesManagementOptions()
    {
        var args = new[] { "-m", "-i", "abc-123", "-k", "secret" };
        var bindableArgs = ArgHelpers.StripModeSwitches(args);
        var configuration = new ConfigurationBuilder()
            .AddCommandLine(bindableArgs, ArgHelpers.GetSwitchMappings(args))
            .Build();

        var options = configuration.Get<CodeGeneratorOptions>();

        options.ManagementOptions.Should().NotBeNull();
        options.ManagementOptions.EnvironmentId.Should().Be("abc-123");
        options.ManagementOptions.ApiKey.Should().Be("secret");
        // -i in management mode should NOT also populate DeliveryOptions.
        options.DeliveryOptions?.EnvironmentId.Should().BeNullOrEmpty();
    }
}
