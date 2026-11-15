using Xui.Apps.TestApp;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Runtime.Software.Actual;
using Xui.Runtime.Test;

namespace Xui.Tests.Integration.TestApp;

/// <summary>Scenario interactions anchored to identified views, in window coordinates.</summary>
internal static class ScenarioInput
{
    public static void Prepare(this TestSinglePageApp<Application, MainWindow> app)
    {
        app.SvgFontResolver = SvgDrawingContext.SvgFontResolver.Embedded;
        app.Render();
    }

    public static T Target<T>(this TestSinglePageApp<Application, MainWindow> app, string id, string action) where T : View
        => Assert.IsAssignableFrom<T>(app.RequireView(id, action));

    public static View Reveal(this TestSinglePageApp<Application, MainWindow> app, string id)
    {
        var view = app.RequireView(id, "locating an interaction target");
        for (var attempt = 0; attempt < 12; attempt++)
        {
            var point = TestSinglePageApp<Application, MainWindow>.ViewCenter(view);
            // Leave room for the overlay header and bottom safe area.
            var safe = app.Window.SafeArea;
            var visible = point.Y >= safe.Top + 64 && point.Y < safe.Bottom - 12 && safe.Contains(point);
            if (!view.TryFindParent<ScrollView>(out var scroll))
            {
                Assert.True(app.Window.DisplayArea.Contains(point), $"Element '{id}' is outside the window at {point}.");
                return view;
            }
            var local = scroll.Parent?.GlobalToLocal(point) ?? point;
            if (visible && scroll.Frame.Contains(local)) return view;
            var center = TestSinglePageApp<Application, MainWindow>.ViewCenter(scroll);
            var distance = scroll.Frame.Height * (nfloat)0.55;
            var direction = point.Y < safe.Top + 64 ? 1 : -1;
            var start = new Point(center.X, safe.Top + safe.Height * (direction < 0 ? (nfloat)0.8 : (nfloat)0.3));
            app.PointerDown(start);
            // The first move negotiates capture with a child button. The next
            // move performs scrolling after the ScrollView owns the gesture.
            app.PointerMove(start + new Vector(0, 12 * direction));
            app.PointerMove(start + new Vector(0, distance * direction));
            app.Advance(TimeSpan.FromMilliseconds(200)); // Hold before release: no accidental fling.
            app.PointerUp(start + new Vector(0, distance * direction));
            app.Advance(TimeSpan.FromMilliseconds(800));
        }
        throw new InvalidOperationException($"Cannot reveal element '{id}' within its scroll viewport.");
    }

    public static void Tap(this TestSinglePageApp<Application, MainWindow> app, string id)
    {
        var view = app.Reveal(id);
        app.MouseMove(view);
        app.MouseDown(view);
        app.MouseUp(view);
        app.Advance(TimeSpan.FromMilliseconds(400));
    }

    public static T Open<T>(this TestSinglePageApp<Application, MainWindow> app, string menuId) where T : Example
    {
        app.Tap(menuId);
        return app.Target<T>($"Example.{typeof(T).Name}", $"opening '{menuId}'");
    }

    public static void Back(this TestSinglePageApp<Application, MainWindow> app)
    {
        app.Tap("Back");
        app.RequireView("CanvasTests", "verifying the home page after Back");
    }
}
