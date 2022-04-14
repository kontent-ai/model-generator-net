using System;
using System.Linq;
using System.Text.RegularExpressions;
using Kentico.Kontent.ModelGenerator.Core.Common;

namespace Kentico.Kontent.ModelGenerator.Core.Helpers
{
    public static class TextHelpers
    {
        private static readonly Regex LineEndings = new Regex(@"\r\n|\n\r|\n|\r");
        private const string WordSeparator = " ";

        /// <summary>
        /// Returns a valid CSharp Identifier in a Pascal Case format for given string.
        /// </summary>
        public static string GetValidPascalCaseIdentifierName(string name)
        {
            string sanitizedName = Regex.Replace(name, "[^a-zA-Z0-9]", WordSeparator, RegexOptions.IgnoreCase | RegexOptions.Multiline);

            // Remove leading numbers and leading whitespace (e.g.: '  123Name123' -> 'Name123'
            sanitizedName = Regex.Replace(sanitizedName, "^(\\s|[0-9])+", "");

            if (sanitizedName == string.Empty)
            {
                throw new InvalidIdentifierException($"Unable to create a valid Identifier from '{name}'");
            }

            return sanitizedName
                .ToLower()
                .Split(new[] { WordSeparator }, StringSplitOptions.RemoveEmptyEntries)
                .Select(word => char.ToUpper(word[0]) + word.Substring(1))
                .Aggregate((previous, current) => previous + current);
        }

        public static string NormalizeLineEndings(this string text)
        {
            return LineEndings.Replace(text, Environment.NewLine);
        }

        public static string AppendNewLines(this string text) => $"{text}{Environment.NewLine}{Environment.NewLine}";
    }
}
