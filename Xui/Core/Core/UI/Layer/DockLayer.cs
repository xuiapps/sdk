using Xui.Core.Abstract.Events;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI.Input;

namespace Xui.Core.UI.Layer;

/// <summary>
/// Container for dock layout types.
/// Usage: <c>DockLayer.Dock3&lt;View, StepButton, NumberInputLayer, StepButton&gt;</c>
/// </summary>
public static class DockLayer
{
    /// <summary>How a slot consumes available space.</summary>
    public enum Align { Left, Stretch, Right }

    /// <summary>Pairs a layer with an <see cref="Align"/> value.</summary>
    public struct Docked<T> where T : struct
    {
        public Align Align;
        public T Child;
    }

    /// <summary>Two-slot dock layout.</summary>
    public struct Dock2<TView, T1, T2> : ILayer<TView>
        where TView : ILayerHost
        where T1 : struct, ILayer<TView>
        where T2 : struct, ILayer<TView>
    {
        public Docked<T1> Child1;
        public Docked<T2> Child2;

        private Size s1, s2;

        public void Update(TView view, ref LayoutGuide guide)
        {
            if (guide.IsAnimate) Animate(view, guide.PreviousTime, guide.CurrentTime);
            if (guide.IsMeasure) guide.DesiredSize = Measure(view, guide.AvailableSize, guide.MeasureContext!);
            if (guide.IsArrange) Arrange(view, guide.ArrangedRect, guide.MeasureContext!);
            if (guide.IsRender)  Render(view, guide.RenderContext!);
        }

        public Size Measure(TView view, Size available, IMeasureContext ctx)
        {
            nfloat fixedW = 0, maxH = 0;

            if (Child1.Align != Align.Stretch) { s1 = Child1.Child.Measure(view, available, ctx); fixedW += s1.Width; maxH = nfloat.Max(maxH, s1.Height); }
            if (Child2.Align != Align.Stretch) { s2 = Child2.Child.Measure(view, available, ctx); fixedW += s2.Width; maxH = nfloat.Max(maxH, s2.Height); }

            var rem = new Size(nfloat.Max(0, available.Width - fixedW), available.Height);
            if (Child1.Align == Align.Stretch) { s1 = Child1.Child.Measure(view, rem, ctx); maxH = nfloat.Max(maxH, s1.Height); }
            if (Child2.Align == Align.Stretch) { s2 = Child2.Child.Measure(view, rem, ctx); maxH = nfloat.Max(maxH, s2.Height); }

            bool hasStretch = Child1.Align == Align.Stretch || Child2.Align == Align.Stretch;
            return new Size(hasStretch ? available.Width : fixedW, maxH);
        }

        public void Arrange(TView view, Rect rect, IMeasureContext ctx)
        {
            nfloat lx = rect.X, rx = rect.X + rect.Width;

            if (Child1.Align == Align.Left)  { Child1.Child.Arrange(view, new Rect(lx, rect.Y, s1.Width, rect.Height), ctx); lx += s1.Width; }
            if (Child2.Align == Align.Left)  { Child2.Child.Arrange(view, new Rect(lx, rect.Y, s2.Width, rect.Height), ctx); lx += s2.Width; }

            if (Child2.Align == Align.Right) { rx -= s2.Width; Child2.Child.Arrange(view, new Rect(rx, rect.Y, s2.Width, rect.Height), ctx); }
            if (Child1.Align == Align.Right) { rx -= s1.Width; Child1.Child.Arrange(view, new Rect(rx, rect.Y, s1.Width, rect.Height), ctx); }

            var s = new Rect(lx, rect.Y, rx - lx, rect.Height);
            if (Child1.Align == Align.Stretch) Child1.Child.Arrange(view, s, ctx);
            if (Child2.Align == Align.Stretch) Child2.Child.Arrange(view, s, ctx);
        }

        public void Render(TView view, IContext ctx)                                      { Child1.Child.Render(view, ctx); Child2.Child.Render(view, ctx); }
        public void Animate(TView view, TimeSpan p, TimeSpan c)                          { Child1.Child.Animate(view, p, c); Child2.Child.Animate(view, p, c); }
        public void OnPointerEvent(TView view, ref PointerEventRef e, EventPhase phase)  { Child1.Child.OnPointerEvent(view, ref e, phase); Child2.Child.OnPointerEvent(view, ref e, phase); }
        public void OnKeyDown(TView view, ref KeyEventRef e)                             { Child1.Child.OnKeyDown(view, ref e); Child2.Child.OnKeyDown(view, ref e); }
        public void OnChar(TView view, ref KeyEventRef e)                                { Child1.Child.OnChar(view, ref e); Child2.Child.OnChar(view, ref e); }
        public void OnFocus(TView view)                                                  { Child1.Child.OnFocus(view); Child2.Child.OnFocus(view); }
        public void OnBlur(TView view)                                                   { Child1.Child.OnBlur(view); Child2.Child.OnBlur(view); }
    }

    /// <summary>Three-slot dock layout. Typical usage: Left button | Stretch content | Right button.</summary>
    public struct Dock3<TView, T1, T2, T3> : ILayer<TView>
        where TView : ILayerHost
        where T1 : struct, ILayer<TView>
        where T2 : struct, ILayer<TView>
        where T3 : struct, ILayer<TView>
    {
        public Docked<T1> Child1;
        public Docked<T2> Child2;
        public Docked<T3> Child3;

        private Size s1, s2, s3;

        public void Update(TView view, ref LayoutGuide guide)
        {
            if (guide.IsAnimate) Animate(view, guide.PreviousTime, guide.CurrentTime);
            if (guide.IsMeasure) guide.DesiredSize = Measure(view, guide.AvailableSize, guide.MeasureContext!);
            if (guide.IsArrange) Arrange(view, guide.ArrangedRect, guide.MeasureContext!);
            if (guide.IsRender)  Render(view, guide.RenderContext!);
        }

        public Size Measure(TView view, Size available, IMeasureContext ctx)
        {
            nfloat fixedW = 0, maxH = 0;

            if (Child1.Align != Align.Stretch) { s1 = Child1.Child.Measure(view, available, ctx); fixedW += s1.Width; maxH = nfloat.Max(maxH, s1.Height); }
            if (Child2.Align != Align.Stretch) { s2 = Child2.Child.Measure(view, available, ctx); fixedW += s2.Width; maxH = nfloat.Max(maxH, s2.Height); }
            if (Child3.Align != Align.Stretch) { s3 = Child3.Child.Measure(view, available, ctx); fixedW += s3.Width; maxH = nfloat.Max(maxH, s3.Height); }

            var rem = new Size(nfloat.Max(0, available.Width - fixedW), available.Height);
            if (Child1.Align == Align.Stretch) { s1 = Child1.Child.Measure(view, rem, ctx); maxH = nfloat.Max(maxH, s1.Height); }
            if (Child2.Align == Align.Stretch) { s2 = Child2.Child.Measure(view, rem, ctx); maxH = nfloat.Max(maxH, s2.Height); }
            if (Child3.Align == Align.Stretch) { s3 = Child3.Child.Measure(view, rem, ctx); maxH = nfloat.Max(maxH, s3.Height); }

            bool hasStretch = Child1.Align == Align.Stretch || Child2.Align == Align.Stretch || Child3.Align == Align.Stretch;
            return new Size(hasStretch ? available.Width : fixedW, maxH);
        }

        public void Arrange(TView view, Rect rect, IMeasureContext ctx)
        {
            nfloat lx = rect.X, rx = rect.X + rect.Width;

            if (Child1.Align == Align.Left)  { Child1.Child.Arrange(view, new Rect(lx, rect.Y, s1.Width, rect.Height), ctx); lx += s1.Width; }
            if (Child2.Align == Align.Left)  { Child2.Child.Arrange(view, new Rect(lx, rect.Y, s2.Width, rect.Height), ctx); lx += s2.Width; }
            if (Child3.Align == Align.Left)  { Child3.Child.Arrange(view, new Rect(lx, rect.Y, s3.Width, rect.Height), ctx); lx += s3.Width; }

            if (Child3.Align == Align.Right) { rx -= s3.Width; Child3.Child.Arrange(view, new Rect(rx, rect.Y, s3.Width, rect.Height), ctx); }
            if (Child2.Align == Align.Right) { rx -= s2.Width; Child2.Child.Arrange(view, new Rect(rx, rect.Y, s2.Width, rect.Height), ctx); }
            if (Child1.Align == Align.Right) { rx -= s1.Width; Child1.Child.Arrange(view, new Rect(rx, rect.Y, s1.Width, rect.Height), ctx); }

            var s = new Rect(lx, rect.Y, rx - lx, rect.Height);
            if (Child1.Align == Align.Stretch) Child1.Child.Arrange(view, s, ctx);
            if (Child2.Align == Align.Stretch) Child2.Child.Arrange(view, s, ctx);
            if (Child3.Align == Align.Stretch) Child3.Child.Arrange(view, s, ctx);
        }

        public void Render(TView view, IContext ctx)                                      { Child1.Child.Render(view, ctx); Child2.Child.Render(view, ctx); Child3.Child.Render(view, ctx); }
        public void Animate(TView view, TimeSpan p, TimeSpan c)                          { Child1.Child.Animate(view, p, c); Child2.Child.Animate(view, p, c); Child3.Child.Animate(view, p, c); }
        public void OnPointerEvent(TView view, ref PointerEventRef e, EventPhase phase)  { Child1.Child.OnPointerEvent(view, ref e, phase); Child2.Child.OnPointerEvent(view, ref e, phase); Child3.Child.OnPointerEvent(view, ref e, phase); }
        public void OnKeyDown(TView view, ref KeyEventRef e)                             { Child1.Child.OnKeyDown(view, ref e); Child2.Child.OnKeyDown(view, ref e); Child3.Child.OnKeyDown(view, ref e); }
        public void OnChar(TView view, ref KeyEventRef e)                                { Child1.Child.OnChar(view, ref e); Child2.Child.OnChar(view, ref e); Child3.Child.OnChar(view, ref e); }
        public void OnFocus(TView view)                                                  { Child1.Child.OnFocus(view); Child2.Child.OnFocus(view); Child3.Child.OnFocus(view); }
        public void OnBlur(TView view)                                                   { Child1.Child.OnBlur(view); Child2.Child.OnBlur(view); Child3.Child.OnBlur(view); }
    }

    /// <summary>Four-slot dock layout.</summary>
    public struct Dock4<TView, T1, T2, T3, T4> : ILayer<TView>
        where TView : ILayerHost
        where T1 : struct, ILayer<TView>
        where T2 : struct, ILayer<TView>
        where T3 : struct, ILayer<TView>
        where T4 : struct, ILayer<TView>
    {
        public Docked<T1> Child1;
        public Docked<T2> Child2;
        public Docked<T3> Child3;
        public Docked<T4> Child4;

        private Size s1, s2, s3, s4;

        public void Update(TView view, ref LayoutGuide guide)
        {
            if (guide.IsAnimate) Animate(view, guide.PreviousTime, guide.CurrentTime);
            if (guide.IsMeasure) guide.DesiredSize = Measure(view, guide.AvailableSize, guide.MeasureContext!);
            if (guide.IsArrange) Arrange(view, guide.ArrangedRect, guide.MeasureContext!);
            if (guide.IsRender)  Render(view, guide.RenderContext!);
        }

        public Size Measure(TView view, Size available, IMeasureContext ctx)
        {
            nfloat fixedW = 0, maxH = 0;

            if (Child1.Align != Align.Stretch) { s1 = Child1.Child.Measure(view, available, ctx); fixedW += s1.Width; maxH = nfloat.Max(maxH, s1.Height); }
            if (Child2.Align != Align.Stretch) { s2 = Child2.Child.Measure(view, available, ctx); fixedW += s2.Width; maxH = nfloat.Max(maxH, s2.Height); }
            if (Child3.Align != Align.Stretch) { s3 = Child3.Child.Measure(view, available, ctx); fixedW += s3.Width; maxH = nfloat.Max(maxH, s3.Height); }
            if (Child4.Align != Align.Stretch) { s4 = Child4.Child.Measure(view, available, ctx); fixedW += s4.Width; maxH = nfloat.Max(maxH, s4.Height); }

            var rem = new Size(nfloat.Max(0, available.Width - fixedW), available.Height);
            if (Child1.Align == Align.Stretch) { s1 = Child1.Child.Measure(view, rem, ctx); maxH = nfloat.Max(maxH, s1.Height); }
            if (Child2.Align == Align.Stretch) { s2 = Child2.Child.Measure(view, rem, ctx); maxH = nfloat.Max(maxH, s2.Height); }
            if (Child3.Align == Align.Stretch) { s3 = Child3.Child.Measure(view, rem, ctx); maxH = nfloat.Max(maxH, s3.Height); }
            if (Child4.Align == Align.Stretch) { s4 = Child4.Child.Measure(view, rem, ctx); maxH = nfloat.Max(maxH, s4.Height); }

            bool hasStretch = Child1.Align == Align.Stretch || Child2.Align == Align.Stretch ||
                              Child3.Align == Align.Stretch || Child4.Align == Align.Stretch;
            return new Size(hasStretch ? available.Width : fixedW, maxH);
        }

        public void Arrange(TView view, Rect rect, IMeasureContext ctx)
        {
            nfloat lx = rect.X, rx = rect.X + rect.Width;

            if (Child1.Align == Align.Left)  { Child1.Child.Arrange(view, new Rect(lx, rect.Y, s1.Width, rect.Height), ctx); lx += s1.Width; }
            if (Child2.Align == Align.Left)  { Child2.Child.Arrange(view, new Rect(lx, rect.Y, s2.Width, rect.Height), ctx); lx += s2.Width; }
            if (Child3.Align == Align.Left)  { Child3.Child.Arrange(view, new Rect(lx, rect.Y, s3.Width, rect.Height), ctx); lx += s3.Width; }
            if (Child4.Align == Align.Left)  { Child4.Child.Arrange(view, new Rect(lx, rect.Y, s4.Width, rect.Height), ctx); lx += s4.Width; }

            if (Child4.Align == Align.Right) { rx -= s4.Width; Child4.Child.Arrange(view, new Rect(rx, rect.Y, s4.Width, rect.Height), ctx); }
            if (Child3.Align == Align.Right) { rx -= s3.Width; Child3.Child.Arrange(view, new Rect(rx, rect.Y, s3.Width, rect.Height), ctx); }
            if (Child2.Align == Align.Right) { rx -= s2.Width; Child2.Child.Arrange(view, new Rect(rx, rect.Y, s2.Width, rect.Height), ctx); }
            if (Child1.Align == Align.Right) { rx -= s1.Width; Child1.Child.Arrange(view, new Rect(rx, rect.Y, s1.Width, rect.Height), ctx); }

            var s = new Rect(lx, rect.Y, rx - lx, rect.Height);
            if (Child1.Align == Align.Stretch) Child1.Child.Arrange(view, s, ctx);
            if (Child2.Align == Align.Stretch) Child2.Child.Arrange(view, s, ctx);
            if (Child3.Align == Align.Stretch) Child3.Child.Arrange(view, s, ctx);
            if (Child4.Align == Align.Stretch) Child4.Child.Arrange(view, s, ctx);
        }

        public void Render(TView view, IContext ctx)                                      { Child1.Child.Render(view, ctx); Child2.Child.Render(view, ctx); Child3.Child.Render(view, ctx); Child4.Child.Render(view, ctx); }
        public void Animate(TView view, TimeSpan p, TimeSpan c)                          { Child1.Child.Animate(view, p, c); Child2.Child.Animate(view, p, c); Child3.Child.Animate(view, p, c); Child4.Child.Animate(view, p, c); }
        public void OnPointerEvent(TView view, ref PointerEventRef e, EventPhase phase)  { Child1.Child.OnPointerEvent(view, ref e, phase); Child2.Child.OnPointerEvent(view, ref e, phase); Child3.Child.OnPointerEvent(view, ref e, phase); Child4.Child.OnPointerEvent(view, ref e, phase); }
        public void OnKeyDown(TView view, ref KeyEventRef e)                             { Child1.Child.OnKeyDown(view, ref e); Child2.Child.OnKeyDown(view, ref e); Child3.Child.OnKeyDown(view, ref e); Child4.Child.OnKeyDown(view, ref e); }
        public void OnChar(TView view, ref KeyEventRef e)                                { Child1.Child.OnChar(view, ref e); Child2.Child.OnChar(view, ref e); Child3.Child.OnChar(view, ref e); Child4.Child.OnChar(view, ref e); }
        public void OnFocus(TView view)                                                  { Child1.Child.OnFocus(view); Child2.Child.OnFocus(view); Child3.Child.OnFocus(view); Child4.Child.OnFocus(view); }
        public void OnBlur(TView view)                                                   { Child1.Child.OnBlur(view); Child2.Child.OnBlur(view); Child3.Child.OnBlur(view); Child4.Child.OnBlur(view); }
    }
}
