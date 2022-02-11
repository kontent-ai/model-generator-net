using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kentico.Kontent.Delivery.Abstractions;
using Kentico.Kontent.Management;
using Kentico.Kontent.ModelGenerator.Core.Configuration;
using Xunit;

namespace Kentico.Kontent.ModelGenerator.Tests
{
    public class ProgramTests
    {
        private static IDictionary<string, string> ExpectedDeliveryMappings => new Dictionary<string, string>
        {
            { "-n", nameof(CodeGeneratorOptions.Namespace) },
            { "-o", nameof(CodeGeneratorOptions.OutputDir) },
            { "-f", nameof(CodeGeneratorOptions.FileNameSuffix) },
            { "-g", nameof(CodeGeneratorOptions.GeneratePartials) },
            { "-s", nameof(CodeGeneratorOptions.StructuredModel) },
            { "-b", nameof(CodeGeneratorOptions.BaseClass) },
            { "-p", $"{nameof(DeliveryOptions)}:{nameof(DeliveryOptions.ProjectId)}" },
            { "-t", nameof(CodeGeneratorOptions.WithTypeProvider) }
        };

        [Fact]
        public async Task CreateCodeGeneratorOptions_NoProjectId_ReturnsError()
        {
            var result = await Program.Main(Array.Empty<string>()).ConfigureAwait(false);
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
            var expectedMappings = new Dictionary<string, string>
            {
                { "-n", nameof(CodeGeneratorOptions.Namespace) },
                { "-o", nameof(CodeGeneratorOptions.OutputDir) },
                { "-f", nameof(CodeGeneratorOptions.FileNameSuffix) },
                { "-g", nameof(CodeGeneratorOptions.GeneratePartials) },
                { "-b", nameof(CodeGeneratorOptions.BaseClass) },
                { "-p", $"{nameof(ManagementOptions)}:{nameof(ManagementOptions.ProjectId)}" },
                { "-m", nameof(CodeGeneratorOptions.ManagementApi) },
                { "-k", $"{nameof(ManagementOptions)}:{nameof(ManagementOptions.ApiKey)}" }
            };

            var mappings = Program.GetSwitchMappings(new string[]
            {
                "-p",
                Guid.NewGuid().ToString(),
                "-m",
                "true"
            });

            Assert.Equal(expectedMappings, mappings);
        }
    }
}
