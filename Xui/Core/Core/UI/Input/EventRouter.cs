using Xui.Core.Math2D;
using Xui.Core.Abstract.Events;

namespace Xui.Core.UI.Input
{
    /// <summary>
    /// Routes pointer, mouse, scroll, and touch events through the view tree,
    /// performing hit testing, pointer capture, and enter/leave tracking.
    /// </summary>
    public class EventRouter
    {
        private readonly View _rootView;
        private readonly Dictionary<int, PointerTracking> _pointerTracking = new();
        private readonly List<View> _route = new List<View>(64);

        /// <summary>Initializes a new <see cref="EventRouter"/> rooted at the given view.</summary>
        /// <param name="rootView">The root of the view tree to route events through.</param>
        public EventRouter(View rootView)
        {
            _rootView = rootView;
        }

        /// <summary>Dispatches a touch event, translating each touch point into a pointer event.</summary>
        public void Dispatch(ref TouchEventRef touchEvent)
        {
            foreach (var touch in touchEvent.Touches)
            {
                var eventType = touch.Phase switch
                {
                    TouchPhase.Start => PointerEventType.Down,
                    TouchPhase.Move => PointerEventType.Move,
                    TouchPhase.End => PointerEventType.Up,
                    _ => throw new InvalidOperationException()
                };

                int pointerId = touch.Index;

                var state = new PointerState(
                    position: touch.Position,
                    contactSize: new Size(touch.Radius, touch.Radius),
                    pressure: 1.0f,
                    tangentialPressure: 0,
                    tilt: (0, 0),
                    twist: 0,
                    altitudeAngle: MathF.PI / 2,
                    azimuthAngle: 0,
                    pointerType: PointerType.Touch,
                    button: PointerButton.Left,
                    buttons: PointerButtons.Left);

                var pointerEvent = new PointerEventRef(
                    eventType,
                    pointerId,
                    persistentDeviceId: 0,
                    isPrimary: true,
                    state,
                    ReadOnlySpan<PointerState>.Empty,
                    ReadOnlySpan<PointerState>.Empty);

                DispatchPointer(ref pointerEvent);
            }
        }

        /// <summary>Dispatches a mouse button down event as a pointer down event.</summary>
        public void Dispatch(ref MouseDownEventRef e)
        {
            var state = new PointerState(
                position: e.Position,
                contactSize: new Size(1, 1),
                pressure: 1.0f,
                tangentialPressure: 0,
                tilt: (0, 0),
                twist: 0,
                altitudeAngle: MathF.PI / 2,
                azimuthAngle: 0,
                pointerType: PointerType.Mouse,
                button: PointerButton.Left,
                buttons: PointerButtons.Left);

            var pe = new PointerEventRef(
                PointerEventType.Down,
                pointerId: 0,
                persistentDeviceId: 0,
                isPrimary: true,
                state,
                ReadOnlySpan<PointerState>.Empty,
                ReadOnlySpan<PointerState>.Empty,
                e.TextMeasure);

            DispatchPointer(ref pe);
        }

        /// <summary>Dispatches a mouse button up event as a pointer up event.</summary>
        public void Dispatch(ref MouseUpEventRef e)
        {
            var state = new PointerState(
                position: e.Position,
                contactSize: new Size(1, 1),
                pressure: 0.0f,
                tangentialPressure: 0,
                tilt: (0, 0),
                twist: 0,
                altitudeAngle: MathF.PI / 2,
                azimuthAngle: 0,
                pointerType: PointerType.Mouse,
                button: PointerButton.Left,
                buttons: PointerButtons.None);

            var pe = new PointerEventRef(
                PointerEventType.Up,
                pointerId: 0,
                persistentDeviceId: 0,
                isPrimary: true,
                state,
                ReadOnlySpan<PointerState>.Empty,
                ReadOnlySpan<PointerState>.Empty,
                e.TextMeasure);

            DispatchPointer(ref pe);
        }

        /// <summary>Dispatches a mouse move event as a pointer move event.</summary>
        public void Dispatch(ref MouseMoveEventRef e)
        {
            var state = new PointerState(
                position: e.Position,
                contactSize: new Size(1, 1),
                pressure: 0.0f,
                tangentialPressure: 0,
                tilt: (0, 0),
                twist: 0,
                altitudeAngle: MathF.PI / 2,
                azimuthAngle: 0,
                pointerType: PointerType.Mouse,
                button: PointerButton.None,
                buttons: PointerButtons.None);

            var pe = new PointerEventRef(
                PointerEventType.Move,
                pointerId: 0,
                persistentDeviceId: 0,
                isPrimary: true,
                state,
                ReadOnlySpan<PointerState>.Empty,
                ReadOnlySpan<PointerState>.Empty,
                e.TextMeasure);

            DispatchPointer(ref pe);
        }

        private void DispatchPointer(ref PointerEventRef e)
        {
            // TODO: This is a solid breakdown of what we need to do here...
            // Pretty broken... For each pointer, we need a linked list of elements that had been "entered",
            // then on move (or down, or up, or lost capture) reevaluate the elements under the pointer,
            // calculate diff - and dispatch leave on the old element and enter on the new elements,
            // while not dispatching anything on common root element. 

            if (!_pointerTracking.TryGetValue(e.PointerId, out var tracking))
            {
                _pointerTracking[e.PointerId] = tracking = new PointerTracking();
            }

            View? targetView;

            if (tracking.Captured != null)
            {
                // Pointer is captured — force target to captured view
                targetView = tracking.Captured;
            }
            else
            {
                // Perform hit test
                targetView = HitTest(_rootView, e.State.Position);

                // Handle pointerout/pointerover (only when not captured)
                if (tracking.OverTarget != targetView)
                {
                    if (tracking.OverTarget != null)
                    {
                        var outEvent = new PointerEventRef(
                            PointerEventType.Out,
                            e.PointerId,
                            e.PersistentDeviceId,
                            e.IsPrimary,
                            e.State,
                            ReadOnlySpan<PointerState>.Empty,
                            ReadOnlySpan<PointerState>.Empty);
                        tracking.OverTarget.OnPointerEvent(ref outEvent, EventPhase.Bubble);
                    }

                    if (targetView != null)
                    {
                        var overEvent = new PointerEventRef(
                            PointerEventType.Over,
                            e.PointerId,
                            e.PersistentDeviceId,
                            e.IsPrimary,
                            e.State,
                            ReadOnlySpan<PointerState>.Empty,
                            ReadOnlySpan<PointerState>.Empty);
                        targetView.OnPointerEvent(ref overEvent, EventPhase.Bubble);
                    }

                    tracking = _pointerTracking[e.PointerId];
                    tracking.OverTarget = targetView;
                    _pointerTracking[e.PointerId] = tracking;
                }

                // Handle pointerenter/pointerleave (only when not captured)
                if (tracking.PreviousTarget != targetView)
                {
                    if (tracking.PreviousTarget != null)
                    {
                        var leaveEvent = new PointerEventRef(
                            PointerEventType.Leave,
                            e.PointerId,
                            e.PersistentDeviceId,
                            e.IsPrimary,
                            e.State,
                            ReadOnlySpan<PointerState>.Empty,
                            ReadOnlySpan<PointerState>.Empty);
                        tracking.PreviousTarget.OnPointerEvent(ref leaveEvent, EventPhase.Bubble);
                    }

                    if (targetView != null)
                    {
                        var enterEvent = new PointerEventRef(
                            PointerEventType.Enter,
                            e.PointerId,
                            e.PersistentDeviceId,
                            e.IsPrimary,
                            e.State,
                            ReadOnlySpan<PointerState>.Empty,
                            ReadOnlySpan<PointerState>.Empty);
                        targetView.OnPointerEvent(ref enterEvent, EventPhase.Tunnel);
                    }

                    tracking = _pointerTracking[e.PointerId];
                    tracking.PreviousTarget = targetView;
                    _pointerTracking[e.PointerId] = tracking;
                }
            }

            if (targetView == null)
                return;

            // Route event to target
            BuildRoute(targetView);

            for (int i = _route.Count - 1; i >= 0; i--)
                _route[i].OnPointerEvent(ref e, EventPhase.Tunnel);

            for (int i = 0; i < _route.Count; i++)
                _route[i].OnPointerEvent(ref e, EventPhase.Bubble);

            tracking = _pointerTracking[e.PointerId];
            tracking.LastPosition = e.State.Position;
            tracking.LastState = e.State;
            _pointerTracking[e.PointerId] = tracking;
        }

        /// <summary>Captures all subsequent pointer events for the given pointer ID to <paramref name="view"/>.</summary>
        /// <param name="view">The view that will exclusively receive events for this pointer.</param>
        /// <param name="pointerId">The pointer identifier to capture.</param>
        public void CapturePointer(View view, int pointerId)
        {
            if (_pointerTracking.TryGetValue(pointerId, out var tracking))
            {
                if (tracking.Captured == view)
                    return;

                // Send LostCapture to previous holder before transferring
                var previousCapture = tracking.Captured;
                tracking.Captured = view;
                _pointerTracking[pointerId] = tracking;

                if (previousCapture != null)
                {
                    var lostEvt = new PointerEventRef(PointerEventType.LostCapture, pointerId, 0, true, tracking.LastState, ReadOnlySpan<PointerState>.Empty, ReadOnlySpan<PointerState>.Empty);
                    previousCapture.OnPointerEvent(ref lostEvt, EventPhase.Bubble);
                }

                var evt = new PointerEventRef(PointerEventType.GotCapture, pointerId, 0, true, tracking.LastState, ReadOnlySpan<PointerState>.Empty, ReadOnlySpan<PointerState>.Empty);
                view.OnPointerEvent(ref evt, EventPhase.Bubble);
            }
        }

        /// <summary>Releases pointer capture previously set by <see cref="CapturePointer"/>.</summary>
        /// <param name="view">The view that currently holds the capture.</param>
        /// <param name="pointerId">The pointer identifier to release.</param>
        public void ReleasePointer(View view, int pointerId)
        {
            if (_pointerTracking.TryGetValue(pointerId, out var tracking) && tracking.Captured == view)
            {
                tracking.Captured = null;
                _pointerTracking[pointerId] = tracking;

                var evt = new PointerEventRef(PointerEventType.LostCapture, pointerId, 0, true, tracking.LastState, ReadOnlySpan<PointerState>.Empty, ReadOnlySpan<PointerState>.Empty);
                view.OnPointerEvent(ref evt, EventPhase.Bubble);
            }
        }

        /// <summary>
        /// Dispatches a scroll wheel event to the deepest view under <paramref name="position"/>
        /// that handles it, bubbling up until <see cref="ScrollWheelEventRef.Handled"/> is set.
        /// </summary>
        public void Dispatch(ref ScrollWheelEventRef e, Point position)
        {
            DispatchScrollWheel(_rootView, ref e, position);
        }

        private static void DispatchScrollWheel(View view, ref ScrollWheelEventRef e, Point position)
        {
            if (e.Handled) return;
            if (!view.Frame.Contains(position)) return;

            // Depth-first: innermost child first
            for (int i = view.Count - 1; i >= 0; i--)
            {
                DispatchScrollWheel(view[i], ref e, position);
                if (e.Handled) return;
            }

            // Bubble to this view if no child handled it
            view.OnScrollWheel(ref e);
        }

        private View? HitTest(View view, Point position)
        {
            for (int i = view.Count - 1; i >= 0; i--)
            {
                var hit = HitTest(view[i], position);
                if (hit != null)
                    return hit;
            }
            return view.HitTest(position) ? view : null;
        }

        private void BuildRoute(View target)
        {
            _route.Clear();
            for (var current = target; current != null; current = current.Parent)
                _route.Add(current);
        }

        private struct PointerTracking
        {
            public View? Captured;
            public View? PreviousTarget;
            public View? OverTarget;
            public Point LastPosition;
            public PointerState LastState;
        }
    }
}
