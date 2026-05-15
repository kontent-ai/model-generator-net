using System;
using System.Collections.Generic;
using System.Linq;
using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Contract;
using Kontent.Ai.ModelGenerator.Core.Helpers;

namespace Kontent.Ai.ModelGenerator.Core.Services;

/// <summary>
/// Maps Management API element inputs to <see cref="ManagementElementOutput"/> records ready
/// for emission. Covers text, number, date_time, custom, url_slug, multiple_choice,
/// modular_content, subpages, and taxonomy in this slice; rich text, asset, and snippets are
/// added in later slices.
/// </summary>
public sealed class ManagementElementService : IManagementElementService
{
    public ManagementElementOutput Build(ManagementElementInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return input switch
        {
            TextElementInput t => new ManagementElementOutput(BuildText(t)),
            NumberElementInput n => new ManagementElementOutput(BuildSimple(n.Codename, n.Id, "decimal?")),
            DateTimeElementInput d => new ManagementElementOutput(BuildSimple(d.Codename, d.Id, "DateTimeOffset?")),
            CustomElementInput c => new ManagementElementOutput(BuildSimple(c.Codename, c.Id, "string?")),
            UrlSlugElementInput u => new ManagementElementOutput(BuildUrlSlug(u)),
            MultipleChoiceElementInput m => BuildMultipleChoice(m),
            LinkedItemsElementInput li => new ManagementElementOutput(
                BuildContentItemCollection(li.Codename, li.Id, li.AllowedTypeCodenames, li.ItemCount)),
            SubpagesElementInput sp => new ManagementElementOutput(
                BuildContentItemCollection(sp.Codename, sp.Id, sp.AllowedTypeCodenames, sp.ItemCount)),
            TaxonomyElementInput tx => new ManagementElementOutput(BuildTaxonomy(tx)),
            _ => throw new ArgumentException(
                $"Unsupported management element input type: {input.GetType().Name}",
                nameof(input)),
        };
    }

    private static ManagementProperty BuildText(TextElementInput input)
    {
        var attrs = new List<AttributeSpec> { KontentElement(input.Codename, input.Id) };

        if (input.MaximumCharacters is int max)
        {
            attrs.Add(new AttributeSpec("StringLength", [AttributeArg.Positional(max)]));
        }

        if (!string.IsNullOrWhiteSpace(input.Regex))
        {
            attrs.Add(new AttributeSpec("RegularExpression", [AttributeArg.Positional(input.Regex)]));
        }

        return new ManagementProperty(input.Codename, "string?", input.Id, attrs);
    }

    private static ManagementProperty BuildUrlSlug(UrlSlugElementInput input)
    {
        var attrs = new List<AttributeSpec> { KontentElement(input.Codename, input.Id) };

        if (!string.IsNullOrWhiteSpace(input.Regex))
        {
            attrs.Add(new AttributeSpec("RegularExpression", [AttributeArg.Positional(input.Regex)]));
        }

        return new ManagementProperty(input.Codename, "string?", input.Id, attrs);
    }

    private static ManagementProperty BuildSimple(string codename, string id, string typeName) =>
        new(codename, typeName, id, [KontentElement(codename, id)]);

    private static ManagementElementOutput BuildMultipleChoice(MultipleChoiceElementInput input)
    {
        if (string.IsNullOrWhiteSpace(input.EnumTypeName))
        {
            throw new ArgumentException(
                $"Multiple-choice element '{input.Codename}' has no EnumTypeName — the orchestrator must set one.",
                nameof(input));
        }

        var attrs = new List<AttributeSpec> { KontentElement(input.Codename, input.Id) };

        // Single-select still serializes as a length-1 array on the wire; we constrain the
        // collection size rather than changing the property's C# type.
        if (input.IsSingleSelect)
        {
            attrs.Add(new AttributeSpec("MaxElements", [AttributeArg.Positional(1)]));
        }

        var property = new ManagementProperty(
            input.Codename,
            $"IReadOnlyList<{input.EnumTypeName}>?",
            input.Id,
            attrs);

        var members = input.Options.Select(opt => new EnumMember(
            identifier: TextHelpers.GetValidPascalCaseIdentifierName(opt.Codename),
            attributes:
            [
                new AttributeSpec("KontentEnumValue",
                [
                    AttributeArg.Named("Codename", opt.Codename),
                    AttributeArg.Named("Id", opt.Id),
                ])
            ])).ToList();

        var enumDef = new EnumDefinition(input.EnumTypeName, members);

        return new ManagementElementOutput(property, [enumDef]);
    }

    private static ManagementProperty BuildContentItemCollection(
        string codename,
        string id,
        IReadOnlyList<string> allowedTypeCodenames,
        CountLimit count)
    {
        var attrs = new List<AttributeSpec> { KontentElement(codename, id) };

        if (allowedTypeCodenames is { Count: > 0 })
        {
            attrs.Add(new AttributeSpec(
                "AllowedTypes",
                allowedTypeCodenames.Select(c => AttributeArg.Positional(c)).ToList()));
        }

        AddCountLimitAttribute(attrs, count);

        return new ManagementProperty(codename, "IReadOnlyList<IContentItem>?", id, attrs);
    }

    private static ManagementProperty BuildTaxonomy(TaxonomyElementInput input)
    {
        var attrs = new List<AttributeSpec> { KontentElement(input.Codename, input.Id) };

        if (!string.IsNullOrWhiteSpace(input.TaxonomyGroup))
        {
            attrs.Add(new AttributeSpec(
                "AllowedTaxonomyGroup",
                [AttributeArg.Positional(input.TaxonomyGroup)]));
        }

        AddCountLimitAttribute(attrs, input.TermCount);

        return new ManagementProperty(input.Codename, "IReadOnlyList<Reference>?", input.Id, attrs);
    }

    private static void AddCountLimitAttribute(List<AttributeSpec> attrs, CountLimit count)
    {
        if (count is null)
        {
            return;
        }

        var attrName = count.Mode switch
        {
            CountLimitMode.AtLeast => "MinElements",
            CountLimitMode.AtMost => "MaxElements",
            CountLimitMode.Exactly => "ExactElements",
            _ => throw new ArgumentOutOfRangeException(nameof(count), count.Mode, "Unknown count limit mode."),
        };

        attrs.Add(new AttributeSpec(attrName, [AttributeArg.Positional(count.Value)]));
    }

    private static AttributeSpec KontentElement(string codename, string id) =>
        new("KontentElement",
        [
            AttributeArg.Named("Codename", codename),
            AttributeArg.Named("Id", id),
        ]);
}
