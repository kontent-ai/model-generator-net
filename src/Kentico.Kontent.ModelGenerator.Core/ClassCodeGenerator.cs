﻿using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace Kentico.Kontent.ModelGenerator.Core
{
    public abstract class ClassCodeGenerator
    {
        public const string DefaultNamespace = "KenticoKontentModels";

        public ClassDefinition ClassDefinition { get; }

        public string ClassFilename { get; }

        public bool CustomPartial { get; }

        public string Namespace { get; }

        public bool OverwriteExisting { get; }

        protected ClassCodeGenerator(ClassDefinition classDefinition, string classFilename, string @namespace = DefaultNamespace, bool customPartial = false)
        {
            ClassDefinition = classDefinition ?? throw new ArgumentNullException(nameof(classDefinition));
            ClassFilename = string.IsNullOrWhiteSpace(classFilename) ? ClassDefinition.ClassName : classFilename;
            CustomPartial = customPartial;
            Namespace = string.IsNullOrWhiteSpace(@namespace) ? DefaultNamespace : @namespace;
            OverwriteExisting = !CustomPartial;
        }

        public string GenerateCode()
        {
            var usings = GetApiUsings();
            var classDeclaration = GetClassDeclaration();

            var description = SyntaxFactory.Comment(
                @"// This code was generated by a kontent-generators-net tool 
// (see https://github.com/Kentico/kontent-generators-net).
// 
// Changes to this file may cause incorrect behavior and will be lost if the code is regenerated. 
// For further modifications of the class, create a separate file with the partial class." + Environment.NewLine + Environment.NewLine
            );

            var compilationUnit = SyntaxFactory.CompilationUnit()
                .AddUsings(usings)
                .AddMembers(
                    SyntaxFactory.NamespaceDeclaration(SyntaxFactory.IdentifierName(Namespace))
                        .AddMembers(classDeclaration)
                );

            if (!CustomPartial)
            {
                compilationUnit = compilationUnit.WithLeadingTrivia(description);
            }

            var customWorkspace = new AdhocWorkspace();
            return Formatter.Format(compilationUnit, customWorkspace).ToFullString().NormalizeLineEndings();
        }

        protected abstract UsingDirectiveSyntax[] GetApiUsings();

        protected virtual ClassDeclarationSyntax GetClassDeclaration() => SyntaxFactory.ClassDeclaration(ClassDefinition.ClassName)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword));

        protected static AccessorDeclarationSyntax GetAccessorDeclaration(SyntaxKind kind) =>
            SyntaxFactory.AccessorDeclaration(kind).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
    }
}
