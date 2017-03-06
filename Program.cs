using System;
using Microsoft.Extensions.CommandLineUtils;

namespace KenticoCloudDotNetGenerators
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
            var outputDirOption = app.Option("-o|--outputdir", "Output directory where files will be generated.", CommandOptionType.SingleValue);

            app.OnExecute(() =>
            {
                if (!projectIdOption.HasValue())
                {
                    app.Error.WriteLine("Provide Project ID!");
                    app.ShowHelp();

                    return 1;
                }

                const string CURRENT_DIRECTORY = ".";
                string outputDir = outputDirOption.Value() ?? CURRENT_DIRECTORY;

                var codeGenerator = new CodeGenerator(projectIdOption.Value(), outputDir, namespaceOption.Value());
                codeGenerator.Generate();

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
