using System;
using System.IO;
using Kentico.Kontent.ModelGenerator.Core;
using Xunit;

namespace Kentico.Kontent.ModelGenerator.Tests
{
    public class ManagementClassCodeGeneratorTests
    {
        [Fact]
        public void Constructor_CreatesInstance()
        {
            var classDefinition = new ClassDefinition("Complete content type");

            var classCodeGenerator = new ManagementClassCodeGenerator(classDefinition, classDefinition.ClassName);

            Assert.NotNull(classCodeGenerator);
            Assert.False(classCodeGenerator.CustomPartial);
            Assert.True(classCodeGenerator.OverwriteExisting);
        }

        [Fact]
        public void Build_CreatesClassWithCompleteContentType_CMAPI()
        {
            var classDefinition = new ClassDefinition("Complete content type");
            classDefinition.AddProperty(Property.FromContentType("text", "text", true, "text_element_id"));
            classDefinition.AddProperty(Property.FromContentType("rich_text", "rich_text", true, "rich_text_element_id"));
            classDefinition.AddProperty(Property.FromContentType("number", "number", true, "number_element_id"));
            classDefinition.AddProperty(Property.FromContentType("multiple_choice", "multiple_choice", true, "multiple_choice_element_id"));
            classDefinition.AddProperty(Property.FromContentType("date_time", "date_time", true, "date_time_element_id"));
            classDefinition.AddProperty(Property.FromContentType("asset", "asset", true, "asset_element_id"));
            classDefinition.AddProperty(Property.FromContentType("modular_content", "modular_content", true, "linked_items_element_id"));
            classDefinition.AddProperty(Property.FromContentType("taxonomy", "taxonomy", true, "taxonomy_element_id"));
            classDefinition.AddProperty(Property.FromContentType("url_slug", "url_slug", true, "url_slug_element_id"));
            classDefinition.AddProperty(Property.FromContentType("custom", "custom", true, "custom_element_id"));

            var classCodeGenerator = new ManagementClassCodeGenerator(classDefinition, classDefinition.ClassName);

            var compiledCode = classCodeGenerator.GenerateCode();

            var executingPath = AppContext.BaseDirectory;
            var expectedCode = File.ReadAllText(executingPath + "/Assets/CompleteContentType_CompiledCode_CMAPI.txt");

            Assert.Equal(expectedCode, compiledCode, ignoreWhiteSpaceDifferences: true, ignoreLineEndingDifferences: true);
        }
    }
}
