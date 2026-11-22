using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.DevKit.UI.Mobile.Navigation;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp.Pages.Floorplan;

/// <summary>Polls immutable background data on UI frames; only the UI thread constructs and attaches views.</summary>
internal class FloorplanLoadingView<T> : View
{
    private readonly Task<T> drawing;
    private readonly Func<T, View> createView;
    private View? content;
    private NFloat shimmer;
    private Task? preparation;

    public FloorplanLoadingView(Task<T> drawing, Func<T, View> createView, bool deferHydration = false)
    {
        this.drawing = drawing;
        this.createView = createView;
        Id = "Floorplan.Loading";
        if (!deferHydration && drawing.IsCompleted) Hydrate();
    }

    protected virtual Task PrepareAsync(T data) => Task.CompletedTask;
    protected virtual View CreateView(T data) => createView(data);
    protected void ResetPreparation() => preparation = null;

    public override int Count => content is null ? 0 : 1;
    public override View this[int index] => index == 0 && content is not null
        ? content : throw new IndexOutOfRangeException();

    protected override void OnActivate()
    {
        base.OnActivate();
        if (content is null) RequestAnimationFrame();
    }

    protected override void AnimateCore(TimeSpan previousTime, TimeSpan currentTime)
    {
        if (content is null)
        {
            // Preparation may run during navigation, but attaching the result must wait.
            if (drawing.IsCompletedSuccessfully && preparation is null)
            {
                try { preparation = PrepareAsync(drawing.Result); }
                catch (Exception error) { preparation = Task.FromException(error); }
            }
            // Keep the same cached placeholder for the entire page journey, even if loading finishes.
            if (this.TryFindParent<NavigationView>(out var navigation) && navigation.IsTransitioning)
            {
                RequestAnimationFrame();
                return;
            }
            if (drawing.IsCompleted && (preparation is null || preparation.IsCompleted)) Hydrate();
            else
            {
                shimmer = (shimmer + (NFloat)(currentTime - previousTime).TotalSeconds) % 2;
                InvalidateRender();
                RequestAnimationFrame();
            }
        }
        base.AnimateCore(previousTime, currentTime);
    }

    private void Hydrate()
    {
        View view;
        if (drawing.IsCompletedSuccessfully && preparation?.IsCompletedSuccessfully != false)
            view = CreateView(drawing.Result);
        else
        {
            _ = drawing.Exception; // Observe a failed speculative load, too.
            _ = preparation?.Exception;
            view = new Label
            {
                Id = "Floorplan.LoadError", Text = "Unable to load the office floorplan.",
                FontFamily = "Inter", FontSize = 16, TextColor = Colors.Gray,
                HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Middle,
            };
        }
        Id = "Floorplan.Content";
        SetProtectedChild(ref content, view);
    }

    protected override void RenderCore(IContext context)
    {
        if (content is not null) { base.RenderCore(context); return; }
        // Four lightweight room outlines. No geometry, fonts, gradients or native resources to load.
        var width = Frame.Width * (NFloat).76;
        var height = NFloat.Min(Frame.Height * (NFloat).45, width * (NFloat).85);
        var x = Frame.Center.X - width / 2;
        var y = Frame.Center.Y - height / 2;
        context.Save();
        context.LineWidth = 2;
        Outline((x, y, width * (NFloat).58, height * (NFloat).43), 0);
        Outline((x + width * (NFloat).63, y, width * (NFloat).37, height * (NFloat).61), 1);
        Outline((x, y + height * (NFloat).5, width * (NFloat).34, height * (NFloat).5), 2);
        Outline((x + width * (NFloat).39, y + height * (NFloat).68, width * (NFloat).61, height * (NFloat).32), 3);
        context.Restore();

        void Outline(Rect rect, int index)
        {
            var alpha = (NFloat).45 + (NFloat).3 * ((NFloat)1 + NFloat.Sin(shimmer * NFloat.Pi - index * (NFloat).7)) / 2;
            context.SetStroke(new Color((NFloat).82, (NFloat).83, (NFloat).85, alpha));
            context.BeginPath(); context.Rect(rect); context.Stroke();
        }
    }
}

internal sealed class FloorplanLoadingView(Task<FloorplanDrawing> drawing)
    : FloorplanLoadingView<FloorplanDrawing>(drawing, static data => new FloorplanView(data));
