using System.Runtime.InteropServices;
using Xui.DevKit.UI.Design;

namespace Xui.Apps.TestApp.Examples.DesignSystem;

/// <summary>
/// Service interface for editing design system knobs in the demo.
/// Resolved via GetService from child views.
/// </summary>
internal interface IDesignSystemEditor
{
    NFloat PrimaryHue { get; }
    ColorHarmony Harmony { get; }
    ShapePreset ShapePreset { get; }
    SizingPreset SizingPreset { get; }
    NeutralStyle NeutralStyle { get; }
    MotionPreset MotionPreset { get; }

    void SetHue(NFloat hue);
    void SetHarmony(ColorHarmony harmony);
    void SetShapePreset(ShapePreset preset);
    void SetSizingPreset(SizingPreset preset);
    void SetNeutralStyle(NeutralStyle style);
    void SetMotionPreset(MotionPreset preset);
}
