using Xui.Core.Canvas;

namespace Xui.Core.UI.Input
{
    /// <summary>
    /// Represents a reference to a pointer event routed to a view, containing identification and pointer state information.
    /// </summary>
    public ref struct PointerEventRef
    {
        internal Span<PointerGestureCandidate> PotentialGestureStorage;
        internal bool CanPushPotentialGesture;

        /// <summary>
        /// Up to eight handlers pushed earlier during this event's down tunnel,
        /// oldest first. Storage is shared by the routed event copies and exists
        /// only for this dispatch; do not retain this span across events.
        /// </summary>
        public readonly ReadOnlySpan<PointerGestureCandidate> PotentialGestures
        {
            get
            {
                var count = 0;
                while (count < PotentialGestureStorage.Length && PotentialGestureStorage[count].Handler is not null) count++;
                return PotentialGestureStorage[..count];
            }
        }

        /// <summary>
        /// Advertises a possible gesture without capturing the pointer. Call during
        /// Down/Tunnel so descendants can adapt feedback while recognition is pending.
        /// When full, drops the oldest handler and retains the newest eight.
        /// This does not change capture or the handler's recognition rules.
        /// </summary>
        public readonly void PushPotentialGesture(View handler, IPointerGesture gesture)
        {
            if (!CanPushPotentialGesture || PotentialGestureStorage.IsEmpty) return;
            var count = PotentialGestures.Length;
            if (count == PotentialGestureStorage.Length)
            {
                PotentialGestureStorage[1..].CopyTo(PotentialGestureStorage);
                count--;
            }
            PotentialGestureStorage[count] = new(handler, gesture);
        }

        /// <summary>
        /// Gets the type of pointer event (e.g., Down, Move, Up, Enter, Leave).
        /// </summary>
        public readonly PointerEventType Type;

        /// <summary>
        /// Gets the unique ID of the pointer. Used to distinguish different active pointers (e.g., different fingers or pen tips).
        /// </summary>
        public readonly int PointerId;

        /// <summary>
        /// Gets the persistent device ID, if known. Identifies the underlying hardware across multiple pointer sessions.
        /// </summary>
        public readonly long PersistentDeviceId;

        /// <summary>
        /// Gets a value indicating whether this pointer is the primary pointer in a multi-pointer interaction (e.g., first touch).
        /// </summary>
        public readonly bool IsPrimary;

        /// <summary>
        /// Gets the current physical state of the pointer (position, pressure, tilt, etc.).
        /// </summary>
        public readonly PointerState State;

        /// <summary>
        /// Gets high-frequency coalesced samples recorded between the last and current events.
        /// </summary>
        public readonly ReadOnlySpan<PointerState> CoalescedStates;

        /// <summary>
        /// Gets future-predicted samples estimated from the current pointer movement.
        /// </summary>
        public readonly ReadOnlySpan<PointerState> PredictedStates;

        /// <summary>
        /// Optional text measure context for hit-testing text positions.
        /// May be null on platforms that do not provide one.
        /// </summary>
        public readonly ITextMeasureContext? TextMeasure;

        /// <summary>
        /// Initializes a new instance of the <see cref="PointerEventRef"/> struct.
        /// </summary>
        public PointerEventRef(
            PointerEventType type,
            int pointerId,
            long persistentDeviceId,
            bool isPrimary,
            PointerState state,
            ReadOnlySpan<PointerState> coalescedStates,
            ReadOnlySpan<PointerState> predictedStates,
            ITextMeasureContext? textMeasure = null)
        {
            this.Type = type;
            PointerId = pointerId;
            PersistentDeviceId = persistentDeviceId;
            IsPrimary = isPrimary;
            State = state;
            CoalescedStates = coalescedStates;
            PredictedStates = predictedStates;
            TextMeasure = textMeasure;
        }
    }
}
