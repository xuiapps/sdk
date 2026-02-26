using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xui.Core.Abstract;
using Xui.Core.Abstract.Events;
using Xui.Core.DI;
using Xui.Core.Math2D;
using Xui.Core.UI;
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
    private readonly IHost host;
    private readonly string snapshotsDir;
    private readonly List<SnapshotEntry> snapshots = new();
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

    /// <summary>
    /// Creates a test harness that boots <typeparamref name="TApplication"/> via a host with
    /// <see cref="TestPlatform"/> registered as <see cref="Xui.Core.Actual.IRuntime"/>.
    /// <typeparamref name="TApplication"/> and <typeparamref name="TWindow"/> are registered
    /// automatically as scoped services.
    /// Snapshot artifacts are written to a <c>Snapshots/{testName}/</c> folder next to the
    /// calling test file.
    /// </summary>
    public TestSinglePageApp(
        Size windowSize,
        Action<IServiceCollection>? configure = null,
        [CallerFilePath] string callerPath = "",
        [CallerMemberName] string testName = "")
    {
        this.Size = windowSize;
        this.platform = new TestPlatform();

        this.host = new HostBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton<Xui.Core.Actual.IRuntime>(this.platform);
                services.AddScoped<TApplication>();
                services.AddScoped<TWindow>();
                configure?.Invoke(services);
            })
            .Build();

        this.host.Start();
        var application = this.host.Services.GetRequiredService<TApplication>();
        application.Run();

        this.Window = Window.OpenWindows[Window.OpenWindows.Count - 1];
        this.Window.DisplayArea = new Rect(0, 0, windowSize.Width, windowSize.Height);
        this.Window.SafeArea = this.Window.DisplayArea;

        this.snapshotsDir = Path.Combine(
            Path.GetDirectoryName(callerPath)!, "Snapshots", testName);
        Directory.CreateDirectory(this.snapshotsDir);

        // Provide a software text measure context so pointer events can hit-test text positions.
        var testWindow = this.platform.Windows[this.platform.Windows.Count - 1];
        testWindow.TextMeasureContext = new SoftwareTextMeasureContext(
            new Catalog(Xui.Core.Fonts.Inter.URIs));
    }

    // ── Input ────────────────────────────────────────────────────

    public void MouseMove(Point position)
    {
        this.mousePosition = position;
        this.hasMouseInteraction = true;
        var e = new MouseMoveEventRef { Position = position };
        this.Window.OnMouseMove(ref e);
    }

    public void MouseDown(Point position, MouseButton button = MouseButton.Left)
    {
        this.mousePosition = position;
        this.hasMouseInteraction = true;
        if (button == MouseButton.Left) this.mouseLeftPressed = true;
        var e = new MouseDownEventRef { Position = position, Button = button };
        this.Window.OnMouseDown(ref e);
    }

    public void MouseUp(Point position, MouseButton button = MouseButton.Left)
    {
        this.mousePosition = position;
        this.hasMouseInteraction = true;
        if (button == MouseButton.Left) this.mouseLeftPressed = false;
        var e = new MouseUpEventRef { Position = position, Button = button };
        this.Window.OnMouseUp(ref e);
    }

    public void MouseMove(View view) => MouseMove(view.Frame.Center);
    public void MouseDown(View view, MouseButton button = MouseButton.Left) => MouseDown(view.Frame.Center, button);
    public void MouseUp(View view, MouseButton button = MouseButton.Left) => MouseUp(view.Frame.Center, button);

    public void KeyDown(VirtualKey key, bool shift = false)
    {
        var e = new KeyEventRef { Key = key, Shift = shift };
        this.Window.OnKeyDown(ref e);
    }

    public void Char(char character)
    {
        var e = new KeyEventRef { Character = character };
        this.Window.OnChar(ref e);
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
        this.lastFramePrevious = previous;
        this.lastFrameNext = next;
        var frame = new FrameEventRef(previous, next);
        ((Xui.Core.Abstract.IWindow)this.Window).OnAnimationFrame(ref frame);
    }

    // ── Render ───────────────────────────────────────────────────

    /// <summary>
    /// Renders the window to SVG and returns the SVG string.
    /// Use this for layout-only passes. For snapshot comparison, use <see cref="Snapshot"/>.
    /// </summary>
    public string Render()
    {
        using var stream = new MemoryStream();

        var context = new SvgDrawingContext(
            this.Size, stream, Xui.Core.Fonts.Inter.URIs, keepOpen: true);
        this.platform.CurrentDrawingContext = context;

        var frame = new FrameEventRef(this.lastFramePrevious, this.lastFrameNext);
        var rect = new Rect(0, 0, this.Size.Width, this.Size.Height);
        var render = new RenderEventRef(rect, frame);
        ((Xui.Core.Abstract.IWindow)this.Window).Render(ref render);

        this.platform.CurrentDrawingContext = null;

        stream.Position = 0;
        return new StreamReader(stream).ReadToEnd();
    }

    // ── Snapshots ────────────────────────────────────────────────

    /// <summary>
    /// Renders the window, injects a virtual cursor (if mouse events have been sent),
    /// saves the result as <c>{NN}.{name}.Render.Actual.svg</c>, and compares against
    /// the corresponding <c>.Expected.svg</c>. Failures are collected and reported on
    /// <see cref="Dispose"/>, along with an interactive <c>TestRun.html</c>.
    /// </summary>
    public string Snapshot(string name)
    {
        var svg = Render();

        if (this.hasMouseInteraction)
            svg = InjectCursor(svg);

        this.snapshotCounter++;
        var prefix = $"{this.snapshotCounter:D2}.{name}.Render";
        var actualPath = Path.Combine(this.snapshotsDir, $"{prefix}.Actual.svg");
        var expectedPath = Path.Combine(this.snapshotsDir, $"{prefix}.Expected.svg");

        File.WriteAllText(actualPath, svg);

        string? expectedSvg = null;
        bool passed;
        if (File.Exists(expectedPath))
        {
            expectedSvg = File.ReadAllText(expectedPath);
            passed = NormalizeLineEndings(expectedSvg) == NormalizeLineEndings(svg);
        }
        else
        {
            File.Copy(actualPath, expectedPath);
            expectedSvg = svg;
            passed = false;
        }

        this.snapshots.Add(new SnapshotEntry(this.snapshotCounter, name, svg, expectedSvg, passed));
        return svg;
    }

    private string InjectCursor(string svg)
    {
        var x = ((double)this.mousePosition.X).ToString(CultureInfo.InvariantCulture);
        var y = ((double)this.mousePosition.Y).ToString(CultureInfo.InvariantCulture);
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

    // ── HTML Report ──────────────────────────────────────────────

    private void GenerateTestRunHtml()
    {
        if (this.snapshots.Count == 0)
            return;

        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html lang=\"en\">");
        html.AppendLine("<head><meta charset=\"utf-8\"><title>Test Run</title>");
        html.AppendLine("<style>");
        html.AppendLine(HtmlStyles);
        html.AppendLine("</style></head>");
        html.AppendLine("<body>");

        // Grid root
        html.AppendLine("<div class=\"root\">");

        // Sidebar
        html.AppendLine("  <div class=\"sidebar\">");
        html.AppendLine("    <div class=\"sidebar-title\">Snapshots</div>");
        for (int i = 0; i < this.snapshots.Count; i++)
        {
            var s = this.snapshots[i];
            var cls = s.Passed ? "pass" : "fail";
            var icon = s.Passed ? "&#10003;" : "&#10007;";
            var active = i == 0 ? " active" : "";
            html.AppendLine($"    <div class=\"step {cls}{active}\" onclick=\"selectStep({i})\">");
            html.AppendLine($"      <span class=\"badge\">{icon}</span>");
            html.AppendLine($"      <span class=\"label\">{HtmlEncode(s.Name)}</span>");
            html.AppendLine("    </div>");
        }
        html.AppendLine("  </div>");

        // Compare (wipe) view — outer div scrolls, inner grid stacks layers
        html.AppendLine("  <div class=\"compare\" id=\"compare\">");
        html.AppendLine("    <div class=\"compare-content\" id=\"compare-content\">");
        html.AppendLine("      <div class=\"layer\" id=\"expected-layer\"></div>");
        html.AppendLine("      <div class=\"layer\" id=\"actual-layer\"></div>");
        html.AppendLine("      <div class=\"separator\" id=\"separator\"></div>");
        html.AppendLine("      <div class=\"sep-handle\" id=\"sep-handle\"></div>");
        html.AppendLine("      <span class=\"sep-label\" id=\"label-left\">Actual</span>");
        html.AppendLine("      <span class=\"sep-label\" id=\"label-right\">Expected</span>");
        html.AppendLine("    </div>");
        html.AppendLine("  </div>");

        html.AppendLine("</div>");

        // Embed snapshot data as base64 in a JSON script block
        html.AppendLine("<script id=\"snapshot-data\" type=\"application/json\">[");
        for (int i = 0; i < this.snapshots.Count; i++)
        {
            var s = this.snapshots[i];
            if (i > 0) html.AppendLine(",");
            html.Append($"  {{\"index\":{s.Index},\"name\":\"{JsonEscape(s.Name)}\",\"passed\":{(s.Passed ? "true" : "false")}");
            html.Append($",\"actual\":\"{Base64Encode(s.ActualSvg)}\"");
            if (s.ExpectedSvg is not null)
                html.Append($",\"expected\":\"{Base64Encode(s.ExpectedSvg)}\"");
            html.Append('}');
        }
        html.AppendLine("\n]</script>");

        // JavaScript
        html.AppendLine("<script>");
        html.AppendLine(HtmlScript);
        html.AppendLine("</script>");
        html.AppendLine("</body></html>");

        File.WriteAllText(Path.Combine(this.snapshotsDir, "TestRun.html"), html.ToString());
    }

    private static string HtmlEncode(string text) =>
        text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");

    private static string JsonEscape(string text) =>
        text.Replace("\\", "\\\\").Replace("\"", "\\\"");

    private static string Base64Encode(string text) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(text));

    private static string NormalizeLineEndings(string text) =>
        text.ReplaceLineEndings("\n");

    private const string HtmlStyles = """
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body { font-family: system-ui, sans-serif; height: 100vh; }
        .root { display: grid; grid-template-columns: 200px 1fr; height: 100vh; }
        .sidebar { border-right: 1px solid #ddd; overflow-y: auto; padding: 8px 0; }
        .sidebar-title { padding: 8px 16px; font-size: 11px; text-transform: uppercase; letter-spacing: 1px; color: #999; }
        .step { display: flex; align-items: center; padding: 6px 16px; cursor: pointer; font-size: 13px; }
        .step:hover { background: #f0f0f0; }
        .step.active { background: #e0e0e0; }
        .badge { width: 20px; flex-shrink: 0; }
        .pass .badge { color: green; }
        .fail .badge { color: red; }
        .label { overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
        .compare { overflow: auto; }
        .compare-content { display: grid; position: relative; min-height: 100%; }
        .layer { grid-area: 1 / 1; display: flex; justify-content: center; align-items: start; padding: 24px 250px; pointer-events: none; user-select: none; }
        .layer svg { border: 1px solid #ddd; }
        .separator { position: absolute; top: 0; bottom: 0; width: 2px; background: #000; z-index: 10; pointer-events: none; }
        .sep-handle { position: absolute; top: 0; bottom: 0; width: 20px; transform: translateX(-50%); z-index: 12; cursor: ew-resize; }
        .sep-label { position: absolute; top: 8px; font-size: 11px; color: #666; z-index: 11; pointer-events: none; user-select: none; }
        #label-left { transform: translateX(-100%); padding-right: 8px; }
        #label-right { padding-left: 8px; }
    """;

    private const string HtmlScript = """
        const snapshots = JSON.parse(document.getElementById('snapshot-data').textContent);
        const compare = document.getElementById('compare');
        const compareContent = document.getElementById('compare-content');
        const actualLayer = document.getElementById('actual-layer');
        const expectedLayer = document.getElementById('expected-layer');
        const separator = document.getElementById('separator');
        const sepHandle = document.getElementById('sep-handle');
        const labelLeft = document.getElementById('label-left');
        const labelRight = document.getElementById('label-right');
        let currentStep = 0;
        let separatorRatio = 1;

        function decodeBase64(b64) {
            const bytes = Uint8Array.from(atob(b64), c => c.charCodeAt(0));
            return new TextDecoder().decode(bytes);
        }

        function selectStep(index) {
            currentStep = index;
            const w = compareContent.offsetWidth;
            const maxRatio = w > 0 ? (w - 250 + 50) / w : 1;
            separatorRatio = maxRatio;
            document.querySelectorAll('.step').forEach((el, i) => el.classList.toggle('active', i === index));
            updateView();
        }

        function updateView() {
            const step = snapshots[currentStep];
            actualLayer.innerHTML = step.actual ? decodeBase64(step.actual) : '';
            const has = !!step.expected;
            expectedLayer.innerHTML = has ? decodeBase64(step.expected) : '';
            separator.style.display = has ? '' : 'none';
            sepHandle.style.display = has ? '' : 'none';
            labelLeft.style.display = has ? '' : 'none';
            labelRight.style.display = has ? '' : 'none';
            if (has) { updateSeparator(); } else { actualLayer.style.clipPath = 'none'; expectedLayer.style.clipPath = 'none'; }
        }

        function updateSeparator() {
            const w = compareContent.offsetWidth;
            const margin = 250;
            const allowance = 50;
            const minX = margin - allowance;
            const maxX = w - margin + allowance;
            const x = Math.round(Math.max(minX, Math.min(maxX, separatorRatio * w)));
            actualLayer.style.clipPath = `inset(0 ${w - x}px 0 0)`;
            expectedLayer.style.clipPath = `inset(0 0 0 ${x}px)`;
            separator.style.left = x + 'px';
            sepHandle.style.left = x + 'px';
            labelLeft.style.left = x + 'px';
            labelRight.style.left = x + 'px';
        }

        let dragging = false;
        sepHandle.addEventListener('mousedown', (e) => { e.preventDefault(); dragging = true; });
        document.addEventListener('mousemove', (e) => { if (dragging) { e.preventDefault(); moveSeparator(e); } });
        document.addEventListener('mouseup', () => { dragging = false; });

        function moveSeparator(e) {
            const rect = compare.getBoundingClientRect();
            const w = compareContent.offsetWidth;
            const x = e.clientX - rect.left + compare.scrollLeft;
            const margin = 250;
            const allowance = 50;
            const minX = margin - allowance;
            const maxX = w - margin + allowance;
            separatorRatio = Math.max(minX / w, Math.min(maxX / w, x / w));
            updateSeparator();
        }

        document.addEventListener('keydown', (e) => {
            if (e.key === 'ArrowUp' && currentStep > 0) { selectStep(currentStep - 1); e.preventDefault(); }
            if (e.key === 'ArrowDown' && currentStep < snapshots.length - 1) { selectStep(currentStep + 1); e.preventDefault(); }
        });

        window.addEventListener('resize', () => { if (snapshots[currentStep].expected) updateSeparator(); });
        selectStep(0);
    """;

    // ── Cleanup ──────────────────────────────────────────────────

    /// <summary>
    /// Drains remaining post actions and closes all windows.
    /// </summary>
    public void Quit()
    {
        HandlePostActions();
        this.Window.Closed();
    }

    public void Dispose()
    {
        if (!disposed)
        {
            disposed = true;
            GenerateTestRunHtml();
            Quit();

            this.host.Dispose();

            var failures = this.snapshots.Where(s => !s.Passed).ToList();
            if (failures.Count > 0)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"Snapshot assertion failed ({failures.Count} of {this.snapshots.Count}):");
                foreach (var f in failures)
                {
                    var reason = f.ExpectedSvg is null
                        ? "no expected baseline"
                        : "differs from expected";
                    sb.AppendLine($"  {f.Index:D2}. {f.Name} — {reason}");
                }
                sb.AppendLine();
                sb.AppendLine($"Review: {Path.Combine(this.snapshotsDir, "TestRun.html")}");
                throw new Exception(sb.ToString());
            }
        }
    }

    private record SnapshotEntry(
        int Index, string Name, string ActualSvg, string? ExpectedSvg, bool Passed);
}
