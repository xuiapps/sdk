namespace Xui.Compiler.Runtime.Reflection;

/// <summary>Canonical assembly identity for packaged manifest-resource lookup.</summary>
public class Assembly
{
    private static Assembly? first;
    private readonly string name;
    private readonly Assembly? next;
    private Assembly(string name) { this.name = name; next = first; first = this; }
    public static Assembly FromName(string name)
    {
        for (var current = first; current is not null; current = current.next)
            if (current.name == name) return current;
        return new Assembly(name);
    }
    public IO.Stream? GetManifestResourceStream(string name) => IO.ResourceStream.Open(this.name, name);
}
