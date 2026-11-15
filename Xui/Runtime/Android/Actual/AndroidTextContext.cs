using System.Runtime.InteropServices;
using System.Buffers.Binary;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Runtime.Android.Composition;
using Xui.Runtime.Android.Interop;


namespace Xui.Runtime.Android.Actual;

internal sealed class AndroidTextContext(nint host, AndroidResources resources) : ITextDrawingContext, IDisposable
{
    private Font font = new();
    private TextAlign align;
    private TextBaseline baseline = TextBaseline.Alphabetic;
    private readonly WireBuffer request = new();
    private readonly TextMetricsCache measurements = new();
    public TextAlign TextAlign { set => align = value; }
    public TextBaseline TextBaseline { set => baseline = value; }
    public void SetFont(Font value) => font = value;
    public void FillText(string text, Point pos) => throw new InvalidOperationException("Measurement context cannot draw.");
    public TextMetrics MeasureText(string text)
    {
        // Consult the managed cache before font resolution, encoding, or JNI.
        if (measurements.TryGet(text, in font, align, baseline, out var metrics)) return metrics;
        metrics = MeasureText(text.AsSpan());
        measurements.Set(text, in font, align, baseline, in metrics);
        return metrics;
    }
    public TextMetrics MeasureText(ReadOnlySpan<char> text)
    {
        var id = resources.Font(font);
        request.Clear(); request.Int(id); request.Float(font.FontSize); request.Float(font.FontStretch.Value / 100);
        request.Float(font.FontStyle.IsOblique ? font.FontStyle.ObliqueAngle : 0);
        request.Int((int)align); request.Int((int)baseline); request.Text(text);
        var result = AndroidBridge.Invoke(host, 5, request, 48);
        Span<NFloat> v = stackalloc NFloat[12];
        for (var i = 0; i < v.Length; i++) v[i] = BinaryPrimitives.ReadSingleLittleEndian(result[(i * 4)..]);
        return new(new(v[0], v[1], v[2], v[3], v[4]), new(v[5], v[6], v[7], v[8], v[9], v[10], v[11]));
    }
    public void Dispose() { measurements.Clear(); request.Dispose(); }
}
