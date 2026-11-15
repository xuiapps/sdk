using System.Runtime.InteropServices;

namespace Xui.Core.Canvas.Composition.Commands.Records;

/// <summary>Packed operands for the Restore canvas operation.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct RestoreCommand
{
    /// <summary>The command discriminator, at byte offset zero.</summary>
    public CanvasOp Op;

    /// <summary>Initializes the discriminator and zeroes the operands.</summary>
    public RestoreCommand() { this = default; Op = CanvasOp.Restore; }
}
