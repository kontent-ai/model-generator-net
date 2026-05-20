using System;
using System.IO;
using Kontent.Ai.ModelGenerator.Core.Configuration;
using Kontent.Ai.ModelGenerator.Core.Contract;
using Microsoft.Extensions.Options;

namespace Kontent.Ai.ModelGenerator;

public class FileSystemOutputProvider(IOptions<CodeGeneratorOptions> options) : IOutputProvider
{
    private readonly IOptions<CodeGeneratorOptions> _options = options;

    internal string OutputDir
    {
        get
        {
            var outputDir = _options.Value.OutputDir;

            // Setting OutputDir default value here instead of in the <see cref="Parse"/> method as it would overwrite the JSON value.
            if (string.IsNullOrEmpty(outputDir))
            {
                outputDir = "./";
            }

            // Resolve relative path to full path
            outputDir = Path.GetFullPath(outputDir);

            return outputDir;
        }
    }

    public void Output(string content, string fileName, bool overwriteExisting)
    {
        // Make sure the output dir exists
        Directory.CreateDirectory(OutputDir);

        var outputPath = Path.GetFullPath(Path.Combine(OutputDir, $"{fileName}.cs"));

        // Guard against path traversal: fileName must be a bare class name, not a path.
        // The resolved file must stay directly inside OutputDir.
        if (Path.GetDirectoryName(outputPath) != OutputDir.TrimEnd(Path.DirectorySeparatorChar))
        {
            throw new ArgumentException(
                $"Invalid file name '{fileName}': the resolved path escapes the output directory.", nameof(fileName));
        }

        bool fileExists = File.Exists(outputPath);
        if (!fileExists || overwriteExisting)
        {
            File.WriteAllText(outputPath, content);
        }
    }
}
