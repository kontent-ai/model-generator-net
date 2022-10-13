using Kontent.Ai.ModelGenerator.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Kontent.Ai.Management.Models.Types;
using Kontent.Ai.Management.Models.Types.Elements;
using Kontent.Ai.ModelGenerator.Core.Configuration;
using Xunit;
using Kontent.Ai.ModelGenerator.Core.Generators.Class;

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
    public void Map_LinkedItemsElementIsNull_Throws()
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

        Assert.Throws<ArgumentNullException>(() => TypedDeliveryPropertyMapper.Map(null, contentTypes, options));
    }

    [Fact]
    public void Map_NotLinkedItemsElement_Throws()
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

        Assert.Throws<ArgumentNullException>(() => TypedDeliveryPropertyMapper.Map(element, contentTypes, options));
    }

    [Fact]
    public void Map_ContentTypesIsNull_Throws()
    {
        var element = new LinkedItemsElementMetadataModel();
        var options = new CodeGeneratorOptions
        {
            ExtendedDeliverPreviewModels = false,
            ExtendedDeliverModels = true
        };

        Assert.Throws<ArgumentNullException>(() => TypedDeliveryPropertyMapper.Map(element, null, options));
    }

    [Fact]
    public void Map_ContentTypesIsEmpty_Throws()
    {
        var contentTypes = new List<ContentTypeModel>();
        var element = new LinkedItemsElementMetadataModel();
        var options = new CodeGeneratorOptions
        {
            ExtendedDeliverPreviewModels = false,
            ExtendedDeliverModels = true
        };

        Assert.Throws<ArgumentException>(() => TypedDeliveryPropertyMapper.Map(element, contentTypes, options));
    }

    [Fact]
    public void Map_OptionsIsNull_Throws()
    {
        var contentTypes = new List<ContentTypeModel>
        {
            new ContentTypeModel()
        };
        var element = new LinkedItemsElementMetadataModel();

        Assert.Throws<ArgumentNullException>(() => TypedDeliveryPropertyMapper.Map(element, contentTypes, null));
    }

    [Fact]
    public void Map_GeneralExtendedDeliveryModelsIsFalse_Throws()
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

        Assert.Throws<ArgumentException>(() => TypedDeliveryPropertyMapper.Map(element, contentTypes, options));
    }

    #region Live models

    [Fact]
    public void Map_Live_CouldNotFindAllowedType_Throws()
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

        var element = TestHelper.GenerateLinkedItemsElement(
            Guid.NewGuid().ToString(),
            "articles",
            limitModel,
            linkedContentTypeModels.Select(ct => ct.Id));

        Assert.Throws<ArgumentException>(() => TypedDeliveryPropertyMapper.Map(element, allContentTypes, ExtendedDeliverModelsOptions));
    }

    [Fact]
    public void Map_Live_SingleAllowedTypeMultiItems_Returns()
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

        var element = TestHelper.GenerateLinkedItemsElement(
            Guid.NewGuid().ToString(),
            "articles",
            limitModel,
            linkedContentTypeModels.Select(ct => ct.Id));

        var result = TypedDeliveryPropertyMapper.Map(element, allContentTypes, ExtendedDeliverModelsOptions);

        Assert.Single(result);
        Assert.Equal("Articles_Article", result.First().Codename);
        Assert.Null(result.First().Id);
        Assert.Equal("ArticlesArticle", result.First().Identifier);
        Assert.Equal("IEnumerable<Article>", result.First().TypeName);
    }

    [Fact]
    public void Map_Live_SingleAllowedTypeExactlySingleItem_Returns()
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

        var element = TestHelper.GenerateLinkedItemsElement(
            Guid.NewGuid().ToString(),
            "article",
            limitModel,
            linkedContentTypeModels.Select(ct => ct.Id));

        var result = TypedDeliveryPropertyMapper.Map(element, allContentTypes, ExtendedDeliverModelsOptions);

        Assert.Single(result);
        Assert.Equal("article", result.First().Codename);
        Assert.Null(result.First().Id);
        Assert.Equal("Article", result.First().Identifier);
        Assert.Equal("Article", result.First().TypeName);
    }

    [Fact]
    public void Map_Live_MultiAllowedTypesSingleItem_Returns()
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

        var element = TestHelper.GenerateLinkedItemsElement(
            Guid.NewGuid().ToString(),
            "article",
            limitModel,
            linkedContentTypeModels.Select(ct => ct.Id));

        var result = TypedDeliveryPropertyMapper.Map(element, allContentTypes, ExtendedDeliverModelsOptions);

        Assert.Empty(result);
    }

    [Fact]
    public void Map_Live_MultiAllowedTypesMultiItems_Returns()
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

        var element = TestHelper.GenerateLinkedItemsElement(
            Guid.NewGuid().ToString(),
            "articles",
            limitModel,
            linkedContentTypeModels.Select(ct => ct.Id));

        var result = TypedDeliveryPropertyMapper.Map(element, allContentTypes, ExtendedDeliverModelsOptions);

        Assert.Empty(result);
    }

    #endregion

    #region Preview models

    [Fact]
    public void Map_Preview_CouldNotFindAllowedType_Throws()
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

        var element = TestHelper.GenerateLinkedItemsElement(
            Guid.NewGuid().ToString(),
            "articles",
            limitModel,
            linkedContentTypeModels.Select(ct => ct.Id));

        Assert.Throws<ArgumentException>(() => TypedDeliveryPropertyMapper.Map(element, allContentTypes, ExtendedDeliverPreviewModelsOptions));
    }

    [Fact]
    public void Map_Preview_SingleAllowedTypeMultiItems_Returns()
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

        var element = TestHelper.GenerateLinkedItemsElement(
            Guid.NewGuid().ToString(),
            "articles",
            limitModel,
            linkedContentTypeModels.Select(ct => ct.Id));

        var result = TypedDeliveryPropertyMapper.Map(element, allContentTypes, ExtendedDeliverPreviewModelsOptions);

        Assert.Single(result);
        Assert.Equal("Articles_Article", result.First().Codename);
        Assert.Null(result.First().Id);
        Assert.Equal("ArticlesArticle", result.First().Identifier);
        Assert.Equal($"IEnumerable<{ContentItemClassCodeGenerator.DefaultContentItemClassName}>", result.First().TypeName);
    }

    [Fact]
    public void Map_Preview_SingleAllowedTypeExactlySingleItem_Returns()
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

        var element = TestHelper.GenerateLinkedItemsElement(
            Guid.NewGuid().ToString(),
            "article",
            limitModel,
            linkedContentTypeModels.Select(ct => ct.Id));

        var result = TypedDeliveryPropertyMapper.Map(element, allContentTypes, ExtendedDeliverPreviewModelsOptions);

        Assert.Single(result);
        Assert.Equal("article", result.First().Codename);
        Assert.Null(result.First().Id);
        Assert.Equal("Article", result.First().Identifier);
        Assert.Equal("IEnumerable<IContentItem>", result.First().TypeName);
    }

    [Fact]
    public void Map_Preview_MultiAllowedTypesSingleItem_Returns()
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

        var element = TestHelper.GenerateLinkedItemsElement(
            Guid.NewGuid().ToString(),
            "article",
            limitModel,
            linkedContentTypeModels.Select(ct => ct.Id));

        var result = TypedDeliveryPropertyMapper.Map(element, allContentTypes, ExtendedDeliverPreviewModelsOptions);

        Assert.Empty(result);
    }

    [Fact]
    public void Map_Preview_MultiAllowedTypesMultiItems_Returns()
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

        var element = TestHelper.GenerateLinkedItemsElement(
            Guid.NewGuid().ToString(),
            "articles",
            limitModel,
            linkedContentTypeModels.Select(ct => ct.Id));

        var result = TypedDeliveryPropertyMapper.Map(element, allContentTypes, ExtendedDeliverPreviewModelsOptions);

        Assert.Empty(result);
    }

    #endregion
}
