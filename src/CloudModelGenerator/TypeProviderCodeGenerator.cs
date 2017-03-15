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

        // Codename -> ClassName dictionary
        private readonly Dictionary<string, string> _contentTypes = new Dictionary<string, string>();
        private readonly string _namespace;

        public TypeProviderCodeGenerator(string @namespace = ClassCodeGenerator.DEFAULT_NAMESPACE)
        {
            if (@namespace == String.Empty)
            {
                throw new ArgumentException("Namespace can't be empty string", nameof(@namespace));
            }

            _namespace = @namespace ?? ClassCodeGenerator.DEFAULT_NAMESPACE;
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

            _contentTypes.Add(codename, className);
        }

        public string GenerateCode()
        {
            string switchCases = _contentTypes
                .Select((c) => $"case \"{c.Key}\": return typeof({c.Value});")
                .Aggregate((p, n) => p + Environment.NewLine + n);
            
            var tree = CSharpSyntaxTree.ParseText(
$@"using System;
using KenticoCloud.Delivery;

namespace {_namespace}
{{
    public class {CLASS_NAME} : ICodeFirstTypeProvider
    {{
        public Type GetType(string contentType)
        {{
            switch (contentType)
            {{
                {switchCases}
                default:
                    return null;
            }}
        }}
    }}
}}");

            var cu = (CompilationUnitSyntax)tree.GetRoot().NormalizeWhitespace();

            AdhocWorkspace cw = new AdhocWorkspace();
            return Formatter.Format(cu, cw).ToFullString();
        }
    }
}
