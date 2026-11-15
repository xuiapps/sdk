using System.Runtime.InteropServices;
using Xui.Core.Math2D;

namespace Xui.Core.Canvas.Composition.Commands.Records;

/// <summary>Draws a string retained by reference in the shared resource list.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct TextResourceCommand
{
    /// <summary>The command discriminator, at byte offset zero.</summary>
    public CanvasOp Op;

    /// <summary>The text drawing position.</summary>
    public Point Position;

    /// <summary>The retained string. Span-based text uses TextCommand instead.</summary>
    public ResourceRef Text;

    /// <summary>Initializes the discriminator and zeroes the operands.</summary>
    public TextResourceCommand() { this = default; Op = CanvasOp.TextResource; }
}
