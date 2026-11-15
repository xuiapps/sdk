using Microsoft.Extensions.DependencyInjection;
using System.Xml.Linq;
using Xui.Core.Abstract.Events;
using Xui.Apps.TestApp;
using Xui.Apps.TestApp.Pages.Gestures;
using Xui.Apps.TestApp.Pages.Gestures.Widgets;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.DevKit.UI.Mobile.Navigation;
using Xui.Middleware.EmulatorLegacy.Devices;
using Xui.Runtime.Software.Actual;
using Xui.Runtime.Software.Font;
using Xui.Runtime.Test;

namespace Xui.Tests.Integration.TestApp;

public class InputGalleryTest
{
    [Fact]
    public void TouchScrollPinchAndSwipeCards()
    {
        var device = DeviceCatalog.All.Single(d => d.Model == "iPhone 15 Pro") with
        { LogicalResolution = new Size(430, 932), NotchFrame = new Rect(152, 11, 126, 36) };
        using var app = new TestSinglePageApp<Application, MainWindow>(new Size(478, 996),
            configure: services => services.AddSingleton<ITestSettings>(new TestSettings()),
            runtimeVariant: TestRuntimeVariant.IPhoneEmulator, emulatorDevice: device,
            snapshotSet: "Scenarios.Emulator.largeIPhone", fontSource: Xui.Core.Fonts.Inter.Source);
        app.SvgFontResolver = new EmbeddedFonts();
        app.Render();
        var button = app.Window.RootView.FindViewById("Gestures")!;
        Assert.True(button.TryFindParent<NavigationView>(out var navigation));
        Assert.IsAssignableFrom<Xui.DevKit.UI.Widgets.Button>(button).Clicked!();
        long time = 0;
        void Advance(int milliseconds = 350)
        {
            app.AnimationFrame(TimeSpan.FromMilliseconds(time), TimeSpan.FromMilliseconds(time));
            app.AnimationFrame(TimeSpan.FromMilliseconds(time), TimeSpan.FromMilliseconds(time + milliseconds));
            time += milliseconds; app.Render();
        }
        Advance();
        View Find(string id) => app.Window.RootView.FindViewById(id)!;
        Point Center(View view) => view.Parent!.LocalToGlobal(view.Frame.Center);
        TransformView Scrolled(View view) => Assert.IsAssignableFrom<TransformView>(view[0]);
        var carousel = Find("InputCarousel");
        Assert.True(carousel.TryFindParent<GesturesExample>(out var gallery));
        Assert.Equal(6, gallery.SectionCount);
        void Select(int index) { gallery.SelectSection(index); Advance(); Assert.Equal(index, gallery.SelectedSectionIndex); }
        app.MarkdownHeading("Input: touch, intent, and motion", 1);
        app.MarkdownParagraph("Six tall cards demonstrate independent touch bubbles, nested horizontal and vertical scrolling, tap-versus-drag negotiation, a snapping temperature ruler, a vector floor plan with two-finger zoom, and reversible swipe actions. Swipe the title and description below each demo, or tap a page dot to change cards. Scrollable content reaches the card edges while its text and controls retain their inset.");
        app.Snapshot("GestureVisualizer");
        var bubbles = Assert.IsType<GestureVisualizer>(Find("GestureVisualizer"));
        var center = Center(bubbles);
        app.PointerDown(center + new Vector(-60, -35), 0);
        app.PointerDown(center + new Vector(60, 35), 1);
        app.PointerMove(center + new Vector(-40, -55), 0);
        Assert.Equal(2, bubbles.VisibleBubbleCount);
        Assert.Equal(0, gallery.SelectedSectionIndex); // The carousel must not steal a child's drag.
        var touches = app.Snapshot("TwoTouches"); Assert.Contains("MOVE", touches); Assert.Contains("DOWN", touches);
        Assert.Contains("TOUCH #0", touches); Assert.Contains("TOUCH #1", touches);
        app.PointerUp(center + new Vector(-40, -55), 0);
        app.PointerMove(center + new Vector(70, 25), 1);
        var independent = app.Render();
        XNamespace svg = "http://www.w3.org/2000/svg";
        var labels = XDocument.Parse(independent).Descendants(svg + "text").ToArray();
        Assert.Equal("UP", labels.Single(t => t.Value == "TOUCH #0").ElementsAfterSelf(svg + "text").First().Value);
        Assert.Equal("MOVE", labels.Single(t => t.Value == "TOUCH #1").ElementsAfterSelf(svg + "text").First().Value);
        Assert.Equal(2, bubbles.VisibleBubbleCount);
        app.PointerUp(center + new Vector(60, 35), 1);
        Advance(800); var fading = app.Snapshot("ReleasedBubbles"); Assert.Contains(">UP<", fading);
        Advance(1300); Assert.Equal(0, bubbles.VisibleBubbleCount);
        app.PointerDown(center, 3); app.PointerDown(center + new Vector(50, 0), 8);
        app.Touches(new Touch { Index = 3, Phase = TouchPhase.Cancel, Position = center });
        Assert.Null(app.Window.RootView.EventRouter.GetCapturedView(3));
        Assert.Same(bubbles, app.Window.RootView.EventRouter.GetCapturedView(8));
        app.PointerMove(center + new Vector(70, 0), 8);
        Assert.Contains(">CANCEL<", app.Render());
        app.PointerUp(center + new Vector(70, 0), 8); Advance(2100);
        Assert.Equal(0, bubbles.VisibleBubbleCount);
        // A new burst after idle gets its own full two-second lifetime.
        Advance(5000); app.PointerDown(center, 0); app.PointerUp(center, 0);
        Advance(1000); Assert.Equal(1, bubbles.VisibleBubbleCount);
        Advance(1100); Assert.Equal(0, bubbles.VisibleBubbleCount);
        app.PointerDown(center, 1);
        for (var i = 0; i < 100; i++) { app.PointerDown(center, 0); app.PointerUp(center, 0); }
        Assert.Equal(64, bubbles.VisibleBubbleCount); // Bounded particles preserve held contacts.
        Advance(2100); Assert.Equal(1, bubbles.VisibleBubbleCount);
        app.PointerUp(center, 1); Advance(2100); Assert.Equal(0, bubbles.VisibleBubbleCount);

        // The large title/description area is below the demo and pages near the thumb.
        var details = Find("GestureVisualizer.Details");
        Assert.True(details.Frame.Top > bubbles.Frame.Bottom);
        var heading = Center(details);
        app.PointerDown(heading); app.PointerMove(heading + new Vector(-180, 0)); app.PointerUp(heading + new Vector(-180, 0)); Advance();
        Assert.Equal(1, gallery.SelectedSectionIndex);
        app.Snapshot("NestedScrolls");
        var shelf = Find("DestinationShelf0"); var feed = Find("DestinationFeed");
        var scrollCard = Find("NestedScrollDemo.Card");
        Assert.Equal(scrollCard.Frame.Width, feed.Frame.Width);
        Assert.Equal(feed.Frame.Width, shelf.Frame.Width);
        Assert.Equal(scrollCard.Parent!.LocalToGlobal(scrollCard.Frame.TopLeft).X,
            feed.Parent!.LocalToGlobal(feed.Frame.TopLeft).X);
        Assert.True(Find("NestedScrollDemo.Category").Frame.Bottom < feed.Frame.Top);
        Assert.True(Assert.IsType<ScrollView>(shelf).ScrollbarEndInset > 0);
        Assert.True(Assert.IsType<ScrollView>(shelf).Content!.Margin.Left > 0);
        var shelfCenter = Center(shelf);
        app.PointerDown(shelfCenter); app.PointerMove(shelfCenter + new Vector(-80, 0));
        Assert.True(Scrolled(shelf).Transform.Tx < -60); Assert.Equal((nfloat)0, Scrolled(feed).Transform.Ty);
        var shelfContent = Assert.IsType<ScrollView>(shelf).Content!;
        Assert.True(shelfContent.LocalToGlobal(shelfContent.Frame.TopLeft).X <
            shelf.Parent!.LocalToGlobal(shelf.Frame.TopLeft).X); // Leading inset scrolls away, viewport stays at the edge.
        Assert.Equal(1, gallery.SelectedSectionIndex);
        app.PointerUp(shelfCenter + new Vector(-80, 0));
        app.PointerDown(shelfCenter); app.PointerMove(shelfCenter + new Vector(0, -16)); app.PointerMove(shelfCenter + new Vector(0, -110));
        Assert.True(Scrolled(feed).Transform.Ty < -50);
        app.Snapshot("NestedVerticalScroll"); app.PointerUp(shelfCenter + new Vector(0, -110));

        Select(2); app.Snapshot("DragVsTap");
        var save = Find("SaveSketch1"); var saveCenter = Center(save);
        var saveButton = Assert.IsAssignableFrom<Xui.DevKit.UI.Widgets.Button>(save);
        var saveAction = saveButton.Clicked; var saves = 0;
        saveButton.Clicked = () => { saves++; saveAction!(); };
        app.PointerDown(saveCenter);
        app.Touches(new Touch { Index = 0, Phase = TouchPhase.Cancel, Position = saveCenter });
        Assert.Equal(0, saves);
        Assert.Null(app.Window.RootView.EventRouter.GetCapturedView(0));
        app.PointerDown(saveCenter); app.PointerUp(saveCenter);
        Assert.Contains("Idea 1 saved", Assert.IsType<Label>(Find("SketchStatus")).Text);
        var tapped = XDocument.Parse(app.Render()).Descendants(svg + "text").Single(t => t.Value == "Tapped!");
        var tapY = (double)tapped.Attribute("y")!;
        Advance(200);
        var floating = XDocument.Parse(app.Render()).Descendants(svg + "text").Single(t => t.Value == "Tapped!");
        Assert.True((double)floating.Attribute("y")! < tapY);
        var sketchFeed = Find("SketchFeed"); var pad = Find("SketchPad1"); var padCenter = Center(pad);
        Assert.Equal(Find("DragVsTapDemo.Card").Frame.Width, sketchFeed.Frame.Width);
        var originalY = Scrolled(sketchFeed).Transform.Ty;
        app.PointerDown(padCenter + new Vector(-70, 20));
        app.PointerMove(padCenter + new Vector(-30, -25)); app.PointerMove(padCenter + new Vector(10, 15));
        app.PointerMove(padCenter + new Vector(65, -30)); app.PointerUp(padCenter + new Vector(65, -30));
        Assert.Equal(originalY, Scrolled(sketchFeed).Transform.Ty);
        app.Snapshot("SketchSaved");
        Advance(1300); Assert.DoesNotContain(">Tapped!<", app.Render());

        // A horizontal drag over a button pages the cards without clicking or scrolling the feed.
        app.PointerDown(saveCenter); app.PointerMove(saveCenter + new Vector(-180, 0));
        app.PointerUp(saveCenter + new Vector(-180, 0)); Advance();
        Assert.Equal(3, gallery.SelectedSectionIndex); Assert.Equal(1, saves);
        Assert.Equal(originalY, Scrolled(sketchFeed).Transform.Ty);
        Select(2);
        // The feed's unclaimed space also yields horizontally, not only its buttons.
        var statusCenter = Center(Find("SketchStatus"));
        app.PointerDown(statusCenter); app.PointerMove(statusCenter + new Vector(-180, -5));
        app.PointerUp(statusCenter + new Vector(-180, -5)); Advance();
        Assert.Equal(3, gallery.SelectedSectionIndex); Assert.Equal(originalY, Scrolled(sketchFeed).Transform.Ty);
        Select(2);
        app.PointerDown(saveCenter); app.PointerMove(saveCenter + new Vector(0, -16)); app.PointerMove(saveCenter + new Vector(0, -100));
        Assert.True(Scrolled(sketchFeed).Transform.Ty < originalY - 50);
        app.PointerUp(saveCenter + new Vector(0, -100));
        Assert.Equal(1, saves); // Dragging over the button must cancel its tap.

        Select(3); app.Snapshot("Temperature");
        var dial = Assert.IsType<TemperatureDial>(Find("TemperatureDial")); var dialCenter = Center(dial);
        app.PointerDown(dialCenter); Advance(16);
        for (var i = 1; i <= 5; i++)
        {
            app.PointerMove(dialCenter + new Vector(-17 * i, 0)); Advance(16);
        }
        var releasedTemperature = dial.Temperature;
        app.PointerUp(dialCenter + new Vector(-85, 0)); Advance(120);
        Assert.True(dial.Temperature > releasedTemperature); // Continues in the release direction.
        // A fresh touch immediately arrests the coast.
        app.PointerDown(dialCenter); var interrupted = dial.Temperature; Advance(200);
        Assert.Equal(interrupted, dial.Temperature);
        app.PointerUp(dialCenter); Advance(350);
        Assert.True(dial.Temperature > 23); Assert.Equal(nfloat.Round(dial.Temperature * 2), dial.Temperature * 2);
        Assert.Equal(3, gallery.SelectedSectionIndex); app.Snapshot("TemperatureAdjusted");
        app.PointerDown(dialCenter); Advance(16);
        for (var i = 1; i <= 4; i++)
        {
            app.PointerMove(dialCenter + new Vector(16 * i, 0)); Advance(16);
        }
        var coolingRelease = dial.Temperature;
        app.PointerUp(dialCenter + new Vector(64, 0)); Advance(800); Advance(350);
        Assert.True(dial.Temperature < coolingRelease);
        Assert.InRange(dial.Temperature, (nfloat)16, (nfloat)30);
        Assert.Equal(nfloat.Round(dial.Temperature * 2), dial.Temperature * 2);

        Select(4); app.Snapshot("FloorPlan");
        var plan = Assert.IsType<FloorPlan>(Find("FloorPlan")); var planCenter = Center(plan);
        app.PointerDown(planCenter + new Vector(-30, -20)); app.PointerUp(planCenter + new Vector(-30, -20));
        Assert.True(plan.SelectedRoom >= 0);
        app.PointerDown(planCenter + new Vector(-45, 0), 0); app.PointerDown(planCenter + new Vector(45, 0), 1);
        app.PointerMove(planCenter + new Vector(-85, 0), 0); app.PointerMove(planCenter + new Vector(85, 0), 1);
        Assert.True(plan.Zoom > 1.7f); Assert.Equal(4, gallery.SelectedSectionIndex);
        app.Snapshot("PinchZoom");
        app.PointerUp(planCenter + new Vector(-85, 0), 0);
        var panBefore = plan.Pan;
        app.PointerMove(planCenter + new Vector(100, 20), 1);
        Assert.True(plan.Pan.X != panBefore.X || plan.Pan.Y != panBefore.Y);
        app.PointerUp(planCenter + new Vector(100, 20), 1);
        app.Snapshot("OneFingerContinuesPan");

        Select(5); app.Snapshot("Inbox");
        Assert.Equal(Find("SwipeActions.Card").Frame.Width, Find("InboxFeed").Frame.Width);
        var inbox = Assert.IsType<SwipeActions>(Find("SwipeActions")); var message = Find("SwipeMessage0"); var messageCenter = Center(message);
        app.PointerDown(messageCenter); app.PointerMove(messageCenter + new Vector(-80, 0)); app.PointerUp(messageCenter + new Vector(-80, 0)); Advance();
        Assert.Equal(5, gallery.SelectedSectionIndex); app.Snapshot("ArchiveRevealed");
        var action = message.Parent!.LocalToGlobal(new Point(message.Frame.Right - 35, message.Frame.Center.Y));
        app.PointerDown(action); app.PointerUp(action); Advance();
        Assert.Equal(1, inbox.ArchivedCount); app.Snapshot("Archived");
        app.PointerDown(messageCenter); app.PointerUp(messageCenter); Advance(); Assert.Equal(0, inbox.ArchivedCount);
        var inboxFeed = Find("InboxFeed");
        app.PointerDown(messageCenter); app.PointerMove(messageCenter + new Vector(0, -16));
        app.PointerMove(messageCenter + new Vector(0, -60));
        Assert.True(Scrolled(inboxFeed).Transform.Ty < 0);
        app.PointerUp(messageCenter + new Vector(0, -60)); Assert.Equal(0, inbox.ArchivedCount);
        var firstDot = new Point(carousel.Frame.Center.X - 32, carousel[0].Frame.Bottom + 24);
        app.PointerDown(firstDot); app.PointerUp(firstDot); Advance(); Assert.Equal(0, gallery.SelectedSectionIndex);
        app.PointerDown((10, 450)); app.PointerMove((300, 450)); app.PointerUp((300, 450)); Advance();
        Assert.False(navigation.CanGoBack);
    }
    private sealed class EmbeddedFonts : SvgDrawingContext.SvgFontResolver
    {
        public override SvgDrawingContext.Resolved Resolve(FontFace face, Uri? uri) => new(SvgDrawingContext.SvgFontMode.Embedded, null);
    }
}
