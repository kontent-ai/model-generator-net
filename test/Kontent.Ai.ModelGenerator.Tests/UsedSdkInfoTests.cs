using Kontent.Ai.ModelGenerator.Options;

namespace Kontent.Ai.ModelGenerator.Tests;

public class UsedSdkInfoTests
{
    [Fact]
    public void Constructor_TypeIsNull_Throws()
    {
        var call = () => new UsedSdkInfo(null, "name");

        call.Should().ThrowExactly<ArgumentNullException>().And.ParamName.Should().Be("type");
    }

    [Fact]
    public void Constructor_NameIsNull_Throws()
    {
        var call = () => new UsedSdkInfo(typeof(string), null);

        call.Should().ThrowExactly<ArgumentNullException>().And.ParamName.Should().Be("name");
    }

    [Fact]
    public void Constructor_Returns()
    {
        var result = new UsedSdkInfo(typeof(string), "name");

        result.Name.Should().Be("name");
        result.Version.Should().NotBeNullOrEmpty();
    }
}
