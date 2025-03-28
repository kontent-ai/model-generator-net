﻿using Kontent.Ai.ModelGenerator.Core.Generators.Class;
using Microsoft.CodeAnalysis;

namespace Kontent.Ai.ModelGenerator.Core.Generators;

public abstract class GeneralGenerator(string @namespace = ClassCodeGenerator.DefaultNamespace)
{
    protected const string LostChangesComment = "// Changes to this file may cause incorrect behavior and will be lost if the code is regenerated.";

    public readonly string Namespace = string.IsNullOrWhiteSpace(@namespace) ? ClassCodeGenerator.DefaultNamespace : @namespace;

    protected abstract SyntaxTrivia ClassDescription();
}
