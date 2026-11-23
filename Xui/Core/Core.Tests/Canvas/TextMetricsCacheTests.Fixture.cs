using Xui.Core.Canvas;
using Xunit;
using CanvasFont = Xui.Core.Canvas.Font;

namespace Xui.Core.Tests.Canvas;

public partial class TextMetricsCacheTests
{
    private static readonly CanvasFont Font = new(16, "Inter");

    private static TextMetrics Metrics(int width) => new(new(width, 1, width - 1, 12, 3), default);
}
