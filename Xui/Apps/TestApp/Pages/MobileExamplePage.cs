using Xui.Apps.TestApp.Examples;
using Xui.Core.Canvas;
using Xui.Core.Canvas.Composition.Graphics;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.DevKit.UI.Mobile.Navigation;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp.Pages;

/// <summary>Mobile presentation shell for an individual TestApp example.</summary>
public sealed class MobileExamplePage<T> : ViewCollection, IPage, INavigationAware
    where T : Example, new()
{
    private static readonly NFloat HeaderHeight = 48;
    private readonly T content = new();
    private readonly MobileExampleHeader header;
    private readonly MobileExampleFooter? footer;
    private readonly MobileExampleSectionMenu? sectionMenu;

    public MobileExamplePage()
    {
        if (content is IMobileExamplePresentation mobilePresentation)
            mobilePresentation.UseMobilePresentation();
        header = new MobileExampleHeader(content.Title);
        Add(content);
        Add(header);
        if (content is IExampleSectionNavigator { ShowsMobileSectionControls: true } sections)
        {
            sectionMenu = new MobileExampleSectionMenu(sections);
            footer = new MobileExampleFooter(sections, () => sectionMenu.IsOpen = !sectionMenu.IsOpen);
            Add(footer);
            Add(sectionMenu);
        }
    }

    /// <inheritdoc/>
    public PagePresentation Presentation => new();

    public void OnNavigationStateChanged(in NavigationStateChange change)
    {
        if (content is INavigationAware aware)
            aware.OnNavigationStateChanged(in change);
    }

    protected override Size MeasureCore(Size available, IMeasureContext context)
    {
        header.Measure((available.Width, HeaderHeight), context);
        var footerHeight = footer is null ? 0 : footer.Measure((available.Width - 32, 48), context).Height + 16;
        sectionMenu?.Measure(available, context);
        return content.Measure(available - (0, HeaderHeight + footerHeight), context)
            + new Size(0, HeaderHeight + footerHeight);
    }

    protected override void ArrangeCore(Rect rect, IMeasureContext context)
    {
        header.Arrange((rect.X, rect.Y, rect.Width, HeaderHeight), context);
        var footerHeight = footer is null ? 0 : 64;
        content.Arrange((rect.X, rect.Y + HeaderHeight, rect.Width, rect.Height - HeaderHeight - footerHeight), context);
        footer?.Arrange((rect.X + 16, rect.Bottom - 56, rect.Width - 32, 48), context);
        sectionMenu?.Arrange(rect, context);
    }

    private void RenderSurface(IContext context, View view)
    {
        using var group = context.Group(new GroupOptions
        {
            Key = view, ViewBacking = true, Bounds = new Rect(view.Frame.X - 24, view.Frame.Y - 24, view.Frame.Width + 48, view.Frame.Height + 48),
            HasUpdates = view.RequiresRender(context)
        });
        if (group.NeedsRedraw) RenderChild(context, view,
            this.TryFindParent<RootView>(out var root) ? root.Window.DisplayArea : Frame);
    }

    protected override void RenderCore(IContext context)
    {
        RenderSurface(context, content);
        if (sectionMenu is not null)
            RenderSurface(context, sectionMenu);
        if (footer is not null)
            RenderSurface(context, footer);
        RenderChild(context, header);
    }
}
