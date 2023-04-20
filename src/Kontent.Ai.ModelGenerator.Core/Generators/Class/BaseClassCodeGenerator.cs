using System;
using System.Collections.Generic;
using System.Linq;
using Kontent.Ai.ModelGenerator.Core.Configuration;
using Kontent.Ai.ModelGenerator.Core.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace Kontent.Ai.ModelGenerator.Core.Generators.Class;

public class BaseClassCodeGenerator : GeneralGenerator
{
    /// <summary>
    /// Collection of classes to extend (HashSet ensures that classes get extended only once)
    /// </summary>
    private readonly ICollection<string> _classesToExtend = new HashSet<string>();

    private readonly CodeGeneratorOptions _options;

    /// <summary>
    /// The calculated Extender Classname
    /// </summary>
    public string ExtenderClassName => $"{_options.BaseClass}Extender";

    public BaseClassCodeGenerator(CodeGeneratorOptions options) : base(options.Namespace)
    {
        _options = options;
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
using Kontent.Ai.Delivery.Abstractions;

namespace {Namespace}
{{
    public partial class {_options.BaseClass}
    {{
        // This class can be used to extend the generated classes. They inherit from this type in {ExtenderClassName}.cs.
    }}
}}");

        var cu = (CompilationUnitSyntax)tree.GetRoot().NormalizeWhitespace();
        cu = cu.WithLeadingTrivia(ClassDescription());

        AdhocWorkspace cw = new AdhocWorkspace();
        return Formatter.Format(cu, cw).ToFullString().NormalizeLineEndings();
    }

    /// <summary>
    /// Creates the extender code that uses partials to make all output classes derive from the base class
    /// </summary>
    public string GenerateExtenderCode()
    {
        var extenders = _classesToExtend.OrderBy(c => c)
            .Select((c) => $"public partial class {c} : {_options.BaseClass} {{ }}")
            .Aggregate((p, n) => p + Environment.NewLine + n);

        var tree = CSharpSyntaxTree.ParseText(
            $@"using System;
using Kontent.Ai.Delivery.Abstractions;

namespace {Namespace}
{{
        // These classes extend the generated models to all inherit from the common basetype {_options.BaseClass}.

        {extenders}
}}");

        var cu = (CompilationUnitSyntax)tree.GetRoot().NormalizeWhitespace();
        cu = cu.WithLeadingTrivia(ExtenderClassDescription);

        AdhocWorkspace cw = new AdhocWorkspace();
        return Formatter.Format(cu, cw).ToFullString().NormalizeLineEndings();
    }

    protected override SyntaxTrivia ClassDescription() =>
        ClassDeclarationHelper.GenerateSyntaxTrivia(@"// You can make changes to this class and it will not get overwritten if it already exists.");

    private SyntaxTrivia ExtenderClassDescription => ClassDeclarationHelper.GenerateSyntaxTrivia(
        @$"{LostChangesComment}
// For further modifications of the class, create or modify the '{_options.BaseClass}.cs' file with the partial class.");
}
