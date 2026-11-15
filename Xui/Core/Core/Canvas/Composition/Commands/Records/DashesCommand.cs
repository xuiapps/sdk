using System.Runtime.InteropServices;

namespace Xui.Core.Canvas.Composition.Commands.Records;

/// <summary>Packed operands for the Dashes canvas operation.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct DashesCommand
{
    /// <summary>The command discriminator, at byte offset zero.</summary>
    public CanvasOp Op;

    /// <summary>Number of nfloat elements stored immediately after this record.</summary>
    public int Length;

    /// <summary>Initializes the discriminator and zeroes the operands.</summary>
    public DashesCommand() { this = default; Op = CanvasOp.Dashes; }
}
