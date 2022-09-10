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
            ExtendedDeliverModels = false
        };

        var result = options.ManagementApi();

        Assert.True(result);
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

        Assert.False(result);
    }

    [Fact]
    public void ManagementApi_InvalidConfig_ReturnsFalse()
    {
        var options = new CodeGeneratorOptions
        {
            ManagementApi = true,
            ExtendedDeliverModels = true
        };

        var result = options.ManagementApi();

        Assert.False(result);
    }
}
