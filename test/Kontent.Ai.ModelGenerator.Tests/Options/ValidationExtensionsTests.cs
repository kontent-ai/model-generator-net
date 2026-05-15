using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.Management.Configuration;
using Kontent.Ai.ModelGenerator.Core.Configuration;
using Kontent.Ai.ModelGenerator.Options;

namespace Kontent.Ai.ModelGenerator.Tests.Options;

public class ValidationExtensionsTests
{
    [Fact]
    public void Validate_NoDeliveryOptions_Throws()
    {
        var options = new CodeGeneratorOptions();

        var call = options.Validate;

        call.Should().Throw<Exception>()
            .WithMessage("*EnvironmentId*");
    }

    [Fact]
    public void Validate_EmptyEnvironmentId_Throws()
    {
        var options = new CodeGeneratorOptions
        {
            DeliveryOptions = new DeliveryOptions { EnvironmentId = "" },
        };

        var call = options.Validate;

        call.Should().Throw<Exception>().WithMessage("*EnvironmentId*");
    }

    [Fact]
    public void Validate_DeliveryEnvironmentId_Ok()
    {
        var options = new CodeGeneratorOptions
        {
            DeliveryOptions = new DeliveryOptions { EnvironmentId = "abc-123" },
        };

        var call = options.Validate;

        call.Should().NotThrow();
    }

    [Fact]
    public void ValidateManagement_NoManagementOptions_Throws()
    {
        var options = new CodeGeneratorOptions();

        var call = options.ValidateManagement;

        call.Should().Throw<Exception>().WithMessage("*EnvironmentId*");
    }

    [Fact]
    public void ValidateManagement_EmptyEnvironmentId_Throws()
    {
        var options = new CodeGeneratorOptions
        {
            ManagementOptions = new ManagementOptions { ApiKey = "secret" },
        };

        var call = options.ValidateManagement;

        call.Should().Throw<Exception>().WithMessage("*EnvironmentId*");
    }

    [Fact]
    public void ValidateManagement_EmptyApiKey_Throws()
    {
        var options = new CodeGeneratorOptions
        {
            ManagementOptions = new ManagementOptions { EnvironmentId = "abc-123" },
        };

        var call = options.ValidateManagement;

        call.Should().Throw<Exception>().WithMessage("*ApiKey*");
    }

    [Fact]
    public void ValidateManagement_BothSet_Ok()
    {
        var options = new CodeGeneratorOptions
        {
            ManagementOptions = new ManagementOptions { EnvironmentId = "abc-123", ApiKey = "secret" },
        };

        var call = options.ValidateManagement;

        call.Should().NotThrow();
    }
}
