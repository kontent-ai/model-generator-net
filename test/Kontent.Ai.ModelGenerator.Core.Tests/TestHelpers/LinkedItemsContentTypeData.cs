using Kontent.Ai.Management.Models.Shared;
using Kontent.Ai.Management.Models.Types;
using Kontent.Ai.Management.Models.Types.Elements;

namespace Kontent.Ai.ModelGenerator.Core.Tests.TestHelpers;

public static class LinkedItemsContentTypeData
{
    public static ContentTypeModel HeroContentType = new ContentTypeModel
    {
        Name = "Hero",
        Id = Guid.NewGuid(),
        Codename = "hero"
    };

    public static ContentTypeModel ArticleContentType = new ContentTypeModel
    {
        Name = "Article",
        Id = Guid.NewGuid(),
        Codename = "article"
    };

    /// <summary>
    /// Represents linked items element limited to a single type with at least 1 item
    /// </summary>
    public static LinkedItemsElementMetadataModel SingleAllowedTypeMultiItems = GenerateSingleAllowedTypeMultiItems();

    /// <summary>
    /// Represents linked items element limited to a single type with exactly 1 item
    /// </summary>
    public static LinkedItemsElementMetadataModel SingleAllowedTypeExactlySingleItem = GenerateSingleAllowedTypeExactlySingleItem();

    /// <summary>
    /// Represents linked items element limited to a single type with at most 1 item
    /// </summary>
    public static LinkedItemsElementMetadataModel SingleAllowedTypeAtMostSingleItem = GenerateSingleAllowedTypeAtMostSingleItem();

    /// <summary>
    /// Represents linked items element limited to multiple types with exactly 1 item
    /// </summary>
    public static LinkedItemsElementMetadataModel MultiAllowedTypesExactlySingleItem = GenerateMultiAllowedTypesExactlySingleItem();

    /// <summary>
    /// Represents linked items element limited to multiple types with at most 1 item
    /// </summary>
    public static LinkedItemsElementMetadataModel MultiAllowedTypesAtMostSingleItem = GenerateMultiAllowedTypesAtMostSingleItem();

    /// <summary>
    /// Represents linked items element limited to multiple types with at least 1 item
    /// </summary>
    public static LinkedItemsElementMetadataModel MultiAllowedTypesMultiItems = GenerateMultiAllowedTypesMultiItems();

    private static LinkedItemsElementMetadataModel GenerateSingleAllowedTypeMultiItems()
    {
        var singleAllowedTypeMultiItems = (LinkedItemsElementMetadataModel)TestDataGenerator.
            GenerateElementMetadataBase(Guid.Parse("4fa6bad6-d984-45e8-8ebb-f6be25626ee5"), "modular_content_heroes", ElementMetadataType.LinkedItems);
        singleAllowedTypeMultiItems.AllowedTypes = new List<Reference>(new List<Reference> { Reference.ById(HeroContentType.Id) });
        singleAllowedTypeMultiItems.ItemCountLimit = new LimitModel { Condition = LimitType.AtLeast, Value = 1 };

        return singleAllowedTypeMultiItems;
    }

    private static LinkedItemsElementMetadataModel GenerateSingleAllowedTypeExactlySingleItem()
    {
        var singleAllowedTypeExactlySingleItem = (LinkedItemsElementMetadataModel)TestDataGenerator.
            GenerateElementMetadataBase(Guid.Parse("4fa6bad6-d984-45e8-8ebb-f6be25626ee8"), "modular_content_article", ElementMetadataType.LinkedItems);
        singleAllowedTypeExactlySingleItem.AllowedTypes = new List<Reference>(new List<Reference> { Reference.ById(ArticleContentType.Id) });
        singleAllowedTypeExactlySingleItem.ItemCountLimit = new LimitModel { Condition = LimitType.Exactly, Value = 1 };

        return singleAllowedTypeExactlySingleItem;
    }

    private static LinkedItemsElementMetadataModel GenerateSingleAllowedTypeAtMostSingleItem()
    {
        var singleAllowedTypeExactlySingleItem = (LinkedItemsElementMetadataModel)TestDataGenerator.
            GenerateElementMetadataBase(Guid.Parse("4fa6cccc-d984-45e8-8ebb-f6be25626ee8"), "modular_content_hero", ElementMetadataType.LinkedItems);
        singleAllowedTypeExactlySingleItem.AllowedTypes = new List<Reference>(new List<Reference> { Reference.ById(HeroContentType.Id) });
        singleAllowedTypeExactlySingleItem.ItemCountLimit = new LimitModel { Condition = LimitType.AtMost, Value = 1 };

        return singleAllowedTypeExactlySingleItem;
    }

    private static LinkedItemsElementMetadataModel GenerateMultiAllowedTypesExactlySingleItem()
    {
        var multiAllowedTypesSingleItem = (LinkedItemsElementMetadataModel)TestDataGenerator.
            GenerateElementMetadataBase(Guid.Parse("4fa6bad6-d984-45e8-8ebb-f6be25626ee9"), "modular_content_blog", ElementMetadataType.LinkedItems);
        multiAllowedTypesSingleItem.AllowedTypes = new List<Reference>(new List<Reference>
        {
            Reference.ById(HeroContentType.Id),
            Reference.ById(ArticleContentType.Id)
        });
        multiAllowedTypesSingleItem.ItemCountLimit = new LimitModel { Condition = LimitType.Exactly, Value = 1 };

        return multiAllowedTypesSingleItem;
    }

    private static LinkedItemsElementMetadataModel GenerateMultiAllowedTypesAtMostSingleItem()
    {
        var multiAllowedTypesSingleItem = (LinkedItemsElementMetadataModel)TestDataGenerator.
            GenerateElementMetadataBase(Guid.Parse("4fa6bad6-d984-45cc-8ebb-f6be25626ee9"), "modular_content_coffee", ElementMetadataType.LinkedItems);
        multiAllowedTypesSingleItem.AllowedTypes = new List<Reference>(new List<Reference>
        {
            Reference.ById(HeroContentType.Id),
            Reference.ById(ArticleContentType.Id)
        });
        multiAllowedTypesSingleItem.ItemCountLimit = new LimitModel { Condition = LimitType.AtMost, Value = 1 };

        return multiAllowedTypesSingleItem;
    }

    private static LinkedItemsElementMetadataModel GenerateMultiAllowedTypesMultiItems()
    {
        var multiAllowedTypesMultiItems = (LinkedItemsElementMetadataModel)TestDataGenerator.
            GenerateElementMetadataBase(Guid.Parse("9fa6bad6-d984-45e8-8ebb-f6be25626ee5"), "modular_content_coffees", ElementMetadataType.LinkedItems);
        multiAllowedTypesMultiItems.AllowedTypes = new List<Reference>(new List<Reference>
        {
            Reference.ById(HeroContentType.Id),
            Reference.ById(ArticleContentType.Id)
        });
        multiAllowedTypesMultiItems.ItemCountLimit = new LimitModel { Condition = LimitType.AtLeast, Value = 1 };

        return multiAllowedTypesMultiItems;
    }
}
