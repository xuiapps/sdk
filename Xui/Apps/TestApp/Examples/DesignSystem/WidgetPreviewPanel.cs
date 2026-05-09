using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.DI;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Layout;
using Xui.DevKit.UI.Design;
using Xui.DevKit.UI.Widgets;
using static Xui.Core.UI.Layout.Grid;
using static Xui.Core.UI.Layout.Grid.TrackSize;

namespace Xui.Apps.TestApp.Examples.DesignSystem;

/// <summary>
/// Right panel: showcases Button and Card widgets consuming the design system.
/// </summary>
internal class WidgetPreviewPanel : View
{
    private readonly VerticalStack stack;

    public WidgetPreviewPanel()
    {

        stack = new VerticalStack
        {
            Margin = 24,
            Content =
            [
                // Section: Buttons
                new Label { Text = "Buttons", FontSize = 18, FontWeight = Core.Canvas.FontWeight.SemiBold, Margin = (0, 0, 8, 0) },

                new HorizontalStack
                {
                    Margin = (0, 0, 24, 0),
                    Content =
                    [
                        new Button { Text = "Primary", Role = ColorRole.Primary, Margin = (0, 8, 0, 0) },
                        new Button { Text = "Secondary", Role = ColorRole.Secondary, Margin = (0, 8, 0, 0) },
                        new Button { Text = "Tertiary", Role = ColorRole.Tertiary, Margin = (0, 8, 0, 0) },
                        new Button { Text = "Error", Role = ColorRole.Error, Margin = (0, 8, 0, 0) },
                        new Button { Text = "Neutral", Role = ColorRole.Neutral, Margin = (0, 8, 0, 0) },
                    ]
                },

                // Section: Cards
                new Label { Text = "Cards", FontSize = 18, FontWeight = Core.Canvas.FontWeight.SemiBold, Margin = (0, 0, 8, 0) },

                new Grid
                {
                    Margin = (0, 0, 8, 0),
                    TemplateColumns = [Auto, Auto, Auto, Auto, Auto],
                    TemplateRows = [Auto, Auto],
                    ColumnGap = 8,
                    RowGap = 8,
                    Content =
                    [
                        // Column 1: Card with Actions spanning 2 rows
                        new Card
                        {
                            [ColumnStart] = 1, [RowStart] = 1, [RowSpan] = 2,
                            Content = new VerticalStack
                            {
                                Content =
                                [
                                    new Label { Text = "Confirm Action", FontSize = 16, FontWeight = Core.Canvas.FontWeight.SemiBold },
                                    new Label { Text = "Are you sure you want to proceed?", FontSize = 13, TextColor = new Color(0x666666FF), Margin = (4, 0, 16, 0) },
                                    new HorizontalStack
                                    {
                                        Content =
                                        [
                                            new Button { Text = "Cancel", Role = ColorRole.Secondary, Margin = (0, 8, 0, 0) },
                                            new Button { Text = "Confirm", Role = ColorRole.Primary, Margin = (0, 8, 0, 0) },
                                        ]
                                    }
                                ]
                            }
                        },

                        // Row 1, Columns 2-5: Surface, Primary, Secondary, Tertiary
                        new Card
                        {
                            [ColumnStart] = 2, [RowStart] = 1,
                            MinimumWidth = 120,
                            Content = new VerticalStack
                            {
                                Content =
                                [
                                    new Label { Text = "Surface", FontSize = 14, FontWeight = Core.Canvas.FontWeight.Medium },
                                    new Label { Text = "Default card", FontSize = 12, TextColor = new Color(0x888888FF), Margin = (4, 0, 0, 0) },
                                ]
                            }
                        },
                        new Card
                        {
                            [ColumnStart] = 3, [RowStart] = 1,
                            MinimumWidth = 120,
                            Role = ColorRole.Primary,
                            Content = new VerticalStack
                            {
                                Content =
                                [
                                    new Label { Text = "Primary", FontSize = 14, FontWeight = Core.Canvas.FontWeight.Medium },
                                    new Label { Text = "Brand action", FontSize = 12, TextColor = new Color(0x888888FF), Margin = (4, 0, 0, 0) },
                                ]
                            }
                        },
                        new Card
                        {
                            [ColumnStart] = 4, [RowStart] = 1,
                            MinimumWidth = 120,
                            Role = ColorRole.Secondary,
                            Content = new VerticalStack
                            {
                                Content =
                                [
                                    new Label { Text = "Secondary", FontSize = 14, FontWeight = Core.Canvas.FontWeight.Medium },
                                    new Label { Text = "Supporting", FontSize = 12, TextColor = new Color(0x888888FF), Margin = (4, 0, 0, 0) },
                                ]
                            }
                        },
                        new Card
                        {
                            [ColumnStart] = 5, [RowStart] = 1,
                            MinimumWidth = 120,
                            Role = ColorRole.Tertiary,
                            Content = new VerticalStack
                            {
                                Content =
                                [
                                    new Label { Text = "Tertiary", FontSize = 14, FontWeight = Core.Canvas.FontWeight.Medium },
                                    new Label { Text = "Highlight", FontSize = 12, TextColor = new Color(0x888888FF), Margin = (4, 0, 0, 0) },
                                ]
                            }
                        },

                        // Row 2, Columns 2-5: Warning, Error, Neutral
                        new Card
                        {
                            [ColumnStart] = 2, [RowStart] = 2,
                            MinimumWidth = 120,
                            Role = ColorRole.Warning,
                            Content = new VerticalStack
                            {
                                Content =
                                [
                                    new Label { Text = "Warning", FontSize = 14, FontWeight = Core.Canvas.FontWeight.Medium },
                                    new Label { Text = "Caution", FontSize = 12, TextColor = new Color(0x888888FF), Margin = (4, 0, 0, 0) },
                                ]
                            }
                        },
                        new Card
                        {
                            [ColumnStart] = 3, [RowStart] = 2,
                            MinimumWidth = 120,
                            Role = ColorRole.Error,
                            Content = new VerticalStack
                            {
                                Content =
                                [
                                    new Label { Text = "Error", FontSize = 14, FontWeight = Core.Canvas.FontWeight.Medium },
                                    new Label { Text = "Destructive", FontSize = 12, TextColor = new Color(0x888888FF), Margin = (4, 0, 0, 0) },
                                ]
                            }
                        },
                        new Card
                        {
                            [ColumnStart] = 4, [RowStart] = 2,
                            MinimumWidth = 120,
                            Role = ColorRole.Neutral,
                            Content = new VerticalStack
                            {
                                Content =
                                [
                                    new Label { Text = "Neutral", FontSize = 14, FontWeight = Core.Canvas.FontWeight.Medium },
                                    new Label { Text = "Quiet control", FontSize = 12, TextColor = new Color(0x888888FF), Margin = (4, 0, 0, 0) },
                                ]
                            }
                        },
                    ]
                },

                // Login, Controls, Contacts side-by-side
                new Grid
                {
                    Margin = (24, 0, 8, 0),
                    TemplateColumns = [Fr(1), Fr(1), Fr(1)],
                    TemplateRows = [Auto],
                    ColumnGap = 12,
                    Content =
                    [
                        // Column 1: Login
                        new VerticalStack
                        {
                            [ColumnStart] = 1, [RowStart] = 1,
                            Content =
                            [
                                new Label { Text = "Login Form", FontSize = 18, FontWeight = Core.Canvas.FontWeight.SemiBold, Margin = (0, 0, 8, 0) },
                                new Card
                                {
                                    Content = new VerticalStack
                                    {
                                        Content =
                                        [
                                            new Label { Text = "Sign In", FontSize = 16, FontWeight = Core.Canvas.FontWeight.SemiBold, Margin = (0, 0, 12, 0) },
                                            new Label { Text = "Username", FontSize = 12, TextColor = new Color(0x666666FF), Margin = (0, 0, 4, 0) },
                                            new TextInput { Margin = (0, 0, 12, 0) },
                                            new Label { Text = "Password", FontSize = 12, TextColor = new Color(0x666666FF), Margin = (0, 0, 4, 0) },
                                            new TextInput { IsPassword = true, Margin = (0, 0, 12, 0) },
                                            new HorizontalStack
                                            {
                                                Margin = (0, 0, 16, 0),
                                                Content =
                                                [
                                                    new Checkbox { IsChecked = true, Margin = (0, 6, 0, 0), VerticalAlignment = VerticalAlignment.Middle },
                                                    new Label { Text = "Remember me", FontSize = 13, VerticalAlignment = VerticalAlignment.Middle },
                                                ]
                                            },
                                            new HorizontalStack
                                            {
                                                Content =
                                                [
                                                    new Button { Text = "Sign In", Role = ColorRole.Primary, Margin = (0, 8, 0, 0) },
                                                    new Button { Text = "Forgot Password?", Role = ColorRole.Primary, Variant = ButtonVariant.Text, Margin = (0, 8, 0, 0) },
                                                ]
                                            }
                                        ]
                                    }
                                },
                            ]
                        },

                        // Column 2: Controls
                        new VerticalStack
                        {
                            [ColumnStart] = 2, [RowStart] = 1,
                            Content =
                            [
                                new Label { Text = "Controls", FontSize = 18, FontWeight = Core.Canvas.FontWeight.SemiBold, Margin = (0, 0, 8, 0) },
                                new Card
                                {
                                    Content = new VerticalStack
                                    {
                                        Content =
                                        [
                                            new HorizontalStack
                                            {
                                                Margin = (0, 0, 12, 0),
                                                Content =
                                                [
                                                    new Toggle { IsOn = true, Margin = (0, 8, 0, 0), VerticalAlignment = VerticalAlignment.Middle },
                                                    new Label { Text = "Enable notifications", FontSize = 13, VerticalAlignment = VerticalAlignment.Middle },
                                                ]
                                            },
                                            new HorizontalStack
                                            {
                                                Margin = (0, 0, 16, 0),
                                                Content =
                                                [
                                                    new Toggle { Margin = (0, 8, 0, 0), VerticalAlignment = VerticalAlignment.Middle },
                                                    new Label { Text = "Dark mode", FontSize = 13, VerticalAlignment = VerticalAlignment.Middle },
                                                ]
                                            },
                                            new HorizontalStack
                                            {
                                                Margin = (0, 0, 8, 0),
                                                Content =
                                                [
                                                    new Checkbox { IsChecked = true, Margin = (0, 6, 0, 0), VerticalAlignment = VerticalAlignment.Middle },
                                                    new Label { Text = "Accept terms", FontSize = 13, VerticalAlignment = VerticalAlignment.Middle },
                                                ]
                                            },
                                            new HorizontalStack
                                            {
                                                Margin = (0, 0, 16, 0),
                                                Content =
                                                [
                                                    new Checkbox { Margin = (0, 6, 0, 0), VerticalAlignment = VerticalAlignment.Middle },
                                                    new Label { Text = "Subscribe to newsletter", FontSize = 13, VerticalAlignment = VerticalAlignment.Middle },
                                                ]
                                            },
                                            new Label { Text = "Plan", FontSize = 12, TextColor = new Color(0x666666FF), Margin = (0, 0, 6, 0) },
                                            new HorizontalStack
                                            {
                                                Margin = (0, 0, 6, 0),
                                                Content =
                                                [
                                                    new RadioButton { IsSelected = true, Margin = (0, 6, 0, 0), VerticalAlignment = VerticalAlignment.Middle },
                                                    new Label { Text = "Free", FontSize = 13, VerticalAlignment = VerticalAlignment.Middle },
                                                ]
                                            },
                                            new HorizontalStack
                                            {
                                                Margin = (0, 0, 6, 0),
                                                Content =
                                                [
                                                    new RadioButton { Margin = (0, 6, 0, 0), VerticalAlignment = VerticalAlignment.Middle },
                                                    new Label { Text = "Pro", FontSize = 13, VerticalAlignment = VerticalAlignment.Middle },
                                                ]
                                            },
                                            new HorizontalStack
                                            {
                                                Content =
                                                [
                                                    new RadioButton { Margin = (0, 6, 0, 0), VerticalAlignment = VerticalAlignment.Middle },
                                                    new Label { Text = "Enterprise", FontSize = 13, VerticalAlignment = VerticalAlignment.Middle },
                                                ]
                                            },
                                        ]
                                    }
                                },
                            ]
                        },

                        // Column 3: Contacts
                        new VerticalStack
                        {
                            [ColumnStart] = 3, [RowStart] = 1,
                            Content =
                            [
                                new Label { Text = "Contacts", FontSize = 18, FontWeight = Core.Canvas.FontWeight.SemiBold, Margin = (0, 0, 8, 0) },
                                new ListView
                                {
                                    MaximumHeight = 360,
                                    SelectedIndex = 2,
                                    Content =
                                    [
                                        ContactItem("Alice", "Johnson", "+1 (555) 012-3456"),
                                        ContactItem("Bob", "Smith", "+1 (555) 234-5678"),
                                        ContactItem("Carol", "Williams", "+1 (555) 345-6789"),
                                        ContactItem("David", "Brown", "+1 (555) 456-7890"),
                                        ContactItem("Eve", "Davis", "+1 (555) 567-8901"),
                                        ContactItem("Frank", "Miller", "+1 (555) 678-9012"),
                                        ContactItem("Grace", "Wilson", "+1 (555) 789-0123"),
                                    ]
                                },
                            ]
                        },
                    ]
                },

                // Section: Mini Drawing App
                new Label { Text = "Drawing App", FontSize = 18, FontWeight = Core.Canvas.FontWeight.SemiBold, Margin = (24, 0, 8, 0) },

                new Card
                {
                    Content = new VerticalStack
                    {
                        Content =
                        [
                            // Toolbar
                            new HorizontalStack
                            {
                                Margin = (0, 0, 8, 0),
                                Content =
                                [
                                    new ButtonGroup
                                    {
                                        Margin = (0, 8, 0, 0),
                                        Variant = ButtonVariant.Filled,
                                        Content =
                                        [
                                            new ButtonGroupItem { Text = "Select" },
                                            new ButtonGroupItem { Text = "Pen" },
                                            new ButtonGroupItem { Text = "Rect" },
                                            new ButtonGroupItem { Text = "Circle" },
                                        ],
                                        SelectedIndex = 1,
                                    },
                                    new ButtonGroup
                                    {
                                        Margin = (0, 8, 0, 0),
                                        Variant = ButtonVariant.Outline,
                                        Content =
                                        [
                                            new ButtonGroupItem { Text = "L" },
                                            new ButtonGroupItem { Text = "C" },
                                            new ButtonGroupItem { Text = "R" },
                                            new ButtonGroupItem { Text = "J" },
                                        ],
                                        SelectedIndex = 0,
                                    },
                                    new ButtonGroup
                                    {
                                        Margin = (0, 8, 0, 0),
                                        Variant = ButtonVariant.Text,
                                        Role = ColorRole.Secondary,
                                        Content =
                                        [
                                            new ButtonGroupItem { Text = "Sm" },
                                            new ButtonGroupItem { Text = "Md" },
                                            new ButtonGroupItem { Text = "Lg" },
                                        ],
                                        SelectedIndex = 1,
                                    },
                                    new Button { Text = "Undo", Variant = ButtonVariant.Outline, Role = ColorRole.Secondary, Margin = (0, 4, 0, 0) },
                                    new Button { Text = "Redo", Variant = ButtonVariant.Outline, Role = ColorRole.Secondary, Margin = (0, 4, 0, 0) },
                                    new Button { Text = "Clear", Variant = ButtonVariant.Text, Role = ColorRole.Error, Margin = (0, 4, 0, 0) },
                                ]
                            },
                            // Canvas area (mock)
                            new Border
                            {
                                MinimumHeight = 160,
                                BorderThickness = 1,
                                BorderColor = new Color(0xDDDDDDFF),
                                CornerRadius = 4,
                                BackgroundColor = new Color(0xFAFAFAFF),
                                Content = new Label
                                {
                                    Text = "Canvas area",
                                    FontSize = 12,
                                    TextColor = new Color(0xBBBBBBFF),
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                    VerticalAlignment = VerticalAlignment.Middle,
                                }
                            },
                        ]
                    }
                },

                // Section: Expanders
                new Label { Text = "Expanders", FontSize = 18, FontWeight = Core.Canvas.FontWeight.SemiBold, Margin = (24, 0, 8, 0) },

                new Expander
                {
                    Title = "Account Settings",
                    IsExpanded = true,
                    Margin = (0, 0, 8, 0),
                    Content = new VerticalStack
                    {
                        Content =
                        [
                            new Label { Text = "Manage your account preferences, notification settings, and privacy options.", FontSize = 13, TextColor = new Color(0x666666FF) },
                        ]
                    }
                },

                new Expander
                {
                    Title = "Billing Information",
                    Margin = (0, 0, 8, 0),
                    Content = new VerticalStack
                    {
                        Content =
                        [
                            new Label { Text = "View and update your payment methods and billing history.", FontSize = 13, TextColor = new Color(0x666666FF), Margin = (0, 0, 12, 0) },
                            new HorizontalStack
                            {
                                Content =
                                [
                                    new Button { Text = "Update Card", Role = ColorRole.Primary, Margin = (0, 8, 0, 0) },
                                    new Button { Text = "View History", Variant = ButtonVariant.Outline, Role = ColorRole.Secondary, Margin = (0, 8, 0, 0) },
                                ]
                            }
                        ]
                    }
                },

                new Expander
                {
                    Title = "Danger Zone",
                    Content = new VerticalStack
                    {
                        Content =
                        [
                            new Label { Text = "Irreversible actions. Proceed with caution.", FontSize = 13, TextColor = new Color(0x666666FF), Margin = (0, 0, 12, 0) },
                            new Button { Text = "Delete Account", Role = ColorRole.Error, Margin = (0, 8, 0, 0) },
                        ]
                    }
                },
            ]
        };

        this.AddProtectedChild(stack);
    }

    public override int Count => 1;
    public override View this[int index] => index == 0 ? stack : throw new IndexOutOfRangeException();

    protected override Size MeasureCore(Size available, IMeasureContext context)
    {
        var desired = stack.Measure(available, context);
        return new Size(available.Width, desired.Height);
    }

    protected override void ArrangeCore(Rect rect, IMeasureContext context)
    {
        stack.Arrange(rect, context);
    }

    protected override void RenderCore(IContext context)
    {
        var ds = this.GetService<IDesignSystem>();
        if (ds != null)
        {
            context.SetFill(ds.Colors.Application.Background);
            context.FillRect(this.Frame);
        }

        base.RenderCore(context);
    }

    private static ListViewItem ContactItem(string firstName, string lastName, string phone)
    {
        return new ListViewItem
        {
            ItemContent = new HorizontalStack
            {
                Content =
                [
                    new AvatarView { Initials = $"{firstName[0]}{lastName[0]}", Margin = (0, 10, 0, 0) },
                    new VerticalStack
                    {
                        Content =
                        [
                            new HorizontalStack
                            {
                                Content =
                                [
                                    new Label { Text = firstName, FontSize = 14, FontWeight = Core.Canvas.FontWeight.Medium, Margin = (0, 4, 0, 0) },
                                    new Label { Text = lastName, FontSize = 14 },
                                ]
                            },
                            new Label { Text = phone, FontSize = 12, TextColor = new Color(0x888888FF), Margin = (2, 0, 0, 0) },
                        ]
                    }
                ]
            }
        };
    }
}

/// <summary>
/// A circular avatar with initials. Consumes design system colors.
/// </summary>
internal class AvatarView : View
{
    public string Initials { get; set; } = "";

    private static readonly NFloat Size = 40;

    public override int Count => 0;
    public override View this[int index] => throw new IndexOutOfRangeException();

    protected override Size MeasureCore(Size available, IMeasureContext context)
        => new Size(Size, Size);

    protected override void RenderCore(IContext context)
    {
        var ds = this.GetService<IDesignSystem>();
        var twoPi = (NFloat)(2 * Math.PI);
        var center = new Point(Frame.X + Size / 2, Frame.Y + Size / 2);
        var radius = Size / 2;

        // Circle background
        var bgColor = ds != null ? ds.Colors.Primary.Container : new Color(0xDDDDDDFF);
        context.BeginPath();
        context.Arc(center, radius, 0, twoPi);
        context.SetFill(bgColor);
        context.Fill(FillRule.NonZero);

        // Head silhouette (simplified)
        var headColor = ds != null ? ds.Colors.Primary.OnContainer : new Color(0x666666FF);
        context.SetFill(headColor);

        // Head circle
        context.BeginPath();
        context.Arc(new Point(center.X, center.Y - 4), 7, 0, twoPi);
        context.Fill(FillRule.NonZero);

        // Shoulders arc
        context.BeginPath();
        context.Arc(new Point(center.X, center.Y + 16), 11, (NFloat)Math.PI, 0);
        context.Fill(FillRule.NonZero);
    }
}
