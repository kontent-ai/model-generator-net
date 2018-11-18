using System;
using Xunit;

namespace CloudModelGenerator.Tests
{
    public class ProgramTests
    {
        [Fact]
        public void CreateCodeGeneratorOptions_NoProjectId_ReturnsError()
        {
            Assert.Equal(1, Program.Main(new string[] { }));
        }
    }
}
