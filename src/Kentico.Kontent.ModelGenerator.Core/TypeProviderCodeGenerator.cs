using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kentico.Kontent.Delivery.Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace Kentico.Kontent.ModelGenerator.Core
{
    public class TypeProviderCodeGenerator
    {
        public const string CLASS_NAME = "CustomTypeProvider";

        /// <summary>
        /// Codename -> ClassName dictionary
        /// </summary>
        private readonly Dictionary<string, string> _contentTypes = new Dictionary<string, string>();

        private readonly string _namespace;

        public TypeProviderCodeGenerator(string @namespace = ClassCodeGenerator.DEFAULT_NAMESPACE)
        {
            if (string.IsNullOrEmpty(@namespace))
            {
                @namespace = ClassCodeGenerator.DEFAULT_NAMESPACE;
            }

            _namespace = @namespace;
        }

        public void AddContentType(string codename, string className)
        {
            if (string.IsNullOrEmpty(codename))
            {
                throw new ArgumentException("Codename must be a non empty string", nameof(codename));
            }

            if (string.IsNullOrEmpty(className))
            {
                throw new ArgumentException("Class name must be a non empty string", nameof(className));
            }

            _contentTypes[codename] = className;
        }

        public string GenerateCode()
        {
            if (!_contentTypes.Any())
            {
                return null;
            }

            var tree = CSharpSyntaxTree.ParseText(
                $@"using System;
using System.Collections.Generic;
using System.Linq;
using {typeof(ITypeProvider).Namespace};

namespace {_namespace}
{{
    public class {CLASS_NAME} : ITypeProvider
    {{
        protected static readonly Dictionary<Type, string> _codenames = new Dictionary<Type, string>
        {{
{CreateCodenameDictionaryValues()}
        }};

        public virtual Type GetType(string contentType)
        {{
            return _codenames.Keys.FirstOrDefault(type => GetCodename(type).Equals(contentType));
        }}

        public virtual string GetCodename(Type contentType)
        {{
            return _codenames.TryGetValue(contentType, out var codename) ? codename : null;
        }}
    }}
}}");

            var cu = (CompilationUnitSyntax)tree.GetRoot();

            AdhocWorkspace cw = new AdhocWorkspace();
            return Formatter.Format(cu, cw).ToFullString().NormalizeLineEndings();
        }

        private string CreateCodenameDictionaryValues()
        {
            if (_contentTypes.Count == 0) return null;

            var dictionaryValuesBuilder = new StringBuilder();

            foreach (var entry in _contentTypes.Take(_contentTypes.Count - 1))
            {
                dictionaryValuesBuilder.AppendLine($"\t\t\t{{typeof({entry.Value}), \"{entry.Key}\"}},");
            }

            var lastEntry = _contentTypes.Last();
            dictionaryValuesBuilder
                .Append($"\t\t\t{{typeof({lastEntry.Value}), \"{lastEntry.Key}\"}}");

            return dictionaryValuesBuilder.ToString();
        }
    }
}