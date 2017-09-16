using System.IO;
using Xunit;

namespace CloudModelGenerator.Tests
{
    public class CodeGeneratorTests
    {
        private readonly string TEMP_DIR = Path.GetTempPath() + "/CodeGeneratorTests/";

        public CodeGeneratorTests()
        {
            // Cleanup
            if (Directory.Exists(TEMP_DIR))
            {
                Directory.Delete(TEMP_DIR, true);
            }
            Directory.CreateDirectory(TEMP_DIR);
        }

        [Fact]
        public void IntegrationTest()
        {
            const string PROJECT_ID = "975bf280-fd91-488c-994c-2f04416e5ee3";
            const string @namespace = "CustomNamespace";

            var codeGenerator = new CodeGenerator(PROJECT_ID, TEMP_DIR, @namespace);
            codeGenerator.GenerateContentTypeModels();
            codeGenerator.GenerateTypeProvider();

            Assert.True(Directory.GetFiles(Path.GetFullPath(TEMP_DIR)).Length > 10);
            
            foreach (var filepath in Directory.EnumerateFiles(Path.GetFullPath(TEMP_DIR)))
            {
                Assert.DoesNotContain(".Generated.cs", Path.GetFileName(filepath));
            }

            // Cleanup
            Directory.Delete(TEMP_DIR, true);
        }

        [Fact]
        public void IntegrationTestWithGeneratedSuffix()
        {
            const string PROJECT_ID = "975bf280-fd91-488c-994c-2f04416e5ee3";
            const string @namespace = "CustomNamespace";
            const string transformFilename = "Generated";


            var codeGenerator = new CodeGenerator(PROJECT_ID, TEMP_DIR, @namespace, transformFilename);
            codeGenerator.GenerateContentTypeModels();
            codeGenerator.GenerateTypeProvider();

            Assert.True(Directory.GetFiles(Path.GetFullPath(TEMP_DIR)).Length > 10);

            foreach (var filepath in Directory.EnumerateFiles(Path.GetFullPath(TEMP_DIR)))
            {
                Assert.EndsWith($".{transformFilename}.cs", Path.GetFileName(filepath));
            }

            // Cleanup
            Directory.Delete(TEMP_DIR, true);
        }
    }
}
