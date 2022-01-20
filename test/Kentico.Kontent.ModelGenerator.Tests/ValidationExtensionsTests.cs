using System;
using Kentico.Kontent.Delivery.Abstractions;
using Kentico.Kontent.Management;
using Kentico.Kontent.ModelGenerator.Core.Configuration;
using Xunit;

namespace Kentico.Kontent.ModelGenerator.Tests
{
    public class ValidationExtensionsTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Validate_Success(bool managementApi)
        {
            var projectId = Guid.NewGuid().ToString();
            var codeGeneratorOptions = new CodeGeneratorOptions
            {
                ManagementApi = managementApi,
                DeliveryOptions = new DeliveryOptions
                {
                    ProjectId = projectId
                },
                ManagementOptions = new ManagementOptions
                {
                    ProjectId = projectId,
                    ApiKey = "apiKey"
                }
            };

            codeGeneratorOptions.Validate();
        }

        [Fact]
        public void Validate_DeliveryOptionsIsNull_ThrowsException()
        {
            var codeGeneratorOptions = new CodeGeneratorOptions
            {
                DeliveryOptions = null
            };

            var exception = Assert.Throws<Exception>(() => codeGeneratorOptions.Validate());
            Assert.Equal("You have to provide at least the 'ProjectId' argument. See http://bit.ly/k-params for more details on configuration.", exception.Message);
        }

        [Fact]
        public void Validate_DeliveryOptionsProjectIdIsNull_ThrowsException()
        {
            var codeGeneratorOptions = new CodeGeneratorOptions
            {
                DeliveryOptions = new DeliveryOptions
                {
                    ProjectId = null
                }
            };

            Assert.Throws<ArgumentNullException>(() => codeGeneratorOptions.Validate());
        }

        [Fact]
        public void Validate_ManagementOptionsIsNull_ThrowsException()
        {
            var projectId = Guid.NewGuid().ToString();
            var codeGeneratorOptions = new CodeGeneratorOptions
            {
                ManagementApi = true,
                DeliveryOptions = new DeliveryOptions
                {
                    ProjectId = projectId
                },
                ManagementOptions = null
            };

            var exception = Assert.Throws<Exception>(() => codeGeneratorOptions.Validate());
            Assert.Equal("You have to provide the 'ProjectId' to generate type for Content Management SDK. See http://bit.ly/k-params for more details on configuration.", exception.Message);
        }

        [Fact]
        public void Validate_ManagementOptionsProjectIdIsNull_ThrowsException()
        {
            var projectId = Guid.NewGuid().ToString();
            var codeGeneratorOptions = new CodeGeneratorOptions
            {
                ManagementApi = true,
                DeliveryOptions = new DeliveryOptions
                {
                    ProjectId = projectId
                },
                ManagementOptions = new ManagementOptions
                {
                    ProjectId = null,
                    ApiKey = "apiKey"
                }
            };

            var exception = Assert.Throws<Exception>(() => codeGeneratorOptions.Validate());
            Assert.Equal("You have to provide the 'ProjectId' to generate type for Content Management SDK. See http://bit.ly/k-params for more details on configuration.", exception.Message);
        }

        [Fact]
        public void Validate_ProjectIdsAreNotEqual_ThrowsException()
        {
            var codeGeneratorOptions = new CodeGeneratorOptions
            {
                ManagementApi = true,
                DeliveryOptions = new DeliveryOptions
                {
                    ProjectId = Guid.NewGuid().ToString()
                },
                ManagementOptions = new ManagementOptions
                {
                    ProjectId = Guid.NewGuid().ToString(),
                    ApiKey = "apiKey"
                }
            };

            var exception = Assert.Throws<Exception>(() => codeGeneratorOptions.Validate());
            Assert.Equal("You have to provide same 'ManagementOptions.ProjectId' as 'DeliveryOptions.ProjectId'", exception.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void Validate_ApiKeyIsNullOrWhiteSpace_ThrowsException(string apiKey)
        {
            var projectId = Guid.NewGuid().ToString();
            var codeGeneratorOptions = new CodeGeneratorOptions
            {
                ManagementApi = true,
                DeliveryOptions = new DeliveryOptions
                {
                    ProjectId = projectId
                },
                ManagementOptions = new ManagementOptions
                {
                    ProjectId = projectId,
                    ApiKey = apiKey
                }
            };

            var exception = Assert.Throws<Exception>(() => codeGeneratorOptions.Validate());
            Assert.Equal("You have to provide the 'ApiKey' to generate type for Content Management SDK. See http://bit.ly/k-params for more details on configuration.", exception.Message);
        }
    }
}
