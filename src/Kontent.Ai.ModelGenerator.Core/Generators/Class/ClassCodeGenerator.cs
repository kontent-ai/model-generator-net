﻿using System;
using System.Linq;
using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace Kontent.Ai.ModelGenerator.Core.Generators.Class;

public abstract class ClassCodeGenerator : GeneralGenerator
{
    public const string DefaultNamespace = "KontentAiModels";

    public ClassDefinition ClassDefinition { get; }

    public string ClassFilename { get; }

    protected ClassCodeGenerator(ClassDefinition classDefinition, string classFilename, string @namespace = DefaultNamespace) : base(@namespace)
    {
        ClassDefinition = classDefinition ?? throw new ArgumentNullException(nameof(classDefinition));
        ClassFilename = string.IsNullOrWhiteSpace(classFilename) ? ClassDefinition.ClassName : classFilename;
    }

    public bool OverwriteExisting => GetType() != typeof(PartialClassCodeGenerator);

    public string GenerateCode()
    {
        var usings = GetApiUsings();
        var classDeclaration = GetClassDeclaration();

        var compilationUnit = GetCompilationUnit(classDeclaration, usings);

        var customWorkspace = new AdhocWorkspace();
        return Formatter.Format(compilationUnit, customWorkspace).ToFullString().NormalizeLineEndings();
    }

    protected abstract UsingDirectiveSyntax[] GetApiUsings();

    protected virtual MemberDeclarationSyntax[] Properties
        => ClassDefinition.Properties.OrderBy(p => p.Identifier).Select(element => SyntaxFactory
            .PropertyDeclaration(SyntaxFactory.ParseTypeName(element.TypeName), element.Identifier)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddAccessorListAccessors(
                GetAccessorDeclaration(SyntaxKind.GetAccessorDeclaration),
                GetAccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
            )).ToArray<MemberDeclarationSyntax>();

    protected virtual TypeDeclarationSyntax GetClassDeclaration() => SyntaxFactory.ClassDeclaration(ClassDefinition.ClassName)
        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword));

    protected override SyntaxTrivia ClassDescription() => ClassDeclarationHelper.GenerateSyntaxTrivia(
        @$"{LostChangesComment}
// For further modifications of the class, create a separate file with the partial class.");

    protected static AccessorDeclarationSyntax GetAccessorDeclaration(SyntaxKind kind) =>
        SyntaxFactory.AccessorDeclaration(kind).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

    private CompilationUnitSyntax GetCompilationUnit(TypeDeclarationSyntax classDeclaration, UsingDirectiveSyntax[] usings)
    {
        var compilationUnit = SyntaxFactory.CompilationUnit()
            .AddUsings(usings)
            .AddMembers(
                SyntaxFactory.NamespaceDeclaration(SyntaxFactory.IdentifierName(Namespace))
                    .AddMembers(classDeclaration));

        compilationUnit = compilationUnit.WithLeadingTrivia(ClassDescription());

        return compilationUnit;
    }
}
