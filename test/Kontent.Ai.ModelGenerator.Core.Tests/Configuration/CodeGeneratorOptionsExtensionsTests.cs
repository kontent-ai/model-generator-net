using FluentAssertions;
using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.Management.Configuration;
using Kontent.Ai.ModelGenerator.Core.Configuration;

namespace Kontent.Ai.ModelGenerator.Core.Tests.Configuration;

public class CodeGeneratorOptionsExtensionsTests
{
    [Theory]
    [InlineData(StructuredModelFlags.ModularContent)]
    [InlineData(StructuredModelFlags.DateTime)]
    [InlineData(StructuredModelFlags.RichText)]
    [InlineData(StructuredModelFlags.True)]
    [InlineData(StructuredModelFlags.RichText | StructuredModelFlags.DateTime)]
    [InlineData(StructuredModelFlags.True | StructuredModelFlags.DateTime)]
    [InlineData(StructuredModelFlags.True | StructuredModelFlags.RichText)]
    [InlineData(StructuredModelFlags.True | StructuredModelFlags.ModularContent)]
    [InlineData(StructuredModelFlags.True | StructuredModelFlags.RichText | StructuredModelFlags.DateTime | StructuredModelFlags.ModularContent)]
    public void IsStructuredModelEnabled_Enabled_ReturnsTrue(StructuredModelFlags structuredModel)
    {
        var options = new CodeGeneratorOptions
        {
            StructuredModel = structuredModel.ToString()
        };

        var result = options.IsStructuredModelEnabled();

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(StructuredModelFlags.NotSet)]
    [InlineData(StructuredModelFlags.ValidationIssue)]
    public void IsStructuredModelEnabled_Disabled_ReturnsFalse(StructuredModelFlags structuredModel)
    {
        var options = new CodeGeneratorOptions
        {
            StructuredModel = structuredModel.ToString()
        };

        var result = options.IsStructuredModelEnabled();

        result.Should().BeFalse();
    }

    [Fact]
    public void IsStructuredModelEnabled_Disabled_InvalidString_ReturnsFalse()
    {
        var options = new CodeGeneratorOptions
        {
            StructuredModel = "StructuredModel"
        };

        var result = options.IsStructuredModelEnabled();

        result.Should().BeFalse();
    }


    [Fact]
    public void IsStructuredModelModularContent_Enabled_ReturnsTrue()
    {
        var options = new CodeGeneratorOptions
        {
            StructuredModel = StructuredModelFlags.ModularContent.ToString()
        };

        var result = options.IsStructuredModelModularContent();

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(StructuredModelFlags.DateTime)]
    [InlineData(StructuredModelFlags.ValidationIssue)]
    [InlineData(StructuredModelFlags.RichText)]
    [InlineData(StructuredModelFlags.NotSet)]
    public void IsStructuredModelModularContent_Disabled_ReturnsFalse(StructuredModelFlags structuredModel)
    {
        var options = new CodeGeneratorOptions
        {
            StructuredModel = structuredModel.ToString()
        };

        var result = options.IsStructuredModelModularContent();

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(StructuredModelFlags.True)]
    [InlineData(StructuredModelFlags.RichText)]
    [InlineData(StructuredModelFlags.RichText | StructuredModelFlags.True)]
    public void IsStructuredModelRichText_Enabled_ReturnsTrue(StructuredModelFlags structuredModel)
    {
        var options = new CodeGeneratorOptions
        {
            StructuredModel = structuredModel.ToString()
        };

        var result = options.IsStructuredModelRichText();

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(StructuredModelFlags.ModularContent)]
    [InlineData(StructuredModelFlags.DateTime)]
    [InlineData(StructuredModelFlags.ValidationIssue)]
    [InlineData(StructuredModelFlags.NotSet)]
    public void IsStructuredModelRichText_Disabled_ReturnsFalse(StructuredModelFlags structuredModel)
    {
        var options = new CodeGeneratorOptions
        {
            StructuredModel = structuredModel.ToString()
        };

        var result = options.IsStructuredModelRichText();

        result.Should().BeFalse();
    }

    [Fact]
    public void IsIsStructuredModelRichText_Disabled_InvalidString_ReturnsFalse()
    {
        var options = new CodeGeneratorOptions
        {
            StructuredModel = "StructuredModel"
        };

        var result = options.IsStructuredModelRichText();

        result.Should().BeFalse();
    }

    [Fact]
    public void ManagementApi_ManagementApi_ReturnsTrue()
    {
        var options = new CodeGeneratorOptions
        {
            ManagementApi = true,
            ExtendedDeliveryModels = false
        };

        var result = options.ManagementApi();

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ManagementApi_NotManagementApi_ReturnsFalse(bool extendedDeliveryModels)
    {
        var options = new CodeGeneratorOptions
        {
            ManagementApi = false,
            ExtendedDeliveryModels = extendedDeliveryModels
        };

        var result = options.ManagementApi();

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ManagementApi_InvalidConfig_ReturnsFalse(bool managementApi)
    {
        var options = new CodeGeneratorOptions
        {
            ManagementApi = managementApi,
            ExtendedDeliveryModels = true
        };

        var result = options.ManagementApi();

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ExtendedDeliveryModels_ManagementApiIsTrue_ReturnsFalse(bool extendedDeliveryModels)
    {
        var options = new CodeGeneratorOptions
        {
            ManagementApi = true,
            ExtendedDeliveryModels = extendedDeliveryModels
        };

        var result = options.ExtendedDeliveryModels();

        result.Should().BeFalse();
    }

    [Fact]
    public void ExtendedDeliveryModels_ManagementApiIsFalse_ExtendedDeliveryOptionsIsFalse_ReturnsFalse()
    {
        var options = new CodeGeneratorOptions
        {
            ManagementApi = false,
            ExtendedDeliveryModels = false
        };

        var result = options.ExtendedDeliveryModels();

        result.Should().BeFalse();
    }

    [Fact]
    public void ExtendedDeliveryModels_ManagementApiIsFalse_ReturnsTrue()
    {
        var options = new CodeGeneratorOptions
        {
            ManagementApi = false,
            ExtendedDeliveryModels = true
        };

        var result = options.ExtendedDeliveryModels();

        result.Should().BeTrue();
    }

    [Fact]
    public void DeliveryApi_DefaultOptions_ReturnsTrue()
    {
        var options = new CodeGeneratorOptions();

        var result = options.DeliveryApi();

        result.Should().BeTrue();
    }

    [Fact]
    public void DeliveryApi_ManagementApiIsFalse_ExtendedDeliveryOptionsIsFalse_ReturnsTrue()
    {
        var options = new CodeGeneratorOptions
        {
            ManagementApi = false,
            ExtendedDeliveryModels = false
        };

        var result = options.DeliveryApi();

        result.Should().BeTrue();
    }

    [Fact]
    public void DeliveryApi_ManagementApi_ReturnsFalse()
    {
        var options = new CodeGeneratorOptions
        {
            ManagementApi = true,
            ExtendedDeliveryModels = false
        };

        var result = options.DeliveryApi();

        result.Should().BeFalse();
    }

    [Fact]
    public void DeliveryApi_ExtendedDeliveryOptions_ReturnsFalse()
    {
        var options = new CodeGeneratorOptions
        {
            ManagementApi = false,
            ExtendedDeliveryModels = true
        };

        var result = options.DeliveryApi();

        result.Should().BeFalse();
    }

    [Fact]
    public void DeliveryApi_ManagementApiIsTrue_ExtendedDeliveryOptionsIsTrue_ReturnsTrue()
    {
        var options = new CodeGeneratorOptions
        {
            ManagementApi = true,
            ExtendedDeliveryModels = true
        };

        var result = options.DeliveryApi();

        result.Should().BeTrue();
    }

    [Fact]
    public void GetDesiredModelsType_ManagementApi_ReturnsManagement()
    {
        var options = new CodeGeneratorOptions
        {
            ManagementApi = true,
            ExtendedDeliveryModels = false
        };

        var result = options.GetDesiredModelsType();

        result.Should().Be(DesiredModelsType.Management);
    }

    [Fact]
    public void GetDesiredModelsType_ExtendedDeliveryModels_ReturnsExtendedDelivery()
    {
        var options = new CodeGeneratorOptions
        {
            ManagementApi = false,
            ExtendedDeliveryModels = true
        };

        var result = options.GetDesiredModelsType();

        result.Should().Be(DesiredModelsType.ExtendedDelivery);
    }

    [Fact]
    public void GetDesiredModelsType_ManagementApiIsFalse_ExtendedDeliveryOptionsIsFalse_ReturnsDelivery()
    {
        var options = new CodeGeneratorOptions
        {
            ManagementApi = false,
            ExtendedDeliveryModels = false
        };

        var result = options.GetDesiredModelsType();

        result.Should().Be(DesiredModelsType.Delivery);
    }

    [Fact]
    public void GetDesiredModelsType_DefaultOptions_ReturnsDelivery()
    {
        var options = new CodeGeneratorOptions();

        var result = options.GetDesiredModelsType();

        result.Should().Be(DesiredModelsType.Delivery);
    }

    [Fact]
    public void GetProjectId_DeliveryApi_Returns()
    {
        var projectId = Guid.NewGuid().ToString();

        var options = new CodeGeneratorOptions
        {
            ManagementApi = false,
            ExtendedDeliveryModels = false,
            DeliveryOptions = new DeliveryOptions
            {
                ProjectId = projectId
            }
        };

        var result = options.GetProjectId();

        result.Should().Be(projectId);
    }

    [Fact]
    public void GetProjectId_ManagementApi_Returns()
    {
        var projectId = Guid.NewGuid().ToString();

        var options = new CodeGeneratorOptions
        {
            ManagementApi = true,
            ExtendedDeliveryModels = false,
            ManagementOptions = new ManagementOptions
            {
                ProjectId = projectId
            }
        };

        var result = options.GetProjectId();

        result.Should().Be(projectId);
    }

    [Fact]
    public void GetProjectId_ExtendedDeliveryModels_Returns()
    {
        var projectId = Guid.NewGuid().ToString();

        var options = new CodeGeneratorOptions
        {
            ManagementApi = false,
            ExtendedDeliveryModels = true,
            ManagementOptions = new ManagementOptions
            {
                ProjectId = projectId
            }
        };

        var result = options.GetProjectId();

        result.Should().Be(projectId);
    }
}