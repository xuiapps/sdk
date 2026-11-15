using System.Runtime.InteropServices;

namespace Xui.Core.Canvas.Composition.Commands.Records;

/// <summary>Packed operands for the TextAlign canvas operation.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct TextAlignCommand
{
    /// <summary>The command discriminator, at byte offset zero.</summary>
    public CanvasOp Op;

    /// <summary>The Value operand.</summary>
    public TextAlign Value;

    /// <summary>Initializes the discriminator and zeroes the operands.</summary>
    public TextAlignCommand() { this = default; Op = CanvasOp.TextAlign; }
}
