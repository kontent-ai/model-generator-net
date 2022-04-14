using Kentico.Kontent.ModelGenerator.Core.Generators.Class;
using Microsoft.CodeAnalysis;

namespace Kentico.Kontent.ModelGenerator.Core.Generators
{
    public abstract class GeneralGenerator
    {
        public readonly string Namespace;

        protected GeneralGenerator(string @namespace = ClassCodeGenerator.DefaultNamespace)
        {
            Namespace = string.IsNullOrWhiteSpace(@namespace) ? ClassCodeGenerator.DefaultNamespace : @namespace;
        }

        protected abstract SyntaxTrivia ClassDescription();
    }
}
