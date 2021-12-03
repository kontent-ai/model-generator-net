using System;
using System.Threading.Tasks;
using Xunit;

namespace Kentico.Kontent.ModelGenerator.Tests
{
    public class ProgramTests
    {
        [Fact]
        public async Task CreateCodeGeneratorOptions_NoProjectId_ReturnsError()
        {
            var result = await Program.Main(Array.Empty<string>());
            Assert.Equal(1, result);
        }
    }
}
