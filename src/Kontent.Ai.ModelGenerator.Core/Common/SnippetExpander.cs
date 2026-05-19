using System;
using System.Collections.Generic;
using Kontent.Ai.Management.Models.Shared;
using Kontent.Ai.Management.Models.TypeSnippets;
using Kontent.Ai.Management.Models.Types.Elements;

namespace Kontent.Ai.ModelGenerator.Core.Common;

/// <summary>
/// Flattens snippet references in a content type's element list: each snippet element is
/// replaced inline by the snippet's own elements. MAPI already returns snippet-contributed
/// element codenames pre-prefixed as <c>{snippetCodename}__{elementCodename}</c> — both on the
/// snippet metadata endpoint and in variant responses — so codenames pass through untouched.
/// Expansion only inlines; it does not rewrite codenames. Non-snippet elements pass through
/// unchanged.
/// </summary>
public static class SnippetExpander
{
    public static IEnumerable<ElementMetadataBase> Expand(
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

                    yield return inner;
                }
            }
            else
            {
                yield return element;
            }
        }
    }
}
