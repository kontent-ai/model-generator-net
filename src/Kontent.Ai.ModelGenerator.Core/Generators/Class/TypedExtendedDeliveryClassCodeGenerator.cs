using System;
using Kontent.Ai.ModelGenerator.Core.Common;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kontent.Ai.ModelGenerator.Core.Generators.Class;

public class TypedExtendedDeliveryClassCodeGenerator : ClassCodeGenerator
{
    public TypedExtendedDeliveryClassCodeGenerator(ClassDefinition classDefinition, string classFilename, string @namespace = DefaultNamespace)
        : base(classDefinition, classFilename, @namespace)
    {
    }

    protected override UsingDirectiveSyntax[] GetApiUsings()
    {
        throw new NotImplementedException();
    }

    protected override TypeDeclarationSyntax GetClassDeclaration()
    {
        throw new NotImplementedException();
    }
}
