namespace Xui.Apps.TestApp.Pages.Canvas;

/// <summary>Short introductions and representative canvas calls for the mobile gallery.</summary>
internal static class CanvasCardDescriptions
{
    public static ExampleCardDescription Drawing(string name) => name switch
    {
        "FillRect" => new("COLOR", "Color in one call.", "Simple rectangles. Endless compositions.",
            "ctx.FillRect((0, 0, 100, 100));"),
        "QuadraticCurve" => new("PATHS", "Bend the line.", "One control point shapes the whole curve.",
            "ctx.CurveTo((150, 20), (270, 200));"),
        "CubicCurve" => new("PATHS", "Find your curve.", "Two control points. A little more freedom.",
            "ctx.CurveTo((50, 30),\n    (250, 30), (270, 200));"),
        "HeartCurve" => new("PATHS", "Made with curves.", "A familiar shape, built from flowing paths.",
            "ctx.ClosePath();\n    ctx.Fill();"),
        "Arc" => new("GEOMETRY", "Around the bend.", "Circles and arcs share the same beginning.",
            "ctx.Arc((100, 100), 60, 0, NFloat.Pi);"),
        "ArcFlower" => new("GEOMETRY", "Let it bloom.", "Repeat a simple arc to grow a pattern.",
            "ctx.Arc(center, radius, start, end);"),
        "Ellipse" => new("GEOMETRY", "Circles, stretched.", "Change the radii. Rotate the possibilities.",
            "ctx.Ellipse((80, 60), 60, 30,\n    0, 0, NFloat.Pi * 2);"),
        "ArcTo" => new("PATHS", "Turn softly.", "Round a corner where two lines meet.",
            "ctx.ArcTo((150, 20), (150, 100), 30);"),
        "ArcToFlower" => new("PATHS", "Corners in bloom.", "Rounded turns become a continuous motif.",
            "ctx.ArcTo(corner, next, radius);"),
        "RoundRect" => new("GEOMETRY", "Soften the edges.", "A small radius changes the entire feel.",
            "ctx.RoundRect((0, 0, 100, 80), 16);\n    ctx.Fill();"),
        "LineCapJoin" => new("STROKES", "The finishing touch.", "Explore how strokes meet and how they end.",
            "ctx.LineCap = LineCap.Round;\n    ctx.LineJoin = LineJoin.Round;"),
        "DashPattern" => new("STROKES", "A little rhythm.", "Dashes, dots and spaces set the pace.",
            "ctx.SetLineDash([10, 10]);\n    ctx.Stroke();"),
        "FillRule" => new("FILL", "Inside or outside?", "The fill rule decides which spaces belong.",
            "ctx.Fill(FillRule.EvenOdd);"),
        "PathContinuation" => new("PATHS", "Keep the line going.", "Connect segments into one continuous path.",
            "ctx.LineTo((100, 100));\n    ctx.Stroke();"),
        "Clip" => new("COMPOSITION", "A window into color.", "A path reveals just the artwork inside it.",
            "ctx.Clip();\n    ctx.FillRect((0, 0, 300, 300));"),
        "Transform" => new("COMPOSITION", "A new perspective.", "Move, rotate and scale the drawing space.",
            "ctx.Translate((150, 150));\n    ctx.Rotate(NFloat.Pi / 4);"),
        "Star" => new("PATHS", "Make a point.", "Straight lines come together to shine.",
            "ctx.LineTo(point);\n    ctx.ClosePath();"),
        "GlobalAlpha" => new("COLOR", "See through color.", "Layer translucent shapes to reveal more.",
            "ctx.GlobalAlpha = 0.5f;\n    ctx.Fill();"),
        "Text" => new("TYPOGRAPHY", "Words with character.", "Weight, color and alignment tell a story.",
            "ctx.FillText(\"Hello, canvas.\", (0, 0));"),
        "RainbowTextFill" => new("TYPOGRAPHY", "Type in full color.", "Let a gradient flow through the lettering.",
            "ctx.SetFill(gradient);\n    ctx.FillText(text, origin);"),
        "DrawImage" => new("IMAGES", "Bring pixels along.", "Place an image anywhere on the canvas.",
            "ctx.DrawImage(image, destination);"),
        "BitmapFill" => new("IMAGES", "Texture takes shape.", "An image becomes the fill inside a path.",
            "ctx.SetFill(new ImagePattern(image));\n    ctx.Fill();"),
        _ => throw new ArgumentOutOfRangeException(nameof(name), name, "Unknown drawing example."),
    };

    public static ExampleCardDescription Animation(string name) => name switch
    {
        "BeatingHeart" => new("MOTION", "A little heartbeat.", "A double pulse brings a simple path to life.",
            "ctx.Scale((pulse, pulse));"),
        "MorphingShape" => new("MOTION", "Always becoming.", "Circle, square, flower. One flowing outline.",
            "ctx.LineTo(point);\n    ctx.Fill();"),
        "DownloadComplete" => new("MOTION", "From waiting to done.", "Progress flows into a satisfying check.",
            "ctx.Arc(center, radius, start, end);\n    ctx.Stroke();"),
        _ => throw new ArgumentOutOfRangeException(nameof(name), name, "Unknown animation example."),
    };
}
