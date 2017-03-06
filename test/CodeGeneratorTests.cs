using Microsoft.CodeAnalysis;
using NUnit.Framework;
using System.IO;
using System.Linq;

namespace KenticoCloudDotNetGenerators.Tests
{
    [TestFixture]
    public class CodeGeneratorTests
    {
        [TestCase]
        public void IntegrationTest()
        {
            const string PROJECT_ID = "e1167a11-75af-4a08-ad84-0582b463b010";
            const string @namespace = "CustomNamespace";
            string outputDir = Path.GetTempPath() + "/TestOutput/";

            new CodeGenerator(PROJECT_ID, outputDir, @namespace).Generate();

            Assert.AreEqual(11, Directory.GetFiles(Path.GetFullPath(outputDir)).Count());
        }
    }
}
