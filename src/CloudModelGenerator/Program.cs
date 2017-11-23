using System;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace CloudModelGenerator
{
    class Program
    {
        static IConfigurationRoot Configuration { get; set; }

        static int Main(string[] args)
        {
            var app = new CommandLineApplication
            {
                Name = "content-types-generator",
                Description = "Generates Kentico Cloud Content Types as CSharp classes.",
            };

            app.Option("-p|--projectid", "Kentico Cloud Project ID.", CommandOptionType.SingleValue);
            app.Option("-n|--namespace", "Namespace name of the generated classes.", CommandOptionType.SingleValue);
            app.Option("-o|--outputdir", "Output directory for the generated files.", CommandOptionType.SingleValue);
            app.Option("-sf|--filenamesuffix", "Optionally add a suffix to generated filenames (e.g., News.cs becomes News.Generated.cs).", CommandOptionType.SingleValue);
            app.Option("-gp|--generatepartials", "Generate partial classes for customization (if this option is set filename suffix will default to Generated).", CommandOptionType.NoValue);
            app.Option("-t|--withtypeprovider", "Indicates whether the CustomTypeProvider class should be generated.", CommandOptionType.NoValue);
            app.Option("-s|--structuredmodel", "Indicates whether the classes should be generated with types that represent structured data model.", CommandOptionType.NoValue);

            app.OnExecute(() =>
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Environment.CurrentDirectory)
                    .AddJsonFile("appSettings.json", true)
                    .Add(new CommandLineOptionsProvider(app.Options));

                Configuration = builder.Build();

                CodeGeneratorOptions options = new CodeGeneratorOptions();

                // Load the options from the configuration sources
                new ConfigureFromConfigurationOptions<CodeGeneratorOptions>(Configuration).Configure(options);

                // No projectId was passed as an arg or set in the appSettings.config
                if (string.IsNullOrEmpty(options.ProjectId))
                {
                    app.Error.WriteLine("Provide a Project ID!");
                    app.ShowHelp();

                    return 1;
                }

                var codeGenerator = new CodeGenerator(Options.Create(options));

                codeGenerator.GenerateContentTypeModels(options.StructuredModel);

                if (options.WithTypeProvider)
                {
                    codeGenerator.GenerateTypeProvider();
                }

                return 0;
            });

            app.HelpOption("-? | -h | --help");

            try
            {
                return app.Execute(args);
            }
            catch (CommandParsingException e)
            {
                Console.WriteLine("Invalid arguments!");
                Console.WriteLine(e.Message);
                app.ShowHelp();
                return 1;
            }
        }
    }
}
