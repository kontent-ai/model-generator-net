using System;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;

namespace CloudModelGenerator
{
    class Program
    {
        static IConfigurationRoot configuration { get; set; }
        static int Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("appSettings.json");

            configuration = builder.Build();

            var app = new CommandLineApplication()
            {
                Name = "content-types-generator",
                Description = "Generates Kentico Cloud Content Types as CSharp classes.",
            };

            var projectIdOption = app.Option("-p|--projectid", "Kentico Cloud Project ID.", CommandOptionType.SingleValue);
            var namespaceOption = app.Option("-n|--namespace", "Namespace name of the generated classes.", CommandOptionType.SingleValue);
            var outputDirOption = app.Option("-o|--outputdir", "Output directory for the generated files.", CommandOptionType.SingleValue);
            var fileNameSuffixOption = app.Option("-sf|--filenamesuffix", "Optionally add a suffix to generated filenames (e.g., News.cs becomes News.Generated.cs).", CommandOptionType.SingleValue);
            var includeTypeProvider = app.Option("-t|--withtypeprovider", "Indicates whether the CustomTypeProvider class should be generated.", CommandOptionType.NoValue);
            var structuredModel = app.Option("-s|--structuredmodel", "Indicates whether the classes should be generated with types that represent structured data model.", CommandOptionType.NoValue);


            app.OnExecute(() =>
            {
                // Check if default values are set
                var passedSetProjectId = configuration["defaultFlags:0:projectId"] ?? projectIdOption.Value();
                var passedSetNamespace = configuration["defaultFlags:0:namespace"] ?? namespaceOption.Value();
                var passedSetOutputDir = configuration["defaultFlags:0:outputdir"] ?? outputDirOption.Value();
                var passedSetFileNameSuffix = configuration["defaultFlags:0:filenameSuffix"] ?? fileNameSuffixOption.Value();
                var passedSetIncludeTypeProvider = configuration["defaultFlags:0:withTypeProvider"] ?? includeTypeProvider.Value();
                var passedSetStructuredModel = configuration["defaultFlags:0:structuredModel"] ?? structuredModel.Value();

                // No projectId was passed as an arg or set in the appSettings.config
                if (!projectIdOption.HasValue() && passedSetProjectId.Equals(""))
                {
                    app.Error.WriteLine("Provide a Project ID!");
                    app.ShowHelp();

                    return 1;
                }

                const string CURRENT_DIRECTORY = ".";
                string outputDir = passedSetOutputDir.Equals("") ? CURRENT_DIRECTORY : passedSetOutputDir;

                var codeGenerator = new CodeGenerator(passedSetProjectId, outputDir, passedSetNamespace, 
                    passedSetFileNameSuffix);

                codeGenerator.GenerateContentTypeModels(bool.Parse(passedSetStructuredModel));

                if (bool.Parse(passedSetIncludeTypeProvider))
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
