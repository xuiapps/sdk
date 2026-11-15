using System.Runtime.InteropServices;
using Xui.Core.Math2D;

namespace Xui.Core.Canvas.Composition.Commands.Records;

/// <summary>Packed operands for the LineTo canvas operation.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct LineToCommand
{
    /// <summary>The command discriminator, at byte offset zero.</summary>
    public CanvasOp Op;

    /// <summary>The Position operand.</summary>
    public Point Position;

    /// <summary>Initializes the discriminator and zeroes the operands.</summary>
    public LineToCommand() { this = default; Op = CanvasOp.LineTo; }
}
