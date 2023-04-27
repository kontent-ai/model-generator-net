using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Configuration;
using Kontent.Ai.ModelGenerator.Core.Generators.Class;
using Microsoft.Extensions.Options;
using Moq;

namespace Kontent.Ai.ModelGenerator.Core.Tests;

public class CodeGeneratorBaseTests
{
    private readonly Mock<IClassCodeGeneratorFactory> _classCodeGeneratorFactory;
    private readonly Mock<IOutputProvider> _outputProvider;
    private readonly Mock<IOptions<CodeGeneratorOptions>> _codeGeneratorOptions;

    public CodeGeneratorBaseTests()
    {
        _classCodeGeneratorFactory = new Mock<IClassCodeGeneratorFactory>();
        _outputProvider = new Mock<IOutputProvider>();
        _codeGeneratorOptions = new Mock<IOptions<CodeGeneratorOptions>>();

        _codeGeneratorOptions.Setup(x => x.Value).Returns(new CodeGeneratorOptions
        {
            DeliveryOptions = new DeliveryOptions { ProjectId = Guid.NewGuid().ToString() },
            Namespace = "CustomNamespace",
            ManagementApi = false,
            GeneratePartials = false,
            WithTypeProvider = false
        });
    }

    [Theory]
    [InlineData("ClassName")]
    [InlineData("")]
    [InlineData(null)]
    public void GetFileClassNameTesting_CustomFileNameSuffix_Returns(string className)
    {
        var codeGeneratorOptions = new Mock<IOptions<CodeGeneratorOptions>>();

        codeGeneratorOptions.Setup(x => x.Value).Returns(new CodeGeneratorOptions
        {
            DeliveryOptions = new DeliveryOptions { ProjectId = Guid.NewGuid().ToString() },
            Namespace = "CustomNamespace",
            FileNameSuffix = "CustomFileNameSuffix",
            ManagementApi = false,
            GeneratePartials = false,
            WithTypeProvider = false
        });

        var codeGeneratorBaseFake = new CodeGeneratorBaseFake(codeGeneratorOptions.Object, _outputProvider.Object, _classCodeGeneratorFactory.Object);

        var result = codeGeneratorBaseFake.GetFileClassNameTesting(className);

        result.Should().Be($"{className}.{codeGeneratorOptions.Object.Value.FileNameSuffix}");
    }

    [Theory]
    [InlineData("ClassName")]
    [InlineData("")]
    [InlineData(null)]
    public void GetFileClassNameTesting_DefaultFileNameSuffix_Returns(string className)
    {
        var codeGeneratorBaseFake = new CodeGeneratorBaseFake(_codeGeneratorOptions.Object, _outputProvider.Object, _classCodeGeneratorFactory.Object);

        var result = codeGeneratorBaseFake.GetFileClassNameTesting(className);

        result.Should().Be($"{className}.Generated");
    }

    [Theory]
    [InlineData("", true)]
    [InlineData("  ", false)]
    [InlineData(null, false)]
    public void WriteToOutputProviderTesting_ContentIsNullOrWhitespace_OutputProviderReceivedCallDidNotReceiveCall(string content, bool overwriteExisting)
    {
        var fileName = "fileName.cs";

        var codeGeneratorBaseFake = new CodeGeneratorBaseFake(_codeGeneratorOptions.Object, _outputProvider.Object, _classCodeGeneratorFactory.Object);

        codeGeneratorBaseFake.WriteToOutputProviderTesting(content, fileName, overwriteExisting);

        _outputProvider.Verify(x => x.Output(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void WriteToOutputProviderTesting_OutputProviderReceivedCall(bool overwriteExisting)
    {
        var content = "content";
        var fileName = "fileName.cs";

        var codeGeneratorBaseFake = new CodeGeneratorBaseFake(_codeGeneratorOptions.Object, _outputProvider.Object, _classCodeGeneratorFactory.Object);

        codeGeneratorBaseFake.WriteToOutputProviderTesting(content, fileName, overwriteExisting);

        _outputProvider.Verify(x => x.Output(content, fileName, overwriteExisting), Times.Once);
    }

    [Fact]
    public void GetCustomClassCodeGeneratorTesting_()
    {
        var contentTypeCodename = "ContentTypeCodename";

        var codeGeneratorBaseFake = new CodeGeneratorBaseFake(_codeGeneratorOptions.Object, _outputProvider.Object, _classCodeGeneratorFactory.Object);

        codeGeneratorBaseFake.GetCustomClassCodeGeneratorTesting(contentTypeCodename);

        _classCodeGeneratorFactory.Verify(x =>
            x.CreateClassCodeGenerator(_codeGeneratorOptions.Object.Value, It.IsAny<ClassDefinition>(), It.IsAny<string>(), true), Times.Once);
    }

    [Fact]
    public void WriteToOutputProviderTesting_EmptyList_OutputProviderDidNotReceiveCall()
    {
        var classCodeGenerators = new List<ClassCodeGenerator>();

        var codeGeneratorBaseFake = new CodeGeneratorBaseFake(_codeGeneratorOptions.Object, _outputProvider.Object, _classCodeGeneratorFactory.Object);

        codeGeneratorBaseFake.WriteToOutputProviderTesting(classCodeGenerators);

        _outputProvider.Verify(x => x.Output(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    [Theory]
    [MemberData(nameof(GetClassCodeGenerators))]
    public void WriteToOutputProviderTesting_WithClassCodeGenerators_OutputProviderReceivedCall(List<ClassCodeGenerator> classCodeGenerators)
    {
        var codeGeneratorBaseFake = new CodeGeneratorBaseFake(_codeGeneratorOptions.Object, _outputProvider.Object, _classCodeGeneratorFactory.Object);

        codeGeneratorBaseFake.WriteToOutputProviderTesting(classCodeGenerators);

        var classCodeGenerator = classCodeGenerators.First();
        _outputProvider.Verify(x => x.Output(It.IsAny<string>(), classCodeGenerator.ClassFilename, classCodeGenerator.OverwriteExisting), Times.Once());
    }

    public static IEnumerable<object[]> GetClassCodeGenerators()
    {
        yield return new object[] { new List<ClassCodeGenerator> { new DeliveryClassCodeGenerator(new ClassDefinition("DeliveryClassCodeGenerator"), "DeliveryClassCodeGenerator.cs", "DeliveryClassCodeGeneratorNamespace") } };
        yield return new object[] { new List<ClassCodeGenerator> { new ExtendedDeliveryClassCodeGenerator(new ClassDefinition("ExtendedDeliveryClassCodeGeneratorStructured"), "ExtendedDeliveryClassCodeGeneratorStructured.cs", true, "ExtendedDeliveryClassCodeGeneratorStructuredNamespace") } };
        yield return new object[] { new List<ClassCodeGenerator> { new ExtendedDeliveryClassCodeGenerator(new ClassDefinition("DeliveryClassCodeGeneratorNotStructured"), "DeliveryClassCodeGeneratorNotStructured.cs", false, "DeliveryClassCodeGeneratorNotStructuredNamespace") } };
        yield return new object[] { new List<ClassCodeGenerator> { new ManagementClassCodeGenerator(new ClassDefinition("ManagementClassCodeGenerator"), "ManagementClassCodeGenerator.cs", "ManagementClassCodeGeneratorNamespace") } };
        yield return new object[] { new List<ClassCodeGenerator> { new PartialClassCodeGenerator(new ClassDefinition("PartialClassCodeGenerator"), "PartialClassCodeGenerator.cs", "PartialClassCodeGeneratorNamespace") } };
        yield return new object[] { new List<ClassCodeGenerator> { new TypedExtendedDeliveryClassCodeGenerator(new ClassDefinition("TypedExtendedDeliveryClassCodeGenerator"), "TypedExtendedDeliveryClassCodeGenerator.cs", "TypedExtendedDeliveryClassCodeGeneratorNamespace") } };
    }

    private class CodeGeneratorBaseFake : CodeGeneratorBase
    {
        public CodeGeneratorBaseFake(IOptions<CodeGeneratorOptions> options, IOutputProvider outputProvider, IClassCodeGeneratorFactory classCodeGeneratorFactory)
            : base(options, outputProvider, classCodeGeneratorFactory)
        {
        }

        public string GetFileClassNameTesting(string className) => GetFileClassName(className);

        public ClassCodeGenerator GetCustomClassCodeGeneratorTesting(string contentTypeCodename) => GetCustomClassCodeGenerator(contentTypeCodename);

        public void WriteToOutputProviderTesting(string content, string fileName, bool overwriteExisting) => WriteToOutputProvider(content, fileName, overwriteExisting);

        public void WriteToOutputProviderTesting(ICollection<ClassCodeGenerator> classCodeGenerators) => WriteToOutputProvider(classCodeGenerators);

        protected override Task<ICollection<ClassCodeGenerator>> GetClassCodeGenerators() => throw new NotImplementedException();
    }
}
