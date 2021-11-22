using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kentico.Kontent.ModelGenerator.Core
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

        protected override CompilationUnitSyntax GetCompilationUnit(ClassDeclarationSyntax classDeclaration, UsingDirectiveSyntax[] usings)
        {
            var compilationUnit = base.GetCompilationUnit(classDeclaration, usings);
            compilationUnit = compilationUnit.WithoutLeadingTrivia();
            return compilationUnit;
        }
    }
}
