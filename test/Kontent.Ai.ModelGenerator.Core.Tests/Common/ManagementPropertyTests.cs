using Kontent.Ai.ModelGenerator.Core.Common;

namespace Kontent.Ai.ModelGenerator.Core.Tests.Common;

public class ManagementPropertyTests
{
    [Fact]
    public void Constructor_AllFieldsSet_ExposedCorrectly()
    {
        var attrs = new[]
        {
            new AttributeSpec("KontentElement",
            [
                AttributeArg.Positional("title"),
                AttributeArg.Positional("abc-123"),
            ]),
            new AttributeSpec("StringLength", [AttributeArg.Positional(100)]),
        };

        var property = new ManagementProperty(
            codename: "title",
            typeName: "string?",
            id: "abc-123",
            attributes: attrs);

        property.Codename.Should().Be("title");
        property.TypeName.Should().Be("string?");
        property.Id.Should().Be("abc-123");
        property.Identifier.Should().Be("Title");
        property.Attributes.Should().HaveCount(2);
        property.Attributes[0].Name.Should().Be("KontentElement");
        property.Attributes[1].Name.Should().Be("StringLength");
    }

    [Fact]
    public void Constructor_NullAttributes_Throws()
    {
        var call = () => new ManagementProperty("c", "string?", "id", attributes: null);

        call.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_EmptyAttributes_Allowed()
    {
        var property = new ManagementProperty("c", "string?", "id", attributes: []);

        property.Attributes.Should().BeEmpty();
    }
}
