using System;
using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.Management.Configuration;
using Kontent.Ai.ModelGenerator.Core.Configuration;
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

    [Fact]
    public void Validate_ManagementOptionsIsNull_ThrowsException()
    {
        var projectId = Guid.NewGuid().ToString();
        var codeGeneratorOptions = new CodeGeneratorOptions
        {
            ManagementApi = true,
            DeliveryOptions = new DeliveryOptions
            {
                ProjectId = projectId
            },
            ManagementOptions = null
        };

        var exception = Assert.Throws<Exception>(() => codeGeneratorOptions.Validate());
        Assert.Equal("You have to provide the 'ProjectId' to generate type for Management SDK. See https://bit.ly/3rSMeDA for more details on configuration.", exception.Message);
    }

    [Fact]
    public void Validate_ManagementOptionsProjectIdIsNull_ThrowsException()
    {
        var projectId = Guid.NewGuid().ToString();
        var codeGeneratorOptions = new CodeGeneratorOptions
        {
            ManagementApi = true,
            DeliveryOptions = new DeliveryOptions
            {
                ProjectId = projectId
            },
            ManagementOptions = new ManagementOptions
            {
                ProjectId = null,
                ApiKey = "apiKey"
            }
        };

        var exception = Assert.Throws<Exception>(() => codeGeneratorOptions.Validate());
        Assert.Equal("You have to provide the 'ProjectId' to generate type for Management SDK. See https://bit.ly/3rSMeDA for more details on configuration.", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_ApiKeyIsNullOrWhiteSpace_ThrowsException(string apiKey)
    {
        var projectId = Guid.NewGuid().ToString();
        var codeGeneratorOptions = new CodeGeneratorOptions
        {
            ManagementApi = true,
            DeliveryOptions = new DeliveryOptions
            {
                ProjectId = projectId
            },
            ManagementOptions = new ManagementOptions
            {
                ProjectId = projectId,
                ApiKey = apiKey
            }
        };

        var exception = Assert.Throws<Exception>(() => codeGeneratorOptions.Validate());
        Assert.Equal("You have to provide the 'ApiKey' to generate type for Management SDK. See https://bit.ly/3rSMeDA for more details on configuration.", exception.Message);
    }
}
