using System.Runtime.InteropServices;
using Xui.Runtime.IOS.Actual;

namespace Xui.Tests.Component.IOS;

public class IOSDrawingStateTest
{
    [Fact]
    public void NestedRestoresPreservePatternPhaseAndAlpha()
    {
        var state = new IOSDrawingState();
        Span<NFloat> callerPattern = [2, 4];
        state.SetLineDash(callerPattern);
        callerPattern[0] = 99;
        state.LineDashOffset = 3;
        state.GlobalAlpha = (NFloat)0.5;
        var outer = state.Save();
        state.SetLineDash([1, 2, 3]);
        state.LineDashOffset = 7;
        state.GlobalAlpha = (NFloat)0.25;
        var inner = state.Save();
        state.SetLineDash([]);
        state.Restore(inner);
        Assert.Equal(new NFloat[] { 1, 2, 3, 1, 2, 3 }, state.LineDash.ToArray());
        Assert.Equal((NFloat)7, state.LineDashOffset);
        Assert.Equal((NFloat)0.25, state.GlobalAlpha);
        state.Restore(outer);
        state.LineDashOffset = 5;
        Assert.Equal(new NFloat[] { 2, 4 }, state.LineDash.ToArray());
        Assert.Equal((NFloat)0.5, state.GlobalAlpha);
        // Reusing storage after Restore must still leave an outer saved pattern intact.
        state.SetLineDash([9, 10]);
        state.Restore(outer);
        Assert.Equal(new NFloat[] { 2, 4 }, state.LineDash.ToArray());
    }

    [Fact]
    public void ResetRestoresDefaultsAndImageOpacityMultipliesSavedAlpha()
    {
        var state = new IOSDrawingState();
        state.GlobalAlpha = (NFloat)0.5;
        var saved = state.Save();
        state.GlobalAlpha = (NFloat)0.75;
        Assert.Equal((NFloat)0.375, state.ImageAlpha((NFloat)0.5));
        state.Restore(saved);
        Assert.Equal((NFloat)0.25, state.ImageAlpha((NFloat)0.5));
        Assert.Equal((NFloat)0.5, state.ImageAlpha(1));
        Assert.Equal((NFloat)0, state.ImageAlpha(-1));
        Assert.Equal((NFloat)0.5, state.ImageAlpha(2));
        state.SetLineDash([2]);
        state.LineDashOffset = 5;
        state.Reset();
        Assert.True(state.LineDash.IsEmpty);
        Assert.Equal((NFloat)0, state.LineDashOffset);
        Assert.Equal((NFloat)1, state.ImageAlpha(1));
    }

}
