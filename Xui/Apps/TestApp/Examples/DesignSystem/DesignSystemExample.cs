using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.DI;
using Xui.Core.UI.Layout;
using Xui.DevKit.UI.Design;
using Xui.DevKit.UI.Widgets;
using static Xui.Core.UI.Layout.Grid;
using static Xui.Core.UI.Layout.Grid.TrackSize;

namespace Xui.Apps.TestApp.Examples.DesignSystem;

/// <summary>
/// Root view for the Design System demo. Owns the design system state and
/// provides IDesignSystem and IDesignSystemEditor to all descendants via GetService.
/// </summary>
public class DesignSystemExample : Example, IDesignSystemEditor
{
    private NFloat primaryHue = 240;
    private ColorHarmony harmony = ColorHarmony.SplitComplementary;
    private ShapePreset shapePreset = ShapePreset.Soft;
    private NeutralStyle neutralStyle = NeutralStyle.Monochrome;
    private SizingPreset sizingPreset = SizingPreset.Mobile;
    private MotionPreset motionPreset = MotionPreset.Normal;
    private XuiDesignSystem? designSystem;

    public DesignSystemExample()
    {
        Title = "Design System";

        Content = new Grid
        {
            TemplateColumns = [Px(460), Fr(1)],
            TemplateRows = [Fr(1)],
            Content =
            [
                // Left panel: color card + shape card stacked vertically
                new ScrollView {
                    [ColumnStart] = 1, [RowStart] = 1,
                    Content = new Grid
                    {
                        Margin = 12,
                        TemplateColumns = [Fr(1)],
                        TemplateRows = [Auto, Auto, Auto, Auto],
                        RowGap = 12,
                        Content =
                        [
                            new Card
                            {
                                [ColumnStart] = 1, [RowStart] = 1,
                                Content = new Grid
                                {
                                    TemplateColumns = [Auto, Fr(1)],
                                    TemplateRows = [Auto, Auto, Auto, Auto, Auto, Auto],
                                    Content =
                                    [
                                        new Label { Text = "Colors", FontSize = 14, FontWeight = Core.Canvas.FontWeight.SemiBold, [ColumnStart] = 1, [RowStart] = 1, [ColumnSpan] = 2 },
                                        new ColorWheelView { [ColumnStart] = 1, [RowStart] = 2 },
                                        new HarmonyListView { [ColumnStart] = 2, [RowStart] = 2 },
                                        new Label { Text = "Surface Style", FontSize = 12, FontWeight = Core.Canvas.FontWeight.SemiBold, Margin = (8, 0, 4, 0), [ColumnStart] = 1, [RowStart] = 3, [ColumnSpan] = 2 },
                                        new NeutralStylePicker { [ColumnStart] = 1, [RowStart] = 4, [ColumnSpan] = 2 },
                                        new Label { Text = "Color Groups", FontSize = 12, FontWeight = Core.Canvas.FontWeight.SemiBold, Margin = (8, 0, 4, 0), [ColumnStart] = 1, [RowStart] = 5, [ColumnSpan] = 2 },
                                        new ColorGroupSwatchesView { [ColumnStart] = 1, [RowStart] = 6, [ColumnSpan] = 2 },
                                    ]
                                }
                            },
                            new Card
                            {
                                [ColumnStart] = 1, [RowStart] = 2,
                                Content = new VerticalStack
                                {
                                    Content =
                                    [
                                        new Label { Text = "Shape", FontSize = 14, FontWeight = Core.Canvas.FontWeight.SemiBold, Margin = (0, 0, 8, 0) },
                                        new ShapePresetPicker(),
                                    ]
                                }
                            },
                            new Card
                            {
                                [ColumnStart] = 1, [RowStart] = 3,
                                Content = new VerticalStack
                                {
                                    Content =
                                    [
                                        new Label { Text = "Sizing", FontSize = 14, FontWeight = Core.Canvas.FontWeight.SemiBold, Margin = (0, 0, 8, 0) },
                                        new SizingPresetPicker(),
                                    ]
                                }
                            },
                            new Card
                            {
                                [ColumnStart] = 1, [RowStart] = 4,
                                Content = new VerticalStack
                                {
                                    Content =
                                    [
                                        new Label { Text = "Motion", FontSize = 14, FontWeight = Core.Canvas.FontWeight.SemiBold, Margin = (0, 0, 8, 0) },
                                        new MotionPresetPicker(),
                                    ]
                                }
                            },
                        ]
                    }
                },
                // Right panel: header + scrollable content + footer, with FAB overlay
                new Grid
                {
                    [ColumnStart] = 2, [RowStart] = 1,
                    TemplateColumns = [Fr(1)],
                    TemplateRows = [Px(48), Fr(1), Px(32)],
                    Content =
                    [
                        // Header
                        new DesignSystemBar("Widget Preview", 16)
                        {
                            [ColumnStart] = 1, [RowStart] = 1,
                        },
                        // Scrollable content
                        new ScrollView
                        {
                            [ColumnStart] = 1, [RowStart] = 2,
                            Content = new WidgetPreviewPanel(),
                        },
                        // FAB — overlaid bottom-right on the content row
                        new Button
                        {
                            [ColumnStart] = 1, [RowStart] = 2,
                            Text = "+",
                            Role = ColorRole.Primary,
                            HorizontalAlignment = HorizontalAlignment.Right,
                            VerticalAlignment = VerticalAlignment.Bottom,
                            Margin = (0, 24, 24, 0),
                            MinimumWidth = 48,
                            MinimumHeight = 48,
                        },
                        // Footer
                        new DesignSystemBar("Xui Design System Demo", 11)
                        {
                            [ColumnStart] = 1, [RowStart] = 3,
                        },
                    ]
                },
            ]
        };
    }

    NFloat IDesignSystemEditor.PrimaryHue => primaryHue;
    ColorHarmony IDesignSystemEditor.Harmony => harmony;
    ShapePreset IDesignSystemEditor.ShapePreset => shapePreset;
    SizingPreset IDesignSystemEditor.SizingPreset => sizingPreset;
    NeutralStyle IDesignSystemEditor.NeutralStyle => neutralStyle;
    MotionPreset IDesignSystemEditor.MotionPreset => motionPreset;

    public override object? GetService(Type serviceType)
    {
        if (serviceType == typeof(IDesignSystem) && designSystem != null)
            return designSystem;
        if (serviceType == typeof(IDesignSystemEditor))
            return this;
        return base.GetService(serviceType);
    }

    protected override void OnActivate()
    {
        base.OnActivate();
        RebuildDesignSystem();
    }

    void IDesignSystemEditor.SetHue(NFloat hue)
    {
        primaryHue = hue;
        RebuildDesignSystem();
        this.InvalidateRender();
    }

    void IDesignSystemEditor.SetHarmony(ColorHarmony h)
    {
        harmony = h;
        RebuildDesignSystem();
        this.InvalidateRender();
    }

    void IDesignSystemEditor.SetShapePreset(ShapePreset preset)
    {
        shapePreset = preset;
        RebuildDesignSystem();
        this.InvalidateRender();
    }

    void IDesignSystemEditor.SetSizingPreset(SizingPreset preset)
    {
        sizingPreset = preset;
        RebuildDesignSystem();
        this.InvalidateRender();
    }

    void IDesignSystemEditor.SetNeutralStyle(NeutralStyle style)
    {
        neutralStyle = style;
        RebuildDesignSystem();
        this.InvalidateRender();
    }

    void IDesignSystemEditor.SetMotionPreset(MotionPreset preset)
    {
        motionPreset = preset;
        RebuildDesignSystem();
        this.InvalidateRender();
    }

    private void RebuildDesignSystem()
    {
        var device = this.GetService(typeof(IDeviceInfo)) as IDeviceInfo;
        if (device == null) return;

        designSystem = new XuiDesignSystem(
            new XuiDesignSystemOptions
            {
                PrimaryHue = primaryHue,
                Harmony = harmony,
                Chroma = 0.15f,
                Shape = shapePreset,
                Sizing = sizingPreset,
                NeutralStyle = neutralStyle,
                Motion = motionPreset,
            },
            device
        );
    }
}
