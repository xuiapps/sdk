using System.Runtime.InteropServices;
using Xui.Core.Math2D;

namespace Xui.Core.Canvas.Composition.Commands.Records;

/// <summary>Packed operands for the Translate canvas operation.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct TranslateCommand
{
    /// <summary>The command discriminator, at byte offset zero.</summary>
    public CanvasOp Op;

    /// <summary>The Value operand.</summary>
    public Vector Value;

    /// <summary>Initializes the discriminator and zeroes the operands.</summary>
    public TranslateCommand() { this = default; Op = CanvasOp.Translate; }
}
