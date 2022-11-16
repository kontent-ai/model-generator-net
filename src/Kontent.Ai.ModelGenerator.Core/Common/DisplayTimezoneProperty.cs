using System;
using Kontent.Ai.ModelGenerator.Core.Helpers;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kontent.Ai.ModelGenerator.Core.Common;

public class DisplayTimezoneProperty : Property
{
    public string DateTimeElementCodename { get; init; }

    public DisplayTimezoneProperty(string propertyCodename, string dateTimeElementCodename, string typeName, string id = null) : base(propertyCodename, typeName, id)
    {
        DateTimeElementCodename = dateTimeElementCodename;
    }

    public static new DisplayTimezoneProperty FromContentTypeElement(string elementCodename, string elementType)
    {
        if (elementType == "date_time")
        {
            var propertyCodename = elementCodename + "_display_timezone";
            return new DisplayTimezoneProperty(propertyCodename, elementCodename, "string");
        }

        throw new ArgumentException($"Cannot create DisplayTimezone property from {elementType} type", nameof(elementType));
    }
}
