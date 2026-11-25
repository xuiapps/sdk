using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Xml.Linq;
using Xui.Apps.TestApp;
using Xui.Apps.TestApp.Pages;
using Xui.Apps.TestApp.Pages.Floorplan;
using Xui.Core.Abstract.Events;
using Xui.Core.Canvas;
using Xui.Core.Canvas.Composition.Commands;
using Xui.Core.Canvas.Composition;
using Xui.Core.Canvas.Composition.Graphics;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.DevKit.UI.Mobile.Navigation;
using Xui.Runtime.Software.Actual;
using Xui.Runtime.Test;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Tests.Integration.TestApp;

public partial class FloorplanPerformanceTest
{
    private sealed class Scene : IDisposable
    {
        public TestSinglePageApp<Application, MainWindow> App { get; }
        public FloorplanView Map { get; }
        private int time;
        public Scene(TestRuntimeVariant variant = TestRuntimeVariant.IPhoneEmulator, bool finishNavigation = true)
        {
            App = new((478, 996), configure: s => s.AddSingleton<ITestSettings>(new TestSettings()),
                runtimeVariant: variant, fontSource: Xui.Core.Fonts.Inter.Source, testName: "Floorplan_RenderReference");
            App.Render();
            var link = App.RequireView("OfficeFloorplan", "opening the floorplan performance example");
            Assert.True(link.TryFindParent<NavigationView>(out var navigation));
            navigation.Push(new MobileExamplePage<FloorplanExample>(new(Task.FromResult(FloorplanDrawing.Shared))));
            if (finishNavigation) Advance();
            else App.Render();
            Map = Assert.IsType<FloorplanView>(App.Window.RootView.FindViewById("Floorplan.Map"));
        }
        public void Advance()
        {
            App.AnimationFrame(TimeSpan.FromMilliseconds(time), TimeSpan.FromMilliseconds(time));
            App.AnimationFrame(TimeSpan.FromMilliseconds(time), TimeSpan.FromMilliseconds(time + 500));
            time += 500; App.Render();
        }
        public void Tick(int milliseconds)
        {
            App.AnimationFrame(TimeSpan.FromMilliseconds(time), TimeSpan.FromMilliseconds(time + milliseconds));
            time += milliseconds;
        }
        public void Dispose() => App.Dispose();
    }
}
