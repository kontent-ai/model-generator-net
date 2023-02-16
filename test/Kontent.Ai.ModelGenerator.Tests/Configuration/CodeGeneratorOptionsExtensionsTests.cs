using FluentAssertions;
using Kontent.Ai.ModelGenerator.Core.Configuration;
using Xunit;

namespace Kontent.Ai.ModelGenerator.Tests.Configuration;

public class CodeGeneratorOptionsExtensionsTests
{
    [Theory]
    [InlineData(StructuredModelFlags.DateTime)]
    [InlineData(StructuredModelFlags.RichText)]
    [InlineData(StructuredModelFlags.True)]
    [InlineData(StructuredModelFlags.RichText | StructuredModelFlags.DateTime)]
    [InlineData(StructuredModelFlags.True | StructuredModelFlags.DateTime)]
    [InlineData(StructuredModelFlags.True | StructuredModelFlags.RichText)]
    [InlineData(StructuredModelFlags.True | StructuredModelFlags.RichText | StructuredModelFlags.DateTime)]
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
    [InlineData(StructuredModelFlags.DateTime)]
    [InlineData(StructuredModelFlags.ValidationIssue)]
    [InlineData(StructuredModelFlags.NotSet)]
    public void IsStructuredModelRichText_Disabled_ReturnsTrue(StructuredModelFlags structuredModel)
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
            ExtendedDeliverModels = false
        };

        var result = options.ManagementApi();

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ManagementApi_NotManagementApi_ReturnsFalse(bool extendedDeliverModels)
    {
        var options = new CodeGeneratorOptions
        {
            ManagementApi = false,
            ExtendedDeliverModels = extendedDeliverModels
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
            ManagementApi = true,
            ExtendedDeliverModels = true
        };

        var result = options.ManagementApi();

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ExtendedDeliveryModels_ManagementApiIsTrue_ReturnsFalse(bool extendedDeliverModels)
    {
        var options = new CodeGeneratorOptions
        {
            ManagementApi = true,
            ExtendedDeliverModels = extendedDeliverModels
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
            ExtendedDeliverModels = false
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
            ExtendedDeliverModels = true
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
            ExtendedDeliverModels = false
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
            ExtendedDeliverModels = false
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
            ExtendedDeliverModels = true
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
            ExtendedDeliverModels = true
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
            ExtendedDeliverModels = false
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
            ExtendedDeliverModels = true
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
            ExtendedDeliverModels = false
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
}