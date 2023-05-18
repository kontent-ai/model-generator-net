﻿using System.IO;
using Kontent.Ai.ModelGenerator.Core.Configuration;
using Kontent.Ai.ModelGenerator.Core.Contract;
using Microsoft.Extensions.Options;

namespace Kontent.Ai.ModelGenerator;

public class FileSystemOutputProvider : IOutputProvider
{
    private readonly IOptions<CodeGeneratorOptions> _options;

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

    public FileSystemOutputProvider(IOptions<CodeGeneratorOptions> options)
    {
        _options = options;
    }

    public void Output(string content, string fileName, bool overwriteExisting)
    {
        // Make sure the output dir exists
        Directory.CreateDirectory(OutputDir);

        string outputPath = Path.Combine(OutputDir, $"{fileName}.cs");
        bool fileExists = File.Exists(outputPath);
        if (!fileExists || overwriteExisting)
        {
            File.WriteAllText(outputPath, content);
        }
    }
}
