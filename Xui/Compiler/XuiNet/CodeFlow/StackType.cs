namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>Normalized MSIL evaluation-stack types supported by the first analysis pass.</summary>
/// <remarks>Small integers share Int32; float and double share Float. These are types, not runtime values.</remarks>
public enum StackType
{
    /// <summary>A reference to a concrete class.</summary>
    ObjectReference,
    Int32,
    Int64,
    NativeInt,
    /// <summary>A callable method token used to construct a delegate.</summary>
    MethodPointer,
    /// <summary>A transient closed type token consumed by Type.GetTypeFromHandle.</summary>
    RuntimeTypeHandle,
    /// <summary>A transient field token consumed by RuntimeHelpers.CreateSpan.</summary>
    RuntimeFieldHandle,
    Float,
    String,
    /// <summary>A managed vector with its exact element signature.</summary>
    ArrayReference,
    /// <summary>A named value type; StackEntry retains its module-qualified identity.</summary>
    ValueType,
    /// <summary>A managed address; StackEntry retains its exact declared element type.</summary>
    ManagedPointer,
    /// <summary>An untyped null reference before assignment or conversion.</summary>
    Null
}
