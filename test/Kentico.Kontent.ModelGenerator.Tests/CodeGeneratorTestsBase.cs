using System.IO;

namespace Kontent.Ai.ModelGenerator.Tests
{
    public abstract class CodeGeneratorTestsBase
    {
        protected abstract string TempDir { get; }
        protected const string ProjectId = "975bf280-fd91-488c-994c-2f04416e5ee3";

        protected CodeGeneratorTestsBase()
        {
            // Cleanup
            if (Directory.Exists(TempDir))
            {
                Directory.Delete(TempDir, true);
            }
            Directory.CreateDirectory(TempDir);
        }
    }
}
