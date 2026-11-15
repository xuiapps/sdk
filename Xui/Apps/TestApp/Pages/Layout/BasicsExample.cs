using Xui.Apps.TestApp.Examples;
using Xui.Core.Canvas;
using Xui.Core.UI;
using static Xui.Apps.TestApp.Pages.Layout.LayoutPreviewStyle;

namespace Xui.Apps.TestApp.Pages.Layout;

public sealed class BasicsExample : Example, IExampleSectionNavigator
{
    private static readonly (string Name, Func<View> Create)[] Sections =
    [
        ("Text Layout", static () => new TextLayoutPreview()),
        ("Nested Stacks", static () => new NestedStacksPreview()),
        ("Alignment", static () => new AlignmentPreview()),
    ];
    private readonly ExampleCardCarousel carousel;
    public BasicsExample()
    {
        Title = "Basics";
        Content = carousel = new ExampleCardCarousel(Sections, 0) { Id = "BasicsCarousel", MaximumCardHeight = 632 };
    }
    public bool ShowsMobileSectionControls => false;
    public int SectionCount => Sections.Length;
    public int SelectedSectionIndex => carousel.SelectedIndex;
    public string GetSectionName(int index) => Sections[index].Name;
    public void SelectSection(int index) => carousel.Select(index);
    public event Action? SectionChanged
    {
        add => carousel.SelectionChanged += value;
        remove => carousel.SelectionChanged -= value;
    }

    private sealed class TextLayoutPreview : VerticalStack
    {
        public TextLayoutPreview()
        {
            Margin = 16;
            Add(Heading("TYPE & HIERARCHY"));
            Add(Text("Made to read.", 32, FontWeight.Bold));
            var subtitle = Text("A little space. A clear voice.", 15, color: Muted);
            subtitle.Margin = (8, 0, 26, 0);
            Add(subtitle);
            var story = new VerticalStack();
            story.Add(Text("FIELD NOTES / 01", 10, FontWeight.SemiBold, Blue));
            var title = Text("The quiet details", 23, FontWeight.SemiBold);
            title.Margin = (12, 0, 12, 0);
            story.Add(title);
            story.Add(Text("Small type sets the rhythm.", 14));
            story.Add(Text("Big type tells the story.", 14));
            var quote = Text("Less, but better.", 18, color: Blue);
            quote.FontStyle = FontStyle.Italic;
            quote.Margin = (20, 0, 0, 0);
            story.Add(quote);
            Add(Surface(story, PaleBlue));
            var caption = Text("A caption, aligned to the right.", 12, color: Muted);
            caption.HorizontalAlignment = HorizontalAlignment.Right;
            caption.Margin = (22, 0, 28, 0);
            Add(caption);
            var footer = Text("One family. Many voices.", 14, FontWeight.SemiBold);
            footer.HorizontalAlignment = HorizontalAlignment.Center;
            Add(footer);
        }
    }

    private sealed class NestedStacksPreview : VerticalStack
    {
        public NestedStacksPreview()
        {
            Margin = 16;
            Add(Heading("A DAY, COMPOSED"));
            Add(Text("Make room", 30, FontWeight.Bold));
            Add(Text("for good things.", 30, FontWeight.Bold, Blue));
            var subtitle = Text("Rows inside a vertical rhythm.", 14, color: Muted);
            subtitle.Margin = (10, 0, 26, 0);
            Add(subtitle);
            Add(Activity("09", "Morning focus", "Design something useful.", PaleBlue, Blue));
            Add(Activity("12", "Take a breath", "A walk. A fresh perspective.", PaleTeal, Teal));
            Add(Activity("16", "Share the work", "Good ideas grow together.", PaleRose, new Color(0xB94E70FF)));
            var footer = new HorizontalStack { Margin = (16, 0, 0, 0), HorizontalAlignment = HorizontalAlignment.Center };
            footer.Add(Chip("3 moments", PaleBlue));
            var second = Chip("One good day", PaleTeal); second.Margin = (0, 0, 0, 8); footer.Add(second);
            Add(footer);
        }

        private static View Activity(string hour, string title, string description, Color background, Color accent)
        {
            var row = new HorizontalStack();
            var badge = Surface(Text(hour, 23, FontWeight.Bold, accent), background, 10);
            badge.Margin = (0, 14, 0, 0);
            row.Add(badge);
            var copy = new VerticalStack { VerticalAlignment = VerticalAlignment.Middle };
            copy.Add(Text(title, 16, FontWeight.SemiBold));
            var detail = Text(description, 11, color: Muted); detail.Margin = (5, 0, 0, 0); copy.Add(detail);
            row.Add(copy);
            var surface = Surface(row, Colors.White, 10); surface.Margin = (0, 0, 10, 0);
            return surface;
        }
    }

    private sealed class AlignmentPreview : VerticalStack
    {
        public AlignmentPreview()
        {
            Margin = 16;
            Add(Heading("EVERYTHING IN ITS PLACE"));
            Add(Text("Find your balance.", 25, FontWeight.Bold));
            var subtitle = Text("Nine anchors. One shared space.", 13, color: Muted);
            subtitle.Margin = (10, 0, 22, 0); Add(subtitle);
            var field = new ViewCollection { MinimumHeight = 280, Id = "AlignmentField" };
            foreach (var vertical in new[] { VerticalAlignment.Top, VerticalAlignment.Middle, VerticalAlignment.Bottom })
                foreach (var horizontal in new[] { HorizontalAlignment.Left, HorizontalAlignment.Center, HorizontalAlignment.Right })
                {
                    var text = vertical == VerticalAlignment.Top ? "TOP" : vertical == VerticalAlignment.Middle ? "MID" : "END";
                    var chip = Chip(text, vertical == VerticalAlignment.Middle ? PaleTeal : PaleBlue);
                    chip.HorizontalAlignment = horizontal;
                    chip.VerticalAlignment = vertical;
                    field.Add(chip);
                }
            Add(Surface(field, Colors.White, 10));
            var footer = Text("Left  /  Center  /  Right", 12, color: Muted);
            footer.HorizontalAlignment = HorizontalAlignment.Center;
            footer.Margin = (20, 0, 0, 0); Add(footer);
        }
    }
}
