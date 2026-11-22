# Market Pulse

Open **Performance → Market Pulse · 5,000 rows** on mobile, or **Market Pulse** in the desktop menu.
Tap a company for its 60-session OHLC chart and generated company brief. The standard navigation header and swipe-back gesture return
to the original list offset. Stock details are the third page on the app navigation stack.
The taller introductory row scrolls away, and colorful rows paint underneath the blurred header. Everything is fictional, in memory, and repeatable from seed `0x584D4152`.

## Rendering design

- `MarketData` initially stores only PRNG checkpoints and empty cache slots for 5,000 companies.
  First access to a row generates its prices, list strings, normalized sparkline and mosaic logo;
  later accesses reuse that row. Scrolling or jumping to the end never creates intervening rows.
  The checkpoint stride preserves the original xorshift sequence regardless of access order.
- Opening a company prepares and caches its full OHLC history, chart labels and description.
  List rows retain only the normalized close points, not full candle arrays. Each brief combines
  three distinct sentences from a 50-sentence library, with its directional sentence selected to
  agree with the generated return.
- `MarketGrid` is a single leaf `View`, with zero child views regardless of dataset size. Its height
  is `168 + count × 100` points, including the introductory row. `ScrollView` owns input arbitration, inertia and the scrollbar. On mobile, clipping is owned by
  the navigation page so rows can draw into the header backdrop.
- The content-space culling rectangle determines the first and last visible row in constant time.
  One `DockLayer.Dock4` value composes logo, identity, quote and chevron `ILayer<MarketGrid>` structs.
  Each visible row feeds a cached data item through this composition; no row view is created,
  retained, rebound or recycled. Static generic painter dispatch avoids boxing.
- First visibility allocates only the missing rows in the viewport. Rendering cached rows performs
  arithmetic, reads cached data, and emits canvas commands without formatting strings or allocating
  collections. Row text is not measured. Detail paragraph wrapping happens during layout when the
  width changes. Cached rows and details remain available when returning from a detail page.
- Price and change share the chart's top line; the change aligns left and the price aligns right.
  The chart uses the remaining column width and is 47 points tall, with a wider chevron gutter.
- Row presses use the same `PressFeedbackLayer` as the home buttons: a 100ms delay when an ancestor
  advertises a competing touch drag, immediate feedback on a quick tap, then a 200ms hold and fade.
  Eight inline ripple slots are shared by the whole grid and keyed by row index; scrolling can
  cancel a pending press without flashing a highlight. No feedback state is allocated per row.

## Verification

`MarketPerformanceTest` tests deferred row/detail loading, deterministic random access and OHLC bounds,
viewport-bounded loading and drawing at three sizes, zero managed allocation across 128 cached frames, row
selection after scrolling, drag cancellation, list-position restoration, and phone SVG references.
The allocation assertion includes the real `RetainedCanvasContext`, not a no-op canvas. Native text
shaping/rasterization and OS compositor work are outside this managed recording measurement; it is
not a claim of zero native allocations or a device frame-rate benchmark.
