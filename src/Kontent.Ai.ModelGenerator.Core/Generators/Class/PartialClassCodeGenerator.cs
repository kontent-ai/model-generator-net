using System;
using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kontent.Ai.ModelGenerator.Core.Generators.Class
{
    public class PartialClassCodeGenerator : DeliveryClassCodeGeneratorBase
    {
        public PartialClassCodeGenerator(ClassDefinition classDefinition, string classFilename, string @namespace = DefaultNamespace)
            : base(classDefinition, classFilename, @namespace)
        {
        }

        protected override ClassDeclarationSyntax GetClassDeclaration()
        {
            var classDeclaration = base.GetClassDeclaration();

            classDeclaration = classDeclaration.AddMembers(PropertyCodenameConstants);
            classDeclaration = classDeclaration.AddMembers(Properties);

            return classDeclaration;
        }

        protected override UsingDirectiveSyntax[] GetApiUsings() => Array.Empty<UsingDirectiveSyntax>();

        protected override SyntaxTrivia ClassDescription() =>
            ClassDeclarationHelper.GenerateSyntaxTrivia(@"// Changes to this file will not be lost if the code is regenerated.");
    }
}
