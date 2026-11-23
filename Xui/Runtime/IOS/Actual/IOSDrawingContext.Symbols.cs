using System;
using System.Runtime.CompilerServices;
using Xui.Core.Canvas;
using Xui.Core.Canvas.Symbols;
using static Xui.Runtime.IOS.CoreGraphics;

namespace Xui.Runtime.IOS.Actual;

public partial class IOSDrawingContext
{
    private readonly ConditionalWeakTable<SymbolContent, CGPdfSymbol> vectorSymbols = new();

    /// <summary>Compiles once to Quartz vector instructions and reuses them at any zoom.</summary>
    public void DrawSymbol(SymbolContent content)
    {
        if (cgContextRef == 0) throw new InvalidOperationException("A drawing context must be bound before placing symbols.");
        if (filter.Amount != 1) { content.Replay(this); return; }
        if (!vectorSymbols.TryGetValue(content, out var symbol))
        {
            symbol = CompileSymbol(content);
            vectorSymbols.Add(content, symbol);
        }
        CGContextRef.CGContextSaveGState(cgContextRef);
        try
        {
            // Definition alpha is independent of ambient alpha, as in portable replay.
            CGContextRef.CGContextSetAlpha(cgContextRef, 1);
            symbol.Draw(cgContextRef);
        }
        finally { CGContextRef.CGContextRestoreGState(cgContextRef); }
        ((IContext)this).BeginPath();
    }
    private CGPdfSymbol CompileSymbol(SymbolContent content) => new(content, native =>
    {
        using IContext recorder = new IOSDrawingContext(symbolFontSource);
        var drawing = (IOSDrawingContext)recorder;
        drawing.cgContextRef = native;
        try { content.Replay(recorder); }
        finally { drawing.cgContextRef = 0; }
    });
    private void DisposeSymbols()
    {
        foreach (var pair in vectorSymbols) pair.Value.Dispose();
        vectorSymbols.Clear();
    }
}
