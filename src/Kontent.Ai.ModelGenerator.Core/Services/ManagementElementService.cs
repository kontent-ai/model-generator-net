using System;
using System.Collections.Generic;
using System.Linq;
using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Contract;
using Kontent.Ai.ModelGenerator.Core.Helpers;

namespace Kontent.Ai.ModelGenerator.Core.Services;

/// <summary>
/// Maps Management API element inputs to <see cref="ManagementElementOutput"/> records ready
/// for emission. Covers text, number, date_time, custom, url_slug, and multiple_choice in this
/// slice; rich text, asset, taxonomy, linked items, subpages, and snippets are added in later
/// slices.
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

    private static AttributeSpec KontentElement(string codename, string id) =>
        new("KontentElement",
        [
            AttributeArg.Named("Codename", codename),
            AttributeArg.Named("Id", id),
        ]);
}
