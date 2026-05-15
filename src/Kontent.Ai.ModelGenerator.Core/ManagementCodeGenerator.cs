using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kontent.Ai.Management;
using Kontent.Ai.Management.Models.Types;
using Kontent.Ai.Management.Models.Types.Elements;
using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Configuration;
using Kontent.Ai.ModelGenerator.Core.Contract;
using Kontent.Ai.ModelGenerator.Core.Generators.Class;
using Kontent.Ai.ModelGenerator.Core.Services;
using Microsoft.Extensions.Options;

namespace Kontent.Ai.ModelGenerator.Core;

/// <summary>
/// Orchestrates Management-mode generation: lists content types via <see cref="IManagementClient"/>,
/// adapts each element's MAPI metadata into <see cref="ManagementElementInput"/>, runs it through
/// <see cref="IManagementElementService"/>, and assembles a <see cref="ManagementClassCodeGenerator"/>
/// per content type. Element types not yet handled by this generator slice are skipped with a warning.
/// </summary>
public class ManagementCodeGenerator : CodeGeneratorBase
{
    private readonly IManagementClient _managementClient;
    private readonly IManagementElementService _elementService;

    public ManagementCodeGenerator(
        IOptions<CodeGeneratorOptions> options,
        IOutputProvider outputProvider,
        IManagementClient managementClient,
        IClassCodeGeneratorFactory classCodeGeneratorFactory,
        IClassDefinitionFactory classDefinitionFactory,
        IManagementElementService elementService,
        IUserMessageLogger logger)
        : base(options, outputProvider, classCodeGeneratorFactory, classDefinitionFactory, logger)
    {
        _managementClient = managementClient ?? throw new ArgumentNullException(nameof(managementClient));
        _elementService = elementService ?? throw new ArgumentNullException(nameof(elementService));
    }

    protected override async Task<ICollection<ClassCodeGenerator>> GetClassCodeGenerators()
    {
        var generators = new List<ClassCodeGenerator>();

        var page = await _managementClient.ListContentTypesAsync();
        while (page != null)
        {
            foreach (var contentType in page)
            {
                try
                {
                    generators.Add(BuildClassCodeGenerator(contentType));
                }
                catch (InvalidIdentifierException)
                {
                    WriteConsoleErrorMessage(contentType.Codename);
                }
            }

            page = page.HasNextPage() ? await page.GetNextPage() : null;
        }

        return generators;
    }

    internal ClassCodeGenerator BuildClassCodeGenerator(ContentTypeModel contentType)
    {
        var classDefinition = ClassDefinitionFactory.CreateClassDefinition(contentType.Codename);

        foreach (var element in contentType.Elements ?? [])
        {
            try
            {
                AddElement(classDefinition, contentType.Codename, element);
            }
            catch (Exception ex)
            {
                WriteConsoleErrorMessage(ex, element.Codename, element.Type.ToString(), classDefinition.ClassName);
            }
        }

        var classFilename = GetFileClassName(classDefinition.ClassName);
        return new ManagementClassCodeGenerator(classDefinition, classFilename, Options.Namespace);
    }

    private void AddElement(ClassDefinition classDefinition, string contentTypeCodename, ElementMetadataBase element)
    {
        // Guidelines are editor-only — no wire value, nothing to emit.
        if (element is GuidelinesElementMetadataModel)
        {
            return;
        }

        // Snippets / rich text / asset / linked-items / taxonomy / subpages are handled in
        // later slices. The adapter returns null for those today.
        var input = ManagementElementMetadataAdapter.ToInput(element, classDefinition.ClassName);
        if (input is null)
        {
            Logger.LogWarning(
                $"Element '{element.Codename}' on content type '{contentTypeCodename}' has type " +
                $"'{element.Type}', which is not yet supported by the Management generator. Skipping.");
            return;
        }

        var output = _elementService.Build(input);
        classDefinition.AddPropertyCodenameConstant(output.Property.Codename);
        classDefinition.AddProperty(output.Property);

        foreach (var enumDef in output.Enums)
        {
            classDefinition.AddEnum(enumDef);
        }
    }
}
