using Xui.Core.Canvas;
using Xunit;
using CanvasFont = Xui.Core.Canvas.Font;

namespace Xui.Core.Tests.Canvas;

public partial class TextMetricsCacheTests
{

    [Fact]
    public void ReusesStringIdentityRatherThanEqualCharacters()
    {
        var cache = new TextMetricsCache();
        string text = new(['O', 'K']);
        cache.Set(text, Font, TextAlign.Left, TextBaseline.Alphabetic, Metrics(24));
        Assert.True(cache.TryGet(text, Font, TextAlign.Left, TextBaseline.Alphabetic, out var measured));
        Assert.Equal(24, measured.Line.Width);
        Assert.False(cache.TryGet(new string(['O', 'K']), Font, TextAlign.Left, TextBaseline.Alphabetic, out _));
    }

    [Fact]
    public void FontAlignmentAndBaselineRemainPartOfTheKey()
    {
        var cache = new TextMetricsCache();
        cache.Set("OK", Font, TextAlign.Left, TextBaseline.Alphabetic, Metrics(24));
        CanvasFont[] changed = [Font with {FontSize=17}, Font with {FontFamily="serif"},
            Font with {FontWeight=FontWeight.Bold}, Font with {FontStyle=FontStyle.Italic},
            Font with {FontStyle=FontStyle.CustomOblique(20)}, Font with {FontStretch=new FontStretch(90)},
            Font with {LineHeight=30}];
        foreach (var font in changed)
            Assert.False(cache.TryGet("OK", font, TextAlign.Left, TextBaseline.Alphabetic, out _));
        Assert.False(cache.TryGet("OK", Font, TextAlign.Center, TextBaseline.Alphabetic, out _));
        Assert.False(cache.TryGet("OK", Font, TextAlign.Left, TextBaseline.Top, out _));
        Assert.True(cache.TryGet("OK", Font, TextAlign.Left, TextBaseline.Alphabetic, out _));
    }

    [Fact]
    public void EvictsLeastRecentlyUsedEntryAndUpdatesWithoutEvictingAnother()
    {
        var cache = new TextMetricsCache(2);
        cache.Set("A", Font, TextAlign.Left, TextBaseline.Alphabetic, Metrics(10));
        cache.Set("B", Font, TextAlign.Left, TextBaseline.Alphabetic, Metrics(20));
        Assert.True(cache.TryGet("A", Font, TextAlign.Left, TextBaseline.Alphabetic, out _));
        cache.Set("C", Font, TextAlign.Left, TextBaseline.Alphabetic, Metrics(30));
        Assert.False(cache.TryGet("B", Font, TextAlign.Left, TextBaseline.Alphabetic, out _));
        cache.Set("A", Font, TextAlign.Left, TextBaseline.Alphabetic, Metrics(11));
        Assert.True(cache.TryGet("C", Font, TextAlign.Left, TextBaseline.Alphabetic, out _));
        Assert.True(cache.TryGet("A", Font, TextAlign.Left, TextBaseline.Alphabetic, out var updated));
        Assert.Equal(11, updated.Line.Width);
        cache.Clear();
        Assert.False(cache.TryGet("A", Font, TextAlign.Left, TextBaseline.Alphabetic, out _));
        cache.Set("B", Font, TextAlign.Left, TextBaseline.Alphabetic, Metrics(20));
        Assert.True(cache.TryGet("B", Font, TextAlign.Left, TextBaseline.Alphabetic, out _));
    }

}
