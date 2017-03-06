using System;
using NUnit.Framework;

namespace CloudModelGenerator.Tests
{
    [TestFixture]
    public class TextHelpersTests
    {
        [TestCase]
        public void GetValidPascalCaseIdentifierName_ThrowsAnExceptionForNullValue()
        {
            Assert.Throws<ArgumentNullException>(() => TextHelpers.GetValidPascalCaseIdentifierName(null));
        }

        [TestCase("")]
        [TestCase("  ")]
        [TestCase("-")]
        [TestCase("$^123")]
        public void GetValidPascalCaseIdentifierName_ThrowsAnExceptionForInvalidInput(string name)
        {
            Assert.Throws<InvalidIdentifierException>(() => TextHelpers.GetValidPascalCaseIdentifierName(name));
        }

        [TestCase("Simple name", "SimpleName")]
        [TestCase("Name with special chars & multiple    spaces.", "NameWithSpecialCharsMultipleSpaces")]
        [TestCase("EVERYTHING_IS_ -UPPERCASE", "EverythingIsUppercase")]
        [TestCase("date___time_field", "DateTimeField")]
        [TestCase("Multiline\r\nstring", "MultilineString")]
        [TestCase(" 1 2 3 Starts with space and numbers", "StartsWithSpaceAndNumbers")]
        [TestCase("ends with numbers 1 2 3", "EndsWithNumbers123")]
        public void GetValidPascalCaseIdentifierName(string name, string expected)
        {
            string result = TextHelpers.GetValidPascalCaseIdentifierName(name);
            Assert.AreEqual(expected, result);
        }
    }
}
