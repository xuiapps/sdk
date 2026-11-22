# Floorplan performance asset

Office.svg combines floorplan.svg and the reusable symbols from
https://github.com/pana-cc/floorplan-in-a-nutshell .
It contains vectors and compiled presentation attributes only. Original source/license details
are preserved in NOTICE.txt and provenance.json. Edit the standalone source
first, then regenerate this asset from that repository:

```sh
python3 -m unittest -v test_compile_svg.py
node --test test_detail.cjs
python3 preview.py build
python3 preview.py export --output /path/to/xuiapps/code/Xui/Apps/TestApp/Assets/Floorplan/Office.svg
```

`compile_svg.py` resolves authored styles and CSS variables ahead of time. The
explorer (`index.html`), authoring page (`editor.html`), offline previews, and this
export use the same compiler. The export excludes the raster reference. Never
hand-edit this generated SVG. The runtime imports it once, not on each frame.

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

`FloorplanDrawing` reads presentation attributes, local symbol references,
transforms, gradients, and paths once into immutable arrays. There is no runtime
CSS parser, selector matching, custom-property resolution, or per-element style
dictionary. `SvgPresentation` carries inherited values as a struct. Symbol cache
keys exclude placement IDs/positions, allowing identical furniture to share definitions.
Repeated instances share `Path2D` buffers. The export preserves `data-bounds`,
room identities, and painting order for runtime composition. The map declares
one whole-floorplan surface per overview level (0 and 1), then stable
room/structural groups for detail levels 2 and 3 through
`ICompositionContext`, without adding Xui child views. Offscreen regions are
culled; native backends can retain recently hidden surfaces. Steady camera
transforms reuse warmed surfaces without managed allocations. First exposure
and cache eviction can allocate new composition resources. See
`../../Pages/Floorplan/README.md` for the hierarchy and backing-density policy.

The importer deliberately supports the features used by this asset, not all of
SVG. CSS input is rejected with an instruction to compile it. Text uses bundled Inter. `IContext` has no text-stroke operation, so SVG
label outlines are omitted; overview halos are preserved as radial gradients.
Native rasterizer cost and allocations are outside the managed recording test.

The `office-attributes-v1` contract stores ordinary SVG attributes for its initial
L3/wall-L1 state. `data-state-attrs` lists properties with explicit overrides:
`data-l0w0-*` through `data-l3w1-*` cover the four furniture and two wall levels.
The browser applies these values; Xui selects L0/wall-L0 and L1–L3/wall-L1.
Marker opacity and room-label visibility remain controlled by the viewer.
Definitions with `data-source-id` identify their authored model; placement IDs
and geometry remain unchanged. Compiled source files are larger, but avoid the
selector traversal and temporary objects on every level during startup.

Integration coverage and the visual specification are in
`Xui/Tests/Integration/TestApp/FloorplanPerformanceTest.cs` and its
`Scenarios/Floorplan_RenderReference/README.md`.
