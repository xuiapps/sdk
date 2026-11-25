namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>A proven replacement that leaves the original IL offsets and operands intact.</summary>
public readonly record struct InstructionSpecialization(
    SpecializationKind Kind,
    int End,
    int Value = 0,
    string? Intrinsic = null);

/// <summary>Stack effects used by the closed-type specialization pass.</summary>
public enum SpecializationKind
{
    /// <summary>A type-token producer consumed entirely at compile time.</summary>
    Omit,
    /// <summary>A metadata query replaced by an Int32 constant.</summary>
    Int32,
    /// <summary>A one-operand conditional whose selected successor is known; consume its condition.</summary>
    Branch
}
