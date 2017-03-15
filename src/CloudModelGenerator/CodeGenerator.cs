using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KenticoCloud.Delivery;

namespace CloudModelGenerator
{
    public class CodeGenerator
    {
        private readonly string _projectId;
        private readonly string _namespace;
        private readonly string _outputDir;

        public CodeGenerator(string projectId, string outputDir, string @namespace = null)
        {
            _projectId = projectId;
            _namespace = @namespace;

            // Reslove relative path to full path
            _outputDir = Path.GetFullPath(outputDir).TrimEnd('\\') + "\\";
        }

        public void GenerateContentTypeModels()
        {
            // Make sure the output dir exists
            Directory.CreateDirectory(_outputDir);

            var classCodeGenerators = GetClassCodeGenerators();
            
            foreach (var codeGenerator in classCodeGenerators)
            {
                SaveToFile(codeGenerator.GenerateCode(), codeGenerator.ClassDefinition.ClassName);
            }

            Console.WriteLine($"{classCodeGenerators.Count()} content type models were successfully created.");
        }
        
        public void GenerateTypeProvider()
        {
            // Make sure the output dir exists
            Directory.CreateDirectory(_outputDir);

            var classCodeGenerators = GetClassCodeGenerators();
            var typeProviderCodeGenerator = new TypeProviderCodeGenerator(_namespace);

            foreach (var codeGenerator in classCodeGenerators)
            {
                typeProviderCodeGenerator.AddContentType(codeGenerator.ClassDefinition.Codename, codeGenerator.ClassDefinition.ClassName);
            }
            
            SaveToFile(typeProviderCodeGenerator.GenerateCode(), TypeProviderCodeGenerator.CLASS_NAME);

            Console.WriteLine($"{TypeProviderCodeGenerator.CLASS_NAME} class was successfully created.");
        }

        private void SaveToFile(string content, string fileName)
        {
            string outputPath = _outputDir + $"{fileName}.cs";
            File.WriteAllText(outputPath, content);
        }

        private IEnumerable<ClassCodeGenerator> GetClassCodeGenerators()
        {
            var client = new DeliveryClient(_projectId);

            IEnumerable<ContentType> contentTypes = Task.Run(() => client.GetTypesAsync()).Result.Types;

            var codeGenerators = new List<ClassCodeGenerator>();
            foreach (var contentType in contentTypes)
            {
                try
                {
                    codeGenerators.Add(GetClassCodeGenerator(contentType));
                }
                catch (InvalidIdentifierException)
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
                try
                {
                    var property = Property.FromContentType(element.Codename, element.Type);
                    classDefinition.AddProperty(property);
                }
                catch (InvalidOperationException)
                {
                    Console.WriteLine($"Warning: Element '{element.Codename}' is already present in Content Type '{classDefinition.ClassName}'.");
                }
                catch (InvalidIdentifierException)
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
            catch (InvalidOperationException)
            {
                Console.WriteLine($"Warning: Can't add 'System' property. It's in collision with existing element in Content Type '{classDefinition.ClassName}'.");
            }

            return new ClassCodeGenerator(classDefinition, _namespace);
        }
    }
}
