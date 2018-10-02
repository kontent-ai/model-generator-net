using System;
using System.Collections.Generic;
using System.CommandLine;
using Xunit;

namespace CloudModelGenerator.Tests
{
    public class ProgramTests
    {
        [Fact]
        public void CreateCodeGeneratorOptions_NoProjectId_Throws()
        {
            ArgumentSyntax preparedSyntax = Program.Parse(new string[] { });

            Assert.Throws<InvalidOperationException>(() => Program.CreateCodeGeneratorOptions(preparedSyntax));
        }

        [Fact]
        public void CreateCodeGeneratorOptions_NoOutputSetInJsonNorInParameters_OuputDirHasDefaultValue()
        {
            ArgumentSyntax preparedSyntax = Program.Parse(new string[]
            {
                "--projectid", "00000000-0000-0000-0000-000000000001"
            });

            var options = Program.CreateCodeGeneratorOptions(preparedSyntax);
            Assert.Equal("./", options.OutputDir);
        }

        public static IEnumerable<object[]> GetInputForCorrectArguments()
        {
            yield return new string[][]
            {
                new string[] { "--outputdir", @"C:\KC\demo\"" --namespace Some.NameSpace" },
                new string[] { "--outputdir", @"C:\KC\demo\", "--namespace", "Some.NameSpace" },
            };
            yield return new string[][]
            {
                new string[] { "--outputdir", @"C:\KC\demo\"" -n Some.NameSpace" },
                new string[] { "--outputdir", @"C:\KC\demo\", "-n", "Some.NameSpace" },
            };
            yield return new string[][]
            {
                new string[] { "--outputdir", @"C:\KC\demo\"" --generatepartials"  },
                new string[] { "--outputdir", @"C:\KC\demo\", "--generatepartials" }
            };
            yield return new string[][]
            {
                new string[] { "--outputdir", @"C:\KC\demo\"" --namespace Some.NameSpace -g" },
                new string[] { "--outputdir", @"C:\KC\demo\", "--namespace", "Some.NameSpace", "-g" },
            };
            yield return new string[][]
            {
                new string[] { "--outputdir", @"C:\KC\demo\"" --namespace Some.NameSpace --generatepartials" },
                new string[] { "--outputdir", @"C:\KC\demo\", "--namespace", "Some.NameSpace", "--generatepartials" },
            };
            yield return new string[][]
            {
                new string[] { "--outputdir", @"C:\KC\demo\"" --namespace Some.NameSpace --generatepartials -s --projectId 975bf280-fd91-488c-994c-2f04416e5ee3" },
                new string[] { "--outputdir", @"C:\KC\demo\", "--namespace", "Some.NameSpace", "--generatepartials", "-s", "--projectId", "975bf280-fd91-488c-994c-2f04416e5ee3" },
            };
        }

        [Theory]
        [MemberData(nameof(GetInputForCorrectArguments))]
        public void CorrectArguments_CorrectionPerformed(string[] inputArgs, string[] expectedArgs)
        {
            var correctedArgs = Program.CorrectArguments(inputArgs);
            Assert.Equal(expectedArgs, correctedArgs);
        }
    }
}
