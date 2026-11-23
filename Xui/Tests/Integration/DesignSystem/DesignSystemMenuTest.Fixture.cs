using Xui.Tests.Integration.TestApp;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Xui.Core.Canvas;
using Xui.Core.Abstract.Events;
using Xui.DevKit.UI.Design;
using Xui.Runtime.Software.Actual;
using Xui.Runtime.Software.Font;
using Xui.Middleware.EmulatorLegacy.Devices;
using Microsoft.Extensions.DependencyInjection;
using Xui.Apps.TestApp;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.DevKit.UI.Widgets;
using Xui.Runtime.Test;

namespace Xui.Tests.Integration.DesignSystem;

public partial class DesignSystemMenuTest
{
    private sealed class Scene : IDisposable
    {
        public TestSinglePageApp<Application, MainWindow> App { get; }
        private TimeSpan time;
        public ScrollView Scroll => Assert.IsAssignableFrom<ScrollView>(App.Window.RootView.FindViewById("DesignSystem.PaletteScroll"));

        public Scene(TestRuntimeVariant variant = TestRuntimeVariant.IPhoneEmulator, int? width = null, [CallerMemberName] string name = "", [CallerFilePath] string path = "")
        {
            App = width is null ? IntegrationRuntimeVariants.CreateApp<Application, MainWindow>((600, 400),
                variant, path, name, fontSource: Xui.Core.Fonts.Inter.Source) :
                new TestSinglePageApp<Application, MainWindow>((width.Value + 48, 916),
                    configure: services => services.AddSingleton<ITestSettings>(new TestSettings()),
                    runtimeVariant: TestRuntimeVariant.IPhoneEmulator,
                    emulatorDevice: DeviceCatalog.All.Single(d => d.Model == "iPhone 15 Pro") with { LogicalResolution = new Size(width.Value, 852) },
                    fontSource: Xui.Core.Fonts.Inter.Source);
            App.Render();
            Assert.IsAssignableFrom<Button>(App.Window.RootView.FindViewById("DesignSystem")).Clicked!();
            Advance(0);
            Advance(400);
        }

        public void Advance(int milliseconds)
        {
            var next = time + TimeSpan.FromMilliseconds(milliseconds);
            App.AnimationFrame(time, next);
            time = next;
            App.Render();
        }

        public View Find(string id) => Assert.IsAssignableFrom<View>(App.Window.RootView.FindViewById(id));

        public Point Cell(View view, int index, int count) => view.Parent!.LocalToGlobal(
            new Point(view.Frame.Left + (index + (nfloat)0.5) * view.Frame.Width / count, view.Frame.Center.Y));

        public void TapCell(View view, int index, int count, bool settle = true)
        {
            var point = Cell(view, index, count);
            App.PointerDown(point);
            Advance(20);
            App.PointerUp(point);
            Advance(16);
            if (settle) Advance(900);
        }

        public void Tap(View view, bool header = false)
        {
            var point = view.Parent!.LocalToGlobal(header ? new Point(view.Frame.Center.X, view.Frame.Top + 20) : view.Frame.Center);
            App.PointerDown(point);
            Advance(20);
            App.PointerUp(point);
            Advance(16);
        }

        public void Reveal(View view)
        {
            Assert.True(view.TryFindParent<ScrollView>(out var scroll));
            for (var i = 0; i < 16; i++)
            {
                var top = view.Parent!.LocalToGlobal(view.Frame.TopLeft).Y;
                var bottom = view.Parent!.LocalToGlobal(new Point(view.Frame.Left, view.Frame.Bottom)).Y;
                var viewportTop = scroll.Parent!.LocalToGlobal(scroll.Frame.TopLeft).Y;
                var tabs = Find("DesignSystem.Tabs");
                var viewportBottom = nfloat.Min(scroll.Parent!.LocalToGlobal(new Point(scroll.Frame.Left, scroll.Frame.Bottom)).Y,
                    tabs.Parent!.LocalToGlobal(tabs.Frame.TopLeft).Y);
                if (top >= viewportTop + 8 && bottom <= viewportBottom - 8) return;
                var delta = nfloat.Clamp((top + bottom) / 2 - (viewportTop + viewportBottom) / 2, -220, 220);
                var start = scroll.Parent!.LocalToGlobal(new Point(scroll.Frame.Left + 5, scroll.Frame.Center.Y));
                App.PointerDown(start);
                Advance(30);
                App.PointerMove(start - new Vector(0, delta));
                Advance(200);
                App.PointerUp(start - new Vector(0, delta));
                Advance(16);
                Advance(1000);
            }
            Assert.Fail("Could not reveal " + view.Id);
        }

        public void Dispose() => App.Dispose();
    }
}
