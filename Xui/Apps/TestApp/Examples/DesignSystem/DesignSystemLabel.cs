using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.DevKit.UI.Design;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp.Examples.DesignSystem;

/// <summary>Demo copy that follows token changes through the same event as the widgets.</summary>
internal sealed class DesignSystemLabel : Label
{
    public bool Heading { get; init; }
    public bool Emphasized { get; init; }
    public bool SectionHeading { get; init; }
    public bool AlignWithCardContent { get; init; }
    public bool Accent { get; init; }
    public bool OnPrimary { get; init; }
    private IDesignSystem? design;
    private readonly List<(int Start, int Length)> lines = [];

    protected override Size MeasureCore(Size available, IMeasureContext context)
    {
        context.SetFont(Font);
        lines.Clear();
        NFloat width = 0;
        for (var start = 0; start < Text.Length;)
        {
            var end = start;
            var accepted = start;
            while (end < Text.Length)
            {
                var space = Text.IndexOf(' ', end);
                var next = space < 0 ? Text.Length : space;
                var measured = context.MeasureText(Text.AsSpan(start, next - start)).Size.Width;
                if (measured > available.Width)
                {
                    if (accepted == start)
                    {
                        accepted = start + 1;
                        while (accepted < next && context.MeasureText(Text.AsSpan(start, accepted + 1 - start)).Size.Width <= available.Width)
                            accepted++;
                    }
                    break;
                }
                accepted = next;
                end = next + 1;
            }
            lines.Add((start, accepted - start));
            width = NFloat.Max(width, context.MeasureText(Text.AsSpan(start, accepted - start)).Size.Width);
            start = accepted;
            while (start < Text.Length && Text[start] == ' ') start++;
        }
        return (width, NFloat.Max(1, lines.Count) * Font.LineHeight);
    }

    protected override void RenderCore(IContext context)
    {
        context.SetFont(Font);
        context.SetFill(design is null ? TextColor :
            OnPrimary ? design.Colors.Primary.OnContainer : Accent ? design.Colors.Primary.Background : design.Colors.Surface.Foreground);
        context.TextBaseline = TextBaseline.Top;
        context.TextAlign = TextAlign.Left;
        for (var i = 0; i < lines.Count; i++)
            context.FillText(Text.AsSpan(lines[i].Start, lines[i].Length), (Frame.Left, Frame.Top + i * Font.LineHeight));
    }

    protected override void OnActivate()
    {
        base.OnActivate();
        design = GetService(typeof(IDesignSystem)) as IDesignSystem;
        if (design is not null) design.Changed += Apply;
        Apply(DesignSystemChange.All);
    }

    protected override void OnDeactivate()
    {
        if (design is not null) design.Changed -= Apply;
        design = null;
        base.OnDeactivate();
    }

    private void Apply(DesignSystemChange changes)
    {
        if (design is null) return;
        if ((changes & DesignSystemChange.Typography) != 0)
        {
            var style = SectionHeading ? design.Typography.Title.M : Heading ? design.Typography.Title.L : design.Typography.Body.S;
            // Match the home screen's 17 pt section labels, retaining accessibility scaling.
            var scale = SectionHeading ? (NFloat)17 / 16 : 1;
            Font = new Font(style.FontSize * scale, style.FontFamily, Heading || SectionHeading || Emphasized ? FontWeight.SemiBold : style.FontWeight,
                lineHeight: style.LineHeight * scale);
        }
        if ((changes & DesignSystemChange.Colors) != 0) InvalidateRender();
        if ((changes & DesignSystemChange.Spacing) != 0 && (SectionHeading || AlignWithCardContent))
        {
            // Align with the card's content inset, including live density changes.
            var margin = Margin;
            if (SectionHeading) { margin.Top = 8; margin.Bottom = 10; }
            margin.Left = margin.Right = design.Spacing.Passive.L;
            Margin = margin;
        }
    }
}
