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

        // Pre-fetch all content types into memory (instead of streaming) so [AllowedTypes] can
        // resolve id-only references in `allowed_content_types` to portable codenames. MAPI
        // returns those references without codenames on type metadata responses; without this
        // lookup the constraint would silently disappear from the generated models.
        var types = await FetchAllTypes();
        var resolveTypeCodename = BuildTypeCodenameResolver(types);

        var generators = new List<ClassCodeGenerator>();
        foreach (var contentType in types)
        {
            try
            {
                generators.Add(BuildClassCodeGenerator(contentType, resolveSnippet, resolveTypeCodename));
            }
            catch (InvalidIdentifierException)
            {
                WriteConsoleErrorMessage(contentType.Codename);
            }
        }

        return generators;
    }

    internal ClassCodeGenerator BuildClassCodeGenerator(
        ContentTypeModel contentType,
        Func<Reference, ContentTypeSnippetModel> resolveSnippet,
        Func<Guid, string> resolveTypeCodename = null)
    {
        var classDefinition = ClassDefinitionFactory.CreateClassDefinition(contentType.Codename);
        if (contentType.Id != Guid.Empty)
        {
            classDefinition.Id = contentType.Id.ToString();
        }

        foreach (var expanded in SnippetExpander.Expand(contentType.Elements, resolveSnippet, Logger.LogWarning))
        {
            try
            {
                AddElement(classDefinition, contentType.Codename, expanded.Element, expanded.CodenameOverride, resolveTypeCodename);
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

    private async Task<IReadOnlyList<ContentTypeModel>> FetchAllTypes()
    {
        var all = new List<ContentTypeModel>();
        var page = await _managementClient.ListContentTypesAsync();
        while (page != null)
        {
            all.AddRange(page);
            page = page.HasNextPage() ? await page.GetNextPage() : null;
        }
        return all;
    }

    /// <summary>
    /// Builds an id → codename lookup for content types. Used to hydrate the id-only
    /// references that MAPI returns in <c>allowed_content_types</c> on type metadata responses.
    /// Codenames are environment-portable; ids are not — so we always emit codenames.
    /// </summary>
    private static Func<Guid, string> BuildTypeCodenameResolver(IReadOnlyList<ContentTypeModel> types)
    {
        var byId = new Dictionary<Guid, string>();
        foreach (var t in types)
        {
            if (t.Id != Guid.Empty && !string.IsNullOrEmpty(t.Codename))
            {
                byId[t.Id] = t.Codename;
            }
        }
        return id => byId.TryGetValue(id, out var codename) ? codename : null;
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
        string codenameOverride,
        Func<Guid, string> resolveTypeCodename = null)
    {
        // Guidelines are editor-only — no wire value, nothing to emit. (The expander also drops
        // guidelines inside snippets; this guard handles type-level ones.)
        if (element is GuidelinesElementMetadataModel)
        {
            return;
        }

        var input = ManagementElementMetadataAdapter.ToInput(
            element,
            classDefinition.ClassName,
            codenameOverride,
            resolveTypeCodename,
            Logger.LogWarning);
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
