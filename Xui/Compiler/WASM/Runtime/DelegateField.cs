namespace Xui.Compiler.WASM.Runtime;

/// <summary>Compiler-owned fields of a delegate GC object.</summary>
internal enum DelegateField : uint
{
    /// <summary>The nominal type descriptor shared with other emitted classes.</summary>
    Descriptor,
    /// <summary>The retained instance or closure; null for a static target.</summary>
    Target,
    /// <summary>The bound callable's entry in the module's function table.</summary>
    Callable,
    /// <summary>Canonical emitted method identity, shared by direct and virtual bindings.</summary>
    Method,
    /// <summary>Null for a single target, otherwise a flat immutable invocation list.</summary>
    InvocationList
}
