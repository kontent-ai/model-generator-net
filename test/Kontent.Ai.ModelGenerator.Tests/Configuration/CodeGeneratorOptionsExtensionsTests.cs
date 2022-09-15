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

        Assert.True(result);
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

        Assert.False(result);
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

        Assert.False(result);
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

        Assert.False(result);
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

        Assert.False(result);
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

        Assert.True(result);
    }

    [Fact]
    public void DeliveryApi_DefaultOptions_ReturnsTrue()
    {
        var options = new CodeGeneratorOptions();

        var result = options.DeliveryApi();

        Assert.True(result);
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

        Assert.True(result);
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

        Assert.False(result);
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

        Assert.False(result);
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

        Assert.True(result);
    }

    [Fact]
    public void GetUsedMappingsType_ManagementApi_ReturnsManagement()
    {
        var options = new CodeGeneratorOptions
        {
            ManagementApi = true,
            ExtendedDeliverModels = false,
            ExtendedDeliverPreviewModels = false
        };

        var result = options.GetUsedMappingsType();

        Assert.Equal(UsedMappingsType.Management, result);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void GetUsedMappingsType_ExtendedDeliveryModels_ReturnsExtendedDelivery(bool extendedDeliverModels, bool extendedDeliverPreviewModels)
    {
        var options = new CodeGeneratorOptions
        {
            ManagementApi = false,
            ExtendedDeliverModels = extendedDeliverModels,
            ExtendedDeliverPreviewModels = extendedDeliverPreviewModels
        };

        var result = options.GetUsedMappingsType();

        Assert.Equal(UsedMappingsType.ExtendedDelivery, result);
    }

    [Fact]
    public void GetUsedMappingsType_ManagementApiIsFalse_ExtendedDeliveryOptionsAreFalse_ReturnsDelivery()
    {
        var options = new CodeGeneratorOptions
        {
            ManagementApi = false,
            ExtendedDeliverModels = false,
            ExtendedDeliverPreviewModels = false
        };

        var result = options.GetUsedMappingsType();

        Assert.Equal(UsedMappingsType.Delivery, result);
    }

    [Fact]
    public void GetUsedMappingsType_DefaultOptions_ReturnsDelivery()
    {
        var options = new CodeGeneratorOptions();

        var result = options.GetUsedMappingsType();

        Assert.Equal(UsedMappingsType.Delivery, result);
    }
}
