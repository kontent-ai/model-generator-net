using System;
using Kontent.Ai.ModelGenerator.Core.Common;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kontent.Ai.ModelGenerator.Core.Generators.Class;

public class ContentItemClassCodeGenerator : ClassCodeGenerator
{
    public const string DefaultContentItemClassName = "IContentItem";

    public ContentItemClassCodeGenerator(string @namespace = DefaultNamespace)
        : this(new ClassDefinition(DefaultContentItemClassName), DefaultContentItemClassName, @namespace)
    {
    }

    private ContentItemClassCodeGenerator(ClassDefinition classDefinition, string classFilename, string @namespace = DefaultNamespace)
        : base(classDefinition, classFilename, @namespace)
    {
    }

    protected override UsingDirectiveSyntax[] GetApiUsings()
    {
        throw new NotImplementedException();
    }
}
