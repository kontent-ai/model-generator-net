using System;
using System.Collections.Generic;
using System.Reflection;
using Kentico.Kontent.ModelGenerator.Core.Common;
using Kentico.Kontent.ModelGenerator.Core.Generators.Class;
using Xunit;

namespace Kentico.Kontent.ModelGenerator.Tests
{
    public class ClassCodeGeneratorTests
    {
        [Theory]
        [MemberData(nameof(GetTypes))]
        public void Constructor_ClassDefinitionIsNull_Throws(Type type)
        {
            var exception = Assert.Throws<TargetInvocationException>(() => Activator.CreateInstance(type, ConstructorParams()));

            Assert.NotNull(exception.InnerException);
            Assert.Equal(typeof(ArgumentNullException), exception.InnerException.GetType());
            Assert.Contains("classDefinition", exception.InnerException.Message);
        }

        [Theory]
        [MemberData(nameof(GetTypesWithEmptyStringParam))]
        public void Constructor_ClassFileNameIsNullOrEmptyOrWhiteSpace_Returns_ClassDefinitionCodename(Type type, string classFilename)
        {
            var classDefinitionCodename = "classdefinitioncodename";

            var expectedClassFilename = "Classdefinitioncodename";

            var classCodeGenerator = (ClassCodeGenerator)Activator.CreateInstance(type, ConstructorParams(classDefinitionCodename, classFilename));

            Assert.NotNull(classCodeGenerator);
            Assert.Equal(expectedClassFilename, classCodeGenerator.ClassFilename);
        }

        [Theory]
        [MemberData(nameof(GetTypes))]
        public void Constructor_CustomClassFileName_Returns_CustomClassFileName(Type type)
        {
            var classDefinitionCodename = "codename";
            var classFilename = "CustomClassFileName";

            var expectedClassFilename = "CustomClassFileName";

            var classCodeGenerator = (ClassCodeGenerator)Activator.CreateInstance(type, ConstructorParams(classDefinitionCodename, classFilename));

            Assert.NotNull(classCodeGenerator);
            Assert.Equal(expectedClassFilename, classCodeGenerator.ClassFilename);
        }

        [Theory]
        [MemberData(nameof(GetTypesWithEmptyStringParam))]
        public void Constructor_NamespaceIsNullOrEmptyOrWhiteSpace_Returns_DefaultNamespace(Type type, string @namespace)
        {
            var classDefinitionCodename = "classdefinitioncodename";

            var classCodeGenerator = (ClassCodeGenerator)Activator.CreateInstance(type, ConstructorParams(classDefinitionCodename, null, @namespace));

            Assert.NotNull(classCodeGenerator);
            Assert.Equal(ClassCodeGenerator.DefaultNamespace, classCodeGenerator.Namespace);
        }

        [Theory]
        [MemberData(nameof(GetTypes))]
        public void Constructor_CustomNamespace_Returns_CustomNamespace(Type type)
        {
            var classDefinitionCodename = "classdefinitioncodename";
            var customNamespace = "CustomNamespace";

            var classCodeGenerator = (ClassCodeGenerator)Activator.CreateInstance(type, ConstructorParams(classDefinitionCodename, null, customNamespace));

            Assert.NotNull(classCodeGenerator);
            Assert.Equal(customNamespace, classCodeGenerator.Namespace);
        }

        public static IEnumerable<object[]> GetTypes()
        {
            yield return new object[] { typeof(PartialClassCodeGenerator) };
            yield return new object[] { typeof(ManagementClassCodeGenerator) };
            yield return new object[] { typeof(DeliveryClassCodeGenerator) };
        }

        public static IEnumerable<object[]> GetTypesWithEmptyStringParam()
        {
            yield return new object[] { typeof(PartialClassCodeGenerator), "" };
            yield return new object[] { typeof(ManagementClassCodeGenerator), "  " };
            yield return new object[] { typeof(DeliveryClassCodeGenerator), null };
        }

        private static object[] ConstructorParams(string classDefinitionCodename = null, string classFileName = null, string @namespace = null)
            => new object[] { GetClassDefinition(classDefinitionCodename), classFileName, @namespace };

        private static ClassDefinition GetClassDefinition(string classDefinitionCodename) => classDefinitionCodename == null
            ? (ClassDefinition)null
            : new ClassDefinition(classDefinitionCodename);
    }
}
