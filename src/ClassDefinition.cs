using System;
using System.Collections.Generic;

namespace KenticoCloudDotNetGenerators
{
    public class ClassDefinition
    {
        public List<Property> Properties { get; } = new List<Property>();

        public string ClassName { get; }

        public ClassDefinition(string codeName)
        {
            ClassName = TextHelpers.GetValidPascalCaseIdentifierName(codeName);
        }

        public void AddProperty(Property property)
        {
            if (PropertyIsAlreadyPresent(property))
            {
                throw new InvalidOperationException($"Property with Identifier '{property.Identifier}' is already included. Can't add two properties with the same Identifier.");
            }

            Properties.Add(property);
        }

        public void AddSystemProperty()
        {
            AddProperty(new Property("system", "ContentItemSystemAttributes"));
        }

        private bool PropertyIsAlreadyPresent(Property property)
        {
            return Properties.Exists(e => e.Identifier == property.Identifier);
        }
    }
}
