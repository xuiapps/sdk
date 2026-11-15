namespace Xui.Core.UI.Input
{
    /// <summary>
    /// Singleton gesture markers used by simple widgets when capturing a pointer.
    /// Reusing these avoids per-event heap allocation. Widgets that need to carry
    /// extra state can implement <see cref="IPointerGesture"/> on their own type
    /// and pass an instance to <c>CapturePointer</c> instead.
    /// </summary>
    public static class PointerGestures
    {
        /// <summary>Generic tap (e.g. <c>Button</c>, <c>Checkbox</c>) — may be stolen by an ancestor drag.</summary>
        public static ITap Tap { get; } = new TapGesture();

        /// <summary>Generic drag (e.g. color wheel, slider thumb) — ancestors must not steal.</summary>
        public static IDrag Drag { get; } = new DragGesture();

        /// <summary>
        /// A horizontal-axis drag that is tentatively willing to yield to a vertical-axis
        /// ancestor (see <see cref="IDragHorizontalTentative"/>). Replace with
        /// <see cref="Drag"/> once horizontal motion has firmly committed.
        /// </summary>
        public static IDragHorizontalTentative DragHorizontalTentative { get; } = new DragHorizontalTentativeGesture();

        /// <summary>A vertical drag that can yield to a horizontal ancestor before committing.</summary>
        public static IDragVerticalTentative DragVerticalTentative { get; } = new DragVerticalTentativeGesture();

        private sealed class DragVerticalTentativeGesture : IDragVerticalTentative { }
        private sealed class TapGesture : ITap { }
        private sealed class DragGesture : IDrag { }
        private sealed class DragHorizontalTentativeGesture : IDragHorizontalTentative { }
    }
}
