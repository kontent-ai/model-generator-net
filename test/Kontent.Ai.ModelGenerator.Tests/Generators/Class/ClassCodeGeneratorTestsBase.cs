﻿using Kontent.Ai.ModelGenerator.Core.Common;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.IO;
using System.Linq;
using Xunit;

namespace Kontent.Ai.ModelGenerator.Tests.Generators.Class;

public class ClassCodeGeneratorTestsBase
{
    protected readonly ClassDefinition ClassDefinition = new ClassDefinition("Complete content type");

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

        Assert.True(result.Success, compilationErrors);
    }
}
