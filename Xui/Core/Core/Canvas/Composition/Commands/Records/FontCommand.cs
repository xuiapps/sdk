using System.Runtime.InteropServices;

namespace Xui.Core.Canvas.Composition.Commands.Records;

/// <summary>Packed operands for the Font canvas operation.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct FontCommand
{
    /// <summary>The command discriminator, at byte offset zero.</summary>
    public CanvasOp Op;

    /// <summary>The family-name string retained in the shared resource list.</summary>
    public ResourceRef Family;

    /// <summary>The font size.</summary>
    public nfloat Size;

    /// <summary>The font weight.</summary>
    public FontWeight Weight;

    /// <summary>The font style, stored inline without boxing.</summary>
    public FontStyle Style;

    /// <summary>The font stretch.</summary>
    public FontStretch Stretch;

    /// <summary>The line height.</summary>
    public nfloat LineHeight;

    /// <summary>Initializes the discriminator and zeroes the operands.</summary>
    public FontCommand() { this = default; Op = CanvasOp.Font; }
}
