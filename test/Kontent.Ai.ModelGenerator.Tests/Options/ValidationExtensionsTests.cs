using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.Management.Configuration;
using Kontent.Ai.ModelGenerator.Core.Configuration;
using Kontent.Ai.ModelGenerator.Options;

namespace Kontent.Ai.ModelGenerator.Tests.Options;

public class ValidationExtensionsTests
{
    [Fact]
    public void Validate_ManagementOptions_DoesNotThrow()
    {
        var environmentId = Guid.NewGuid().ToString();
        var codeGeneratorOptions = new CodeGeneratorOptions
        {
            ManagementApi = true,
            ManagementOptions = new ManagementOptions
            {
                EnvironmentId = environmentId,
                ApiKey = "apiKey"
            }
        };

        var validateCall = () => codeGeneratorOptions.Validate();

        validateCall.Should().NotThrow();
    }

    [Theory]
    [InlineData(StructuredModelFlags.NotSet)]
    [InlineData(StructuredModelFlags.DateTime)]
    [InlineData(StructuredModelFlags.RichText)]
    [InlineData(StructuredModelFlags.True)]
    [InlineData(StructuredModelFlags.RichText | StructuredModelFlags.DateTime | StructuredModelFlags.True)]
    public void Validate_DeliveryOptions_DoesNotThrow(StructuredModelFlags structuredModel)
    {
        var environmentId = Guid.NewGuid().ToString();
        var codeGeneratorOptions = new CodeGeneratorOptions
        {
            ManagementApi = false,
            DeliveryOptions = new DeliveryOptions
            {
                EnvironmentId = environmentId
            },
            StructuredModel = structuredModel.ToString()
        };

        var validateCall = () => codeGeneratorOptions.Validate();

        validateCall.Should().NotThrow();
    }

    [Fact]
    public void Validate_ExtendedDeliveryModels_DoesNotThrow()
    {
        var environmentId = Guid.NewGuid().ToString();
        var codeGeneratorOptions = new CodeGeneratorOptions
        {
            ManagementApi = false,
            ExtendedDeliveryModels = true,
            ManagementOptions = new ManagementOptions
            {
                EnvironmentId = environmentId,
                ApiKey = "apiKey"
            }
        };

        var validateCall = () => codeGeneratorOptions.Validate();

        validateCall.Should().NotThrow();
    }

    [Fact]
    public void Validate_DeliveryOptionsIsNull_ThrowsException()
    {
        var codeGeneratorOptions = new CodeGeneratorOptions
        {
            DeliveryOptions = null
        };

        var validateCall = () => codeGeneratorOptions.Validate();

        validateCall.Should().ThrowExactly<Exception>()
            .And.Message.Should().Be("You have to provide at least the 'EnvironmentId' argument. See http://bit.ly/k-params for more details on configuration.");
    }

    [Fact]
    public void Validate_DeliveryOptionsEnvironmentIdIsNull_ThrowsException()
    {
        var codeGeneratorOptions = new CodeGeneratorOptions
        {
            DeliveryOptions = new DeliveryOptions
            {
                EnvironmentId = null
            }
        };

        var validateCall = () => codeGeneratorOptions.Validate();

        validateCall.Should().ThrowExactly<ArgumentNullException>();
    }

    [Theory]
    [InlineData(StructuredModelFlags.ValidationIssue)]
    [InlineData(StructuredModelFlags.ValidationIssue | StructuredModelFlags.DateTime)]
    [InlineData(StructuredModelFlags.DateTime | StructuredModelFlags.ValidationIssue | StructuredModelFlags.RichText)]
    public void Validate_DeliveryOptionsStructuredModelFlagsContainValidationIssue_ThrowsException(StructuredModelFlags structuredModel)
    {
        var environmentId = Guid.NewGuid().ToString();
        var codeGeneratorOptions = new CodeGeneratorOptions
        {
            ManagementApi = false,
            DeliveryOptions = new DeliveryOptions
            {
                EnvironmentId = environmentId
            },
            StructuredModel = structuredModel.ToString()
        };

        var validateCall = () => codeGeneratorOptions.Validate();

        validateCall.Should().ThrowExactly<Exception>();
    }

    [Theory]
    [MemberData(nameof(OptionsUsingManagementApiOptionsData))]
    public void Validate_ManagementOptionsIsNull_ThrowsException(CodeGeneratorOptions codeGeneratorOptions, string expectedSdkName, string expectedUrl)
    {
        var environmentId = Guid.NewGuid().ToString();
        codeGeneratorOptions.DeliveryOptions = new DeliveryOptions
        {
            EnvironmentId = environmentId
        };
        codeGeneratorOptions.ManagementOptions = null;

        var validateCall = () => codeGeneratorOptions.Validate();

        validateCall.Should().ThrowExactly<Exception>()
            .And.Message.Should().Be($"You have to provide the 'EnvironmentId' to generate type for {expectedSdkName} SDK. See {expectedUrl} for more details on configuration.");
    }

    [Theory]
    [MemberData(nameof(OptionsUsingManagementApiOptionsData))]
    public void Validate_ManagementOptionsEnvironmentIdIsNull_ThrowsException(CodeGeneratorOptions codeGeneratorOptions, string expectedSdkName, string expectedUrl)
    {
        var environmentId = Guid.NewGuid().ToString();
        codeGeneratorOptions.DeliveryOptions = new DeliveryOptions
        {
            EnvironmentId = environmentId
        };
        codeGeneratorOptions.ManagementOptions = new ManagementOptions
        {
            EnvironmentId = null,
            ApiKey = "apiKey"
        };

        var validateCall = () => codeGeneratorOptions.Validate();

        validateCall.Should().ThrowExactly<Exception>()
            .And.Message.Should().Be($"You have to provide the 'EnvironmentId' to generate type for {expectedSdkName} SDK. See {expectedUrl} for more details on configuration.");
    }

    [Theory]
    [MemberData(nameof(OptionsUsingManagementApiOptionsApiKeyIsNullOrWhiteSpaceData))]
    public void Validate_ApiKeyIsNullOrWhiteSpace_ThrowsException(
        CodeGeneratorOptions codeGeneratorOptions,
        string apiKey,
        string expectedSdkName,
        string expectedUrl)
    {
        var environmentId = Guid.NewGuid().ToString();
        codeGeneratorOptions.DeliveryOptions = new DeliveryOptions
        {
            EnvironmentId = environmentId
        };
        codeGeneratorOptions.ManagementOptions = new ManagementOptions
        {
            EnvironmentId = environmentId,
            ApiKey = apiKey
        };

        var validateCall = () => codeGeneratorOptions.Validate();

        validateCall.Should().ThrowExactly<Exception>()
            .And.Message.Should().Be($"You have to provide the 'ApiKey' to generate type for {expectedSdkName} SDK. See {expectedUrl} for more details on configuration.");
    }

    public static IEnumerable<object[]> OptionsUsingManagementApiOptionsData => new List<object[]>
    {
        new object[]
        {
            new CodeGeneratorOptions
            {
                ManagementApi = true,
                ExtendedDeliveryModels = false
            },
            "Management",
            "https://bit.ly/3rSMeDA"
        },
        new object[]
        {
            new CodeGeneratorOptions
            {
                ManagementApi = false,
                ExtendedDeliveryModels = true
            },
            "Delivery",
            "https://bit.ly/3rSMeDA"
        }
    };

    public static IEnumerable<object[]> OptionsUsingManagementApiOptionsApiKeyIsNullOrWhiteSpaceData => new List<object[]>
    {
        new object[]
        {
            new CodeGeneratorOptions
            {
                ManagementApi = true,
                ExtendedDeliveryModels = false
            },
            null,
            "Management",
            "https://bit.ly/3rSMeDA"
        },
        new object[]
        {
            new CodeGeneratorOptions
            {
                ManagementApi = false,
                ExtendedDeliveryModels = true
            },
            "",
            "Delivery",
            "https://bit.ly/3rSMeDA"
        }
    };
}
