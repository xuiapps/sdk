using Microsoft.Extensions.DependencyInjection;
using Xui.Apps.TestApp;
using Xui.Apps.TestApp.Pages;
using Xui.Apps.TestApp.Pages.FloorplanGpu;
using Xui.Core.UI;
using Xui.DevKit.UI.Mobile.Navigation;
using Xui.Runtime.Test;

namespace Xui.Tests.Integration.TestApp;

public class FloorplanGpuTest
{
    [Fact]
    public async Task FloorplanGpu_LoadsAfterNavigationAndKeepsTextVector()
    {
        var model = await FloorplanGpuModel.LoadSharedAsync();
        using var app = new TestSinglePageApp<Application, MainWindow>((478, 996),
            configure: s => s.AddSingleton<ITestSettings>(new TestSettings()),
            runtimeVariant: TestRuntimeVariant.IPhoneEmulator, fontSource: Xui.Core.Fonts.Inter.Source);
        app.Run(() =>
        {
            app.Render();
            var button = app.RequireView("FloorplanGpu", "opening the GPU floorplan");
            Assert.True(button.TryFindParent<NavigationView>(out var navigation));
            var loading = new TaskCompletionSource<FloorplanGpuModel>();
            navigation.Push(new MobileExamplePage<FloorplanGpuExample>(new(loading.Task)));
            app.Render();
            loading.SetResult(model);
            app.Advance(TimeSpan.FromMilliseconds(100));
            Assert.True(navigation.IsTransitioning);
            Assert.Null(app.Window.RootView.FindViewById("FloorplanGpu.Map"));
            app.Advance(TimeSpan.FromMilliseconds(600));
            app.Advance(TimeSpan.FromMilliseconds(16));
            var map = Assert.IsType<FloorplanGpuView>(app.RequireView("FloorplanGpu.Map", "checking GPU model hydration"));
            Assert.False(navigation.IsTransitioning);
            app.MarkdownHeading("Floorplan GPU", 1);
            app.MarkdownParagraph("Generated furniture atlas and wall/door quad meshes, with vector text. The test executes the same C# shader in software.");
            app.Snapshot("Overview");
            // Zoom into furnished offices, not the empty central stair/lift area.
            var visible = map.VisibleWorld;
            var anchor = new Xui.Core.Math2D.Point(map.Frame.X + (210 - visible.X) / visible.Width * map.Frame.Width,
                map.Frame.Y + (520 - visible.Y) / visible.Height * map.Frame.Height);
            map.ZoomAt(anchor, 12);
            app.Advance(TimeSpan.FromMilliseconds(800));
            Assert.Equal(3, map.Transition.Current);
            Assert.False(map.Transition.Active);
            app.Snapshot("Detail");
            Assert.Equal(0, map.Count);
            var document = System.Xml.Linq.XDocument.Parse(app.Render());
            var text = document.Descendants(System.Xml.Linq.XName.Get("text", "http://www.w3.org/2000/svg")).Select(e => e.Value).ToArray();
            Assert.Contains(text, value => model.Labels[3].Any(label => label.Text == value));
        });
    }
}
