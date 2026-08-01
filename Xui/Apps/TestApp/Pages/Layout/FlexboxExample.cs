using Xui.Apps.TestApp.Examples;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Layout;
using NFloat = System.Runtime.InteropServices.NFloat;
using static Xui.Apps.TestApp.Pages.Layout.LayoutPreviewStyle;

namespace Xui.Apps.TestApp.Pages.Layout;

public sealed class FlexboxExample : Example, IExampleSectionNavigator
{
    private static readonly (string Name, Func<View> Create)[] Sections =
    [
        ("Live Wrapping", static () => new WrappingPreview()),
        ("Flexible Space", static () => new GrowingPreview()),
        ("Justify Content", static () => new JustifyPreview()),
    ];
    private readonly ExampleCardCarousel carousel;
    public FlexboxExample()
    {
        Title = "Flexbox Layout";
        Content = carousel = new ExampleCardCarousel(Sections, 0) { Id = "FlexboxCarousel", MaximumCardHeight = 632 };
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

    private sealed class WrappingPreview : ViewCollection
    {
        private readonly VerticalStack intro;
        private readonly Border panel;
        private readonly Label note;
        private TimeSpan? start;
        private NFloat progress;

        public WrappingPreview()
        {
            Add(intro = new VerticalStack { Content =
            [Heading("SPACE THAT ADAPTS"), Text("Room to flow.", 29, FontWeight.Bold),
                Text("Watch the chips find their place.", 13, color: Muted)] });
            var flex = new FlexBox
            {
                Id = "WrappingChips", FlexWrap = FlexBox.Wrap.Wrap,
                FlexAlignItems = FlexBox.AlignItems.FlexStart,
                FlexAlignContent = FlexBox.AlignContent.FlexStart,
                RowGap = 8, ColumnGap = 8,
            };
            var names = new[] { "Design", "Build", "Canvas", "Motion", "Type", "Color", "Layout", "Create" };
            for (var i = 0; i < names.Length; i++)
            {
                var chip = Chip(names[i], i % 3 == 0 ? PaleBlue : i % 3 == 1 ? PaleTeal : PaleRose);
                chip[FlexBox.Shrink] = 0;
                flex.Add(chip);
            }
            Add(panel = Surface(flex, Colors.White, 12));
            panel.Id = "WrappingPanel";
            Add(note = Text("One row becomes many. Automatically.", 12, color: Muted));
        }
        protected override void OnActivate()
        {
            base.OnActivate(); start = null; progress = 0; RequestAnimationFrame();
        }
        protected override void AnimateCore(TimeSpan previous, TimeSpan current)
        {
            start ??= current;
            progress = (1 - NFloat.Cos((NFloat)(current - start.Value).TotalSeconds * NFloat.Pi / 3)) / 2;
            InvalidateArrange(); InvalidateRender(); RequestAnimationFrame();
        }
        protected override Size MeasureCore(Size available, IMeasureContext context)
        {
            intro.Measure((available.Width - 32, 120), context);
            panel.Measure((available.Width - 32, 280), context);
            note.Measure((available.Width - 32, 30), context);
            return available;
        }
        protected override void ArrangeCore(Rect rect, IMeasureContext context)
        {
            intro.Arrange((rect.X + 16, rect.Y + 16, rect.Width - 32, 110), context);
            var width = (rect.Width - 32) * (1 - 0.34f * progress);
            panel.Arrange((rect.Center.X - width / 2, rect.Y + 150, width, 290), context);
            note.Arrange((rect.X + 16, rect.Y + 466, rect.Width - 32, 30), context);
        }
    }

    private sealed class GrowingPreview : VerticalStack
    {
        public GrowingPreview()
        {
            Margin = 16;
            Add(Heading("SHARE THE SPACE"));
            Add(Text("A flexible rhythm.", 27, FontWeight.Bold));
            var note = Text("Same row. Different proportions.", 13, color: Muted);
            note.Margin = (10, 0, 28, 0); Add(note);
            Add(Row("EQUAL / 1 : 1 : 1", 1, 1, 1));
            Add(Row("EMPHASIS / 1 : 2 : 1", 1, 2, 1));
            Add(Row("HERO / 3 : 1 : 1", 3, 1, 1));
            var footer = Text("Available space becomes possibility.", 12, color: Muted);
            footer.Margin = (16, 0, 0, 0); Add(footer);
        }
        private static View Row(string name, int a, int b, int c)
        {
            var stack = new VerticalStack { Margin = (0, 0, 22, 0) };
            var label = Text(name, 10, FontWeight.SemiBold, Muted); label.Margin = (0, 0, 10, 0); stack.Add(label);
            var flex = new FlexBox { ColumnGap = 8, MinimumHeight = 66 };
            var weights = new[] { a, b, c };
            for (var i = 0; i < 3; i++)
            {
                var chip = Chip(weights[i].ToString(), i == 0 ? PaleBlue : i == 1 ? PaleTeal : PaleRose);
                chip[FlexBox.Grow] = weights[i]; chip[FlexBox.Basis] = 0;
                flex.Add(chip);
            }
            stack.Add(flex); return stack;
        }
    }

    private sealed class JustifyPreview : VerticalStack
    {
        public JustifyPreview()
        {
            Margin = 16;
            Add(Heading("A SENSE OF BALANCE"));
            Add(Text("Place with purpose.", 25, FontWeight.Bold));
            var note = Text("Alignment changes the whole rhythm.", 12, color: Muted);
            note.Margin = (10, 0, 24, 0); Add(note);
            Add(Row("START", FlexBox.JustifyContent.FlexStart));
            Add(Row("CENTER", FlexBox.JustifyContent.Center));
            Add(Row("SPACE BETWEEN", FlexBox.JustifyContent.SpaceBetween));
            Add(Row("SPACE EVENLY", FlexBox.JustifyContent.SpaceEvenly));
        }
        private static View Row(string name, FlexBox.JustifyContent justify)
        {
            var stack = new VerticalStack { Margin = (0, 0, 20, 0) };
            var label = Text(name, 10, FontWeight.SemiBold, Muted); label.Margin = (0, 0, 8, 0); stack.Add(label);
            var flex = new FlexBox { FlexJustifyContent = justify, ColumnGap = 8, MinimumHeight = 46 };
            for (var i = 0; i < 3; i++)
            {
                var chip = Chip(((char)('A' + i)).ToString(), i == 0 ? PaleBlue : i == 1 ? PaleTeal : PaleRose);
                chip[FlexBox.Shrink] = 0; flex.Add(chip);
            }
            stack.Add(flex); return stack;
        }
    }
}
