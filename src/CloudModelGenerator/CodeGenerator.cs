using KenticoCloud.Delivery;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CloudModelGenerator
{
    public class CodeGenerator
    {
        private readonly CodeGeneratorOptions _options;

        public DeliveryClient Client { get; }

        public CodeGenerator(IOptions<CodeGeneratorOptions> options)
        {
            _options = options.Value;

            if (_options.GeneratePartials && string.IsNullOrEmpty(_options.FileNameSuffix))
            {
                _options.FileNameSuffix = "Generated";
            }

            // Resolve relative path to full path
            _options.OutputDir = Path.GetFullPath(_options.OutputDir);

            // Initialize DeliveryClient
            Client = new DeliveryClient(_options.DeliveryOptions);
        }

        public void GenerateContentTypeModels(bool structuredModel = false)
        {
            // Make sure the output dir exists
            Directory.CreateDirectory(_options.OutputDir);

            var classCodeGenerators = GetClassCodeGenerators(structuredModel);

            if (classCodeGenerators.Count() > 0)
            {
                foreach (var codeGenerator in classCodeGenerators)
                {
                    SaveToFile(codeGenerator.GenerateCode(_options.ContentManagementApi), codeGenerator.ClassFilename, codeGenerator.OverwriteExisting);
                }

                Console.WriteLine($"{classCodeGenerators.Count()} content type models were successfully created.");
            }
            else
            {
                Console.WriteLine($@"No content type available for the project ({_options.DeliveryOptions.ProjectId}). Please make sure you have the Delivery API enabled at https://app.kenticocloud.com/.");
            }
        }

        public void GenerateTypeProvider()
        {
            // Make sure the output dir exists
            Directory.CreateDirectory(_options.OutputDir);

            var classCodeGenerators = GetClassCodeGenerators();

            if (classCodeGenerators.Count() > 0)
            {
                var typeProviderCodeGenerator = new TypeProviderCodeGenerator(_options.Namespace);

                foreach (var codeGenerator in classCodeGenerators)
                {
                    typeProviderCodeGenerator.AddContentType(codeGenerator.ClassDefinition.Codename, codeGenerator.ClassDefinition.ClassName);
                }

                var typeProviderCode = typeProviderCodeGenerator.GenerateCode();
                if (!string.IsNullOrEmpty(typeProviderCode))
                {
                    SaveToFile(typeProviderCode, TypeProviderCodeGenerator.CLASS_NAME);
                    Console.WriteLine($"{TypeProviderCodeGenerator.CLASS_NAME} class was successfully created.");
                }
            }
            else
            {
                Console.WriteLine($@"No content type available for the project ({_options.DeliveryOptions.ProjectId}). Please make sure you have the Delivery API enabled at https://app.kenticocloud.com/.");
            }
        }

        private void SaveToFile(string content, string fileName, bool overwriteExisting = true)
        {
            string outputPath = Path.Combine(_options.OutputDir, $"{fileName}.cs");
            bool fileExists = File.Exists(outputPath);
            if (!fileExists || overwriteExisting)
            {
                File.WriteAllText(outputPath, content);
            }
        }

        private IEnumerable<ClassCodeGenerator> GetClassCodeGenerators(bool structuredModel = false)
        {
            IEnumerable<ContentType> contentTypes = null;
            try
            {
                contentTypes = Task.Run(() => Client.GetTypesAsync()).Result.Types;
            }
            catch (AggregateException aex)
            {
                if ((aex.InnerExceptions.Count == 1) && aex.InnerException is DeliveryException)
                {
                    // Return friendlier message
                    Console.WriteLine(aex.InnerException.Message);
                }
            }
            var codeGenerators = new List<ClassCodeGenerator>();
            if (contentTypes != null)
            {
                foreach (var contentType in contentTypes)
                {
                    try
                    {
                        if (_options.GeneratePartials)
                        {
                            codeGenerators.Add(GetCustomClassCodeGenerator(contentType));
                        }
                        codeGenerators.Add(GetClassCodeGenerator(contentType, structuredModel));
                    }
                    catch (InvalidIdentifierException)
                    {
                        Console.WriteLine($"Warning: Skipping Content Type '{contentType.System.Codename}'. Can't create valid C# identifier from its name.");
                    }
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
                    if (structuredModel && Property.IsContentTypeSupported(elementType + Property.STRUCTURED_SUFFIX, _options.ContentManagementApi))
                    {
                        elementType += Property.STRUCTURED_SUFFIX;
                    }
                    var property = Property.FromContentType(element.Codename, elementType, _options.ContentManagementApi);
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

            if (!_options.ContentManagementApi)
            {
                try
                {
                    classDefinition.AddSystemProperty();
                }
                catch (InvalidOperationException)
                {
                    Console.WriteLine($"Warning: Can't add 'System' property. It's in collision with existing element in Content Type '{classDefinition.ClassName}'.");
                }
            }

            string suffix = string.IsNullOrEmpty(_options.FileNameSuffix) ? "" : $".{_options.FileNameSuffix}";
            string classFilename = $"{classDefinition.ClassName}{suffix}";

            return new ClassCodeGenerator(classDefinition, classFilename, _options.Namespace);
        }

        private ClassCodeGenerator GetCustomClassCodeGenerator(ContentType contentType)
        {
            var classDefinition = new ClassDefinition(contentType.System.Codename);
            string classFilename = $"{classDefinition.ClassName}";

            return new ClassCodeGenerator(classDefinition, classFilename, _options.Namespace, true);
        }

        public void GenerateBaseClass()
        {
            // Make sure the output dir exists
            Directory.CreateDirectory(_options.OutputDir);

            var classCodeGenerators = GetClassCodeGenerators();

            if (classCodeGenerators.Count() > 0)
            {
                var baseClassCodeGenerator = new BaseClassCodeGenerator(_options.BaseClass, _options.Namespace);

                foreach (var codeGenerator in classCodeGenerators)
                {
                    baseClassCodeGenerator.AddClassNameToExtend(codeGenerator.ClassDefinition.ClassName);
                }

                var baseClassCode = baseClassCodeGenerator.GenerateBaseClassCode();
                if (!string.IsNullOrEmpty(baseClassCode))
                {
                    SaveToFile(baseClassCode, _options.BaseClass);
                    Console.WriteLine($"{_options.BaseClass} class was successfully created.");
                }

                var baseClassExtenderCode = baseClassCodeGenerator.GenereateExtenderCode();
                if (!string.IsNullOrEmpty(baseClassExtenderCode))
                {
                    SaveToFile(baseClassExtenderCode, baseClassCodeGenerator.ExtenderClassName);
                    Console.WriteLine($"{baseClassCodeGenerator.ExtenderClassName} class was successfully created.");
                }
            }
            else
            {
                Console.WriteLine($@"No content type available for the project ({_options.DeliveryOptions.ProjectId}). Please make sure you have the Delivery API enabled at https://app.kenticocloud.com/.");
            }
        }
    }
}
