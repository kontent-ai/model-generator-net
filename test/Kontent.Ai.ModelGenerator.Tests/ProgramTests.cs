using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.Management.Configuration;
using Kontent.Ai.ModelGenerator.Core.Configuration;
using Xunit;

namespace Kontent.Ai.ModelGenerator.Tests
{
    public class ProgramTests
    {
        private static IEnumerable<string> GeneralOptionArgs => ToLower(new List<string>
        {
            nameof(CodeGeneratorOptions.Namespace),
            nameof(CodeGeneratorOptions.OutputDir),
            nameof(CodeGeneratorOptions.FileNameSuffix),
            nameof(CodeGeneratorOptions.GeneratePartials),
            nameof(CodeGeneratorOptions.BaseClass)
        });

        private static IDictionary<string, string> ExpectedManagementMappings => new Dictionary<string, string>
        {
            { "-n", nameof(CodeGeneratorOptions.Namespace) },
            { "-o", nameof(CodeGeneratorOptions.OutputDir) },
            { "-f", nameof(CodeGeneratorOptions.FileNameSuffix) },
            { "-g", nameof(CodeGeneratorOptions.GeneratePartials) },
            { "-b", nameof(CodeGeneratorOptions.BaseClass) },
            { "-p", $"{nameof(ManagementOptions)}:{nameof(ManagementOptions.ProjectId)}" },
            { "--projectid", $"{nameof(ManagementOptions)}:{nameof(ManagementOptions.ProjectId)}" },
            { "-m", nameof(CodeGeneratorOptions.ManagementApi) },
            { "-k", $"{nameof(ManagementOptions)}:{nameof(ManagementOptions.ApiKey)}" },
            { "--apikey", $"{nameof(ManagementOptions)}:{nameof(ManagementOptions.ApiKey)}" }
        };

        private static IDictionary<string, string> ExpectedDeliveryMappings => new Dictionary<string, string>
        {
            { "-n", nameof(CodeGeneratorOptions.Namespace) },
            { "-o", nameof(CodeGeneratorOptions.OutputDir) },
            { "-f", nameof(CodeGeneratorOptions.FileNameSuffix) },
            { "-g", nameof(CodeGeneratorOptions.GeneratePartials) },
            { "-s", nameof(CodeGeneratorOptions.StructuredModel) },
            { "-b", nameof(CodeGeneratorOptions.BaseClass) },
            { "-p", $"{nameof(DeliveryOptions)}:{nameof(DeliveryOptions.ProjectId)}" },
            { "--projectid", $"{nameof(DeliveryOptions)}:{nameof(DeliveryOptions.ProjectId)}" },
            { "-t", nameof(CodeGeneratorOptions.WithTypeProvider) }
        };

        [Fact]
        public async Task CreateCodeGeneratorOptions_NoProjectId_ReturnsError()
        {
            var result = await Program.Main(Array.Empty<string>());
            Assert.Equal(1, result);
        }

        [Fact]
        public void GetSwitchMappings_MissingMapiSwitch_ReturnsDeliveryMappings()
        {
            var mappings = Program.GetSwitchMappings(new string[]
            {
                "-p",
                Guid.NewGuid().ToString()
            });

            Assert.Equal(ExpectedDeliveryMappings, mappings);
        }

        [Fact]
        public void GetSwitchMappings_MapiSwitchIsFalse_ReturnsDeliveryMappings()
        {
            var mappings = Program.GetSwitchMappings(new string[]
            {
                "-p",
                Guid.NewGuid().ToString(),
                "-m",
                "false"
            });

            Assert.Equal(ExpectedDeliveryMappings, mappings);
        }

        [Fact]
        public void GetSwitchMappings_MapiSwitchIsTrue_ReturnsManagementMappings()
        {
            var mappings = Program.GetSwitchMappings(new string[]
            {
                "-p",
                Guid.NewGuid().ToString(),
                "-m",
                "true"
            });

            Assert.Equal(ExpectedManagementMappings, mappings);
        }

        [Fact]
        public void ContainsContainsUnsupportedArg_SupportedDeliveryOptions_ReturnsFalse()
        {
            var args = AppendValuesToArgs(ExpectedDeliveryMappings)
                .Concat(AppendValuesToArgs(ToLower(new List<string>
                {
                    nameof(CodeGeneratorOptions.StructuredModel),
                    nameof(CodeGeneratorOptions.WithTypeProvider)
                })))
                .Concat(AppendValuesToArgs(GeneralOptionArgs))
                .Concat(AppendValuesToArgs(typeof(DeliveryOptions)))
                .ToArray();

            var result = Program.ContainsContainsUnsupportedArg(args, ExpectedDeliveryMappings);

            Assert.False(result);
        }

        [Theory]
        [InlineData("-x")]
        [InlineData("--projectidX")]
        [InlineData("--DeliveryOptionsX:UseSecureAccess")]
        [InlineData("--DeliveryOptions:UseSecureAccessX")]
        public void ContainsContainsUnsupportedArg_UnsupportedDeliveryOptions_ReturnsTrue(string arg)
        {
            var args = new[]
            {
                arg,
                "arg_value"
            };
            var result = Program.ContainsContainsUnsupportedArg(args, ExpectedDeliveryMappings);

            Assert.True(result);
        }

        [Fact]
        public void ContainsContainsUnsupportedArg_SupportedManagementOptions_ReturnsFalse()
        {
            var args = AppendValuesToArgs(ExpectedManagementMappings)
                .Concat(AppendValuesToArgs(ToLower(new List<string>
                {
                    nameof(CodeGeneratorOptions.ManagementApi)
                })))
                .Concat(AppendValuesToArgs(GeneralOptionArgs))
                .Concat(AppendValuesToArgs(typeof(ManagementOptions)))
                .ToArray();

            var result = Program.ContainsContainsUnsupportedArg(args, ExpectedManagementMappings);

            Assert.False(result);
        }

        [Theory]
        [InlineData("-x")]
        [InlineData("--contentmanagementapi")]
        [InlineData("--managementapiX")]
        [InlineData("--ManagementOptions:ApiKeyX")]
        [InlineData("--ManagementOptionsX:ApiKey")]
        public void ContainsContainsUnsupportedArg_UnsupportedManagementOptions_ReturnsTrue(string arg)
        {
            var args = new[]
            {
                arg,
                "arg_value"
            };
            var result = Program.ContainsContainsUnsupportedArg(args, ExpectedManagementMappings);

            Assert.True(result);
        }

        private static IEnumerable<string> AppendValuesToArgs(IDictionary<string, string> mappings) => AppendValuesToArgs(mappings.Keys);

        private static IEnumerable<string> AppendValuesToArgs(Type type) =>
            AppendValuesToArgs(type.GetProperties().Select(p => $"{type.Name}:{p.Name}"));

        private static IEnumerable<string> ToLower(IEnumerable<string> args) => args.Select(a => a.ToLower());

        private static IEnumerable<string> AppendValuesToArgs(IEnumerable<string> args)
        {
            var argsWithValue = new List<string>();
            foreach (var arg in args)
            {
                argsWithValue.Add(arg.StartsWith('-') ? arg : $"--{arg}");
                argsWithValue.Add("arg_value");
            }
            return argsWithValue;
        }
    }
}
