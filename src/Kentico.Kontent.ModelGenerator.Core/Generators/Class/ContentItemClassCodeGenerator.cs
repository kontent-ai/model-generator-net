using Kentico.Kontent.ModelGenerator.Core.Common;
using Kentico.Kontent.ModelGenerator.Core.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kentico.Kontent.ModelGenerator.Core.Generators.Class
{
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

        protected override UsingDirectiveSyntax[] GetApiUsings() => new UsingDirectiveSyntax[]
        {
            SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(typeof(Delivery.Abstractions.IApiResponse).Namespace!))
        };

        protected override TypeDeclarationSyntax GetClassDeclaration()
        {
            ClassDefinition.TryAddSystemProperty();

            var classDeclaration = SyntaxFactory.InterfaceDeclaration(ClassDefinition.Codename)
               .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
               .AddMembers(Properties);

            return classDeclaration;
        }

        protected override SyntaxTrivia ClassDescription() => ClassDeclarationHelper.GenerateSyntaxTrivia(
            @$"{LostChangesComment}
// Class is meant to represent common Content Item base, thus is not suitable for further modifications.
// If you require to extend all of your generated models you can use base classes see https://bit.ly/3yugE2z.");
    }
}
