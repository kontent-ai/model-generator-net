using System;
using System.Reflection;

namespace Kontent.Ai.ModelGenerator;

internal class UsedSdkInfo
{
    private const int SemanticVersionFieldCount = 3;

    public string Name { get; }
    public string Version { get; }

    public UsedSdkInfo(Type type, string name)
    {
        Version = type == null
            ? throw new ArgumentNullException(nameof(type))
            : Assembly.GetAssembly(type).GetName().Version.ToString(SemanticVersionFieldCount);
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }
}
