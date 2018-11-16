using KenticoCloud.Delivery;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.CommandLine;

namespace CloudModelGenerator
{
    internal class Program
    {
        private static IConfigurationRoot Configuration { get; set; }

        private const string HelpOption = "help";

        private static int Main(string[] args)
        {
            var correctedArgs = ArgumentParser.CorrectArguments(args);
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

        private static int Execute(ArgumentSyntax argSyntax)
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

            // Setup DI
            IServiceProvider serviceProvider = new ServiceCollection()
                .AddDeliveryClient(options.DeliveryOptions)

                .BuildServiceProvider();



            var codeGenerator = new CodeGenerator(codeGeneratorOptions, serviceProvider.GetService<IDeliveryClient>());
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

            // Load Code Generator Options from the configuration sources
            new ConfigureFromConfigurationOptions<CodeGeneratorOptions>(Configuration).Configure(options);

            // Load Delivery Options from the configuration sources
            new ConfigureFromConfigurationOptions<DeliveryOptions>(Configuration.GetSection(nameof(DeliveryOptions))).Configure(options.DeliveryOptions);

            // No projectId was passed as an arg or set in the appSettings.config
            if (string.IsNullOrEmpty(options.DeliveryOptions.ProjectId))
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
