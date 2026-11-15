using Microsoft.Extensions.DependencyInjection;
using Xui.Apps.TestApp;
using Xui.Apps.TestApp.Examples;
using Xui.Core.DI;
using Xui.Core.UI;
using Xui.Runtime.Test;

namespace Xui.Tests.Integration.TestApp;

public class TestHarnessTest
{
    [Fact]
    public void DesktopVariantSuppliesDesktopIdentityAndShell()
    {
        using var temp = new TemporaryScenario();
        using var app = temp.Create();
        app.Run(() =>
        {
            app.Render();
            var device = Assert.IsAssignableFrom<IDeviceInfo>(app.Window.GetService(typeof(IDeviceInfo)));
            Assert.Equal(DeviceFormFactor.Desktop, device.FormFactor);
            Assert.Equal(PointerModel.Mouse, device.PointerModel);
            app.RequireView("Layers", "verifying the desktop-only menu");
            Assert.Null(app.Window.RootView.FindViewById("Basics"));
        });
    }

    [Fact]
    public void ViewClickUsesAncestorTransformsAndReachesTheIntendedPage()
    {
        using var temp = new TemporaryScenario();
        using var app = temp.Create(TestRuntimeVariant.IPhoneEmulator);
        app.Run(() =>
        {
            app.Render();
            var button = app.RequireView("TextMetrics", "testing transformed input");
            Assert.NotEqual(button.Frame.Center, TestSinglePageApp<Application, MainWindow>.ViewCenter(button));
            app.MouseDown(button); app.MouseUp(button); app.Advance(TimeSpan.FromMilliseconds(400));
            Assert.IsType<TextMetricsExample>(app.RequireView("Example.TextMetricsExample", "verifying transformed input"));
            Assert.Null(app.Window.RootView.FindViewById("CanvasAnimationsCarousel"));
        });
    }

    [Fact]
    public void MissingTargetPreservesItsIdAndActionDespiteEarlierSnapshotMismatch()
    {
        using var temp = new TemporaryScenario();
        using var app = temp.Create();
        temp.WriteMismatchedBaseline();
        var error = Assert.Throws<InvalidOperationException>(() => app.Run(() =>
        {
            app.Snapshot("Mismatch");
            app.RequireView("Missing.Target", "opening the test page");
        }));
        Assert.Contains("Missing.Target", error.Message);
        Assert.Contains("opening the test page", error.Message);
        app.Dispose(); // Must not replace the scenario error with a snapshot error.
        Assert.True(File.Exists(Path.Combine(temp.ScenarioPath, "README.DIFF.md")));
    }

    [Fact]
    public void SnapshotMismatchStillFailsWhenScenarioAssertionsSucceed()
    {
        using var temp = new TemporaryScenario();
        using var app = temp.Create();
        temp.WriteMismatchedBaseline();
        app.Run(() => app.Snapshot("Mismatch"));
        var error = Assert.Throws<Exception>(() => app.Dispose());
        Assert.Contains("Snapshot assertion failed", error.Message);
    }

    private sealed class TemporaryScenario : IDisposable
    {
        private readonly string directory = Path.Combine(Path.GetTempPath(), "xui-harness-" + Guid.NewGuid().ToString("N"));
        public string ScenarioPath => Path.Combine(directory, "Scenarios", "Harness");
        public TestSinglePageApp<Application, MainWindow> Create(TestRuntimeVariant variant = TestRuntimeVariant.Desktop) =>
            new((600, 600), configure: services => services.AddSingleton<ITestSettings>(new TestSettings()),
                runtimeVariant: variant, callerPath: Path.Combine(directory, "Harness.cs"), testName: "Harness");
        public void WriteMismatchedBaseline()
        {
            Directory.CreateDirectory(ScenarioPath);
            File.WriteAllText(Path.Combine(ScenarioPath, "01.Mismatch.Render.svg"), "<svg/>");
        }
        public void Dispose() { if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true); }
    }
}
