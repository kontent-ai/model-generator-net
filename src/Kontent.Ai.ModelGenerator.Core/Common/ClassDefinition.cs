using System;
using System.Collections.Generic;
using Kontent.Ai.ModelGenerator.Core.Helpers;

namespace Kontent.Ai.ModelGenerator.Core.Common;

public class ClassDefinition(string codeName)
{
    public const string ContentTypeCodenameIdentifier = "ContentTypeCodename";

    private readonly HashSet<string> _renamedCodenameConstants = [];

    public List<Property> Properties { get; } = [];

    /// <summary>
    /// Sibling enum types to emit alongside the content-type record. Populated by the
    /// Management emission path for multiple-choice elements; the Delivery path leaves this empty.
    /// </summary>
    public List<EnumDefinition> Enums { get; } = [];

    public List<string> PropertyCodenameConstants { get; } = [];

    public IReadOnlySet<string> RenamedCodenameConstants => _renamedCodenameConstants;

    public string ClassName => TextHelpers.GetValidPascalCaseIdentifierName(Codename);

    public string Codename { get; } = codeName;

    public void AddProperty(Property property)
    {
        if (property.Identifier == ContentTypeCodenameIdentifier)
        {
            property = new Property(property.Codename, property.TypeName, property.Id, property.Initializer)
            {
                IdentifierOverride = "_" + ContentTypeCodenameIdentifier
            };
        }

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

        var constantIdentifier = TextHelpers.GetValidPascalCaseIdentifierName(codeName) + "Codename";
        if (constantIdentifier == ContentTypeCodenameIdentifier)
        {
            _renamedCodenameConstants.Add(codeName);
        }

        PropertyCodenameConstants.Add(codeName);
    }

    public void AddEnum(EnumDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (Enums.Exists(e => e.Name == definition.Name))
        {
            throw new InvalidOperationException(
                $"Enum with name '{definition.Name}' is already included.");
        }

        Enums.Add(definition);
    }

    private bool PropertyIsAlreadyPresent(Property property)
    {
        return Properties.Exists(e => e.Identifier == property.Identifier);
    }
}
