using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Kontent.Ai.ModelGenerator
{
    /// <summary>
    /// Donet argument parsing is currently not working correctly when using backslash at the end of the path argument value. 
    /// This class is fixing the bug.
    /// See <see href="https://github.com/dotnet/corefxlab/issues/1311"/>.
    /// </summary>
    internal static class ArgumentParser
    {
        internal static string[] CorrectArguments(string[] args)
        {
            var parsedArgs = new List<string>();
            foreach (var arg in args)
            {
                // An argument name at the start of the current 'arg'.
                var isArgumentName = Regex.Match(arg, @"^(-{1,2}\w+)", RegexOptions.Compiled);
                if (isArgumentName.Success)
                {
                    parsedArgs.Add(arg.Trim());
                    continue;
                }

                // Arguments and their values that the current 'arg' contains.
                var argumentStarts = Regex.Matches(arg, @"\s+(-{1,2}\w+)", RegexOptions.Compiled).ToList();
                if (argumentStarts.Count > 0)
                {
                    parsedArgs.AddRange(ParseCorruptedArguments(arg, argumentStarts));
                    continue;
                }

                if (!string.IsNullOrEmpty(arg))
                {
                    // The current 'arg' contains neither an argument name nor a mix of arguments and their values.
                    parsedArgs.AddRange(ParseArgumentValues(arg));
                }
            }

            return parsedArgs.ToArray();
        }

        internal static List<string> ParseCorruptedArguments(string arg, List<Match> argumentStarts)
        {
            // Trailing value of the preceding argument.
            string trailingValue = arg.Substring(0, argumentStarts.First().Index).Trim();
            var parsedArgs = new List<string>();
            if (!string.IsNullOrEmpty(trailingValue))
            {
                parsedArgs.AddRange(ParseArgumentValues(trailingValue));
            }

            for (int argumentIndex = 0; argumentIndex <= argumentStarts.Count - 1; argumentIndex++)
            {
                parsedArgs.AddRange(ParseArgumentPair(arg, argumentStarts, argumentIndex));
            }
            return parsedArgs;
        }

        internal static List<string> ParseArgumentPair(string arg, List<Match> argumentStarts, int argumentIndex)
        {
            var parsedArgs = new List<string>
            {
                // Add the argument name itself.
                arg.Substring(argumentStarts[argumentIndex].Index, argumentStarts[argumentIndex].Length).Trim()
            };

            // Calculate the span of the raw value.
            var valueStart = argumentStarts[argumentIndex].Index + argumentStarts[argumentIndex].Length;
            var valueEnd = argumentIndex == argumentStarts.Count - 1 ? arg.Length : argumentStarts[argumentIndex + 1].Index;

            var rawValue = arg[valueStart..valueEnd].Trim();

            if (!string.IsNullOrEmpty(rawValue))
            {
                parsedArgs.AddRange(ParseArgumentValues(rawValue));
            }
            return parsedArgs;
        }

        internal static List<string> ParseArgumentValues(string rawValue)
        {
            var (quotedValueList, lastDoubleQuotesIndex) = ParsePartiallyQuotedValues(rawValue);
            var trailingValues = new List<string>();

            if (lastDoubleQuotesIndex < rawValue.Length)
            {
                // Trailing values, not terminated with double quotes.
                trailingValues.AddRange(rawValue[lastDoubleQuotesIndex..].Trim().Split(' '));
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
                .Select(value => value.Value.Trim(' ', '"', '\\'))
                .ToList();

            var lastQuotedValue = valuesWithTrailingQuotes.LastOrDefault();
            var lastDoubleQuotesIndex = lastQuotedValue != null ? lastQuotedValue.Index + lastQuotedValue.Length : 0;

            return (quotedValueList, lastDoubleQuotesIndex);
        }
    }
}
