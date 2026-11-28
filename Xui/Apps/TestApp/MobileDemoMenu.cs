using Xui.Apps.TestApp.Examples;
using Xui.Apps.TestApp.Examples.DesignSystem;
using Xui.Apps.TestApp.Pages.Canvas;
using Xui.Apps.TestApp.Pages.Gestures;
using Xui.Apps.TestApp.Pages.Grid;
using Xui.Apps.TestApp.Pages.Layout;
using Xui.Apps.TestApp.Pages.ThreeD;
using Xui.Core.Canvas;
using Xui.Core.UI;

namespace Xui.Apps.TestApp;

/// <summary>Organizes the mobile TestApp demos into touch-friendly categories.</summary>
internal sealed class MobileDemoMenu : MobileSurfaceStack
{
    public MobileDemoMenu()
    {
        foreach (var category in ExampleCatalog.Categories)
        {
            if (!category.Entries.Any(entry => entry.Mobile))
                continue;
            var panel = AddPanel(category.Name);
            foreach (var entry in category.Entries)
            {
                if (!entry.Mobile)
                    continue;
                var button = panel.AddExample(entry);
                if (entry.Id == "OfficeFloorplan")
                {
                    button.ClickMaybe += Pages.Floorplan.FloorplanDrawing.Preload;
                    button.ClickFarFetch += Pages.Floorplan.FloorplanDrawing.Preload;
                }
                else if (entry.Id == "FloorplanGpu")
                {
                    button.ClickMaybe += Pages.FloorplanGpu.FloorplanGpuModel.Preload;
                    button.ClickFarFetch += Pages.FloorplanGpu.FloorplanGpuModel.Preload;
                }
            }
        }
    }

    private MobileDemoCategoryPanel AddPanel(string heading)
    {
        var panel = new MobileDemoCategoryPanel(heading)
        {
            Margin = (0, 0, 28, 0),
        };
        this.Add(panel);
        return panel;
    }
}
