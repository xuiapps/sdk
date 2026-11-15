# Floorplan performance asset

Office.svg combines floorplan.svg and the reusable symbols from
https://github.com/pana-cc/floorplan-in-a-nutshell .
It contains vectors and embedded styles only. Original source/license details
are preserved in NOTICE.txt and provenance.json. Edit the standalone source
first, then reassemble this asset using that repository’s preview.compose(False),
removing source-image. The runtime imports this SVG once, not on each frame.

TestApp exposes **Performance → Office Floorplan**. One `FloorplanView` draws
through `IContext`: pan with one pointer, pinch with two, or zoom with the mouse
wheel. Initial fit leaves 10% space at each edge of the limiting dimension;
desktop/tablet maximum zoom is 500% of the viewport width. Furniture switches at
125%, 220%, and 400%; overview markers independently fade at 160%.
Phones (including emulators) double these values: maximum 1000%, furniture at
250%, 440%, and 800%, markers at 320%. Walls switch with the first furniture
level; room labels start at L2. Initial fit and gesture sensitivity stay the same.
On mobile, the map extends behind the header and status bar so the native blur
samples the floorplan colors. The header supplies no additional background fill.

`FloorplanDrawing` resolves styles, local symbol references, transforms,
gradients, and paths once into immutable arrays. Repeated symbol instances
share `Path2D` buffers. Rendering culls by conservative world-space room/element
bounds and allocates no managed objects after the context's buffers warm up.
The standard page shell still hosts the map; the map itself creates no native
groups, furniture views, or intermediate bitmaps.

The importer deliberately supports the features used by this asset, not all of
SVG/CSS. Text uses bundled Inter. `IContext` has no text-stroke operation, so SVG
label outlines are omitted; overview halos are preserved as radial gradients.
Native rasterizer cost and allocations are outside the managed recording test.

Integration coverage and the visual specification are in
`Xui/Tests/Integration/TestApp/FloorplanPerformanceTest.cs` and its
`Scenarios/Floorplan_RenderReference/README.md`.
