﻿using System;
using Kentico.Kontent.Management.Models.Types.Elements;
using Newtonsoft.Json.Linq;

namespace Kentico.Kontent.ModelGenerator.Tests
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
