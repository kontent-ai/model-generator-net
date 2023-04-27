using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Configuration;
using Kontent.Ai.ModelGenerator.Core.Generators.Class;
using Microsoft.Extensions.Options;

namespace Kontent.Ai.ModelGenerator.Core;

public abstract class CodeGeneratorBase
{
    protected readonly IClassCodeGeneratorFactory ClassCodeGeneratorFactory;
    protected readonly CodeGeneratorOptions Options;
    protected readonly IOutputProvider OutputProvider;

    protected string FilenameSuffix => string.IsNullOrEmpty(Options.FileNameSuffix) ? "" : $".{Options.FileNameSuffix}";
    protected string NoContentTypeAvailableMessage =>
        $@"No content type available for the project ({Options.GetProjectId()}). Please make sure you have the Delivery API enabled at https://app.kontent.ai/.";

    protected CodeGeneratorBase(IOptions<CodeGeneratorOptions> options, IOutputProvider outputProvider, IClassCodeGeneratorFactory classCodeGeneratorFactory)
    {
        ClassCodeGeneratorFactory = classCodeGeneratorFactory;
        Options = options.Value;
        OutputProvider = outputProvider;
    }

    public async Task<int> RunAsync()
    {
        await GenerateContentTypeModels();

        if (!string.IsNullOrEmpty(Options.BaseClass))
        {
            await GenerateBaseClass();
        }

        return 0;
    }

    protected string GetFileClassName(string className) => $"{className}{FilenameSuffix}";

    protected void WriteToOutputProvider(string content, string fileName, bool overwriteExisting)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return;
        }

        OutputProvider.Output(content, fileName, overwriteExisting);
        Console.WriteLine($"{fileName} class was successfully created.");
    }

    protected void WriteToOutputProvider(ICollection<ClassCodeGenerator> classCodeGenerators)
    {
        foreach (var codeGenerator in classCodeGenerators)
        {
            OutputProvider.Output(codeGenerator.GenerateCode(), codeGenerator.ClassFilename,
                codeGenerator.OverwriteExisting);
        }

        Console.WriteLine($"{classCodeGenerators.Count} content type models were successfully created.");
    }

    protected ClassCodeGenerator GetCustomClassCodeGenerator(string contentTypeCodename)
    {
        var classDefinition = new ClassDefinition(contentTypeCodename);
        var classFilename = $"{classDefinition.ClassName}";

        return ClassCodeGeneratorFactory.CreateClassCodeGenerator(Options, classDefinition, classFilename, true);
    }

    protected static void AddProperty(Property property, ref ClassDefinition classDefinition)
    {
        classDefinition.AddPropertyCodenameConstant(property.Codename);
        classDefinition.AddProperty(property);
    }

    protected abstract Task<ICollection<ClassCodeGenerator>> GetClassCodeGenerators();

    protected static void WriteConsoleErrorMessage(Exception exception, string elementCodename, string elementType, string className)
    {
        switch (exception)
        {
            case InvalidOperationException:
                Console.WriteLine($"Warning: Element '{elementCodename}' is already present in Content Type '{className}'.");
                break;
            case InvalidIdentifierException:
                Console.WriteLine($"Warning: Can't create valid C# Identifier from '{elementCodename}'. Skipping element.");
                break;
            case ArgumentNullException or ArgumentException:
                Console.WriteLine($"Warning: Skipping unknown Content Element type '{elementType}'. (Content Type: '{className}', Element Codename: '{elementCodename}').");
                break;
        }
    }

    protected static void WriteConsoleErrorMessage(string contentTypeCodename)
    {
        Console.WriteLine($"Warning: Skipping Content Type '{contentTypeCodename}'. Can't create valid C# identifier from its name.");
    }

    private async Task GenerateContentTypeModels()
    {
        var classCodeGenerators = await GetClassCodeGenerators();

        if (!classCodeGenerators.Any())
        {
            Console.WriteLine(NoContentTypeAvailableMessage);
            return;
        }

        WriteToOutputProvider(classCodeGenerators);
    }

    private async Task GenerateBaseClass()
    {
        var classCodeGenerators = await GetClassCodeGenerators();

        if (!classCodeGenerators.Any())
        {
            Console.WriteLine(NoContentTypeAvailableMessage);
            return;
        }

        var baseClassCodeGenerator = new BaseClassCodeGenerator(Options);

        foreach (var codeGenerator in classCodeGenerators)
        {
            baseClassCodeGenerator.AddClassNameToExtend(codeGenerator.ClassDefinition.ClassName);
        }

        var baseClassCode = baseClassCodeGenerator.GenerateBaseClassCode();
        WriteToOutputProvider(baseClassCode, Options.BaseClass, false);

        var baseClassExtenderCode = baseClassCodeGenerator.GenerateExtenderCode();
        WriteToOutputProvider(baseClassExtenderCode, baseClassCodeGenerator.ExtenderClassName, true);
    }
}
