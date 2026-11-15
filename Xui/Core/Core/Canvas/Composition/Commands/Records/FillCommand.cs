using System.Runtime.InteropServices;

namespace Xui.Core.Canvas.Composition.Commands.Records;

/// <summary>Packed operands for the Fill canvas operation.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct FillCommand
{
    /// <summary>The command discriminator, at byte offset zero.</summary>
    public CanvasOp Op;

    /// <summary>The Rule operand.</summary>
    public FillRule Rule;

    /// <summary>Initializes the discriminator and zeroes the operands.</summary>
    public FillCommand() { this = default; Op = CanvasOp.Fill; }
}
