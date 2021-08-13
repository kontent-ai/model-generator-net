﻿using System;
using FluentAssertions;
using JetBrains.Annotations;
using Kentico.Kontent.ModelGenerator.Core;
using Kentico.Kontent.ModelGenerator.Core.Configuration;
using Xunit;

namespace Kentico.Kontent.ModelGenerator.Tests
{
    public class ClassCodeGeneratorFactoryTests
    {
        [Fact]
        public void CreateClassCodeGenerator_CodeGeneratorOptionsIsNull_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                ClassCodeGeneratorFactory.CreateClassCodeGenerator(null, new ClassDefinition("codename"), "classFileName"));
        }

        [Fact]
        public void CreateClassCodeGenerator_ClassDefinitionIsNull_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                ClassCodeGeneratorFactory.CreateClassCodeGenerator(new CodeGeneratorOptions(), null, "classFileName"));
        }

        [Fact]
        public void CreateClassCodeGenerator_ClassFilenameIsNull_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                ClassCodeGeneratorFactory.CreateClassCodeGenerator(new CodeGeneratorOptions(), new ClassDefinition("codename"), null));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void CreateClassCodeGenerator_DeliveryClassCodeGenerator_WithCustomPartialProperty_Returns(bool customPartial)
        {
            var classDefinitionCodename = "codename";
            var classFileName = "classFileName";
            var codeGeneratorOptions = new CodeGeneratorOptions
            {
                ContentManagementApi = false
            };

            var result = ClassCodeGeneratorFactory.CreateClassCodeGenerator(codeGeneratorOptions, new ClassDefinition(classDefinitionCodename), classFileName, customPartial);

            AssertClassCodeGenerator<DeliveryClassCodeGenerator>(result, classDefinitionCodename, classFileName, ClassCodeGeneratorBase.DEFAULT_NAMESPACE, customPartial);
        }

        [Fact]
        public void CreateClassCodeGenerator_DeliveryClassCodeGenerator_NoCustomPartialProperty_Returns()
        {
            var classDefinitionCodename = "codename";
            var classFileName = "classFileName";
            var codeGeneratorOptions = new CodeGeneratorOptions
            {
                ContentManagementApi = false
            };

            var result = ClassCodeGeneratorFactory.CreateClassCodeGenerator(codeGeneratorOptions, new ClassDefinition(classDefinitionCodename), classFileName);

            AssertClassCodeGenerator<DeliveryClassCodeGenerator>(result, classDefinitionCodename, classFileName, ClassCodeGeneratorBase.DEFAULT_NAMESPACE);
        }

        [Fact]
        public void CreateClassCodeGenerator_DeliveryClassCodeGenerator_CustomNamespace_Returns()
        {
            var classDefinitionCodename = "codename";
            var classFileName = "classFileName";
            var customNamespace = "CustomNameSpace";
            var codeGeneratorOptions = new CodeGeneratorOptions
            {
                ContentManagementApi = false,
                Namespace = customNamespace
            };

            var result = ClassCodeGeneratorFactory.CreateClassCodeGenerator(codeGeneratorOptions, new ClassDefinition(classDefinitionCodename), classFileName);

            AssertClassCodeGenerator<DeliveryClassCodeGenerator>(result, classDefinitionCodename, classFileName, customNamespace);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void CreateClassCodeGenerator_ManagementClassCodeGenerator_WithCustomPartialProperty_Returns(bool customPartial)
        {
            var classDefinitionCodename = "codename";
            var classFileName = "classFileName";
            var expectedCustomPartial = false;
            var codeGeneratorOptions = new CodeGeneratorOptions
            {
                ContentManagementApi = true
            };

            var result = ClassCodeGeneratorFactory.CreateClassCodeGenerator(codeGeneratorOptions, new ClassDefinition(classDefinitionCodename), classFileName, customPartial);

            AssertClassCodeGenerator<ManagementClassCodeGenerator>(result, classDefinitionCodename, classFileName, ClassCodeGeneratorBase.DEFAULT_NAMESPACE, expectedCustomPartial);
        }

        [Fact]
        public void CreateClassCodeGenerator_ManagementClassCodeGenerator_NoCustomPartialProperty_Returns()
        {
            var classDefinitionCodename = "codename";
            var classFileName = "classFileName";
            var codeGeneratorOptions = new CodeGeneratorOptions
            {
                ContentManagementApi = true
            };

            var result = ClassCodeGeneratorFactory.CreateClassCodeGenerator(codeGeneratorOptions, new ClassDefinition(classDefinitionCodename), classFileName);

            AssertClassCodeGenerator<ManagementClassCodeGenerator>(result, classDefinitionCodename, classFileName, ClassCodeGeneratorBase.DEFAULT_NAMESPACE);
        }

        [Fact]
        public void CreateClassCodeGenerator_ManagementClassCodeGenerator_CustomNamespace_Returns()
        {
            var classDefinitionCodename = "codename";
            var classFileName = "classFileName";
            var customNamespace = "CustomNameSpace";
            var codeGeneratorOptions = new CodeGeneratorOptions
            {
                ContentManagementApi = true,
                Namespace = customNamespace
            };

            var result = ClassCodeGeneratorFactory.CreateClassCodeGenerator(codeGeneratorOptions, new ClassDefinition(classDefinitionCodename), classFileName);

            AssertClassCodeGenerator<ManagementClassCodeGenerator>(result, classDefinitionCodename, classFileName, customNamespace);
        }

        [AssertionMethod]
        private static void AssertClassCodeGenerator<T>(ClassCodeGeneratorBase result, string classDefinitionCodename, string classFileName, string @namespace, bool customPartial = false)
        {
            result.Should().BeOfType<T>();
            result.ClassDefinition.Codename.Should().Be(classDefinitionCodename);
            result.ClassFilename.Should().Be(classFileName);
            result.CustomPartial.Should().Be(customPartial);
            result.Namespace.Should().Be(@namespace);
        }
    }
}
