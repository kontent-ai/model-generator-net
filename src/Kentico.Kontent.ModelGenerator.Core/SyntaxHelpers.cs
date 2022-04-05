
using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kentico.Kontent.ModelGenerator.Core
{
    public static class SyntaxHelpers
    {
        public static SyntaxTrivia GenerateComment(string comment)
        {
            return SyntaxFactory.Comment(comment + Environment.NewLine + Environment.NewLine);
        }
    }
}
