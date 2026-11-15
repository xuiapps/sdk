# Market Pulse

Open **Performance → Market Pulse · 5,000 rows** on mobile, or **Market Pulse** in the desktop menu.
Tap a company for its 60-session OHLC chart and generated company brief. The standard navigation header and swipe-back gesture return
to the original list offset. Stock details are the third page on the app navigation stack.
The taller introductory row scrolls away, and colorful rows paint underneath the blurred header. Everything is fictional, in memory, and repeatable from seed `0x584D4152`.

## Rendering design

- `MarketData` generates 5,000 companies once, using an explicit xorshift PRNG. Prices, formatted
  strings, normalized sparkline points and mirrored mosaic logo rectangles are prepared up front.
  Each brief combines three distinct sentences from a 50-sentence library, with its directional
  sentence selected to agree with the generated return.
- `MarketGrid` is a single leaf `View`, with zero child views regardless of dataset size. Its height
  is `168 + count × 100` points, including the introductory row. `ScrollView` owns input arbitration, inertia and the scrollbar. On mobile, clipping is owned by
  the navigation page so rows can draw into the header backdrop.
- The content-space culling rectangle determines the first and last visible row in constant time.
  One `DockLayer.Dock4` value composes logo, identity, quote and chevron `ILayer<MarketGrid>` structs.
  Each visible row feeds a different data item through this composition; no row object is created,
  retained, rebound or recycled. Static generic painter dispatch avoids boxing.
- Rendering performs arithmetic, reads cached data, and emits canvas commands. It does not format
  strings, measure row text, run LINQ, allocate collections or load assets. Detail paragraph wrapping
  happens during layout when the width changes. Page creation and initial dataset generation may allocate.
- Price and change share the chart's top line; the change aligns left and the price aligns right.
  The chart uses the remaining column width and is 47 points tall, with a wider chevron gutter.
- Row presses use the same `PressFeedbackLayer` as the home buttons: a 100ms delay when an ancestor
  advertises a competing touch drag, immediate feedback on a quick tap, then a 200ms hold and fade.
  Eight inline ripple slots are shared by the whole grid and keyed by row index; scrolling can
  cancel a pending press without flashing a highlight. No feedback state is allocated per row.

## Verification

`MarketPerformanceTest` tests deterministic data and OHLC bounds, viewport-bounded drawing at three
sizes, zero managed allocation across 128 warmed command-recording frames revealing new rows, row
selection after scrolling, drag cancellation, list-position restoration, and phone SVG references.
The allocation assertion includes the real `RetainedCanvasContext`, not a no-op canvas. Native text
shaping/rasterization and OS compositor work are outside this managed recording measurement; it is
not a claim of zero native allocations or a device frame-rate benchmark.
