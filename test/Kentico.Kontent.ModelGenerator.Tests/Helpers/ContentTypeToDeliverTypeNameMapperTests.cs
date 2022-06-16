//using System;
//using System.Collections.Generic;
//using Kentico.Kontent.Management.Models.Shared;
//using Kentico.Kontent.Management.Models.Types;
//using Kentico.Kontent.Management.Models.Types.Elements;
//using Kentico.Kontent.ModelGenerator.Core.Configuration;
//using Kentico.Kontent.ModelGenerator.Core.Generators.Class;
//using Kentico.Kontent.ModelGenerator.Core.Helpers;
//using Xunit;

//namespace Kentico.Kontent.ModelGenerator.Tests.Helpers
//{
//    public class ContentTypeToDeliverTypeNameMapperTests
//    {
//        [Fact]
//        public void Map_NotLinkedItemsElement_Throws()
//        {
//            Assert.Throws<ArgumentNullException>(() => ContentTypeToDeliverTypeNameMapper.Map(new CustomElementMetadataModel(), null, null));
//        }

//        [Fact]
//        public void Map_ContentTypesIsNull_Throws()
//        {
//            Assert.Throws<ArgumentNullException>(() => ContentTypeToDeliverTypeNameMapper.Map(new LinkedItemsElementMetadataModel(), null, null));
//        }

//        [Fact]
//        public void Map_ContentTypesIsEmpty_Throws()
//        {
//            Assert.Throws<ArgumentException>(() => ContentTypeToDeliverTypeNameMapper.Map(new LinkedItemsElementMetadataModel(), new List<ContentTypeModel>(), null));
//        }

//        [Fact]
//        public void Map_OptionsIsNull_Throws()
//        {
//            Assert.Throws<ArgumentNullException>(() =>
//                ContentTypeToDeliverTypeNameMapper.Map(new LinkedItemsElementMetadataModel(), new List<ContentTypeModel> { new ContentTypeModel() }, null));
//        }

//        [Fact]
//        public void Map_DoesNotContainCorrectType_Throws()
//        {
//            var linkedItemsElement = new LinkedItemsElementMetadataModel
//            {
//                AllowedTypes = new List<Reference>
//                {
//                    Reference.ById(Guid.NewGuid())
//                }
//            };

//            var contentType = new ContentTypeModel
//            {
//                Id = Guid.NewGuid()
//            };

//            Assert.Throws<ArgumentNullException>(() =>
//                ContentTypeToDeliverTypeNameMapper.Map(linkedItemsElement, new List<ContentTypeModel> { contentType }, null));
//        }

//        [Fact]
//        public void Map_MultipleAllowedTypes_Returns()
//        {
//            var contentType = new ContentTypeModel
//            {
//                Id = Guid.NewGuid()
//            };

//            var linkedItemsElement = new LinkedItemsElementMetadataModel
//            {
//                AllowedTypes = new List<Reference>
//                {
//                    Reference.ById(contentType.Id),
//                    Reference.ById(Guid.NewGuid())
//                }
//            };

//            var result = ContentTypeToDeliverTypeNameMapper.Map(linkedItemsElement, new List<ContentTypeModel> { contentType }, new CodeGeneratorOptions());

//            Assert.Equal($"IEnumerable<{ContentItemClassCodeGenerator.DefaultContentItemClassName}>", result);
//        }

//        [Fact]
//        public void Map_MultipleAllowedTypes_Preview_Returns()
//        {
//            var contentType = new ContentTypeModel
//            {
//                Id = Guid.NewGuid()
//            };

//            var linkedItemsElement = new LinkedItemsElementMetadataModel
//            {
//                AllowedTypes = new List<Reference>
//                {
//                    Reference.ById(contentType.Id),
//                    Reference.ById(Guid.NewGuid())
//                }
//            };

//            var options = new CodeGeneratorOptions
//            {
//                ExtendedDeliverPreviewModels = true
//            };

//            var result = ContentTypeToDeliverTypeNameMapper.Map(linkedItemsElement, new List<ContentTypeModel> { contentType }, options);

//            Assert.Equal($"IEnumerable<{ContentItemClassCodeGenerator.DefaultContentItemClassName}>", result);
//        }

//        [Fact]
//        public void Map_SingleAllowedType_Returns()
//        {
//            var contentType = new ContentTypeModel
//            {
//                Id = Guid.NewGuid(),
//                Codename = "typename"
//            };

//            var contentType2 = new ContentTypeModel
//            {
//                Id = Guid.NewGuid(),
//                Codename = "typename2"
//            };

//            var linkedItemsElement = new LinkedItemsElementMetadataModel
//            {
//                AllowedTypes = new List<Reference>
//                {
//                    Reference.ById(contentType.Id),
//                }
//            };

//            var result = ContentTypeToDeliverTypeNameMapper.Map(linkedItemsElement, new List<ContentTypeModel> { contentType, contentType2 }, new CodeGeneratorOptions());

//            Assert.Equal($"IEnumerable<{char.ToUpper(contentType.Codename[0]) + contentType.Codename.Substring(1)}>", result);
//        }

//        [Fact]
//        public void Map_SingleAllowedType_Preview_Returns()
//        {
//            var contentType = new ContentTypeModel
//            {
//                Id = Guid.NewGuid(),
//            };

//            var contentType2 = new ContentTypeModel
//            {
//                Id = Guid.NewGuid(),
//                Codename = "typename2"
//            };

//            var linkedItemsElement = new LinkedItemsElementMetadataModel
//            {
//                AllowedTypes = new List<Reference>
//                {
//                    Reference.ById(contentType.Id),
//                }
//            };

//            var options = new CodeGeneratorOptions
//            {
//                ExtendedDeliverPreviewModels = true
//            };

//            var result = ContentTypeToDeliverTypeNameMapper.Map(linkedItemsElement, new List<ContentTypeModel> { contentType, contentType2 }, options);

//            Assert.Equal($"IEnumerable<{ContentItemClassCodeGenerator.DefaultContentItemClassName}>", result);
//        }
//    }
//}
