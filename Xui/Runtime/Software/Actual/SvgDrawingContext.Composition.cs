using System;
using System.Collections.Generic;
using Xui.Core.Canvas;
using Xui.Core.Canvas.Composition.Graphics;

namespace Xui.Runtime.Software.Actual;

public sealed partial class SvgDrawingContext
{
    private readonly Stack<int> compositionStateDepths = new();
    private readonly Stack<bool> backedScopes = new();
    private readonly Stack<bool> drawingPhases = new();
    private int surfaceEffectId;

    private void CheckCompositionDrawing()
    {
        if (drawingPhases.TryPeek(out var children) && children)
            throw new InvalidOperationException("Draw background content before child groups; use an overlay group for foreground drawing.");
    }

    /// <summary>
    /// Emits an explicit SVG group, even with no transform or drawing. A new export always
    /// needs content, independently of HasUpdates and any other context's native cache.
    /// </summary>
    public CompositionGroup Group(scoped in GroupOptions options)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        options.Validate();
        if (options.ViewBacking)
        {
            if (drawingPhases.Count != 0) { drawingPhases.Pop(); drawingPhases.Push(true); }
            drawingPhases.Push(false);
        }
        backedScopes.Push(options.ViewBacking);
        var context = (IContext)this;
        context.Save();
        try
        {
            PushGroup(options.Transform);
            EmitSurfaceEffects(in options);
            if (options.ClipToBounds)
            {
                context.BeginPath();
                context.RoundRect(options.Bounds!.Value, options.CornerRadius);
                context.Clip();
                context.BeginPath();
            }
            compositionStateDepths.Push(stateStack.Count);
            return new CompositionGroup(this, needsRedraw: true);
        }
        catch
        {
            if (backedScopes.Pop()) drawingPhases.Pop();
            context.Restore();
            throw;
        }
    }

    /// <inheritdoc/>
    public void EndGroup()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (!compositionStateDepths.TryPeek(out var depth))
            throw new InvalidOperationException("No composition group to close.");
        if (stateStack.Count != depth)
            throw new InvalidOperationException("Balance Save/Restore inside the group before closing it.");
        compositionStateDepths.Pop();
        ((IStateContext)this).Restore();
        if (backedScopes.Pop()) drawingPhases.Pop();
    }

    private void EmitSurfaceEffects(in GroupOptions options)
    {
        if (options.Bounds is not { } bounds) return;
        var geometry = $"x=\"{F(bounds.X)}\" y=\"{F(bounds.Y)}\" width=\"{F(bounds.Width)}\" height=\"{F(bounds.Height)}\" rx=\"{F(options.CornerRadius)}\"";
        if (options.Shadow is { } shadow && shadow.Color.Alpha > 0)
        {
            var id = $"surfaceShadow{++surfaceEffectId}";
            var spread = shadow.Radius * 3;
            defsWriter.WriteLine($"    <filter id=\"{id}\" filterUnits=\"userSpaceOnUse\" x=\"{F(bounds.X - spread)}\" y=\"{F(bounds.Y - spread)}\" width=\"{F(bounds.Width + spread * 2)}\" height=\"{F(bounds.Height + spread * 2)}\" color-interpolation-filters=\"sRGB\"><feGaussianBlur stdDeviation=\"{F(shadow.Radius)}\"/></filter>");
            bodyWriter.WriteLine($"<rect {geometry} transform=\"translate({F(shadow.Offset.X)} {F(shadow.Offset.Y)})\" fill=\"{ToSvgColor(shadow.Color, out _)}\" fill-opacity=\"{F(shadow.Color.Alpha)}\" filter=\"url(#{id})\"/>");
        }
        if (options.Backdrop != BackdropMaterial.None)
        {
            var id = $"surfaceGlass{++surfaceEffectId}";
            defsWriter.WriteLine($"<linearGradient id=\"{id}\" x1=\"0\" y1=\"0\" x2=\"0\" y2=\"1\"><stop offset=\"0\" stop-color=\"#FFFFFF\" stop-opacity=\"0.88\"/><stop offset=\"1\" stop-color=\"#DBE6FF\" stop-opacity=\"0.72\"/></linearGradient>");
            // Portable SVG has no UIKit backdrop sampler. Preserve crisp foreground content
            // and approximate the material with a translucent highlight and a soft surface edge.
            defsWriter.WriteLine($"<filter id=\"{id}Soft\" x=\"-5%\" y=\"-5%\" width=\"110%\" height=\"110%\"><feGaussianBlur stdDeviation=\"0.4\"/></filter>");
            bodyWriter.WriteLine($"<rect {geometry} data-xui-material=\"{options.Backdrop}\" fill=\"url(#{id})\" stroke=\"#FFFFFF\" stroke-opacity=\"0.7\" filter=\"url(#{id}Soft)\"/>");
        }
        if (options.Background is { } color)
            bodyWriter.WriteLine($"<rect {geometry} fill=\"{ToSvgColor(color, out _)}\" fill-opacity=\"{F(color.Alpha)}\"/>");
        if (options.Border is { } border && border.Width > 0)
        {
            var width = nfloat.Min(border.Width, nfloat.Min(bounds.Width, bounds.Height));
            bodyWriter.WriteLine($"<rect x=\"{F(bounds.X + width / 2)}\" y=\"{F(bounds.Y + width / 2)}\" width=\"{F(bounds.Width - width)}\" height=\"{F(bounds.Height - width)}\" rx=\"{F(nfloat.Max(0, options.CornerRadius - width / 2))}\" fill=\"none\" stroke=\"{ToSvgColor(border.Color, out _)}\" stroke-opacity=\"{F(border.Color.Alpha)}\" stroke-width=\"{F(width)}\"/>");
        }
    }
}
