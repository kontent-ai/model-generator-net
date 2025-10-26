using Kontent.Ai.ModelGenerator.Core.Common;
using Moq;
using Kontent.Ai.ModelGenerator.Core.Contract;

namespace Kontent.Ai.ModelGenerator.Core.Tests;

/// <summary>
/// Base class for code generator tests.
/// Modern beta version only supports Delivery SDK models.
/// </summary>
public abstract class CodeGeneratorTestsBase
{
    protected abstract string TempDir { get; }
    protected const string EnvironmentId = "975bf280-fd91-488c-994c-2f04416e5ee3";
    protected readonly IClassCodeGeneratorFactory ClassCodeGeneratorFactory;
    protected readonly IClassDefinitionFactory ClassDefinitionFactory;
    protected readonly Mock<IUserMessageLogger> Logger;

    protected CodeGeneratorTestsBase()
    {
        Logger = new Mock<IUserMessageLogger>();
        ClassCodeGeneratorFactory = new ClassCodeGeneratorFactory();
        ClassDefinitionFactory = new ClassDefinitionFactory();
        // Cleanup
        if (Directory.Exists(TempDir))
        {
            Directory.Delete(TempDir, true);
        }
        Directory.CreateDirectory(TempDir);
    }
}
