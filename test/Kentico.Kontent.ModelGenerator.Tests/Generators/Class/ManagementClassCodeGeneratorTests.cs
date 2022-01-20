using System;
using System.IO;
using Kentico.Kontent.ModelGenerator.Core.Common;
using Kentico.Kontent.ModelGenerator.Core.Generators.Class;
using Xunit;

namespace Kentico.Kontent.ModelGenerator.Tests.Generators.Class
{
    public class ManagementClassCodeGeneratorTests
    {
        [Fact]
        public void Constructor_CreatesInstance()
        {
            var classDefinition = new ClassDefinition("Complete content type");

            var classCodeGenerator = new ManagementClassCodeGenerator(classDefinition, classDefinition.ClassName);

            Assert.NotNull(classCodeGenerator);
            Assert.True(classCodeGenerator.OverwriteExisting);
        }

        [Fact]
        public void Build_CreatesClassWithCompleteContentType_ManagementApi()
        {
            var classDefinition = new ClassDefinition("Complete content type");
            classDefinition.AddProperty(Property.FromContentType("text", "text", true, "6712e528-8504-4a36-b716-a28327d6205f"));
            classDefinition.AddProperty(Property.FromContentType("rich_text", "rich_text", true, "014d2125-923d-4428-93b4-ad1590274912"));
            classDefinition.AddProperty(Property.FromContentType("number", "number", true, "9d23ff46-117c-432c-8fb2-3273acfbbbf5"));
            classDefinition.AddProperty(Property.FromContentType("multiple_choice", "multiple_choice", true, "2115b9ad-5df5-45b8-aa0f-490b5119afa6"));
            classDefinition.AddProperty(Property.FromContentType("date_time", "date_time", true, "66756a72-6af8-44a4-b58c-485425586a90"));
            classDefinition.AddProperty(Property.FromContentType("asset", "asset", true, "af569649-ee18-4d6a-a095-ea6ffa012546"));
            classDefinition.AddProperty(Property.FromContentType("modular_content", "modular_content", true, "4fa6bad6-d984-45e8-8ebb-f6be25626ee5"));
            classDefinition.AddProperty(Property.FromContentType("subpages", "subpages", true, "44924563-44d4-4272-a20f-b8745698b082"));
            classDefinition.AddProperty(Property.FromContentType("taxonomy", "taxonomy", true, "83011da2-559d-458c-a4b5-c81a001f4139"));
            classDefinition.AddProperty(Property.FromContentType("url_slug", "url_slug", true, "14390f27-213e-4f8d-9c31-cbf9a7c0a0d8"));
            classDefinition.AddProperty(Property.FromContentType("custom", "custom", true, "23154ba2-73fc-450c-99d4-c18ba45bb743"));

            var classCodeGenerator = new ManagementClassCodeGenerator(classDefinition, classDefinition.ClassName);

            var compiledCode = classCodeGenerator.GenerateCode();

            var executingPath = AppContext.BaseDirectory;
            var expectedCode = File.ReadAllText(executingPath + "/Assets/CompleteContentType_CompiledCode_ManagementApi.txt");

            Assert.Equal(expectedCode, compiledCode, ignoreWhiteSpaceDifferences: true, ignoreLineEndingDifferences: true);
        }
    }
}
