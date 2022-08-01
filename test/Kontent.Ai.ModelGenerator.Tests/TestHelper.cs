using System;
using Kontent.Ai.Management.Models.Types.Elements;
using Newtonsoft.Json.Linq;

namespace Kontent.Ai.ModelGenerator.Tests
{
    internal static class TestHelper
    {
        public static ElementMetadataBase GenerateElementMetadataBase(Guid elementId, string elementCodename, ElementMetadataType type = ElementMetadataType.Text) =>
            JObject.FromObject(new
            {
                Id = elementId,
                Codename = elementCodename,
                type
            }).ToObject<ElementMetadataBase>();

        public static ElementMetadataBase GenerateGuidelinesElement(Guid elementId, string elementCodename) =>
            JObject.FromObject(new
            {
                Id = elementId,
                Codename = elementCodename,
                type = ElementMetadataType.Guidelines,
                guidelines = "guidelines"

            }).ToObject<ElementMetadataBase>();
    }
}
