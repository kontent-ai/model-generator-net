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
}
