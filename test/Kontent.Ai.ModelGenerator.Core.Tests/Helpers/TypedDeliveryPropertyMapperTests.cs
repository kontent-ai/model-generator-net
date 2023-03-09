using FluentAssertions;
using Kontent.Ai.Management.Models.Types;
using Kontent.Ai.Management.Models.Types.Elements;
using Kontent.Ai.ModelGenerator.Core.Configuration;
using Kontent.Ai.ModelGenerator.Core.Helpers;
using Kontent.Ai.ModelGenerator.Core.Tests.TestHelpers;

namespace Kontent.Ai.ModelGenerator.Core.Tests.Helpers;

public class TypedDeliveryPropertyMapperTests
{
    private static readonly CodeGeneratorOptions ExtendedDeliveryModelsOptions = new CodeGeneratorOptions
    {
        ExtendedDeliveryModels = true
    };

    [Fact]
    public void TryMap_ElementIsNull_Throws()
    {
        var contentTypes = new List<ContentTypeModel>
        {
            new ContentTypeModel()
        };

        var tryMapCall = () => TypedDeliveryPropertyMapper.TryMap(null, contentTypes, ExtendedDeliveryModelsOptions, out _);

        tryMapCall.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void TryMap_NotLinkedItemsElementOrSubpages_Throws()
    {
        var contentTypes = new List<ContentTypeModel>
        {
            new ContentTypeModel()
        };
        var element = new AssetElementMetadataModel();

        var tryMapCall = () => TypedDeliveryPropertyMapper.TryMap(element, contentTypes, ExtendedDeliveryModelsOptions, out _);

        tryMapCall.Should().ThrowExactly<ArgumentException>();
    }

    [Theory]
    [MemberData(nameof(GetBasicAllowedElements))]
    public void TryMap_ContentTypesIsNull_Throws(ElementMetadataBase element)
    {
        var tryMapCall = () => TypedDeliveryPropertyMapper.TryMap(element, null, ExtendedDeliveryModelsOptions, out _);

        tryMapCall.Should().ThrowExactly<ArgumentNullException>();
    }

    [Theory]
    [MemberData(nameof(GetBasicAllowedElements))]
    public void TryMap_ContentTypesIsEmpty_Throws(ElementMetadataBase element)
    {
        var contentTypes = new List<ContentTypeModel>();

        var tryMapCall = () => TypedDeliveryPropertyMapper.TryMap(element, contentTypes, ExtendedDeliveryModelsOptions, out _);

        tryMapCall.Should().ThrowExactly<ArgumentException>();
    }

    [Theory]
    [MemberData(nameof(GetBasicAllowedElements))]
    public void TryMap_OptionsIsNull_Throws(ElementMetadataBase element)
    {
        var contentTypes = new List<ContentTypeModel>
        {
            new ContentTypeModel()
        };

        var tryMapCall = () => TypedDeliveryPropertyMapper.TryMap(element, contentTypes, null, out _);

        tryMapCall.Should().ThrowExactly<ArgumentNullException>();
    }

    [Theory]
    [MemberData(nameof(GetBasicAllowedElements))]
    public void TryMap_GeneralExtendedDeliveryModelsIsFalse_Throws(ElementMetadataBase element)
    {
        var contentTypes = new List<ContentTypeModel>
        {
            new ContentTypeModel()
        };
        var options = new CodeGeneratorOptions
        {
            ExtendedDeliveryModels = false
        };

        var tryMapCall = () => TypedDeliveryPropertyMapper.TryMap(element, contentTypes, options, out _);

        tryMapCall.Should().ThrowExactly<ArgumentException>();
    }

    [Theory]
    [MemberData(nameof(GetNonMatchingAllowedTypesElements))]
    public void TryMap_Live_CouldNotFindAllowedType_Throws(ElementMetadataBase element)
    {

        var allContentTypes = new List<ContentTypeModel>()
        {
            new ContentTypeModel
            {
                Codename = "grinder",
                Id = Guid.NewGuid()
            },
            new ContentTypeModel
            {
                Codename = "coffee",
                Id = Guid.NewGuid()
            }
        };

        var tryMapCall = () => TypedDeliveryPropertyMapper.TryMap(element, allContentTypes, ExtendedDeliveryModelsOptions, out _);

        tryMapCall.Should().ThrowExactly<ArgumentException>();
    }

    [Theory]
    [MemberData(nameof(GetSingleAllowedTypeMultiItems))]
    public void TryMap_Live_SingleAllowedTypeMultiItems_Returns(List<ContentTypeModel> linkedContentTypeModels, ElementMetadataBase element)
    {
        var allContentTypes = new List<ContentTypeModel>()
        {
            new ContentTypeModel
            {
                Codename = "grinder",
                Id = Guid.NewGuid()
            },
            new ContentTypeModel
            {
                Codename = "coffee",
                Id = Guid.NewGuid()
            }
        }.Union(linkedContentTypeModels).ToList();

        var result = TypedDeliveryPropertyMapper.TryMap(element, allContentTypes, ExtendedDeliveryModelsOptions, out var typedProperty);

        result.Should().BeTrue();
        typedProperty.Codename.Should().Be("Just_Articles_Article");
        typedProperty.Id.Should().BeNull();
        typedProperty.Identifier.Should().Be("JustArticlesArticle");
        typedProperty.TypeName.Should().Be("IEnumerable<Article>");
    }

    [Theory]
    [MemberData(nameof(GetSingleAllowedTypeExactlySingleItem))]
    public void TryMap_Live_SingleAllowedTypeExactlySingleItem_Returns(List<ContentTypeModel> linkedContentTypeModels, ElementMetadataBase element)
    {
        var allContentTypes = new List<ContentTypeModel>()
        {
            new ContentTypeModel
            {
                Codename = "grinder",
                Id = Guid.NewGuid()
            },
            new ContentTypeModel
            {
                Codename = "coffee",
                Id = Guid.NewGuid()
            }
        }.Union(linkedContentTypeModels).ToList();

        var result = TypedDeliveryPropertyMapper.TryMap(element, allContentTypes, ExtendedDeliveryModelsOptions, out var typedProperty);

        result.Should().BeTrue();
        typedProperty.Codename.Should().Be("article");
        typedProperty.Id.Should().BeNull();
        typedProperty.Identifier.Should().Be("Article");
        typedProperty.TypeName.Should().Be("Article");
    }

    [Theory]
    [MemberData(nameof(GetSingleAllowedTypeAtMostSingleItem))]
    public void TryMap_Live_SingleAllowedTypeAtMostSingleItem_Returns(List<ContentTypeModel> linkedContentTypeModels, ElementMetadataBase element)
    {
        var allContentTypes = new List<ContentTypeModel>()
        {
            new ContentTypeModel
            {
                Codename = "grinder",
                Id = Guid.NewGuid()
            },
            new ContentTypeModel
            {
                Codename = "coffee",
                Id = Guid.NewGuid()
            }
        }.Union(linkedContentTypeModels).ToList();

        var result = TypedDeliveryPropertyMapper.TryMap(element, allContentTypes, ExtendedDeliveryModelsOptions, out var typedProperty);

        result.Should().BeTrue();
        typedProperty.Codename.Should().Be("article");
        typedProperty.Id.Should().BeNull();
        typedProperty.Identifier.Should().Be("Article");
        typedProperty.TypeName.Should().Be("Article");
    }

    [Theory]
    [MemberData(nameof(GetMultiAllowedTypesSingleItem))]
    public void TryMap_Live_MultiAllowedTypesSingleItem_Returns(List<ContentTypeModel> linkedContentTypeModels, ElementMetadataBase element)
    {
        var allContentTypes = new List<ContentTypeModel>()
        {
            new ContentTypeModel
            {
                Codename = "grinder",
                Id = Guid.NewGuid()
            },
            new ContentTypeModel
            {
                Codename = "coffee",
                Id = Guid.NewGuid()
            }
        }.Union(linkedContentTypeModels).ToList();

        var result = TypedDeliveryPropertyMapper.TryMap(element, allContentTypes, ExtendedDeliveryModelsOptions, out var typedProperty);

        result.Should().BeFalse();
        typedProperty.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(GetMultiAllowedTypesMultiItems))]
    public void TryMap_Live_MultiAllowedTypesMultiItems_Returns(List<ContentTypeModel> linkedContentTypeModels, ElementMetadataBase element)
    {
        var allContentTypes = new List<ContentTypeModel>()
        {
            new ContentTypeModel
            {
                Codename = "grinder",
                Id = Guid.NewGuid()
            },
            new ContentTypeModel
            {
                Codename = "coffee",
                Id = Guid.NewGuid()
            }
        }.Union(linkedContentTypeModels).ToList();

        var result = TypedDeliveryPropertyMapper.TryMap(element, allContentTypes, ExtendedDeliveryModelsOptions, out var typedProperty);

        result.Should().BeFalse();
        typedProperty.Should().BeNull();
    }

    public static IEnumerable<object[]> GetBasicAllowedElements()
    {
        yield return new object[] { new LinkedItemsElementMetadataModel() };
        yield return new object[] { new SubpagesElementMetadataModel() };
    }

    public static IEnumerable<object[]> GetNonMatchingAllowedTypesElements()
    {
        var limitModel = new LimitModel
        {
            Condition = LimitType.AtLeast,
            Value = 1
        };

        var linkedContentTypeModels = new List<ContentTypeModel>()
        {
            new ContentTypeModel
            {
                Codename = "article",
                Id = Guid.NewGuid()
            }
        };

        yield return new object[]
        {
            TestDataGenerator.GenerateLinkedItemsElement(
                Guid.NewGuid().ToString(),
                "articles",
                limitModel,
                linkedContentTypeModels.Select(ct => ct.Id))
        };
        yield return new object[]
        {
            TestDataGenerator.GenerateSubpagesElement(
                Guid.NewGuid().ToString(),
                "articles",
                limitModel,
                linkedContentTypeModels.Select(ct => ct.Id))
        };
    }

    public static IEnumerable<object[]> GetSingleAllowedTypeMultiItems()
    {
        var limitModel = new LimitModel
        {
            Condition = LimitType.AtLeast,
            Value = 1
        };

        var linkedContentTypeModels = new List<ContentTypeModel>()
        {
            new ContentTypeModel
            {
                Codename = "article",
                Id = Guid.NewGuid()
            }
        };

        yield return new object[]
        {
            linkedContentTypeModels,
            TestDataGenerator.GenerateLinkedItemsElement(
                Guid.NewGuid().ToString(),
                "just_articles",
                limitModel,
                linkedContentTypeModels.Select(ct => ct.Id))
        };
        yield return new object[]
        {
            linkedContentTypeModels,
            TestDataGenerator.GenerateSubpagesElement(
                Guid.NewGuid().ToString(),
                "just_articles",
                limitModel,
                linkedContentTypeModels.Select(ct => ct.Id))
        };
        yield return new object[]
        {
            linkedContentTypeModels,
            TestDataGenerator.GenerateSubpagesElement(
                Guid.NewGuid().ToString(),
                "just_articles",
                null,
                linkedContentTypeModels.Select(ct => ct.Id))
        };
    }

    public static IEnumerable<object[]> GetSingleAllowedTypeExactlySingleItem()
    {
        var limitModel = new LimitModel
        {
            Condition = LimitType.Exactly,
            Value = 1
        };

        var linkedContentTypeModels = new List<ContentTypeModel>()
        {
            new ContentTypeModel
            {
                Codename = "article",
                Id = Guid.NewGuid()
            }
        };

        yield return new object[]
        {
            linkedContentTypeModels,
            TestDataGenerator.GenerateLinkedItemsElement(
                Guid.NewGuid().ToString(),
                "article",
                limitModel,
                linkedContentTypeModels.Select(ct => ct.Id))
        };
        yield return new object[]
        {
            linkedContentTypeModels,
            TestDataGenerator.GenerateSubpagesElement(
                Guid.NewGuid().ToString(),
                "article",
                limitModel,
                linkedContentTypeModels.Select(ct => ct.Id))
        };
    }

    public static IEnumerable<object[]> GetSingleAllowedTypeAtMostSingleItem()
    {
        var limitModel = new LimitModel
        {
            Condition = LimitType.AtMost,
            Value = 1
        };

        var linkedContentTypeModels = new List<ContentTypeModel>()
        {
            new ContentTypeModel
            {
                Codename = "article",
                Id = Guid.NewGuid()
            }
        };

        yield return new object[]
        {
            linkedContentTypeModels,
            TestDataGenerator.GenerateLinkedItemsElement(
                Guid.NewGuid().ToString(),
                "article",
                limitModel,
                linkedContentTypeModels.Select(ct => ct.Id))
        };
        yield return new object[]
        {
            linkedContentTypeModels,
            TestDataGenerator.GenerateSubpagesElement(
                Guid.NewGuid().ToString(),
                "article",
                limitModel,
                linkedContentTypeModels.Select(ct => ct.Id))
        };
    }

    public static IEnumerable<object[]> GetMultiAllowedTypesSingleItem()
    {
        var limitModel = new LimitModel
        {
            Condition = LimitType.Exactly,
            Value = 1
        };

        var linkedContentTypeModels = new List<ContentTypeModel>()
        {
            new ContentTypeModel
            {
                Codename = "article",
                Id = Guid.NewGuid()
            },
            new ContentTypeModel
            {
                Codename = "blog",
                Id = Guid.NewGuid()
            }
        };

        yield return new object[]
        {
            linkedContentTypeModels,
            TestDataGenerator.GenerateLinkedItemsElement(
                Guid.NewGuid().ToString(),
                "article",
                limitModel,
                linkedContentTypeModels.Select(ct => ct.Id))
        };
        yield return new object[]
        {
            linkedContentTypeModels,
            TestDataGenerator.GenerateSubpagesElement(
                Guid.NewGuid().ToString(),
                "article",
                limitModel,
                linkedContentTypeModels.Select(ct => ct.Id))
        };
    }

    public static IEnumerable<object[]> GetMultiAllowedTypesMultiItems()
    {
        var limitModel = new LimitModel
        {
            Condition = LimitType.AtLeast,
            Value = 1
        };

        var linkedContentTypeModels = new List<ContentTypeModel>()
        {
            new ContentTypeModel
            {
                Codename = "article",
                Id = Guid.NewGuid()
            },
            new ContentTypeModel
            {
                Codename = "blog",
                Id = Guid.NewGuid()
            }
        };

        yield return new object[]
        {
            linkedContentTypeModels,
            TestDataGenerator.GenerateLinkedItemsElement(
                Guid.NewGuid().ToString(),
                "articles",
                limitModel,
                linkedContentTypeModels.Select(ct => ct.Id))
        };
        yield return new object[]
        {
            linkedContentTypeModels,
            TestDataGenerator.GenerateSubpagesElement(
                Guid.NewGuid().ToString(),
                "articles",
                limitModel,
                linkedContentTypeModels.Select(ct => ct.Id))
        };
    }
}
