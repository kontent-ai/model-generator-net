﻿using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace Kentico.Kontent.ModelGenerator.Tests
{
    public class ArgumentParserTests
    {
        public static IEnumerable<object[]> CorrectArguments_GetFixture()
        {
            yield return new object[]
            {
                new[] { "-o", @"C:\KC\demo\"" --namespace Some.NameSpace" },
                new[] { "-o", @"C:\KC\demo", "--namespace", "Some.NameSpace" },
            };
            yield return new object[]
            {
                new[] { "--outputdir", @"C:\KC\demo\"" --namespace Some.NameSpace" },
                new[] { "--outputdir", @"C:\KC\demo", "--namespace", "Some.NameSpace" },
            };
            yield return new object[]
            {
                new[] { "-o", @"C:\KC\demo\"" -n Some.NameSpace" },
                new[] { "-o", @"C:\KC\demo", "-n", "Some.NameSpace" },
            };
            yield return new object[]
            {
                new[] { "-o", @"C:\KC\demo\"" --generatepartials"  },
                new[] { "-o", @"C:\KC\demo", "--generatepartials" }
            };
            yield return new object[]
            {
                new[] { "-o", @"C:\KC\demo\"" --namespace Some.NameSpace -g" },
                new[] { "-o", @"C:\KC\demo", "--namespace", "Some.NameSpace", "-g" },
            };
            yield return new object[]
            {
                new[] { "-o", @"C:\KC\demo\"" --namespace Some.NameSpace --generatepartials" },
                new[] { "-o", @"C:\KC\demo", "--namespace", "Some.NameSpace", "--generatepartials" },
            };
            yield return new object[]
            {
                new[] { "-o", @"C:\KC\demo\"" --namespace Some.NameSpace --generatepartials -s --projectId 975bf280-fd91-488c-994c-2f04416e5ee3" },
                new[] { "-o", @"C:\KC\demo", "--namespace", "Some.NameSpace", "--generatepartials", "-s", "--projectId", "975bf280-fd91-488c-994c-2f04416e5ee3" },
            };
            yield return new object[]
            {
                new[] { "-o", @"C:\KC\demo\"" -s --projectId 975bf280-fd91-488c-994c-2f04416e5ee3" },
                new[] { "-o", @"C:\KC\demo", "-s", "--projectId", "975bf280-fd91-488c-994c-2f04416e5ee3" },
            };
            yield return new object[]
            {
                new[] { "-o", @"C:\KC\demo\"" --structuredmodel --projectId 975bf280-fd91-488c-994c-2f04416e5ee3" },
                new[] { "-o", @"C:\KC\demo", "--structuredmodel", "--projectId", "975bf280-fd91-488c-994c-2f04416e5ee3" },
            };
            yield return new object[]
            {
                new[] { "-o", @"C:\KC\demo\"" -b ""CodeGeneratorBase"" -p 975bf280-fd91-488c-994c-2f04416e5ee3" },
                new[] { "-o", @"C:\KC\demo", "-b", "CodeGeneratorBase", "-p", "975bf280-fd91-488c-994c-2f04416e5ee3" },
            };
            yield return new object[]
            {
                new[] { "-o", @"C:\KC\demo\"" --baseclass ""CodeGeneratorBase""  -p 975bf280-fd91-488c-994c-2f04416e5ee3" },
                new[] { "-o", @"C:\KC\demo", "--baseclass", "CodeGeneratorBase", "-p", "975bf280-fd91-488c-994c-2f04416e5ee3" },
            };
            yield return new object[]
            {
                new[] { "-o", @"C:\KC\demo\"" -t -p 975bf280-fd91-488c-994c-2f04416e5ee3" },
                new[] { "-o", @"C:\KC\demo", "-t", "-p", "975bf280-fd91-488c-994c-2f04416e5ee3" },
            };
            yield return new object[]
            {
                new[] { "-o", @"C:\KC\demo\"" --withtypeprovider -p 975bf280-fd91-488c-994c-2f04416e5ee3" },
                new[] { "-o", @"C:\KC\demo", "--withtypeprovider", "-p", "975bf280-fd91-488c-994c-2f04416e5ee3" },
            };
            yield return new object[]
            {
                new[] { "-o", @"C:\KC\demo\"" --managementapi true --apikey ""key"" -p 975bf280-fd91-488c-994c-2f04416e5ee3" },
                new[] { "-o", @"C:\KC\demo", "--managementapi", "true", "--apikey", "key", "-p", "975bf280-fd91-488c-994c-2f04416e5ee3" },
            };
            yield return new object[]
            {
                new[] { "-o", @"C:\KC\demo\"" -m true -k ""key"" -p 975bf280-fd91-488c-994c-2f04416e5ee3" },
                new[] { "-o", @"C:\KC\demo", "-m", "true", "-k", "key", "-p", "975bf280-fd91-488c-994c-2f04416e5ee3" },
            };
        }

        [Theory]
        [MemberData(nameof(CorrectArguments_GetFixture))]
        public void CorrectArguments_CorrectionPerformed(string[] inputArgs, string[] expectedArgs)
        {
            var correctedArgs = ArgumentParser.CorrectArguments(inputArgs);
            Assert.Equal(expectedArgs, correctedArgs);
        }

        public static IEnumerable<object[]> ParseCorruptedArguments_GetFixture()
        {
            var input = new Dictionary<string, List<string>>
            {
                {
                    "value1 --argument1",
                    new List<string>
                    {
                        "value1",
                        "--argument1"
                    }
                },
                {
                    "value1 value2 --argument1",
                    new List<string>
                    {
                        "value1",
                        "value2",
                        "--argument1"
                    }
                },
                {
                    "--argument1",
                    new List<string>
                    {
                        "--argument1"
                    }
                }
            };

            foreach (var item in input)
            {
                var argumentStarts = Regex.Matches(item.Key, @"(-{1,2}\w+)", RegexOptions.Compiled).ToList();

                yield return new object[]
                {
                    item.Key,
                    argumentStarts,
                    item.Value
                };
            }
        }

        [Theory]
        [MemberData(nameof(ParseCorruptedArguments_GetFixture))]
        public void ParseCorruptedArguments_ParsesArguments(string arg, List<Match> argumentStarts, List<string> expectedResult)
        {
            var parsedArgs = ArgumentParser.ParseCorruptedArguments(arg, argumentStarts);

            Assert.Equal(expectedResult, parsedArgs);
        }

        public static IEnumerable<object[]> ParseArgumentPairs_GetFixture()
        {
            var arg = "value1 --argument1 value2\" --argument2 value3 value4 -a3 value5 --argument4 value6";
            var argumentStarts = Regex.Matches(arg, @"\s+(-{1,2}\w+)", RegexOptions.Compiled).ToList();

            return new List<object[]>
            {
                new object[]
                {
                    arg,
                    argumentStarts,
                    0,
                    new List<string>
                    {
                        "--argument1",
                        "value2"
                    }
                },
                new object[]
                {
                    arg,
                    argumentStarts,
                    1,
                    new List<string>
                    {
                        "--argument2",
                        "value3",
                        "value4"
                    }
                },
                new object[]
                {
                    arg,
                    argumentStarts,
                    2,
                    new List<string>
                    {
                        "-a3",
                        "value5"
                    }
                },
                new object[]
                {
                    arg,
                    argumentStarts,
                    3,
                    new List<string>
                    {
                        "--argument4",
                        "value6"
                    }
                },
            };
        }

        [Theory]
        [MemberData(nameof(ParseArgumentPairs_GetFixture))]
        public void ParseArgumentPair_ParsesPair(string arg, List<Match> argumentStarts, int i, List<string> expectedResult)
        {
            var parsedArgs = ArgumentParser.ParseArgumentPair(arg, argumentStarts, i);

            Assert.Equal(expectedResult, parsedArgs);
        }

        public static IEnumerable<object[]> ParseArgumentValues_GetFixture() =>
            new List<object[]>
            {
                new object[]
                {
                    "value1\" value2 value3",
                    new List<string>
                    {
                        "value1",
                        "value2",
                        "value3"
                    }
                },
                new object[]
                {
                    "value1 value2\" value3",
                    new List<string>
                    {
                        "value1 value2",
                        "value3"
                    }
                },
                new object[]
                {
                    "value1\" value2\" value3",
                    new List<string>
                    {
                        "value1",
                        "value2",
                        "value3"
                    }
                },
                new object[]
                {
                    "\" value1\" value2\" value3",
                    new List<string>
                    {
                        "value1",
                        "value2",
                        "value3"
                    }
                },
            };

        [Theory]
        [MemberData(nameof(ParseArgumentValues_GetFixture))]
        public void ParseArgumentValues_ParsesValues(string rawValue, List<string> expectedResult)
        {
            var result = ArgumentParser.ParseArgumentValues(rawValue);

            Assert.Equal(expectedResult, result);
        }

        public static IEnumerable<object[]> ParsePartiallyQuotedValues_GetFixture() =>
            new List<object[]>
            {
                new object[]
                {
                    "value1\" value2",
                    new List<string>
                    {
                        "value1"
                    },
                    7
                },
                new object[]
                {
                    "value1 value2\"",
                    new List<string>
                    {
                        "value1 value2"
                    },
                    14
                },
                new object[]
                {
                    "value1\" value2\"",
                    new List<string>
                    {
                        "value1",
                        "value2"
                    },
                    15
                },
                new object[]
                {
                    "\" value1\" value2\"",
                    new List<string>
                    {
                        "value1",
                        "value2"
                    },
                    17
                },
            };

        [Theory]
        [MemberData(nameof(ParsePartiallyQuotedValues_GetFixture))]
        public void ParsePartiallyQuotedValues_ParsesValues(string rawValue, List<string> expectedResult, int expectedLastIndex)
        {
            var (quotedValueList, lastDoubleQuotesIndex) = ArgumentParser.ParsePartiallyQuotedValues(rawValue);

            Assert.Equal(expectedResult, quotedValueList);
            Assert.Equal(expectedLastIndex, lastDoubleQuotesIndex);
        }
    }
}
