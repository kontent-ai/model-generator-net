using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CloudModelGenerator
{
    internal static class ArgumentParser
    {
        internal static string[] CorrectArguments(string[] args)
        {
            var parsedArgs = new List<string>();

            foreach (var arg in args)
            {
                // An argument name at the start of the current 'arg'.
                var isArgumentName = Regex.Match(arg, @"^(-{1,2}\w+)", RegexOptions.Compiled);

                // Arguments and their values that the current 'arg' contains.
                var argumentStarts = Regex.Matches(arg, @"\s+(-{1,2}\w+)", RegexOptions.Compiled).ToList();

                if (isArgumentName.Success)
                {
                    parsedArgs.Add(arg.Trim());
                }
                else if (argumentStarts.Count > 0)
                {
                    ParseCorruptedArguments(parsedArgs, arg, argumentStarts);
                }
                else if (!string.IsNullOrEmpty(arg))
                {
                    // The current 'arg' contains neither an argument name nor a mix of arguments and their values.
                    parsedArgs.AddRange(ParseArgumentValues(arg));
                }
            }

            return parsedArgs.ToArray();
        }

        internal static void ParseCorruptedArguments(List<string> parsedArgs, string arg, List<Match> argumentStarts)
        {
            // Trailing value of the preceding argument.
            string trailingValue = arg.Substring(0, argumentStarts.First().Index).Trim();

            if (!string.IsNullOrEmpty(trailingValue))
            {
                parsedArgs.AddRange(ParseArgumentValues(trailingValue));
            }

            for (int i = 0; i <= argumentStarts.Count - 1; i++)
            {
                ParseArgumentPair(parsedArgs, arg, argumentStarts, i);
            }
        }

        internal static void ParseArgumentPair(List<string> parsedArgs, string arg, List<Match> argumentStarts, int i)
        {
            // Add the argument name itself.
            parsedArgs.Add(arg.Substring(argumentStarts[i].Index, argumentStarts[i].Length).Trim());

            // Calculate the span of the raw value.
            var valueStart = argumentStarts[i].Index + argumentStarts[i].Length;
            var valueEnd = i == argumentStarts.Count - 1 ? arg.Length : argumentStarts[i + 1].Index;

            var rawValue = arg.Substring(valueStart, valueEnd - valueStart).Trim();

            if (!string.IsNullOrEmpty(rawValue))
            {
                parsedArgs.AddRange(ParseArgumentValues(rawValue));
            }
        }

        internal static List<string> ParseArgumentValues(string rawValue)
        {
            (var quotedValueList, var lastDoubleQuotesIndex) = ParsePartiallyQuotedValues(rawValue);
            var trailingValues = new List<string>();

            if (lastDoubleQuotesIndex < rawValue.Length)
            {
                // Trailing values, not terminated with double quotes.
                trailingValues.AddRange(rawValue.Substring(lastDoubleQuotesIndex, rawValue.Length - lastDoubleQuotesIndex).Trim().Split(' '));
            }

            var merge = quotedValueList.Concat(trailingValues).ToList();

            return merge.Count > 0
                ? merge
                : new List<string>(new[] { rawValue.Trim() });
        }

        internal static (List<string> quotedValueList, int lastDoubleQuotesIndex) ParsePartiallyQuotedValues(string rawValue)
        {
            // Argument value incorrectly parsed by the runtime (due to the use of a "backslash and double quotes" sequence).
            var valuesWithTrailingQuotes = Regex.Matches(rawValue, @"([^""]+)("")", RegexOptions.Compiled);

            var quotedValueList = valuesWithTrailingQuotes
                .Select(value => value.Value.Trim(new char[] { ' ', '"', '\\' }))
                .ToList();

            var lastQuotedValue = valuesWithTrailingQuotes.LastOrDefault();
            var lastDoubleQuotesIndex = lastQuotedValue != null ? lastQuotedValue.Index + lastQuotedValue.Length : 0;

            return (quotedValueList, lastDoubleQuotesIndex);
        }
    }
}
