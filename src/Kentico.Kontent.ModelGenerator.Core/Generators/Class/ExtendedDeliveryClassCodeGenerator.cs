using Kentico.Kontent.ModelGenerator.Core.Common;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kentico.Kontent.ModelGenerator.Core.Generators.Class
{
    public class ExtendedDeliveryClassCodeGenerator : DeliveryClassCodeGenerator
    {
        public ExtendedDeliveryClassCodeGenerator(ClassDefinition classDefinition, string classFilename, string @namespace = DefaultNamespace)
            : base(classDefinition, classFilename, @namespace)
        {
        }

        protected override ClassDeclarationSyntax GetClassDeclaration()
        {
            var classDeclaration = base.GetClassDeclaration();

            var baseType = SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(ContentItemClassCodeGenerator.DefaultContentItemClassName));
            classDeclaration = classDeclaration.AddBaseListTypes(baseType);

            return classDeclaration;
        }
    }
}
