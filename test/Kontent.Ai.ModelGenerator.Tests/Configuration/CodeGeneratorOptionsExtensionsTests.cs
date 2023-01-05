using FluentAssertions;
using Kontent.Ai.ModelGenerator.Core.Configuration;
using Xunit;

namespace Kontent.Ai.ModelGenerator.Tests.Configuration;

public class CodeGeneratorOptionsExtensionsTests
{
    [Fact]
    public void ManagementApi_ManagementApi_ReturnsTrue()
    {
        var options = new CodeGeneratorOptions
        {
            ManagementApi = true,
            ExtendedDeliverModels = false,
            ExtendedDeliverPreviewModels = false
        };

        var result = options.ManagementApi();

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void ManagementApi_NotManagementApi_ReturnsFalse(bool extendedDeliverModels, bool extendedDeliverPreviewModels)
    {
        var options = new CodeGeneratorOptions
        {
            ManagementApi = false,
            ExtendedDeliverModels = extendedDeliverModels,
            ExtendedDeliverPreviewModels = extendedDeliverPreviewModels
        };

        var result = options.ManagementApi();

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void ManagementApi_InvalidConfig_ReturnsFalse(bool extendedDeliverModels, bool extendedDeliverPreviewModels)
    {
        var options = new CodeGeneratorOptions
        {
            ManagementApi = true,
            ExtendedDeliverModels = extendedDeliverModels,
            ExtendedDeliverPreviewModels = extendedDeliverPreviewModels
        };

        var result = options.ManagementApi();

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void ExtendedDeliveryModels_ManagementApiIsTrue_ReturnsFalse(bool extendedDeliverModels, bool extendedDeliverPreviewModels)
    {
        var options = new CodeGeneratorOptions
        {
            ManagementApi = true,
            ExtendedDeliverModels = extendedDeliverModels,
            ExtendedDeliverPreviewModels = extendedDeliverPreviewModels
        };

        var result = options.ExtendedDeliveryModels();

        result.Should().BeFalse();
    }

    [Fact]
    public void ExtendedDeliveryModels_ManagementApiIsFalse_ExtendedDeliveryOptionsAreFalse_ReturnsFalse()
    {
        var options = new CodeGeneratorOptions
        {
            ManagementApi = false,
            ExtendedDeliverModels = false,
            ExtendedDeliverPreviewModels = false
        };

        var result = options.ExtendedDeliveryModels();

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void ExtendedDeliveryModels_ManagementApiIsFalse_ReturnsTrue(bool extendedDeliverModels, bool extendedDeliverPreviewModels)
    {
        var options = new CodeGeneratorOptions
        {
            ManagementApi = false,
            ExtendedDeliverModels = extendedDeliverModels,
            ExtendedDeliverPreviewModels = extendedDeliverPreviewModels
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
    public void DeliveryApi_ManagementApiIsFalse_ExtendedDeliveryOptionsAreFalse_ReturnsTrue()
    {
        var options = new CodeGeneratorOptions
        {
            ManagementApi = false,
            ExtendedDeliverModels = false,
            ExtendedDeliverPreviewModels = false
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
            ExtendedDeliverModels = false,
            ExtendedDeliverPreviewModels = false
        };

        var result = options.DeliveryApi();

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void DeliveryApi_ExtendedDeliveryOptions_ReturnsFalse(bool extendedDeliverModels, bool extendedDeliverPreviewModels)
    {
        var options = new CodeGeneratorOptions
        {
            ManagementApi = false,
            ExtendedDeliverModels = extendedDeliverModels,
            ExtendedDeliverPreviewModels = extendedDeliverPreviewModels
        };

        var result = options.DeliveryApi();

        result.Should().BeFalse();
    }

    [Fact]
    public void DeliveryApi_ManagementApiIsTrue_ExtendedDeliveryOptionsAreTrue_ReturnsTrue()
    {
        var options = new CodeGeneratorOptions
        {
            ManagementApi = true,
            ExtendedDeliverModels = true,
            ExtendedDeliverPreviewModels = true
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
            ExtendedDeliverModels = false,
            ExtendedDeliverPreviewModels = false
        };

        var result = options.GetDesiredModelsType();

        result.Should().Be(DesiredModelsType.Management);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void GetDesiredModelsType_ExtendedDeliveryModels_ReturnsExtendedDelivery(bool extendedDeliverModels, bool extendedDeliverPreviewModels)
    {
        var options = new CodeGeneratorOptions
        {
            ManagementApi = false,
            ExtendedDeliverModels = extendedDeliverModels,
            ExtendedDeliverPreviewModels = extendedDeliverPreviewModels
        };

        var result = options.GetDesiredModelsType();

        result.Should().Be(DesiredModelsType.ExtendedDelivery);
    }

    [Fact]
    public void GetDesiredModelsType_ManagementApiIsFalse_ExtendedDeliveryOptionsAreFalse_ReturnsDelivery()
    {
        var options = new CodeGeneratorOptions
        {
            ManagementApi = false,
            ExtendedDeliverModels = false,
            ExtendedDeliverPreviewModels = false
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
