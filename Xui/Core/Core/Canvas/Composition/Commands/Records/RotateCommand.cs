using System.Runtime.InteropServices;

namespace Xui.Core.Canvas.Composition.Commands.Records;

/// <summary>Packed operands for the Rotate canvas operation.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct RotateCommand
{
    /// <summary>The command discriminator, at byte offset zero.</summary>
    public CanvasOp Op;

    /// <summary>The Angle operand.</summary>
    public nfloat Angle;

    /// <summary>Initializes the discriminator and zeroes the operands.</summary>
    public RotateCommand() { this = default; Op = CanvasOp.Rotate; }
}
