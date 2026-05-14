using System;
using System.Collections.Generic;
using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Contract;

namespace Kontent.Ai.ModelGenerator.Core.Services;

/// <summary>
/// Maps Management API element inputs to <see cref="ManagementProperty"/> records ready for emission.
/// Handles only the "simple" element types in this slice — text, number, date_time, custom, url_slug.
/// Collection types (asset/taxonomy/linked items/subpages), rich_text, and multiple_choice are added in
/// later slices.
/// </summary>
public sealed class ManagementElementService : IManagementElementService
{
    public ManagementProperty BuildProperty(ManagementElementInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return input switch
        {
            TextElementInput t => BuildText(t),
            NumberElementInput n => BuildSimple(n.Codename, n.Id, "decimal?"),
            DateTimeElementInput d => BuildSimple(d.Codename, d.Id, "DateTimeOffset?"),
            CustomElementInput c => BuildSimple(c.Codename, c.Id, "string?"),
            UrlSlugElementInput u => BuildUrlSlug(u),
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

    private static AttributeSpec KontentElement(string codename, string id) =>
        new("KontentElement",
        [
            AttributeArg.Named("Codename", codename),
            AttributeArg.Named("Id", id),
        ]);
}
