using Kentico.Kontent.ModelGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kentico.Kontent.ModelGenerator
{
    public class BaseClassCodeGenerator
    {
        /// <summary>
        /// List of classes to extend
        /// </summary>
        private readonly ICollection<string> _classesToExtend = new List<string>();

        private readonly string _namespace;
        private readonly string _className;

        /// <summary>
        /// The calculated Extender Classname
        /// </summary>
        public string ExtenderClassName
        {
            get
            {
                return $"{_className}Extender";
            }
        }

        public BaseClassCodeGenerator(string className, string @namespace = ClassCodeGenerator.DEFAULT_NAMESPACE)
        {
            _className = className;

            if (string.IsNullOrEmpty(@namespace))
            {
                @namespace = ClassCodeGenerator.DEFAULT_NAMESPACE;
            }

            _namespace = @namespace;
        }

        /// <summary>
        /// Add string values for classes that should be added as partials so they inherit from the base class
        /// </summary>
        /// <param name="className"></param>
        public void AddClassNameToExtend(string className)
        {
            if (string.IsNullOrEmpty(className))
            {
                throw new ArgumentException("Class name must be a non empty string", nameof(className));
            }

            _classesToExtend.Add(className);
        }

        /// <summary>
        /// Creates the base class output
        /// </summary>
        /// <returns></returns>
        public string GenerateBaseClassCode()
        {
            var tree = CSharpSyntaxTree.ParseText(
$@"using System;
using Kentico.Kontent.Delivery;

namespace {_namespace}
{{
    public partial class {_className}
    {{
        // This class can be used to extend the generated classes. They inherit from this type in {ExtenderClassName}.cs.
    }}
}}");

            var cu = (CompilationUnitSyntax)tree.GetRoot().NormalizeWhitespace();

            AdhocWorkspace cw = new AdhocWorkspace();
            return Formatter.Format(cu, cw).ToFullString();
        }

        /// <summary>
        /// Creates the extender code that uses partials to make all output classes derive from the base class
        /// </summary>
        public string GenereateExtenderCode()
        {

            string extenders = _classesToExtend.OrderBy(c => c)
                .Select((c) => $"public partial class {c} : {_className} {{ }}")
                .Aggregate((p, n) => p + Environment.NewLine + n);

            var tree = CSharpSyntaxTree.ParseText(
$@"using System;
using Kentico.Kontent.Delivery;

namespace {_namespace}
{{
        // These classes extend the generated models to all inherit from the common basetype {_className}.

        {extenders}
}}");

            var cu = (CompilationUnitSyntax)tree.GetRoot().NormalizeWhitespace();

            AdhocWorkspace cw = new AdhocWorkspace();
            return Formatter.Format(cu, cw).ToFullString();
        }
    }
}
