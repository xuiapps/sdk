namespace Xui.Compiler.Runtime;

/// <summary>Marks a narrow browser host operation in the explicitly selected runtime implementation.</summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class RuntimeImportAttribute(string module, string name) : Attribute
{
    public string Module { get; } = module;
    public string Name { get; } = name;
}
