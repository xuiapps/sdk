using Xui.Apps.BlankApp;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Runtime.Test;

namespace Xui.Tests.Integration.TestApp;

/// <summary>
/// Integration tests that boot the real TestApp via the test platform,
/// render the home page, send mouse events to navigate, and capture SVG snapshots.
/// </summary>
public class SdkNavigationTest
{
    private static Size WindowSize = (600, 400);

    [Fact]
    public void HomePage_Renders()
    {
        using var app = new TestSinglePageApp<Application, MainWindow>(WindowSize);
        app.Snapshot("HomePage");
    }

    [Fact]
    public void Navigate_To_TextMetrics()
    {
        using var app = new TestSinglePageApp<Application, MainWindow>(WindowSize);
        app.Snapshot("HomePage");

        var button = app.Window.RootView.FindViewById("TextMetrics");
        Assert.NotNull(button);

        app.MouseMove(button);
        app.Snapshot("Hover");
        app.MouseDown(button);
        app.Snapshot("Pressed");
        app.MouseUp(button);
        app.Snapshot("TextMetrics");
    }

    [Fact]
    public void Navigate_Through_All()
    {
        using var app = new TestSinglePageApp<Application, MainWindow>(WindowSize);
        app.Snapshot("HomePage");

        string[] pages = ["TextMetrics", "TextLayout", "NestedStacks", "ViewCollectionAlignment", "AnimatedHeart", "TextBox"];

        foreach (var page in pages)
        {
            var button = app.Window.RootView.FindViewById(page);
            Assert.NotNull(button);
            app.MouseMove(button);
            app.MouseDown(button);
            app.MouseUp(button);

            app.AnimationFrame(TimeSpan.Zero, TimeSpan.Zero);
            app.Snapshot(page);

            var back = app.Window.RootView.FindViewById("Back");
            Assert.NotNull(back);
            app.MouseMove(back);
            app.MouseDown(back);
            app.MouseUp(back);
        }
    }

    /// <summary>
    /// Bug: After navigating to an example and back, the button that was
    /// hovered before navigation still shows its hover effect.
    /// Views removed from the tree should receive a pointer-leave event.
    /// </summary>
    [Fact]
    public void Pending_Hover_After_Navigation()
    {
        using var app = new TestSinglePageApp<Application, MainWindow>(WindowSize);
        app.Snapshot("HomePage");

        // Hover and click the NestedStacks button
        var button = app.Window.RootView.FindViewById("NestedStacks");
        Assert.NotNull(button);
        app.MouseMove(button);
        app.Snapshot("HoverNestedStacks");
        app.MouseDown(button);
        app.Snapshot("PressedNestedStacks");
        app.MouseUp(button);

        app.Snapshot("NestedStacks");

        // Click back
        var back = app.Window.RootView.FindViewById("Back");
        Assert.NotNull(back);
        app.MouseMove(back);
        app.Snapshot("HoverBack");
        app.MouseDown(back);
        app.Snapshot("PressHoverBack");
        app.MouseUp(back);
        app.Snapshot("ClickedHomePage");

        // Bug: NestedStacks button still shows hover despite being re-created
        // Move mouse away from any button to a neutral position
        app.MouseMove(new Point(10, 10));
        app.Snapshot("MouseMovedAway");
    }

    /// <summary>
    /// Bug: After moving the mouse over the back button (without clicking),
    /// the animated heart stops animating — animation frames no longer
    /// produce different renders at rest vs peak times.
    /// </summary>
    [Fact]
    public void Heartbeat_Stops_After_Mouse_Over_Back()
    {
        using var app = new TestSinglePageApp<Application, MainWindow>(WindowSize);

        // Navigate to AnimatedHeart
        var button = app.Window.RootView.FindViewById("AnimatedHeart");
        app.Snapshot("Home");

        Assert.NotNull(button);
        app.MouseMove(button);
        app.MouseDown(button);
        app.MouseUp(button);

        // Capture heart at rest (t=0.0s) and primary peak (t=0.10s)
        app.AnimationFrame(TimeSpan.Zero, TimeSpan.Zero);
        app.Snapshot("Heart.Rest");
        app.AnimationFrame(TimeSpan.Zero, TimeSpan.FromSeconds(0.10));
        app.Snapshot("Heart.PrimaryPeak");

        // Move mouse over the back button (do NOT click)
        var back = app.Window.RootView.FindViewById("Back");
        Assert.NotNull(back);
        app.MouseMove(back);
        app.Snapshot("MouseOverBack");

        // Bug: heart should still animate but it stops
        // Capture the same two phases again
        app.AnimationFrame(TimeSpan.FromSeconds(0.10), TimeSpan.FromSeconds(0.833));
        app.Snapshot("Heart.Rest.AfterMouseOver");
        app.AnimationFrame(TimeSpan.FromSeconds(0.833), TimeSpan.FromSeconds(0.933));
        app.Snapshot("Heart.PrimaryPeak.AfterMouseOver");
    }
}
