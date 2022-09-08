using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kontent.Ai.Management;
using Kontent.Ai.Management.Extensions;
using Kontent.Ai.Management.Models.Types;
using Kontent.Ai.Management.Models.Types.Elements;
using Kontent.Ai.Management.Models.TypeSnippets;
using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Configuration;
using Kontent.Ai.ModelGenerator.Core.Generators.Class;
using Kontent.Ai.ModelGenerator.Core.Helpers;
using Microsoft.Extensions.Options;

namespace Kontent.Ai.ModelGenerator.Core;

public class ManagementCodeGenerator : CodeGeneratorBase
{
    private readonly IManagementClient _managementClient;

    public ManagementCodeGenerator(IOptions<CodeGeneratorOptions> options, IOutputProvider outputProvider, IManagementClient managementClient)
        : base(options, outputProvider)
    {
        if (!options.Value.ManagementApi)
        {
            throw new InvalidOperationException("Cannot create Management models with Delivery API options.");
        }

        _managementClient = managementClient;
    }

    protected override async Task<ICollection<ClassCodeGenerator>> GetClassCodeGenerators()
    {
        var managementTypes = await _managementClient.ListContentTypesAsync().GetAllAsync();
        var managementSnippets = await _managementClient.ListContentTypeSnippetsAsync().GetAllAsync();

        var codeGenerators = new List<ClassCodeGenerator>();
        if (managementTypes == null || !managementTypes.Any())
        {
            return codeGenerators;
        }

        foreach (var contentType in managementTypes)
        {
            try
            {
                if (Options.GeneratePartials)
                {
                    codeGenerators.Add(GetCustomClassCodeGenerator(contentType.Codename));
                }

                codeGenerators.Add(GetClassCodeGenerator(contentType, managementSnippets));
            }
            catch (InvalidIdentifierException)
            {
                WriteConsoleErrorMessage(contentType.Codename);
            }
        }

        return codeGenerators;
    }

    internal ClassCodeGenerator GetClassCodeGenerator(ContentTypeModel contentType, IEnumerable<ContentTypeSnippetModel> managementSnippets)
    {
        var classDefinition = new ClassDefinition(contentType.Codename);

        foreach (var element in contentType.Elements)
        {
            try
            {
                if (element.Type != ElementMetadataType.ContentTypeSnippet)
                {
                    AddProperty(Property.FromContentTypeElement(element), ref classDefinition);
                }
                else
                {
                    var snippetElements = ManagementElementHelper.GetManagementContentTypeSnippetElements(element, managementSnippets);
                    foreach (var snippetElement in snippetElements)
                    {
                        AddProperty(Property.FromContentTypeElement(snippetElement), ref classDefinition);
                    }
                }
            }
            catch (Exception e)
            {
                WriteConsoleErrorMessage(e, element.Codename, element.Type.ToString(), classDefinition.ClassName);
            }
        }

        var classFilename = GetFileClassName(classDefinition.ClassName);

        return ClassCodeGeneratorFactory.CreateClassCodeGenerator(Options, classDefinition, classFilename);
    }
}
