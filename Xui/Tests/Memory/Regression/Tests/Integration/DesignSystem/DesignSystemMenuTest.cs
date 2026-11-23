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
    [Fact]
    public void ShadeTriangleSelectsWhiteBlackAndHueAndKeepsDragAllocationsAtZero()
    {
        using var scene = new Scene();
        var wheel = scene.Find("DesignSystem.Hue");
        var design = (IDesignSystem)wheel.GetService(typeof(IDesignSystem))!;
        var palette = design.Colors;
        var center = wheel.Parent!.LocalToGlobal(wheel.Frame.Center);
        var radius = (nfloat)115 * .83f - 12;
        var white = center + new Vector(-radius / 2, -nfloat.Sqrt(3) * radius / 2);
        var black = center + new Vector(-radius / 2, nfloat.Sqrt(3) * radius / 2);
        var hue = center + new Vector(radius, 0);
        scene.App.PointerDown(center);
        // Continue outside each corner: projection should land exactly on that vertex.
        scene.App.PointerMove(white + new Vector(-5, -5));
        Assert.InRange((double)new Oklch(palette.Primary.Background).L, .999, 1.001);
        scene.App.PointerMove(black + new Vector(-5, 5));
        Assert.InRange((double)new Oklch(palette.Primary.Background).L, 0, .001);
        scene.App.PointerMove(hue + new Vector(5, 0));
        Assert.InRange((double)new Oklch(palette.Primary.Background).C, .1, .4);
        long allocated = 0;
        for (var i = 0; i < 32; i++)
        {
            var point = center + new Vector(0, (i % 16 - 8) * 2);
            var before = GC.GetAllocatedBytesForCurrentThread();
            scene.App.PointerMove(point);
            if (i >= 16) allocated += GC.GetAllocatedBytesForCurrentThread() - before;
            scene.Advance(8);
        }
        scene.App.PointerUp(center);
        Assert.Equal(0, allocated);
        Assert.Same(palette, design.Colors);
        var selected = palette.Primary.Background;
        scene.App.PointerMove(white);
        Assert.Equal(selected, palette.Primary.Background);
    }

    [Fact]
    public void HueUpdatesDoNotReallocateTheThemePerPointerMove()
    {
        using var scene = new Scene();
        var wheel = scene.Find("DesignSystem.Hue");
        var center = wheel.Parent!.LocalToGlobal(wheel.Frame.Center);
        scene.App.PointerDown(center + new Vector(105, 0));
        long allocated = 0;
        for (var i = 0; i < 32; i++)
        {
            var angle = (nfloat)i * nfloat.Pi / 16;
            var point = center + new Vector(nfloat.Cos(angle) * 105, nfloat.Sin(angle) * 105);
            var before = GC.GetAllocatedBytesForCurrentThread();
            scene.App.PointerMove(point);
            var bytes = GC.GetAllocatedBytesForCurrentThread() - before;
            if (i >= 16) allocated += bytes;
            // Reset invalidation flags outside the allocation measurement, as a real frame does.
            scene.Advance(8);
        }
        scene.App.PointerUp(center + new Vector(105, 0));
        Assert.True(allocated == 0, $"Hue update allocated {allocated / 16} B per pointer move.");
    }
}
