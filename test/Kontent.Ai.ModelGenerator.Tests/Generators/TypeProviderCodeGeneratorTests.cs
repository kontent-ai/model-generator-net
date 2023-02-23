using System;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Kontent.Ai.ModelGenerator.Core.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using static System.String;

namespace Kontent.Ai.ModelGenerator.Tests.Generators;

public class TypeProviderCodeGeneratorTests
{
    [Fact]
    public void GenerateCodeTests()
    {
        var codeGenerator = new TypeProviderCodeGenerator();
        codeGenerator.AddContentType("article", "Article");
        codeGenerator.AddContentType("office", "Office");

        var executingPath = AppContext.BaseDirectory;
        var expectedCode = File.ReadAllText(executingPath + "/Assets/CustomTypeProvider_CompiledCode.txt");

        var compiledCode = codeGenerator.GenerateCode();

        compiledCode.Should().Be(expectedCode);
    }

    [Fact]
    public void IntegrationTest_GeneratedCodeCompilesWithoutErrors()
    {
        var codeGenerator = new TypeProviderCodeGenerator();
        codeGenerator.AddContentType("article", "Article");
        codeGenerator.AddContentType("office", "Office");

        var compiledCode = codeGenerator.GenerateCode();

        // Dummy implementation of Article and Office class to make compilation work
        var dummyClasses = "public class Article {} public class Office {}";

        var compilation = CSharpCompilation.Create(
            assemblyName: Path.GetRandomFileName(),
            syntaxTrees: new[] {
                CSharpSyntaxTree.ParseText(compiledCode),
                CSharpSyntaxTree.ParseText(dummyClasses)
            },
            references: new[] {
                MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("netstandard")).Location),
                MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("mscorlib")).Location),
                MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Runtime")).Location),
                MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Linq")).Location),
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Delivery.Abstractions.IApiResponse).GetTypeInfo().Assembly.Location)
            },
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);
        var compilationErrors = $"Compilation errors:{Environment.NewLine}";

        if (!result.Success)
        {
            var failures = result.Diagnostics.Where(diagnostic =>
                diagnostic.IsWarningAsError ||
                diagnostic.Severity == DiagnosticSeverity.Error);

            foreach (var diagnostic in failures)
            {
                compilationErrors += Format($"{diagnostic.Id}: {diagnostic.GetMessage()}{Environment.NewLine}");
            }
        }

        result.Success.Should().BeTrue(compilationErrors);
    }
}
