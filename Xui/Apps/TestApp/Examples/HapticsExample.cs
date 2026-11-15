using Xui.Core.Actual;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using Xui.DevKit.UI.Widgets;
using static Xui.Apps.TestApp.Pages.Layout.LayoutPreviewStyle;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp.Examples;

/// <summary>A tactile sampler of the native haptic presets.</summary>
public sealed class HapticsExample : Example, IMobileExamplePresentation
{
    public void UseMobilePresentation()
    {
        if (Content is ScrollView scroll) { scroll.UseCompositionGroup = true; scroll.ClipToViewport = false; }
    }
    public HapticsExample()
    {
        Title = "Haptics";
        var rows = new VerticalStack { Margin = (20, 16) };
        rows.Add(Heading("FEEL THE DIFFERENCE"));
        var introduction = Text("Tap an effect to try it.", 17, FontWeight.SemiBold);
        introduction.Margin = (0, 0, 16, 0);
        rows.Add(introduction);
        rows.Add(new EffectRow(HapticEffect.Tick, "Tick", "A tiny, crisp pulse.", "Like a ruler notch."));
        rows.Add(new EffectRow(HapticEffect.LightImpact, "Light impact", "A gentle tap.", "Small, subtle feedback."));
        rows.Add(new EffectRow(HapticEffect.MediumImpact, "Medium impact", "A noticeable bump.", "A little more weight."));
        rows.Add(new EffectRow(HapticEffect.HeavyImpact, "Heavy impact", "A strong impact.", "A solid landing."));
        rows.Add(new EffectRow(HapticEffect.SoftImpact, "Soft impact", "A cushioned touch.", "Rounded and gentle."));
        rows.Add(new EffectRow(HapticEffect.RigidImpact, "Rigid impact", "A firm, sharp tap.", "Crisp and defined."));
        rows.Add(new EffectRow(HapticEffect.Success, "Success", "A positive finish.", "Something went right."));
        rows.Add(new EffectRow(HapticEffect.Warning, "Warning", "A moment of caution.", "Pause and take notice."));
        rows.Add(new EffectRow(HapticEffect.Error, "Error", "A clear rejection.", "Something needs attention."));
        Content = new ScrollView { Id = "HapticsScroll", Direction = ScrollDirection.Vertical, Content = rows };
    }

    private sealed class EffectRow : ViewCollection
    {
        private readonly Button button;
        private readonly VerticalStack description;

        public EffectRow(HapticEffect effect, string name, string firstLine, string secondLine)
        {
            Id = $"HapticRow.{effect}";
            button = new EffectButton(effect)
            {
                Id = $"Haptic.{effect}", Text = name,
                Variant = ButtonVariant.Outline,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                MinimumHeight = 48,
                Clicked = () => (GetService(typeof(IHapticFeedback)) as IHapticFeedback)?.Play(effect),
            };
            description = new VerticalStack { Id = $"HapticDescription.{effect}" };
            description.Add(Text(firstLine, 13, FontWeight.Medium));
            var detail = Text(secondLine, 12, color: Muted);
            detail.Margin = (4, 0, 0, 0);
            description.Add(detail);
            Add(button);
            Add(description);
        }

        protected override Size MeasureCore(Size available, IMeasureContext context)
        {
            var buttonSize = button.Measure((available.Width / 3, 48), context);
            var descriptionSize = description.Measure((available.Width * 2 / 3 - 16, NFloat.PositiveInfinity), context);
            return (available.Width, NFloat.Max(48, NFloat.Max(buttonSize.Height, descriptionSize.Height)) + 20);
        }

        protected override void ArrangeCore(Rect rect, IMeasureContext context)
        {
            var width = rect.Width / 3;
            var height = rect.Height - 20;
            button.Arrange((rect.X, rect.Y, width, height), context);
            var size = description.Measure((rect.Width - width - 16, height), context);
            description.Arrange((rect.X + width + 16, rect.Y + (height - size.Height) / 2,
                rect.Width - width - 16, size.Height), context);
        }

        protected override void RenderCore(IContext context)
        {
            RenderChild(context, button);
            RenderChild(context, description);
        }
    }

    private sealed class EffectButton(HapticEffect effect) : Button
    {
        public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
        {
            if (phase == EventPhase.Tunnel && e.Type == PointerEventType.Down)
                (GetService(typeof(IHapticFeedback)) as IHapticFeedback)?.Prepare(effect);
            base.OnPointerEvent(ref e, phase);
        }
    }
}
