using System.IO;
using System.Linq;
using NUnit.Framework;

namespace CloudModelGenerator.Tests
{
    [TestFixture]
    public class CodeGeneratorTests
    {
        private string TEMP_DIR = Path.GetTempPath() + "/CodeGeneratorTests/";

        [TestCase]
        public void IntegrationTest()
        {
            const string PROJECT_ID = "e1167a11-75af-4a08-ad84-0582b463b010";
            const string @namespace = "CustomNamespace";

            new CodeGenerator(PROJECT_ID, TEMP_DIR, @namespace).Generate();

            Assert.AreEqual(10, Directory.GetFiles(Path.GetFullPath(TEMP_DIR)).Count());
        }

        [TearDown]
        public void DeleteTempFolder() 
        {
            Directory.Delete(TEMP_DIR, recursive: true);
        }
    }
}
