using Xui.Core.Actual;

namespace Xui.Core.Tests;

public class VibrationPatternTests
{
    [Fact]
    public void SamplesInterpolateBothChannelsAndClampOutsidePattern()
    {
        VibrationPoint[] points = [new(TimeSpan.Zero, 0, 1), new(TimeSpan.FromSeconds(2), 1, 0)];
        VibrationPattern.Validate(points);
        var sample = VibrationPattern.Sample(points, TimeSpan.FromMilliseconds(500));
        Assert.Equal((nfloat)0.25, sample.Strength);
        Assert.Equal((nfloat)0.75, sample.Frequency);
        Assert.Equal(points[0], VibrationPattern.Sample(points, TimeSpan.FromSeconds(-1)));
        Assert.Equal(points[1], VibrationPattern.Sample(points, TimeSpan.FromSeconds(3)));
    }

    [Theory]
    [InlineData(-0.1, 0.5)]
    [InlineData(1.1, 0.5)]
    [InlineData(0.5, -0.1)]
    [InlineData(0.5, 1.1)]
    [InlineData(double.NaN, 0.5)]
    [InlineData(0.5, double.PositiveInfinity)]
    public void RejectsInvalidNormalizedValues(double strength, double frequency) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => VibrationPattern.Validate((nfloat)strength, (nfloat)frequency));

    [Fact]
    public void RejectsPatternsWithoutAnUnambiguousBoundedTimeline()
    {
        Assert.Throws<ArgumentException>(() => VibrationPattern.Validate([]));
        Assert.Throws<ArgumentException>(() => VibrationPattern.Validate([new(TimeSpan.Zero, 1, 1)]));
        Assert.Throws<ArgumentException>(() => VibrationPattern.Validate([new(TimeSpan.FromSeconds(1), 1, 1), new(TimeSpan.FromSeconds(2), 0, 0)]));
        Assert.Throws<ArgumentException>(() => VibrationPattern.Validate([new(TimeSpan.Zero, 1, 1), new(TimeSpan.Zero, 0, 0)]));
        Assert.Throws<ArgumentException>(() => VibrationPattern.Validate([new(TimeSpan.Zero, 1, 1), new(TimeSpan.FromSeconds(-1), 0, 0)]));
        Assert.Throws<ArgumentException>(() => VibrationPattern.Validate([new(TimeSpan.Zero, 1, 1), new(TimeSpan.FromSeconds(31), 0, 0)]));
        VibrationPattern.Validate([new(TimeSpan.Zero, 1, 1), new(VibrationPattern.MaximumDuration, 0, 0)]);
    }
}
