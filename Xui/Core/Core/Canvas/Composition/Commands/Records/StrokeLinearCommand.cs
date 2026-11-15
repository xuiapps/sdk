using System.Runtime.InteropServices;
using Xui.Core.Math2D;

namespace Xui.Core.Canvas.Composition.Commands.Records;

/// <summary>Packed operands for the StrokeLinear canvas operation.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct StrokeLinearCommand
{
    /// <summary>The command discriminator, at byte offset zero.</summary>
    public CanvasOp Op;

    /// <summary>The StartPoint operand.</summary>
    public Point StartPoint;

    /// <summary>The EndPoint operand.</summary>
    public Point EndPoint;

    /// <summary>Number of GradientStop elements stored immediately after this record.</summary>
    public int Length;

    /// <summary>Initializes the discriminator and zeroes the operands.</summary>
    public StrokeLinearCommand() { this = default; Op = CanvasOp.StrokeLinear; }
}
