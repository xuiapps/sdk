using Xui.Core.Canvas;
using Xui.DevKit.UI.Design;
using Xui.Core.Math2D;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp.Examples.DesignSystem;

/// <summary>Palette and live-preview tabs with a sliding selection indicator.</summary>
internal sealed class MobileDesignSystemToolbar(Action<int> selectPage) : DesignSystemSegments
{
    protected override bool DrawSurface => false;
    protected override Color SelectionColor => Design is { } ds
        ? new Color(ds.Colors.Primary.Background.Red, ds.Colors.Primary.Background.Green,
            ds.Colors.Primary.Background.Blue, (NFloat).14) : Colors.Transparent;
    protected override Color SelectedTextColor => Design!.Colors.Surface.Foreground;
    public NFloat SurfaceRadius => Design is { } ds
        ? NFloat.Min(Frame.Height / 2, GetService(typeof(IDesignSystemEditor)) is IDesignSystemEditor { ShapePreset: ShapePreset.Soft } ? Frame.Height / 2 : ds.Shape.Full.TopLeft)
        : Frame.Height / 2;
    public int SelectedPage { get; set; }
    public NFloat PagePosition { get; set; }
    protected override bool AnimateSelection => false;
    protected override NFloat SelectionPosition => PagePosition;
    protected override int OptionCount => 2;
    protected override int SelectedIndex => SelectedPage;
    protected override bool Compact => true;
    protected override string Name(int index) => index == 0 ? "Palette" : "Preview";
    protected override void Select(int index) => selectPage(index);
    protected override void DrawIcon(IContext context, Point center, int index, Color color) =>
        DesignSystemIcons.Draw(context, center, index == 0 ? MobilePresetVisual.Harmony : MobilePresetVisual.SurfaceStyle, 0, color);
}
