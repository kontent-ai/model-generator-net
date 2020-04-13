using Kentico.Kontent.Delivery.Abstractions;
using System;
using System.Collections.Generic;

namespace Kentico.Kontent.ModelGenerator
{
    public class ClassDefinition
    {
        public List<Property> Properties { get; } = new List<Property>();

        public List<ContentElement> PropertyCodenameConstants { get; } = new List<ContentElement>();

        public string ClassName => TextHelpers.GetValidPascalCaseIdentifierName(Codename);

        public string Codename { get; }

        public ClassDefinition(string codeName)
        {
            Codename = codeName;
        }

        public void AddProperty(Property property)
        {
            if (PropertyIsAlreadyPresent(property))
            {
                throw new InvalidOperationException($"Property with Identifier '{property.Identifier}' is already included. Can't add two properties with the same Identifier.");
            }

            Properties.Add(property);
        }

        public void AddPropertyCodenameConstant(ContentElement element)
        {
            if (PropertyCodenameConstantIsAlreadyPresent(element))
            {
                throw new InvalidOperationException($"Property with code name '{element.Codename}' is already included. Can't add two members with the same code name.");
            }

            PropertyCodenameConstants.Add(element);
        }

        public void AddSystemProperty()
        {
            AddProperty(new Property("system", "ContentItemSystemAttributes"));
        }

        private bool PropertyIsAlreadyPresent(Property property)
        {
            return Properties.Exists(e => e.Identifier == property.Identifier);
        }

        private bool PropertyCodenameConstantIsAlreadyPresent(ContentElement element)
        {
            return PropertyCodenameConstants.Exists(e => e.Codename == element.Codename);
        }
    }
}
