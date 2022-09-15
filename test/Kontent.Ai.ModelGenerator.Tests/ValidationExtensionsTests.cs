using System;
using System.Collections.Generic;
using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.Management.Configuration;
using Kontent.Ai.ModelGenerator.Core.Configuration;
using Kontent.Ai.ModelGenerator.Options;
using Xunit;

namespace Kontent.Ai.ModelGenerator.Tests;

public class ValidationExtensionsTests
{
    [Fact]
    public void Validate_ManagementOptions_Success()
    {
        var projectId = Guid.NewGuid().ToString();
        var codeGeneratorOptions = new CodeGeneratorOptions
        {
            ManagementApi = true,
            ManagementOptions = new ManagementOptions
            {
                ProjectId = projectId,
                ApiKey = "apiKey"
            }
        };

        codeGeneratorOptions.Validate();
    }

    [Fact]
    public void Validate_DeliveryOptions_Success()
    {
        var projectId = Guid.NewGuid().ToString();
        var codeGeneratorOptions = new CodeGeneratorOptions
        {
            ManagementApi = false,
            DeliveryOptions = new DeliveryOptions
            {
                ProjectId = projectId
            }
        };

        codeGeneratorOptions.Validate();
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void Validate_ExtendedDeliveryModels_Success(bool extendedDeliverModels, bool extendedDeliverPreviewModels)
    {
        var projectId = Guid.NewGuid().ToString();
        var codeGeneratorOptions = new CodeGeneratorOptions
        {
            ManagementApi = false,
            ExtendedDeliverModels = extendedDeliverModels,
            ExtendedDeliverPreviewModels = extendedDeliverPreviewModels,
            ManagementOptions = new ManagementOptions
            {
                ProjectId = projectId,
                ApiKey = "apiKey"
            }
        };

        codeGeneratorOptions.Validate();
    }

    [Fact]
    public void Validate_DeliveryOptionsIsNull_ThrowsException()
    {
        var codeGeneratorOptions = new CodeGeneratorOptions
        {
            DeliveryOptions = null
        };

        var exception = Assert.Throws<Exception>(() => codeGeneratorOptions.Validate());
        Assert.Equal("You have to provide at least the 'ProjectId' argument. See http://bit.ly/k-params for more details on configuration.", exception.Message);
    }

    [Fact]
    public void Validate_DeliveryOptionsProjectIdIsNull_ThrowsException()
    {
        var codeGeneratorOptions = new CodeGeneratorOptions
        {
            DeliveryOptions = new DeliveryOptions
            {
                ProjectId = null
            }
        };

        Assert.Throws<ArgumentNullException>(() => codeGeneratorOptions.Validate());
    }

    [Theory]
    [MemberData(nameof(OptionsUsingManagementApiOptionsData))]
    public void Validate_ManagementOptionsIsNull_ThrowsException(CodeGeneratorOptions codeGeneratorOptions, string expectedSdkName, string expectedUrl)
    {
        var projectId = Guid.NewGuid().ToString();
        codeGeneratorOptions.DeliveryOptions = new DeliveryOptions
        {
            ProjectId = projectId
        };
        codeGeneratorOptions.ManagementOptions = null;

        var exception = Assert.Throws<Exception>(() => codeGeneratorOptions.Validate());
        Assert.Equal($"You have to provide the 'ProjectId' to generate type for {expectedSdkName} SDK. See {expectedUrl} for more details on configuration.", exception.Message);
    }

    [Theory]
    [MemberData(nameof(OptionsUsingManagementApiOptionsData))]
    public void Validate_ManagementOptionsProjectIdIsNull_ThrowsException(CodeGeneratorOptions codeGeneratorOptions, string expectedSdkName, string expectedUrl)
    {
        var projectId = Guid.NewGuid().ToString();
        codeGeneratorOptions.DeliveryOptions = new DeliveryOptions
        {
            ProjectId = projectId
        };
        codeGeneratorOptions.ManagementOptions = new ManagementOptions
        {
            ProjectId = null,
            ApiKey = "apiKey"
        };

        var exception = Assert.Throws<Exception>(() => codeGeneratorOptions.Validate());
        Assert.Equal($"You have to provide the 'ProjectId' to generate type for {expectedSdkName} SDK. See {expectedUrl} for more details on configuration.", exception.Message);
    }

    [Theory]
    [MemberData(nameof(OptionsUsingManagementApiOptionsApiKeyIsNullOrWhiteSpaceData))]
    public void Validate_ApiKeyIsNullOrWhiteSpace_ThrowsException(
        CodeGeneratorOptions codeGeneratorOptions,
        string apiKey,
        string expectedSdkName,
        string expectedUrl)
    {
        var projectId = Guid.NewGuid().ToString();
        codeGeneratorOptions.DeliveryOptions = new DeliveryOptions
        {
            ProjectId = projectId
        };
        codeGeneratorOptions.ManagementOptions = new ManagementOptions
        {
            ProjectId = projectId,
            ApiKey = apiKey
        };

        var exception = Assert.Throws<Exception>(() => codeGeneratorOptions.Validate());
        Assert.Equal($"You have to provide the 'ApiKey' to generate type for {expectedSdkName} SDK. See {expectedUrl} for more details on configuration.", exception.Message);
    }

    public static IEnumerable<object[]> OptionsUsingManagementApiOptionsData => new List<object[]>
    {
        new object[]
        {
            new CodeGeneratorOptions
            {
                ManagementApi = true,
                ExtendedDeliverModels = false,
                ExtendedDeliverPreviewModels = false
            },
            "Management",
            "https://bit.ly/3rSMeDA"
        },
        new object[]
        {
            new CodeGeneratorOptions
            {
                ManagementApi = false,
                ExtendedDeliverModels = true,
                ExtendedDeliverPreviewModels = false
            },
            "Delivery",
            "https://bit.ly/3rSMeDA"
        },
        new object[]
        {
            new CodeGeneratorOptions
            {
                ManagementApi = false,
                ExtendedDeliverModels = false,
                ExtendedDeliverPreviewModels = true
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
                ExtendedDeliverModels = false,
                ExtendedDeliverPreviewModels = false
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
                ExtendedDeliverModels = true,
                ExtendedDeliverPreviewModels = false
            },
            "",
            "Delivery",
            "https://bit.ly/3rSMeDA"
        },
        new object[]
        {
            new CodeGeneratorOptions
            {
                ManagementApi = false,
                ExtendedDeliverModels = false,
                ExtendedDeliverPreviewModels = true
            },
            "    ",
            "Delivery",
            "https://bit.ly/3rSMeDA"

        }
    };
}
