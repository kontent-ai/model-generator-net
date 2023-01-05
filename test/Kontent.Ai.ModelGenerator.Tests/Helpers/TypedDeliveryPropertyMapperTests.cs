using Kontent.Ai.ModelGenerator.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Kontent.Ai.Management.Models.Types;
using Kontent.Ai.Management.Models.Types.Elements;
using Kontent.Ai.ModelGenerator.Core.Configuration;
using Xunit;
using Kontent.Ai.ModelGenerator.Core.Generators.Class;
using Kontent.Ai.ModelGenerator.Tests.TestHelpers;

namespace Kontent.Ai.ModelGenerator.Tests.Helpers;

public class TypedDeliveryPropertyMapperTests
{
    private static readonly CodeGeneratorOptions ExtendedDeliverModelsOptions = new CodeGeneratorOptions
    {
        ExtendedDeliverModels = true,
        ExtendedDeliverPreviewModels = false
    };

    private static readonly CodeGeneratorOptions ExtendedDeliverPreviewModelsOptions = new CodeGeneratorOptions
    {
        ExtendedDeliverModels = false,
        ExtendedDeliverPreviewModels = true
    };

    [Fact]
    public void TryMap_LinkedItemsElementIsNull_Throws()
    {
        var contentTypes = new List<ContentTypeModel>
        {
            new ContentTypeModel()
        };
        var options = new CodeGeneratorOptions
        {
            ExtendedDeliverPreviewModels = false,
            ExtendedDeliverModels = true
        };

        var tryMapCall = () => TypedDeliveryPropertyMapper.TryMap(null, contentTypes, options, out _);

        tryMapCall.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void TryMap_NotLinkedItemsElement_Throws()
    {
        var contentTypes = new List<ContentTypeModel>
        {
            new ContentTypeModel()
        };
        var element = new AssetElementMetadataModel();
        var options = new CodeGeneratorOptions
        {
            ExtendedDeliverPreviewModels = false,
            ExtendedDeliverModels = true
        };

        var tryMapCall = () => TypedDeliveryPropertyMapper.TryMap(element, contentTypes, options, out _);

        tryMapCall.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void TryMap_ContentTypesIsNull_Throws()
    {
        var element = new LinkedItemsElementMetadataModel();
        var options = new CodeGeneratorOptions
        {
            ExtendedDeliverPreviewModels = false,
            ExtendedDeliverModels = true
        };

        var tryMapCall = () => TypedDeliveryPropertyMapper.TryMap(element, null, options, out _);

        tryMapCall.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void TryMap_ContentTypesIsEmpty_Throws()
    {
        var contentTypes = new List<ContentTypeModel>();
        var element = new LinkedItemsElementMetadataModel();
        var options = new CodeGeneratorOptions
        {
            ExtendedDeliverPreviewModels = false,
            ExtendedDeliverModels = true
        };

        var tryMapCall = () => TypedDeliveryPropertyMapper.TryMap(element, contentTypes, options, out _);

        tryMapCall.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void TryMap_OptionsIsNull_Throws()
    {
        var contentTypes = new List<ContentTypeModel>
        {
            new ContentTypeModel()
        };
        var element = new LinkedItemsElementMetadataModel();

        var tryMapCall = () => TypedDeliveryPropertyMapper.TryMap(element, contentTypes, null, out _);

        tryMapCall.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void TryMap_GeneralExtendedDeliveryModelsIsFalse_Throws()
    {
        var contentTypes = new List<ContentTypeModel>
        {
            new ContentTypeModel()
        };
        var element = new LinkedItemsElementMetadataModel();
        var options = new CodeGeneratorOptions
        {
            ExtendedDeliverPreviewModels = false,
            ExtendedDeliverModels = false
        };

        var tryMapCall = () => TypedDeliveryPropertyMapper.TryMap(element, contentTypes, options, out _);

        tryMapCall.Should().ThrowExactly<ArgumentException>();
    }

    #region Live models

    [Fact]
    public void TryMap_Live_CouldNotFindAllowedType_Throws()
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

        var element = TestDataGenerator.GenerateLinkedItemsElement(
            Guid.NewGuid().ToString(),
            "articles",
            limitModel,
            linkedContentTypeModels.Select(ct => ct.Id));

        var tryMapCall = () => TypedDeliveryPropertyMapper.TryMap(element, allContentTypes, ExtendedDeliverModelsOptions, out _);

        tryMapCall.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void TryMap_Live_SingleAllowedTypeMultiItems_Returns()
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

        var element = TestDataGenerator.GenerateLinkedItemsElement(
            Guid.NewGuid().ToString(),
            "articles",
            limitModel,
            linkedContentTypeModels.Select(ct => ct.Id));

        var result = TypedDeliveryPropertyMapper.TryMap(element, allContentTypes, ExtendedDeliverModelsOptions, out var typedProperty);

        result.Should().BeTrue();
        typedProperty.Codename.Should().Be("Articles_Article");
        typedProperty.Id.Should().BeNull();
        typedProperty.Identifier.Should().Be("ArticlesArticle");
        typedProperty.TypeName.Should().Be("IEnumerable<Article>");
    }

    [Fact]
    public void TryMap_Live_SingleAllowedTypeExactlySingleItem_Returns()
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

        var element = TestDataGenerator.GenerateLinkedItemsElement(
            Guid.NewGuid().ToString(),
            "article",
            limitModel,
            linkedContentTypeModels.Select(ct => ct.Id));

        var result = TypedDeliveryPropertyMapper.TryMap(element, allContentTypes, ExtendedDeliverModelsOptions, out var typedProperty);

        result.Should().BeTrue();
        typedProperty.Codename.Should().Be("article");
        typedProperty.Id.Should().BeNull();
        typedProperty.Identifier.Should().Be("Article");
        typedProperty.TypeName.Should().Be("Article");
    }

    [Fact]
    public void TryMap_Live_MultiAllowedTypesSingleItem_Returns()
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

        var element = TestDataGenerator.GenerateLinkedItemsElement(
            Guid.NewGuid().ToString(),
            "article",
            limitModel,
            linkedContentTypeModels.Select(ct => ct.Id));

        var result = TypedDeliveryPropertyMapper.TryMap(element, allContentTypes, ExtendedDeliverModelsOptions, out var typedProperty);

        result.Should().BeFalse();
        typedProperty.Should().BeNull();
    }

    [Fact]
    public void TryMap_Live_MultiAllowedTypesMultiItems_Returns()
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

        var element = TestDataGenerator.GenerateLinkedItemsElement(
            Guid.NewGuid().ToString(),
            "articles",
            limitModel,
            linkedContentTypeModels.Select(ct => ct.Id));

        var result = TypedDeliveryPropertyMapper.TryMap(element, allContentTypes, ExtendedDeliverModelsOptions, out var typedProperty);

        result.Should().BeFalse();
        typedProperty.Should().BeNull();
    }

    #endregion

    #region Preview models

    [Fact]
    public void TryMap_Preview_CouldNotFindAllowedType_Throws()
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

        var element = TestDataGenerator.GenerateLinkedItemsElement(
            Guid.NewGuid().ToString(),
            "articles",
            limitModel,
            linkedContentTypeModels.Select(ct => ct.Id));

        var tryMapCall = () => TypedDeliveryPropertyMapper.TryMap(element, allContentTypes, ExtendedDeliverPreviewModelsOptions, out _);

        tryMapCall.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void TryMap_Preview_SingleAllowedTypeMultiItems_Returns()
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

        var element = TestDataGenerator.GenerateLinkedItemsElement(
            Guid.NewGuid().ToString(),
            "articles",
            limitModel,
            linkedContentTypeModels.Select(ct => ct.Id));

        var result = TypedDeliveryPropertyMapper.TryMap(element, allContentTypes, ExtendedDeliverPreviewModelsOptions, out var typedProperty);

        result.Should().BeTrue();
        typedProperty.Codename.Should().Be("Articles_Article");
        typedProperty.Id.Should().BeNull();
        typedProperty.Identifier.Should().Be("ArticlesArticle");
        typedProperty.TypeName.Should().Be($"IEnumerable<{ContentItemClassCodeGenerator.DefaultContentItemClassName}>");
    }

    [Fact]
    public void TryMap_Preview_SingleAllowedTypeExactlySingleItem_Returns()
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

        var element = TestDataGenerator.GenerateLinkedItemsElement(
            Guid.NewGuid().ToString(),
            "article",
            limitModel,
            linkedContentTypeModels.Select(ct => ct.Id));

        var result = TypedDeliveryPropertyMapper.TryMap(element, allContentTypes, ExtendedDeliverPreviewModelsOptions, out var typedProperty);

        result.Should().BeTrue();
        typedProperty.Codename.Should().Be("article");
        typedProperty.Id.Should().BeNull();
        typedProperty.Identifier.Should().Be("Article");
        typedProperty.TypeName.Should().Be("IEnumerable<IContentItem>");
    }

    [Fact]
    public void TryMap_Preview_MultiAllowedTypesSingleItem_Returns()
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

        var element = TestDataGenerator.GenerateLinkedItemsElement(
            Guid.NewGuid().ToString(),
            "article",
            limitModel,
            linkedContentTypeModels.Select(ct => ct.Id));

        var result = TypedDeliveryPropertyMapper.TryMap(element, allContentTypes, ExtendedDeliverPreviewModelsOptions, out var typedProperty);

        result.Should().BeFalse();
        typedProperty.Should().BeNull();
    }

    [Fact]
    public void TryMap_Preview_MultiAllowedTypesMultiItems_Returns()
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

        var element = TestDataGenerator.GenerateLinkedItemsElement(
            Guid.NewGuid().ToString(),
            "articles",
            limitModel,
            linkedContentTypeModels.Select(ct => ct.Id));

        var result = TypedDeliveryPropertyMapper.TryMap(element, allContentTypes, ExtendedDeliverPreviewModelsOptions, out var typedProperty);

        result.Should().BeFalse();
        typedProperty.Should().BeNull();
    }

    #endregion
}
