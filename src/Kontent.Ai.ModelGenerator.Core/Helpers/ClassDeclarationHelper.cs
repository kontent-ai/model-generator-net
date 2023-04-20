using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Kontent.Ai.ModelGenerator.Core.Helpers;

internal static class ClassDeclarationHelper
{
    public static SyntaxTrivia GenerateSyntaxTrivia(string customComment)
    {
        if (string.IsNullOrWhiteSpace(customComment))
        {
            throw new ArgumentNullException(nameof(customComment));
        }

        if (!customComment.StartsWith("// "))
        {
            throw new ArgumentException("Comment is invalid.", nameof(customComment));
        }

        return SyntaxFactory.Comment(TextHelpers.GenerateCommentString(customComment));
    }
}
