using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Contract;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Moq;

namespace Kontent.Ai.ModelGenerator.Core.Tests.Generators.Class;

public abstract class ClassCodeGeneratorTestsBase
{
    protected readonly ClassDefinition ClassDefinition = new("Complete content type");
    protected readonly Mock<IUserMessageLogger> LoggerMock = new();

    protected void AssertCompiledCode(CSharpCompilation compilation)
    {
        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);
        var compilationErrors = "Compilation errors:\n";

        if (!result.Success)
        {
            var failures = result.Diagnostics.Where(diagnostic =>
                diagnostic.IsWarningAsError ||
                diagnostic.Severity == DiagnosticSeverity.Error);

            foreach (var diagnostic in failures)
            {
                compilationErrors += $"{diagnostic.Id}: {diagnostic.GetMessage()}\n";
            }
        }

        result.Success.Should().BeTrue(compilationErrors);
    }
}
