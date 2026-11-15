using System.Runtime.CompilerServices;
using Xui.Apps.TestApp;
using Xui.Apps.TestApp.Examples;
using Xui.Apps.TestApp.Pages.Layout;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Runtime.Software.Actual;
using Xui.Runtime.Test;

namespace Xui.Tests.Integration.TestApp;

public class SdkNavigationTest
{
    private static void Scenario(TestRuntimeVariant variant, Action<TestSinglePageApp<Application, MainWindow>> action,
        [CallerFilePath] string path = "", [CallerMemberName] string name = "")
    {
        using var app = IntegrationRuntimeVariants.CreateApp<Application, MainWindow>((600, 400), variant, path, name);
        app.Run(() => { app.Prepare(); app.MarkdownHeading(name, 1); action(app); });
    }

    [Theory, MemberData(nameof(IntegrationRuntimeVariants.All), MemberType = typeof(IntegrationRuntimeVariants))]
    public void HomePage_Renders(TestRuntimeVariant runtimeVariant) => Scenario(runtimeVariant, app =>
    {
        app.RequireView("CanvasTests", "verifying the home menu");
        app.Snapshot("HomePage");
    });

    [Theory, MemberData(nameof(IntegrationRuntimeVariants.All), MemberType = typeof(IntegrationRuntimeVariants))]
    public void Navigate_To_TextMetrics(TestRuntimeVariant runtimeVariant) => Scenario(runtimeVariant, app =>
    {
        app.Snapshot("HomePage");
        var button = app.Reveal("TextMetrics");
        app.MouseMove(button); app.Snapshot("Hover");
        app.MouseDown(button); app.Snapshot("Pressed");
        app.MouseUp(button); app.Advance(TimeSpan.FromMilliseconds(400));
        app.Target<TextMetricsExample>("Example.TextMetricsExample", "verifying Text Metrics navigation");
        app.Snapshot("TextMetrics");
    });

    [Theory, MemberData(nameof(IntegrationRuntimeVariants.All), MemberType = typeof(IntegrationRuntimeVariants))]
    public void Navigate_Through_All(TestRuntimeVariant runtimeVariant) => Scenario(runtimeVariant, app =>
    {
        app.Snapshot("HomePage");
        (string Menu, string Example)[] pages = runtimeVariant == TestRuntimeVariant.Desktop
            ? [("TextMetrics", "TextMetricsExample"), ("TextLayout", "TextLayoutExample"),
               ("NestedStacks", "NestedStacksExample"), ("ViewCollectionAlignment", "ViewCollectionAlignmentExample"),
               ("AnimatedHeart", "CanvasAnimationsExample"), ("TextBox", "TextBoxExample")]
            : [("TextMetrics", "TextMetricsExample"), ("Basics", "BasicsExample"),
               ("GridLayout", "GridExample"), ("FlexboxLayout", "FlexboxExample"),
               ("AnimatedHeart", "CanvasAnimationsExample"), ("TextBox", "TextBoxExample")];
        foreach (var (menu, example) in pages)
        {
            app.Tap(menu);
            app.RequireView("Example." + example, $"verifying destination for '{menu}'");
            app.Snapshot(menu);
            app.Back();
            Assert.Null(app.Window.RootView.FindViewById("Example." + example));
        }
    });

    [Theory, MemberData(nameof(IntegrationRuntimeVariants.All), MemberType = typeof(IntegrationRuntimeVariants))]
    public void Pending_Hover_After_Navigation(TestRuntimeVariant runtimeVariant) => Scenario(runtimeVariant, app =>
    {
        // TextMetrics is present on both current menus. Compare its own drawing so
        // the intentionally retained press ripple/home-header animation cannot mask hover state.
        var button = app.Reveal("TextMetrics");
        var idle = Drawing(button);
        app.MouseMove(button); app.Snapshot("HoverTextMetrics");
        app.MouseDown(button); app.Snapshot("PressedTextMetrics");
        app.MouseUp(button); app.Advance(TimeSpan.FromMilliseconds(400));
        app.RequireView("Example.TextMetricsExample", "verifying forward navigation");
        app.Snapshot("TextMetrics");
        app.Back();
        app.Advance(TimeSpan.FromSeconds(1));
        var neutral = app.Window.SafeArea.TopLeft + new Vector(2, 2);
        app.MouseMove(neutral);
        button = app.Reveal("TextMetrics");
        Assert.Equal(idle, Drawing(button));
        app.Snapshot("MouseMovedAway");
    });

    [Theory, MemberData(nameof(IntegrationRuntimeVariants.All), MemberType = typeof(IntegrationRuntimeVariants))]
    public void Heartbeat_Stops_After_Mouse_Over_Back(TestRuntimeVariant runtimeVariant) => Scenario(runtimeVariant, app =>
    {
        app.Open<CanvasAnimationsExample>("AnimatedHeart");
        var heart = app.RequireView("CanvasAnimations.Heart", "checking heartbeat animation");
        var before = Drawing(heart); app.Snapshot("Heart.Before");
        app.Advance(TimeSpan.FromMilliseconds(100));
        Assert.NotEqual(before, Drawing(heart)); app.Snapshot("Heart.After");
        var back = app.RequireView("Back", "hovering Back without navigating");
        app.MouseMove(back);
        before = Drawing(heart); app.Snapshot("MouseOverBack");
        // A heartbeat has a resting interval. Observe a complete beat rather
        // than assuming any particular pair of timestamps must differ.
        var changed = false;
        for (var frame = 0; frame < 12; frame++)
        {
            app.Advance(TimeSpan.FromMilliseconds(80));
            changed |= before != Drawing(heart);
        }
        Assert.True(changed, "The heart stopped animating while Back was hovered.");
        Assert.Same(heart, app.RequireView("CanvasAnimations.Heart", "verifying hover kept the page active"));
        app.Snapshot("Heart.AfterMouseOver");
    });

    private static string Drawing(View view)
    {
        using var stream = new MemoryStream();
        using (var context = new SvgDrawingContext((600, 1000), stream, Xui.Core.Fonts.Inter.URIs, keepOpen: true))
            view.Render(context);
        return System.Text.Encoding.UTF8.GetString(stream.ToArray());
    }
}
