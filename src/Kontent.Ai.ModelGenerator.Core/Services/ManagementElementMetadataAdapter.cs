using Kontent.Ai.Management.Models.Types.Elements;
using Kontent.Ai.ModelGenerator.Core.Common;

namespace Kontent.Ai.ModelGenerator.Core.Services;

/// <summary>
/// Adapts MAPI's <see cref="ElementMetadataBase"/> hierarchy into the
/// <see cref="ManagementElementInput"/> records that <see cref="ManagementElementService"/> consumes.
/// Pure function. Returns <c>null</c> for element types that the current generator slice doesn't
/// emit yet (the orchestrator turns those into warn-and-skip).
/// </summary>
internal static class ManagementElementMetadataAdapter
{
    public static ManagementElementInput ToInput(ElementMetadataBase element) =>
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
}
