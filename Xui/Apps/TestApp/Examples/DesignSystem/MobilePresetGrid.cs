using Xui.Core.Canvas;
using Xui.Core.Math2D;

namespace Xui.Apps.TestApp.Examples.DesignSystem;

internal enum MobilePresetVisual { SurfaceStyle, Shape, Sizing, Motion, Harmony }

/// <summary>Vector preset options sharing an animated segmented selection.</summary>
internal sealed class MobilePresetGrid(
    MobilePresetVisual visualKind,
    (string Name, Func<IDesignSystemEditor, bool> IsSelected, Action<IDesignSystemEditor> Select)[] items) : DesignSystemSegments
{
    private IDesignSystemEditor? Editor => GetService(typeof(IDesignSystemEditor)) as IDesignSystemEditor;
    protected override int OptionCount => items.Length;
    protected override int SelectedIndex
    {
        get
        {
            var editor = Editor;
            for (var i = 0; i < items.Length; i++)
                if (editor is not null && items[i].IsSelected(editor)) return i;
            return 0;
        }
    }
    protected override string Name(int index) => items[index].Name;
    protected override void Select(int index)
    {
        if (Editor is { } editor) items[index].Select(editor);
    }
    protected override void DrawIcon(IContext context, Point center, int index, Color color) =>
        DesignSystemIcons.Draw(context, center, visualKind, index, color);
}
