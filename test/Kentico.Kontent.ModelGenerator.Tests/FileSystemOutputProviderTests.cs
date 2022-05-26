using System;
using System.IO;
using Kentico.Kontent.ModelGenerator.Core.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Kentico.Kontent.ModelGenerator.Tests
{
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
            Assert.Empty(options.OutputDir);
            Assert.NotEmpty(outputProvider.OutputDir);
        }

        [Fact]
        public void CreateCodeGeneratorOptions_OutputSetInParameters_OutputDirHasCustomValue()
        {
            var expectedOutputDir = Environment.CurrentDirectory;
            var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
            var options = new CodeGeneratorOptions
            {
                OutputDir = ""
            };
            mockOptions.Setup(x => x.Value).Returns(options);

            var outputProvider = new FileSystemOutputProvider(mockOptions.Object);
            Assert.Equal(expectedOutputDir.TrimEnd(Path.DirectorySeparatorChar), outputProvider.OutputDir.TrimEnd(Path.DirectorySeparatorChar));
        }
    }
}
