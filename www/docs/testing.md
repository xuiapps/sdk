---
title: Testing
description: Snapshot tests using SvgDrawingContext — no GPU or display required.
---

# Testing

Xui views are testable without a real window or GPU. `SvgDrawingContext` is a software renderer that emits SVG, making it straightforward to write deterministic snapshot tests that run on any CI machine.

**Source:** `Xui/Runtime/Software/Actual/SvgDrawingContext.cs`, `Xui/Tests/Docs/Canvas/CanvasDocsTest.cs`.

## Rendering a view to SVG

```csharp
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Runtime.Software.Actual;

static string RenderToSvg(View view, Size size)
{
    using var stream  = new MemoryStream();
    using (var context = new SvgDrawingContext(size, stream, Xui.Core.Fonts.Inter.URIs, keepOpen: true))
    {
        view.Update(new LayoutGuide
        {
            Pass           = LayoutGuide.LayoutPass.Measure
                           | LayoutGuide.LayoutPass.Arrange
                           | LayoutGuide.LayoutPass.Render,
            AvailableSize  = size,
            Anchor         = (0, 0),
            XSize          = LayoutGuide.SizeTo.Exact,
            YSize          = LayoutGuide.SizeTo.Exact,
            XAlign         = LayoutGuide.Align.Start,
            YAlign         = LayoutGuide.Align.Start,
            MeasureContext = context,
            RenderContext  = context,
        });
    }

    stream.Position = 0;
    return new StreamReader(stream).ReadToEnd();
}
```

`keepOpen: true` keeps the underlying stream open so you can read it after the `SvgDrawingContext` is disposed.

## Snapshot test pattern

Write the SVG to a committed reference file on first run; diff on subsequent runs:

```csharp
[Fact]
public void ProgressBarRendersCorrectly()
{
    var view = new ProgressBar { Value = 0.6 };
    var svg  = RenderToSvg(view, new Size(320, 24));

    var snapshotPath = Path.Combine("Snapshots", "progress-bar.svg");

    if (!File.Exists(snapshotPath))
    {
        Directory.CreateDirectory(Path.GetDirectoryName(snapshotPath)!);
        File.WriteAllText(snapshotPath, svg);   // baseline — commit this file
        return;
    }

    Assert.Equal(File.ReadAllText(snapshotPath), svg);
}
```

Commit the `.svg` snapshots alongside your tests. Any visual regression appears as a text diff in pull requests.

## Using CallerFilePath for path resolution

When a test generates files that live next to the source, use `[CallerFilePath]` to resolve the output path without hard-coding absolute paths:

```csharp
private static void Generate(
    View view, string fileName,
    [CallerFilePath] string callerPath = "")
{
    var dir = Path.GetDirectoryName(callerPath)!;
    var svg = RenderToSvg(view, new Size(480, 240));
    File.WriteAllText(Path.Combine(dir, "Snapshots", fileName), svg);
}
```

## Docs figure generation

The `Xui/Tests/Docs/` project uses this pattern to generate the SVG figures embedded in `www/docs/img/canvas/`. Tests always pass — they just write the latest output and commit it:

```csharp
[Fact] public void FillAndStroke() => Generate(new FillAndStrokeView(), "fill-and-stroke.svg");
[Fact] public void Clip()          => Generate(new ClipView(),          "clip.svg");
```

The helper navigates four levels up from the test file to reach the repo root:

```csharp
var repoRoot = Path.GetFullPath(Path.Combine(sourceDir, "../../../.."));
var outDir   = Path.Combine(repoRoot, "www", "docs", "img", "canvas");
```

Run them before serving docs:

```
dotnet test Xui/Tests/Docs/Xui.Tests.Docs.csproj
docfx build docfx.json --serve
```

## Project setup

Add a reference to `Xui.Runtime.Software` in your test project — no platform runtime or display connection required:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="xunit" Version="2.*" />
    <ProjectReference Include="../../Core/Core/Xui.Core.csproj" />
    <ProjectReference Include="../../Core/Fonts/Xui.Core.Fonts.csproj" />
    <ProjectReference Include="../../Runtime/Software/Xui.Runtime.Software.csproj" />
  </ItemGroup>
</Project>
```

All rendering runs in-process. No GPU, no window manager, no display needed.
