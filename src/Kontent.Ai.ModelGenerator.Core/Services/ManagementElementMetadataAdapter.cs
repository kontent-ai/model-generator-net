using System.Linq;
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
    public static ManagementElementInput ToInput(ElementMetadataBase element, string contentTypeClassName) =>
        element switch
        {
            TextElementMetadataModel t => new TextElementInput(
                Codename: t.Codename,
                Id: t.Id.ToString(),
                MaximumCharacters: ResolveCharacterLimit(t.MaximumTextLength),
                Regex: ResolveRegex(t.ValidationRegex)),

            NumberElementMetadataModel n => new NumberElementInput(n.Codename, n.Id.ToString()),

            DateTimeElementMetadataModel d => new DateTimeElementInput(d.Codename, d.Id.ToString()),

            CustomElementMetadataModel c => new CustomElementInput(c.Codename, c.Id.ToString()),

            UrlSlugElementMetadataModel u => new UrlSlugElementInput(
                Codename: u.Codename,
                Id: u.Id.ToString(),
                Regex: ResolveRegex(u.ValidationRegex)),

            MultipleChoiceElementMetadataModel mc => new MultipleChoiceElementInput(
                Codename: mc.Codename,
                Id: mc.Id.ToString(),
                EnumTypeName: BuildEnumTypeName(contentTypeClassName, mc.Codename),
                IsSingleSelect: mc.Mode == MultipleChoiceMode.Single,
                Options: (mc.Options ?? []).Select(o =>
                    new MultipleChoiceOptionInput(o.Codename, o.Id.ToString())).ToList()),

            _ => null,
        };

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
}
