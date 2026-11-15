using System.Runtime.InteropServices;

namespace Xui.Core.Canvas.Composition.Commands.Records;

/// <summary>Packed operands for the FillColor canvas operation.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct FillColorCommand
{
    /// <summary>The command discriminator, at byte offset zero.</summary>
    public CanvasOp Op;

    /// <summary>The Color operand.</summary>
    public Color Color;

    /// <summary>Initializes the discriminator and zeroes the operands.</summary>
    public FillColorCommand() { this = default; Op = CanvasOp.FillColor; }
}
