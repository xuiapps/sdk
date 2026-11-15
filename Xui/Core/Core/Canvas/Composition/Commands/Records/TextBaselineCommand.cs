using System.Runtime.InteropServices;

namespace Xui.Core.Canvas.Composition.Commands.Records;

/// <summary>Packed operands for the TextBaseline canvas operation.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct TextBaselineCommand
{
    /// <summary>The command discriminator, at byte offset zero.</summary>
    public CanvasOp Op;

    /// <summary>The Value operand.</summary>
    public TextBaseline Value;

    /// <summary>Initializes the discriminator and zeroes the operands.</summary>
    public TextBaselineCommand() { this = default; Op = CanvasOp.TextBaseline; }
}
