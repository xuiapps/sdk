using System.Runtime.CompilerServices;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Runtime.Software.Actual;
using Xui.Tests.Docs.Canvas.Views;
using Xunit;

namespace Xui.Tests.Docs.Canvas;

/// <summary>
/// Generates SVG figures used in www/docs/canvas.md.
///
/// These are "generator" tests — they always write the output file and pass.
/// Run them locally after changing an example, then commit the updated SVGs.
///
/// Output: www/docs/img/canvas/*.svg  (relative to repo root via CallerFilePath)
/// </summary>
public class CanvasDocsTest
{
    private static readonly Size FigureSize = new Size(480, 240);

    [Fact] public void FillAndStroke() => Generate(new FillAndStrokeView(), "fill-and-stroke.svg");
    [Fact] public void Paths()         => Generate(new PathsView(),         "paths.svg");
    [Fact] public void Gradients()     => Generate(new GradientsView(),     "gradients.svg");
    [Fact] public void Clip()          => Generate(new ClipView(),          "clip.svg");
    [Fact] public void Transforms()    => Generate(new TransformsView(),    "transforms.svg");

    private static void Generate(View view, string fileName, [CallerFilePath] string callerPath = "")
    {
        // Navigate from Xui/Tests/Docs/Canvas/ (4 levels) to repo root, then into www/docs/img/canvas/
        var sourceDir = Path.GetDirectoryName(callerPath)!;
        var repoRoot  = Path.GetFullPath(Path.Combine(sourceDir, "../../../.."));
        var outDir    = Path.Combine(repoRoot, "www", "docs", "img", "canvas");
        Directory.CreateDirectory(outDir);

        using var stream = new MemoryStream();
        using (var context = new SvgDrawingContext(FigureSize, stream, Xui.Core.Fonts.Inter.URIs, keepOpen: true))
        {
            view.Update(new LayoutGuide
            {
                Anchor        = (0, 0),
                AvailableSize = FigureSize,
                Pass          = LayoutGuide.LayoutPass.Measure
                              | LayoutGuide.LayoutPass.Arrange
                              | LayoutGuide.LayoutPass.Render,
                MeasureContext = context,
                RenderContext  = context,
                XAlign = LayoutGuide.Align.Start,
                YAlign = LayoutGuide.Align.Start,
                XSize  = LayoutGuide.SizeTo.Exact,
                YSize  = LayoutGuide.SizeTo.Exact,
            });
        }

        stream.Position = 0;
        var svg = new StreamReader(stream).ReadToEnd();
        File.WriteAllText(Path.Combine(outDir, fileName), svg);
    }
}
