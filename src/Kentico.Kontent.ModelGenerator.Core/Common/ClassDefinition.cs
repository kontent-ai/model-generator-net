using System;
using System.Collections.Generic;
using Kentico.Kontent.Delivery.Abstractions;
using Kentico.Kontent.ModelGenerator.Core.Helpers;

namespace Kentico.Kontent.ModelGenerator.Core.Common
{
    public class ClassDefinition
    {
        public List<Property> Properties { get; } = new List<Property>();

        public List<IContentElement> PropertyCodenameConstants { get; } = new List<IContentElement>();

        public string ClassName => TextHelpers.GetValidPascalCaseIdentifierName(Codename);

        public string Codename { get; }

        public ClassDefinition(string codeName)
        {
            Codename = string.IsNullOrWhiteSpace(codeName)
                ? throw new ArgumentException("Class codeName must be a non null and not white space string.", nameof(codeName))
                : codeName;
        }

        public void AddProperty(Property property)
        {
            if (PropertyIsAlreadyPresent(property))
            {
                throw new InvalidOperationException($"Property with Identifier '{property.Identifier}' is already included. Can't add two properties with the same Identifier.");
            }

            Properties.Add(property);
        }

        public void AddPropertyCodenameConstant(IContentElement element)
        {
            if (PropertyCodenameConstantIsAlreadyPresent(element))
            {
                throw new InvalidOperationException($"Property with code name '{element.Codename}' is already included. Can't add two members with the same code name.");
            }

            PropertyCodenameConstants.Add(element);
        }

        public void AddSystemProperty()
        {
            AddProperty(new Property("system", nameof(IContentItemSystemAttributes)));
        }

        private bool PropertyIsAlreadyPresent(Property property)
        {
            return Properties.Exists(e => e.Identifier == property.Identifier);
        }

        private bool PropertyCodenameConstantIsAlreadyPresent(IContentElement element)
        {
            return PropertyCodenameConstants.Exists(e => e.Codename == element.Codename);
        }
    }
}
