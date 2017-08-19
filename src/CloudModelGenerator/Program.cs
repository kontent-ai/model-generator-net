using System;
using Microsoft.Extensions.CommandLineUtils;

namespace CloudModelGenerator
{
    class Program
    {
        static int Main(string[] args)
        {
            var app = new CommandLineApplication()
            {
                Name = "content-types-generator",
                Description = "Generates Kentico Cloud Content Types as CSharp classes.",
            };

            var projectIdOption = app.Option("-p|--projectid", "Kentico Cloud Project ID.", CommandOptionType.SingleValue);
            var namespaceOption = app.Option("-n|--namespace", "Namespace name of the generated classes.", CommandOptionType.SingleValue);
            var outputDirOption = app.Option("-o|--outputdir", "Output directory for the generated files.", CommandOptionType.SingleValue);
            var transformFilenamesOption = app.Option("-f|--transformfilenames", "Optionally add a suffix to generated filenames (e.g., News.cs becomes News.Generated.cs).", CommandOptionType.SingleValue);
            var includeTypeProvider = app.Option("-t|--withtypeprovider", "Indicates whether the CustomTypeProvider class should be generated.", CommandOptionType.NoValue);

            app.OnExecute(() =>
            {
                if (!projectIdOption.HasValue())
                {
                    app.Error.WriteLine("Provide a Project ID!");
                    app.ShowHelp();

                    return 1;
                }

                const string CURRENT_DIRECTORY = ".";
                string outputDir = outputDirOption.Value() ?? CURRENT_DIRECTORY;

                var codeGenerator = new CodeGenerator(projectIdOption.Value(), outputDir, namespaceOption.Value(), transformFilenamesOption.Value());
                codeGenerator.GenerateContentTypeModels();

                if (includeTypeProvider.HasValue())
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
