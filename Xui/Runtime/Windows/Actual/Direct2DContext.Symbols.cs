using System;
using System.Runtime.CompilerServices;
using Xui.Core.Canvas;
using Xui.Core.Canvas.Symbols;
using static Xui.Runtime.Windows.D2D1;

namespace Xui.Runtime.Windows.Actual;

public partial class Direct2DContext
{
    private readonly ConditionalWeakTable<SymbolContent, CommandList> vectorSymbols = new();

    public void DrawSymbol(SymbolContent content)
    {
        if (RenderTarget is not DeviceContext deviceContext || filter.Amount != 1)
        { content.Replay(this); return; }
        if (!vectorSymbols.TryGetValue(content, out var list))
        {
            using var device = deviceContext.GetDevice();
            using var recording = device.CreateDeviceContext(DeviceContextOptions.None);
            list = recording.CreateCommandList();
            var drawing = new Direct2DContext(recording, D2D1Factory, DWriteFactory);
            try
            {
                recording.SetImageTarget(list);
                recording.BeginDraw();
                try { content.Replay(drawing); }
                finally { drawing.EndDraw(); recording.EndDraw(); recording.SetImageTarget(null); }
                list.Close();
                vectorSymbols.Add(content, list);
            }
            catch { list.Dispose(); throw; }
            finally { drawing.ReleaseSymbolRecorder(); }
        }
        deviceContext.DrawImage(list);
        GC.KeepAlive(list);
        ((IContext)this).BeginPath();
    }

    // Ordinary Direct2DContext.Dispose currently closes a borrowed drawing scope. This
    // private recording context instead owns all of its COM references and releases them.
    private void ReleaseSymbolRecorder()
    {
        fill.Dispose(); stroke.Dispose(); textFormat.Dispose();
        Path.Dispose(); StrokeStyle.Dispose();
        RenderTarget.Release(); D2D1Factory.Release(); DWriteFactory.Release();
        GC.SuppressFinalize(this);
    }
}
