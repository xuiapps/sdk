using System.Runtime.InteropServices;
using Xui.Core.Math2D;

namespace Xui.Core.Canvas.Composition.Commands.Records;

/// <summary>Draws text copied from a span into the binary stream immediately after this record.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct TextCommand
{
    /// <summary>The command discriminator, at byte offset zero.</summary>
    public CanvasOp Op;

    /// <summary>The Position operand.</summary>
    public Point Position;

    /// <summary>Number of char elements stored immediately after this record.</summary>
    public int Length;

    /// <summary>Initializes the discriminator and zeroes the operands.</summary>
    public TextCommand() { this = default; Op = CanvasOp.Text; }
}
