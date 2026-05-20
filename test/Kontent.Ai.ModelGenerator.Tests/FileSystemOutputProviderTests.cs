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

    [Fact]
    public void Output_ValidFileName_WritesFileInsideOutputDir()
    {
        var outputDir = CreateTempDir();
        try
        {
            var outputProvider = CreateOutputProvider(outputDir);

            outputProvider.Output("public class MyClass {}", "MyClass", overwriteExisting: true);

            var expectedFile = Path.Combine(outputDir, "MyClass.cs");
            File.Exists(expectedFile).Should().BeTrue();
            File.ReadAllText(expectedFile).Should().Be("public class MyClass {}");
        }
        finally
        {
            Directory.Delete(outputDir, recursive: true);
        }
    }

    [Fact]
    public void Output_FileNameContainsTraversal_ThrowsArgumentException()
    {
        var outputDir = CreateTempDir();
        try
        {
            var outputProvider = CreateOutputProvider(outputDir);

            var act = () => outputProvider.Output("malicious", "../EscapedClass", overwriteExisting: true);

            act.Should().Throw<ArgumentException>().WithParameterName("fileName");
            File.Exists(Path.Combine(outputDir, "..", "EscapedClass.cs")).Should().BeFalse();
        }
        finally
        {
            Directory.Delete(outputDir, recursive: true);
        }
    }

    [Fact]
    public void Output_FileNameIsRootedPath_ThrowsArgumentException()
    {
        var outputDir = CreateTempDir();
        try
        {
            var outputProvider = CreateOutputProvider(outputDir);

            // A rooted fileName makes Path.Combine discard OutputDir entirely.
            var rootedFileName = Path.Combine(Path.GetTempPath(), "EscapedClass");

            var act = () => outputProvider.Output("malicious", rootedFileName, overwriteExisting: true);

            act.Should().Throw<ArgumentException>().WithParameterName("fileName");
            File.Exists(rootedFileName + ".cs").Should().BeFalse();
        }
        finally
        {
            Directory.Delete(outputDir, recursive: true);
        }
    }

    private static FileSystemOutputProvider CreateOutputProvider(string outputDir)
    {
        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.Setup(x => x.Value).Returns(new CodeGeneratorOptions { OutputDir = outputDir });

        return new FileSystemOutputProvider(mockOptions.Object);
    }

    private static string CreateTempDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"fsop-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(dir);
        return dir;
    }
}
