using System;
using System.CommandLine;
using Xunit;

namespace CloudModelGenerator.Tests
{
    public class ProgramTests
    {
        [Fact]
        public void CreateCodeGeneratorOptions_NoProjectId_Throws()
        {
            ArgumentSyntax preparedSyntax = Program.Parse(new string[] { });

            Assert.Throws<InvalidOperationException>(() => Program.CreateCodeGeneratorOptions(preparedSyntax));
        }

        [Fact]
        public void CreateCodeGeneratorOptions_NoOutputSetInJsonNorInParameters_OuputDirHasDefaultValue()
        {
            ArgumentSyntax preparedSyntax = Program.Parse(new string[]
            {
                "--projectid", "00000000-0000-0000-0000-000000000001"
            });

            var options = Program.CreateCodeGeneratorOptions(preparedSyntax);
            Assert.Equal("./", options.OutputDir);
        }
    }
}
