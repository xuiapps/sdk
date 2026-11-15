using Xui.Core.UI;
using Xui.Apps.TestApp;
using Xui.Apps.TestApp.Pages.Grid;
using Xui.Runtime.Test;

namespace Xui.Tests.Integration.TestApp;

public class GridLayoutTest
{
    [Theory, MemberData(nameof(IntegrationRuntimeVariants.All), MemberType = typeof(IntegrationRuntimeVariants))]
    public void Grid_Scenarios(TestRuntimeVariant runtimeVariant)
    {
        using var app = IntegrationRuntimeVariants.CreateApp<Application, MainWindow>((800, 560), runtimeVariant,
            SourcePath(), nameof(Grid_Scenarios));
        app.Run(() =>
        {
            app.Prepare();
            app.MarkdownHeading("Grid Layout scenarios", 1);
            var grid = app.Open<GridExample>("GridLayout");
            app.Snapshot("GridLayout.Home");
            for (var index = 0; index < grid.SectionCount; index++)
            {
                // Sidebar navigation belongs to desktop. Mobile sections use the carousel,
                // whose routed swipe/scroll negotiation is covered by GridCardsTest.
                if (runtimeVariant == TestRuntimeVariant.Desktop) app.Tap(grid.GetSectionName(index));
                else { grid.SelectSection(index); app.Advance(TimeSpan.FromMilliseconds(400)); }
                Assert.Equal(index, grid.SelectedSectionIndex);
                app.Snapshot(grid.GetSectionName(index).Replace(" ", "."));
            }
            app.Back();
            Assert.Null(app.Window.RootView.FindViewById("Example.GridExample"));
            app.Snapshot("HomePage");
        });
    }
    private static string SourcePath([System.Runtime.CompilerServices.CallerFilePath] string path = "") => path;
}
