# Floorplan asset compiler

Run `tools/FloorplanCompiler/generate.sh` from any directory. Requires macOS and
.NET 10 because generation uses CoreGraphics. Ordinary app builds consume the
checked-in generated assets and do not run the compiler.

Optional arguments are the source SVG path and output directory, relative to the
repository root. For a reproducibility check:

```sh
tools/FloorplanCompiler/generate.sh Xui/Apps/TestApp/Assets/Floorplan/Office.svg /tmp/floorplan-atlas-check
diff -rq Xui/Apps/TestApp/Assets/FloorplanGpu /tmp/floorplan-atlas-check
```

The tool links the app's importer source so resolved styles and path behavior
stay aligned. Its compiler-only partial class adds export/rasterization code;
none of that export code ships in TestApp. See
`Xui/Apps/TestApp/Pages/FloorplanGpu/README.md` for asset formats and rendering.
