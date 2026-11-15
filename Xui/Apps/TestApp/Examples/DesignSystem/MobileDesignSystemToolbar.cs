using Xui.Core.Canvas;
using Xui.Core.Math2D;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp.Examples.DesignSystem;

/// <summary>Palette and live-preview tabs with a sliding selection indicator.</summary>
internal sealed class MobileDesignSystemToolbar(Action<int> selectPage) : DesignSystemSegments
{
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
