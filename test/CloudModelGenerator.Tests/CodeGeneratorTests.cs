using System.IO;
using Xunit;

namespace CloudModelGenerator.Tests
{
    public class CodeGeneratorTests
    {
        private readonly string TEMP_DIR = Path.GetTempPath() + "/CodeGeneratorTests/";

        [Fact]
        public void IntegrationTest()
        {
            const string PROJECT_ID = "e1167a11-75af-4a08-ad84-0582b463b010";
            const string @namespace = "CustomNamespace";

            var codeGenerator = new CodeGenerator(PROJECT_ID, TEMP_DIR, @namespace);
            codeGenerator.GenerateContentTypeModels();
            codeGenerator.GenerateTypeProvider();
            
            Assert.Equal(8, Directory.GetFiles(Path.GetFullPath(TEMP_DIR)).Length);

            // Cleanup
            Directory.Delete(TEMP_DIR, recursive: true);
        }
    }
}
