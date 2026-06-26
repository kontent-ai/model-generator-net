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
    /// per-element enum names (<c>{ClassName}{PascalElementCodename}</c>) so the same multiple-choice
    /// element on two content types produces two distinct, collision-free enums. Unused for
    /// non-enum-producing element types.
    /// </param>
    public static ManagementElementInput ToInput(ElementMetadataBase element, string contentTypeClassName) =>
        element switch
        {
            TextElementMetadataModel t => new TextElementInput(element.Codename, t.Id.ToString()),
            NumberElementMetadataModel n => new NumberElementInput(element.Codename, n.Id.ToString()),
            DateTimeElementMetadataModel d => new DateTimeElementInput(element.Codename, d.Id.ToString()),
            CustomElementMetadataModel c => new CustomElementInput(element.Codename, c.Id.ToString()),
            UrlSlugElementMetadataModel u => new UrlSlugElementInput(element.Codename, u.Id.ToString()),
            MultipleChoiceElementMetadataModel mc => new MultipleChoiceElementInput(
                Codename: element.Codename,
                Id: mc.Id.ToString(),
                EnumTypeName: BuildEnumTypeName(contentTypeClassName, element.Codename),
                Options: (mc.Options ?? []).Select(o =>
                    new MultipleChoiceOptionInput(o.Codename, o.Id.ToString())).ToList()),
            LinkedItemsElementMetadataModel li => new LinkedItemsElementInput(element.Codename, li.Id.ToString()),
            SubpagesElementMetadataModel sp => new SubpagesElementInput(element.Codename, sp.Id.ToString()),
            TaxonomyElementMetadataModel tx => new TaxonomyElementInput(element.Codename, tx.Id.ToString()),
            RichTextElementMetadataModel rt => new RichTextElementInput(element.Codename, rt.Id.ToString()),
            AssetElementMetadataModel a => new AssetElementInput(element.Codename, a.Id.ToString()),
            _ => null,
        };

    private static string BuildEnumTypeName(string contentTypeClassName, string elementCodename) =>
        contentTypeClassName + TextHelpers.GetValidPascalCaseIdentifierName(elementCodename);
}
