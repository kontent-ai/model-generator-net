using System;

namespace Kontent.Ai.ModelGenerator.Core.Configuration;

[Flags]
public enum StructuredModelFlags
{
    NotSet = 1,
    RichText = 2,
    DateTime = 4,
    ValidationIssue = 8,
    True = 16
}
