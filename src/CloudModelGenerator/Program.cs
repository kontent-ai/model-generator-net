using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.CommandLine;
using System;
using System.Collections.Generic;

namespace CloudModelGenerator
{
    class Program
    {
        static IConfigurationRoot Configuration { get; set; }

        const string HelpOption = "help";

        static int Main(string[] args)
        {
            var syntax = Parse(args);
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

        internal static ArgumentSyntax Parse(string[] args)
        {
            string projectIdDefaultValue = null;
            string namespaceDefaultValue = null;
            string outputDirDefaultValue = null;
            string fileNameSuffixDefaultValue = null;

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
