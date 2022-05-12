using Kentico.Kontent.ModelGenerator.Core.Common;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kentico.Kontent.ModelGenerator.Core.Generators.Class
{
    public class ExtendedDeliveryClassCodeGenerator : DeliveryClassCodeGenerator
    {
        private readonly string BaseTypeName;

        public ExtendedDeliveryClassCodeGenerator(ClassDefinition classDefinition, string classFilename, string baseTypeName, string @namespace = DefaultNamespace)
            : base(classDefinition, classFilename, @namespace)
        {
            BaseTypeName = baseTypeName;
        }

        private ExtendedDeliveryClassCodeGenerator(ClassDefinition classDefinition, string classFilename, string @namespace = DefaultNamespace)
            : base(classDefinition, classFilename, @namespace)
        {
        }

        protected override ClassDeclarationSyntax GetClassDeclaration()
        {
            var classDeclaration = base.GetClassDeclaration();

            var baseType = SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(BaseTypeName));
            classDeclaration = classDeclaration.AddBaseListTypes(baseType);

            return classDeclaration;
        }
    }
}
