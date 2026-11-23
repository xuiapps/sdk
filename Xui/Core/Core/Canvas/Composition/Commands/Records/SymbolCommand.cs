using System.Runtime.InteropServices;
namespace Xui.Core.Canvas.Composition.Commands.Records;
/// <summary>A retained reference to an immutable symbol definition.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SymbolCommand
{
    /// <summary>Operation discriminator.</summary>
    public CanvasOp Op;
    /// <summary>SymbolContent in the buffer's resource table.</summary>
    public ResourceRef Content;
    /// <summary>Initializes the operation.</summary>
    public SymbolCommand() { this=default; Op=CanvasOp.Symbol; }
}
