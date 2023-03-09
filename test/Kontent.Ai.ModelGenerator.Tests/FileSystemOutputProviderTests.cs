using Kontent.Ai.ModelGenerator.Core.Configuration;
using Microsoft.Extensions.Options;
using Moq;

namespace Kontent.Ai.ModelGenerator.Tests;

public class FileSystemOutputProviderTests
{
    [Fact]
    public void CreateCodeGeneratorOptions_NoOutputSetInJsonNorInParameters_OutputDirHasDefaultValue()
    {
        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        var options = new CodeGeneratorOptions
        {
            OutputDir = ""
        };
        mockOptions.Setup(x => x.Value).Returns(options);

        var outputProvider = new FileSystemOutputProvider(mockOptions.Object);

        options.OutputDir.Should().BeEmpty();
        outputProvider.OutputDir.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void CreateCodeGeneratorOptions_OutputSetInParameters_OutputDirHasCustomValue()
    {
        var expectedOutputDir = Environment.CurrentDirectory.TrimEnd(Path.DirectorySeparatorChar);
        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        var options = new CodeGeneratorOptions
        {
            OutputDir = ""
        };
        mockOptions.Setup(x => x.Value).Returns(options);

        var outputProvider = new FileSystemOutputProvider(mockOptions.Object);

        var result = outputProvider.OutputDir.TrimEnd(Path.DirectorySeparatorChar);

        result.Should().Be(expectedOutputDir);
    }
}
