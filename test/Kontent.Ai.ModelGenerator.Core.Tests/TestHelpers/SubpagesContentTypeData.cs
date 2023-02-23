using Kontent.Ai.Management.Models.Shared;
using Kontent.Ai.Management.Models.Types;
using Kontent.Ai.Management.Models.Types.Elements;

namespace Kontent.Ai.ModelGenerator.Core.Tests.TestHelpers;

public static class SubpagesContentTypeData
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
    public static SubpagesElementMetadataModel SingleAllowedTypeMultiItems = GenerateSingleAllowedTypeMultiItems();

    /// <summary>
    /// Represents linked items element limited to a single type with exactly 1 item
    /// </summary>
    public static SubpagesElementMetadataModel SingleAllowedTypeExactlySingleItem = GenerateSingleAllowedTypeExactlySingleItem();

    /// <summary>
    /// Represents linked items element limited to a single type with at most 1 item
    /// </summary>
    public static SubpagesElementMetadataModel SingleAllowedTypeAtMostSingleItem = GenerateSingleAllowedTypeAtMostSingleItem();

    /// <summary>
    /// Represents linked items element limited to multiple types with exactly 1 item
    /// </summary>
    public static SubpagesElementMetadataModel MultiAllowedTypesExactlySingleItem = GenerateMultiAllowedTypesExactlySingleItem();

    /// <summary>
    /// Represents linked items element limited to multiple types with at most 1 item
    /// </summary>
    public static SubpagesElementMetadataModel MultiAllowedTypesAtMostSingleItem = GenerateMultiAllowedTypesAtMostSingleItem();

    /// <summary>
    /// Represents linked items element limited to multiple types with at least 1 item
    /// </summary>
    public static SubpagesElementMetadataModel MultiAllowedTypesMultiItems = GenerateMultiAllowedTypesMultiItems();

    private static SubpagesElementMetadataModel GenerateSingleAllowedTypeMultiItems()
    {
        var singleAllowedTypeMultiItems = (SubpagesElementMetadataModel)TestDataGenerator.
            GenerateElementMetadataBase(Guid.Parse("8fa6bad6-d984-45e8-8ebb-f6be25626ee5"), "subpages_heroes", ElementMetadataType.Subpages);
        singleAllowedTypeMultiItems.AllowedContentTypes = new List<Reference>(new List<Reference> { Reference.ById(HeroContentType.Id) });
        singleAllowedTypeMultiItems.ItemCountLimit = new LimitModel { Condition = LimitType.AtLeast, Value = 1 };

        return singleAllowedTypeMultiItems;
    }

    private static SubpagesElementMetadataModel GenerateSingleAllowedTypeExactlySingleItem()
    {
        var singleAllowedTypeExactlySingleItem = (SubpagesElementMetadataModel)TestDataGenerator.
            GenerateElementMetadataBase(Guid.Parse("8fa6bad6-d984-45e8-8ebb-f6be25626ee8"), "subpages_article", ElementMetadataType.Subpages);
        singleAllowedTypeExactlySingleItem.AllowedContentTypes = new List<Reference>(new List<Reference> { Reference.ById(ArticleContentType.Id) });
        singleAllowedTypeExactlySingleItem.ItemCountLimit = new LimitModel { Condition = LimitType.Exactly, Value = 1 };

        return singleAllowedTypeExactlySingleItem;
    }

    private static SubpagesElementMetadataModel GenerateSingleAllowedTypeAtMostSingleItem()
    {
        var singleAllowedTypeAtMostSingleItem = (SubpagesElementMetadataModel)TestDataGenerator.
            GenerateElementMetadataBase(Guid.Parse("8fa6bad6-2284-45e8-8ebb-f6be25626ee8"), "subpages_hero", ElementMetadataType.Subpages);
        singleAllowedTypeAtMostSingleItem.AllowedContentTypes = new List<Reference>(new List<Reference> { Reference.ById(HeroContentType.Id) });
        singleAllowedTypeAtMostSingleItem.ItemCountLimit = new LimitModel { Condition = LimitType.Exactly, Value = 1 };

        return singleAllowedTypeAtMostSingleItem;
    }

    private static SubpagesElementMetadataModel GenerateMultiAllowedTypesExactlySingleItem()
    {
        var multiAllowedTypesSingleItem = (SubpagesElementMetadataModel)TestDataGenerator.
            GenerateElementMetadataBase(Guid.Parse("8fa6bad6-d984-45e8-8ebb-f6be25626ee9"), "subpages_blog", ElementMetadataType.Subpages);
        multiAllowedTypesSingleItem.AllowedContentTypes = new List<Reference>(new List<Reference>
        {
            Reference.ById(HeroContentType.Id),
            Reference.ById(ArticleContentType.Id)
        });
        multiAllowedTypesSingleItem.ItemCountLimit = new LimitModel { Condition = LimitType.Exactly, Value = 1 };

        return multiAllowedTypesSingleItem;
    }

    private static SubpagesElementMetadataModel GenerateMultiAllowedTypesAtMostSingleItem()
    {
        var multiAllowedTypesSingleItem = (SubpagesElementMetadataModel)TestDataGenerator.
            GenerateElementMetadataBase(Guid.Parse("8fa6ccd6-d984-45e8-cccc-f6be25626ee9"), "subpages_coffee", ElementMetadataType.Subpages);
        multiAllowedTypesSingleItem.AllowedContentTypes = new List<Reference>(new List<Reference>
        {
            Reference.ById(HeroContentType.Id),
            Reference.ById(ArticleContentType.Id)
        });
        multiAllowedTypesSingleItem.ItemCountLimit = new LimitModel { Condition = LimitType.AtMost, Value = 1 };

        return multiAllowedTypesSingleItem;
    }

    private static SubpagesElementMetadataModel GenerateMultiAllowedTypesMultiItems()
    {
        var multiAllowedTypesMultiItems = (SubpagesElementMetadataModel)TestDataGenerator.
            GenerateElementMetadataBase(Guid.Parse("8fa6bad6-d984-45e8-8ebb-f6be25626ee6"), "subpages_coffees", ElementMetadataType.Subpages);
        multiAllowedTypesMultiItems.AllowedContentTypes = new List<Reference>(new List<Reference>
        {
            Reference.ById(HeroContentType.Id),
            Reference.ById(ArticleContentType.Id)
        });
        multiAllowedTypesMultiItems.ItemCountLimit = new LimitModel { Condition = LimitType.AtLeast, Value = 1 };

        return multiAllowedTypesMultiItems;
    }
}
