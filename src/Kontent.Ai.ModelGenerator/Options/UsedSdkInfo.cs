using System;
using System.Reflection;

namespace Kontent.Ai.ModelGenerator.Options;

internal class UsedSdkInfo(Type type, string name)
{
    private const int SemanticVersionFieldCount = 3;

    public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));
    public string Version { get; } = type == null
            ? throw new ArgumentNullException(nameof(type))
            : Assembly.GetAssembly(type).GetName().Version.ToString(SemanticVersionFieldCount);
}
