using System;
using Xunit;

namespace Kentico.Kontent.ModelGenerator.Tests
{
    public class TextHelpersTests
    {
        [Fact]
        public void GetValidPascalCaseIdentifierName_ThrowsAnExceptionForNullValue()
        {
            Assert.Throws<ArgumentNullException>(() => TextHelpers.GetValidPascalCaseIdentifierName(null));
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("-")]
        [InlineData("$^123")]
        public void GetValidPascalCaseIdentifierName_ThrowsAnExceptionForInvalidInput(string name)
        {
            Assert.Throws<InvalidIdentifierException>(() => TextHelpers.GetValidPascalCaseIdentifierName(name));
        }

        [Theory]
        [InlineData("Simple name", "SimpleName")]
        [InlineData("Name with special chars & multiple    spaces.", "NameWithSpecialCharsMultipleSpaces")]
        [InlineData("EVERYTHING_IS_ -UPPERCASE", "EverythingIsUppercase")]
        [InlineData("date___time_field", "DateTimeField")]
        [InlineData("Multiline\r\nstring", "MultilineString")]
        [InlineData(" 1 2 3 Starts with space and numbers", "StartsWithSpaceAndNumbers")]
        [InlineData("ends with numbers 1 2 3", "EndsWithNumbers123")]
        public void GetValidPascalCaseIdentifierName(string name, string expected)
        {
            string result = TextHelpers.GetValidPascalCaseIdentifierName(name);
            Assert.Equal(expected, result);
        }
    }
}
