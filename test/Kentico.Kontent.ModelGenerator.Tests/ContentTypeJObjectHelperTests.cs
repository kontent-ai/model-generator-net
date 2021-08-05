using System;
using Kentico.Kontent.ModelGenerator.Core;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Kentico.Kontent.ModelGenerator.Tests
{
    public class ContentTypeJObjectHelperTests
    {
        [Fact]
        public void GetElementIdFromContentType_MissingElementsObject_ThrowsException()
        {
            var managementType = JObject.FromObject(new
            {
                id = Guid.NewGuid(),
                codename = "codename",
                last_modified = DateTime.UtcNow,
                name = "Name"
            });

            var exception = Assert.Throws<InvalidIdException>(() => ContentTypeJObjectHelper.GetElementIdFromContentType(managementType, "element_codename"));

            Assert.Equal("Unable to create a valid Id for 'element_codename', couldn't find elements.", exception.Message);
        }

        [Fact]
        public void GetElementIdFromContentType_EmptyElements_ThrowsException()
        {
            var managementType = JObject.FromObject(new
            {
                id = Guid.NewGuid(),
                codename = "codename",
                last_modified = DateTime.UtcNow,
                name = "Name",
                elements = Array.Empty<object>()
            });

            var exception = Assert.Throws<InvalidIdException>(() => ContentTypeJObjectHelper.GetElementIdFromContentType(managementType, "element_codename"));

            Assert.Equal("Unable to create a valid Id for 'element_codename', missing element.", exception.Message);
        }

        [Fact]
        public void GetElementIdFromContentType_InvalidElementsType_ThrowsException()
        {
            var managementType = JObject.FromObject(new
            {
                id = Guid.NewGuid(),
                codename = "codename",
                last_modified = DateTime.UtcNow,
                name = "Name",
                elements = ""
            });

            var exception = Assert.Throws<InvalidIdException>(() => ContentTypeJObjectHelper.GetElementIdFromContentType(managementType, "element_codename"));

            Assert.Equal("Unable to create a valid Id for 'element_codename', elements has invalid type.", exception.Message);
        }

        [Fact]
        public void GetElementIdFromContentType_MissingElementObject_ThrowsException()
        {
            var managementType = JObject.FromObject(new
            {
                id = Guid.NewGuid(),
                codename = "codename",
                last_modified = DateTime.UtcNow,
                name = "Name",
                elements = new object[]
                {
                    new {
                        maximum_text_length= 5,
                        name= "element_name",
                        guidelines= "guidelines",
                        is_required= false,
                        type="text",
                        id= Guid.NewGuid(),
                        codename= "element_codename"
                    }
                }
            });

            var exception = Assert.Throws<InvalidIdException>(() => ContentTypeJObjectHelper.GetElementIdFromContentType(managementType, "codename"));

            Assert.Equal("Unable to create a valid Id for 'codename', missing element.", exception.Message);
        }

        [Fact]
        public void GetElementIdFromContentType_MissingElementIdObject_ThrowsException()
        {
            var managementType = JObject.FromObject(new
            {
                id = Guid.NewGuid(),
                codename = "codename",
                last_modified = DateTime.UtcNow,
                name = "Name",
                elements = new object[]
                {
                    new {
                        maximum_text_length= 5,
                        name= "element_name",
                        guidelines= "guidelines",
                        is_required= false,
                        type="text",
                        codename= "element_codename"
                    }
                }
            });

            var exception = Assert.Throws<InvalidIdException>(() => ContentTypeJObjectHelper.GetElementIdFromContentType(managementType, "element_codename"));

            Assert.Equal("Unable to create a valid Id for 'element_codename', couldn't find elementId.", exception.Message);
        }

        [Fact]
        public void GetElementIdFromContentType_InvalidElementIdType_ThrowsException()
        {
            var managementType = JObject.FromObject(new
            {
                id = Guid.NewGuid(),
                codename = "codename",
                last_modified = DateTime.UtcNow,
                name = "Name",
                elements = new object[]
                {
                    new {
                        maximum_text_length= 5,
                        name= "element_name",
                        guidelines= "guidelines",
                        is_required= false,
                        type="text",
                        id = 4,
                        codename= "element_codename"
                    }
                }
            });

            var exception = Assert.Throws<InvalidIdException>(() => ContentTypeJObjectHelper.GetElementIdFromContentType(managementType, "element_codename"));

            Assert.Equal("Unable to create a valid Id for 'element_codename', elementId has invalid type.", exception.Message);
        }

        [Fact]
        public void GetElementIdFromContentType_Returns()
        {
            var elementId = Guid.NewGuid().ToString();
            var managementType = JObject.FromObject(new
            {
                id = Guid.NewGuid(),
                codename = "codename",
                last_modified = DateTime.UtcNow,
                name = "Name",
                elements = new object[]
                {
                    new {
                        maximum_text_length= 5,
                        name= "element_name",
                        guidelines= "guidelines",
                        is_required= false,
                        type="text",
                        id = elementId,
                        codename= "element_codename"
                    }
                }
            });

            var result = ContentTypeJObjectHelper.GetElementIdFromContentType(managementType, "element_codename");

            Assert.Equal(elementId, result);
        }
    }
}
