﻿using Kontent.Ai.ModelGenerator.Core.Helpers;
using Microsoft.CodeAnalysis.CSharp;

namespace Kontent.Ai.ModelGenerator.Core.Tests.Helpers;

public class ClassDeclarationHelperTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void GenerateSyntaxTrivia_CustomCommentIsNullOrWhiteSpace_Throws(string customComment)
    {
        var call = () => ClassDeclarationHelper.GenerateSyntaxTrivia(customComment);

        call.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be(nameof(customComment));
    }

    [Theory]
    [InlineData("some comment")]
    [InlineData("//some comment")]
    public void GenerateSyntaxTrivia_CustomCommentIsNotComment_Throws(string customComment)
    {
        var call = () => ClassDeclarationHelper.GenerateSyntaxTrivia(customComment);

        call.Should()
            .Throw<ArgumentException>();
    }

    [Fact]
    public void GenerateSyntaxTrivia_Returns()
    {
        var customComment = "// custom comment";

        var expected = SyntaxFactory.Comment(@$"// <auto-generated>
// This code was generated by a kontent-generators-net tool
// (see https://github.com/kontent-ai/model-generator-net).
//
// custom comment
// </auto-generated>{Environment.NewLine}{Environment.NewLine}");

        var result = ClassDeclarationHelper.GenerateSyntaxTrivia(customComment);
        var isEqual = result.IsEquivalentTo(expected);

        isEqual.Should().BeTrue();
    }
}
