using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Configuration;
using Kontent.Ai.ModelGenerator.Core.Contract;
using Kontent.Ai.ModelGenerator.Core.Generators.Class;
using Microsoft.Extensions.Options;

namespace Kontent.Ai.ModelGenerator.Core;

public abstract class CodeGeneratorBase
{
    protected readonly IUserMessageLogger Logger;
    protected readonly IClassCodeGeneratorFactory ClassCodeGeneratorFactory;
    protected readonly IClassDefinitionFactory ClassDefinitionFactory;
    protected readonly CodeGeneratorOptions Options;
    protected readonly IOutputProvider OutputProvider;

    protected string FilenameSuffix => string.IsNullOrEmpty(Options.FileNameSuffix) ? "" : $".{Options.FileNameSuffix}";
    private string NoContentTypeAvailableMessage =>
        $@"No content type available for the project ({Options.GetEnvironmentId()}). Please make sure you have the Delivery API enabled at https://app.kontent.ai/.";

    protected CodeGeneratorBase(
        IOptions<CodeGeneratorOptions> options,
        IOutputProvider outputProvider,
        IClassCodeGeneratorFactory classCodeGeneratorFactory,
        IClassDefinitionFactory classDefinitionFactory,
        IUserMessageLogger logger)
    {
        ClassCodeGeneratorFactory = classCodeGeneratorFactory;
        ClassDefinitionFactory = classDefinitionFactory;
        Options = options.Value;
        OutputProvider = outputProvider;
        Logger = logger;
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
        Logger.LogInfo($"{fileName} class was successfully created.");
    }

    protected void WriteToOutputProvider(ICollection<ClassCodeGenerator> classCodeGenerators)
    {
        foreach (var codeGenerator in classCodeGenerators)
        {
            OutputProvider.Output(codeGenerator.GenerateCode(), codeGenerator.ClassFilename,
                codeGenerator.OverwriteExisting);
        }

        Logger.LogInfo($"{classCodeGenerators.Count} content type models were successfully created.");
    }

    protected ClassCodeGenerator GetCustomClassCodeGenerator(string contentTypeCodename)
    {
        var classDefinition = ClassDefinitionFactory.CreateClassDefinition(contentTypeCodename);
        var classFilename = $"{classDefinition.ClassName}";

        return ClassCodeGeneratorFactory.CreateClassCodeGenerator(Options, classDefinition, classFilename, Logger, true);
    }

    protected static void AddProperty(Property property, ref ClassDefinition classDefinition)
    {
        classDefinition.AddPropertyCodenameConstant(property.Codename);
        classDefinition.AddProperty(property);
    }

    protected abstract Task<ICollection<ClassCodeGenerator>> GetClassCodeGenerators();

    protected void WriteConsoleErrorMessage(Exception exception, string elementCodename, string elementType, string className)
    {
        switch (exception)
        {
            case InvalidOperationException:
                Logger.LogWarning($"Element '{elementCodename}' is already present in Content Type '{className}'.");
                break;
            case InvalidIdentifierException:
                Logger.LogWarning($"Can't create valid C# Identifier from '{elementCodename}'. Skipping element.");
                break;
            case ArgumentNullException or ArgumentException:
                Logger.LogWarning($"Skipping unknown Content Element type '{elementType}'. (Content Type: '{className}', Element Codename: '{elementCodename}').");
                break;
        }
    }

    protected void WriteConsoleErrorMessage(string contentTypeCodename)
    {
        Logger.LogWarning($"Skipping Content Type '{contentTypeCodename}'. Can't create valid C# identifier from its name.");
    }

    private async Task GenerateContentTypeModels()
    {
        var classCodeGenerators = await GetClassCodeGenerators();

        if (!classCodeGenerators.Any())
        {
            Logger.LogInfo(NoContentTypeAvailableMessage);
            return;
        }

        WriteToOutputProvider(classCodeGenerators);
    }

    private async Task GenerateBaseClass()
    {
        var classCodeGenerators = await GetClassCodeGenerators();

        if (!classCodeGenerators.Any())
        {
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
