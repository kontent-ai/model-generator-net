using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Contract;

namespace Kontent.Ai.ModelGenerator.Core.Tests.Common;

public class ClassDefinitionFactoryTests
{
    private readonly IClassDefinitionFactory _factory;

    public ClassDefinitionFactoryTests()
    {
        _factory = new ClassDefinitionFactory();
    }

    [Fact]
    public void Constructor_SetsClassNameIdentifier()
    {
        var definition = _factory.CreateClassDefinition("Article type");

        definition.ClassName.Should().Be("ArticleType");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Constructor_CodenameIsNullEmptyOrWhiteSpace_Throws(string codename)
    {
        var call = () => _factory.CreateClassDefinition(codename);

        call.Should().ThrowExactly<ArgumentException>();
    }
}
