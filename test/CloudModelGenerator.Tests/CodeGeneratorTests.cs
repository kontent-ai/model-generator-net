using System;
using System.IO;
using System.Linq;
using Xunit;

namespace CloudModelGenerator.Tests
{
    public class CodeGeneratorTests
    {
        private string TEMP_DIR = Path.GetTempPath() + "/CodeGeneratorTests/";

        [Fact]
        public void IntegrationTest()
        {
            const string PROJECT_ID = "e1167a11-75af-4a08-ad84-0582b463b010";
            const string @namespace = "CustomNamespace";

            var codeGenerator = new CodeGenerator(PROJECT_ID, TEMP_DIR, @namespace);
            codeGenerator.GenerateContentTypeModels();
            codeGenerator.GenerateTypeProvider();
            
            Assert.Equal(11, Directory.GetFiles(Path.GetFullPath(TEMP_DIR)).Count());

            // Cleanup
            Directory.Delete(TEMP_DIR, recursive: true);
        }
    }
}
