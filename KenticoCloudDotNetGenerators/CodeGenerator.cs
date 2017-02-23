using KenticoCloud.Delivery;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace KenticoCloudDotNetGenerators
{
    public class CodeGenerator
    {
        private Options _options;

        public CodeGenerator(Options options)
        {
            _options = options;
        }

        public void Run()
        {
            if (!IsProjectId(_options.ProjectId))
            {
                Console.WriteLine("Invalid Project ID!");
                return;
            }

            // Reslove relative path to full path
            _options.OutputDir = Path.GetFullPath(_options.OutputDir).TrimEnd('\\') + "\\";

            Console.WriteLine("Starting class generation...");
            Console.WriteLine("ProjectId: {0}", _options.ProjectId);
            Console.WriteLine("OutputDir: {0}", _options.OutputDir);
            Console.WriteLine("Namespace: {0}", _options.Namespace);

            GenerateClassesToOutputDirectory();
        }

        private static bool IsProjectId(string projectId)
        {
            Guid guid;
            return Guid.TryParse(projectId, out guid);
        }

        private void GenerateClassesToOutputDirectory()
        {
            // Make sure the output dir exists
            Directory.CreateDirectory(_options.OutputDir);

            var classCodeGenerators = GetClassCodeGenerators();

            foreach (var codeGenerator in classCodeGenerators)
            {
                string outputPath = _options.OutputDir + codeGenerator.ClassDefinition.ClassName + ".cs";
                File.WriteAllText(outputPath, codeGenerator.GenerateCode());
            }

            Console.WriteLine($"{classCodeGenerators.Count()} files was successfully created.");
        }

        private IEnumerable<ClassCodeGenerator> GetClassCodeGenerators()
        {
            var client = new DeliveryClient(_options.ProjectId);

            IEnumerable<ContentType> contentTypes = Task.Run(() => client.GetTypesAsync()).Result.Types;

            var codeGenerators = new List<ClassCodeGenerator>();
            foreach (var contentType in contentTypes)
            {
                try
                {
                    codeGenerators.Add(GetClassCodeGenerator(contentType));
                }
                catch (InvalidIdentifierException e)
                {
                    Console.WriteLine($"Warning: Skipping Content Type '{contentType.System.Codename}'. Can't create valid C# Identifier from its name.");
                }
            }

            return codeGenerators;
        }

        private ClassCodeGenerator GetClassCodeGenerator(ContentType contentType)
        {
            var classDefinition = new ClassDefinition(contentType.System.Codename);

            foreach (var element in contentType.Elements.Values)
            {
                if (element.Type == "url_slug")
                {
                    // TODO: not sure what to do with url slug.
                    continue;
                }

                try
                {
                    var property = Property.FromContentType(element.Codename, element.Type);
                    classDefinition.AddProperty(property);
                }
                catch (InvalidOperationException e)
                {
                    Console.WriteLine($"Warning: Element '{element.Codename}' is already present in Content Type '{classDefinition.ClassName}'.");
                }
                catch (InvalidIdentifierException e)
                {
                    Console.WriteLine($"Warning: Can't create valid C# Identifier from '{element.Codename}'. Skipping element.");
                }
                catch (ArgumentException)
                {
                    Console.WriteLine($"Warning: Skipping unknown Content Element type '{element.Type}'. (Content Type: '{classDefinition.ClassName}', Element Codename: '{element.Codename}').");
                }
            }

            try
            {
                classDefinition.AddSystemProperty();
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine($"Warning: Can't add 'System' property. It's in collision with existing element in Content Type '{classDefinition.ClassName}'.");
            }

            return new ClassCodeGenerator(classDefinition, _options.Namespace);
        }
    }
}
