using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Kentico.Kontent.ModelGenerator.Core.Helpers
{
    internal static class ClassDeclarationHelper
    {
        public static SyntaxTrivia GenerateSyntaxTrivia(string customComment) => SyntaxFactory.Comment(TextHelpers.GenerateCommentString(customComment));
    }
}
