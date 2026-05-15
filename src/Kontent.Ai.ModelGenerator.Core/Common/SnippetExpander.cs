using System;
using System.Collections.Generic;
using Kontent.Ai.Management.Models.Shared;
using Kontent.Ai.Management.Models.TypeSnippets;
using Kontent.Ai.Management.Models.Types.Elements;

namespace Kontent.Ai.ModelGenerator.Core.Common;

/// <summary>
/// Flattens snippet references in a content type's element list. Snippet elements are
/// replaced by the snippet's own elements with prefixed codenames
/// (<c>{snippetCodename}__{elementCodename}</c>) — matching MAPI's wire shape for
/// snippet-contributed elements in variant responses. Non-snippet elements pass through
/// unchanged with no codename override.
/// </summary>
public static class SnippetExpander
{
    /// <summary>
    /// One element ready for downstream emission. <paramref name="CodenameOverride"/> is non-null
    /// for elements contributed by a snippet — the orchestrator must pass it to the adapter
    /// instead of reading <see cref="ElementMetadataBase.Codename"/> from the inner element.
    /// </summary>
    public sealed record ExpandedElement(ElementMetadataBase Element, string CodenameOverride);

    public static IEnumerable<ExpandedElement> Expand(
        IEnumerable<ElementMetadataBase> elements,
        Func<Reference, ContentTypeSnippetModel> resolveSnippet,
        Action<string> warn)
    {
        ArgumentNullException.ThrowIfNull(resolveSnippet);
        ArgumentNullException.ThrowIfNull(warn);

        if (elements is null)
        {
            yield break;
        }

        foreach (var element in elements)
        {
            if (element is ContentTypeSnippetElementMetadataModel snippetEl)
            {
                var snippet = snippetEl.SnippetIdentifier is null
                    ? null
                    : resolveSnippet(snippetEl.SnippetIdentifier);

                if (snippet is null)
                {
                    warn($"Could not resolve snippet reference on snippet element (id={snippetEl.Id}); skipping.");
                    continue;
                }

                foreach (var inner in snippet.Elements ?? [])
                {
                    if (inner is ContentTypeSnippetElementMetadataModel)
                    {
                        // MAPI doesn't allow snippets-in-snippets. Defensive guard rather than
                        // silent recursion if the API ever surfaces one.
                        warn($"Snippet '{snippet.Codename}' contains a nested snippet element — MAPI should not allow this. Skipping inner element.");
                        continue;
                    }

                    // Guidelines inside a snippet behave the same as guidelines on a type — drop silently.
                    if (inner is GuidelinesElementMetadataModel)
                    {
                        continue;
                    }

                    yield return new ExpandedElement(inner, $"{snippet.Codename}__{inner.Codename}");
                }
            }
            else
            {
                yield return new ExpandedElement(element, CodenameOverride: null);
            }
        }
    }
}
