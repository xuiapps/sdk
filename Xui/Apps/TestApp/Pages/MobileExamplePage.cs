using Xui.Apps.TestApp.Examples;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.DevKit.UI.Mobile.Navigation;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp.Pages;

/// <summary>Mobile presentation shell for an individual TestApp example.</summary>
public sealed class MobileExamplePage<T> : ViewCollection, IPage
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

    protected override void RenderCore(IContext context)
    {
        this.RenderChild(context, content);
        if (sectionMenu is not null)
            this.RenderChild(context, sectionMenu);
        if (footer is not null)
            this.RenderChild(context, footer);
        this.RenderChild(context, header);
    }
}
