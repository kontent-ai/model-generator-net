using System;
using Xunit;

namespace CloudModelGenerator.Tests
{
    public class ProgramTests
    {
        [Fact]
        public void CreateCodeGeneratorOptions_NoProjectId_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => Program.Main(new string[] { }));
        }
    }
}
