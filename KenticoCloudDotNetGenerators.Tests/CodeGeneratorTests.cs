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
            var options = new Options()
            {
                Namespace = "CustomNamespace",
                OutputDir = Path.GetTempPath() + "/TestOutput/",
                ProjectId = PROJECT_ID
            };

            new CodeGenerator(options).Run();

            Assert.AreEqual(11, Directory.GetFiles(options.OutputDir).Count());
        }
    }
}
