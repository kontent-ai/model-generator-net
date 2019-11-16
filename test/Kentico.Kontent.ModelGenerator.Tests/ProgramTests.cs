using Xunit;

namespace Kentico.Kontent.ModelGenerator.Tests
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
