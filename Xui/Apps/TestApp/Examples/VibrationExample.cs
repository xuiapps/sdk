using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.DevKit.UI.Mobile.Navigation;
using Xui.DevKit.UI.Widgets;
using static Xui.Apps.TestApp.Pages.Layout.LayoutPreviewStyle;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp.Examples;

public sealed class LiveVibrationExample : VibrationExample
{
    public LiveVibrationExample() : base(false) { }
}

public sealed class RecordedVibrationExample : VibrationExample
{
    public RecordedVibrationExample() : base(true) { }
}

/// <summary>Full-page vibration playground with navigation-scoped playback.</summary>
public abstract class VibrationExample : Example, INavigationAware
{
    private readonly VibrationSurface surface;

    protected VibrationExample(bool record)
    {
        Title = record ? "Record & repeat" : "Live vibration";
        Content = surface = new VibrationSurface(record) { Margin = 24 };
    }

    public void OnNavigationStateChanged(in NavigationStateChange change)
    {
        if (change.Phase is NavigationPhase.WillDisappear or NavigationPhase.Disappeared) surface.Pad.Stop();
    }

    protected override void OnDeactivate() { surface.Pad.Stop(); base.OnDeactivate(); }

    private sealed class VibrationSurface : ViewCollection
    {
        public VibrationPad Pad { get; }
        private readonly Label heading, hint, detail;
        private readonly Button? playback;

        public VibrationSurface(bool record)
        {
            Add(heading = Heading(record ? "DRAW / RECORD / REPEAT" : "TOUCH / FEEL / EXPLORE"));
            heading.Margin = 0;
            Add(hint = Text(record ? "Draw your own rhythm." : "Feel it change.", 22, FontWeight.SemiBold));
            Add(detail = Text(record ? "Release to loop. Touch to record again." : "Hold to play. Release to stop.", 12, color: Muted));
            Add(Pad = new VibrationPad(record));
            if (record)
            {
                playback = new Button
                {
                    Id = "Vibration.Record.Playback", Text = "Start",
                    HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch,
                    MinimumHeight = 48, Clicked = Pad.TogglePlayback,
                };
                Pad.PlaybackChanged += UpdatePlayback;
            }
        }

        private void UpdatePlayback()
        {
            if (playback is null) return;
            // Show the control once a complete recording exists, retaining it while re-recording.
            if (Pad.HasPattern && playback.Parent is null) Add(playback);
            else if (!Pad.HasPattern && !Pad.IsPlaying && playback.Parent is not null) Remove(playback);
            playback.Text = Pad.IsPlaying ? "Stop" : "Start";
        }

        protected override Size MeasureCore(Size available, IMeasureContext context)
        {
            heading.Measure(available, context);
            hint.Measure(available, context);
            detail.Measure(available, context);
            if (playback?.Parent is not null) playback.Measure(available, context);
            var side = PadSide(available);
            Pad.Measure((side, side), context);
            return available;
        }

        protected override void ArrangeCore(Rect rect, IMeasureContext context)
        {
            heading.Arrange((rect.X, rect.Y, rect.Width, 18), context);
            hint.Arrange((rect.X, rect.Y + 26, rect.Width, 30), context);
            detail.Arrange((rect.X, rect.Y + 61, rect.Width, 20), context);
            var side = PadSide(rect.Size);
            Pad.Arrange((rect.Center.X - side / 2, rect.Center.Y - side / 2, side, side), context);
            if (playback?.Parent is not null) playback.Arrange((rect.X, rect.Bottom - 48, rect.Width, 48), context);
        }

        // Reserve symmetric room for the heading, outside-the-pad labels and footer.
        private static NFloat PadSide(Size available) => NFloat.Max(1, NFloat.Min(available.Width, available.Height - 264));
    }
}
