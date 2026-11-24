namespace Xui.Compiler.WASM.Runtime;

/// <summary>Compiler-owned fields of a single-target delegate GC object.</summary>
internal enum DelegateField : uint
{
    /// <summary>The nominal type descriptor shared with other emitted classes.</summary>
    Descriptor,
    /// <summary>The retained instance or closure; null for a static target.</summary>
    Target,
    /// <summary>The bound callable's entry in the module's function table.</summary>
    Callable
}
