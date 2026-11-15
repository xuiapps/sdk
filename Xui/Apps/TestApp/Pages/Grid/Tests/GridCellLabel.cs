using Xui.Core.UI;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp.Pages.Grid.Tests;

/// <summary>Explicit lines for compact grid annotations; Label draws a single line.</summary>
internal sealed class GridCellLabel : VerticalStack
{
    public GridCellLabel(string text, NFloat fontSize)
    {
        HorizontalAlignment = HorizontalAlignment.Center;
        VerticalAlignment = VerticalAlignment.Middle;
        foreach (var line in text.Split('\n'))
            Add(new Label
            {
                Text = line,
                FontFamily = "Inter",
                FontSize = fontSize,
                HorizontalAlignment = HorizontalAlignment.Center,
            });
    }
}
