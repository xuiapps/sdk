using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xui.Core.Abstract;
using Xui.Core.Abstract.Events;
using Xui.Core.Actual;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Middleware.EmulatorLegacy.Actual;
using Xui.Middleware.EmulatorLegacy.Devices;
using Xui.Runtime.Software.Actual;
using Xui.Runtime.Software.Font;
using Xui.Runtime.Test.Actual;

namespace Xui.Runtime.Test;

/// <summary>
/// A unit-test platform harness that boots a real <see cref="Application"/> subclass
/// via the host DI container and exposes synchronous methods to drive input, animation, and rendering.
/// <para>
/// Like the Browser platform, <c>Run()</c> calls <c>Start()</c> and returns immediately.
/// The test then drives events manually — no OS event loop is involved.
/// </para>
/// </summary>
public class TestSinglePageApp<TApplication, TWindow> : IDisposable
    where TApplication : Application
    where TWindow : Window
{
    private readonly TestPlatform platform;
    private readonly FrameTimeProvider inputClock = new();
    private readonly Xui.Core.Abstract.IWindow renderWindow;
    private readonly EmulatorWindow? emulatorWindow;
    private readonly FixedClock? fixedClock;
    private readonly IHost host;
    private readonly Xui.Core.Canvas.IFontSource? fontSource;
    private readonly string snapshotsDir;
    private readonly List<SnapshotEntry> snapshots = new();
    private readonly List<ReportEntry> reportEntries = new();
    private readonly Dictionary<int, Touch> activeTouchContacts = new();
    private int snapshotCounter;
    private Point mousePosition;
    private bool mouseLeftPressed;
    private bool hasMouseInteraction;
    private bool disposed;
    private TimeSpan lastFramePrevious;
    private TimeSpan lastFrameNext;

    /// <summary>
    /// The first (and typically only) window created by the application.
    /// </summary>
    public Window Window { get; }

    /// <summary>
    /// The window size used for rendering.
    /// </summary>
    public Size Size { get; }

    /// <summary>Optional font emission policy for SVG snapshots; null preserves system-font output.</summary>
    public SvgDrawingContext.SvgFontResolver? SvgFontResolver { get; set; }

    public IClock Clock => host.Services.GetRequiredService<IClock>();

    public IRandom Random => host.Services.GetRequiredService<IRandom>();

    /// <summary>
    /// Creates a test harness that boots <typeparamref name="TApplication"/> via a host with
    /// <see cref="TestPlatform"/> registered as <see cref="Xui.Core.Actual.IRuntime"/>.
    /// <typeparamref name="TApplication"/> and <typeparamref name="TWindow"/> are registered
    /// automatically as scoped services.
    /// Snapshot artifacts are written to a <c>Scenarios/{testName}/</c> folder next to the
    /// calling test file.
    /// </summary>
    public TestSinglePageApp(
        Size windowSize,
        Action<IServiceCollection>? configure = null,
        TestRuntimeVariant runtimeVariant = TestRuntimeVariant.Desktop,
        DeviceProfile? emulatorDevice = null,
        EmulatorStatusBarStyle? emulatorStatusBarStyleOverride = null,
        string? snapshotSet = null,
        [CallerFilePath] string callerPath = "",
        [CallerMemberName] string testName = "",
        Xui.Core.Canvas.IFontSource? fontSource = null)
    {
        this.fontSource = fontSource;
        this.Size = windowSize;
        this.platform = new TestPlatform();
        IRuntime runtime = this.platform;
        var isEmulator = runtimeVariant != TestRuntimeVariant.Desktop;
        if (isEmulator)
            runtime = new EmulatorPlatform(this.platform);

        if (isEmulator)
            this.fixedClock = new FixedClock(new DateTime(2025, 4, 1, 9, 41, 0));

        IRandom? random = isEmulator
            ? new SeededRandom(41)
            : null;
        IClock clock = (IClock?)this.fixedClock ?? SystemClock.Default;

        this.host = new HostBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton(runtime);
                services.AddSingleton<IClock>(clock);
                services.AddSingleton<TimeProvider>(this.inputClock);
                services.AddSingleton<IRandom>(random ?? SystemRandom.Default);
                services.AddScoped<TApplication>();
                services.AddScoped<TWindow>();
                configure?.Invoke(services);
            })
            .Build();

        this.host.Start();
        var application = this.host.Services.GetRequiredService<TApplication>();
        application.Run();

        var createdWindow = this.platform.Windows[this.platform.Windows.Count - 1];
        if (createdWindow.Abstract is EmulatorHostWindow emulatorHost)
        {
            var emulator = emulatorHost.Emulator;
            this.emulatorWindow = emulator;
            if (emulatorDevice.HasValue)
                this.emulatorWindow.CurrentDevice = emulatorDevice.Value;
            this.emulatorWindow.StatusBarStyleOverride = emulatorStatusBarStyleOverride ?? EmulatorStatusBarStyle.Deterministic;
            this.emulatorWindow.RenderTouchIndicators = false;
            // Snapshot tests render the isolated phone surface. Keep their synthetic input
            // on that same virtual surface; host-window routing is exercised by real runtimes.
            this.renderWindow = emulator;
            this.Window = (Window)emulator.AppWindow;
        }
        else
        {
            this.Window = (Window)createdWindow.Abstract;
            this.Window.DisplayArea = new Rect(0, 0, windowSize.Width, windowSize.Height);
            this.Window.SafeArea = this.Window.DisplayArea;
            this.renderWindow = this.Window;
        }

        this.snapshotsDir = snapshotSet is null
            ? Path.Combine(Path.GetDirectoryName(callerPath)!, "Scenarios", testName)
            : Path.Combine(Path.GetDirectoryName(callerPath)!, snapshotSet, testName);
        Directory.CreateDirectory(this.snapshotsDir);

        // Provide a software text measure context so pointer events can hit-test text positions.
        var testWindow = this.platform.Windows[this.platform.Windows.Count - 1];
        testWindow.TextMeasureContext = new SoftwareTextMeasureContext(
            fontSource is null ? new Catalog(Xui.Core.Fonts.Inter.URIs) : new Catalog(fontSource));
    }

    // ── Input ────────────────────────────────────────────────────

    public void MouseMove(Point position)
    {
        if (emulatorWindow is not null)
        {
            this.hasMouseInteraction = true;
            if (activeTouchContacts.ContainsKey(0))
            {
                var mouseMove = new MouseMoveEventRef { Position = position };
                this.Window.OnMouseMove(ref mouseMove);
                PointerMove(position);
            }
            return;
        }

        this.mousePosition = position;
        this.hasMouseInteraction = true;
        var runtimePosition = MapInputPoint(position);
        var e = new MouseMoveEventRef { Position = runtimePosition };
        renderWindow.OnMouseMove(ref e);
    }

    public void MouseDown(Point position, MouseButton button = MouseButton.Left)
    {
        if (emulatorWindow is not null && button == MouseButton.Left)
        {
            var mouseDown = new MouseDownEventRef { Position = position, Button = button };
            this.Window.OnMouseDown(ref mouseDown);
            this.hasMouseInteraction = true;
            PointerDown(position);
            return;
        }

        this.mousePosition = position;
        this.hasMouseInteraction = true;
        if (button == MouseButton.Left) this.mouseLeftPressed = true;
        var runtimePosition = MapInputPoint(position);
        var e = new MouseDownEventRef { Position = runtimePosition, Button = button };
        renderWindow.OnMouseDown(ref e);
    }

    public void MouseUp(Point position, MouseButton button = MouseButton.Left)
    {
        if (emulatorWindow is not null && button == MouseButton.Left)
        {
            var mouseUp = new MouseUpEventRef { Position = position, Button = button };
            this.Window.OnMouseUp(ref mouseUp);
            this.hasMouseInteraction = true;
            PointerUp(position);
            return;
        }

        this.mousePosition = position;
        this.hasMouseInteraction = true;
        if (button == MouseButton.Left) this.mouseLeftPressed = false;
        var runtimePosition = MapInputPoint(position);
        var e = new MouseUpEventRef { Position = runtimePosition, Button = button };
        renderWindow.OnMouseUp(ref e);
    }

    public void MouseMove(View view) => MouseMove(view.Frame.Center);
    public void MouseDown(View view, MouseButton button = MouseButton.Left) => MouseDown(view.Frame.Center, button);
    public void MouseUp(View view, MouseButton button = MouseButton.Left) => MouseUp(view.Frame.Center, button);

    public void PointerDown(Point position, int index = 0, float radius = 0.5f) =>
        DispatchTouch([new Touch
        {
            Index = index,
            Phase = TouchPhase.Start,
            Position = position,
            Radius = radius
        }]);

    public void PointerMove(Point position, int index = 0, float radius = 0.5f)
    {
        if (!activeTouchContacts.ContainsKey(index))
            return;

        DispatchTouch([new Touch
        {
            Index = index,
            Phase = TouchPhase.Move,
            Position = position,
            Radius = radius
        }]);
    }

    public void PointerUp(Point position, int index = 0, float radius = 0.5f)
    {
        if (!activeTouchContacts.ContainsKey(index))
            return;

        DispatchTouch([new Touch
        {
            Index = index,
            Phase = TouchPhase.End,
            Position = position,
            Radius = radius
        }]);
    }

    public void Touches(params Touch[] touches)
    {
        if (touches.Length == 0)
            return;

        DispatchTouch(touches);
    }

    public void KeyDown(VirtualKey key, bool shift = false)
    {
        var e = new KeyEventRef { Key = key, Shift = shift };
        renderWindow.OnKeyDown(ref e);
    }

    public void Char(char character)
    {
        var e = new KeyEventRef { Character = character };
        renderWindow.OnChar(ref e);
    }

    /// <summary>
    /// Simulates typing a string by sending a <see cref="Char"/> event for each character.
    /// </summary>
    public void Type(string text)
    {
        foreach (var ch in text)
            Char(ch);
    }

    // ── Ticks ────────────────────────────────────────────────────

    /// <summary>
    /// Drains all pending callbacks from the dispatcher post queue.
    /// </summary>
    public void HandlePostActions()
    {
        while (this.platform.PostQueue.TryDequeue(out var action))
        {
            action();
        }
    }

    /// <summary>
    /// Sends a <see cref="FrameEventRef"/> to the window, driving <c>AnimateCore</c>
    /// on all views that requested an animation frame.
    /// </summary>
    public void AnimationFrame(TimeSpan previous, TimeSpan next)
    {
        this.inputClock.Timestamp = next.Ticks;
        this.lastFramePrevious = previous;
        this.lastFrameNext = next;
        var frame = new FrameEventRef(previous, next);
        renderWindow.OnAnimationFrame(ref frame);
    }

    // ── Render ───────────────────────────────────────────────────

    /// <summary>
    /// Renders the window to SVG and returns the SVG string.
    /// Use this for layout-only passes. For snapshot comparison, use <see cref="Snapshot"/>.
    /// </summary>
    public string Render()
    {
        using var stream = new MemoryStream();
        var renderSize = emulatorWindow?.SnapshotSize ?? this.Size;

        using (var context = new SvgDrawingContext(
            renderSize, stream,
            fontSource is null ? new Catalog(Xui.Core.Fonts.Inter.URIs) : new Catalog(fontSource),
            resolver: SvgFontResolver, keepOpen: true))
        {
            this.platform.CurrentDrawingContext = context;

            var frame = new FrameEventRef(this.lastFramePrevious, this.lastFrameNext);
            var rect = new Rect(0, 0, renderSize.Width, renderSize.Height);
            var render = new RenderEventRef(rect, frame, context);
            if (emulatorWindow is null)
                renderWindow.Render(ref render);
            else
                emulatorWindow.RenderSnapshot(ref render);

            this.platform.CurrentDrawingContext = null;
        } // Dispose flushes SVG footer before we read the stream

        stream.Position = 0;
        return new StreamReader(stream).ReadToEnd();
    }

    // ── Snapshots ────────────────────────────────────────────────

    /// <summary>
    /// Renders the window, injects a virtual cursor (if mouse events have been sent),
    /// saves the result as <c>{NN}.{name}.Render.DIFF.svg</c>, and compares against
    /// the corresponding expected SVG. Failures are collected and reported on
    /// <see cref="Dispose"/>, along with a markdown report.
    /// </summary>
    public string Snapshot(string name)
    {
        var svg = Render();

        if (this.hasMouseInteraction)
            svg = InjectInputOverlay(svg);

        this.snapshotCounter++;
        var prefix = $"{this.snapshotCounter:D2}.{name}.Render";
        var expectedFileName = $"{prefix}.svg";
        var diffFileName = $"{prefix}.DIFF.svg";
        var expectedPath = Path.Combine(this.snapshotsDir, expectedFileName);
        var legacyExpectedPath = Path.Combine(this.snapshotsDir, $"{prefix}.Expected.svg");
        var diffPath = Path.Combine(this.snapshotsDir, diffFileName);
        var legacyDiffPath = Path.Combine(this.snapshotsDir, $"{prefix}.Actual.svg");

        string? expectedSvg = null;
        string expectedImageFileName = expectedFileName;
        bool passed;
        if (File.Exists(expectedPath))
        {
            expectedSvg = File.ReadAllText(expectedPath);
            passed = NormalizeLineEndings(expectedSvg) == NormalizeLineEndings(svg);
        }
        else if (File.Exists(legacyExpectedPath))
        {
            expectedSvg = File.ReadAllText(legacyExpectedPath);
            File.WriteAllText(expectedPath, expectedSvg);
            expectedImageFileName = expectedFileName;
            passed = NormalizeLineEndings(expectedSvg) == NormalizeLineEndings(svg);
        }
        else
        {
            File.WriteAllText(expectedPath, svg);
            expectedSvg = svg;
            expectedImageFileName = expectedFileName;
            passed = false;
        }

        if (!passed)
        {
            File.WriteAllText(diffPath, svg);
            if (File.Exists(legacyDiffPath))
                File.Delete(legacyDiffPath);
        }
        else
        {
            if (File.Exists(diffPath))
                File.Delete(diffPath);
            if (File.Exists(legacyDiffPath))
                File.Delete(legacyDiffPath);
        }

        var entry = new SnapshotEntry(
            this.snapshotCounter,
            name,
            svg,
            expectedSvg,
            passed,
            expectedImageFileName,
            diffFileName);
        this.snapshots.Add(entry);
        this.reportEntries.Add(new SnapshotReportEntry(entry));
        return svg;
    }

    /// <summary>
    /// Appends a markdown heading to the test report.
    /// </summary>
    public void MarkdownHeading(string text, int level = 2)
    {
        level = Math.Clamp(level, 1, 6);
        this.reportEntries.Add(new MarkdownReportEntry($"{new string('#', level)} {text}"));
    }

    /// <summary>
    /// Appends a markdown paragraph to the test report.
    /// </summary>
    public void MarkdownParagraph(string text)
    {
        this.reportEntries.Add(new MarkdownReportEntry(text));
    }

    /// <summary>
    /// Appends a markdown list to the test report.
    /// </summary>
    public void MarkdownList(IEnumerable<string> items, bool ordered = false)
    {
        var sb = new StringBuilder();
        int index = 1;
        foreach (var item in items)
        {
            var prefix = ordered ? $"{index}. " : "- ";
            sb.AppendLine($"{prefix}{item}");
            index++;
        }

        this.reportEntries.Add(new MarkdownReportEntry(sb.ToString().TrimEnd()));
    }

    /// <summary>
    /// Appends a fenced code block to the test report.
    /// </summary>
    public void MarkdownCode(string code, string language = "")
    {
        var lang = string.IsNullOrWhiteSpace(language) ? string.Empty : language.Trim();
        var markdown = new StringBuilder();
        markdown.AppendLine($"```{lang}");
        markdown.AppendLine(code);
        markdown.AppendLine("```");
        this.reportEntries.Add(new MarkdownReportEntry(markdown.ToString().TrimEnd()));
    }

    /// <summary>
    /// Appends raw markdown content to the test report.
    /// </summary>
    public void MarkdownRaw(string markdown)
    {
        this.reportEntries.Add(new MarkdownReportEntry(markdown));
    }

    public void SetClockNow(DateTime now)
    {
        if (fixedClock is null)
            throw new InvalidOperationException("Fixed clock is only available for emulator runtime variant.");

        fixedClock.Set(now);
    }

    public void AdvanceClock(TimeSpan delta)
    {
        if (fixedClock is null)
            throw new InvalidOperationException("Fixed clock is only available for emulator runtime variant.");

        fixedClock.Advance(delta);
    }

    private Point MapInputPoint(Point appPoint) =>
        emulatorWindow is null ? appPoint : emulatorWindow.MapEmulatorToHost(appPoint, this.Size);

    private void DispatchTouch(ReadOnlySpan<Touch> touches)
    {
        this.hasMouseInteraction = true;
        UpdateTouchOverlay(touches);

        var e = new TouchEventRef(touches);
        renderWindow.OnTouch(ref e);
    }

    private void UpdateTouchOverlay(ReadOnlySpan<Touch> touches)
    {
        foreach (var touch in touches)
        {
            switch (touch.Phase)
            {
                case TouchPhase.Start:
                case TouchPhase.Move:
                    activeTouchContacts[touch.Index] = touch;
                    break;
                case TouchPhase.End:
                case TouchPhase.Cancel:
                    activeTouchContacts.Remove(touch.Index);
                    break;
            }
        }
    }

    private string InjectInputOverlay(string svg)
    {
        if (emulatorWindow is null)
            return InjectCursor(svg);

        return InjectTouches(svg);
    }

    private string InjectCursor(string svg)
    {
        var cursorPosition = emulatorWindow is null
            ? this.mousePosition
            : emulatorWindow.MapEmulatorToSnapshot(this.mousePosition);
        var x = ((double)cursorPosition.X).ToString(CultureInfo.InvariantCulture);
        var y = ((double)cursorPosition.Y).ToString(CultureInfo.InvariantCulture);
        var fill = this.mouseLeftPressed ? "#FFCC00" : "white";

        var cursorSvg =
            $"  <g transform=\"translate({x} {y})\" opacity=\"0.9\">\n" +
            $"    <polygon points=\"0,0 0,12 3,9 5,12 7,11 5,8 9,8\" fill=\"{fill}\" stroke=\"black\" stroke-width=\"0.7\"/>\n" +
            $"  </g>\n";

        var insertPos = svg.LastIndexOf("</svg>");
        if (insertPos >= 0)
            return svg.Insert(insertPos, cursorSvg);

        return svg;
    }

    private string InjectTouches(string svg)
    {
        if (activeTouchContacts.Count == 0)
            return svg;

        var sb = new StringBuilder();
        foreach (var touch in activeTouchContacts.Values)
        {
            var mapped = emulatorWindow!.MapEmulatorToSnapshot(touch.Position);
            var x = ((double)mapped.X).ToString(CultureInfo.InvariantCulture);
            var y = ((double)mapped.Y).ToString(CultureInfo.InvariantCulture);
            var radius = ((double)(touch.Radius * 30f)).ToString(CultureInfo.InvariantCulture);

            sb.AppendLine($"  <g transform=\"translate({x} {y})\" opacity=\"0.9\">");
            sb.AppendLine($"    <circle cx=\"0\" cy=\"0\" r=\"{radius}\" fill=\"#66888888\" />");
            sb.AppendLine($"    <circle cx=\"0\" cy=\"0\" r=\"{radius}\" fill=\"none\" stroke=\"#88AAAAAA\" stroke-width=\"3\" />");
            sb.AppendLine("  </g>");
        }

        var insertPos = svg.LastIndexOf("</svg>");
        if (insertPos >= 0)
            return svg.Insert(insertPos, sb.ToString());

        return svg;
    }

    // ── Markdown Report ──────────────────────────────────────────

    private bool GenerateTestRunMarkdown()
    {
        if (this.snapshots.Count == 0)
            return true;

        var expectedMarkdown = BuildExpectedMarkdown();
        var diffMarkdown = BuildDiffMarkdown();
        var expectedPath = Path.Combine(this.snapshotsDir, "README.md");
        var diffPath = Path.Combine(this.snapshotsDir, "README.DIFF.md");
        var legacyExpectedPath = Path.Combine(this.snapshotsDir, "TestRun.Expected.md");

        if (!File.Exists(expectedPath) && File.Exists(legacyExpectedPath))
            File.WriteAllText(expectedPath, expectedMarkdown);

        if (!File.Exists(expectedPath))
        {
            File.WriteAllText(expectedPath, expectedMarkdown);
            File.WriteAllText(diffPath, diffMarkdown);
            return false;
        }

        var expected = File.ReadAllText(expectedPath);
        var markdownMatches = NormalizeLineEndings(expected) == NormalizeLineEndings(expectedMarkdown);
        var hasSnapshotDiff = this.snapshots.Any(s => !s.Passed);
        if (markdownMatches && !hasSnapshotDiff)
        {
            if (File.Exists(diffPath))
                File.Delete(diffPath);
            return true;
        }

        File.WriteAllText(diffPath, diffMarkdown);
        return markdownMatches;
    }

    private string BuildExpectedMarkdown()
    {
        var markdown = new StringBuilder();
        markdown.AppendLine("# Integration Test Run");
        markdown.AppendLine();
        markdown.AppendLine("## Timeline");
        markdown.AppendLine();

        foreach (var entry in this.reportEntries)
        {
            switch (entry)
            {
                case MarkdownReportEntry md:
                    markdown.AppendLine(md.Markdown.TrimEnd());
                    markdown.AppendLine();
                    break;
                case SnapshotReportEntry snap:
                    AppendExpectedSnapshotMarkdown(markdown, snap.Snapshot);
                    break;
            }
        }

        return markdown.ToString().TrimEnd() + Environment.NewLine;
    }

    private string BuildDiffMarkdown()
    {
        var markdown = new StringBuilder();
        markdown.AppendLine("# Integration Test Run (Diff)");
        markdown.AppendLine();
        markdown.AppendLine("## Snapshot summary");
        markdown.AppendLine();
        foreach (var snapshot in this.snapshots)
            markdown.AppendLine($"- {(snapshot.Passed ? "✅" : "❌")} {snapshot.Index:D2}. {snapshot.Name}");
        markdown.AppendLine();
        markdown.AppendLine("## Timeline");
        markdown.AppendLine();

        foreach (var entry in this.reportEntries)
        {
            switch (entry)
            {
                case MarkdownReportEntry md:
                    markdown.AppendLine(md.Markdown.TrimEnd());
                    markdown.AppendLine();
                    break;
                case SnapshotReportEntry snap:
                    AppendDiffSnapshotMarkdown(markdown, snap.Snapshot);
                    break;
            }
        }

        return markdown.ToString().TrimEnd() + Environment.NewLine;
    }

    private static void AppendExpectedSnapshotMarkdown(StringBuilder markdown, SnapshotEntry snapshot)
    {
        markdown.AppendLine($"### {snapshot.Index:D2}. {snapshot.Name}");
        markdown.AppendLine();
        markdown.AppendLine($"<img src=\"./{snapshot.ExpectedImageFileName}\" alt=\"{snapshot.Index:D2}. {snapshot.Name} expected\" />");
        markdown.AppendLine();
    }

    private static void AppendDiffSnapshotMarkdown(StringBuilder markdown, SnapshotEntry snapshot)
    {
        markdown.AppendLine($"### {snapshot.Index:D2}. {snapshot.Name}");
        markdown.AppendLine();
        markdown.AppendLine("#### Expected");
        markdown.AppendLine();
        markdown.AppendLine($"<img src=\"./{snapshot.ExpectedImageFileName}\" alt=\"{snapshot.Index:D2}. {snapshot.Name} expected\" />");
        markdown.AppendLine();
        markdown.AppendLine("#### Actual");
        markdown.AppendLine();
        if (snapshot.Passed)
            markdown.AppendLine($"<img src=\"./{snapshot.ExpectedImageFileName}\" alt=\"{snapshot.Index:D2}. {snapshot.Name} actual (matches expected)\" />");
        else
            markdown.AppendLine($"<img src=\"./{snapshot.DiffImageFileName}\" alt=\"{snapshot.Index:D2}. {snapshot.Name} actual\" />");
        markdown.AppendLine();
    }

    private static string NormalizeLineEndings(string text) =>
        text.ReplaceLineEndings("\n");

    // ── Cleanup ──────────────────────────────────────────────────

    /// <summary>
    /// Drains remaining post actions and closes all windows.
    /// </summary>
    public void Quit()
    {
        HandlePostActions();
        renderWindow.Closed();
    }

    public void Dispose()
    {
        if (!disposed)
        {
            disposed = true;
            var markdownPassed = GenerateTestRunMarkdown();
            Quit();

            this.host.Dispose();

            var failures = this.snapshots.Where(s => !s.Passed).ToList();
            if (failures.Count > 0 || !markdownPassed)
            {
                var sb = new StringBuilder();
                if (failures.Count > 0)
                {
                    sb.AppendLine($"Snapshot assertion failed ({failures.Count} of {this.snapshots.Count}):");
                    foreach (var f in failures)
                    {
                        var reason = f.ExpectedSvg is null
                            ? "no expected baseline"
                            : "differs from expected";
                        sb.AppendLine($"  {f.Index:D2}. {f.Name} — {reason}");
                    }
                    sb.AppendLine();
                }

                if (!markdownPassed)
                {
                    sb.AppendLine("Markdown report differs from expected baseline.");
                    sb.AppendLine();
                }

                sb.AppendLine($"Review: {Path.Combine(this.snapshotsDir, "README.DIFF.md")}");
                throw new Exception(sb.ToString());
            }
        }
    }

    private sealed class FrameTimeProvider : TimeProvider
    {
        public long Timestamp;
        public override long TimestampFrequency => TimeSpan.TicksPerSecond;
        public override long GetTimestamp() => Timestamp;
    }

    private abstract record ReportEntry;
    private sealed record MarkdownReportEntry(string Markdown) : ReportEntry;
    private sealed record SnapshotReportEntry(SnapshotEntry Snapshot) : ReportEntry;

    private record SnapshotEntry(
        int Index,
        string Name,
        string ActualSvg,
        string? ExpectedSvg,
        bool Passed,
        string ExpectedImageFileName,
        string DiffImageFileName);
}
