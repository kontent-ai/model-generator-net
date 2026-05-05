using System;
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

    /// <summary>
    /// Determines if this generator creates a record instead of a class.
    /// </summary>
    protected virtual bool IsRecord => false;

    /// <summary>
    /// Determines if this generator uses file-scoped namespace.
    /// </summary>
    protected virtual bool UseFileScopedNamespace => false;

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
        => ClassDefinition.Properties.OrderBy(p => p.Identifier).Select(element =>
        {
            var property = SyntaxFactory
                .PropertyDeclaration(SyntaxFactory.ParseTypeName(element.TypeName), element.Identifier)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            // Add JSON attribute with property name
            property = property.AddAttributeLists(
                SyntaxFactory.AttributeList(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Attribute(
                            SyntaxFactory.IdentifierName("JsonPropertyName"),
                            SyntaxFactory.AttributeArgumentList(
                                SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.AttributeArgument(
                                        SyntaxFactory.LiteralExpression(
                                            SyntaxKind.StringLiteralExpression,
                                            SyntaxFactory.Literal(element.Codename)))))))));

            // Add accessor list (init instead of set for records/modern delivery)
            if (IsRecord)
            {
                // Records need { get; init; }
                property = property.AddAccessorListAccessors(
                    GetAccessorDeclaration(SyntaxKind.GetAccessorDeclaration),
                    GetAccessorDeclaration(SyntaxKind.InitAccessorDeclaration));

                // Emit explicit initializer (e.g. = string.Empty / = [] / = RichTextContent.Empty)
                // when the Property carries one. Used by Semantic nullability mode.
                if (element.HasInitializer)
                {
                    property = property.WithInitializer(
                        SyntaxFactory.EqualsValueClause(SyntaxFactory.ParseExpression(element.Initializer)))
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
                }
            }
            else
            {
                property = property.AddAccessorListAccessors(
                    GetAccessorDeclaration(SyntaxKind.GetAccessorDeclaration),
                    GetAccessorDeclaration(SyntaxKind.SetAccessorDeclaration));
            }

            return property;
        }).ToArray<MemberDeclarationSyntax>();

    protected virtual TypeDeclarationSyntax GetClassDeclaration()
    {
        TypeDeclarationSyntax typeDeclaration;

        if (IsRecord)
        {
            // Create record declaration with proper braces
            typeDeclaration = SyntaxFactory.RecordDeclaration(
                attributeLists: default,
                modifiers: default,
                keyword: SyntaxFactory.Token(SyntaxKind.RecordKeyword),
                identifier: SyntaxFactory.Identifier(ClassDefinition.ClassName),
                typeParameterList: null,
                parameterList: null,
                baseList: null,
                constraintClauses: default,
                openBraceToken: SyntaxFactory.Token(SyntaxKind.OpenBraceToken),
                members: default,
                closeBraceToken: SyntaxFactory.Token(SyntaxKind.CloseBraceToken),
                semicolonToken: default);
        }
        else
        {
            typeDeclaration = SyntaxFactory.ClassDeclaration(ClassDefinition.ClassName);
        }

        return typeDeclaration
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword));
    }

    protected override SyntaxTrivia ClassDescription()
    {
        var typeWord = IsRecord ? "record" : "class";
        return ClassDeclarationHelper.GenerateSyntaxTrivia(
            @$"{LostChangesComment}
// To extend this {typeWord}, create a separate partial {typeWord} with the same name.");
    }

    protected static AccessorDeclarationSyntax GetAccessorDeclaration(SyntaxKind kind) =>
        SyntaxFactory.AccessorDeclaration(kind).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

    private CompilationUnitSyntax GetCompilationUnit(TypeDeclarationSyntax classDeclaration, UsingDirectiveSyntax[] usings)
    {
        CompilationUnitSyntax compilationUnit;

        if (UseFileScopedNamespace)
        {
            // File-scoped namespace: namespace Foo;
            var fileScopedNamespace = SyntaxFactory.FileScopedNamespaceDeclaration(
                SyntaxFactory.IdentifierName(Namespace))
                .AddMembers(classDeclaration);

            compilationUnit = SyntaxFactory.CompilationUnit()
                .AddUsings(usings)
                .AddMembers(fileScopedNamespace);
        }
        else
        {
            // Traditional namespace: namespace Foo { }
            compilationUnit = SyntaxFactory.CompilationUnit()
                .AddUsings(usings)
                .AddMembers(
                    SyntaxFactory.NamespaceDeclaration(SyntaxFactory.IdentifierName(Namespace))
                        .AddMembers(classDeclaration));
        }

        var leadingTrivia = SyntaxFactory.TriviaList(
            ClassDescription(),
            SyntaxFactory.Trivia(SyntaxFactory.NullableDirectiveTrivia(
                SyntaxFactory.Token(SyntaxKind.EnableKeyword), isActive: true)),
            SyntaxFactory.CarriageReturnLineFeed,
            SyntaxFactory.CarriageReturnLineFeed);

        compilationUnit = compilationUnit.WithLeadingTrivia(leadingTrivia);

        return compilationUnit;
    }
}
