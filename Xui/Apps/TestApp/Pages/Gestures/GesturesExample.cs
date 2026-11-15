using Xui.Apps.TestApp.Examples;
using Xui.Apps.TestApp.Pages.Gestures.Tests;
using Xui.Apps.TestApp.Pages.Gestures.Widgets;
using Xui.Core.UI;

namespace Xui.Apps.TestApp.Pages.Gestures;

/// <summary>Hands-on input examples in retained, swipeable cards.</summary>
public sealed class GesturesExample : Example, IExampleSectionNavigator
{
    private static readonly (string Name, Func<View> Create)[] Sections =
    [
        ("Gestures", static () => new InputDemoCard("TOUCH / PLAY", "Every touch has a story.",
            "Tap, drag, or use a few fingers at once.", "Release to send each bubble floating away.", new GestureVisualizer())),
        ("Nested Scrolls", static () => new InputDemoCard("EXPLORE / SCROLL", "Find your next escape.",
            "Browse places sideways. Scroll down for more.", "Two directions, one natural conversation.", new NestedScrollDemo(), edgeToEdge: true)),
        ("Drag vs Tap", static () => new InputDemoCard("TOUCH / INTENT", "Make your mark.",
            "Tap to save. Drag a button to scroll.", "Draw on the sketchpad to keep the page still.", new DragVsTapDemo(), edgeToEdge: true)),
        ("Temperature", static () => new InputDemoCard("HOME / COMFORT", "Just right.",
            "Roll the ruler to find your comfort zone.", "Flick to coast, then settle on a half degree.", new TemperatureDial())),
        ("Floor Plan", static () => new InputDemoCard("SPACE / EXPLORE", "A place to call home.",
            "Drag to explore. Pinch with two fingers to zoom.", "Tap a room to discover it.", new FloorPlan())),
        ("Swipe Actions", static () => new InputDemoCard("INBOX / ORGANIZE", "A little less clutter.",
            "Swipe a message left to reveal an action.", "Tap Archive, or swipe right to close it.", new SwipeActions(), edgeToEdge: true)),
    ];
    private readonly ExampleCardCarousel carousel;

    public GesturesExample()
    {
        Title = "Input";
        Content = carousel = new ExampleCardCarousel(Sections, 0, cardPadding: 0, clipCardContent: true)
        {
            Id = "InputCarousel", MaximumCardHeight = 632, RespectChildGestures = true,
        };
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
}
