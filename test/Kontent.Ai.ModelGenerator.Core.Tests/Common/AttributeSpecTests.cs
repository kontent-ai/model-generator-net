using Kontent.Ai.ModelGenerator.Core.Common;

namespace Kontent.Ai.ModelGenerator.Core.Tests.Common;

public class AttributeSpecTests
{
    [Fact]
    public void Constructor_NameOnly_NoArguments()
    {
        var spec = new AttributeSpec("Required");

        spec.Name.Should().Be("Required");
        spec.Arguments.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithArguments_PreservesOrder()
    {
        var args = new[]
        {
            AttributeArg.Positional("title"),
            AttributeArg.Named("Id", "abc-123"),
        };

        var spec = new AttributeSpec("KontentElement", args);

        spec.Arguments.Should().HaveCount(2);
        spec.Arguments[0].Name.Should().BeNull();
        spec.Arguments[0].Value.Should().Be("title");
        spec.Arguments[1].Name.Should().Be("Id");
        spec.Arguments[1].Value.Should().Be("abc-123");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_NameIsNullOrWhitespace_Throws(string name)
    {
        var call = () => new AttributeSpec(name);

        call.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AttributeArg_PositionalFactory_NoName()
    {
        var arg = AttributeArg.Positional(42);

        arg.Name.Should().BeNull();
        arg.Value.Should().Be(42);
    }

    [Fact]
    public void AttributeArg_NamedFactory_PreservesName()
    {
        var arg = AttributeArg.Named("Codename", "foo");

        arg.Name.Should().Be("Codename");
        arg.Value.Should().Be("foo");
    }
}
