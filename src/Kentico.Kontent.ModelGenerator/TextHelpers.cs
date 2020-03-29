using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Kentico.Kontent.ModelGenerator
{
    public static class TextHelpers
    {
        private static readonly Regex lineEndings = new Regex(@"\r\n|\n\r|\n|\r");

        /// <summary>
        /// Returns a valid CSharp Identifier in a Pascal Case format for given string.
        /// </summary>
        public static string GetValidPascalCaseIdentifierName(string name)
        {
            const string WORD_SEPARATOR = " ";
            string sanitizedName = Regex.Replace(name, "[^a-zA-Z0-9]", WORD_SEPARATOR, RegexOptions.IgnoreCase | RegexOptions.Multiline);

            // Remove leading numbers and leading whitespace (e.g.: '  123Name123' -> 'Name123'
            sanitizedName = Regex.Replace(sanitizedName, "^(\\s|[0-9])+", "");

            if (sanitizedName == String.Empty)
            {
                throw new InvalidIdentifierException($"Unable to create a valid Identifier from '{name}'");
            }

            return sanitizedName
                .ToLower()
                .Split(new[] { WORD_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries)
                .Select(word => Char.ToUpper(word[0]) + word.Substring(1))
                .Aggregate((previous, current) => previous + current); ;
        }

        public static string NormalizeLineEndings(this string text)
        {
            return lineEndings.Replace(text, "\r\n");
        }
    }
}
