using Kentico.Kontent.ModelGenerator.Core.Common;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kentico.Kontent.ModelGenerator.Core.Generators.Class
{
    public class DeliveryClassCodeGenerator : DeliveryClassCodeGeneratorBase
    {
        public DeliveryClassCodeGenerator(ClassDefinition classDefinition, string classFilename, string @namespace = DefaultNamespace)
            : base(classDefinition, classFilename, @namespace)
        {
        }

        protected override ClassDeclarationSyntax GetClassDeclaration()
        {
            var classDeclaration = base.GetClassDeclaration();

            classDeclaration = classDeclaration.AddMembers(ClassCodenameConstant);
            classDeclaration = classDeclaration.AddMembers(PropertyCodenameConstants);
            classDeclaration = classDeclaration.AddMembers(Properties);

            return classDeclaration;
        }

        protected override UsingDirectiveSyntax[] GetApiUsings() => new[]
        {
            SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(nameof(System))),
            SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(typeof(System.Collections.Generic.IEnumerable<>).Namespace!)),
            SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(typeof(Delivery.Abstractions.IApiResponse).Namespace!))
        };

        private FieldDeclarationSyntax ClassCodenameConstant => GetFieldDeclaration("string", "Codename", ClassDefinition.Codename);
    }
}
