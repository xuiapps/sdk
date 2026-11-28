using Xui.Apps.TestApp.Examples;
using Xui.Apps.TestApp.Pages;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using Xui.DevKit.UI.Design;
using Xui.DevKit.UI.Mobile.Navigation;
using Xui.DevKit.UI.Widgets;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp;

/// <summary>Native desktop host for the shared TestApp demo catalog.</summary>
internal sealed class DesktopDemoShell : ViewCollection, IExampleNavigation
{
    private static readonly NFloat SidebarWidth = 240;
    private readonly ScrollView sidebar;
    private readonly VerticalStack menu = new();
    private readonly HashSet<string> expandedCategories = new(StringComparer.Ordinal);
    private readonly Stack<View> history = new();
    private View content;
    private string? selectedId;

    public DesktopDemoShell()
    {
        sidebar = new ScrollView
        {
            Id = "DesktopDemoSidebar",
            Direction = ScrollDirection.Vertical,
            Content = menu,
        };
        content = new DesktopDemoWelcome();
        Add(sidebar);
        Add(content);
        foreach (var category in ExampleCatalog.Categories)
            expandedCategories.Add(category.Name);
        RebuildMenu();
    }

    public override int Count => 2;

    public override View this[int index] => index switch
    {
        0 => sidebar,
        1 => content,
        _ => throw new IndexOutOfRangeException(),
    };

    public void PopExample()
    {
        if (history.TryPop(out var previous))
        {
            SetContent(previous);
            selectedId = previous is DesktopExamplePage page ? page.ExampleId : null;
        }
        else
        {
            SetWelcome();
        }
        RebuildMenu();
    }

    public void PushExamplePage<TPage>() where TPage : Example, new() =>
        PushExamplePage(new TPage());

    public void PushExamplePage(Example page)
    {
        history.Push(content);
        SetContent(new DesktopExamplePage(page, showBack: true));
        RebuildMenu();
    }

    private void SelectExample(ExampleCatalogEntry entry)
    {
        history.Clear();
        selectedId = entry.Id;
        SetContent(new DesktopExamplePage(entry.Create(), showBack: true, entry.Id));
        RebuildMenu();
    }

    private void SetWelcome()
    {
        selectedId = null;
        SetContent(new DesktopDemoWelcome());
    }

    private void SetContent(View next)
    {
        Remove(content);
        content = next;
        Add(content);
        InvalidateMeasure();
        InvalidateRender();
    }

    private void RebuildMenu()
    {
        menu.Clear();
        foreach (var category in ExampleCatalog.Categories)
        {
            var entries = category.Entries
                .Where(entry => entry.Mobile || category.Name == "Desktop")
                .Where(entry => entry.IsDesktopAvailable?.Invoke(this) != false)
                .ToArray();
            if (entries.Length == 0)
                continue;

            var categoryName = category.Name;
            var isExpanded = expandedCategories.Contains(categoryName);
            menu.Add(new DesktopCategoryHeader(categoryName)
            {
                Text = (isExpanded ? "v " : "> ") + categoryName,
                Clicked = () =>
                {
                    if (isExpanded) expandedCategories.Remove(categoryName);
                    else expandedCategories.Add(categoryName);
                    RebuildMenu();
                },
            });

            if (!isExpanded)
                continue;
            foreach (var entry in entries)
            {
                var row = new DesktopDemoRow(entry.Name, entry.Icon)
                {
                    Id = entry.Id,
                    Text = entry.Name,
                    IsSelected = entry.Id == selectedId,
                    Clicked = () => SelectExample(entry),
                };
                menu.Add(row);
            }
        }
    }

    protected override Size MeasureCore(Size available, IMeasureContext context)
    {
        sidebar.Measure((SidebarWidth, available.Height), context);
        content.Measure((NFloat.Max(0, available.Width - SidebarWidth), available.Height), context);
        return available;
    }

    protected override void ArrangeCore(Rect rect, IMeasureContext context)
    {
        sidebar.Arrange((rect.X, rect.Y, SidebarWidth, rect.Height), context);
        content.Arrange((rect.X + SidebarWidth, rect.Y, NFloat.Max(0, rect.Width - SidebarWidth), rect.Height), context);
    }

    protected override void RenderCore(IContext context)
    {
        context.SetFill(new Color(0xF7F7FAFF));
        context.FillRect(sidebar.Frame);
        RenderChild(context, sidebar);

        context.SetFill(Colors.White);
        context.FillRect(content.Frame);
        RenderChild(context, content);

        context.SetStroke(new Color(0xDDE1E8FF));
        context.LineWidth = 1;
        context.BeginPath();
        context.MoveTo((sidebar.Frame.Right, sidebar.Frame.Top));
        context.LineTo((sidebar.Frame.Right, sidebar.Frame.Bottom));
        context.Stroke();
    }
}

internal sealed class DesktopCategoryHeader : Button
{
    public DesktopCategoryHeader(string name)
    {
        Id = $"DesktopCategory.{name}";
        Variant = ButtonVariant.Text;
        Role = ColorRole.Neutral;
        LabelTextAlign = TextAlign.Left;
        HorizontalAlignment = HorizontalAlignment.Stretch;
        MinimumHeight = 34;
    }
}

internal sealed class DesktopDemoRow : Button
{
    private readonly MobileDemoIcon icon;
    private bool isSelected;

    public bool IsSelected
    {
        get => isSelected;
        set
        {
            if (isSelected == value)
                return;
            isSelected = value;
            InvalidateRender();
        }
    }

    public DesktopDemoRow(string name, MobileDemoIcon icon)
    {
        this.icon = icon;
        Variant = ButtonVariant.Text;
        Role = ColorRole.Neutral;
        LabelTextAlign = TextAlign.Left;
        HorizontalAlignment = HorizontalAlignment.Stretch;
        MinimumHeight = 38;
        Margin = (8, 0, 4, 8);
    }

    protected override Rect LabelBounds => new(Frame.X + 42, Frame.Y,
        NFloat.Max(0, Frame.Width - 50), Frame.Height);

    protected override void RenderCore(IContext context)
    {
        if (IsSelected)
        {
            context.SetFill(new Color(0xEEEAFBFF));
            context.BeginPath();
            context.RoundRect(Frame, 8);
            context.Fill();
        }
        base.RenderCore(context);
        var inset = Frame.X + 10;
        MobileDemoIcons.Draw(context, (inset, Frame.Center.Y - 16), icon);
    }
}

internal sealed class DesktopDemoWelcome : View
{
    protected override Size MeasureCore(Size available, IMeasureContext context) => available;

    protected override void RenderCore(IContext context)
    {
        var rect = Frame;
        context.SetFill(new Color(0x1F1F24FF));
        context.SetFont(new Font(24, "Inter", FontWeight.SemiBold));
        context.TextAlign = TextAlign.Left;
        context.TextBaseline = TextBaseline.Middle;
        context.FillText("Xui TestApp", (rect.X + 32, rect.Y + 70));

        context.SetFill(new Color(0x686B74FF));
        context.SetFont(new Font(14, "Inter"));
        context.FillText("Choose a demo from the sidebar.", (rect.X + 32, rect.Y + 102));
    }
}

internal sealed class DesktopExamplePage : ViewCollection, INavigationAware
{
    private static readonly NFloat HeaderHeight = 48;
    private readonly Example content;
    private readonly View header;

    public string ExampleId { get; }

    public DesktopExamplePage(Example content, bool showBack, string? exampleId = null)
    {
        this.content = content;
        ExampleId = exampleId ?? content.Id ?? "Example";
        header = new HorizontalStack
        {
            MinimumHeight = HeaderHeight,
            MaximumHeight = HeaderHeight,
            Content =
            [
                showBack ? new SdkExampleBackButton { Margin = (12, 0, 8, 0) } : new View { MinimumWidth = 12 },
                new Label
                {
                    Text = content.Title,
                    FontFamily = "Inter",
                    FontSize = 16,
                    FontWeight = FontWeight.SemiBold,
                    VerticalAlignment = VerticalAlignment.Middle,
                }
            ],
        };
        Add(header);
        Add(content);
    }

    public override int Count => 2;
    public override View this[int index] => index switch
    {
        0 => header,
        1 => content,
        _ => throw new IndexOutOfRangeException(),
    };

    public void OnNavigationStateChanged(in NavigationStateChange change)
    {
        if (content is INavigationAware aware)
            aware.OnNavigationStateChanged(in change);
    }

    protected override void OnActivate()
    {
        base.OnActivate();
        if (content is INavigationAware aware)
            aware.OnNavigationStateChanged(new NavigationStateChange(
                NavigationPhase.Appeared, NavigationDirection.Forward, NavigationRole.Incoming, 1, false));
    }

    protected override void OnDeactivate()
    {
        if (content is INavigationAware aware)
        {
            aware.OnNavigationStateChanged(new NavigationStateChange(
                NavigationPhase.WillDisappear, NavigationDirection.Backward, NavigationRole.Outgoing, 1, false));
            aware.OnNavigationStateChanged(new NavigationStateChange(
                NavigationPhase.Disappeared, NavigationDirection.Backward, NavigationRole.Outgoing, 1, false));
        }
        base.OnDeactivate();
    }

    protected override Size MeasureCore(Size available, IMeasureContext context)
    {
        header.Measure((available.Width, HeaderHeight), context);
        content.Measure((available.Width, NFloat.Max(0, available.Height - HeaderHeight)), context);
        return available;
    }

    protected override void ArrangeCore(Rect rect, IMeasureContext context)
    {
        header.Arrange((rect.X, rect.Y, rect.Width, HeaderHeight), context);
        content.Arrange((rect.X, rect.Y + HeaderHeight, rect.Width, NFloat.Max(0, rect.Height - HeaderHeight)), context);
    }

    protected override void RenderCore(IContext context)
    {
        context.SetFill(new Color(0xFFFFFFFF));
        context.FillRect(header.Frame);
        context.SetStroke(new Color(0xE8EAEEFF));
        context.BeginPath();
        context.MoveTo((header.Frame.Left, header.Frame.Bottom));
        context.LineTo((header.Frame.Right, header.Frame.Bottom));
        context.Stroke();
        RenderChild(context, header);
        RenderChild(context, content);
    }
}
