using System.Runtime.InteropServices;

namespace Xui.Core.Canvas.Composition.Commands.Records;

/// <summary>Packed operands for the Brightness canvas operation.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct BrightnessCommand
{
    /// <summary>The command discriminator, at byte offset zero.</summary>
    public CanvasOp Op;

    /// <summary>The Amount operand.</summary>
    public nfloat Amount;

    /// <summary>Initializes the discriminator and zeroes the operands.</summary>
    public BrightnessCommand() { this = default; Op = CanvasOp.Brightness; }
}
