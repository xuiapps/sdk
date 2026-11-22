# Loading the office floorplan

The mobile menu starts `FloorplanDrawing.Preload` from both `ClickMaybe` and
`ClickFarFetch`. They join one shared task; `Task.Run` performs XML parsing and
geometry compilation. The task produces immutable drawing data, never views or
native drawing resources.

`FloorplanExample` immediately opens with four light-gray room outlines when the
drawing is not ready. Its loading view checks completion on UI animation frames
and creates `FloorplanView` there. While navigation is transitioning, it keeps
the same cached placeholder and defers hydration. After navigation, the outlines
shimmer until the drawing is ready. A warm load uses the map immediately; a failed
load displays an error instead of animating forever. Leaving a loading page does
not cancel the shared work, so a later visit can reuse it.

The mobile demo buttons enable `WaitForPressFeedback` on the shared DevKit button:

- `ClickMaybe`: once per press, when delayed highlight recognition succeeds (or
  when a quick tap commits). This is speculative; scrolling can still cancel it.
- `ClickFarFetch`: immediately on a valid release. This commits the click and is
  suitable for starting nonblocking work.
- `Click`: on release if the highlight has already rendered; otherwise on the
  animation tick after its highlighted render. Navigation runs here, outside the
  render traversal. The existing `Clicked` callback also runs at this point.

Handlers should start asynchronous preparation and return promptly. UI hydration
belongs on the UI thread. Regular buttons keep their existing immediate release
behavior unless they enable `WaitForPressFeedback`.

The synchronous `FloorplanDrawing.Shared` accessor and explicit drawing/task
constructors support rendering tools and deterministic tests. Interactive page
construction uses `LoadSharedAsync` and never waits on the loading task.

## Retained room composition

`FloorplanView` remains the only map view for input and layout. Its
`FloorplanDrawing.Composition` owns viewport/camera identities and declares
`ICompositionContext.Group` surfaces using stable `Region` and `DetailKey` data
objects. The SVG's `data-bounds` and room metadata partition drawing commands at
load time. Underlays, shared structure, rooms, and columns retain their original
painting order; nested rooms and discontiguous drawing runs remain separate.

The viewport clips through a backed composition group. Camera motion changes a
group transform, rather than the inherited canvas state or an arbitrary canvas
clip, so it does not invalidate cached content. Levels 0 and 1 each use one
whole-floorplan drawing surface, with independent stable keys. They preserve all
geometry in painting order and reuse the surface when the camera moves; no room
groups are declared at these levels. This keeps the native composition tree small
when most rooms are visible.

Levels 2 and 3 split drawing by room/structural region. Each visible region has a child
surface for its selected detail level and label/symbol mode. That child records
the entire region once, including portions outside the current viewport.
Offscreen regions are omitted; the backend keeps at most eight hidden children
per parent. Reappearing evicted regions are redrawn when `NeedsRedraw` requests it.

Backing density stays fixed within each detail level (1x, 2x, or 4x world units,
in addition to device scale). This lets pinch transforms reuse surfaces while
keeping the current phone's supported zoom range sharp. Larger displays can
stretch the capped 4x backing. A separate viewport-sized overlay repaints the
markers during camera motion and their opacity fade, then reuses them while
stationary. This bounds its texture size even if zoom changes before the markers
have finished fading out.

On iOS these groups map to native UIKit surfaces. This trades additional native
objects and backing storage for fewer redraws; first exposure, an evicted room,
a different detail/mode, or changed backing bounds/device scale can still need
new drawing. SVG and immediate backends always visit the drawing commands.
