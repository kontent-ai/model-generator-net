using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kontent.Ai.Management;
using Kontent.Ai.Management.Models.Shared;
using Kontent.Ai.Management.Models.TypeSnippets;
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
/// Orchestrates Management-mode generation: pre-fetches snippets, lists content types via
/// <see cref="IManagementClient"/>, expands snippet references inline (codenames prefixed with
/// <c>{snippetCodename}__</c>), adapts each element's MAPI metadata into a
/// <see cref="ManagementElementInput"/>, and assembles a <see cref="ManagementClassCodeGenerator"/>
/// per content type.
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
        var snippets = await FetchAllSnippets();
        var resolveSnippet = BuildSnippetResolver(snippets);

        var generators = new List<ClassCodeGenerator>();

        var page = await _managementClient.ListContentTypesAsync();
        while (page != null)
        {
            foreach (var contentType in page)
            {
                try
                {
                    generators.Add(BuildClassCodeGenerator(contentType, resolveSnippet));
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

    internal ClassCodeGenerator BuildClassCodeGenerator(
        ContentTypeModel contentType,
        Func<Reference, ContentTypeSnippetModel> resolveSnippet)
    {
        var classDefinition = ClassDefinitionFactory.CreateClassDefinition(contentType.Codename);

        foreach (var expanded in SnippetExpander.Expand(contentType.Elements, resolveSnippet, Logger.LogWarning))
        {
            try
            {
                AddElement(classDefinition, contentType.Codename, expanded.Element, expanded.CodenameOverride);
            }
            catch (Exception ex)
            {
                WriteConsoleErrorMessage(ex, expanded.Element.Codename, expanded.Element.Type.ToString(), classDefinition.ClassName);
            }
        }

        var classFilename = GetFileClassName(classDefinition.ClassName);
        return new ManagementClassCodeGenerator(classDefinition, classFilename, Options.Namespace);
    }

    private async Task<IReadOnlyList<ContentTypeSnippetModel>> FetchAllSnippets()
    {
        var all = new List<ContentTypeSnippetModel>();
        var page = await _managementClient.ListContentTypeSnippetsAsync();
        while (page != null)
        {
            all.AddRange(page);
            page = page.HasNextPage() ? await page.GetNextPage() : null;
        }
        return all;
    }

    /// <summary>
    /// Builds a lookup function that resolves a snippet <see cref="Reference"/> against the
    /// pre-fetched snippet list. The MAPI returns snippet refs as <c>{id}</c> in the type
    /// metadata; we accept codename and external-id matches too for resilience.
    /// </summary>
    private static Func<Reference, ContentTypeSnippetModel> BuildSnippetResolver(
        IReadOnlyList<ContentTypeSnippetModel> snippets)
    {
        var byId = new Dictionary<Guid, ContentTypeSnippetModel>();
        var byCodename = new Dictionary<string, ContentTypeSnippetModel>();
        var byExternalId = new Dictionary<string, ContentTypeSnippetModel>();

        foreach (var s in snippets)
        {
            if (s.Id != Guid.Empty)
            {
                byId[s.Id] = s;
            }
            if (!string.IsNullOrEmpty(s.Codename))
            {
                byCodename[s.Codename] = s;
            }
            if (!string.IsNullOrEmpty(s.ExternalId))
            {
                byExternalId[s.ExternalId] = s;
            }
        }

        return reference =>
        {
            if (reference.Id is Guid id && byId.TryGetValue(id, out var byIdMatch))
            {
                return byIdMatch;
            }
            if (!string.IsNullOrEmpty(reference.Codename) && byCodename.TryGetValue(reference.Codename, out var byCnMatch))
            {
                return byCnMatch;
            }
            if (!string.IsNullOrEmpty(reference.ExternalId) && byExternalId.TryGetValue(reference.ExternalId, out var byExtMatch))
            {
                return byExtMatch;
            }
            return null;
        };
    }

    private void AddElement(
        ClassDefinition classDefinition,
        string contentTypeCodename,
        ElementMetadataBase element,
        string codenameOverride)
    {
        // Guidelines are editor-only — no wire value, nothing to emit. (The expander also drops
        // guidelines inside snippets; this guard handles type-level ones.)
        if (element is GuidelinesElementMetadataModel)
        {
            return;
        }

        var input = ManagementElementMetadataAdapter.ToInput(element, classDefinition.ClassName, codenameOverride);
        if (input is null)
        {
            Logger.LogWarning(
                $"Element '{codenameOverride ?? element.Codename}' on content type '{contentTypeCodename}' has type " +
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
