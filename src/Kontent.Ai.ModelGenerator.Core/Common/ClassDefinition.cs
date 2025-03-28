using System;
using System.Collections.Generic;
using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.ModelGenerator.Core.Helpers;

namespace Kontent.Ai.ModelGenerator.Core.Common;

public class ClassDefinition(string codeName)
{
    public List<Property> Properties { get; } = [];

    public List<string> PropertyCodenameConstants { get; } = [];

    public string ClassName => TextHelpers.GetValidPascalCaseIdentifierName(Codename);

    public string Codename { get; } = codeName;

    public void AddProperty(Property property)
    {
        if (PropertyIsAlreadyPresent(property))
        {
            throw new InvalidOperationException($"Property with Identifier '{property.Identifier}' is already included. Can't add two properties with the same Identifier.");
        }

        Properties.Add(property);
    }

    public void AddPropertyCodenameConstant(string codeName)
    {
        if (PropertyCodenameConstants.Contains(codeName))
        {
            throw new InvalidOperationException($"Property with code name '{codeName}' is already included. Can't add two members with the same code name.");
        }

        PropertyCodenameConstants.Add(codeName);
    }

    public void AddSystemProperty()
    {
        AddProperty(new Property("system", nameof(IContentItemSystemAttributes)));
    }

    private bool PropertyIsAlreadyPresent(Property property)
    {
        return Properties.Exists(e => e.Identifier == property.Identifier);
    }
}
