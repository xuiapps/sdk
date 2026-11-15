using System.Runtime.InteropServices;

namespace Xui.Core.Canvas.Composition.Commands.Records;

/// <summary>Packed operands for the StrokePattern canvas operation.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct StrokePatternCommand
{
    /// <summary>The command discriminator, at byte offset zero.</summary>
    public CanvasOp Op;

    /// <summary>The image object retained in the shared resource list.</summary>
    public ResourceRef Image;

    /// <summary>The Repeat operand.</summary>
    public PatternRepeat Repeat;

    /// <summary>Initializes the discriminator and zeroes the operands.</summary>
    public StrokePatternCommand() { this = default; Op = CanvasOp.StrokePattern; }
}
