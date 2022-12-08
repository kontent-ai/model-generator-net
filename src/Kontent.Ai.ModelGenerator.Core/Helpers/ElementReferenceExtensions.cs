using Kontent.Ai.ModelGenerator.Core.Configuration;

namespace Kontent.Ai.ModelGenerator.Core.Helpers;

public static class ElementReferenceExtensions
{
    public static bool HasErrorFlag(this ElementReferenceType elementReference) =>
        elementReference.HasFlag(ElementReferenceType.ValidationIssue) || elementReference.HasFlag(ElementReferenceType.NotSet);
}
