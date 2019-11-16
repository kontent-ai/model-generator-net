using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Text;
using System.Threading.Tasks;

namespace Kentico.Kontent.ModelGenerator
{
    internal class Program
    {
        public static async Task<int> Main(string[] args)
        {
            try
            {
                // Parse command line input
                var syntax = ParseCommandLine(ArgumentParser.CorrectArguments(args));

                // Create an instance of a DI container
                var services = new ServiceCollection();

                // Build a configuration object from given sources
                var configuration = new ConfigurationBuilder()
                            .SetBasePath(Environment.CurrentDirectory)
                            .AddJsonFile("appSettings.json", true)
                            .Add(new CommandLineOptionsProvider(syntax))
                            .Build();

                // Fill the DI container
                services.Configure<CodeGeneratorOptions>(configuration);
                services.AddDeliveryClient(configuration);
                services.AddTransient<CodeGenerator>();

                // Build the DI container
                var serviceProvider = services.BuildServiceProvider();

                // Validate configuration of the Delivery Client
                serviceProvider.GetService<IOptions<CodeGeneratorOptions>>().Value.DeliveryOptions.Validate();                

                // Code generator entry point
                return await serviceProvider.GetService<CodeGenerator>().RunAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }
        }

        internal static ArgumentSyntax ParseCommandLine(string[] args)
        {
            string projectIdDefaultValue = null;
            string namespaceDefaultValue = null;
            string outputDirDefaultValue = null;
            string fileNameSuffixDefaultValue = null;
            string baseClassDefaultValue = null;

            var syntax = ArgumentSyntax.Parse(args, s =>
            {
                s.ErrorOnUnexpectedArguments = false;
                s.DefineOption("p|projectid", ref projectIdDefaultValue, "Kentico Kontent Project ID.");
                s.DefineOption("n|namespace", ref namespaceDefaultValue, "-n|--namespace");
                s.DefineOption("o|outputdir", ref outputDirDefaultValue, "Output directory for the generated files.");
                s.DefineOption("f|filenamesuffix", ref fileNameSuffixDefaultValue, "Optionally add a suffix to generated filenames (e.g., News.cs becomes News.Generated.cs).");
                s.DefineOption("g|generatepartials", ref CodeGeneratorOptions.DefaultGeneratePartials, "Generate partial classes for customization (if this option is set filename suffix will default to Generated).");
                s.DefineOption("t|withtypeprovider", ref CodeGeneratorOptions.DefaultWithTypeProvider, "Indicates whether the CustomTypeProvider class should be generated.");
                s.DefineOption("s|structuredmodel", ref CodeGeneratorOptions.DefaultStructuredModel, "Indicates whether the classes should be generated with types that represent structured data model.");
                s.DefineOption("c|contentmanagementapi", ref CodeGeneratorOptions.DefaultContentManagementApi, "Indicates whether the classes should be generated for CM API SDK instead.");
                s.DefineOption("b|baseclass", ref baseClassDefaultValue, "Optionally set the name of a base type that all generated classes derive from. If not set, they will not inherit any base class.");
                s.ApplicationName = "content-types-generator";
            });

            var unexpectedArgs = new List<string>(syntax.RemainingArguments);

            if (unexpectedArgs.Count > 0)
            {
                StringBuilder err = new StringBuilder();
                err.AppendLine("Invalid arguments!");
                foreach (var unexpectedArgument in unexpectedArgs)
                {
                    err.AppendLine($"Unrecognized option '{unexpectedArgument}'");
                }
                err.AppendLine(syntax.GetHelpText());

                throw new Exception(err.ToString());
            }

            return syntax;
        }
    }
}
