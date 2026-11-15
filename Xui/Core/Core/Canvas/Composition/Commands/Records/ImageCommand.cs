using System.Runtime.InteropServices;
using Xui.Core.Math2D;

namespace Xui.Core.Canvas.Composition.Commands.Records;

/// <summary>Packed operands for the Image canvas operation.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ImageCommand
{
    /// <summary>The command discriminator, at byte offset zero.</summary>
    public CanvasOp Op;

    /// <summary>The image object retained in the shared resource list.</summary>
    public ResourceRef Image;

    /// <summary>The Destination operand.</summary>
    public Rect Destination;

    /// <summary>The Opacity operand.</summary>
    public nfloat Opacity;

    /// <summary>Initializes the discriminator and zeroes the operands.</summary>
    public ImageCommand() { this = default; Op = CanvasOp.Image; }
}
