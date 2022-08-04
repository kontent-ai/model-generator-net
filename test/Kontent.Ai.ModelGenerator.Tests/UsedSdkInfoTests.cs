using System;
using Xunit;

namespace Kontent.Ai.ModelGenerator.Tests;

public class UsedSdkInfoTests
{
    [Fact]
    public void Constructor_TypeIsNull_Throws()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new UsedSdkInfo(null, "name"));
        Assert.Equal("type", exception.ParamName);
    }

    [Fact]
    public void Constructor_NameIsNull_Throws()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new UsedSdkInfo(typeof(string), null));
        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void Constructor_Returns()
    {
        var result = new UsedSdkInfo(typeof(string), "name");

        Assert.Equal("name", result.Name);
        Assert.NotNull(result.Version);
        Assert.NotEmpty(result.Version);
    }
}
