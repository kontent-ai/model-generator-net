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
        private readonly string _fileNameSuffix;
        private readonly bool _generatePartials;

        public DeliveryClient Client { get; }

        public CodeGenerator(string projectId, string outputDir, string @namespace = null, string fileNameSuffix = null, bool generatePartials = false)
        {
            _projectId = projectId;
            _namespace = @namespace;
            _fileNameSuffix = fileNameSuffix;
            _generatePartials = generatePartials;

            if (_generatePartials && string.IsNullOrEmpty(_fileNameSuffix))
            {
                _fileNameSuffix = "Generated";
            }

            // Resolve relative path to full path
            _outputDir = Path.GetFullPath(outputDir).TrimEnd('\\') + "\\";

            // initialise DeliveryClient
            Client = new DeliveryClient(_projectId);
        }

        public void GenerateContentTypeModels(bool structuredModel = false)
        {
            // Make sure the output dir exists
            Directory.CreateDirectory(_outputDir);

            var classCodeGenerators = GetClassCodeGenerators(structuredModel);

            foreach (var codeGenerator in classCodeGenerators)
            {
                SaveToFile(codeGenerator.GenerateCode(), codeGenerator.ClassFilename);
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

        private void SaveToFile(string content, string fileName, bool overwriteExisting = true)
        {
            string outputPath = _outputDir + $"{fileName}.cs";
            bool fileExists = File.Exists(outputPath);
            if (!fileExists || overwriteExisting)
            {
                File.WriteAllText(outputPath, content);
            }
        }

        private IEnumerable<ClassCodeGenerator> GetClassCodeGenerators(bool structuredModel = false)
        {
            IEnumerable<ContentType> contentTypes = Task.Run(() => Client.GetTypesAsync()).Result.Types;

            var codeGenerators = new List<ClassCodeGenerator>();
            foreach (var contentType in contentTypes)
            {
                try
                {
                    if (_generatePartials)
                    {
                        codeGenerators.Add(GetCustomClassCodeGenerator(contentType));
                    }
                    codeGenerators.Add(GetClassCodeGenerator(contentType, structuredModel));
                }
                catch (InvalidIdentifierException)
                {
                    Console.WriteLine($"Warning: Skipping Content Type '{contentType.System.Codename}'. Can't create valid C# Identifier from its name.");
                }
            }

            return codeGenerators;
        }

        private ClassCodeGenerator GetClassCodeGenerator(ContentType contentType, bool structuredModel)
        {
            var classDefinition = new ClassDefinition(contentType.System.Codename);

            foreach (var element in contentType.Elements.Values)
            {
                try
                {
                    var elementType = element.Type;
                    if (structuredModel && Property.IsContentTypeSupported(elementType + Property.STRUCTURED_SUFFIX))
                    {
                        elementType += Property.STRUCTURED_SUFFIX;
                    }
                    var property = Property.FromContentType(element.Codename, elementType);
                    classDefinition.AddPropertyCodenameConstant(element);
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

            string suffix = string.IsNullOrEmpty(_fileNameSuffix) ? "" : $".{_fileNameSuffix}";
            string classFilename = $"{classDefinition.ClassName}{suffix}";
            return new ClassCodeGenerator(classDefinition, classFilename, _namespace);
        }

        private ClassCodeGenerator GetCustomClassCodeGenerator(ContentType contentType)
        {
            var classDefinition = new ClassDefinition(contentType.System.Codename);
            string classFilename = $"{classDefinition.ClassName}";
            return new ClassCodeGenerator(classDefinition, classFilename, _namespace, false);
        }
    }
}
