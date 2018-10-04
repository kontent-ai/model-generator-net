using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text.RegularExpressions;

namespace CloudModelGenerator
{
    class Program
    {
        static IConfigurationRoot Configuration { get; set; }

        const string HelpOption = "help";

        static int Main(string[] args)
        {
            var correctedArgs = CorrectArguments(args);
            var syntax = Parse(correctedArgs);
            var unexpectedArgs = new List<string>(syntax.RemainingArguments);

            if (unexpectedArgs.Count > 0)
            {
                Console.WriteLine("Invalid arguments!");
                foreach (var unexpectedArgument in unexpectedArgs)
                {
                    Console.WriteLine($"Unrecognized option '{unexpectedArgument}'");
                }
                Console.WriteLine(syntax.GetHelpText());

                return 1;
            }

            return Execute(syntax);
        }

        internal static string[] CorrectArguments(string[] args)
        {
            var parsedArgs = new List<string>();
            var joinedArgs = " " + string.Join(" ", args);

            foreach (var arg in args)
            {
                // An argument name at the start of the current 'arg'.
                var isArgumentName = Regex.Match(arg, @"^(-{1,2}\w+)");

                // Arguments and their values that the current 'arg' contains.
                var argumentStarts = Regex.Matches(arg, @"\s+(-{1,2}\w+)").ToList();

                if (isArgumentName.Success)
                {
                    parsedArgs.Add(arg.Trim());
                }
                else if (argumentStarts.Count > 0)
                {
                    // Trailing value of the preceding argument.
                    string trailingValue = arg.Substring(0, argumentStarts.First().Index).Trim();

                    if (!string.IsNullOrEmpty(trailingValue))
                    {
                        parsedArgs.AddRange(ParseValues(trailingValue));
                    }

                    for (int i = 0; i <= argumentStarts.Count - 1; i++)
                    {
                        // Add the argument name itself.
                        parsedArgs.Add(arg.Substring(argumentStarts[i].Index, argumentStarts[i].Length).Trim());

                        // Calculate the span of the raw value.
                        var valueStart = argumentStarts[i].Index + argumentStarts[i].Length;
                        var valueEnd = i == argumentStarts.Count - 1 ? arg.Length : argumentStarts[i + 1].Index;

                        var rawValue = arg.Substring(valueStart, valueEnd - valueStart).Trim();

                        if (!string.IsNullOrEmpty(rawValue))
                        {
                            parsedArgs.AddRange(ParseValues(rawValue));
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(arg))
                {
                    // The current 'arg' contains neither an argument name nor a mix of arguments and their values.
                    parsedArgs.AddRange(ParseValues(arg));
                }
            }

            return parsedArgs.ToArray();
        }

        internal static List<string> ParseValues(string rawValue)
        {
            // Argument value incorrectly parsed by the runtime (due to the use of a "backslash and double quotes" sequence).
            var valuesWithTrailingQuotes = Regex.Matches(rawValue, @"([^""]+)("")");

            var quotedValuesList = valuesWithTrailingQuotes
                .Select(value => value.Value.Trim(new char[] { ' ', '"', '\\' }))
                .ToList();

            var lastQuotedValue = valuesWithTrailingQuotes.LastOrDefault();
            var lastDoubleQuotesIndex = lastQuotedValue != null ? lastQuotedValue.Index + lastQuotedValue.Length : 0;
            var trailingValues = new List<string>();

            if (lastDoubleQuotesIndex < rawValue.Length)
            {
                // Trailing values, not terminated with double quotes.
                trailingValues.AddRange(rawValue.Substring(lastDoubleQuotesIndex, rawValue.Length).Split(' '));
            }

            var merge = quotedValuesList.Concat(trailingValues).ToList();

            return merge.Count > 0
                ? merge
                : new List<string>(new[] { rawValue.Trim() });
        }

        internal static ArgumentSyntax Parse(string[] args)
        {
            string projectIdDefaultValue = null;
            string namespaceDefaultValue = null;
            string outputDirDefaultValue = null;
            string fileNameSuffixDefaultValue = null;
            string baseClassDefaultValue = null;

            var result = ArgumentSyntax.Parse(args, syntax =>
            {
                syntax.ErrorOnUnexpectedArguments = false;
                syntax.DefineOption("p|projectid", ref projectIdDefaultValue, "Kentico Cloud Project ID.");
                syntax.DefineOption("n|namespace", ref namespaceDefaultValue, "-n|--namespace");
                syntax.DefineOption("o|outputdir", ref outputDirDefaultValue, "Output directory for the generated files.");
                syntax.DefineOption("f|filenamesuffix", ref fileNameSuffixDefaultValue, "Optionally add a suffix to generated filenames (e.g., News.cs becomes News.Generated.cs).");
                syntax.DefineOption("g|generatepartials", ref CodeGeneratorOptions.DefaultGeneratePartials, "Generate partial classes for customization (if this option is set filename suffix will default to Generated).");
                syntax.DefineOption("t|withtypeprovider", ref CodeGeneratorOptions.DefaultWithTypeProvider, "Indicates whether the CustomTypeProvider class should be generated.");
                syntax.DefineOption("s|structuredmodel", ref CodeGeneratorOptions.DefaultStructuredModel, "Indicates whether the classes should be generated with types that represent structured data model.");
                syntax.DefineOption("c|contentmanagementapi", ref CodeGeneratorOptions.DefaultContentManagementApi, "Indicates whether the classes should be generated for CM API SDK instead.");
                syntax.DefineOption("b|baseclass", ref baseClassDefaultValue, "Optionally set the name of a base type that all generated classes derive from. If not set, they will not inherit any base class.");
                syntax.ApplicationName = "content-types-generator";
            });

            return result;
        }

        static int Execute(ArgumentSyntax argSyntax)
        {
            CodeGeneratorOptions options;

            try
            {
                options = CreateCodeGeneratorOptions(argSyntax);
            }
            catch (InvalidOperationException exception)
            {
                Console.Error.WriteLine(exception.Message);
                Console.WriteLine(argSyntax.GetHelpText());
                return 1;
            }

            var codeGeneratorOptions = Options.Create(options);

            var codeGenerator = new CodeGenerator(codeGeneratorOptions);
            codeGenerator.GenerateContentTypeModels(options.StructuredModel);

            if (!options.ContentManagementApi && options.WithTypeProvider)
            {
                codeGenerator.GenerateTypeProvider();
            }

            if (!string.IsNullOrEmpty(options.BaseClass))
            {
                codeGenerator.GenerateBaseClass();
            }

            return 0;
        }

        internal static CodeGeneratorOptions CreateCodeGeneratorOptions(ArgumentSyntax argSyntax)
        {
            var builder = new ConfigurationBuilder()
                            .SetBasePath(Environment.CurrentDirectory)
                            .AddJsonFile("appSettings.json", true)
                            .Add(new CommandLineOptionsProvider(argSyntax.GetOptions()));

            Configuration = builder.Build();

            CodeGeneratorOptions options = new CodeGeneratorOptions();

            // Load the options from the configuration sources
            new ConfigureFromConfigurationOptions<CodeGeneratorOptions>(Configuration).Configure(options);

            // No projectId was passed as an arg or set in the appSettings.config
            if (string.IsNullOrEmpty(options.ProjectId))
            {
                throw new InvalidOperationException("Provide a Project ID!");
            }

            /// Setting OutputDir default value here instead of in the <see cref="Parse"/> method as it would overwrite the JSON value.
            if (string.IsNullOrEmpty(options.OutputDir))
            {
                options.OutputDir = "./";
            }

            return options;
        }
    }
}
