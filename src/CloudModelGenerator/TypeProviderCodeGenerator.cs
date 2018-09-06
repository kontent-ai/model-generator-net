using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CloudModelGenerator
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
            if (String.IsNullOrEmpty(codename))
            {
                throw new ArgumentException("Codename must be a non empty string", nameof(codename));
            }

            if (String.IsNullOrEmpty(className))
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

            var codenameDictionaryValues = _contentTypes
                .Select(entry => $"\t\t\t{{typeof({entry.Value}), \"{entry.Key}\"}}")
                .Aggregate((previous, next) => previous + "," + Environment.NewLine + next);

            var tree = CSharpSyntaxTree.ParseText(
$@"using System;
using System.Collections.Generic;
using System.Linq;
using KenticoCloud.Delivery;

namespace {_namespace}
{{
    public class {CLASS_NAME} : ICodeFirstTypeProvider
    {{
        private static readonly Dictionary<Type, string> _codenames = new Dictionary<Type, string>
        {{
{codenameDictionaryValues}
        }};

        public Type GetType(string contentType)
        {{
            return _codenames.Keys.FirstOrDefault(type => GetCodename(type).Equals(contentType));
        }}

        public string GetCodename(Type contentType)
        {{
            return _codenames.TryGetValue(contentType, out var codename) ? codename : null;
        }}
    }}
}}");

            var cu = (CompilationUnitSyntax)tree.GetRoot();

            AdhocWorkspace cw = new AdhocWorkspace();
            return Formatter.Format(cu, cw).ToFullString();
        }
    }
}
