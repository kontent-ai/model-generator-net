using System;
using System.Collections.Generic;
using System.Linq;
using Kontent.Ai.Management.Models.Shared;
using Kontent.Ai.Management.Models.Types;
using Kontent.Ai.Management.Models.Types.Elements;
using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Helpers;

namespace Kontent.Ai.ModelGenerator.Core.Services;

/// <summary>
/// Adapts MAPI's <see cref="ElementMetadataBase"/> hierarchy into the
/// <see cref="ManagementElementInput"/> records that <see cref="ManagementElementService"/> consumes.
/// Pure function. Returns <c>null</c> for element types that the current generator slice doesn't
/// emit yet (the orchestrator turns those into warn-and-skip).
/// </summary>
internal static class ManagementElementMetadataAdapter
{
    /// <param name="contentTypeClassName">
    /// PascalCased name of the content-type class the element lives on. Used to build unique
    /// per-element enum names (e.g. <c>{ClassName}{PascalElementCodename}</c>) so the same
    /// multiple-choice element on two content types produces two distinct, collision-free enums.
    /// Unused for non-enum-producing element types.
    /// </param>
    /// <param name="resolveTypeCodename">
    /// Resolves a content-type id to its codename. MAPI returns <c>allowed_content_types</c>
    /// references as id-only on type metadata responses; without resolution, the generator would
    /// drop the constraint entirely. The orchestrator builds this from the pre-fetched type list.
    /// Returns <c>null</c> when the id can't be resolved (e.g. type was deleted but a stale
    /// reference lingers) — the adapter then warns via <paramref name="warn"/> and drops that entry.
    /// </param>
    /// <param name="warn">
    /// Optional callback for soft constraint losses (e.g. an unresolvable allowed-type reference).
    /// </param>
    public static ManagementElementInput ToInput(
        ElementMetadataBase element,
        string contentTypeClassName,
        Func<Guid, string> resolveTypeCodename = null,
        Action<string> warn = null)
    {
        var codename = element.Codename;

        return element switch
        {
            TextElementMetadataModel t => new TextElementInput(
                Codename: codename,
                Id: t.Id.ToString(),
                MaximumCharacters: ResolveCharacterLimit(t.MaximumTextLength),
                Regex: ResolveRegex(t.ValidationRegex)),

            NumberElementMetadataModel n => new NumberElementInput(codename, n.Id.ToString()),

            DateTimeElementMetadataModel d => new DateTimeElementInput(codename, d.Id.ToString()),

            CustomElementMetadataModel c => new CustomElementInput(codename, c.Id.ToString()),

            UrlSlugElementMetadataModel u => new UrlSlugElementInput(
                Codename: codename,
                Id: u.Id.ToString(),
                Regex: ResolveRegex(u.ValidationRegex)),

            MultipleChoiceElementMetadataModel mc => new MultipleChoiceElementInput(
                Codename: codename,
                Id: mc.Id.ToString(),
                EnumTypeName: BuildEnumTypeName(contentTypeClassName, codename),
                IsSingleSelect: mc.Mode == MultipleChoiceMode.Single,
                Options: (mc.Options ?? []).Select(o =>
                    new MultipleChoiceOptionInput(o.Codename, o.Id.ToString())).ToList()),

            LinkedItemsElementMetadataModel li => new LinkedItemsElementInput(
                Codename: codename,
                Id: li.Id.ToString(),
                AllowedTypeCodenames: ResolveAllowedTypeCodenames(li.AllowedTypes, resolveTypeCodename, warn, codename),
                ItemCount: ResolveCountLimit(li.ItemCountLimit)),

            SubpagesElementMetadataModel sp => new SubpagesElementInput(
                Codename: codename,
                Id: sp.Id.ToString(),
                AllowedTypeCodenames: ResolveAllowedTypeCodenames(sp.AllowedContentTypes, resolveTypeCodename, warn, codename),
                ItemCount: ResolveCountLimit(sp.ItemCountLimit)),

            TaxonomyElementMetadataModel tx => new TaxonomyElementInput(
                Codename: codename,
                Id: tx.Id.ToString(),
                TaxonomyGroup: ResolveReferenceKey(tx.TaxonomyGroup),
                TermCount: ResolveCountLimit(tx.TermCountLimit)),

            RichTextElementMetadataModel rt => new RichTextElementInput(
                Codename: codename,
                Id: rt.Id.ToString(),
                AllowedTypeCodenames: ResolveAllowedTypeCodenames(rt.AllowedTypes, resolveTypeCodename, warn, codename),
                AllowedItemLinkTypeCodenames: ResolveAllowedTypeCodenames(rt.AllowedItemLinkTypes, resolveTypeCodename, warn, codename),
                MaximumCharacters: ResolveCharacterLimit(rt.MaximumTextLength)),

            AssetElementMetadataModel a => new AssetElementInput(
                Codename: codename,
                Id: a.Id.ToString(),
                AssetCount: ResolveCountLimit(a.AssetCountLimit),
                MaximumFileSizeBytes: a.MaximumFileSize,
                AllowedFileType: ResolveAssetFileType(a.AllowedFileTypes)),

            _ => null,
        };
    }

    private static int? ResolveCharacterLimit(MaximumTextLengthModel limit) =>
        limit is { AppliesTo: TextLengthLimitType.Characters }
            ? limit.Value
            : null;

    private static string ResolveRegex(ValidationRegexModel regex) =>
        regex is { IsActive: true } && !string.IsNullOrWhiteSpace(regex.Regex)
            ? regex.Regex
            : null;

    private static string BuildEnumTypeName(string contentTypeClassName, string elementCodename) =>
        contentTypeClassName + TextHelpers.GetValidPascalCaseIdentifierName(elementCodename);

    /// <summary>
    /// Returns codenames for the given references. Each reference is resolved to a codename:
    /// preferred from <see cref="Reference.Codename"/> if present, else looked up via the
    /// <paramref name="resolveTypeCodename"/> callback (MAPI typically returns id-only refs on
    /// type metadata responses). An entry that resolves to nothing emits a warning and is dropped.
    /// Returns null when no entries survive — the service then skips emitting an empty
    /// <c>[AllowedTypes()]</c>. A non-null but empty input also returns null (matches MAPI's
    /// "no constraint" semantics for an empty <c>allowed_content_types</c>).
    /// </summary>
    private static IReadOnlyList<string> ResolveAllowedTypeCodenames(
        IEnumerable<Reference> references,
        Func<Guid, string> resolveTypeCodename,
        Action<string> warn,
        string ownerElementCodename)
    {
        if (references is null)
        {
            return null;
        }

        var codenames = new List<string>();
        foreach (var reference in references)
        {
            if (!string.IsNullOrWhiteSpace(reference.Codename))
            {
                codenames.Add(reference.Codename);
                continue;
            }

            if (reference.Id is Guid id && resolveTypeCodename is not null)
            {
                var resolved = resolveTypeCodename(id);
                if (!string.IsNullOrWhiteSpace(resolved))
                {
                    codenames.Add(resolved);
                    continue;
                }
            }

            warn?.Invoke(
                $"Could not resolve allowed-type reference (id={reference.Id?.ToString() ?? "?"}) " +
                $"on element '{ownerElementCodename}'; constraint entry dropped.");
        }

        return codenames.Count == 0 ? null : codenames;
    }

    /// <summary>
    /// Returns the reference's codename (preferred) or id (fallback) as a single string —
    /// used for the taxonomy-group key. Null if the reference has neither.
    /// </summary>
    private static string ResolveReferenceKey(Reference reference)
    {
        if (reference is null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(reference.Codename))
        {
            return reference.Codename;
        }

        return reference.Id?.ToString();
    }

    private static CountLimit ResolveCountLimit(LimitModel limit) =>
        limit is null
            ? null
            : new CountLimit(limit.Value, MapLimitMode(limit.Condition));

    private static CountLimitMode MapLimitMode(LimitType limitType) => limitType switch
    {
        LimitType.AtLeast => CountLimitMode.AtLeast,
        LimitType.AtMost => CountLimitMode.AtMost,
        LimitType.Exactly => CountLimitMode.Exactly,
        _ => throw new ArgumentOutOfRangeException(nameof(limitType), limitType, "Unknown limit type."),
    };

    private static AssetFileType? ResolveAssetFileType(FileType fileType) => fileType switch
    {
        FileType.Adjustable => AssetFileType.Adjustable,
        // FileType.Any → null (no constraint to emit).
        _ => null,
    };
}
