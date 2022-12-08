using System;

namespace Kontent.Ai.ModelGenerator.Core.Configuration;

[Flags]
public enum ElementReferenceType
{
    Id = 1,
    Codename = 2,
    ExternalId = 4,
    Empty = 16,
    Error = 32
}
