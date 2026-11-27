# TestApp: real View layout and rendering

[Open standalone demo](FillRectRender.html)

Uses the explicit `XUI_COMPILER_CANVAS` Core profile: instrumentation calls are omitted, and default font weight/stretch are constructed directly. See [profile details](../README.md).

Source: [Render.cs](Fixtures/TestApp/Render.cs), [CanvasContext.cs](Fixtures/TestApp/CanvasContext.cs). The test reads the unchanged TestApp `Pages/Canvas/Tests/FillRectTest.cs` from the repository.

## Driver

```csharp
using System;
using TestAppCanvas;
using Xui.Apps.TestApp.Pages.Canvas.Tests;
using Xui.Core.Math2D;

public static class Program
{
    public static int Main()
    {
        var context = new CanvasContext();
        var view = new FillRectTest();
        var desired = view.Measure((400, 240), context);
        var frame = view.Arrange((20, 30, 400, 240), context, desired);
        view.Render(context);
        return frame == new Rect(20, 30, 400, 240) ? 0 : 1;
    }
}
```

## TestApp drawing code

```csharp
using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;
using static Xui.Core.Canvas.Colors;

namespace Xui.Apps.TestApp.Pages.Canvas.Tests;

public class FillRectTest : View, ICanvasDrawingExample
{
    Rect ICanvasDrawingExample.GetArtworkBounds(IMeasureContext context) => new(10, 10, 340, 160);

    protected override void RenderCore(IContext context)
    {
        NFloat x = this.Frame.X;
        NFloat y = this.Frame.Y;

        // Red rectangle
        context.SetFill(Red);
        context.FillRect(new Rect(x + 10, y + 10, 100, 80));

        // Green rectangle
        context.SetFill(Green);
        context.FillRect(new Rect(x + 130, y + 10, 100, 80));

        // Blue rectangle
        context.SetFill(Blue);
        context.FillRect(new Rect(x + 250, y + 10, 100, 80));

        // Overlapping: orange over partial red
        context.SetFill(Orange);
        context.FillRect(new Rect(x + 60, y + 50, 100, 80));

        // Small black square
        context.SetFill(Black);
        context.FillRect(new Rect(x + 10, y + 150, 20, 20));
    }
}
```

## Drawing commands

```json
[
  [
    "Save",
    0,
    0,
    0,
    0,
    ""
  ],
  [
    "GlobalAlpha",
    1,
    0,
    0,
    0,
    ""
  ],
  [
    "LineCap",
    0,
    0,
    0,
    0,
    ""
  ],
  [
    "LineJoin",
    0,
    0,
    0,
    0,
    ""
  ],
  [
    "LineWidth",
    1,
    0,
    0,
    0,
    ""
  ],
  [
    "MiterLimit",
    10,
    0,
    0,
    0,
    ""
  ],
  [
    "LineDashOffset",
    0,
    0,
    0,
    0,
    ""
  ],
  [
    "SetLineDash",
    0,
    0,
    0,
    0,
    ""
  ],
  [
    "SetStroke",
    0,
    0,
    0,
    1,
    ""
  ],
  [
    "SetFill",
    0.501960813999176,
    0.501960813999176,
    0.501960813999176,
    1,
    ""
  ],
  [
    "Font",
    12,
    400,
    100,
    14.40000057220459,
    "Inter"
  ],
  [
    "FontStyle",
    0,
    0,
    14,
    0,
    ""
  ],
  [
    "TextAlign",
    0,
    0,
    0,
    0,
    ""
  ],
  [
    "TextBaseline",
    3,
    0,
    0,
    0,
    ""
  ],
  [
    "BeginPath",
    0,
    0,
    0,
    0,
    ""
  ],
  [
    "SetFill",
    1,
    0,
    0,
    1,
    ""
  ],
  [
    "FillRect",
    30,
    40,
    100,
    80,
    ""
  ],
  [
    "SetFill",
    0,
    0.501960813999176,
    0,
    1,
    ""
  ],
  [
    "FillRect",
    150,
    40,
    100,
    80,
    ""
  ],
  [
    "SetFill",
    0,
    0,
    1,
    1,
    ""
  ],
  [
    "FillRect",
    270,
    40,
    100,
    80,
    ""
  ],
  [
    "SetFill",
    1,
    0.6470588445663452,
    0,
    1,
    ""
  ],
  [
    "FillRect",
    80,
    80,
    100,
    80,
    ""
  ],
  [
    "SetFill",
    0,
    0,
    0,
    1,
    ""
  ],
  [
    "FillRect",
    30,
    180,
    20,
    20,
    ""
  ],
  [
    "Restore",
    0,
    0,
    0,
    0,
    ""
  ]
]
```

## Exit Code

```text
0
```

## Retained methods

- `TestApp.Render.exe`: `Program.Main`
- `TestApp.Render.exe`: `TestAppCanvas.CanvasContext..ctor`
- `TestApp.Canvas.dll`: `Xui.Apps.TestApp.Pages.Canvas.Tests.FillRectTest..ctor`
- `System.Private.CoreLib.dll`: `System.ValueTuple<System.Runtime.InteropServices.NFloat, System.Runtime.InteropServices.NFloat>..ctor`
- `Xui.Core.dll`: `Xui.Core.Math2D.Size.op_Implicit`
- `Xui.Core.dll`: `Xui.Core.UI.View.Measure`
- `System.Private.CoreLib.dll`: `System.ValueTuple<System.Runtime.InteropServices.NFloat, System.Runtime.InteropServices.NFloat, System.Runtime.InteropServices.NFloat, System.Runtime.InteropServices.NFloat>..ctor`
- `Xui.Core.dll`: `Xui.Core.Math2D.Rect.op_Implicit`
- `System.Private.CoreLib.dll`: `System.Nullable<Xui.Core.Math2D.Size>..ctor`
- `Xui.Core.dll`: `Xui.Core.UI.View.Arrange`
- `Xui.Core.dll`: `Xui.Core.UI.View.Render`
- `Xui.Core.dll`: `Xui.Core.Math2D.Rect..ctor`
- `Xui.Core.dll`: `Xui.Core.Math2D.Rect.op_Equality`
- `Xui.Core.dll`: `Xui.Core.UI.View..ctor`
- `Xui.Core.dll`: `Xui.Core.Math2D.Size..ctor`
- `Xui.Core.dll`: `Xui.Core.UI.LayoutFrameContext..ctor`
- `Xui.Core.dll`: `Xui.Core.UI.MeasureConstraints..ctor`
- `Xui.Core.dll`: `Xui.Core.UI.View.MeasureShell`
- `System.Private.CoreLib.dll`: `System.Nullable<Xui.Core.Math2D.Size>.get_HasValue`
- `Xui.Core.dll`: `Xui.Core.Math2D.Rect.get_Size`
- `System.Private.CoreLib.dll`: `System.Nullable<Xui.Core.Math2D.Size>.GetValueOrDefault`
- `Xui.Core.dll`: `Xui.Core.Math2D.Rect.get_TopLeft`
- `Xui.Core.dll`: `Xui.Core.UI.ArrangeConstraints..ctor`
- `Xui.Core.dll`: `Xui.Core.UI.View.ArrangeShell`
- `Xui.Core.dll`: `Xui.Core.UI.View.RenderShell`
- `Xui.Core.dll`: `Xui.Core.Math2D.Frame.op_Implicit`
- `Xui.Core.dll`: `Xui.Core.UI.View.CanReuseMeasure`
- `Xui.Core.dll`: `Xui.Core.UI.MeasureConstraints.get_AvailableSize`
- `Xui.Core.dll`: `Xui.Core.Math2D.Size.Max`
- `Xui.Core.dll`: `Xui.Core.UI.MeasureConstraints.get_XSize`
- `Xui.Core.dll`: `Xui.Core.UI.View.get_HorizontalAlignment`
- `Xui.Core.dll`: `Xui.Core.UI.MeasureConstraints.get_YSize`
- `Xui.Core.dll`: `Xui.Core.UI.View.get_VerticalAlignment`
- `Xui.Core.dll`: `Xui.Core.UI.View.get_Margin`
- `Xui.Core.dll`: `Xui.Core.Math2D.Size.op_Subtraction`
- `Xui.Core.dll`: `Xui.Core.UI.LayoutFrameContext.get_MeasureContext`
- `Xui.Core.dll`: `Xui.Core.UI.View.get_MinimumWidth`
- `Xui.Core.dll`: `Xui.Core.UI.View.get_MaximumWidth`
- `Xui.Core.dll`: `Xui.Core.UI.View.get_MinimumHeight`
- `Xui.Core.dll`: `Xui.Core.UI.View.get_MaximumHeight`
- `Xui.Core.dll`: `Xui.Core.Math2D.Size.op_Addition`
- `Xui.Core.dll`: `Xui.Core.UI.View.ValidateMeasure`
- `Xui.Core.dll`: `Xui.Core.Math2D.Point..ctor`
- `Xui.Core.dll`: `Xui.Core.UI.View.CanReuseArrange`
- `Xui.Core.dll`: `Xui.Core.UI.View.get_Frame`
- `Xui.Core.dll`: `Xui.Core.UI.ArrangeConstraints.get_Anchor`
- `Xui.Core.dll`: `Xui.Core.UI.ArrangeConstraints.get_DesiredSize`
- `Xui.Core.dll`: `Xui.Core.UI.ArrangeConstraints.get_XAlign`
- `Xui.Core.dll`: `Xui.Core.UI.ArrangeConstraints.get_YAlign`
- `Xui.Core.dll`: `Xui.Core.UI.ArrangeConstraints.get_AvailableSize`
- `Xui.Core.dll`: `Xui.Core.UI.ArrangeConstraints.get_XSize`
- `Xui.Core.dll`: `Xui.Core.UI.ArrangeConstraints.get_YSize`
- `Xui.Core.dll`: `Xui.Core.Math2D.Rect.op_Subtraction`
- `Xui.Core.dll`: `Xui.Core.Math2D.Rect.op_Inequality`
- `Xui.Core.dll`: `Xui.Core.UI.View.InvalidateRender`
- `Xui.Core.dll`: `Xui.Core.UI.View.set_Frame`
- `Xui.Core.dll`: `Xui.Core.UI.View.ValidateArrange`
- `System.Private.CoreLib.dll`: `System.Nullable<Xui.Core.Math2D.Rect>.get_HasValue`
- `Xui.Core.dll`: `Xui.Core.UI.View.get_Parent`
- `System.Private.CoreLib.dll`: `System.Nullable<Xui.Core.Math2D.Rect>..ctor`
- `System.Private.CoreLib.dll`: `System.Nullable<Xui.Core.Math2D.Rect>.GetValueOrDefault`
- `Xui.Core.dll`: `Xui.Core.Math2D.Rect.Intersects`
- `Xui.Core.dll`: `Xui.Core.UI.LayoutFrameContext.get_RenderContext`
- `Xui.Core.dll`: `Xui.Core.UI.View.get_HasRenderUpdates`
- `Xui.Core.dll`: `Xui.Core.UI.View.ValidateRender`
- `Xui.Core.dll`: `Xui.Core.Math2D.Frame..ctor`
- `Xui.Core.dll`: `Xui.Core.UI.View.get_Flags`
- `Xui.Core.dll`: `Xui.Core.Math2D.Size.op_Equality`
- `Xui.Core.dll`: `Xui.Core.UI.View.MeasureCore`
- `Xui.Core.dll`: `Xui.Core.UI.View.set_Flags`
- `Xui.Core.dll`: `Xui.Core.Math2D.Point.op_Equality`
- `Xui.Core.dll`: `Xui.Core.UI.View.ArrangeCore`
- `Xui.Core.dll`: `Xui.Core.UI.View.get_CullingFrame`
- `Xui.Core.dll`: `Xui.Core.Math2D.Rect.get_Left`
- `Xui.Core.dll`: `Xui.Core.Math2D.Rect.get_Right`
- `Xui.Core.dll`: `Xui.Core.Math2D.Rect.get_Top`
- `Xui.Core.dll`: `Xui.Core.Math2D.Rect.get_Bottom`
- `Xui.Core.dll`: `Xui.Core.UI.View.TransformRect`
- `Xui.Core.dll`: `Xui.Core.Canvas.IStateContext.Save`
- `TestApp.Render.exe`: `TestAppCanvas.CanvasContext.Save`
- `Xui.Core.dll`: `Xui.Core.UI.View.SetDefaultRenderState`
- `Xui.Core.dll`: `Xui.Core.UI.View.RenderCore`
- `TestApp.Canvas.dll`: `Xui.Apps.TestApp.Pages.Canvas.Tests.FillRectTest.RenderCore`
- `Xui.Core.dll`: `Xui.Core.Canvas.IStateContext.Restore`
- `TestApp.Render.exe`: `TestAppCanvas.CanvasContext.Restore`
- `Xui.Core.dll`: `Xui.Core.UI.View.OnChildRenderChanged`
- `Xui.Core.dll`: `Xui.Core.Canvas.Color.op_Implicit`
- `Xui.Core.dll`: `Xui.Core.Canvas.Font..ctor`
- `Xui.Core.dll`: `Xui.Core.UI.View.RenderChild`
- `Xui.Core.dll`: `Xui.Core.UI.View.get_Item`
- `Xui.Core.dll`: `Xui.Core.UI.View.get_Count`
- `Xui.Core.dll`: `Xui.Core.Canvas.IPenContext.set_GlobalAlpha`
- `TestApp.Render.exe`: `TestAppCanvas.CanvasContext.set_GlobalAlpha`
- `Xui.Core.dll`: `Xui.Core.Canvas.IPenContext.set_LineCap`
- `TestApp.Render.exe`: `TestAppCanvas.CanvasContext.set_LineCap`
- `Xui.Core.dll`: `Xui.Core.Canvas.IPenContext.set_LineJoin`
- `TestApp.Render.exe`: `TestAppCanvas.CanvasContext.set_LineJoin`
- `Xui.Core.dll`: `Xui.Core.Canvas.IPenContext.set_LineWidth`
- `TestApp.Render.exe`: `TestAppCanvas.CanvasContext.set_LineWidth`
- `Xui.Core.dll`: `Xui.Core.Canvas.IPenContext.set_MiterLimit`
- `TestApp.Render.exe`: `TestAppCanvas.CanvasContext.set_MiterLimit`
- `Xui.Core.dll`: `Xui.Core.Canvas.IPenContext.set_LineDashOffset`
- `TestApp.Render.exe`: `TestAppCanvas.CanvasContext.set_LineDashOffset`
- `Xui.Core.dll`: `Xui.Core.Canvas.IPenContext.SetLineDash`
- `TestApp.Render.exe`: `TestAppCanvas.CanvasContext.SetLineDash`
- `Xui.Core.dll`: `Xui.Core.Canvas.Color..ctor`
- `Xui.Core.dll`: `Xui.Core.Canvas.IPenContext.SetStroke`
- `TestApp.Render.exe`: `TestAppCanvas.CanvasContext.SetStroke`
- `Xui.Core.dll`: `Xui.Core.Canvas.IPenContext.SetFill`
- `TestApp.Render.exe`: `TestAppCanvas.CanvasContext.SetFill`
- `Xui.Core.dll`: `Xui.Core.Canvas.Font.set_FontFamily`
- `Xui.Core.dll`: `Xui.Core.Canvas.Font.set_FontSize`
- `System.Private.CoreLib.dll`: `System.Nullable<Xui.Core.Canvas.FontWeight>.get_HasValue`
- `Xui.Core.dll`: `Xui.Core.Canvas.FontWeight..ctor`
- `System.Private.CoreLib.dll`: `System.Nullable<Xui.Core.Canvas.FontWeight>.GetValueOrDefault`
- `Xui.Core.dll`: `Xui.Core.Canvas.Font.set_FontWeight`
- `System.Private.CoreLib.dll`: `System.Nullable<Xui.Core.Canvas.FontStyle>.get_HasValue`
- `Xui.Core.dll`: `Xui.Core.Canvas.FontStyle.get_Normal`
- `System.Private.CoreLib.dll`: `System.Nullable<Xui.Core.Canvas.FontStyle>.GetValueOrDefault`
- `Xui.Core.dll`: `Xui.Core.Canvas.Font.set_FontStyle`
- `System.Private.CoreLib.dll`: `System.Nullable<Xui.Core.Canvas.FontStretch>.get_HasValue`
- `Xui.Core.dll`: `Xui.Core.Canvas.FontStretch..ctor`
- `System.Private.CoreLib.dll`: `System.Nullable<Xui.Core.Canvas.FontStretch>.GetValueOrDefault`
- `Xui.Core.dll`: `Xui.Core.Canvas.Font.set_FontStretch`
- `System.Private.CoreLib.dll`: `System.Nullable<System.Runtime.InteropServices.NFloat>.get_HasValue`
- `System.Private.CoreLib.dll`: `System.Nullable<System.Runtime.InteropServices.NFloat>.GetValueOrDefault`
- `Xui.Core.dll`: `Xui.Core.Canvas.Font.set_LineHeight`
- `Xui.Core.dll`: `Xui.Core.Canvas.ITextMeasureContext.SetFont`
- `TestApp.Render.exe`: `TestAppCanvas.CanvasContext.SetFont`
- `Xui.Core.dll`: `Xui.Core.Canvas.ITextDrawingContext.set_TextAlign`
- `TestApp.Render.exe`: `TestAppCanvas.CanvasContext.set_TextAlign`
- `Xui.Core.dll`: `Xui.Core.Canvas.ITextDrawingContext.set_TextBaseline`
- `TestApp.Render.exe`: `TestAppCanvas.CanvasContext.set_TextBaseline`
- `Xui.Core.dll`: `Xui.Core.Canvas.IPathBuilder.BeginPath`
- `TestApp.Render.exe`: `TestAppCanvas.CanvasContext.BeginPath`
- `Xui.Core.dll`: `Xui.Core.UI.View.Render`
- `Xui.Core.dll`: `Xui.Core.Canvas.IRectDrawingContext.FillRect`
- `TestApp.Render.exe`: `TestAppCanvas.CanvasContext.FillRect`
- `Xui.Core.dll`: `Xui.Core.Canvas.FontStyle..ctor`
- `Xui.Core.dll`: `Xui.Core.Canvas.Font.get_FontSize`
- `Xui.Core.dll`: `Xui.Core.Canvas.Font.get_FontWeight`
- `Xui.Core.dll`: `Xui.Core.Canvas.Font.get_FontStretch`
- `Xui.Core.dll`: `Xui.Core.Canvas.Font.get_LineHeight`
- `Xui.Core.dll`: `Xui.Core.Canvas.Font.get_FontFamily`
- `Xui.Core.dll`: `Xui.Core.Canvas.Font.get_FontStyle`
- `Xui.Core.dll`: `Xui.Core.Canvas.FontStyle.get_IsItalic`
- `Xui.Core.dll`: `Xui.Core.Canvas.FontStyle.get_IsOblique`
- `Xui.Core.dll`: `Xui.Core.Canvas.FontStyle.get_ObliqueAngle`

## WAT

```wat
(module
  (type (;0;) (array (mut i16)))
  (type (;1;) (func (param (ref null 0) i32 i32) (result (ref extern))))
  (type (;2;) (func (param externref f64 f64 f64 f64 externref)))
  (type (;3;) (struct (field i32) (field externref)))
  (type (;4;) (func (param eqref)))
  (type (;5;) (func (result eqref)))
  (type (;6;) (func (result eqref exnref)))
  (type (;7;) (func (result exnref)))
  (type (;8;) (func))
  (type (;9;) (struct (field i32) (field eqref) (field i32) (field (ref null 0)) (field i32) (field i32)))
  (type (;10;) (sub (struct (field (mut eqref)))))
  (type (;11;) (func (param eqref eqref) (result eqref)))
  (type (;12;) (sub 10 (struct (field (mut eqref)))))
  (type (;13;) (sub 10 (struct (field (mut eqref)) (field (mut eqref)) (field (mut i32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut eqref)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut eqref)) (field (mut i32)) (field (mut i32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut eqref)) (field (mut i32)) (field (mut i32)) (field (mut eqref)) (field (mut externref)) (field (mut externref)) (field (mut externref)) (field (mut externref)) (field (mut eqref)) (field (mut eqref)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)))))
  (type (;14;) (struct (field (mut f32)) (field (mut f32))))
  (type (;15;) (struct (field (mut i32))))
  (type (;16;) (struct (field (mut f32)) (field (mut f32))))
  (type (;17;) (struct (field (mut i32))))
  (type (;18;) (struct (field (mut i32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32))))
  (type (;19;) (struct (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32))))
  (type (;20;) (struct (field (mut eqref))))
  (type (;21;) (struct (field (mut i32))))
  (type (;22;) (struct (field (mut externref)) (field (mut externref)) (field (mut externref)) (field (mut eqref))))
  (type (;23;) (sub 10 (struct (field (mut eqref)) (field (mut eqref)) (field (mut eqref)) (field (mut i64)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut eqref)))))
  (type (;24;) (struct (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32))))
  (type (;25;) (struct (field (mut i32))))
  (type (;26;) (struct (field (mut i32))))
  (type (;27;) (struct (field (mut i32))))
  (type (;28;) (struct (field (mut i32))))
  (type (;29;) (struct (field (mut i32))))
  (type (;30;) (sub 10 (struct (field (mut eqref)) (field (mut eqref)) (field (mut eqref)) (field (mut i64)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut eqref)) (field (mut eqref)) (field (mut eqref)))))
  (type (;31;) (sub 10 (struct (field (mut eqref)) (field (mut eqref)))))
  (type (;32;) (sub 10 (struct (field (mut eqref)) (field (mut eqref)))))
  (type (;33;) (sub 13 (struct (field (mut eqref)) (field (mut eqref)) (field (mut i32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut eqref)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut eqref)) (field (mut i32)) (field (mut i32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut eqref)) (field (mut i32)) (field (mut i32)) (field (mut eqref)) (field (mut externref)) (field (mut externref)) (field (mut externref)) (field (mut externref)) (field (mut eqref)) (field (mut eqref)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)))))
  (type (;34;) (struct (field (mut f32)) (field (mut f32))))
  (type (;35;) (struct (field (mut i64)) (field (mut i64)) (field (mut eqref)) (field (mut eqref)) (field (mut eqref))))
  (type (;36;) (struct (field (mut i64))))
  (type (;37;) (struct (field (mut f32)) (field (mut f32)) (field (mut i32)) (field (mut i32))))
  (type (;38;) (struct (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32))))
  (type (;39;) (struct (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32))))
  (type (;40;) (struct (field (mut i32)) (field (mut f32)) (field (mut f32))))
  (type (;41;) (struct (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut i32))))
  (type (;42;) (struct (field (mut eqref)) (field (mut i32)) (field (mut i32))))
  (type (;43;) (struct (field (mut i32)) (field (mut f32))))
  (type (;44;) (struct (field (mut f32))))
  (type (;45;) (struct (field (mut i32)) (field (mut i32)) (field (mut i32)) (field (mut f32))))
  (type (;46;) (struct (field (mut i32)) (field (mut i32)) (field (mut f32))))
  (type (;47;) (struct (field (mut i32)) (field (mut f32))))
  (type (;48;) (struct (field (mut f32))))
  (type (;49;) (struct (field (mut i32)) (field (mut f32))))
  (type (;50;) (struct (field (mut externref)) (field (mut f32)) (field (mut i32)) (field (mut i32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32))))
  (type (;51;) (struct (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32))))
  (type (;52;) (sub 10 (struct (field (mut eqref)))))
  (type (;53;) (struct (field (mut eqref)) (field (mut eqref))))
  (type (;54;) (struct (field (mut i32)) (field (mut i32)) (field (mut eqref)) (field (mut eqref))))
  (type (;55;) (struct (field (mut i32)) (field (mut i32)) (field (mut externref))))
  (type (;56;) (array (mut i32)))
  (type (;57;) (array (mut eqref)))
  (type (;58;) (array (mut eqref)))
  (type (;59;) (func (result i32)))
  (type (;60;) (func (param eqref i32) (result eqref)))
  (type (;61;) (func (param eqref i32 eqref)))
  (type (;62;) (func (param eqref i32) (result externref)))
  (type (;63;) (func (param eqref i32 externref)))
  (type (;64;) (func (param eqref i32) (result f32)))
  (type (;65;) (func (param eqref i32 f32)))
  (type (;66;) (func (param eqref i32) (result i64)))
  (type (;67;) (func (param eqref i32 i64)))
  (type (;68;) (func (param eqref i32) (result i32)))
  (type (;69;) (func (param eqref i32 i32)))
  (type (;70;) (func (param eqref i32) (result i32)))
  (type (;71;) (func (param eqref i32 i32)))
  (type (;72;) (func (param eqref i32) (result i32)))
  (type (;73;) (func (param eqref i32 i32)))
  (type (;74;) (func (result i32)))
  (type (;75;) (func (param eqref)))
  (type (;76;) (func (param eqref)))
  (type (;77;) (func (param eqref f32 f32 i32)))
  (type (;78;) (func (param i32 i32) (result i32)))
  (type (;79;) (func (param eqref i32 eqref i32) (result i32)))
  (type (;80;) (func (param eqref f32 f32 f32 f32 i32)))
  (type (;81;) (func (param i32 i32) (result i32)))
  (type (;82;) (func (param eqref i32 i32)))
  (type (;83;) (func (param eqref i32 eqref i32 i32) (result i32)))
  (type (;84;) (func (result i32)))
  (type (;85;) (func (param eqref eqref)))
  (type (;86;) (func (param eqref f32 f32 f32 f32 i32)))
  (type (;87;) (func (param i32 i32) (result i32)))
  (type (;88;) (func))
  (type (;89;) (func))
  (type (;90;) (func))
  (type (;91;) (func (param eqref)))
  (type (;92;) (func (param eqref f32 f32 i32)))
  (type (;93;) (func (param eqref i32 i32 eqref eqref i32 i32)))
  (type (;94;) (func (param eqref i32 i32 i32 i32)))
  (type (;95;) (func (param eqref eqref eqref eqref i32 i32 i32)))
  (type (;96;) (func))
  (type (;97;) (func (result i32)))
  (type (;98;) (func (result i32)))
  (type (;99;) (func (result i32)))
  (type (;100;) (func (result i32)))
  (type (;101;) (func))
  (type (;102;) (func))
  (type (;103;) (func))
  (type (;104;) (func (param eqref i32) (result i32)))
  (type (;105;) (func (param eqref i32 i32) (result i32)))
  (type (;106;) (func (param eqref i32 i32) (result i32)))
  (type (;107;) (func (param eqref i32 i32) (result i32)))
  (type (;108;) (func (param eqref i32 i32 i32 i32 i32 i32 i32 i32)))
  (type (;109;) (func (param eqref eqref eqref eqref i32 i32 i32)))
  (type (;110;) (func))
  (type (;111;) (func (result f64)))
  (type (;112;) (func (result f64)))
  (type (;113;) (func))
  (type (;114;) (func))
  (type (;115;) (func))
  (type (;116;) (func))
  (type (;117;) (func))
  (type (;118;) (func))
  (type (;119;) (func))
  (type (;120;) (func (param eqref eqref i32 i32)))
  (type (;121;) (func))
  (type (;122;) (func (result i32)))
  (type (;123;) (func))
  (type (;124;) (func))
  (type (;125;) (func (result i32)))
  (type (;126;) (func (param i32 i32) (result i32)))
  (type (;127;) (func (param eqref eqref eqref i32 i32) (result i32)))
  (type (;128;) (func))
  (type (;129;) (func))
  (type (;130;) (func))
  (type (;131;) (func))
  (type (;132;) (func))
  (type (;133;) (func (param eqref i32 i32) (result i32)))
  (type (;134;) (func (param i32 i32 i32) (result i32)))
  (type (;135;) (func (param eqref i32) (result i32)))
  (type (;136;) (func (param eqref) (result i32)))
  (type (;137;) (func (param eqref i32) (result i32)))
  (type (;138;) (func (param eqref) (result i32)))
  (type (;139;) (func (param eqref i32) (result i32)))
  (type (;140;) (func (param i32 i32 i32) (result i32)))
  (type (;141;) (func (param eqref i32) (result eqref)))
  (type (;142;) (func (param eqref) (result f32)))
  (type (;143;) (func (param eqref) (result f32)))
  (type (;144;) (func (param eqref) (result f32)))
  (type (;145;) (func (param eqref) (result f32)))
  (type (;146;) (func (param i32 i32 i32) (result i32)))
  (type (;147;) (func (param eqref)))
  (type (;148;) (func (param eqref f32 f32 i32)))
  (type (;149;) (func (param eqref eqref eqref i32 i32) (result i32)))
  (type (;150;) (func))
  (type (;151;) (func))
  (type (;152;) (func))
  (type (;153;) (func))
  (type (;154;) (func))
  (type (;155;) (func))
  (type (;156;) (func))
  (type (;157;) (func))
  (type (;158;) (func))
  (type (;159;) (func (param eqref i32) (result i32)))
  (type (;160;) (func (param eqref i32 i32) (result i32)))
  (type (;161;) (func (param eqref i32 i32) (result i32)))
  (type (;162;) (func (param eqref i32) (result i32)))
  (type (;163;) (func (param eqref i32) (result i32)))
  (type (;164;) (func (param eqref i32 i32) (result i32)))
  (type (;165;) (func (param eqref i32) (result i32)))
  (type (;166;) (func (param eqref i32) (result i32)))
  (type (;167;) (func (param i32 i32 i32) (result i32)))
  (type (;168;) (func (param i32 i32) (result i32)))
  (type (;169;) (func))
  (type (;170;) (func))
  (type (;171;) (func))
  (type (;172;) (func (param eqref)))
  (type (;173;) (func))
  (type (;174;) (func (param eqref)))
  (type (;175;) (func (param eqref i32)))
  (type (;176;) (func (param eqref)))
  (type (;177;) (func (param eqref i32) (result i32)))
  (type (;178;) (func (param eqref) (result eqref)))
  (type (;179;) (func (param eqref i32 i32)))
  (type (;180;) (func (param eqref i32 i32) (result i32)))
  (type (;181;) (func (param eqref i32 i32) (result i32)))
  (type (;182;) (func))
  (type (;183;) (func))
  (type (;184;) (func))
  (type (;185;) (func (param eqref i32) (result eqref)))
  (type (;186;) (func (param eqref) (result i32)))
  (type (;187;) (func))
  (type (;188;) (func (param eqref)))
  (type (;189;) (func (param eqref f32 f32 f32 f32 i32)))
  (type (;190;) (func (param eqref) (result i32)))
  (type (;191;) (func (param i32 i32) (result i32)))
  (type (;192;) (func))
  (type (;193;) (func (param eqref i32 eqref i32) (result i32)))
  (type (;194;) (func))
  (type (;195;) (func))
  (type (;196;) (func (param eqref i32)))
  (type (;197;) (func (param i32 i32) (result i32)))
  (type (;198;) (func))
  (type (;199;) (func (param eqref i32 eqref)))
  (type (;200;) (func))
  (type (;201;) (func))
  (type (;202;) (func (param eqref i32) (result i32)))
  (type (;203;) (func (param eqref i32) (result f32)))
  (type (;204;) (func (param eqref i32) (result f32)))
  (type (;205;) (func (param eqref i32) (result f32)))
  (type (;206;) (func (param eqref i32) (result f32)))
  (type (;207;) (func (param eqref i32 i32) (result i32)))
  (type (;208;) (func (param eqref)))
  (type (;209;) (func (param eqref)))
  (type (;210;) (func (param eqref eqref)))
  (type (;211;) (func (param eqref eqref)))
  (type (;212;) (func))
  (type (;213;) (func))
  (type (;214;) (func (param eqref eqref)))
  (type (;215;) (func (param eqref)))
  (type (;216;) (func (param eqref)))
  (type (;217;) (func (param eqref eqref)))
  (type (;218;) (func))
  (type (;219;) (func (param eqref)))
  (type (;220;) (func (param i32 i32) (result i32)))
  (type (;221;) (func (param eqref f32 externref i32 i32 i32 i32 i32)))
  (type (;222;) (func (result i32)))
  (type (;223;) (func (result i32)))
  (type (;224;) (func (result i32)))
  (type (;225;) (func (result f64)))
  (type (;226;) (func (param eqref eqref eqref)))
  (type (;227;) (func))
  (type (;228;) (func (param eqref i32) (result eqref)))
  (type (;229;) (func (param eqref) (result i32)))
  (type (;230;) (func (param eqref f32)))
  (type (;231;) (func (param eqref f32)))
  (type (;232;) (func (param eqref i32)))
  (type (;233;) (func (param eqref i32)))
  (type (;234;) (func (param eqref i32)))
  (type (;235;) (func (param eqref i32)))
  (type (;236;) (func (param eqref f32)))
  (type (;237;) (func (param eqref f32)))
  (type (;238;) (func (param eqref f32)))
  (type (;239;) (func (param eqref f32)))
  (type (;240;) (func (param eqref f32)))
  (type (;241;) (func (param eqref f32)))
  (type (;242;) (func (param eqref i32)))
  (type (;243;) (func (param eqref i32)))
  (type (;244;) (func))
  (type (;245;) (func (param eqref i32 i32)))
  (type (;246;) (func (param eqref i32)))
  (type (;247;) (func (param eqref i32)))
  (type (;248;) (func (param eqref i32)))
  (type (;249;) (func (param eqref i32)))
  (type (;250;) (func (param eqref externref i32)))
  (type (;251;) (func (param eqref f32 i32)))
  (type (;252;) (func (param eqref i32) (result i32)))
  (type (;253;) (func (param eqref f32 i32)))
  (type (;254;) (func (param eqref i32 i32) (result i32)))
  (type (;255;) (func (param eqref i32 i32)))
  (type (;256;) (func (param eqref i32) (result i32)))
  (type (;257;) (func (param i32) (result i32)))
  (type (;258;) (func (param eqref i32 i32) (result i32)))
  (type (;259;) (func (param eqref i32 i32)))
  (type (;260;) (func (param eqref i32) (result i32)))
  (type (;261;) (func (param eqref f32 i32)))
  (type (;262;) (func (param eqref i32 i32) (result i32)))
  (type (;263;) (func (param eqref i32 i32)))
  (type (;264;) (func (param eqref i32) (result i32)))
  (type (;265;) (func (param eqref i32) (result f32)))
  (type (;266;) (func (param eqref f32 i32)))
  (type (;267;) (func (param eqref i32)))
  (type (;268;) (func (param eqref i32)))
  (type (;269;) (func (param eqref i32)))
  (type (;270;) (func (param eqref i32)))
  (type (;271;) (func (param eqref i32)))
  (type (;272;) (func (param eqref i32)))
  (type (;273;) (func (param eqref)))
  (type (;274;) (func (param eqref)))
  (type (;275;) (func (param eqref eqref i32)))
  (type (;276;) (func (param eqref i32)))
  (type (;277;) (func (param eqref i32)))
  (type (;278;) (func (param eqref i32 i32 f32 i32)))
  (type (;279;) (func (param eqref i32) (result f32)))
  (type (;280;) (func (param eqref i32 i32) (result i32)))
  (type (;281;) (func (param eqref i32 i32) (result i32)))
  (type (;282;) (func (param eqref i32) (result f32)))
  (type (;283;) (func (param eqref i32) (result externref)))
  (type (;284;) (func (param eqref i32 i32) (result i32)))
  (type (;285;) (func (param eqref i32) (result i32)))
  (type (;286;) (func (param eqref i32) (result i32)))
  (type (;287;) (func (param eqref i32) (result f32)))
  (import "canvas" "Command" (func (;0;) (type 2) (param externref f64 f64 f64 f64 externref)))
  (import "wasm:js-string" "fromCharCodeArray" (func (;1;) (type 1) (param (ref null 0) i32 i32) (result (ref extern))))
  (table (;0;) 342 342 funcref)
  (table (;1;) 65536 65536 eqref)
  (table (;2;) 65536 65536 externref)
  (memory (;0;) 16 32768)
  (tag (;0;) (type 4) (param eqref))
  (global (;0;) (mut externref) ref.null extern)
  (global (;1;) (mut externref) ref.null extern)
  (global (;2;) (mut externref) ref.null extern)
  (global (;3;) (mut externref) ref.null extern)
  (global (;4;) (mut externref) ref.null extern)
  (global (;5;) (mut externref) ref.null extern)
  (global (;6;) (mut externref) ref.null extern)
  (global (;7;) (mut externref) ref.null extern)
  (global (;8;) (mut externref) ref.null extern)
  (global (;9;) (mut externref) ref.null extern)
  (global (;10;) (mut externref) ref.null extern)
  (global (;11;) (mut externref) ref.null extern)
  (global (;12;) (mut externref) ref.null extern)
  (global (;13;) (mut externref) ref.null extern)
  (global (;14;) (mut externref) ref.null extern)
  (global (;15;) (mut externref) ref.null extern)
  (global (;16;) (mut externref) ref.null extern)
  (global (;17;) (mut externref) ref.null extern)
  (global (;18;) (mut externref) ref.null extern)
  (global (;19;) (mut externref) ref.null extern)
  (global (;20;) (mut externref) ref.null extern)
  (global (;21;) (mut externref) ref.null extern)
  (global (;22;) (mut externref) ref.null extern)
  (global (;23;) (mut externref) ref.null extern)
  (global (;24;) (mut externref) ref.null extern)
  (global (;25;) (mut externref) ref.null extern)
  (global (;26;) (mut externref) ref.null extern)
  (global (;27;) (mut i32) i32.const 16)
  (global (;28;) (mut i32) i32.const 0)
  (global (;29;) eqref i32.const 1 ref.null eq i32.const 0 i32.const 84 i32.const 101 i32.const 115 i32.const 116 i32.const 65 i32.const 112 i32.const 112 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 67 i32.const 111 i32.const 110 i32.const 116 i32.const 101 i32.const 120 i32.const 116 array.new_fixed 0 27 i32.const 52 i32.const 1 struct.new 9)
  (global (;30;) eqref i32.const 2 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 67 i32.const 111 i32.const 110 i32.const 116 i32.const 101 i32.const 120 i32.const 116 array.new_fixed 0 24 i32.const 0 i32.const 0 struct.new 9)
  (global (;31;) eqref i32.const 3 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 77 i32.const 101 i32.const 97 i32.const 115 i32.const 117 i32.const 114 i32.const 101 i32.const 67 i32.const 111 i32.const 110 i32.const 116 i32.const 101 i32.const 120 i32.const 116 array.new_fixed 0 31 i32.const 0 i32.const 0 struct.new 9)
  (global (;32;) eqref i32.const 4 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 84 i32.const 101 i32.const 120 i32.const 116 i32.const 77 i32.const 101 i32.const 97 i32.const 115 i32.const 117 i32.const 114 i32.const 101 i32.const 67 i32.const 111 i32.const 110 i32.const 116 i32.const 101 i32.const 120 i32.const 116 array.new_fixed 0 35 i32.const 0 i32.const 0 struct.new 9)
  (global (;33;) eqref i32.const 5 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 83 i32.const 116 i32.const 97 i32.const 116 i32.const 101 i32.const 67 i32.const 111 i32.const 110 i32.const 116 i32.const 101 i32.const 120 i32.const 116 array.new_fixed 0 29 i32.const 0 i32.const 0 struct.new 9)
  (global (;34;) eqref i32.const 6 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 83 i32.const 121 i32.const 109 i32.const 98 i32.const 111 i32.const 108 i32.const 67 i32.const 111 i32.const 110 i32.const 116 i32.const 101 i32.const 120 i32.const 116 array.new_fixed 0 30 i32.const 0 i32.const 0 struct.new 9)
  (global (;35;) eqref i32.const 7 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 67 i32.const 111 i32.const 109 i32.const 112 i32.const 111 i32.const 115 i32.const 105 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 67 i32.const 111 i32.const 110 i32.const 116 i32.const 101 i32.const 120 i32.const 116 array.new_fixed 0 35 i32.const 0 i32.const 0 struct.new 9)
  (global (;36;) eqref i32.const 8 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 70 i32.const 105 i32.const 108 i32.const 116 i32.const 101 i32.const 114 i32.const 67 i32.const 111 i32.const 110 i32.const 116 i32.const 101 i32.const 120 i32.const 116 array.new_fixed 0 30 i32.const 0 i32.const 0 struct.new 9)
  (global (;37;) eqref i32.const 9 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 80 i32.const 101 i32.const 110 i32.const 67 i32.const 111 i32.const 110 i32.const 116 i32.const 101 i32.const 120 i32.const 116 array.new_fixed 0 27 i32.const 0 i32.const 0 struct.new 9)
  (global (;38;) eqref i32.const 10 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 80 i32.const 97 i32.const 116 i32.const 104 i32.const 68 i32.const 114 i32.const 97 i32.const 119 i32.const 105 i32.const 110 i32.const 103 i32.const 67 i32.const 111 i32.const 110 i32.const 116 i32.const 101 i32.const 120 i32.const 116 array.new_fixed 0 35 i32.const 0 i32.const 0 struct.new 9)
  (global (;39;) eqref i32.const 11 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 80 i32.const 97 i32.const 116 i32.const 104 i32.const 66 i32.const 117 i32.const 105 i32.const 108 i32.const 100 i32.const 101 i32.const 114 array.new_fixed 0 28 i32.const 0 i32.const 0 struct.new 9)
  (global (;40;) eqref i32.const 12 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 71 i32.const 108 i32.const 121 i32.const 112 i32.const 104 i32.const 80 i32.const 97 i32.const 116 i32.const 104 i32.const 66 i32.const 117 i32.const 105 i32.const 108 i32.const 100 i32.const 101 i32.const 114 array.new_fixed 0 33 i32.const 0 i32.const 0 struct.new 9)
  (global (;41;) eqref i32.const 13 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 80 i32.const 97 i32.const 116 i32.const 104 i32.const 68 i32.const 114 i32.const 97 i32.const 119 i32.const 105 i32.const 110 i32.const 103 array.new_fixed 0 28 i32.const 0 i32.const 0 struct.new 9)
  (global (;42;) eqref i32.const 14 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 80 i32.const 97 i32.const 116 i32.const 104 i32.const 67 i32.const 108 i32.const 105 i32.const 112 i32.const 112 i32.const 105 i32.const 110 i32.const 103 array.new_fixed 0 29 i32.const 0 i32.const 0 struct.new 9)
  (global (;43;) eqref i32.const 15 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 82 i32.const 101 i32.const 99 i32.const 116 i32.const 68 i32.const 114 i32.const 97 i32.const 119 i32.const 105 i32.const 110 i32.const 103 i32.const 67 i32.const 111 i32.const 110 i32.const 116 i32.const 101 i32.const 120 i32.const 116 array.new_fixed 0 35 i32.const 0 i32.const 0 struct.new 9)
  (global (;44;) eqref i32.const 16 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 84 i32.const 101 i32.const 120 i32.const 116 i32.const 68 i32.const 114 i32.const 97 i32.const 119 i32.const 105 i32.const 110 i32.const 103 i32.const 67 i32.const 111 i32.const 110 i32.const 116 i32.const 101 i32.const 120 i32.const 116 array.new_fixed 0 35 i32.const 0 i32.const 0 struct.new 9)
  (global (;45;) eqref i32.const 17 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 73 i32.const 109 i32.const 97 i32.const 103 i32.const 101 i32.const 68 i32.const 114 i32.const 97 i32.const 119 i32.const 105 i32.const 110 i32.const 103 i32.const 67 i32.const 111 i32.const 110 i32.const 116 i32.const 101 i32.const 120 i32.const 116 array.new_fixed 0 36 i32.const 0 i32.const 0 struct.new 9)
  (global (;46;) eqref i32.const 18 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 84 i32.const 114 i32.const 97 i32.const 110 i32.const 115 i32.const 102 i32.const 111 i32.const 114 i32.const 109 i32.const 67 i32.const 111 i32.const 110 i32.const 116 i32.const 101 i32.const 120 i32.const 116 array.new_fixed 0 33 i32.const 0 i32.const 0 struct.new 9)
  (global (;47;) eqref i32.const 19 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 73 i32.const 68 i32.const 105 i32.const 115 i32.const 112 i32.const 111 i32.const 115 i32.const 97 i32.const 98 i32.const 108 i32.const 101 array.new_fixed 0 18 i32.const 0 i32.const 0 struct.new 9)
  (global (;48;) eqref i32.const 20 ref.null eq i32.const 68 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 85 i32.const 73 i32.const 46 i32.const 86 i32.const 105 i32.const 101 i32.const 119 array.new_fixed 0 16 i32.const 40 i32.const 55 struct.new 9)
  (global (;49;) eqref i32.const 21 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 73 i32.const 83 i32.const 101 i32.const 114 i32.const 118 i32.const 105 i32.const 99 i32.const 101 i32.const 80 i32.const 114 i32.const 111 i32.const 118 i32.const 105 i32.const 100 i32.const 101 i32.const 114 array.new_fixed 0 23 i32.const 0 i32.const 0 struct.new 9)
  (global (;50;) eqref i32.const 22 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 85 i32.const 73 i32.const 46 i32.const 76 i32.const 97 i32.const 121 i32.const 101 i32.const 114 i32.const 46 i32.const 73 i32.const 76 i32.const 97 i32.const 121 i32.const 101 i32.const 114 i32.const 72 i32.const 111 i32.const 115 i32.const 116 array.new_fixed 0 28 i32.const 0 i32.const 0 struct.new 9)
  (global (;51;) eqref i32.const 23 global.get 48 i32.const 268 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 65 i32.const 112 i32.const 112 i32.const 115 i32.const 46 i32.const 84 i32.const 101 i32.const 115 i32.const 116 i32.const 65 i32.const 112 i32.const 112 i32.const 46 i32.const 80 i32.const 97 i32.const 103 i32.const 101 i32.const 115 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 84 i32.const 101 i32.const 115 i32.const 116 i32.const 115 i32.const 46 i32.const 70 i32.const 105 i32.const 108 i32.const 108 i32.const 82 i32.const 101 i32.const 99 i32.const 116 i32.const 84 i32.const 101 i32.const 115 i32.const 116 array.new_fixed 0 48 i32.const 41 i32.const 55 struct.new 9)
  (global (;52;) eqref i32.const 24 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 65 i32.const 112 i32.const 112 i32.const 115 i32.const 46 i32.const 84 i32.const 101 i32.const 115 i32.const 116 i32.const 65 i32.const 112 i32.const 112 i32.const 46 i32.const 80 i32.const 97 i32.const 103 i32.const 101 i32.const 115 i32.const 46 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 46 i32.const 73 i32.const 67 i32.const 97 i32.const 110 i32.const 118 i32.const 97 i32.const 115 i32.const 68 i32.const 114 i32.const 97 i32.const 119 i32.const 105 i32.const 110 i32.const 103 i32.const 69 i32.const 120 i32.const 97 i32.const 109 i32.const 112 i32.const 108 i32.const 101 array.new_fixed 0 51 i32.const 0 i32.const 0 struct.new 9)
  (global (;53;) eqref i32.const 25 ref.null eq i32.const 0 i32.const 88 i32.const 117 i32.const 105 i32.const 46 i32.const 67 i32.const 111 i32.const 114 i32.const 101 i32.const 46 i32.const 68 i32.const 101 i32.const 98 i32.const 117 i32.const 103 i32.const 46 i32.const 73 i32.const 86 i32.const 105 i32.const 101 i32.const 119 i32.const 73 i32.const 110 i32.const 115 i32.const 116 i32.const 114 i32.const 117 i32.const 109 i32.const 101 i32.const 110 i32.const 116 i32.const 115 i32.const 83 i32.const 105 i32.const 110 i32.const 107 array.new_fixed 0 35 i32.const 0 i32.const 0 struct.new 9)
  (global (;54;) eqref i32.const 26 ref.null eq i32.const 124 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 72 i32.const 97 i32.const 115 i32.const 104 i32.const 83 i32.const 101 i32.const 116 i32.const 60 i32.const 83 i32.const 116 i32.const 114 i32.const 105 i32.const 110 i32.const 103 i32.const 62 array.new_fixed 0 42 i32.const 22 i32.const 9 struct.new 9)
  (global (;55;) eqref i32.const 27 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 60 i32.const 83 i32.const 116 i32.const 114 i32.const 105 i32.const 110 i32.const 103 i32.const 62 array.new_fixed 0 46 i32.const 0 i32.const 0 struct.new 9)
  (global (;56;) eqref i32.const 28 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 69 i32.const 110 i32.const 117 i32.const 109 i32.const 101 i32.const 114 i32.const 97 i32.const 98 i32.const 108 i32.const 101 i32.const 60 i32.const 83 i32.const 116 i32.const 114 i32.const 105 i32.const 110 i32.const 103 i32.const 62 array.new_fixed 0 46 i32.const 0 i32.const 0 struct.new 9)
  (global (;57;) eqref i32.const 29 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 73 i32.const 69 i32.const 110 i32.const 117 i32.const 109 i32.const 101 i32.const 114 i32.const 97 i32.const 98 i32.const 108 i32.const 101 array.new_fixed 0 30 i32.const 0 i32.const 0 struct.new 9)
  (global (;58;) eqref i32.const 30 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 83 i32.const 101 i32.const 116 i32.const 60 i32.const 83 i32.const 116 i32.const 114 i32.const 105 i32.const 110 i32.const 103 i32.const 62 array.new_fixed 0 39 i32.const 0 i32.const 0 struct.new 9)
  (global (;59;) eqref i32.const 31 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 82 i32.const 101 i32.const 97 i32.const 100 i32.const 79 i32.const 110 i32.const 108 i32.const 121 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 60 i32.const 83 i32.const 116 i32.const 114 i32.const 105 i32.const 110 i32.const 103 i32.const 62 array.new_fixed 0 54 i32.const 0 i32.const 0 struct.new 9)
  (global (;60;) eqref i32.const 32 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 82 i32.const 101 i32.const 97 i32.const 100 i32.const 79 i32.const 110 i32.const 108 i32.const 121 i32.const 83 i32.const 101 i32.const 116 i32.const 60 i32.const 83 i32.const 116 i32.const 114 i32.const 105 i32.const 110 i32.const 103 i32.const 62 array.new_fixed 0 47 i32.const 0 i32.const 0 struct.new 9)
  (global (;61;) eqref i32.const 33 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 82 i32.const 117 i32.const 110 i32.const 116 i32.const 105 i32.const 109 i32.const 101 i32.const 46 i32.const 83 i32.const 101 i32.const 114 i32.const 105 i32.const 97 i32.const 108 i32.const 105 i32.const 122 i32.const 97 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 46 i32.const 73 i32.const 83 i32.const 101 i32.const 114 i32.const 105 i32.const 97 i32.const 108 i32.const 105 i32.const 122 i32.const 97 i32.const 98 i32.const 108 i32.const 101 array.new_fixed 0 42 i32.const 0 i32.const 0 struct.new 9)
  (global (;62;) eqref i32.const 34 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 82 i32.const 117 i32.const 110 i32.const 116 i32.const 105 i32.const 109 i32.const 101 i32.const 46 i32.const 83 i32.const 101 i32.const 114 i32.const 105 i32.const 97 i32.const 108 i32.const 105 i32.const 122 i32.const 97 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 46 i32.const 73 i32.const 68 i32.const 101 i32.const 115 i32.const 101 i32.const 114 i32.const 105 i32.const 97 i32.const 108 i32.const 105 i32.const 122 i32.const 97 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 67 i32.const 97 i32.const 108 i32.const 108 i32.const 98 i32.const 97 i32.const 99 i32.const 107 array.new_fixed 0 53 i32.const 0 i32.const 0 struct.new 9)
  (global (;63;) eqref i32.const 35 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 69 i32.const 113 i32.const 117 i32.const 97 i32.const 108 i32.const 105 i32.const 116 i32.const 121 i32.const 67 i32.const 111 i32.const 109 i32.const 112 i32.const 97 i32.const 114 i32.const 101 i32.const 114 i32.const 60 i32.const 83 i32.const 116 i32.const 114 i32.const 105 i32.const 110 i32.const 103 i32.const 62 array.new_fixed 0 52 i32.const 0 i32.const 0 struct.new 9)
  (global (;64;) eqref i32.const 36 ref.null eq i32.const 162 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 68 i32.const 105 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 97 i32.const 114 i32.const 121 i32.const 60 i32.const 69 i32.const 120 i32.const 116 i32.const 114 i32.const 97 i32.const 44 i32.const 32 i32.const 79 i32.const 98 i32.const 106 i32.const 101 i32.const 99 i32.const 116 i32.const 62 array.new_fixed 0 52 i32.const 34 i32.const 11 struct.new 9)
  (global (;65;) eqref i32.const 37 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 68 i32.const 105 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 97 i32.const 114 i32.const 121 i32.const 60 i32.const 69 i32.const 120 i32.const 116 i32.const 114 i32.const 97 i32.const 44 i32.const 32 i32.const 79 i32.const 98 i32.const 106 i32.const 101 i32.const 99 i32.const 116 i32.const 62 array.new_fixed 0 53 i32.const 0 i32.const 0 struct.new 9)
  (global (;66;) eqref i32.const 38 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 60 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 75 i32.const 101 i32.const 121 i32.const 86 i32.const 97 i32.const 108 i32.const 117 i32.const 101 i32.const 80 i32.const 97 i32.const 105 i32.const 114 i32.const 60 i32.const 69 i32.const 120 i32.const 116 i32.const 114 i32.const 97 i32.const 44 i32.const 32 i32.const 79 i32.const 98 i32.const 106 i32.const 101 i32.const 99 i32.const 116 i32.const 62 i32.const 62 array.new_fixed 0 94 i32.const 0 i32.const 0 struct.new 9)
  (global (;67;) eqref i32.const 39 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 69 i32.const 110 i32.const 117 i32.const 109 i32.const 101 i32.const 114 i32.const 97 i32.const 98 i32.const 108 i32.const 101 i32.const 60 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 75 i32.const 101 i32.const 121 i32.const 86 i32.const 97 i32.const 108 i32.const 117 i32.const 101 i32.const 80 i32.const 97 i32.const 105 i32.const 114 i32.const 60 i32.const 69 i32.const 120 i32.const 116 i32.const 114 i32.const 97 i32.const 44 i32.const 32 i32.const 79 i32.const 98 i32.const 106 i32.const 101 i32.const 99 i32.const 116 i32.const 62 i32.const 62 array.new_fixed 0 94 i32.const 0 i32.const 0 struct.new 9)
  (global (;68;) eqref i32.const 40 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 73 i32.const 68 i32.const 105 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 97 i32.const 114 i32.const 121 array.new_fixed 0 30 i32.const 0 i32.const 0 struct.new 9)
  (global (;69;) eqref i32.const 41 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 73 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 array.new_fixed 0 30 i32.const 0 i32.const 0 struct.new 9)
  (global (;70;) eqref i32.const 42 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 82 i32.const 101 i32.const 97 i32.const 100 i32.const 79 i32.const 110 i32.const 108 i32.const 121 i32.const 68 i32.const 105 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 97 i32.const 114 i32.const 121 i32.const 60 i32.const 69 i32.const 120 i32.const 116 i32.const 114 i32.const 97 i32.const 44 i32.const 32 i32.const 79 i32.const 98 i32.const 106 i32.const 101 i32.const 99 i32.const 116 i32.const 62 array.new_fixed 0 61 i32.const 0 i32.const 0 struct.new 9)
  (global (;71;) eqref i32.const 43 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 82 i32.const 101 i32.const 97 i32.const 100 i32.const 79 i32.const 110 i32.const 108 i32.const 121 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 60 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 75 i32.const 101 i32.const 121 i32.const 86 i32.const 97 i32.const 108 i32.const 117 i32.const 101 i32.const 80 i32.const 97 i32.const 105 i32.const 114 i32.const 60 i32.const 69 i32.const 120 i32.const 116 i32.const 114 i32.const 97 i32.const 44 i32.const 32 i32.const 79 i32.const 98 i32.const 106 i32.const 101 i32.const 99 i32.const 116 i32.const 62 i32.const 62 array.new_fixed 0 102 i32.const 0 i32.const 0 struct.new 9)
  (global (;72;) eqref i32.const 44 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 69 i32.const 113 i32.const 117 i32.const 97 i32.const 108 i32.const 105 i32.const 116 i32.const 121 i32.const 67 i32.const 111 i32.const 109 i32.const 112 i32.const 97 i32.const 114 i32.const 101 i32.const 114 i32.const 60 i32.const 69 i32.const 120 i32.const 116 i32.const 114 i32.const 97 i32.const 62 array.new_fixed 0 51 i32.const 0 i32.const 0 struct.new 9)
  (global (;73;) eqref i32.const 45 ref.null eq i32.const 212 i32.const 75 i32.const 101 i32.const 121 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 60 i32.const 69 i32.const 120 i32.const 116 i32.const 114 i32.const 97 i32.const 44 i32.const 32 i32.const 79 i32.const 98 i32.const 106 i32.const 101 i32.const 99 i32.const 116 i32.const 62 array.new_fixed 0 28 i32.const 12 i32.const 2 struct.new 9)
  (global (;74;) eqref i32.const 46 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 60 i32.const 69 i32.const 120 i32.const 116 i32.const 114 i32.const 97 i32.const 62 array.new_fixed 0 45 i32.const 0 i32.const 0 struct.new 9)
  (global (;75;) eqref i32.const 47 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 69 i32.const 110 i32.const 117 i32.const 109 i32.const 101 i32.const 114 i32.const 97 i32.const 98 i32.const 108 i32.const 101 i32.const 60 i32.const 69 i32.const 120 i32.const 116 i32.const 114 i32.const 97 i32.const 62 array.new_fixed 0 45 i32.const 0 i32.const 0 struct.new 9)
  (global (;76;) eqref i32.const 48 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 82 i32.const 101 i32.const 97 i32.const 100 i32.const 79 i32.const 110 i32.const 108 i32.const 121 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 60 i32.const 69 i32.const 120 i32.const 116 i32.const 114 i32.const 97 i32.const 62 array.new_fixed 0 53 i32.const 0 i32.const 0 struct.new 9)
  (global (;77;) eqref i32.const 49 ref.null eq i32.const 240 i32.const 86 i32.const 97 i32.const 108 i32.const 117 i32.const 101 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 60 i32.const 69 i32.const 120 i32.const 116 i32.const 114 i32.const 97 i32.const 44 i32.const 32 i32.const 79 i32.const 98 i32.const 106 i32.const 101 i32.const 99 i32.const 116 i32.const 62 array.new_fixed 0 30 i32.const 12 i32.const 2 struct.new 9)
  (global (;78;) eqref i32.const 50 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 60 i32.const 79 i32.const 98 i32.const 106 i32.const 101 i32.const 99 i32.const 116 i32.const 62 array.new_fixed 0 46 i32.const 0 i32.const 0 struct.new 9)
  (global (;79;) eqref i32.const 51 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 69 i32.const 110 i32.const 117 i32.const 109 i32.const 101 i32.const 114 i32.const 97 i32.const 98 i32.const 108 i32.const 101 i32.const 60 i32.const 79 i32.const 98 i32.const 106 i32.const 101 i32.const 99 i32.const 116 i32.const 62 array.new_fixed 0 46 i32.const 0 i32.const 0 struct.new 9)
  (global (;80;) eqref i32.const 52 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 115 i32.const 46 i32.const 71 i32.const 101 i32.const 110 i32.const 101 i32.const 114 i32.const 105 i32.const 99 i32.const 46 i32.const 73 i32.const 82 i32.const 101 i32.const 97 i32.const 100 i32.const 79 i32.const 110 i32.const 108 i32.const 121 i32.const 67 i32.const 111 i32.const 108 i32.const 108 i32.const 101 i32.const 99 i32.const 116 i32.const 105 i32.const 111 i32.const 110 i32.const 60 i32.const 79 i32.const 98 i32.const 106 i32.const 101 i32.const 99 i32.const 116 i32.const 62 array.new_fixed 0 54 i32.const 0 i32.const 0 struct.new 9)
  (global (;81;) eqref i32.const 53 ref.null eq i32.const 325 i32.const 69 i32.const 120 i32.const 116 i32.const 114 i32.const 97 array.new_fixed 0 5 i32.const 1 i32.const 1 struct.new 9)
  (export "main" (func 2))
  (elem (;0;) (i32.const 16) func 80)
  (elem (;1;) (i32.const 17) func 84)
  (elem (;2;) (i32.const 18) func 91)
  (elem (;3;) (i32.const 19) func 92)
  (elem (;4;) (i32.const 20) func 93)
  (elem (;5;) (i32.const 21) func 94)
  (elem (;6;) (i32.const 22) func 95)
  (elem (;7;) (i32.const 23) func 96)
  (elem (;8;) (i32.const 24) func 119)
  (elem (;9;) (i32.const 25) func 120)
  (elem (;10;) (i32.const 26) func 97)
  (elem (;11;) (i32.const 27) func 99)
  (elem (;12;) (i32.const 28) func 100)
  (elem (;13;) (i32.const 29) func 123)
  (elem (;14;) (i32.const 30) func 118)
  (elem (;15;) (i32.const 31) func 121)
  (elem (;16;) (i32.const 0) func 80)
  (elem (;17;) (i32.const 1) func 84)
  (elem (;18;) (i32.const 2) func 91)
  (elem (;19;) (i32.const 3) func 92)
  (elem (;20;) (i32.const 4) func 93)
  (elem (;21;) (i32.const 5) func 94)
  (elem (;22;) (i32.const 6) func 95)
  (elem (;23;) (i32.const 7) func 96)
  (elem (;24;) (i32.const 8) func 97)
  (elem (;25;) (i32.const 9) func 99)
  (elem (;26;) (i32.const 10) func 100)
  (elem (;27;) (i32.const 11) func 118)
  (elem (;28;) (i32.const 12) func 119)
  (elem (;29;) (i32.const 13) func 120)
  (elem (;30;) (i32.const 14) func 121)
  (elem (;31;) (i32.const 15) func 123)
  (elem (;32;) (i32.const 91) func 90)
  (elem (;33;) (i32.const 92) func 89)
  (elem (;34;) (i32.const 99) func 70)
  (elem (;35;) (i32.const 100) func 73)
  (elem (;36;) (i32.const 101) func 82)
  (elem (;37;) (i32.const 102) func 81)
  (elem (;38;) (i32.const 110) func 56)
  (elem (;39;) (i32.const 113) func 85)
  (elem (;40;) (i32.const 117) func 74)
  (elem (;41;) (i32.const 120) func 79)
  (elem (;42;) (i32.const 122) func 46)
  (elem (;43;) (i32.const 291) func 90)
  (elem (;44;) (i32.const 292) func 89)
  (elem (;45;) (i32.const 299) func 70)
  (elem (;46;) (i32.const 300) func 73)
  (elem (;47;) (i32.const 301) func 83)
  (elem (;48;) (i32.const 302) func 81)
  (elem (;49;) (i32.const 310) func 56)
  (elem (;50;) (i32.const 313) func 85)
  (elem (;51;) (i32.const 317) func 74)
  (elem (;52;) (i32.const 320) func 79)
  (elem (;53;) (i32.const 322) func 46)
  (func (;2;) (type 74) (result i32)
    (local eqref eqref i32 eqref i32 eqref i32 f32 eqref eqref eqref f32 f32 i32 eqref i32 eqref eqref f32 f32 f32 f32 i32 eqref i32 i32 eqref i32 eqref i32 eqref eqref eqref f32 f32 f32 f32 i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 43
    i32.const 1048448
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 43
    i32.const 128
    i32.add
    global.set 27
    local.get 43
    i32.const 0
    i32.add
    local.set 2
    local.get 2
    i32.const 0
    i32.const 8
    memory.fill
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        struct.new_default 12
        local.set 8
        local.get 8
        ref.cast (ref 12)
        global.get 29
        struct.set 12 0
        local.get 8
        call 3
        local.get 8
        local.set 0
        struct.new_default 33
        local.set 9
        local.get 9
        ref.cast (ref 33)
        global.get 51
        struct.set 33 0
        local.get 9
        call 4
        local.get 9
        local.set 1
        local.get 1
        i32.const 400
        f32.convert_i32_s
        f64.promote_f32
        i32.const 240
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 12
        f32.demote_f64
        local.set 11
        local.get 43
        i32.const 8
        i32.add
        i32.const 0
        i32.const 8
        memory.fill
        ref.null eq
        local.set 10
        local.get 43
        i32.const 8
        i32.add
        local.set 13
        local.get 10
        local.get 11
        local.get 12
        local.get 13
        call 5
        local.get 43
        i32.const 8
        i32.add
        local.get 43
        i32.const 16
        i32.add
        call 6
        local.get 0
        local.set 16
        local.set 15
        local.set 14
        local.get 14
        local.tee 49
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 49
        ref.cast (ref 13)
        local.get 15
        local.get 16
        local.get 43
        i32.const 24
        i32.add
        call 7
        local.set 44
        local.get 2
        local.get 44
        i32.const 8
        memory.copy
        local.get 1
        i32.const 20
        f32.convert_i32_s
        f64.promote_f32
        i32.const 30
        f32.convert_i32_s
        f64.promote_f32
        i32.const 400
        f32.convert_i32_s
        f64.promote_f32
        i32.const 240
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 21
        f32.demote_f64
        local.set 20
        f32.demote_f64
        local.set 19
        f32.demote_f64
        local.set 18
        local.get 43
        i32.const 32
        i32.add
        i32.const 0
        i32.const 16
        memory.fill
        ref.null eq
        local.set 17
        local.get 43
        i32.const 32
        i32.add
        local.set 22
        local.get 17
        local.get 18
        local.get 19
        local.get 20
        local.get 21
        local.get 22
        call 8
        local.get 43
        i32.const 32
        i32.add
        local.get 43
        i32.const 48
        i32.add
        call 9
        local.get 0
        local.get 2
        local.set 44
        local.get 43
        i32.const 64
        i32.add
        local.get 44
        i32.const 8
        memory.copy
        local.get 43
        i32.const 64
        i32.add
        local.set 24
        local.get 43
        i32.const 72
        i32.add
        i32.const 0
        i32.const 12
        memory.fill
        ref.null eq
        local.set 23
        local.get 43
        i32.const 72
        i32.add
        local.set 25
        local.get 23
        local.get 24
        local.get 25
        call 10
        local.get 43
        i32.const 72
        i32.add
        local.set 29
        local.set 28
        local.set 27
        local.set 26
        local.get 26
        local.tee 49
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 49
        ref.cast (ref 13)
        local.get 27
        local.get 28
        local.get 29
        local.get 43
        i32.const 84
        i32.add
        call 11
        local.get 1
        local.get 0
        local.set 31
        local.set 30
        local.get 30
        local.tee 49
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 49
        ref.cast (ref 13)
        local.get 31
        call 12
        i32.const 20
        f32.convert_i32_s
        f64.promote_f32
        i32.const 30
        f32.convert_i32_s
        f64.promote_f32
        i32.const 400
        f32.convert_i32_s
        f64.promote_f32
        i32.const 240
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 36
        f32.demote_f64
        local.set 35
        f32.demote_f64
        local.set 34
        f32.demote_f64
        local.set 33
        local.get 43
        i32.const 100
        i32.add
        i32.const 0
        i32.const 16
        memory.fill
        ref.null eq
        local.set 32
        local.get 43
        i32.const 100
        i32.add
        local.set 37
        local.get 32
        local.get 33
        local.get 34
        local.get 35
        local.get 36
        local.get 37
        call 13
        local.get 43
        i32.const 100
        i32.add
        call 14
        i32.const 0
        i32.eq
        local.get 43
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 38
    local.get 43
    global.set 27
    local.get 38
    throw_ref
  )
  (func (;3;) (type 75) (param eqref)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    drop
    return
  )
  (func (;4;) (type 76) (param eqref)
    (local eqref eqref eqref i32 i64 f64 externref)
    local.get 0
    local.set 1
    local.get 1
    call 15
    return
  )
  (func (;5;) (type 77) (param eqref f32 f32 i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 13
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 13
    i32.const 0
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 3
        local.get 1
        f64.promote_f32
        local.set 22
        local.set 5
        local.set 4
        local.get 4
        ref.is_null
        local.get 5
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 4
        local.get 4
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 5
          i32.const 0
          i32.add
        else
          local.get 5
          i32.const 0
          i32.add
        end
        local.get 22
        f32.demote_f64
        call 139
        local.get 0
        local.get 3
        local.get 2
        f64.promote_f32
        local.set 22
        local.set 5
        local.set 4
        local.get 4
        ref.is_null
        local.get 5
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 4
        local.get 4
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 5
          i32.const 4
          i32.add
        else
          local.get 5
          i32.const 1
          i32.add
        end
        local.get 22
        f32.demote_f64
        call 139
        local.get 13
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 8
    local.get 13
    global.set 27
    local.get 8
    throw_ref
  )
  (func (;6;) (type 78) (param i32 i32) (result i32)
    (local eqref i32 eqref i32 eqref f32 f32 i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 15
    i32.const 1048544
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 15
    i32.const 32
    i32.add
    global.set 27
    local.get 15
    i32.const 0
    i32.add
    local.get 0
    i32.const 8
    memory.copy
    local.get 15
    i32.const 0
    i32.add
    local.set 0
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.set 16
        local.get 15
        i32.const 8
        i32.add
        local.get 16
        i32.const 8
        memory.copy
        local.get 15
        i32.const 8
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 0
        local.set 16
        local.get 15
        i32.const 16
        i32.add
        local.get 16
        i32.const 8
        memory.copy
        local.get 15
        i32.const 16
        i32.add
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 8
        f32.demote_f64
        local.set 7
        local.get 15
        i32.const 24
        i32.add
        i32.const 0
        i32.const 8
        memory.fill
        ref.null eq
        local.set 6
        local.get 15
        i32.const 24
        i32.add
        local.set 9
        local.get 6
        local.get 7
        local.get 8
        local.get 9
        call 16
        local.get 15
        i32.const 24
        i32.add
        local.set 16
        local.get 1
        local.get 16
        i32.const 8
        memory.copy
        local.get 1
        local.get 15
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 10
    local.get 15
    global.set 27
    local.get 10
    throw_ref
  )
  (func (;7;) (type 79) (param eqref i32 eqref i32) (result i32)
    (local i32 i32 i32 eqref i32 eqref i32 eqref i32 i32 eqref eqref i32 i32 eqref i32 i32 i32 i32 eqref eqref eqref eqref i32 i32 i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 35
    i32.const 1048432
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 35
    i32.const 144
    i32.add
    global.set 27
    local.get 35
    i32.const 8
    i32.add
    local.set 4
    local.get 4
    i32.const 0
    i32.const 32
    memory.fill
    local.get 35
    i32.const 40
    i32.add
    local.set 5
    local.get 5
    i32.const 0
    i32.const 12
    memory.fill
    local.get 35
    i32.const 52
    i32.add
    local.set 6
    local.get 6
    i32.const 0
    i32.const 24
    memory.fill
    global.get 28
    local.tee 32
    i32.const 65532
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 32
    i32.const 4
    i32.add
    global.set 28
    local.get 35
    i32.const 24
    i32.add
    local.get 32
    i32.const 0
    i32.add
    i32.store align=1
    local.get 35
    i32.const 28
    i32.add
    local.get 32
    i32.const 1
    i32.add
    i32.store align=1
    local.get 35
    i32.const 32
    i32.add
    local.get 32
    i32.const 2
    i32.add
    i32.store align=1
    local.get 35
    i32.const 96
    i32.add
    local.get 32
    i32.const 3
    i32.add
    i32.store align=1
    local.get 35
    i32.const 0
    i32.add
    local.get 1
    i32.const 8
    memory.copy
    local.get 35
    i32.const 0
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 4
        local.set 8
        ref.null eq
        local.get 8
        local.get 35
        i32.const 80
        i32.add
        i32.const 0
        i32.const 8
        memory.fill
        local.get 35
        i32.const 80
        i32.add
        local.get 35
        i32.const 88
        i32.add
        i32.const 0
        i32.const 8
        memory.fill
        local.get 35
        i32.const 88
        i32.add
        local.get 2
        ref.null eq
        local.get 0
        local.tee 41
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 41
        ref.cast (ref 13)
        local.set 41
        local.get 35
        i32.const 96
        i32.add
        local.get 41
        ref.cast (ref 13)
        struct.get 13 28
        local.set 31
        i32.load align=1
        local.get 31
        table.set 1
        local.get 35
        i32.const 96
        i32.add
        local.set 16
        local.set 15
        local.set 14
        local.set 13
        local.set 12
        local.set 17
        local.set 11
        local.get 11
        local.get 12
        local.get 13
        local.get 14
        local.get 15
        local.get 16
        local.get 17
        call 17
        local.get 5
        local.set 8
        ref.null eq
        local.get 8
        local.get 1
        local.set 36
        local.get 35
        i32.const 100
        i32.add
        local.get 36
        i32.const 8
        memory.copy
        local.get 35
        i32.const 100
        i32.add
        i32.const 1
        i32.const 1
        i32.const 255
        i32.and
        local.set 21
        i32.const 255
        i32.and
        local.set 20
        local.set 19
        local.set 22
        local.set 18
        local.get 18
        local.get 19
        local.get 20
        local.get 21
        local.get 22
        call 18
        local.get 6
        local.set 8
        ref.null eq
        local.get 8
        local.set 8
        local.set 7
        local.get 7
        ref.is_null
        local.get 8
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 7
        ref.is_null
        local.get 8
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 7
        local.get 7
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 8
          i32.const 0
          i32.add
        else
          local.get 8
          i32.const 0
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 139
        local.get 7
        ref.is_null
        local.get 8
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 7
        local.get 7
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 8
          i32.const 4
          i32.add
        else
          local.get 8
          i32.const 1
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 139
        local.get 7
        ref.is_null
        local.get 8
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 7
        local.get 7
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 8
          i32.const 8
          i32.add
        else
          local.get 8
          i32.const 2
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 139
        local.get 7
        ref.is_null
        local.get 8
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 7
        local.get 7
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 8
          i32.const 12
          i32.add
        else
          local.get 8
          i32.const 3
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 139
        local.get 7
        ref.is_null
        local.get 8
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 7
        local.get 7
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 8
          i32.const 16
          i32.add
        else
          local.get 8
          i32.const 4
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 139
        local.get 7
        ref.is_null
        local.get 8
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 7
        local.get 7
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 8
          i32.const 20
          i32.add
        else
          local.get 8
          i32.const 5
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 139
        local.get 0
        local.get 4
        local.set 8
        ref.null eq
        local.get 8
        local.get 5
        local.set 8
        ref.null eq
        local.get 8
        local.get 6
        local.set 8
        ref.null eq
        local.get 8
        local.set 29
        local.set 26
        local.set 28
        local.set 25
        local.set 27
        local.set 24
        local.set 23
        local.get 23
        local.get 24
        local.get 25
        local.get 26
        local.get 27
        local.get 28
        local.get 29
        call 19
        local.get 6
        local.set 36
        local.get 35
        i32.const 108
        i32.add
        local.get 36
        i32.const 24
        memory.copy
        local.get 35
        i32.const 108
        i32.add
        i32.const 0
        i32.add
        local.set 36
        local.get 35
        i32.const 132
        i32.add
        local.get 36
        i32.const 8
        memory.copy
        local.get 35
        i32.const 132
        i32.add
        local.set 36
        local.get 3
        local.get 36
        i32.const 8
        memory.copy
        local.get 3
        local.get 32
        ref.null eq
        i32.const 4
        table.fill 1
        local.get 32
        ref.null extern
        i32.const 4
        table.fill 2
        local.get 32
        global.set 28
        local.get 35
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 30
    local.get 32
    ref.null eq
    i32.const 4
    table.fill 1
    local.get 32
    ref.null extern
    i32.const 4
    table.fill 2
    local.get 32
    global.set 28
    local.get 35
    global.set 27
    local.get 30
    throw_ref
  )
  (func (;8;) (type 80) (param eqref f32 f32 f32 f32 i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 15
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 15
    i32.const 0
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 5
        local.get 1
        f64.promote_f32
        local.set 24
        local.set 7
        local.set 6
        local.get 6
        ref.is_null
        local.get 7
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 6
        local.get 6
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 7
          i32.const 0
          i32.add
        else
          local.get 7
          i32.const 0
          i32.add
        end
        local.get 24
        f32.demote_f64
        call 139
        local.get 0
        local.get 5
        local.get 2
        f64.promote_f32
        local.set 24
        local.set 7
        local.set 6
        local.get 6
        ref.is_null
        local.get 7
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 6
        local.get 6
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 7
          i32.const 4
          i32.add
        else
          local.get 7
          i32.const 1
          i32.add
        end
        local.get 24
        f32.demote_f64
        call 139
        local.get 0
        local.get 5
        local.get 3
        f64.promote_f32
        local.set 24
        local.set 7
        local.set 6
        local.get 6
        ref.is_null
        local.get 7
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 6
        local.get 6
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 7
          i32.const 8
          i32.add
        else
          local.get 7
          i32.const 2
          i32.add
        end
        local.get 24
        f32.demote_f64
        call 139
        local.get 0
        local.get 5
        local.get 4
        f64.promote_f32
        local.set 24
        local.set 7
        local.set 6
        local.get 6
        ref.is_null
        local.get 7
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 6
        local.get 6
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 7
          i32.const 12
          i32.add
        else
          local.get 7
          i32.const 3
          i32.add
        end
        local.get 24
        f32.demote_f64
        call 139
        local.get 15
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 10
    local.get 15
    global.set 27
    local.get 10
    throw_ref
  )
  (func (;9;) (type 81) (param i32 i32) (result i32)
    (local eqref i32 eqref i32 eqref f32 f32 f32 f32 i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 17
    i32.const 1048480
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 17
    i32.const 96
    i32.add
    global.set 27
    local.get 17
    i32.const 0
    i32.add
    local.get 0
    i32.const 16
    memory.copy
    local.get 17
    i32.const 0
    i32.add
    local.set 0
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.set 18
        local.get 17
        i32.const 16
        i32.add
        local.get 18
        i32.const 16
        memory.copy
        local.get 17
        i32.const 16
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 0
        local.set 18
        local.get 17
        i32.const 32
        i32.add
        local.get 18
        i32.const 16
        memory.copy
        local.get 17
        i32.const 32
        i32.add
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 0
        local.set 18
        local.get 17
        i32.const 48
        i32.add
        local.get 18
        i32.const 16
        memory.copy
        local.get 17
        i32.const 48
        i32.add
        i32.const 8
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 0
        local.set 18
        local.get 17
        i32.const 64
        i32.add
        local.get 18
        i32.const 16
        memory.copy
        local.get 17
        i32.const 64
        i32.add
        i32.const 12
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 10
        f32.demote_f64
        local.set 9
        f32.demote_f64
        local.set 8
        f32.demote_f64
        local.set 7
        local.get 17
        i32.const 80
        i32.add
        i32.const 0
        i32.const 16
        memory.fill
        ref.null eq
        local.set 6
        local.get 17
        i32.const 80
        i32.add
        local.set 11
        local.get 6
        local.get 7
        local.get 8
        local.get 9
        local.get 10
        local.get 11
        call 13
        local.get 17
        i32.const 80
        i32.add
        local.set 18
        local.get 1
        local.get 18
        i32.const 16
        memory.copy
        local.get 1
        local.get 17
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 12
    local.get 17
    global.set 27
    local.get 12
    throw_ref
  )
  (func (;10;) (type 82) (param eqref i32 i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 12
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 12
    i32.const 16
    i32.add
    global.set 27
    local.get 12
    i32.const 0
    i32.add
    local.get 1
    i32.const 8
    memory.copy
    local.get 12
    i32.const 0
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 2
        local.get 1
        local.set 13
        local.get 12
        i32.const 8
        i32.add
        local.get 13
        i32.const 8
        memory.copy
        local.get 12
        i32.const 8
        i32.add
        local.set 19
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 4
          i32.add
        else
          local.get 4
          i32.const 1
          i32.add
        end
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
        else
          local.get 4
          i32.const 0
          i32.add
        end
        local.get 19
        i32.const 0
        i32.add
        f32.load align=1
        call 139
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 4
          i32.add
        else
          local.get 4
          i32.const 1
          i32.add
        end
        local.get 19
        i32.const 4
        i32.add
        f32.load align=1
        call 139
        local.get 0
        local.get 2
        i32.const 1
        local.set 19
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
        else
          local.get 4
          i32.const 0
          i32.add
        end
        local.get 19
        i32.const 255
        i32.and
        call 145
        local.get 12
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 7
    local.get 12
    global.set 27
    local.get 7
    throw_ref
  )
  (func (;11;) (type 83) (param eqref i32 eqref i32 i32) (result i32)
    (local i32 i32 i32 i32 i32 eqref i32 eqref i32 eqref i32 i32 eqref eqref i32 i32 eqref i32 eqref i32 eqref i32 eqref eqref i32 eqref i32 eqref i32 i32 i32 i32 i32 i32 i32 i32 eqref eqref eqref eqref i32 i32 i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 53
    i32.const 1048320
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 53
    i32.const 256
    i32.add
    global.set 27
    local.get 53
    i32.const 32
    i32.add
    local.set 5
    local.get 5
    i32.const 0
    i32.const 32
    memory.fill
    local.get 53
    i32.const 64
    i32.add
    local.set 6
    local.get 6
    i32.const 0
    i32.const 8
    memory.fill
    local.get 53
    i32.const 72
    i32.add
    local.set 7
    local.get 7
    i32.const 0
    i32.const 28
    memory.fill
    local.get 53
    i32.const 100
    i32.add
    local.set 8
    local.get 8
    i32.const 0
    i32.const 24
    memory.fill
    local.get 53
    i32.const 124
    i32.add
    local.set 9
    local.get 9
    i32.const 0
    i32.const 12
    memory.fill
    global.get 28
    local.tee 50
    i32.const 65532
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 50
    i32.const 4
    i32.add
    global.set 28
    local.get 53
    i32.const 48
    i32.add
    local.get 50
    i32.const 0
    i32.add
    i32.store align=1
    local.get 53
    i32.const 52
    i32.add
    local.get 50
    i32.const 1
    i32.add
    i32.store align=1
    local.get 53
    i32.const 56
    i32.add
    local.get 50
    i32.const 2
    i32.add
    i32.store align=1
    local.get 53
    i32.const 152
    i32.add
    local.get 50
    i32.const 3
    i32.add
    i32.store align=1
    local.get 53
    i32.const 0
    i32.add
    local.get 1
    i32.const 16
    memory.copy
    local.get 53
    i32.const 0
    i32.add
    local.set 1
    local.get 53
    i32.const 16
    i32.add
    local.get 3
    i32.const 12
    memory.copy
    local.get 53
    i32.const 16
    i32.add
    local.set 3
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 5
        local.set 11
        ref.null eq
        local.get 11
        local.get 53
        i32.const 136
        i32.add
        i32.const 0
        i32.const 8
        memory.fill
        local.get 53
        i32.const 136
        i32.add
        local.get 53
        i32.const 144
        i32.add
        i32.const 0
        i32.const 8
        memory.fill
        local.get 53
        i32.const 144
        i32.add
        local.get 2
        ref.null eq
        local.get 0
        local.tee 59
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 59
        ref.cast (ref 13)
        local.set 59
        local.get 53
        i32.const 152
        i32.add
        local.get 59
        ref.cast (ref 13)
        struct.get 13 28
        local.set 49
        i32.load align=1
        local.get 49
        table.set 1
        local.get 53
        i32.const 152
        i32.add
        local.set 19
        local.set 18
        local.set 17
        local.set 16
        local.set 15
        local.set 20
        local.set 14
        local.get 14
        local.get 15
        local.get 16
        local.get 17
        local.get 18
        local.get 19
        local.get 20
        call 17
        local.get 3
        local.set 54
        local.get 53
        i32.const 156
        i32.add
        local.get 54
        i32.const 12
        memory.copy
        local.get 53
        i32.const 156
        i32.add
        local.set 54
        local.get 9
        local.get 54
        i32.const 12
        memory.copy
        local.get 9
        local.set 11
        ref.null eq
        local.get 11
        local.set 22
        local.set 21
        local.get 21
        local.get 22
        call 20
        if (type 84) (result i32) ;; label = @3
          local.get 9
          local.set 11
          ref.null eq
          local.get 11
          local.set 29
          local.set 28
          local.get 28
          local.get 29
          local.get 53
          i32.const 184
          i32.add
          call 22
        else
          local.get 0
          local.get 1
          local.set 11
          ref.null eq
          local.get 11
          local.set 24
          local.set 23
          local.get 23
          local.get 24
          local.get 53
          i32.const 168
          i32.add
          call 21
          local.get 2
          local.set 27
          local.set 26
          local.set 25
          local.get 25
          local.get 26
          local.get 27
          local.get 53
          i32.const 176
          i32.add
          call 7
        end
        local.set 54
        local.get 6
        local.get 54
        i32.const 8
        memory.copy
        local.get 7
        local.set 11
        ref.null eq
        local.get 11
        local.get 1
        local.set 11
        ref.null eq
        local.get 11
        local.set 24
        local.set 23
        local.get 23
        local.get 24
        local.get 53
        i32.const 192
        i32.add
        call 21
        local.get 6
        local.set 54
        local.get 53
        i32.const 200
        i32.add
        local.get 54
        i32.const 8
        memory.copy
        local.get 53
        i32.const 200
        i32.add
        local.get 1
        local.set 11
        ref.null eq
        local.get 11
        local.set 31
        local.set 30
        local.get 30
        local.get 31
        local.get 53
        i32.const 208
        i32.add
        call 23
        i32.const 0
        i32.const 0
        i32.const 0
        i32.const 0
        i32.const 255
        i32.and
        local.set 39
        i32.const 255
        i32.and
        local.set 38
        i32.const 255
        i32.and
        local.set 37
        i32.const 255
        i32.and
        local.set 36
        local.set 35
        local.set 34
        local.set 33
        local.set 40
        local.set 32
        local.get 32
        local.get 33
        local.get 34
        local.get 35
        local.get 36
        local.get 37
        local.get 38
        local.get 39
        local.get 40
        call 24
        local.get 8
        local.set 11
        ref.null eq
        local.get 11
        local.set 11
        local.set 10
        local.get 10
        ref.is_null
        local.get 11
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 10
        ref.is_null
        local.get 11
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 10
        local.get 10
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 11
          i32.const 0
          i32.add
        else
          local.get 11
          i32.const 0
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 139
        local.get 10
        ref.is_null
        local.get 11
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 10
        local.get 10
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 11
          i32.const 4
          i32.add
        else
          local.get 11
          i32.const 1
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 139
        local.get 10
        ref.is_null
        local.get 11
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 10
        local.get 10
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 11
          i32.const 8
          i32.add
        else
          local.get 11
          i32.const 2
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 139
        local.get 10
        ref.is_null
        local.get 11
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 10
        local.get 10
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 11
          i32.const 12
          i32.add
        else
          local.get 11
          i32.const 3
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 139
        local.get 10
        ref.is_null
        local.get 11
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 10
        local.get 10
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 11
          i32.const 16
          i32.add
        else
          local.get 11
          i32.const 4
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 139
        local.get 10
        ref.is_null
        local.get 11
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 10
        local.get 10
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 11
          i32.const 20
          i32.add
        else
          local.get 11
          i32.const 5
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 139
        local.get 0
        local.get 5
        local.set 11
        ref.null eq
        local.get 11
        local.get 7
        local.set 11
        ref.null eq
        local.get 11
        local.get 8
        local.set 11
        ref.null eq
        local.get 11
        local.set 47
        local.set 44
        local.set 46
        local.set 43
        local.set 45
        local.set 42
        local.set 41
        local.get 41
        local.get 42
        local.get 43
        local.get 44
        local.get 45
        local.get 46
        local.get 47
        call 25
        local.get 8
        local.set 54
        local.get 53
        i32.const 216
        i32.add
        local.get 54
        i32.const 24
        memory.copy
        local.get 53
        i32.const 216
        i32.add
        i32.const 8
        i32.add
        local.set 54
        local.get 53
        i32.const 240
        i32.add
        local.get 54
        i32.const 16
        memory.copy
        local.get 53
        i32.const 240
        i32.add
        local.set 54
        local.get 4
        local.get 54
        i32.const 16
        memory.copy
        local.get 4
        local.get 50
        ref.null eq
        i32.const 4
        table.fill 1
        local.get 50
        ref.null extern
        i32.const 4
        table.fill 2
        local.get 50
        global.set 28
        local.get 53
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 48
    local.get 50
    ref.null eq
    i32.const 4
    table.fill 1
    local.get 50
    ref.null extern
    i32.const 4
    table.fill 2
    local.get 50
    global.set 28
    local.get 53
    global.set 27
    local.get 48
    throw_ref
  )
  (func (;12;) (type 85) (param eqref eqref)
    (local i32 i32 eqref i32 eqref i32 eqref i32 i32 eqref eqref i32 i32 eqref eqref i32 i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 24
    i32.const 1048480
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 24
    i32.const 96
    i32.add
    global.set 27
    local.get 24
    i32.const 0
    i32.add
    local.set 2
    local.get 2
    i32.const 0
    i32.const 32
    memory.fill
    local.get 24
    i32.const 32
    i32.add
    local.set 3
    local.get 3
    i32.const 0
    i32.const 20
    memory.fill
    global.get 28
    local.tee 21
    i32.const 65532
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 21
    i32.const 4
    i32.add
    global.set 28
    local.get 24
    i32.const 16
    i32.add
    local.get 21
    i32.const 0
    i32.add
    i32.store align=1
    local.get 24
    i32.const 20
    i32.add
    local.get 21
    i32.const 1
    i32.add
    i32.store align=1
    local.get 24
    i32.const 24
    i32.add
    local.get 21
    i32.const 2
    i32.add
    i32.store align=1
    local.get 24
    i32.const 72
    i32.add
    local.get 21
    i32.const 3
    i32.add
    i32.store align=1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 2
        local.set 5
        ref.null eq
        local.get 5
        local.get 24
        i32.const 56
        i32.add
        i32.const 0
        i32.const 8
        memory.fill
        local.get 24
        i32.const 56
        i32.add
        local.get 24
        i32.const 64
        i32.add
        i32.const 0
        i32.const 8
        memory.fill
        local.get 24
        i32.const 64
        i32.add
        local.get 1
        local.get 1
        local.get 0
        local.tee 30
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 30
        ref.cast (ref 13)
        local.set 30
        local.get 24
        i32.const 72
        i32.add
        local.get 30
        ref.cast (ref 13)
        struct.get 13 28
        local.set 20
        i32.load align=1
        local.get 20
        table.set 1
        local.get 24
        i32.const 72
        i32.add
        local.set 13
        local.set 12
        local.set 11
        local.set 10
        local.set 9
        local.set 14
        local.set 8
        local.get 8
        local.get 9
        local.get 10
        local.get 11
        local.get 12
        local.get 13
        local.get 14
        call 17
        local.get 0
        local.get 2
        local.set 5
        ref.null eq
        local.get 5
        local.get 3
        local.set 5
        ref.null eq
        local.get 5
        local.set 5
        local.set 4
        local.get 4
        ref.is_null
        local.get 5
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 4
        ref.is_null
        local.get 5
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 4
        local.get 4
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 5
          i32.const 0
          i32.add
        else
          local.get 5
          i32.const 0
          i32.add
        end
        i32.const 0
        call 145
        local.get 4
        ref.is_null
        local.get 5
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 4
        local.get 4
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 5
          i32.const 4
          i32.add
        else
          local.get 5
          i32.const 1
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 139
        local.get 4
        ref.is_null
        local.get 5
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 4
        local.get 4
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 5
          i32.const 8
          i32.add
        else
          local.get 5
          i32.const 2
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 139
        local.get 4
        ref.is_null
        local.get 5
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 4
        local.get 4
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 5
          i32.const 12
          i32.add
        else
          local.get 5
          i32.const 3
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 139
        local.get 4
        ref.is_null
        local.get 5
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 4
        local.get 4
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 5
          i32.const 16
          i32.add
        else
          local.get 5
          i32.const 4
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 139
        local.get 3
        local.set 25
        local.get 24
        i32.const 76
        i32.add
        local.get 25
        i32.const 20
        memory.copy
        local.get 24
        i32.const 76
        i32.add
        local.set 17
        local.set 18
        local.set 16
        local.set 15
        local.get 15
        local.get 16
        local.get 17
        local.get 18
        call 26
        local.get 21
        ref.null eq
        i32.const 4
        table.fill 1
        local.get 21
        ref.null extern
        i32.const 4
        table.fill 2
        local.get 21
        global.set 28
        local.get 24
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 19
    local.get 21
    ref.null eq
    i32.const 4
    table.fill 1
    local.get 21
    ref.null extern
    i32.const 4
    table.fill 2
    local.get 21
    global.set 28
    local.get 24
    global.set 27
    local.get 19
    throw_ref
  )
  (func (;13;) (type 86) (param eqref f32 f32 f32 f32 i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 15
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 15
    i32.const 0
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 5
        local.get 1
        f64.promote_f32
        local.set 24
        local.set 7
        local.set 6
        local.get 6
        ref.is_null
        local.get 7
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 6
        local.get 6
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 7
          i32.const 0
          i32.add
        else
          local.get 7
          i32.const 0
          i32.add
        end
        local.get 24
        f32.demote_f64
        call 139
        local.get 0
        local.get 5
        local.get 2
        f64.promote_f32
        local.set 24
        local.set 7
        local.set 6
        local.get 6
        ref.is_null
        local.get 7
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 6
        local.get 6
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 7
          i32.const 4
          i32.add
        else
          local.get 7
          i32.const 1
          i32.add
        end
        local.get 24
        f32.demote_f64
        call 139
        local.get 0
        local.get 5
        local.get 3
        f64.promote_f32
        local.set 24
        local.set 7
        local.set 6
        local.get 6
        ref.is_null
        local.get 7
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 6
        local.get 6
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 7
          i32.const 8
          i32.add
        else
          local.get 7
          i32.const 2
          i32.add
        end
        local.get 24
        f32.demote_f64
        call 139
        local.get 0
        local.get 5
        local.get 4
        f64.promote_f32
        local.set 24
        local.set 7
        local.set 6
        local.get 6
        ref.is_null
        local.get 7
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 6
        local.get 6
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 7
          i32.const 12
          i32.add
        else
          local.get 7
          i32.const 3
          i32.add
        end
        local.get 24
        f32.demote_f64
        call 139
        local.get 15
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 10
    local.get 15
    global.set 27
    local.get 10
    throw_ref
  )
  (func (;14;) (type 87) (param i32 i32) (result i32)
    (local f32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 8
    i32.const 1048416
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 8
    i32.const 160
    i32.add
    global.set 27
    local.get 8
    i32.const 0
    i32.add
    local.get 0
    i32.const 16
    memory.copy
    local.get 8
    i32.const 0
    i32.add
    local.set 0
    local.get 8
    i32.const 16
    i32.add
    local.get 1
    i32.const 16
    memory.copy
    local.get 8
    i32.const 16
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.set 9
        local.get 8
        i32.const 32
        i32.add
        local.get 9
        i32.const 16
        memory.copy
        local.get 8
        i32.const 32
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 9
        local.get 8
        i32.const 48
        i32.add
        local.get 9
        i32.const 16
        memory.copy
        local.get 8
        i32.const 48
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 2
        f32.demote_f64
        local.get 2
        f32.eq
        i32.eqz
        if (type 88) ;; label = @3
        else
          local.get 0
          local.set 9
          local.get 8
          i32.const 64
          i32.add
          local.get 9
          i32.const 16
          memory.copy
          local.get 8
          i32.const 64
          i32.add
          i32.const 4
          i32.add
          f32.load align=1
          f64.promote_f32
          local.get 1
          local.set 9
          local.get 8
          i32.const 80
          i32.add
          local.get 9
          i32.const 16
          memory.copy
          local.get 8
          i32.const 80
          i32.add
          i32.const 4
          i32.add
          f32.load align=1
          f64.promote_f32
          f32.demote_f64
          local.set 2
          f32.demote_f64
          local.get 2
          f32.eq
          i32.eqz
          if (type 89) ;; label = @4
          else
            local.get 0
            local.set 9
            local.get 8
            i32.const 96
            i32.add
            local.get 9
            i32.const 16
            memory.copy
            local.get 8
            i32.const 96
            i32.add
            i32.const 8
            i32.add
            f32.load align=1
            f64.promote_f32
            local.get 1
            local.set 9
            local.get 8
            i32.const 112
            i32.add
            local.get 9
            i32.const 16
            memory.copy
            local.get 8
            i32.const 112
            i32.add
            i32.const 8
            i32.add
            f32.load align=1
            f64.promote_f32
            f32.demote_f64
            local.set 2
            f32.demote_f64
            local.get 2
            f32.eq
            i32.eqz
            if (type 90) ;; label = @5
            else
              local.get 0
              local.set 9
              local.get 8
              i32.const 128
              i32.add
              local.get 9
              i32.const 16
              memory.copy
              local.get 8
              i32.const 128
              i32.add
              i32.const 12
              i32.add
              f32.load align=1
              f64.promote_f32
              local.get 1
              local.set 9
              local.get 8
              i32.const 144
              i32.add
              local.get 9
              i32.const 16
              memory.copy
              local.get 8
              i32.const 144
              i32.add
              i32.const 12
              i32.add
              f32.load align=1
              f64.promote_f32
              f32.demote_f64
              local.set 2
              f32.demote_f64
              local.get 2
              f32.eq
              i32.const 255
              i32.and
              local.get 8
              global.set 27
              return
            end
          end
        end
        i32.const 0
        i32.const 255
        i32.and
        local.get 8
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 3
    local.get 8
    global.set 27
    local.get 3
    throw_ref
  )
  (func (;15;) (type 91) (param eqref)
    (local eqref i32 eqref i32 f32 eqref f32 f32 i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 15
    i32.const 1048544
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 15
    i32.const 32
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 0
        f32.convert_i32_s
        f64.promote_f32
        i32.const 0
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 8
        f32.demote_f64
        local.set 7
        local.get 15
        i32.const 0
        i32.add
        i32.const 0
        i32.const 8
        memory.fill
        ref.null eq
        local.set 6
        local.get 15
        i32.const 0
        i32.add
        local.set 9
        local.get 6
        local.get 7
        local.get 8
        local.get 9
        call 5
        local.get 15
        i32.const 0
        i32.add
        local.get 15
        i32.const 8
        i32.add
        call 27
        local.set 22
        local.tee 21
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 21
        ref.cast (ref 13)
        local.set 21
        local.get 21
        ref.cast (ref 13)
        local.get 22
        i32.const 0
        i32.add
        f32.load align=1
        struct.set 13 42
        local.get 21
        ref.cast (ref 13)
        local.get 22
        i32.const 4
        i32.add
        f32.load align=1
        struct.set 13 43
        local.get 21
        ref.cast (ref 13)
        local.get 22
        i32.const 8
        i32.add
        f32.load align=1
        struct.set 13 44
        local.get 21
        ref.cast (ref 13)
        local.get 22
        i32.const 12
        i32.add
        f32.load align=1
        struct.set 13 45
        local.get 0
        i32.const 1
        local.set 22
        local.tee 21
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 21
        ref.cast (ref 13)
        local.set 21
        local.get 21
        ref.cast (ref 13)
        local.get 22
        i32.const 255
        i32.and
        struct.set 13 49
        local.get 0
        i32.const 1
        local.set 22
        local.tee 21
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 21
        ref.cast (ref 13)
        local.set 21
        local.get 21
        ref.cast (ref 13)
        local.get 22
        i32.const 255
        i32.and
        struct.set 13 50
        local.get 0
        i32.const 0
        f32.convert_i32_s
        f64.promote_f32
        local.set 24
        local.tee 21
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 21
        ref.cast (ref 13)
        local.set 21
        local.get 21
        ref.cast (ref 13)
        local.get 24
        f32.demote_f64
        struct.set 13 51
        local.get 0
        i32.const 0
        f32.convert_i32_s
        f64.promote_f32
        local.set 24
        local.tee 21
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 21
        ref.cast (ref 13)
        local.set 21
        local.get 21
        ref.cast (ref 13)
        local.get 24
        f32.demote_f64
        struct.set 13 52
        local.get 0
        f64.const inf (;=inf;)
        local.set 24
        local.tee 21
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 21
        ref.cast (ref 13)
        local.set 21
        local.get 21
        ref.cast (ref 13)
        local.get 24
        f32.demote_f64
        struct.set 13 53
        local.get 0
        f64.const inf (;=inf;)
        local.set 24
        local.tee 21
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 21
        ref.cast (ref 13)
        local.set 21
        local.get 21
        ref.cast (ref 13)
        local.get 24
        f32.demote_f64
        struct.set 13 54
        local.get 0
        drop
        local.get 15
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 10
    local.get 15
    global.set 27
    local.get 10
    throw_ref
  )
  (func (;16;) (type 92) (param eqref f32 f32 i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 13
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 13
    i32.const 0
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 3
        local.get 1
        f64.promote_f32
        local.set 22
        local.set 5
        local.set 4
        local.get 4
        ref.is_null
        local.get 5
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 4
        local.get 4
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 5
          i32.const 0
          i32.add
        else
          local.get 5
          i32.const 0
          i32.add
        end
        local.get 22
        f32.demote_f64
        call 139
        local.get 0
        local.get 3
        local.get 2
        f64.promote_f32
        local.set 22
        local.set 5
        local.set 4
        local.get 4
        ref.is_null
        local.get 5
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 4
        local.get 4
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 5
          i32.const 4
          i32.add
        else
          local.get 5
          i32.const 1
          i32.add
        end
        local.get 22
        f32.demote_f64
        call 139
        local.get 13
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 8
    local.get 13
    global.set 27
    local.get 8
    throw_ref
  )
  (func (;17;) (type 93) (param eqref i32 i32 eqref eqref i32 i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 16
    i32.const 1048528
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 16
    i32.const 48
    i32.add
    global.set 27
    global.get 28
    local.tee 13
    i32.const 65534
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 13
    i32.const 2
    i32.add
    global.set 28
    local.get 16
    i32.const 16
    i32.add
    local.get 13
    i32.const 0
    i32.add
    i32.store align=1
    local.get 16
    i32.const 40
    i32.add
    local.get 13
    i32.const 1
    i32.add
    i32.store align=1
    local.get 16
    i32.const 0
    i32.add
    local.get 1
    i32.const 8
    memory.copy
    local.get 16
    i32.const 0
    i32.add
    local.set 1
    local.get 16
    i32.const 8
    i32.add
    local.get 2
    i32.const 8
    memory.copy
    local.get 16
    i32.const 8
    i32.add
    local.set 2
    local.get 16
    i32.const 16
    i32.add
    local.get 5
    local.set 14
    local.set 15
    local.get 15
    i32.const 0
    i32.add
    local.get 14
    i32.const 0
    i32.add
    i32.load align=1
    table.get 1
    local.set 12
    i32.load align=1
    local.get 12
    table.set 1
    local.get 16
    i32.const 16
    i32.add
    local.set 5
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 6
        local.get 1
        local.set 17
        local.get 16
        i32.const 24
        i32.add
        local.get 17
        i32.const 8
        memory.copy
        local.get 16
        i32.const 24
        i32.add
        local.set 23
        local.set 8
        local.set 7
        local.get 7
        ref.is_null
        local.get 8
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 7
        local.get 7
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 8
          i32.const 0
          i32.add
        else
          local.get 8
          i32.const 0
          i32.add
        end
        local.set 8
        local.set 7
        local.get 7
        ref.is_null
        local.get 8
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 7
        local.get 7
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 8
          i32.const 0
          i32.add
        else
          local.get 8
          i32.const 0
          i32.add
        end
        local.get 23
        i32.const 0
        i32.add
        i64.load align=1
        call 141
        local.get 0
        local.get 6
        local.get 2
        local.set 17
        local.get 16
        i32.const 32
        i32.add
        local.get 17
        i32.const 8
        memory.copy
        local.get 16
        i32.const 32
        i32.add
        local.set 23
        local.set 8
        local.set 7
        local.get 7
        ref.is_null
        local.get 8
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 7
        local.get 7
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 8
          i32.const 8
          i32.add
        else
          local.get 8
          i32.const 1
          i32.add
        end
        local.set 8
        local.set 7
        local.get 7
        ref.is_null
        local.get 8
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 7
        local.get 7
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 8
          i32.const 0
          i32.add
        else
          local.get 8
          i32.const 0
          i32.add
        end
        local.get 23
        i32.const 0
        i32.add
        i64.load align=1
        call 141
        local.get 0
        local.get 6
        local.get 3
        local.set 21
        local.set 8
        local.set 7
        local.get 7
        ref.is_null
        local.get 8
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 7
        local.get 7
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 8
          i32.const 16
          i32.add
          i32.load align=1
        else
          local.get 8
          i32.const 2
          i32.add
        end
        local.get 21
        call 135
        local.get 0
        local.get 6
        local.get 4
        local.set 21
        local.set 8
        local.set 7
        local.get 7
        ref.is_null
        local.get 8
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 7
        local.get 7
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 8
          i32.const 20
          i32.add
          i32.load align=1
        else
          local.get 8
          i32.const 3
          i32.add
        end
        local.get 21
        call 135
        local.get 0
        local.get 6
        local.get 5
        local.set 17
        local.get 16
        i32.const 40
        i32.add
        local.get 17
        local.set 14
        local.set 15
        local.get 15
        i32.const 0
        i32.add
        local.get 14
        i32.const 0
        i32.add
        i32.load align=1
        table.get 1
        local.set 12
        i32.load align=1
        local.get 12
        table.set 1
        local.get 16
        i32.const 40
        i32.add
        local.set 23
        local.set 8
        local.set 7
        local.get 7
        ref.is_null
        local.get 8
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 7
        local.get 7
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 8
          i32.const 24
          i32.add
        else
          local.get 8
          i32.const 4
          i32.add
        end
        local.set 8
        local.set 7
        local.get 7
        ref.is_null
        local.get 8
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 7
        local.get 7
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 8
          i32.const 0
          i32.add
          i32.load align=1
        else
          local.get 8
          i32.const 0
          i32.add
        end
        local.get 23
        i32.const 0
        i32.add
        i32.load align=1
        table.get 1
        call 135
        local.get 13
        ref.null eq
        i32.const 2
        table.fill 1
        local.get 13
        ref.null extern
        i32.const 2
        table.fill 2
        local.get 13
        global.set 28
        local.get 16
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 11
    local.get 13
    ref.null eq
    i32.const 2
    table.fill 1
    local.get 13
    ref.null extern
    i32.const 2
    table.fill 2
    local.get 13
    global.set 28
    local.get 16
    global.set 27
    local.get 11
    throw_ref
  )
  (func (;18;) (type 94) (param eqref i32 i32 i32 i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 14
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 14
    i32.const 16
    i32.add
    global.set 27
    local.get 14
    i32.const 0
    i32.add
    local.get 1
    i32.const 8
    memory.copy
    local.get 14
    i32.const 0
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 4
        local.get 1
        local.set 15
        local.get 14
        i32.const 8
        i32.add
        local.get 15
        i32.const 8
        memory.copy
        local.get 14
        i32.const 8
        i32.add
        local.set 21
        local.set 6
        local.set 5
        local.get 5
        ref.is_null
        local.get 6
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 5
        local.get 5
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 6
          i32.const 0
          i32.add
        else
          local.get 6
          i32.const 0
          i32.add
        end
        local.set 6
        local.set 5
        local.get 5
        ref.is_null
        local.get 6
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 5
        local.get 5
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 6
          i32.const 0
          i32.add
        else
          local.get 6
          i32.const 0
          i32.add
        end
        local.get 21
        i32.const 0
        i32.add
        f32.load align=1
        call 139
        local.get 5
        ref.is_null
        local.get 6
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 5
        local.get 5
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 6
          i32.const 4
          i32.add
        else
          local.get 6
          i32.const 1
          i32.add
        end
        local.get 21
        i32.const 4
        i32.add
        f32.load align=1
        call 139
        local.get 0
        local.get 4
        local.get 2
        local.set 21
        local.set 6
        local.set 5
        local.get 5
        ref.is_null
        local.get 6
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 5
        local.get 5
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 6
          i32.const 8
          i32.add
        else
          local.get 6
          i32.const 2
          i32.add
        end
        local.get 21
        i32.const 255
        i32.and
        call 143
        local.get 0
        local.get 4
        local.get 3
        local.set 21
        local.set 6
        local.set 5
        local.get 5
        ref.is_null
        local.get 6
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 5
        local.get 5
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 6
          i32.const 9
          i32.add
        else
          local.get 6
          i32.const 3
          i32.add
        end
        local.get 21
        i32.const 255
        i32.and
        call 143
        local.get 14
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 9
    local.get 14
    global.set 27
    local.get 9
    throw_ref
  )
  (func (;19;) (type 95) (param eqref eqref eqref eqref i32 i32 i32)
    (local i32 i32 i32 i32 eqref i32 eqref i32 f32 f32 f32 eqref eqref eqref i32 i32 eqref f32 f32 i32 eqref i32 eqref i32 eqref eqref i32 eqref eqref eqref i32 eqref i32 eqref eqref eqref eqref eqref eqref exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 51
    i32.const 1048336
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 51
    i32.const 240
    i32.add
    global.set 27
    local.get 51
    i32.const 0
    i32.add
    local.set 7
    local.get 7
    i32.const 0
    i32.const 8
    memory.fill
    local.get 51
    i32.const 8
    i32.add
    local.set 10
    local.get 10
    i32.const 0
    i32.const 8
    memory.fill
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.get 4
        local.get 2
        local.get 5
        local.set 22
        local.set 20
        local.set 21
        local.set 19
        local.set 18
        local.get 18
        local.get 19
        local.get 20
        local.get 21
        local.get 22
        call 28
        i32.eqz
        if (type 96) ;; label = @3
          i32.const 0
          f32.convert_i32_s
          f64.promote_f32
          i32.const 0
          f32.convert_i32_s
          f64.promote_f32
          f32.demote_f64
          local.set 25
          f32.demote_f64
          local.set 24
          local.get 51
          i32.const 24
          i32.add
          i32.const 0
          i32.const 8
          memory.fill
          ref.null eq
          local.set 23
          local.get 51
          i32.const 24
          i32.add
          local.set 26
          local.get 23
          local.get 24
          local.get 25
          local.get 26
          call 5
          local.get 51
          i32.const 24
          i32.add
          local.get 51
          i32.const 32
          i32.add
          call 6
          local.get 2
          local.get 5
          local.set 28
          local.set 27
          local.get 27
          local.get 28
          local.get 51
          i32.const 40
          i32.add
          call 29
          local.get 51
          i32.const 48
          i32.add
          call 30
          local.set 52
          local.get 7
          local.get 52
          i32.const 8
          memory.copy
          local.get 2
          local.get 5
          local.set 30
          local.set 29
          local.get 29
          local.get 30
          call 31
          if (type 97) (result i32) ;; label = @4
            i32.const 0
          else
            local.get 2
            local.get 5
            local.set 28
            local.set 27
            local.get 27
            local.get 28
            local.get 51
            i32.const 56
            i32.add
            call 29
            i32.const 0
            i32.add
            f32.load align=1
            f64.promote_f32
            f32.demote_f64
            f32.abs
            f32.const inf (;=inf;)
            f32.lt
            i32.eqz
            if (type 98) (result i32) ;; label = @5
              i32.const 0
            else
              local.get 0
              local.set 31
              local.get 31
              call 32
              i32.const 0
              i32.eq
            end
          end
          i32.const 255
          i32.and
          local.set 8
          local.get 2
          local.get 5
          local.set 33
          local.set 32
          local.get 32
          local.get 33
          call 33
          if (type 99) (result i32) ;; label = @4
            i32.const 0
          else
            local.get 2
            local.get 5
            local.set 28
            local.set 27
            local.get 27
            local.get 28
            local.get 51
            i32.const 64
            i32.add
            call 29
            i32.const 4
            i32.add
            f32.load align=1
            f64.promote_f32
            f32.demote_f64
            f32.abs
            f32.const inf (;=inf;)
            f32.lt
            i32.eqz
            if (type 100) (result i32) ;; label = @5
              i32.const 0
            else
              local.get 0
              local.set 34
              local.get 34
              call 34
              i32.const 0
              i32.eq
            end
          end
          i32.const 255
          i32.and
          local.set 9
          local.get 8
          local.get 9
          i32.and
          i32.eqz
          if (type 101) ;; label = @4
            local.get 0
            local.get 7
            local.set 52
            local.get 51
            i32.const 104
            i32.add
            local.get 52
            i32.const 8
            memory.copy
            local.get 51
            i32.const 104
            i32.add
            local.get 0
            local.set 35
            local.get 35
            local.get 51
            i32.const 112
            i32.add
            call 35
            local.get 51
            i32.const 128
            i32.add
            call 36
            local.get 1
            local.get 4
            local.set 37
            local.set 36
            local.get 36
            local.get 37
            call 37
            local.set 40
            local.set 39
            local.set 38
            local.get 38
            local.tee 57
            ref.is_null
            if ;; label = @5
              i32.const 4
              global.get 23
              ref.is_null
              if ;; label = @6
                i32.const 0
                i32.const 53
                array.new_data 0 23
                i32.const 0
                i32.const 53
                call 1
                global.set 23
              end
              global.get 23
              struct.new 3
              throw 0
            end
            local.get 57
            ref.cast (ref 13)
            local.get 39
            local.get 40
            local.get 51
            i32.const 136
            i32.add
            local.get 38
            ref.cast (ref 10)
            struct.get 10 0
            ref.cast (ref 9)
            struct.get 9 2
            i32.const 31
            i32.add
            call_indirect (type 193)
            local.set 52
            local.get 10
            local.get 52
            i32.const 8
            memory.copy
            local.get 8
            i32.eqz
            if (type 102) ;; label = @5
            else
              local.get 10
              local.set 12
              ref.null eq
              local.get 12
              local.get 2
              local.get 5
              local.set 28
              local.set 27
              local.get 27
              local.get 28
              local.get 51
              i32.const 144
              i32.add
              call 29
              i32.const 0
              i32.add
              f32.load align=1
              f64.promote_f32
              local.set 60
              local.set 12
              local.set 11
              local.get 11
              ref.is_null
              local.get 12
              i32.const -1
              i32.eq
              i32.and
              if ;; label = @6
                i32.const 4
                global.get 23
                ref.is_null
                if ;; label = @7
                  i32.const 0
                  i32.const 53
                  array.new_data 0 23
                  i32.const 0
                  i32.const 53
                  call 1
                  global.set 23
                end
                global.get 23
                struct.new 3
                throw 0
              end
              local.get 11
              local.get 11
              ref.is_null
              if (type 59) (result i32) ;; label = @6
                local.get 12
                i32.const 0
                i32.add
              else
                local.get 12
                i32.const 0
                i32.add
              end
              local.get 60
              f32.demote_f64
              call 139
            end
            local.get 9
            i32.eqz
            if (type 103) ;; label = @5
            else
              local.get 10
              local.set 12
              ref.null eq
              local.get 12
              local.get 2
              local.get 5
              local.set 28
              local.set 27
              local.get 27
              local.get 28
              local.get 51
              i32.const 152
              i32.add
              call 29
              i32.const 4
              i32.add
              f32.load align=1
              f64.promote_f32
              local.set 60
              local.set 12
              local.set 11
              local.get 11
              ref.is_null
              local.get 12
              i32.const -1
              i32.eq
              i32.and
              if ;; label = @6
                i32.const 4
                global.get 23
                ref.is_null
                if ;; label = @7
                  i32.const 0
                  i32.const 53
                  array.new_data 0 23
                  i32.const 0
                  i32.const 53
                  call 1
                  global.set 23
                end
                global.get 23
                struct.new 3
                throw 0
              end
              local.get 11
              local.get 11
              ref.is_null
              if (type 59) (result i32) ;; label = @6
                local.get 12
                i32.const 4
                i32.add
              else
                local.get 12
                i32.const 1
                i32.add
              end
              local.get 60
              f32.demote_f64
              call 139
            end
          else
            local.get 7
            local.set 52
            local.get 51
            i32.const 72
            i32.add
            local.get 52
            i32.const 8
            memory.copy
            local.get 51
            i32.const 72
            i32.add
            local.get 0
            local.set 35
            local.get 35
            local.get 51
            i32.const 80
            i32.add
            call 35
            local.get 51
            i32.const 96
            i32.add
            call 36
            local.set 52
            local.get 10
            local.get 52
            i32.const 8
            memory.copy
          end
          local.get 10
          local.set 52
          local.get 51
          i32.const 160
          i32.add
          local.get 52
          i32.const 8
          memory.copy
          local.get 51
          i32.const 160
          i32.add
          i32.const 0
          i32.add
          f32.load align=1
          f64.promote_f32
          local.get 0
          local.set 41
          local.get 41
          call 38
          f64.promote_f32
          local.get 0
          local.set 42
          local.get 42
          call 39
          f64.promote_f32
          f32.demote_f64
          local.set 17
          f32.demote_f64
          local.set 16
          f32.demote_f64
          local.set 15
          local.get 16
          local.get 17
          f32.gt
          if ;; label = @4
            i32.const 6
            global.get 25
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 46
              array.new_data 0 25
              i32.const 0
              i32.const 46
              call 1
              global.set 25
            end
            global.get 25
            struct.new 3
            throw 0
          end
          local.get 15
          local.get 16
          f32.max
          local.get 17
          f32.min
          f64.promote_f32
          local.get 10
          local.set 52
          local.get 51
          i32.const 168
          i32.add
          local.get 52
          i32.const 8
          memory.copy
          local.get 51
          i32.const 168
          i32.add
          i32.const 4
          i32.add
          f32.load align=1
          f64.promote_f32
          local.get 0
          local.set 43
          local.get 43
          call 40
          f64.promote_f32
          local.get 0
          local.set 44
          local.get 44
          call 41
          f64.promote_f32
          f32.demote_f64
          local.set 17
          f32.demote_f64
          local.set 16
          f32.demote_f64
          local.set 15
          local.get 16
          local.get 17
          f32.gt
          if ;; label = @4
            i32.const 6
            global.get 25
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 46
              array.new_data 0 25
              i32.const 0
              i32.const 46
              call 1
              global.set 25
            end
            global.get 25
            struct.new 3
            throw 0
          end
          local.get 15
          local.get 16
          f32.max
          local.get 17
          f32.min
          f64.promote_f32
          f32.demote_f64
          local.set 25
          f32.demote_f64
          local.set 24
          local.get 51
          i32.const 176
          i32.add
          i32.const 0
          i32.const 8
          memory.fill
          ref.null eq
          local.set 23
          local.get 51
          i32.const 176
          i32.add
          local.set 26
          local.get 23
          local.get 24
          local.get 25
          local.get 26
          call 5
          local.get 51
          i32.const 176
          i32.add
          local.get 51
          i32.const 184
          i32.add
          call 6
          local.set 52
          local.get 10
          local.get 52
          i32.const 8
          memory.copy
          local.get 3
          local.get 6
          local.get 10
          local.set 52
          local.get 51
          i32.const 192
          i32.add
          local.get 52
          i32.const 8
          memory.copy
          local.get 51
          i32.const 192
          i32.add
          local.get 0
          local.set 35
          local.get 35
          local.get 51
          i32.const 200
          i32.add
          call 35
          local.get 51
          i32.const 216
          i32.add
          call 42
          local.set 58
          local.set 12
          local.set 11
          local.get 11
          ref.is_null
          local.get 12
          i32.const -1
          i32.eq
          i32.and
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 11
          local.get 11
          ref.is_null
          if (type 59) (result i32) ;; label = @4
            local.get 12
            i32.const 0
            i32.add
          else
            local.get 12
            i32.const 0
            i32.add
          end
          local.set 12
          local.set 11
          local.get 11
          ref.is_null
          local.get 12
          i32.const -1
          i32.eq
          i32.and
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 11
          local.get 11
          ref.is_null
          if (type 59) (result i32) ;; label = @4
            local.get 12
            i32.const 0
            i32.add
          else
            local.get 12
            i32.const 0
            i32.add
          end
          local.get 58
          i32.const 0
          i32.add
          f32.load align=1
          call 139
          local.get 11
          ref.is_null
          local.get 12
          i32.const -1
          i32.eq
          i32.and
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 11
          local.get 11
          ref.is_null
          if (type 59) (result i32) ;; label = @4
            local.get 12
            i32.const 4
            i32.add
          else
            local.get 12
            i32.const 1
            i32.add
          end
          local.get 58
          i32.const 4
          i32.add
          f32.load align=1
          call 139
          local.get 0
          local.get 2
          local.get 5
          local.set 28
          local.set 27
          local.get 27
          local.get 28
          local.get 51
          i32.const 224
          i32.add
          call 29
          local.set 58
          local.tee 57
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 57
          ref.cast (ref 13)
          local.set 57
          local.get 57
          ref.cast (ref 13)
          local.get 58
          i32.const 0
          i32.add
          f32.load align=1
          struct.set 13 3
          local.get 57
          ref.cast (ref 13)
          local.get 58
          i32.const 4
          i32.add
          f32.load align=1
          struct.set 13 4
          local.get 0
          local.get 3
          local.get 6
          local.set 12
          local.set 11
          local.get 11
          ref.is_null
          local.get 12
          i32.const -1
          i32.eq
          i32.and
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 11
          local.get 11
          ref.is_null
          if (type 59) (result i32) ;; label = @4
            local.get 12
            i32.const 0
            i32.add
          else
            local.get 12
            i32.const 0
            i32.add
          end
          local.set 12
          local.set 11
          local.get 11
          ref.is_null
          local.get 12
          i32.const -1
          i32.eq
          i32.and
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 51
          i32.const 232
          i32.add
          local.get 11
          ref.is_null
          local.get 12
          i32.const -1
          i32.eq
          i32.and
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 11
          local.get 11
          ref.is_null
          if (type 59) (result i32) ;; label = @4
            local.get 12
            i32.const 0
            i32.add
          else
            local.get 12
            i32.const 0
            i32.add
          end
          call 138
          f32.store align=1
          local.get 51
          i32.const 236
          i32.add
          local.get 11
          ref.is_null
          local.get 12
          i32.const -1
          i32.eq
          i32.and
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 11
          local.get 11
          ref.is_null
          if (type 59) (result i32) ;; label = @4
            local.get 12
            i32.const 4
            i32.add
          else
            local.get 12
            i32.const 1
            i32.add
          end
          call 138
          f32.store align=1
          local.get 51
          i32.const 232
          i32.add
          local.set 58
          local.tee 57
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 57
          ref.cast (ref 13)
          local.set 57
          local.get 57
          ref.cast (ref 13)
          local.get 58
          i32.const 0
          i32.add
          f32.load align=1
          struct.set 13 5
          local.get 57
          ref.cast (ref 13)
          local.get 58
          i32.const 4
          i32.add
          f32.load align=1
          struct.set 13 6
          local.get 0
          local.get 1
          local.get 4
          local.set 37
          local.set 36
          local.get 36
          local.get 37
          call 37
          local.set 56
          local.tee 57
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 57
          ref.cast (ref 13)
          local.set 57
          local.get 57
          ref.cast (ref 13)
          local.get 56
          struct.set 13 7
          local.get 0
          local.get 2
          local.get 5
          local.set 30
          local.set 29
          local.get 29
          local.get 30
          call 31
          local.set 58
          local.tee 57
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 57
          ref.cast (ref 13)
          local.set 57
          local.get 57
          ref.cast (ref 13)
          local.get 58
          i32.const 255
          i32.and
          struct.set 13 8
          local.get 0
          local.get 2
          local.get 5
          local.set 33
          local.set 32
          local.get 32
          local.get 33
          call 33
          local.set 58
          local.tee 57
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 57
          ref.cast (ref 13)
          local.set 57
          local.get 57
          ref.cast (ref 13)
          local.get 58
          i32.const 255
          i32.and
          struct.set 13 9
          local.get 0
          i32.const 1
          local.set 58
          local.tee 57
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 57
          ref.cast (ref 13)
          local.set 57
          local.get 57
          ref.cast (ref 13)
          local.get 58
          i32.const 255
          i32.and
          struct.set 13 10
          local.get 0
          local.set 45
          local.get 45
          call 43
          local.get 51
          global.set 27
          return
        else
          local.get 3
          local.get 6
          local.get 0
          local.tee 57
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 57
          ref.cast (ref 13)
          local.set 57
          local.get 51
          i32.const 16
          i32.add
          local.get 57
          ref.cast (ref 13)
          struct.get 13 5
          f32.store align=1
          local.get 51
          i32.const 20
          i32.add
          local.get 57
          ref.cast (ref 13)
          struct.get 13 6
          f32.store align=1
          local.get 51
          i32.const 16
          i32.add
          local.set 58
          local.set 12
          local.set 11
          local.get 11
          ref.is_null
          local.get 12
          i32.const -1
          i32.eq
          i32.and
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 11
          local.get 11
          ref.is_null
          if (type 59) (result i32) ;; label = @4
            local.get 12
            i32.const 0
            i32.add
          else
            local.get 12
            i32.const 0
            i32.add
          end
          local.set 12
          local.set 11
          local.get 11
          ref.is_null
          local.get 12
          i32.const -1
          i32.eq
          i32.and
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 11
          local.get 11
          ref.is_null
          if (type 59) (result i32) ;; label = @4
            local.get 12
            i32.const 0
            i32.add
          else
            local.get 12
            i32.const 0
            i32.add
          end
          local.get 58
          i32.const 0
          i32.add
          f32.load align=1
          call 139
          local.get 11
          ref.is_null
          local.get 12
          i32.const -1
          i32.eq
          i32.and
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 11
          local.get 11
          ref.is_null
          if (type 59) (result i32) ;; label = @4
            local.get 12
            i32.const 4
            i32.add
          else
            local.get 12
            i32.const 1
            i32.add
          end
          local.get 58
          i32.const 4
          i32.add
          f32.load align=1
          call 139
          local.get 51
          global.set 27
          return
        end
        unreachable
        unreachable
      end
      unreachable
    end
    local.set 46
    local.get 51
    global.set 27
    local.get 46
    throw_ref
  )
  (func (;20;) (type 104) (param eqref i32) (result i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 11
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 11
    i32.const 0
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 3
        local.set 2
        local.get 2
        ref.is_null
        local.get 3
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 2
        local.get 2
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 3
          i32.const 0
          i32.add
        else
          local.get 3
          i32.const 0
          i32.add
        end
        call 144
        i32.const 255
        i32.and
        local.get 11
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 6
    local.get 11
    global.set 27
    local.get 6
    throw_ref
  )
  (func (;21;) (type 105) (param eqref i32 i32) (result i32)
    (local eqref i32 eqref i32 eqref f32 f32 i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 16
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 16
    i32.const 16
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 8
          i32.add
        else
          local.get 4
          i32.const 2
          i32.add
        end
        call 138
        f64.promote_f32
        local.get 0
        local.get 1
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 12
          i32.add
        else
          local.get 4
          i32.const 3
          i32.add
        end
        call 138
        f64.promote_f32
        f32.demote_f64
        local.set 9
        f32.demote_f64
        local.set 8
        local.get 16
        i32.const 0
        i32.add
        i32.const 0
        i32.const 8
        memory.fill
        ref.null eq
        local.set 7
        local.get 16
        i32.const 0
        i32.add
        local.set 10
        local.get 7
        local.get 8
        local.get 9
        local.get 10
        call 16
        local.get 16
        i32.const 0
        i32.add
        local.set 17
        local.get 2
        local.get 17
        i32.const 8
        memory.copy
        local.get 2
        local.get 16
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 11
    local.get 16
    global.set 27
    local.get 11
    throw_ref
  )
  (func (;22;) (type 106) (param eqref i32 i32) (result i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 12
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 12
    i32.const 16
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 4
          i32.add
        else
          local.get 4
          i32.const 1
          i32.add
        end
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 12
        i32.const 0
        i32.add
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
        else
          local.get 4
          i32.const 0
          i32.add
        end
        call 138
        f32.store align=1
        local.get 12
        i32.const 4
        i32.add
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 4
          i32.add
        else
          local.get 4
          i32.const 1
          i32.add
        end
        call 138
        f32.store align=1
        local.get 12
        i32.const 0
        i32.add
        local.set 13
        local.get 2
        local.get 13
        i32.const 8
        memory.copy
        local.get 2
        local.get 12
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 7
    local.get 12
    global.set 27
    local.get 7
    throw_ref
  )
  (func (;23;) (type 107) (param eqref i32 i32) (result i32)
    (local eqref i32 eqref i32 eqref f32 f32 i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 16
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 16
    i32.const 16
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
        else
          local.get 4
          i32.const 0
          i32.add
        end
        call 138
        f64.promote_f32
        local.get 0
        local.get 1
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 4
          i32.add
        else
          local.get 4
          i32.const 1
          i32.add
        end
        call 138
        f64.promote_f32
        f32.demote_f64
        local.set 9
        f32.demote_f64
        local.set 8
        local.get 16
        i32.const 0
        i32.add
        i32.const 0
        i32.const 8
        memory.fill
        ref.null eq
        local.set 7
        local.get 16
        i32.const 0
        i32.add
        local.set 10
        local.get 7
        local.get 8
        local.get 9
        local.get 10
        call 44
        local.get 16
        i32.const 0
        i32.add
        local.set 17
        local.get 2
        local.get 17
        i32.const 8
        memory.copy
        local.get 2
        local.get 16
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 11
    local.get 16
    global.set 27
    local.get 11
    throw_ref
  )
  (func (;24;) (type 108) (param eqref i32 i32 i32 i32 i32 i32 i32 i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 18
    i32.const 1048528
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 18
    i32.const 48
    i32.add
    global.set 27
    local.get 18
    i32.const 0
    i32.add
    local.get 1
    i32.const 8
    memory.copy
    local.get 18
    i32.const 0
    i32.add
    local.set 1
    local.get 18
    i32.const 8
    i32.add
    local.get 2
    i32.const 8
    memory.copy
    local.get 18
    i32.const 8
    i32.add
    local.set 2
    local.get 18
    i32.const 16
    i32.add
    local.get 3
    i32.const 8
    memory.copy
    local.get 18
    i32.const 16
    i32.add
    local.set 3
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 8
        local.get 1
        local.set 19
        local.get 18
        i32.const 24
        i32.add
        local.get 19
        i32.const 8
        memory.copy
        local.get 18
        i32.const 24
        i32.add
        local.set 25
        local.set 10
        local.set 9
        local.get 9
        ref.is_null
        local.get 10
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 9
        local.get 9
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 10
          i32.const 0
          i32.add
        else
          local.get 10
          i32.const 0
          i32.add
        end
        local.set 10
        local.set 9
        local.get 9
        ref.is_null
        local.get 10
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 9
        local.get 9
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 10
          i32.const 0
          i32.add
        else
          local.get 10
          i32.const 0
          i32.add
        end
        local.get 25
        i32.const 0
        i32.add
        f32.load align=1
        call 139
        local.get 9
        ref.is_null
        local.get 10
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 9
        local.get 9
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 10
          i32.const 4
          i32.add
        else
          local.get 10
          i32.const 1
          i32.add
        end
        local.get 25
        i32.const 4
        i32.add
        f32.load align=1
        call 139
        local.get 0
        local.get 8
        local.get 2
        local.set 19
        local.get 18
        i32.const 32
        i32.add
        local.get 19
        i32.const 8
        memory.copy
        local.get 18
        i32.const 32
        i32.add
        local.set 25
        local.set 10
        local.set 9
        local.get 9
        ref.is_null
        local.get 10
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 9
        local.get 9
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 10
          i32.const 8
          i32.add
        else
          local.get 10
          i32.const 2
          i32.add
        end
        local.set 10
        local.set 9
        local.get 9
        ref.is_null
        local.get 10
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 9
        local.get 9
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 10
          i32.const 0
          i32.add
        else
          local.get 10
          i32.const 0
          i32.add
        end
        local.get 25
        i32.const 0
        i32.add
        f32.load align=1
        call 139
        local.get 9
        ref.is_null
        local.get 10
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 9
        local.get 9
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 10
          i32.const 4
          i32.add
        else
          local.get 10
          i32.const 1
          i32.add
        end
        local.get 25
        i32.const 4
        i32.add
        f32.load align=1
        call 139
        local.get 0
        local.get 8
        local.get 3
        local.set 19
        local.get 18
        i32.const 40
        i32.add
        local.get 19
        i32.const 8
        memory.copy
        local.get 18
        i32.const 40
        i32.add
        local.set 25
        local.set 10
        local.set 9
        local.get 9
        ref.is_null
        local.get 10
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 9
        local.get 9
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 10
          i32.const 16
          i32.add
        else
          local.get 10
          i32.const 4
          i32.add
        end
        local.set 10
        local.set 9
        local.get 9
        ref.is_null
        local.get 10
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 9
        local.get 9
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 10
          i32.const 0
          i32.add
        else
          local.get 10
          i32.const 0
          i32.add
        end
        local.get 25
        i32.const 0
        i32.add
        f32.load align=1
        call 139
        local.get 9
        ref.is_null
        local.get 10
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 9
        local.get 9
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 10
          i32.const 4
          i32.add
        else
          local.get 10
          i32.const 1
          i32.add
        end
        local.get 25
        i32.const 4
        i32.add
        f32.load align=1
        call 139
        local.get 0
        local.get 8
        local.get 4
        local.set 25
        local.set 10
        local.set 9
        local.get 9
        ref.is_null
        local.get 10
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 9
        local.get 9
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 10
          i32.const 24
          i32.add
        else
          local.get 10
          i32.const 6
          i32.add
        end
        local.get 25
        i32.const 255
        i32.and
        call 143
        local.get 0
        local.get 8
        local.get 5
        local.set 25
        local.set 10
        local.set 9
        local.get 9
        ref.is_null
        local.get 10
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 9
        local.get 9
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 10
          i32.const 25
          i32.add
        else
          local.get 10
          i32.const 7
          i32.add
        end
        local.get 25
        i32.const 255
        i32.and
        call 143
        local.get 0
        local.get 8
        local.get 6
        local.set 25
        local.set 10
        local.set 9
        local.get 9
        ref.is_null
        local.get 10
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 9
        local.get 9
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 10
          i32.const 26
          i32.add
        else
          local.get 10
          i32.const 8
          i32.add
        end
        local.get 25
        i32.const 255
        i32.and
        call 143
        local.get 0
        local.get 8
        local.get 7
        local.set 25
        local.set 10
        local.set 9
        local.get 9
        ref.is_null
        local.get 10
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 9
        local.get 9
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 10
          i32.const 27
          i32.add
        else
          local.get 10
          i32.const 9
          i32.add
        end
        local.get 25
        i32.const 255
        i32.and
        call 143
        local.get 18
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 13
    local.get 18
    global.set 27
    local.get 13
    throw_ref
  )
  (func (;25;) (type 109) (param eqref eqref eqref eqref i32 i32 i32)
    (local f32 f32 f32 f32 f32 f32 eqref i32 eqref i32 f32 eqref eqref eqref i32 i32 eqref eqref i32 eqref i32 eqref i32 eqref i32 eqref eqref i32 eqref eqref i32 eqref i32 eqref f32 f32 f32 f32 i32 eqref eqref eqref i32 eqref eqref i32 eqref i32 eqref exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 61
    i32.const 1048304
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 61
    i32.const 272
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.get 4
        local.get 2
        local.get 5
        local.set 22
        local.set 20
        local.set 21
        local.set 19
        local.set 18
        local.get 18
        local.get 19
        local.get 20
        local.get 21
        local.get 22
        call 45
        i32.eqz
        if (type 110) ;; label = @3
          local.get 2
          local.get 5
          local.set 25
          local.set 24
          local.get 24
          local.get 25
          local.get 61
          i32.const 16
          i32.add
          call 47
          i32.const 0
          i32.add
          f32.load align=1
          f64.promote_f32
          local.get 2
          local.get 5
          local.set 27
          local.set 26
          local.get 26
          local.get 27
          local.get 61
          i32.const 24
          i32.add
          call 48
          i32.const 0
          i32.add
          f32.load align=1
          f64.promote_f32
          local.get 2
          local.get 5
          local.set 29
          local.set 28
          local.get 28
          local.get 29
          call 49
          f32.convert_i32_s
          f64.promote_f32
          f32.demote_f64
          local.set 17
          f32.demote_f64
          local.get 17
          f32.mul
          f64.promote_f32
          f64.const 0x1p-1 (;=0.5;)
          f32.demote_f64
          f64.promote_f32
          f32.demote_f64
          local.set 17
          f32.demote_f64
          local.get 17
          f32.mul
          f64.promote_f32
          f32.demote_f64
          local.set 17
          f32.demote_f64
          local.get 17
          f32.sub
          f64.promote_f32
          f32.demote_f64
          local.set 7
          local.get 2
          local.get 5
          local.set 25
          local.set 24
          local.get 24
          local.get 25
          local.get 61
          i32.const 32
          i32.add
          call 47
          i32.const 4
          i32.add
          f32.load align=1
          f64.promote_f32
          local.get 2
          local.get 5
          local.set 27
          local.set 26
          local.get 26
          local.get 27
          local.get 61
          i32.const 40
          i32.add
          call 48
          i32.const 4
          i32.add
          f32.load align=1
          f64.promote_f32
          local.get 2
          local.get 5
          local.set 31
          local.set 30
          local.get 30
          local.get 31
          call 50
          f32.convert_i32_s
          f64.promote_f32
          f32.demote_f64
          local.set 17
          f32.demote_f64
          local.get 17
          f32.mul
          f64.promote_f32
          f64.const 0x1p-1 (;=0.5;)
          f32.demote_f64
          f64.promote_f32
          f32.demote_f64
          local.set 17
          f32.demote_f64
          local.get 17
          f32.mul
          f64.promote_f32
          f32.demote_f64
          local.set 17
          f32.demote_f64
          local.get 17
          f32.sub
          f64.promote_f32
          f32.demote_f64
          local.set 8
          local.get 0
          local.set 32
          local.get 32
          call 32
          i32.eqz
          if (type 111) (result f64) ;; label = @4
            local.get 2
            local.get 5
            local.set 34
            local.set 33
            local.get 33
            local.get 34
            local.get 61
            i32.const 56
            i32.add
            call 51
            i32.const 0
            i32.add
            f32.load align=1
            f64.promote_f32
          else
            local.get 2
            local.get 5
            local.set 27
            local.set 26
            local.get 26
            local.get 27
            local.get 61
            i32.const 48
            i32.add
            call 48
            i32.const 0
            i32.add
            f32.load align=1
            f64.promote_f32
          end
          f32.demote_f64
          local.set 9
          local.get 0
          local.set 35
          local.get 35
          call 34
          i32.eqz
          if (type 112) (result f64) ;; label = @4
            local.get 2
            local.get 5
            local.set 34
            local.set 33
            local.get 33
            local.get 34
            local.get 61
            i32.const 72
            i32.add
            call 51
            i32.const 4
            i32.add
            f32.load align=1
            f64.promote_f32
          else
            local.get 2
            local.get 5
            local.set 27
            local.set 26
            local.get 26
            local.get 27
            local.get 61
            i32.const 64
            i32.add
            call 48
            i32.const 4
            i32.add
            f32.load align=1
            f64.promote_f32
          end
          f32.demote_f64
          local.set 10
          local.get 2
          local.get 5
          local.set 37
          local.set 36
          local.get 36
          local.get 37
          call 52
          if (type 113) ;; label = @4
          else
            local.get 0
            local.set 32
            local.get 32
            call 32
            i32.eqz
            if (type 114) ;; label = @5
            else
              local.get 2
              local.get 5
              local.set 34
              local.set 33
              local.get 33
              local.get 34
              local.get 61
              i32.const 80
              i32.add
              call 51
              i32.const 0
              i32.add
              f32.load align=1
              f64.promote_f32
              f32.demote_f64
              f32.abs
              f32.const inf (;=inf;)
              f32.lt
              i32.eqz
              if (type 115) ;; label = @6
              else
                local.get 0
                local.set 32
                local.get 32
                call 32
                i32.const 1
                i32.sub
                local.get 2
                local.get 5
                local.set 29
                local.set 28
                local.get 28
                local.get 29
                call 49
                i32.sub
                f32.convert_i32_s
                f64.promote_f32
                f64.const 0x1p-1 (;=0.5;)
                f32.demote_f64
                f64.promote_f32
                f32.demote_f64
                local.set 17
                f32.demote_f64
                local.get 17
                f32.mul
                f64.promote_f32
                f32.demote_f64
                local.set 11
                local.get 7
                f64.promote_f32
                local.get 2
                local.get 5
                local.set 34
                local.set 33
                local.get 33
                local.get 34
                local.get 61
                i32.const 88
                i32.add
                call 51
                i32.const 0
                i32.add
                f32.load align=1
                f64.promote_f32
                local.get 2
                local.get 5
                local.set 27
                local.set 26
                local.get 26
                local.get 27
                local.get 61
                i32.const 96
                i32.add
                call 48
                i32.const 0
                i32.add
                f32.load align=1
                f64.promote_f32
                f32.demote_f64
                local.set 17
                f32.demote_f64
                local.get 17
                f32.sub
                f64.promote_f32
                local.get 11
                f64.promote_f32
                f32.demote_f64
                local.set 17
                f32.demote_f64
                local.get 17
                f32.mul
                f64.promote_f32
                f32.demote_f64
                local.set 17
                f32.demote_f64
                local.get 17
                f32.add
                f64.promote_f32
                f32.demote_f64
                local.set 7
              end
            end
          end
          local.get 2
          local.get 5
          local.set 39
          local.set 38
          local.get 38
          local.get 39
          call 53
          if (type 116) ;; label = @4
          else
            local.get 0
            local.set 35
            local.get 35
            call 34
            i32.eqz
            if (type 117) ;; label = @5
            else
              local.get 2
              local.get 5
              local.set 34
              local.set 33
              local.get 33
              local.get 34
              local.get 61
              i32.const 104
              i32.add
              call 51
              i32.const 4
              i32.add
              f32.load align=1
              f64.promote_f32
              f32.demote_f64
              f32.abs
              f32.const inf (;=inf;)
              f32.lt
              i32.eqz
              if (type 118) ;; label = @6
              else
                local.get 0
                local.set 35
                local.get 35
                call 34
                i32.const 1
                i32.sub
                local.get 2
                local.get 5
                local.set 31
                local.set 30
                local.get 30
                local.get 31
                call 50
                i32.sub
                f32.convert_i32_s
                f64.promote_f32
                f64.const 0x1p-1 (;=0.5;)
                f32.demote_f64
                f64.promote_f32
                f32.demote_f64
                local.set 17
                f32.demote_f64
                local.get 17
                f32.mul
                f64.promote_f32
                f32.demote_f64
                local.set 12
                local.get 8
                f64.promote_f32
                local.get 2
                local.get 5
                local.set 34
                local.set 33
                local.get 33
                local.get 34
                local.get 61
                i32.const 112
                i32.add
                call 51
                i32.const 4
                i32.add
                f32.load align=1
                f64.promote_f32
                local.get 2
                local.get 5
                local.set 27
                local.set 26
                local.get 26
                local.get 27
                local.get 61
                i32.const 120
                i32.add
                call 48
                i32.const 4
                i32.add
                f32.load align=1
                f64.promote_f32
                f32.demote_f64
                local.set 17
                f32.demote_f64
                local.get 17
                f32.sub
                f64.promote_f32
                local.get 12
                f64.promote_f32
                f32.demote_f64
                local.set 17
                f32.demote_f64
                local.get 17
                f32.mul
                f64.promote_f32
                f32.demote_f64
                local.set 17
                f32.demote_f64
                local.get 17
                f32.add
                f64.promote_f32
                f32.demote_f64
                local.set 8
              end
            end
          end
          local.get 3
          local.get 6
          local.get 7
          f64.promote_f32
          local.get 8
          f64.promote_f32
          local.get 9
          f64.promote_f32
          local.get 10
          f64.promote_f32
          f32.demote_f64
          local.set 44
          f32.demote_f64
          local.set 43
          f32.demote_f64
          local.set 42
          f32.demote_f64
          local.set 41
          local.get 61
          i32.const 128
          i32.add
          i32.const 0
          i32.const 16
          memory.fill
          ref.null eq
          local.set 40
          local.get 61
          i32.const 128
          i32.add
          local.set 45
          local.get 40
          local.get 41
          local.get 42
          local.get 43
          local.get 44
          local.get 45
          call 13
          local.get 61
          i32.const 128
          i32.add
          local.get 0
          local.set 46
          local.get 46
          local.get 61
          i32.const 144
          i32.add
          call 35
          local.get 61
          i32.const 160
          i32.add
          call 54
          local.set 68
          local.set 14
          local.set 13
          local.get 13
          ref.is_null
          local.get 14
          i32.const -1
          i32.eq
          i32.and
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 13
          local.get 13
          ref.is_null
          if (type 59) (result i32) ;; label = @4
            local.get 14
            i32.const 8
            i32.add
          else
            local.get 14
            i32.const 2
            i32.add
          end
          local.set 14
          local.set 13
          local.get 13
          ref.is_null
          local.get 14
          i32.const -1
          i32.eq
          i32.and
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 13
          local.get 13
          ref.is_null
          if (type 59) (result i32) ;; label = @4
            local.get 14
            i32.const 0
            i32.add
          else
            local.get 14
            i32.const 0
            i32.add
          end
          local.get 68
          i32.const 0
          i32.add
          f32.load align=1
          call 139
          local.get 13
          ref.is_null
          local.get 14
          i32.const -1
          i32.eq
          i32.and
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 13
          local.get 13
          ref.is_null
          if (type 59) (result i32) ;; label = @4
            local.get 14
            i32.const 4
            i32.add
          else
            local.get 14
            i32.const 1
            i32.add
          end
          local.get 68
          i32.const 4
          i32.add
          f32.load align=1
          call 139
          local.get 13
          ref.is_null
          local.get 14
          i32.const -1
          i32.eq
          i32.and
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 13
          local.get 13
          ref.is_null
          if (type 59) (result i32) ;; label = @4
            local.get 14
            i32.const 8
            i32.add
          else
            local.get 14
            i32.const 2
            i32.add
          end
          local.get 68
          i32.const 8
          i32.add
          f32.load align=1
          call 139
          local.get 13
          ref.is_null
          local.get 14
          i32.const -1
          i32.eq
          i32.and
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 13
          local.get 13
          ref.is_null
          if (type 59) (result i32) ;; label = @4
            local.get 14
            i32.const 12
            i32.add
          else
            local.get 14
            i32.const 3
            i32.add
          end
          local.get 68
          i32.const 12
          i32.add
          f32.load align=1
          call 139
          local.get 0
          local.set 23
          local.get 23
          local.get 61
          i32.const 176
          i32.add
          call 46
          local.get 3
          local.get 6
          local.set 14
          local.set 13
          local.get 13
          ref.is_null
          local.get 14
          i32.const -1
          i32.eq
          i32.and
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 13
          local.get 13
          ref.is_null
          if (type 59) (result i32) ;; label = @4
            local.get 14
            i32.const 8
            i32.add
          else
            local.get 14
            i32.const 2
            i32.add
          end
          local.set 14
          local.set 13
          local.get 13
          ref.is_null
          local.get 14
          i32.const -1
          i32.eq
          i32.and
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 61
          i32.const 192
          i32.add
          local.get 13
          ref.is_null
          local.get 14
          i32.const -1
          i32.eq
          i32.and
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 13
          local.get 13
          ref.is_null
          if (type 59) (result i32) ;; label = @4
            local.get 14
            i32.const 0
            i32.add
          else
            local.get 14
            i32.const 0
            i32.add
          end
          call 138
          f32.store align=1
          local.get 61
          i32.const 196
          i32.add
          local.get 13
          ref.is_null
          local.get 14
          i32.const -1
          i32.eq
          i32.and
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 13
          local.get 13
          ref.is_null
          if (type 59) (result i32) ;; label = @4
            local.get 14
            i32.const 4
            i32.add
          else
            local.get 14
            i32.const 1
            i32.add
          end
          call 138
          f32.store align=1
          local.get 61
          i32.const 200
          i32.add
          local.get 13
          ref.is_null
          local.get 14
          i32.const -1
          i32.eq
          i32.and
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 13
          local.get 13
          ref.is_null
          if (type 59) (result i32) ;; label = @4
            local.get 14
            i32.const 8
            i32.add
          else
            local.get 14
            i32.const 2
            i32.add
          end
          call 138
          f32.store align=1
          local.get 61
          i32.const 204
          i32.add
          local.get 13
          ref.is_null
          local.get 14
          i32.const -1
          i32.eq
          i32.and
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 13
          local.get 13
          ref.is_null
          if (type 59) (result i32) ;; label = @4
            local.get 14
            i32.const 12
            i32.add
          else
            local.get 14
            i32.const 3
            i32.add
          end
          call 138
          f32.store align=1
          local.get 61
          i32.const 192
          i32.add
          call 55
          i32.eqz
          if (type 119) ;; label = @4
          else
            local.get 0
            local.set 47
            local.get 47
            call 56
          end
          local.get 0
          local.get 3
          local.get 6
          local.set 14
          local.set 13
          local.get 13
          ref.is_null
          local.get 14
          i32.const -1
          i32.eq
          i32.and
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 13
          local.get 13
          ref.is_null
          if (type 59) (result i32) ;; label = @4
            local.get 14
            i32.const 8
            i32.add
          else
            local.get 14
            i32.const 2
            i32.add
          end
          local.set 14
          local.set 13
          local.get 13
          ref.is_null
          local.get 14
          i32.const -1
          i32.eq
          i32.and
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 61
          i32.const 208
          i32.add
          local.get 13
          ref.is_null
          local.get 14
          i32.const -1
          i32.eq
          i32.and
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 13
          local.get 13
          ref.is_null
          if (type 59) (result i32) ;; label = @4
            local.get 14
            i32.const 0
            i32.add
          else
            local.get 14
            i32.const 0
            i32.add
          end
          call 138
          f32.store align=1
          local.get 61
          i32.const 212
          i32.add
          local.get 13
          ref.is_null
          local.get 14
          i32.const -1
          i32.eq
          i32.and
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 13
          local.get 13
          ref.is_null
          if (type 59) (result i32) ;; label = @4
            local.get 14
            i32.const 4
            i32.add
          else
            local.get 14
            i32.const 1
            i32.add
          end
          call 138
          f32.store align=1
          local.get 61
          i32.const 216
          i32.add
          local.get 13
          ref.is_null
          local.get 14
          i32.const -1
          i32.eq
          i32.and
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 13
          local.get 13
          ref.is_null
          if (type 59) (result i32) ;; label = @4
            local.get 14
            i32.const 8
            i32.add
          else
            local.get 14
            i32.const 2
            i32.add
          end
          call 138
          f32.store align=1
          local.get 61
          i32.const 220
          i32.add
          local.get 13
          ref.is_null
          local.get 14
          i32.const -1
          i32.eq
          i32.and
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 13
          local.get 13
          ref.is_null
          if (type 59) (result i32) ;; label = @4
            local.get 14
            i32.const 12
            i32.add
          else
            local.get 14
            i32.const 3
            i32.add
          end
          call 138
          f32.store align=1
          local.get 61
          i32.const 208
          i32.add
          local.set 49
          local.set 48
          local.get 48
          local.get 49
          call 57
          local.get 0
          local.set 50
          local.get 50
          call 58
          local.get 0
          local.get 3
          local.get 6
          local.set 14
          local.set 13
          local.get 13
          ref.is_null
          local.get 14
          i32.const -1
          i32.eq
          i32.and
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 13
          local.get 13
          ref.is_null
          if (type 59) (result i32) ;; label = @4
            local.get 14
            i32.const 8
            i32.add
          else
            local.get 14
            i32.const 2
            i32.add
          end
          local.set 14
          local.set 13
          local.get 13
          ref.is_null
          local.get 14
          i32.const -1
          i32.eq
          i32.and
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 61
          i32.const 224
          i32.add
          local.get 13
          ref.is_null
          local.get 14
          i32.const -1
          i32.eq
          i32.and
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 13
          local.get 13
          ref.is_null
          if (type 59) (result i32) ;; label = @4
            local.get 14
            i32.const 0
            i32.add
          else
            local.get 14
            i32.const 0
            i32.add
          end
          call 138
          f32.store align=1
          local.get 61
          i32.const 228
          i32.add
          local.get 13
          ref.is_null
          local.get 14
          i32.const -1
          i32.eq
          i32.and
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 13
          local.get 13
          ref.is_null
          if (type 59) (result i32) ;; label = @4
            local.get 14
            i32.const 4
            i32.add
          else
            local.get 14
            i32.const 1
            i32.add
          end
          call 138
          f32.store align=1
          local.get 61
          i32.const 232
          i32.add
          local.get 13
          ref.is_null
          local.get 14
          i32.const -1
          i32.eq
          i32.and
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 13
          local.get 13
          ref.is_null
          if (type 59) (result i32) ;; label = @4
            local.get 14
            i32.const 8
            i32.add
          else
            local.get 14
            i32.const 2
            i32.add
          end
          call 138
          f32.store align=1
          local.get 61
          i32.const 236
          i32.add
          local.get 13
          ref.is_null
          local.get 14
          i32.const -1
          i32.eq
          i32.and
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 13
          local.get 13
          ref.is_null
          if (type 59) (result i32) ;; label = @4
            local.get 14
            i32.const 12
            i32.add
          else
            local.get 14
            i32.const 3
            i32.add
          end
          call 138
          f32.store align=1
          local.get 61
          i32.const 224
          i32.add
          local.get 1
          local.get 4
          local.set 52
          local.set 51
          local.get 51
          local.get 52
          call 37
          local.set 55
          local.set 54
          local.set 53
          local.get 53
          local.tee 67
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 67
          ref.cast (ref 13)
          local.get 54
          local.get 55
          local.get 53
          ref.cast (ref 10)
          struct.get 10 0
          ref.cast (ref 9)
          struct.get 9 2
          i32.const 32
          i32.add
          call_indirect (type 199)
          local.get 0
          local.get 2
          local.get 5
          local.set 34
          local.set 33
          local.get 33
          local.get 34
          local.get 61
          i32.const 240
          i32.add
          call 51
          local.set 68
          local.tee 67
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 67
          ref.cast (ref 13)
          local.set 67
          local.get 67
          ref.cast (ref 13)
          local.get 68
          i32.const 0
          i32.add
          f32.load align=1
          struct.set 13 11
          local.get 67
          ref.cast (ref 13)
          local.get 68
          i32.const 4
          i32.add
          f32.load align=1
          struct.set 13 12
          local.get 0
          local.get 2
          local.get 5
          local.set 27
          local.set 26
          local.get 26
          local.get 27
          local.get 61
          i32.const 248
          i32.add
          call 48
          local.set 68
          local.tee 67
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 67
          ref.cast (ref 13)
          local.set 67
          local.get 67
          ref.cast (ref 13)
          local.get 68
          i32.const 0
          i32.add
          f32.load align=1
          struct.set 13 13
          local.get 67
          ref.cast (ref 13)
          local.get 68
          i32.const 4
          i32.add
          f32.load align=1
          struct.set 13 14
          local.get 0
          local.get 2
          local.get 5
          local.set 25
          local.set 24
          local.get 24
          local.get 25
          local.get 61
          i32.const 256
          i32.add
          call 47
          local.set 68
          local.tee 67
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 67
          ref.cast (ref 13)
          local.set 67
          local.get 67
          ref.cast (ref 13)
          local.get 68
          i32.const 0
          i32.add
          f32.load align=1
          struct.set 13 15
          local.get 67
          ref.cast (ref 13)
          local.get 68
          i32.const 4
          i32.add
          f32.load align=1
          struct.set 13 16
          local.get 0
          local.get 2
          local.get 5
          local.set 37
          local.set 36
          local.get 36
          local.get 37
          call 52
          local.set 68
          local.tee 67
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 67
          ref.cast (ref 13)
          local.set 67
          local.get 67
          ref.cast (ref 13)
          local.get 68
          i32.const 255
          i32.and
          struct.set 13 17
          local.get 0
          local.get 2
          local.get 5
          local.set 39
          local.set 38
          local.get 38
          local.get 39
          call 53
          local.set 68
          local.tee 67
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 67
          ref.cast (ref 13)
          local.set 67
          local.get 67
          ref.cast (ref 13)
          local.get 68
          i32.const 255
          i32.and
          struct.set 13 18
          local.get 0
          local.get 2
          local.get 5
          local.set 29
          local.set 28
          local.get 28
          local.get 29
          call 49
          local.set 68
          local.tee 67
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 67
          ref.cast (ref 13)
          local.set 67
          local.get 67
          ref.cast (ref 13)
          local.get 68
          i32.const 255
          i32.and
          struct.set 13 19
          local.get 0
          local.get 2
          local.get 5
          local.set 31
          local.set 30
          local.get 30
          local.get 31
          call 50
          local.set 68
          local.tee 67
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 67
          ref.cast (ref 13)
          local.set 67
          local.get 67
          ref.cast (ref 13)
          local.get 68
          i32.const 255
          i32.and
          struct.set 13 20
          local.get 0
          local.get 1
          local.get 4
          local.set 52
          local.set 51
          local.get 51
          local.get 52
          call 37
          local.set 66
          local.tee 67
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 67
          ref.cast (ref 13)
          local.set 67
          local.get 67
          ref.cast (ref 13)
          local.get 66
          struct.set 13 21
          local.get 0
          i32.const 1
          local.set 68
          local.tee 67
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 67
          ref.cast (ref 13)
          local.set 67
          local.get 67
          ref.cast (ref 13)
          local.get 68
          i32.const 255
          i32.and
          struct.set 13 22
          local.get 61
          global.set 27
          return
        else
          local.get 3
          local.get 6
          local.get 0
          local.set 23
          local.get 23
          local.get 61
          i32.const 0
          i32.add
          call 46
          local.set 68
          local.set 14
          local.set 13
          local.get 13
          ref.is_null
          local.get 14
          i32.const -1
          i32.eq
          i32.and
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 13
          local.get 13
          ref.is_null
          if (type 59) (result i32) ;; label = @4
            local.get 14
            i32.const 8
            i32.add
          else
            local.get 14
            i32.const 2
            i32.add
          end
          local.set 14
          local.set 13
          local.get 13
          ref.is_null
          local.get 14
          i32.const -1
          i32.eq
          i32.and
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 13
          local.get 13
          ref.is_null
          if (type 59) (result i32) ;; label = @4
            local.get 14
            i32.const 0
            i32.add
          else
            local.get 14
            i32.const 0
            i32.add
          end
          local.get 68
          i32.const 0
          i32.add
          f32.load align=1
          call 139
          local.get 13
          ref.is_null
          local.get 14
          i32.const -1
          i32.eq
          i32.and
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 13
          local.get 13
          ref.is_null
          if (type 59) (result i32) ;; label = @4
            local.get 14
            i32.const 4
            i32.add
          else
            local.get 14
            i32.const 1
            i32.add
          end
          local.get 68
          i32.const 4
          i32.add
          f32.load align=1
          call 139
          local.get 13
          ref.is_null
          local.get 14
          i32.const -1
          i32.eq
          i32.and
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 13
          local.get 13
          ref.is_null
          if (type 59) (result i32) ;; label = @4
            local.get 14
            i32.const 8
            i32.add
          else
            local.get 14
            i32.const 2
            i32.add
          end
          local.get 68
          i32.const 8
          i32.add
          f32.load align=1
          call 139
          local.get 13
          ref.is_null
          local.get 14
          i32.const -1
          i32.eq
          i32.and
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 13
          local.get 13
          ref.is_null
          if (type 59) (result i32) ;; label = @4
            local.get 14
            i32.const 12
            i32.add
          else
            local.get 14
            i32.const 3
            i32.add
          end
          local.get 68
          i32.const 12
          i32.add
          f32.load align=1
          call 139
          local.get 61
          global.set 27
          return
        end
        unreachable
        unreachable
      end
      unreachable
    end
    local.set 56
    local.get 61
    global.set 27
    local.get 56
    throw_ref
  )
  (func (;26;) (type 120) (param eqref eqref i32 i32)
    (local i32 i32 i32 eqref i32 i32 i32 i32 eqref i32 eqref i32 eqref i32 eqref eqref eqref i32 i32 eqref i32 eqref eqref i32 i32 eqref i32 eqref i32 eqref eqref eqref eqref eqref eqref eqref eqref eqref eqref exnref exnref exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 50
    i32.const 1048192
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 50
    i32.const 384
    i32.add
    global.set 27
    local.get 50
    i32.const 20
    i32.add
    local.set 4
    local.get 4
    i32.const 0
    i32.const 16
    memory.fill
    local.get 50
    i32.const 36
    i32.add
    local.set 5
    local.get 5
    i32.const 0
    i32.const 20
    memory.fill
    local.get 50
    i32.const 56
    i32.add
    local.set 6
    local.get 6
    i32.const 0
    i32.const 16
    memory.fill
    local.get 50
    i32.const 72
    i32.add
    local.set 9
    local.get 9
    i32.const 0
    i32.const 20
    memory.fill
    local.get 50
    i32.const 92
    i32.add
    local.set 10
    local.get 10
    i32.const 0
    i32.const 20
    memory.fill
    local.get 50
    i32.const 112
    i32.add
    local.set 11
    local.get 11
    i32.const 0
    i32.const 16
    memory.fill
    local.get 50
    i32.const 0
    i32.add
    local.get 2
    i32.const 20
    memory.copy
    local.get 50
    i32.const 0
    i32.add
    local.set 2
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 2
        local.set 51
        local.get 50
        i32.const 128
        i32.add
        local.get 51
        i32.const 20
        memory.copy
        local.get 50
        i32.const 128
        i32.add
        local.set 51
        local.get 9
        local.get 51
        i32.const 20
        memory.copy
        local.get 9
        local.set 13
        ref.null eq
        local.get 13
        local.set 17
        local.set 16
        local.get 16
        local.get 17
        call 59
        if (type 121) ;; label = @3
        else
          local.get 0
          local.set 18
          local.get 18
          call 60
          ref.is_null
          if (type 122) (result i32) ;; label = @4
            local.get 0
            local.set 19
            local.get 19
            local.get 50
            i32.const 168
            i32.add
            call 46
            local.set 21
            local.get 50
            i32.const 184
            i32.add
            i32.const 0
            i32.const 20
            memory.fill
            ref.null eq
            local.set 20
            local.get 50
            i32.const 184
            i32.add
            local.set 22
            local.get 20
            local.get 21
            local.get 22
            call 61
            local.get 50
            i32.const 184
            i32.add
          else
            local.get 10
            local.set 13
            ref.null eq
            local.get 13
            local.set 13
            local.set 12
            local.get 12
            ref.is_null
            local.get 13
            i32.const -1
            i32.eq
            i32.and
            if ;; label = @5
              i32.const 4
              global.get 23
              ref.is_null
              if ;; label = @6
                i32.const 0
                i32.const 53
                array.new_data 0 23
                i32.const 0
                i32.const 53
                call 1
                global.set 23
              end
              global.get 23
              struct.new 3
              throw 0
            end
            local.get 12
            ref.is_null
            local.get 13
            i32.const -1
            i32.eq
            i32.and
            if ;; label = @5
              i32.const 4
              global.get 23
              ref.is_null
              if ;; label = @6
                i32.const 0
                i32.const 53
                array.new_data 0 23
                i32.const 0
                i32.const 53
                call 1
                global.set 23
              end
              global.get 23
              struct.new 3
              throw 0
            end
            local.get 12
            local.get 12
            ref.is_null
            if (type 59) (result i32) ;; label = @5
              local.get 13
              i32.const 0
              i32.add
            else
              local.get 13
              i32.const 0
              i32.add
            end
            i32.const 0
            call 145
            local.get 12
            ref.is_null
            local.get 13
            i32.const -1
            i32.eq
            i32.and
            if ;; label = @5
              i32.const 4
              global.get 23
              ref.is_null
              if ;; label = @6
                i32.const 0
                i32.const 53
                array.new_data 0 23
                i32.const 0
                i32.const 53
                call 1
                global.set 23
              end
              global.get 23
              struct.new 3
              throw 0
            end
            local.get 12
            local.get 12
            ref.is_null
            if (type 59) (result i32) ;; label = @5
              local.get 13
              i32.const 4
              i32.add
            else
              local.get 13
              i32.const 1
              i32.add
            end
            f32.const 0x0p+0 (;=0;)
            call 139
            local.get 12
            ref.is_null
            local.get 13
            i32.const -1
            i32.eq
            i32.and
            if ;; label = @5
              i32.const 4
              global.get 23
              ref.is_null
              if ;; label = @6
                i32.const 0
                i32.const 53
                array.new_data 0 23
                i32.const 0
                i32.const 53
                call 1
                global.set 23
              end
              global.get 23
              struct.new 3
              throw 0
            end
            local.get 12
            local.get 12
            ref.is_null
            if (type 59) (result i32) ;; label = @5
              local.get 13
              i32.const 8
              i32.add
            else
              local.get 13
              i32.const 2
              i32.add
            end
            f32.const 0x0p+0 (;=0;)
            call 139
            local.get 12
            ref.is_null
            local.get 13
            i32.const -1
            i32.eq
            i32.and
            if ;; label = @5
              i32.const 4
              global.get 23
              ref.is_null
              if ;; label = @6
                i32.const 0
                i32.const 53
                array.new_data 0 23
                i32.const 0
                i32.const 53
                call 1
                global.set 23
              end
              global.get 23
              struct.new 3
              throw 0
            end
            local.get 12
            local.get 12
            ref.is_null
            if (type 59) (result i32) ;; label = @5
              local.get 13
              i32.const 12
              i32.add
            else
              local.get 13
              i32.const 3
              i32.add
            end
            f32.const 0x0p+0 (;=0;)
            call 139
            local.get 12
            ref.is_null
            local.get 13
            i32.const -1
            i32.eq
            i32.and
            if ;; label = @5
              i32.const 4
              global.get 23
              ref.is_null
              if ;; label = @6
                i32.const 0
                i32.const 53
                array.new_data 0 23
                i32.const 0
                i32.const 53
                call 1
                global.set 23
              end
              global.get 23
              struct.new 3
              throw 0
            end
            local.get 12
            local.get 12
            ref.is_null
            if (type 59) (result i32) ;; label = @5
              local.get 13
              i32.const 16
              i32.add
            else
              local.get 13
              i32.const 4
              i32.add
            end
            f32.const 0x0p+0 (;=0;)
            call 139
            local.get 10
            local.set 51
            local.get 50
            i32.const 148
            i32.add
            local.get 51
            i32.const 20
            memory.copy
            local.get 50
            i32.const 148
            i32.add
          end
          local.set 51
          local.get 2
          local.get 51
          i32.const 20
          memory.copy
        end
        local.get 2
        local.set 13
        ref.null eq
        local.get 13
        local.set 17
        local.set 16
        local.get 16
        local.get 17
        call 59
        i32.eqz
        if (type 123) ;; label = @3
        else
          local.get 2
          local.set 13
          ref.null eq
          local.get 13
          local.set 24
          local.set 23
          local.get 23
          local.get 24
          local.get 50
          i32.const 204
          i32.add
          call 62
          local.set 51
          local.get 4
          local.get 51
          i32.const 16
          memory.copy
          local.get 0
          local.set 25
          local.get 25
          local.tee 56
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 56
          ref.cast (ref 13)
          local.get 50
          i32.const 220
          i32.add
          local.get 25
          ref.cast (ref 10)
          struct.get 10 0
          ref.cast (ref 9)
          struct.get 9 2
          i32.const 49
          i32.add
          call_indirect (type 202)
          local.set 51
          local.get 11
          local.get 51
          i32.const 16
          memory.copy
          local.get 11
          local.set 13
          ref.null eq
          local.get 13
          local.get 4
          local.set 51
          local.get 50
          i32.const 236
          i32.add
          local.get 51
          i32.const 16
          memory.copy
          local.get 50
          i32.const 236
          i32.add
          local.set 27
          local.set 28
          local.set 26
          local.get 26
          local.get 27
          local.get 28
          call 63
          if (type 124) ;; label = @4
          else
            local.get 50
            global.set 27
            return
          end
        end
        local.get 0
        local.tee 56
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 56
        ref.cast (ref 13)
        local.set 56
        local.get 50
        i32.const 252
        i32.add
        local.get 56
        ref.cast (ref 13)
        struct.get 13 23
        i32.store8
        local.get 50
        i32.const 256
        i32.add
        local.get 56
        ref.cast (ref 13)
        struct.get 13 24
        f32.store align=1
        local.get 50
        i32.const 260
        i32.add
        local.get 56
        ref.cast (ref 13)
        struct.get 13 25
        f32.store align=1
        local.get 50
        i32.const 264
        i32.add
        local.get 56
        ref.cast (ref 13)
        struct.get 13 26
        f32.store align=1
        local.get 50
        i32.const 268
        i32.add
        local.get 56
        ref.cast (ref 13)
        struct.get 13 27
        f32.store align=1
        local.get 50
        i32.const 252
        i32.add
        local.set 51
        local.get 5
        local.get 51
        i32.const 20
        memory.copy
        local.get 0
        local.get 2
        local.set 13
        ref.null eq
        local.get 13
        local.set 17
        local.set 16
        local.get 16
        local.get 17
        call 59
        i32.eqz
        if (type 125) (result i32) ;; label = @3
          local.get 9
          local.set 13
          ref.null eq
          local.get 13
          local.set 13
          local.set 12
          local.get 12
          ref.is_null
          local.get 13
          i32.const -1
          i32.eq
          i32.and
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 12
          ref.is_null
          local.get 13
          i32.const -1
          i32.eq
          i32.and
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 12
          local.get 12
          ref.is_null
          if (type 59) (result i32) ;; label = @4
            local.get 13
            i32.const 0
            i32.add
          else
            local.get 13
            i32.const 0
            i32.add
          end
          i32.const 0
          call 145
          local.get 12
          ref.is_null
          local.get 13
          i32.const -1
          i32.eq
          i32.and
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 12
          local.get 12
          ref.is_null
          if (type 59) (result i32) ;; label = @4
            local.get 13
            i32.const 4
            i32.add
          else
            local.get 13
            i32.const 1
            i32.add
          end
          f32.const 0x0p+0 (;=0;)
          call 139
          local.get 12
          ref.is_null
          local.get 13
          i32.const -1
          i32.eq
          i32.and
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 12
          local.get 12
          ref.is_null
          if (type 59) (result i32) ;; label = @4
            local.get 13
            i32.const 8
            i32.add
          else
            local.get 13
            i32.const 2
            i32.add
          end
          f32.const 0x0p+0 (;=0;)
          call 139
          local.get 12
          ref.is_null
          local.get 13
          i32.const -1
          i32.eq
          i32.and
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 12
          local.get 12
          ref.is_null
          if (type 59) (result i32) ;; label = @4
            local.get 13
            i32.const 12
            i32.add
          else
            local.get 13
            i32.const 3
            i32.add
          end
          f32.const 0x0p+0 (;=0;)
          call 139
          local.get 12
          ref.is_null
          local.get 13
          i32.const -1
          i32.eq
          i32.and
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 12
          local.get 12
          ref.is_null
          if (type 59) (result i32) ;; label = @4
            local.get 13
            i32.const 16
            i32.add
          else
            local.get 13
            i32.const 4
            i32.add
          end
          f32.const 0x0p+0 (;=0;)
          call 139
          local.get 9
          local.set 51
          local.get 50
          i32.const 288
          i32.add
          local.get 51
          i32.const 20
          memory.copy
          local.get 50
          i32.const 288
          i32.add
        else
          local.get 2
          local.set 13
          ref.null eq
          local.get 13
          local.set 24
          local.set 23
          local.get 23
          local.get 24
          local.get 50
          i32.const 272
          i32.add
          call 62
          local.set 51
          local.get 6
          local.get 51
          i32.const 16
          memory.copy
          local.get 0
          local.get 6
          local.set 51
          local.get 50
          i32.const 308
          i32.add
          local.get 51
          i32.const 16
          memory.copy
          local.get 50
          i32.const 308
          i32.add
          local.set 30
          local.set 29
          local.get 29
          local.tee 56
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 56
          ref.cast (ref 13)
          local.get 30
          local.get 50
          i32.const 324
          i32.add
          local.get 29
          ref.cast (ref 10)
          struct.get 10 0
          ref.cast (ref 9)
          struct.get 9 2
          i32.const 52
          i32.add
          call_indirect (type 207)
          local.set 21
          local.get 50
          i32.const 340
          i32.add
          i32.const 0
          i32.const 20
          memory.fill
          ref.null eq
          local.set 20
          local.get 50
          i32.const 340
          i32.add
          local.set 22
          local.get 20
          local.get 21
          local.get 22
          call 61
          local.get 50
          i32.const 340
          i32.add
        end
        local.set 57
        local.tee 56
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 56
        ref.cast (ref 13)
        local.set 56
        local.get 56
        ref.cast (ref 13)
        local.get 57
        i32.const 0
        i32.add
        i32.load8_u
        struct.set 13 23
        local.get 56
        ref.cast (ref 13)
        local.get 57
        i32.const 4
        i32.add
        f32.load align=1
        struct.set 13 24
        local.get 56
        ref.cast (ref 13)
        local.get 57
        i32.const 8
        i32.add
        f32.load align=1
        struct.set 13 25
        local.get 56
        ref.cast (ref 13)
        local.get 57
        i32.const 12
        i32.add
        f32.load align=1
        struct.set 13 26
        local.get 56
        ref.cast (ref 13)
        local.get 57
        i32.const 16
        i32.add
        f32.load align=1
        struct.set 13 27
        local.get 1
        local.get 3
        local.set 32
        local.set 31
        local.get 31
        local.get 32
        call 64
        local.set 7
        local.get 7
        local.set 33
        local.get 33
        local.tee 56
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 56
        ref.cast (ref 10)
        local.get 33
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 0
        i32.add
        call_indirect (type 208)
        local.get 0
        local.tee 56
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 56
        ref.cast (ref 13)
        struct.get 13 30
        i32.const 255
        i32.and
        local.set 8
        local.get 0
        local.get 0
        local.set 34
        local.get 34
        call 65
        local.set 57
        local.tee 56
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 56
        ref.cast (ref 13)
        local.set 56
        local.get 56
        ref.cast (ref 13)
        local.get 57
        i32.const 255
        i32.and
        struct.set 13 30
        block (type 8) ;; label = @3
          block (type 7) (result exnref) ;; label = @4
            try_table (catch_all_ref 0 (;@4;)) ;; label = @5
              block (type 8) ;; label = @6
                block (type 6) (result eqref exnref) ;; label = @7
                  try_table (catch_ref 0 0 (;@7;)) ;; label = @8
                    local.get 0
                    local.set 35
                    local.get 35
                    call 66
                    local.get 0
                    local.get 7
                    local.set 37
                    local.set 36
                    local.get 36
                    local.tee 56
                    ref.is_null
                    if ;; label = @9
                      i32.const 4
                      global.get 23
                      ref.is_null
                      if ;; label = @10
                        i32.const 0
                        i32.const 53
                        array.new_data 0 23
                        i32.const 0
                        i32.const 53
                        call 1
                        global.set 23
                      end
                      global.get 23
                      struct.new 3
                      throw 0
                    end
                    local.get 56
                    ref.cast (ref 13)
                    local.get 37
                    local.get 36
                    ref.cast (ref 10)
                    struct.get 10 0
                    ref.cast (ref 9)
                    struct.get 9 2
                    i32.const 34
                    i32.add
                    call_indirect (type 210)
                    local.get 0
                    local.get 7
                    local.set 39
                    local.set 38
                    local.get 38
                    local.tee 56
                    ref.is_null
                    if ;; label = @9
                      i32.const 4
                      global.get 23
                      ref.is_null
                      if ;; label = @10
                        i32.const 0
                        i32.const 53
                        array.new_data 0 23
                        i32.const 0
                        i32.const 53
                        call 1
                        global.set 23
                      end
                      global.get 23
                      struct.new 3
                      throw 0
                    end
                    local.get 56
                    ref.cast (ref 13)
                    local.get 39
                    local.get 38
                    ref.cast (ref 10)
                    struct.get 10 0
                    ref.cast (ref 9)
                    struct.get 9 2
                    i32.const 33
                    i32.add
                    call_indirect (type 211)
                    local.get 0
                    local.get 7
                    local.set 55
                    local.tee 56
                    ref.is_null
                    if ;; label = @9
                      i32.const 4
                      global.get 23
                      ref.is_null
                      if ;; label = @10
                        i32.const 0
                        i32.const 53
                        array.new_data 0 23
                        i32.const 0
                        i32.const 53
                        call 1
                        global.set 23
                      end
                      global.get 23
                      struct.new 3
                      throw 0
                    end
                    local.get 56
                    ref.cast (ref 13)
                    local.set 56
                    local.get 56
                    ref.cast (ref 13)
                    local.get 55
                    struct.set 13 31
                    br 2 (;@6;)
                  end
                  br 1 (;@6;)
                end
                local.set 43
                local.set 42
                i32.const 1
                if ;; label = @7
                  local.get 42
                  local.tee 42
                  local.get 43
                  local.set 43
                  drop
                  local.get 0
                  local.set 40
                  local.get 40
                  call 56
                  local.get 43
                  throw_ref
                  br 1 (;@6;)
                end
                local.get 43
                throw_ref
              end
              br 2 (;@3;)
            end
            br 1 (;@3;)
          end
          local.set 44
          block (type 8) ;; label = @4
            local.get 0
            local.get 8
            local.set 57
            local.tee 56
            ref.is_null
            if ;; label = @5
              i32.const 4
              global.get 23
              ref.is_null
              if ;; label = @6
                i32.const 0
                i32.const 53
                array.new_data 0 23
                i32.const 0
                i32.const 53
                call 1
                global.set 23
              end
              global.get 23
              struct.new 3
              throw 0
            end
            local.get 56
            ref.cast (ref 13)
            local.set 56
            local.get 56
            ref.cast (ref 13)
            local.get 57
            i32.const 255
            i32.and
            struct.set 13 30
            local.get 7
            local.set 41
            local.get 41
            local.tee 56
            ref.is_null
            if ;; label = @5
              i32.const 4
              global.get 23
              ref.is_null
              if ;; label = @6
                i32.const 0
                i32.const 53
                array.new_data 0 23
                i32.const 0
                i32.const 53
                call 1
                global.set 23
              end
              global.get 23
              struct.new 3
              throw 0
            end
            local.get 56
            ref.cast (ref 10)
            local.get 41
            ref.cast (ref 10)
            struct.get 10 0
            ref.cast (ref 9)
            struct.get 9 2
            i32.const 1
            i32.add
            call_indirect (type 215)
            local.get 0
            local.get 5
            local.set 51
            local.get 50
            i32.const 360
            i32.add
            local.get 51
            i32.const 20
            memory.copy
            local.get 50
            i32.const 360
            i32.add
            local.set 57
            local.tee 56
            ref.is_null
            if ;; label = @5
              i32.const 4
              global.get 23
              ref.is_null
              if ;; label = @6
                i32.const 0
                i32.const 53
                array.new_data 0 23
                i32.const 0
                i32.const 53
                call 1
                global.set 23
              end
              global.get 23
              struct.new 3
              throw 0
            end
            local.get 56
            ref.cast (ref 13)
            local.set 56
            local.get 56
            ref.cast (ref 13)
            local.get 57
            i32.const 0
            i32.add
            i32.load8_u
            struct.set 13 23
            local.get 56
            ref.cast (ref 13)
            local.get 57
            i32.const 4
            i32.add
            f32.load align=1
            struct.set 13 24
            local.get 56
            ref.cast (ref 13)
            local.get 57
            i32.const 8
            i32.add
            f32.load align=1
            struct.set 13 25
            local.get 56
            ref.cast (ref 13)
            local.get 57
            i32.const 12
            i32.add
            f32.load align=1
            struct.set 13 26
            local.get 56
            ref.cast (ref 13)
            local.get 57
            i32.const 16
            i32.add
            f32.load align=1
            struct.set 13 27
            br 0 (;@4;)
          end
          local.get 44
          throw_ref
        end
        block (type 8) ;; label = @3
          local.get 0
          local.get 8
          local.set 57
          local.tee 56
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 56
          ref.cast (ref 13)
          local.set 56
          local.get 56
          ref.cast (ref 13)
          local.get 57
          i32.const 255
          i32.and
          struct.set 13 30
          local.get 7
          local.set 41
          local.get 41
          local.tee 56
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 56
          ref.cast (ref 10)
          local.get 41
          ref.cast (ref 10)
          struct.get 10 0
          ref.cast (ref 9)
          struct.get 9 2
          i32.const 1
          i32.add
          call_indirect (type 215)
          local.get 0
          local.get 5
          local.set 51
          local.get 50
          i32.const 360
          i32.add
          local.get 51
          i32.const 20
          memory.copy
          local.get 50
          i32.const 360
          i32.add
          local.set 57
          local.tee 56
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 56
          ref.cast (ref 13)
          local.set 56
          local.get 56
          ref.cast (ref 13)
          local.get 57
          i32.const 0
          i32.add
          i32.load8_u
          struct.set 13 23
          local.get 56
          ref.cast (ref 13)
          local.get 57
          i32.const 4
          i32.add
          f32.load align=1
          struct.set 13 24
          local.get 56
          ref.cast (ref 13)
          local.get 57
          i32.const 8
          i32.add
          f32.load align=1
          struct.set 13 25
          local.get 56
          ref.cast (ref 13)
          local.get 57
          i32.const 12
          i32.add
          f32.load align=1
          struct.set 13 26
          local.get 56
          ref.cast (ref 13)
          local.get 57
          i32.const 16
          i32.add
          f32.load align=1
          struct.set 13 27
          br 0 (;@3;)
        end
        local.get 50
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 45
    local.get 50
    global.set 27
    local.get 45
    throw_ref
  )
  (func (;27;) (type 126) (param i32 i32) (result i32)
    (local eqref i32 eqref i32 eqref f32 f32 f32 f32 i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 17
    i32.const 1048512
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 17
    i32.const 64
    i32.add
    global.set 27
    local.get 17
    i32.const 0
    i32.add
    local.get 0
    i32.const 8
    memory.copy
    local.get 17
    i32.const 0
    i32.add
    local.set 0
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.set 18
        local.get 17
        i32.const 8
        i32.add
        local.get 18
        i32.const 8
        memory.copy
        local.get 17
        i32.const 8
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 0
        local.set 18
        local.get 17
        i32.const 16
        i32.add
        local.get 18
        i32.const 8
        memory.copy
        local.get 17
        i32.const 16
        i32.add
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 0
        local.set 18
        local.get 17
        i32.const 24
        i32.add
        local.get 18
        i32.const 8
        memory.copy
        local.get 17
        i32.const 24
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 0
        local.set 18
        local.get 17
        i32.const 32
        i32.add
        local.get 18
        i32.const 8
        memory.copy
        local.get 17
        i32.const 32
        i32.add
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 10
        f32.demote_f64
        local.set 9
        f32.demote_f64
        local.set 8
        f32.demote_f64
        local.set 7
        local.get 17
        i32.const 40
        i32.add
        i32.const 0
        i32.const 16
        memory.fill
        ref.null eq
        local.set 6
        local.get 17
        i32.const 40
        i32.add
        local.set 11
        local.get 6
        local.get 7
        local.get 8
        local.get 9
        local.get 10
        local.get 11
        call 67
        local.get 17
        i32.const 40
        i32.add
        local.set 18
        local.get 1
        local.get 18
        i32.const 16
        memory.copy
        local.get 1
        local.get 17
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 12
    local.get 17
    global.set 27
    local.get 12
    throw_ref
  )
  (func (;28;) (type 127) (param eqref eqref eqref i32 i32) (result i32)
    (local eqref i32 eqref i32 eqref eqref i32 eqref i32 eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 23
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 23
    i32.const 16
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.tee 29
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 29
        ref.cast (ref 13)
        struct.get 13 10
        i32.eqz
        if (type 128) ;; label = @3
        else
          local.get 0
          local.set 9
          local.get 9
          call 68
          i32.const 4
          i32.and
          if (type 129) ;; label = @4
          else
            local.get 0
            local.tee 29
            ref.is_null
            if ;; label = @5
              i32.const 4
              global.get 23
              ref.is_null
              if ;; label = @6
                i32.const 0
                i32.const 53
                array.new_data 0 23
                i32.const 0
                i32.const 53
                call 1
                global.set 23
              end
              global.get 23
              struct.new 3
              throw 0
            end
            local.get 29
            ref.cast (ref 13)
            local.set 29
            local.get 23
            i32.const 0
            i32.add
            local.get 29
            ref.cast (ref 13)
            struct.get 13 3
            f32.store align=1
            local.get 23
            i32.const 4
            i32.add
            local.get 29
            ref.cast (ref 13)
            struct.get 13 4
            f32.store align=1
            local.get 23
            i32.const 0
            i32.add
            local.get 2
            local.get 4
            local.set 11
            local.set 10
            local.get 10
            local.get 11
            local.get 23
            i32.const 8
            i32.add
            call 29
            call 69
            i32.eqz
            if (type 130) ;; label = @5
            else
              local.get 0
              local.tee 29
              ref.is_null
              if ;; label = @6
                i32.const 4
                global.get 23
                ref.is_null
                if ;; label = @7
                  i32.const 0
                  i32.const 53
                  array.new_data 0 23
                  i32.const 0
                  i32.const 53
                  call 1
                  global.set 23
                end
                global.get 23
                struct.new 3
                throw 0
              end
              local.get 29
              ref.cast (ref 13)
              struct.get 13 8
              local.get 2
              local.get 4
              local.set 13
              local.set 12
              local.get 12
              local.get 13
              call 31
              i32.eq
              i32.eqz
              if (type 131) ;; label = @6
              else
                local.get 0
                local.tee 29
                ref.is_null
                if ;; label = @7
                  i32.const 4
                  global.get 23
                  ref.is_null
                  if ;; label = @8
                    i32.const 0
                    i32.const 53
                    array.new_data 0 23
                    i32.const 0
                    i32.const 53
                    call 1
                    global.set 23
                  end
                  global.get 23
                  struct.new 3
                  throw 0
                end
                local.get 29
                ref.cast (ref 13)
                struct.get 13 9
                local.get 2
                local.get 4
                local.set 15
                local.set 14
                local.get 14
                local.get 15
                call 33
                i32.eq
                i32.eqz
                if (type 132) ;; label = @7
                else
                  local.get 0
                  local.tee 29
                  ref.is_null
                  if ;; label = @8
                    i32.const 4
                    global.get 23
                    ref.is_null
                    if ;; label = @9
                      i32.const 0
                      i32.const 53
                      array.new_data 0 23
                      i32.const 0
                      i32.const 53
                      call 1
                      global.set 23
                    end
                    global.get 23
                    struct.new 3
                    throw 0
                  end
                  local.get 29
                  ref.cast (ref 13)
                  struct.get 13 7
                  local.get 1
                  local.get 3
                  local.set 17
                  local.set 16
                  local.get 16
                  local.get 17
                  call 37
                  ref.eq
                  i32.const 255
                  i32.and
                  local.get 23
                  global.set 27
                  return
                end
              end
            end
          end
        end
        i32.const 0
        i32.const 255
        i32.and
        local.get 23
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 18
    local.get 23
    global.set 27
    local.get 18
    throw_ref
  )
  (func (;29;) (type 133) (param eqref i32 i32) (result i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 12
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 12
    i32.const 16
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
        else
          local.get 4
          i32.const 0
          i32.add
        end
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 12
        i32.const 0
        i32.add
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
        else
          local.get 4
          i32.const 0
          i32.add
        end
        call 138
        f32.store align=1
        local.get 12
        i32.const 4
        i32.add
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 4
          i32.add
        else
          local.get 4
          i32.const 1
          i32.add
        end
        call 138
        f32.store align=1
        local.get 12
        i32.const 0
        i32.add
        local.set 13
        local.get 2
        local.get 13
        i32.const 8
        memory.copy
        local.get 2
        local.get 12
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 7
    local.get 12
    global.set 27
    local.get 7
    throw_ref
  )
  (func (;30;) (type 134) (param i32 i32 i32) (result i32)
    (local eqref i32 eqref i32 f32 eqref f32 f32 i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 17
    i32.const 1048512
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 17
    i32.const 64
    i32.add
    global.set 27
    local.get 17
    i32.const 0
    i32.add
    local.get 0
    i32.const 8
    memory.copy
    local.get 17
    i32.const 0
    i32.add
    local.set 0
    local.get 17
    i32.const 8
    i32.add
    local.get 1
    i32.const 8
    memory.copy
    local.get 17
    i32.const 8
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.set 18
        local.get 17
        i32.const 16
        i32.add
        local.get 18
        i32.const 8
        memory.copy
        local.get 17
        i32.const 16
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 18
        local.get 17
        i32.const 24
        i32.add
        local.get 18
        i32.const 8
        memory.copy
        local.get 17
        i32.const 24
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 7
        f32.demote_f64
        local.get 7
        f32.max
        f64.promote_f32
        local.get 0
        local.set 18
        local.get 17
        i32.const 32
        i32.add
        local.get 18
        i32.const 8
        memory.copy
        local.get 17
        i32.const 32
        i32.add
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 18
        local.get 17
        i32.const 40
        i32.add
        local.get 18
        i32.const 8
        memory.copy
        local.get 17
        i32.const 40
        i32.add
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 7
        f32.demote_f64
        local.get 7
        f32.max
        f64.promote_f32
        f32.demote_f64
        local.set 10
        f32.demote_f64
        local.set 9
        local.get 17
        i32.const 48
        i32.add
        i32.const 0
        i32.const 8
        memory.fill
        ref.null eq
        local.set 8
        local.get 17
        i32.const 48
        i32.add
        local.set 11
        local.get 8
        local.get 9
        local.get 10
        local.get 11
        call 5
        local.get 17
        i32.const 48
        i32.add
        local.get 17
        i32.const 56
        i32.add
        call 6
        local.set 18
        local.get 2
        local.get 18
        i32.const 8
        memory.copy
        local.get 2
        local.get 17
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 12
    local.get 17
    global.set 27
    local.get 12
    throw_ref
  )
  (func (;31;) (type 135) (param eqref i32) (result i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 11
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 11
    i32.const 0
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 3
        local.set 2
        local.get 2
        ref.is_null
        local.get 3
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 2
        local.get 2
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 3
          i32.const 8
          i32.add
        else
          local.get 3
          i32.const 2
          i32.add
        end
        call 142
        i32.const 255
        i32.and
        local.get 11
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 6
    local.get 11
    global.set 27
    local.get 6
    throw_ref
  )
  (func (;32;) (type 136) (param eqref) (result i32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.tee 2
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 23
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 23
        i32.const 0
        i32.const 53
        call 1
        global.set 23
      end
      global.get 23
      struct.new 3
      throw 0
    end
    local.get 2
    ref.cast (ref 13)
    struct.get 13 46
    i32.const 255
    i32.and
    return
  )
  (func (;33;) (type 137) (param eqref i32) (result i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 11
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 11
    i32.const 0
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 3
        local.set 2
        local.get 2
        ref.is_null
        local.get 3
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 2
        local.get 2
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 3
          i32.const 9
          i32.add
        else
          local.get 3
          i32.const 3
          i32.add
        end
        call 142
        i32.const 255
        i32.and
        local.get 11
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 6
    local.get 11
    global.set 27
    local.get 6
    throw_ref
  )
  (func (;34;) (type 138) (param eqref) (result i32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.tee 2
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 23
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 23
        i32.const 0
        i32.const 53
        call 1
        global.set 23
      end
      global.get 23
      struct.new 3
      throw 0
    end
    local.get 2
    ref.cast (ref 13)
    struct.get 13 47
    i32.const 255
    i32.and
    return
  )
  (func (;35;) (type 139) (param eqref i32) (result i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 7
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 7
    i32.const 16
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.tee 13
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 13
        ref.cast (ref 13)
        local.set 13
        local.get 7
        i32.const 0
        i32.add
        local.get 13
        ref.cast (ref 13)
        struct.get 13 42
        f32.store align=1
        local.get 7
        i32.const 4
        i32.add
        local.get 13
        ref.cast (ref 13)
        struct.get 13 43
        f32.store align=1
        local.get 7
        i32.const 8
        i32.add
        local.get 13
        ref.cast (ref 13)
        struct.get 13 44
        f32.store align=1
        local.get 7
        i32.const 12
        i32.add
        local.get 13
        ref.cast (ref 13)
        struct.get 13 45
        f32.store align=1
        local.get 7
        i32.const 0
        i32.add
        local.set 8
        local.get 1
        local.get 8
        i32.const 16
        memory.copy
        local.get 1
        local.get 7
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 2
    local.get 7
    global.set 27
    local.get 2
    throw_ref
  )
  (func (;36;) (type 140) (param i32 i32 i32) (result i32)
    (local eqref i32 eqref i32 f32 eqref f32 f32 i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 17
    i32.const 1048448
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 17
    i32.const 128
    i32.add
    global.set 27
    local.get 17
    i32.const 0
    i32.add
    local.get 0
    i32.const 8
    memory.copy
    local.get 17
    i32.const 0
    i32.add
    local.set 0
    local.get 17
    i32.const 8
    i32.add
    local.get 1
    i32.const 16
    memory.copy
    local.get 17
    i32.const 8
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.set 18
        local.get 17
        i32.const 24
        i32.add
        local.get 18
        i32.const 8
        memory.copy
        local.get 17
        i32.const 24
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 18
        local.get 17
        i32.const 32
        i32.add
        local.get 18
        i32.const 16
        memory.copy
        local.get 17
        i32.const 32
        i32.add
        i32.const 12
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 7
        f32.demote_f64
        local.get 7
        f32.sub
        f64.promote_f32
        local.get 1
        local.set 18
        local.get 17
        i32.const 48
        i32.add
        local.get 18
        i32.const 16
        memory.copy
        local.get 17
        i32.const 48
        i32.add
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 7
        f32.demote_f64
        local.get 7
        f32.sub
        f64.promote_f32
        local.get 0
        local.set 18
        local.get 17
        i32.const 64
        i32.add
        local.get 18
        i32.const 8
        memory.copy
        local.get 17
        i32.const 64
        i32.add
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 18
        local.get 17
        i32.const 72
        i32.add
        local.get 18
        i32.const 16
        memory.copy
        local.get 17
        i32.const 72
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 7
        f32.demote_f64
        local.get 7
        f32.sub
        f64.promote_f32
        local.get 1
        local.set 18
        local.get 17
        i32.const 88
        i32.add
        local.get 18
        i32.const 16
        memory.copy
        local.get 17
        i32.const 88
        i32.add
        i32.const 8
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 7
        f32.demote_f64
        local.get 7
        f32.sub
        f64.promote_f32
        f32.demote_f64
        local.set 10
        f32.demote_f64
        local.set 9
        local.get 17
        i32.const 104
        i32.add
        i32.const 0
        i32.const 8
        memory.fill
        ref.null eq
        local.set 8
        local.get 17
        i32.const 104
        i32.add
        local.set 11
        local.get 8
        local.get 9
        local.get 10
        local.get 11
        call 5
        local.get 17
        i32.const 104
        i32.add
        local.get 17
        i32.const 112
        i32.add
        call 6
        local.set 18
        local.get 2
        local.get 18
        i32.const 8
        memory.copy
        local.get 2
        local.get 17
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 12
    local.get 17
    global.set 27
    local.get 12
    throw_ref
  )
  (func (;37;) (type 141) (param eqref i32) (result eqref)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 11
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 11
    i32.const 0
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 3
        local.set 2
        local.get 2
        ref.is_null
        local.get 3
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 2
        local.get 2
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 3
          i32.const 16
          i32.add
          i32.load align=1
        else
          local.get 3
          i32.const 2
          i32.add
        end
        call 134
        local.get 11
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 6
    local.get 11
    global.set 27
    local.get 6
    throw_ref
  )
  (func (;38;) (type 142) (param eqref) (result f32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.tee 2
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 23
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 23
        i32.const 0
        i32.const 53
        call 1
        global.set 23
      end
      global.get 23
      struct.new 3
      throw 0
    end
    local.get 2
    ref.cast (ref 13)
    struct.get 13 51
    f64.promote_f32
    f32.demote_f64
    return
  )
  (func (;39;) (type 143) (param eqref) (result f32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.tee 2
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 23
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 23
        i32.const 0
        i32.const 53
        call 1
        global.set 23
      end
      global.get 23
      struct.new 3
      throw 0
    end
    local.get 2
    ref.cast (ref 13)
    struct.get 13 53
    f64.promote_f32
    f32.demote_f64
    return
  )
  (func (;40;) (type 144) (param eqref) (result f32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.tee 2
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 23
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 23
        i32.const 0
        i32.const 53
        call 1
        global.set 23
      end
      global.get 23
      struct.new 3
      throw 0
    end
    local.get 2
    ref.cast (ref 13)
    struct.get 13 52
    f64.promote_f32
    f32.demote_f64
    return
  )
  (func (;41;) (type 145) (param eqref) (result f32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.tee 2
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 23
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 23
        i32.const 0
        i32.const 53
        call 1
        global.set 23
      end
      global.get 23
      struct.new 3
      throw 0
    end
    local.get 2
    ref.cast (ref 13)
    struct.get 13 54
    f64.promote_f32
    f32.demote_f64
    return
  )
  (func (;42;) (type 146) (param i32 i32 i32) (result i32)
    (local eqref i32 eqref i32 f32 eqref f32 f32 i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 17
    i32.const 1048448
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 17
    i32.const 128
    i32.add
    global.set 27
    local.get 17
    i32.const 0
    i32.add
    local.get 0
    i32.const 8
    memory.copy
    local.get 17
    i32.const 0
    i32.add
    local.set 0
    local.get 17
    i32.const 8
    i32.add
    local.get 1
    i32.const 16
    memory.copy
    local.get 17
    i32.const 8
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.set 18
        local.get 17
        i32.const 24
        i32.add
        local.get 18
        i32.const 8
        memory.copy
        local.get 17
        i32.const 24
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 18
        local.get 17
        i32.const 32
        i32.add
        local.get 18
        i32.const 16
        memory.copy
        local.get 17
        i32.const 32
        i32.add
        i32.const 12
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 7
        f32.demote_f64
        local.get 7
        f32.add
        f64.promote_f32
        local.get 1
        local.set 18
        local.get 17
        i32.const 48
        i32.add
        local.get 18
        i32.const 16
        memory.copy
        local.get 17
        i32.const 48
        i32.add
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 7
        f32.demote_f64
        local.get 7
        f32.add
        f64.promote_f32
        local.get 0
        local.set 18
        local.get 17
        i32.const 64
        i32.add
        local.get 18
        i32.const 8
        memory.copy
        local.get 17
        i32.const 64
        i32.add
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 18
        local.get 17
        i32.const 72
        i32.add
        local.get 18
        i32.const 16
        memory.copy
        local.get 17
        i32.const 72
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 7
        f32.demote_f64
        local.get 7
        f32.add
        f64.promote_f32
        local.get 1
        local.set 18
        local.get 17
        i32.const 88
        i32.add
        local.get 18
        i32.const 16
        memory.copy
        local.get 17
        i32.const 88
        i32.add
        i32.const 8
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 7
        f32.demote_f64
        local.get 7
        f32.add
        f64.promote_f32
        f32.demote_f64
        local.set 10
        f32.demote_f64
        local.set 9
        local.get 17
        i32.const 104
        i32.add
        i32.const 0
        i32.const 8
        memory.fill
        ref.null eq
        local.set 8
        local.get 17
        i32.const 104
        i32.add
        local.set 11
        local.get 8
        local.get 9
        local.get 10
        local.get 11
        call 5
        local.get 17
        i32.const 104
        i32.add
        local.get 17
        i32.const 112
        i32.add
        call 6
        local.set 18
        local.get 2
        local.get 18
        i32.const 8
        memory.copy
        local.get 2
        local.get 17
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 12
    local.get 17
    global.set 27
    local.get 12
    throw_ref
  )
  (func (;43;) (type 147) (param eqref)
    (local eqref eqref i32 eqref eqref i32 i64 f64 externref)
    local.get 0
    local.get 0
    local.set 1
    local.get 1
    call 68
    i32.const -5
    i32.and
    local.set 3
    local.set 2
    local.get 2
    local.get 3
    call 71
    return
  )
  (func (;44;) (type 148) (param eqref f32 f32 i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 13
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 13
    i32.const 0
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 3
        local.get 1
        f64.promote_f32
        local.set 22
        local.set 5
        local.set 4
        local.get 4
        ref.is_null
        local.get 5
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 4
        local.get 4
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 5
          i32.const 0
          i32.add
        else
          local.get 5
          i32.const 0
          i32.add
        end
        local.get 22
        f32.demote_f64
        call 139
        local.get 0
        local.get 3
        local.get 2
        f64.promote_f32
        local.set 22
        local.set 5
        local.set 4
        local.get 4
        ref.is_null
        local.get 5
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 4
        local.get 4
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 5
          i32.const 4
          i32.add
        else
          local.get 5
          i32.const 1
          i32.add
        end
        local.get 22
        f32.demote_f64
        call 139
        local.get 13
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 8
    local.get 13
    global.set 27
    local.get 8
    throw_ref
  )
  (func (;45;) (type 149) (param eqref eqref eqref i32 i32) (result i32)
    (local eqref i32 eqref i32 eqref eqref i32 eqref i32 eqref i32 eqref i32 eqref i32 eqref i32 eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 31
    i32.const 1048528
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 31
    i32.const 48
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.tee 37
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 37
        ref.cast (ref 13)
        struct.get 13 22
        i32.eqz
        if (type 150) ;; label = @3
        else
          local.get 0
          local.set 9
          local.get 9
          call 68
          i32.const 24
          i32.and
          if (type 151) ;; label = @4
          else
            local.get 0
            local.tee 37
            ref.is_null
            if ;; label = @5
              i32.const 4
              global.get 23
              ref.is_null
              if ;; label = @6
                i32.const 0
                i32.const 53
                array.new_data 0 23
                i32.const 0
                i32.const 53
                call 1
                global.set 23
              end
              global.get 23
              struct.new 3
              throw 0
            end
            local.get 37
            ref.cast (ref 13)
            local.set 37
            local.get 31
            i32.const 0
            i32.add
            local.get 37
            ref.cast (ref 13)
            struct.get 13 11
            f32.store align=1
            local.get 31
            i32.const 4
            i32.add
            local.get 37
            ref.cast (ref 13)
            struct.get 13 12
            f32.store align=1
            local.get 31
            i32.const 0
            i32.add
            local.get 2
            local.get 4
            local.set 11
            local.set 10
            local.get 10
            local.get 11
            local.get 31
            i32.const 8
            i32.add
            call 51
            call 69
            i32.eqz
            if (type 152) ;; label = @5
            else
              local.get 0
              local.tee 37
              ref.is_null
              if ;; label = @6
                i32.const 4
                global.get 23
                ref.is_null
                if ;; label = @7
                  i32.const 0
                  i32.const 53
                  array.new_data 0 23
                  i32.const 0
                  i32.const 53
                  call 1
                  global.set 23
                end
                global.get 23
                struct.new 3
                throw 0
              end
              local.get 37
              ref.cast (ref 13)
              local.set 37
              local.get 31
              i32.const 16
              i32.add
              local.get 37
              ref.cast (ref 13)
              struct.get 13 13
              f32.store align=1
              local.get 31
              i32.const 20
              i32.add
              local.get 37
              ref.cast (ref 13)
              struct.get 13 14
              f32.store align=1
              local.get 31
              i32.const 16
              i32.add
              local.get 2
              local.get 4
              local.set 13
              local.set 12
              local.get 12
              local.get 13
              local.get 31
              i32.const 24
              i32.add
              call 48
              call 69
              i32.eqz
              if (type 153) ;; label = @6
              else
                local.get 0
                local.tee 37
                ref.is_null
                if ;; label = @7
                  i32.const 4
                  global.get 23
                  ref.is_null
                  if ;; label = @8
                    i32.const 0
                    i32.const 53
                    array.new_data 0 23
                    i32.const 0
                    i32.const 53
                    call 1
                    global.set 23
                  end
                  global.get 23
                  struct.new 3
                  throw 0
                end
                local.get 37
                ref.cast (ref 13)
                local.set 37
                local.get 31
                i32.const 32
                i32.add
                local.get 37
                ref.cast (ref 13)
                struct.get 13 15
                f32.store align=1
                local.get 31
                i32.const 36
                i32.add
                local.get 37
                ref.cast (ref 13)
                struct.get 13 16
                f32.store align=1
                local.get 31
                i32.const 32
                i32.add
                local.get 2
                local.get 4
                local.set 15
                local.set 14
                local.get 14
                local.get 15
                local.get 31
                i32.const 40
                i32.add
                call 47
                call 72
                i32.eqz
                if (type 154) ;; label = @7
                else
                  local.get 0
                  local.tee 37
                  ref.is_null
                  if ;; label = @8
                    i32.const 4
                    global.get 23
                    ref.is_null
                    if ;; label = @9
                      i32.const 0
                      i32.const 53
                      array.new_data 0 23
                      i32.const 0
                      i32.const 53
                      call 1
                      global.set 23
                    end
                    global.get 23
                    struct.new 3
                    throw 0
                  end
                  local.get 37
                  ref.cast (ref 13)
                  struct.get 13 17
                  local.get 2
                  local.get 4
                  local.set 17
                  local.set 16
                  local.get 16
                  local.get 17
                  call 52
                  i32.eq
                  i32.eqz
                  if (type 155) ;; label = @8
                  else
                    local.get 0
                    local.tee 37
                    ref.is_null
                    if ;; label = @9
                      i32.const 4
                      global.get 23
                      ref.is_null
                      if ;; label = @10
                        i32.const 0
                        i32.const 53
                        array.new_data 0 23
                        i32.const 0
                        i32.const 53
                        call 1
                        global.set 23
                      end
                      global.get 23
                      struct.new 3
                      throw 0
                    end
                    local.get 37
                    ref.cast (ref 13)
                    struct.get 13 18
                    local.get 2
                    local.get 4
                    local.set 19
                    local.set 18
                    local.get 18
                    local.get 19
                    call 53
                    i32.eq
                    i32.eqz
                    if (type 156) ;; label = @9
                    else
                      local.get 0
                      local.tee 37
                      ref.is_null
                      if ;; label = @10
                        i32.const 4
                        global.get 23
                        ref.is_null
                        if ;; label = @11
                          i32.const 0
                          i32.const 53
                          array.new_data 0 23
                          i32.const 0
                          i32.const 53
                          call 1
                          global.set 23
                        end
                        global.get 23
                        struct.new 3
                        throw 0
                      end
                      local.get 37
                      ref.cast (ref 13)
                      struct.get 13 19
                      local.get 2
                      local.get 4
                      local.set 21
                      local.set 20
                      local.get 20
                      local.get 21
                      call 49
                      i32.eq
                      i32.eqz
                      if (type 157) ;; label = @10
                      else
                        local.get 0
                        local.tee 37
                        ref.is_null
                        if ;; label = @11
                          i32.const 4
                          global.get 23
                          ref.is_null
                          if ;; label = @12
                            i32.const 0
                            i32.const 53
                            array.new_data 0 23
                            i32.const 0
                            i32.const 53
                            call 1
                            global.set 23
                          end
                          global.get 23
                          struct.new 3
                          throw 0
                        end
                        local.get 37
                        ref.cast (ref 13)
                        struct.get 13 20
                        local.get 2
                        local.get 4
                        local.set 23
                        local.set 22
                        local.get 22
                        local.get 23
                        call 50
                        i32.eq
                        i32.eqz
                        if (type 158) ;; label = @11
                        else
                          local.get 0
                          local.tee 37
                          ref.is_null
                          if ;; label = @12
                            i32.const 4
                            global.get 23
                            ref.is_null
                            if ;; label = @13
                              i32.const 0
                              i32.const 53
                              array.new_data 0 23
                              i32.const 0
                              i32.const 53
                              call 1
                              global.set 23
                            end
                            global.get 23
                            struct.new 3
                            throw 0
                          end
                          local.get 37
                          ref.cast (ref 13)
                          struct.get 13 21
                          local.get 1
                          local.get 3
                          local.set 25
                          local.set 24
                          local.get 24
                          local.get 25
                          call 37
                          ref.eq
                          i32.const 255
                          i32.and
                          local.get 31
                          global.set 27
                          return
                        end
                      end
                    end
                  end
                end
              end
            end
          end
        end
        i32.const 0
        i32.const 255
        i32.and
        local.get 31
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 26
    local.get 31
    global.set 27
    local.get 26
    throw_ref
  )
  (func (;46;) (type 159) (param eqref i32) (result i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 7
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 7
    i32.const 16
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.tee 13
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 13
        ref.cast (ref 13)
        local.set 13
        local.get 7
        i32.const 0
        i32.add
        local.get 13
        ref.cast (ref 13)
        struct.get 13 38
        f32.store align=1
        local.get 7
        i32.const 4
        i32.add
        local.get 13
        ref.cast (ref 13)
        struct.get 13 39
        f32.store align=1
        local.get 7
        i32.const 8
        i32.add
        local.get 13
        ref.cast (ref 13)
        struct.get 13 40
        f32.store align=1
        local.get 7
        i32.const 12
        i32.add
        local.get 13
        ref.cast (ref 13)
        struct.get 13 41
        f32.store align=1
        local.get 7
        i32.const 0
        i32.add
        local.set 8
        local.get 1
        local.get 8
        i32.const 16
        memory.copy
        local.get 1
        local.get 7
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 2
    local.get 7
    global.set 27
    local.get 2
    throw_ref
  )
  (func (;47;) (type 160) (param eqref i32 i32) (result i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 12
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 12
    i32.const 16
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 16
          i32.add
        else
          local.get 4
          i32.const 4
          i32.add
        end
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 12
        i32.const 0
        i32.add
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
        else
          local.get 4
          i32.const 0
          i32.add
        end
        call 138
        f32.store align=1
        local.get 12
        i32.const 4
        i32.add
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 4
          i32.add
        else
          local.get 4
          i32.const 1
          i32.add
        end
        call 138
        f32.store align=1
        local.get 12
        i32.const 0
        i32.add
        local.set 13
        local.get 2
        local.get 13
        i32.const 8
        memory.copy
        local.get 2
        local.get 12
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 7
    local.get 12
    global.set 27
    local.get 7
    throw_ref
  )
  (func (;48;) (type 161) (param eqref i32 i32) (result i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 12
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 12
    i32.const 16
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 8
          i32.add
        else
          local.get 4
          i32.const 2
          i32.add
        end
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 12
        i32.const 0
        i32.add
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
        else
          local.get 4
          i32.const 0
          i32.add
        end
        call 138
        f32.store align=1
        local.get 12
        i32.const 4
        i32.add
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 4
          i32.add
        else
          local.get 4
          i32.const 1
          i32.add
        end
        call 138
        f32.store align=1
        local.get 12
        i32.const 0
        i32.add
        local.set 13
        local.get 2
        local.get 13
        i32.const 8
        memory.copy
        local.get 2
        local.get 12
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 7
    local.get 12
    global.set 27
    local.get 7
    throw_ref
  )
  (func (;49;) (type 162) (param eqref i32) (result i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 11
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 11
    i32.const 0
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 3
        local.set 2
        local.get 2
        ref.is_null
        local.get 3
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 2
        local.get 2
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 3
          i32.const 26
          i32.add
        else
          local.get 3
          i32.const 8
          i32.add
        end
        call 142
        i32.const 255
        i32.and
        local.get 11
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 6
    local.get 11
    global.set 27
    local.get 6
    throw_ref
  )
  (func (;50;) (type 163) (param eqref i32) (result i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 11
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 11
    i32.const 0
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 3
        local.set 2
        local.get 2
        ref.is_null
        local.get 3
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 2
        local.get 2
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 3
          i32.const 27
          i32.add
        else
          local.get 3
          i32.const 9
          i32.add
        end
        call 142
        i32.const 255
        i32.and
        local.get 11
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 6
    local.get 11
    global.set 27
    local.get 6
    throw_ref
  )
  (func (;51;) (type 164) (param eqref i32 i32) (result i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 12
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 12
    i32.const 16
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
        else
          local.get 4
          i32.const 0
          i32.add
        end
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 12
        i32.const 0
        i32.add
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
        else
          local.get 4
          i32.const 0
          i32.add
        end
        call 138
        f32.store align=1
        local.get 12
        i32.const 4
        i32.add
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 4
          i32.add
        else
          local.get 4
          i32.const 1
          i32.add
        end
        call 138
        f32.store align=1
        local.get 12
        i32.const 0
        i32.add
        local.set 13
        local.get 2
        local.get 13
        i32.const 8
        memory.copy
        local.get 2
        local.get 12
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 7
    local.get 12
    global.set 27
    local.get 7
    throw_ref
  )
  (func (;52;) (type 165) (param eqref i32) (result i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 11
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 11
    i32.const 0
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 3
        local.set 2
        local.get 2
        ref.is_null
        local.get 3
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 2
        local.get 2
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 3
          i32.const 24
          i32.add
        else
          local.get 3
          i32.const 6
          i32.add
        end
        call 142
        i32.const 255
        i32.and
        local.get 11
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 6
    local.get 11
    global.set 27
    local.get 6
    throw_ref
  )
  (func (;53;) (type 166) (param eqref i32) (result i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 11
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 11
    i32.const 0
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 3
        local.set 2
        local.get 2
        ref.is_null
        local.get 3
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 2
        local.get 2
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 3
          i32.const 25
          i32.add
        else
          local.get 3
          i32.const 7
          i32.add
        end
        call 142
        i32.const 255
        i32.and
        local.get 11
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 6
    local.get 11
    global.set 27
    local.get 6
    throw_ref
  )
  (func (;54;) (type 167) (param i32 i32 i32) (result i32)
    (local eqref i32 eqref i32 f32 eqref f32 f32 f32 f32 i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 19
    i32.const 1048368
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 19
    i32.const 208
    i32.add
    global.set 27
    local.get 19
    i32.const 0
    i32.add
    local.get 0
    i32.const 16
    memory.copy
    local.get 19
    i32.const 0
    i32.add
    local.set 0
    local.get 19
    i32.const 16
    i32.add
    local.get 1
    i32.const 16
    memory.copy
    local.get 19
    i32.const 16
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.set 20
        local.get 19
        i32.const 32
        i32.add
        local.get 20
        i32.const 16
        memory.copy
        local.get 19
        i32.const 32
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 20
        local.get 19
        i32.const 48
        i32.add
        local.get 20
        i32.const 16
        memory.copy
        local.get 19
        i32.const 48
        i32.add
        i32.const 12
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 7
        f32.demote_f64
        local.get 7
        f32.add
        f64.promote_f32
        local.get 0
        local.set 20
        local.get 19
        i32.const 64
        i32.add
        local.get 20
        i32.const 16
        memory.copy
        local.get 19
        i32.const 64
        i32.add
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 20
        local.get 19
        i32.const 80
        i32.add
        local.get 20
        i32.const 16
        memory.copy
        local.get 19
        i32.const 80
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 7
        f32.demote_f64
        local.get 7
        f32.add
        f64.promote_f32
        local.get 0
        local.set 20
        local.get 19
        i32.const 96
        i32.add
        local.get 20
        i32.const 16
        memory.copy
        local.get 19
        i32.const 96
        i32.add
        i32.const 8
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 20
        local.get 19
        i32.const 112
        i32.add
        local.get 20
        i32.const 16
        memory.copy
        local.get 19
        i32.const 112
        i32.add
        i32.const 12
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 7
        f32.demote_f64
        local.get 7
        f32.sub
        f64.promote_f32
        local.get 1
        local.set 20
        local.get 19
        i32.const 128
        i32.add
        local.get 20
        i32.const 16
        memory.copy
        local.get 19
        i32.const 128
        i32.add
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 7
        f32.demote_f64
        local.get 7
        f32.sub
        f64.promote_f32
        local.get 0
        local.set 20
        local.get 19
        i32.const 144
        i32.add
        local.get 20
        i32.const 16
        memory.copy
        local.get 19
        i32.const 144
        i32.add
        i32.const 12
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 20
        local.get 19
        i32.const 160
        i32.add
        local.get 20
        i32.const 16
        memory.copy
        local.get 19
        i32.const 160
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 7
        f32.demote_f64
        local.get 7
        f32.sub
        f64.promote_f32
        local.get 1
        local.set 20
        local.get 19
        i32.const 176
        i32.add
        local.get 20
        i32.const 16
        memory.copy
        local.get 19
        i32.const 176
        i32.add
        i32.const 8
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 7
        f32.demote_f64
        local.get 7
        f32.sub
        f64.promote_f32
        f32.demote_f64
        local.set 12
        f32.demote_f64
        local.set 11
        f32.demote_f64
        local.set 10
        f32.demote_f64
        local.set 9
        local.get 19
        i32.const 192
        i32.add
        i32.const 0
        i32.const 16
        memory.fill
        ref.null eq
        local.set 8
        local.get 19
        i32.const 192
        i32.add
        local.set 13
        local.get 8
        local.get 9
        local.get 10
        local.get 11
        local.get 12
        local.get 13
        call 13
        local.get 19
        i32.const 192
        i32.add
        local.set 20
        local.get 2
        local.get 20
        i32.const 16
        memory.copy
        local.get 2
        local.get 19
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 14
    local.get 19
    global.set 27
    local.get 14
    throw_ref
  )
  (func (;55;) (type 168) (param i32 i32) (result i32)
    (local f32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 8
    i32.const 1048416
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 8
    i32.const 160
    i32.add
    global.set 27
    local.get 8
    i32.const 0
    i32.add
    local.get 0
    i32.const 16
    memory.copy
    local.get 8
    i32.const 0
    i32.add
    local.set 0
    local.get 8
    i32.const 16
    i32.add
    local.get 1
    i32.const 16
    memory.copy
    local.get 8
    i32.const 16
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.set 9
        local.get 8
        i32.const 32
        i32.add
        local.get 9
        i32.const 16
        memory.copy
        local.get 8
        i32.const 32
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 9
        local.get 8
        i32.const 48
        i32.add
        local.get 9
        i32.const 16
        memory.copy
        local.get 8
        i32.const 48
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 2
        f32.demote_f64
        local.get 2
        f32.ne
        if (type 169) ;; label = @3
        else
          local.get 0
          local.set 9
          local.get 8
          i32.const 64
          i32.add
          local.get 9
          i32.const 16
          memory.copy
          local.get 8
          i32.const 64
          i32.add
          i32.const 4
          i32.add
          f32.load align=1
          f64.promote_f32
          local.get 1
          local.set 9
          local.get 8
          i32.const 80
          i32.add
          local.get 9
          i32.const 16
          memory.copy
          local.get 8
          i32.const 80
          i32.add
          i32.const 4
          i32.add
          f32.load align=1
          f64.promote_f32
          f32.demote_f64
          local.set 2
          f32.demote_f64
          local.get 2
          f32.ne
          if (type 170) ;; label = @4
          else
            local.get 0
            local.set 9
            local.get 8
            i32.const 96
            i32.add
            local.get 9
            i32.const 16
            memory.copy
            local.get 8
            i32.const 96
            i32.add
            i32.const 8
            i32.add
            f32.load align=1
            f64.promote_f32
            local.get 1
            local.set 9
            local.get 8
            i32.const 112
            i32.add
            local.get 9
            i32.const 16
            memory.copy
            local.get 8
            i32.const 112
            i32.add
            i32.const 8
            i32.add
            f32.load align=1
            f64.promote_f32
            f32.demote_f64
            local.set 2
            f32.demote_f64
            local.get 2
            f32.ne
            if (type 171) ;; label = @5
            else
              local.get 0
              local.set 9
              local.get 8
              i32.const 128
              i32.add
              local.get 9
              i32.const 16
              memory.copy
              local.get 8
              i32.const 128
              i32.add
              i32.const 12
              i32.add
              f32.load align=1
              f64.promote_f32
              local.get 1
              local.set 9
              local.get 8
              i32.const 144
              i32.add
              local.get 9
              i32.const 16
              memory.copy
              local.get 8
              i32.const 144
              i32.add
              i32.const 12
              i32.add
              f32.load align=1
              f64.promote_f32
              f32.demote_f64
              local.set 2
              f32.demote_f64
              local.get 2
              f32.ne
              i32.const 255
              i32.and
              local.get 8
              global.set 27
              return
            end
          end
        end
        i32.const 1
        i32.const 255
        i32.and
        local.get 8
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 3
    local.get 8
    global.set 27
    local.get 3
    throw_ref
  )
  (func (;56;) (type 172) (param eqref)
    (local i32 i64 f64 externref eqref eqref i32 eqref eqref eqref eqref eqref i32 i64 f64 externref)
    local.get 0
    local.set 5
    local.get 5
    call 68
    i32.const 32
    i32.and
    i32.eqz
    if (type 173) ;; label = @1
      local.get 0
      local.get 0
      local.set 5
      local.get 5
      call 68
      i32.const 32
      i32.or
      local.set 7
      local.set 6
      local.get 6
      local.get 7
      call 71
      local.get 0
      local.set 8
      local.get 8
      call 60
      local.tee 11
      local.get 11
      ref.is_null
      i32.eqz
      if (type 174) (param eqref) ;; label = @2
        local.get 0
        local.set 10
        local.set 9
        local.get 9
        local.tee 12
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 12
        ref.cast (ref 13)
        local.get 10
        local.get 9
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 45
        i32.add
        call_indirect (type 217)
        return
      else
        drop
        return
      end
      unreachable
    else
      return
    end
    unreachable
  )
  (func (;57;) (type 175) (param eqref i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 7
    i32.const 1048544
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 7
    i32.const 32
    i32.add
    global.set 27
    local.get 7
    i32.const 0
    i32.add
    local.get 1
    i32.const 16
    memory.copy
    local.get 7
    i32.const 0
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 8
        local.get 7
        i32.const 16
        i32.add
        local.get 8
        i32.const 16
        memory.copy
        local.get 7
        i32.const 16
        i32.add
        local.set 14
        local.tee 13
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 13
        ref.cast (ref 13)
        local.set 13
        local.get 13
        ref.cast (ref 13)
        local.get 14
        i32.const 0
        i32.add
        f32.load align=1
        struct.set 13 38
        local.get 13
        ref.cast (ref 13)
        local.get 14
        i32.const 4
        i32.add
        f32.load align=1
        struct.set 13 39
        local.get 13
        ref.cast (ref 13)
        local.get 14
        i32.const 8
        i32.add
        f32.load align=1
        struct.set 13 40
        local.get 13
        ref.cast (ref 13)
        local.get 14
        i32.const 12
        i32.add
        f32.load align=1
        struct.set 13 41
        local.get 7
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 2
    local.get 7
    global.set 27
    local.get 2
    throw_ref
  )
  (func (;58;) (type 176) (param eqref)
    (local eqref eqref i32 eqref eqref i32 i64 f64 externref)
    local.get 0
    local.get 0
    local.set 1
    local.get 1
    call 68
    i32.const -9
    i32.and
    local.set 3
    local.set 2
    local.get 2
    local.get 3
    call 71
    local.get 0
    local.get 0
    local.set 1
    local.get 1
    call 68
    i32.const -17
    i32.and
    local.set 3
    local.set 2
    local.get 2
    local.get 3
    call 71
    return
  )
  (func (;59;) (type 177) (param eqref i32) (result i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 11
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 11
    i32.const 0
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 3
        local.set 2
        local.get 2
        ref.is_null
        local.get 3
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 2
        local.get 2
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 3
          i32.const 0
          i32.add
        else
          local.get 3
          i32.const 0
          i32.add
        end
        call 144
        i32.const 255
        i32.and
        local.get 11
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 6
    local.get 11
    global.set 27
    local.get 6
    throw_ref
  )
  (func (;60;) (type 178) (param eqref) (result eqref)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.tee 2
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 23
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 23
        i32.const 0
        i32.const 53
        call 1
        global.set 23
      end
      global.get 23
      struct.new 3
      throw 0
    end
    local.get 2
    ref.cast (ref 13)
    struct.get 13 37
    return
  )
  (func (;61;) (type 179) (param eqref i32 i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 12
    i32.const 1048544
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 12
    i32.const 32
    i32.add
    global.set 27
    local.get 12
    i32.const 0
    i32.add
    local.get 1
    i32.const 16
    memory.copy
    local.get 12
    i32.const 0
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 2
        local.get 1
        local.set 13
        local.get 12
        i32.const 16
        i32.add
        local.get 13
        i32.const 16
        memory.copy
        local.get 12
        i32.const 16
        i32.add
        local.set 19
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 4
          i32.add
        else
          local.get 4
          i32.const 1
          i32.add
        end
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
        else
          local.get 4
          i32.const 0
          i32.add
        end
        local.get 19
        i32.const 0
        i32.add
        f32.load align=1
        call 139
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 4
          i32.add
        else
          local.get 4
          i32.const 1
          i32.add
        end
        local.get 19
        i32.const 4
        i32.add
        f32.load align=1
        call 139
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 8
          i32.add
        else
          local.get 4
          i32.const 2
          i32.add
        end
        local.get 19
        i32.const 8
        i32.add
        f32.load align=1
        call 139
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 12
          i32.add
        else
          local.get 4
          i32.const 3
          i32.add
        end
        local.get 19
        i32.const 12
        i32.add
        f32.load align=1
        call 139
        local.get 0
        local.get 2
        i32.const 1
        local.set 19
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
        else
          local.get 4
          i32.const 0
          i32.add
        end
        local.get 19
        i32.const 255
        i32.and
        call 145
        local.get 12
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 7
    local.get 12
    global.set 27
    local.get 7
    throw_ref
  )
  (func (;62;) (type 180) (param eqref i32 i32) (result i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 12
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 12
    i32.const 16
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 4
          i32.add
        else
          local.get 4
          i32.const 1
          i32.add
        end
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 12
        i32.const 0
        i32.add
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
        else
          local.get 4
          i32.const 0
          i32.add
        end
        call 138
        f32.store align=1
        local.get 12
        i32.const 4
        i32.add
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 4
          i32.add
        else
          local.get 4
          i32.const 1
          i32.add
        end
        call 138
        f32.store align=1
        local.get 12
        i32.const 8
        i32.add
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 8
          i32.add
        else
          local.get 4
          i32.const 2
          i32.add
        end
        call 138
        f32.store align=1
        local.get 12
        i32.const 12
        i32.add
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 12
          i32.add
        else
          local.get 4
          i32.const 3
          i32.add
        end
        call 138
        f32.store align=1
        local.get 12
        i32.const 0
        i32.add
        local.set 13
        local.get 2
        local.get 13
        i32.const 16
        memory.copy
        local.get 2
        local.get 12
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 7
    local.get 12
    global.set 27
    local.get 7
    throw_ref
  )
  (func (;63;) (type 181) (param eqref i32 i32) (result i32)
    (local eqref i32 eqref i32 f32 eqref i32 eqref i32 eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 21
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 21
    i32.const 16
    i32.add
    global.set 27
    local.get 21
    i32.const 0
    i32.add
    local.get 1
    i32.const 16
    memory.copy
    local.get 21
    i32.const 0
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 2
        local.set 9
        local.set 8
        local.get 8
        local.get 9
        call 75
        f64.promote_f32
        local.get 1
        local.set 4
        ref.null eq
        local.get 4
        local.set 11
        local.set 10
        local.get 10
        local.get 11
        call 76
        f64.promote_f32
        f32.demote_f64
        local.set 7
        f32.demote_f64
        local.get 7
        f32.le
        i32.eqz
        if (type 182) ;; label = @3
        else
          local.get 0
          local.get 2
          local.set 11
          local.set 10
          local.get 10
          local.get 11
          call 76
          f64.promote_f32
          local.get 1
          local.set 4
          ref.null eq
          local.get 4
          local.set 9
          local.set 8
          local.get 8
          local.get 9
          call 75
          f64.promote_f32
          f32.demote_f64
          local.set 7
          f32.demote_f64
          local.get 7
          f32.ge
          i32.eqz
          if (type 183) ;; label = @4
          else
            local.get 0
            local.get 2
            local.set 13
            local.set 12
            local.get 12
            local.get 13
            call 77
            f64.promote_f32
            local.get 1
            local.set 4
            ref.null eq
            local.get 4
            local.set 15
            local.set 14
            local.get 14
            local.get 15
            call 78
            f64.promote_f32
            f32.demote_f64
            local.set 7
            f32.demote_f64
            local.get 7
            f32.le
            i32.eqz
            if (type 184) ;; label = @5
            else
              local.get 0
              local.get 2
              local.set 15
              local.set 14
              local.get 14
              local.get 15
              call 78
              f64.promote_f32
              local.get 1
              local.set 4
              ref.null eq
              local.get 4
              local.set 13
              local.set 12
              local.get 12
              local.get 13
              call 77
              f64.promote_f32
              f32.demote_f64
              local.set 7
              f32.demote_f64
              local.get 7
              f32.ge
              i32.const 255
              i32.and
              local.get 21
              global.set 27
              return
            end
          end
        end
        i32.const 0
        i32.const 255
        i32.and
        local.get 21
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 16
    local.get 21
    global.set 27
    local.get 16
    throw_ref
  )
  (func (;64;) (type 185) (param eqref i32) (result eqref)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 11
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 11
    i32.const 0
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 3
        local.set 2
        local.get 2
        ref.is_null
        local.get 3
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 2
        local.get 2
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 3
          i32.const 20
          i32.add
          i32.load align=1
        else
          local.get 3
          i32.const 3
          i32.add
        end
        call 134
        local.get 11
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 6
    local.get 11
    global.set 27
    local.get 6
    throw_ref
  )
  (func (;65;) (type 186) (param eqref) (result i32)
    (local eqref eqref eqref i32 i64 f64 externref)
    local.get 0
    local.tee 3
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 23
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 23
        i32.const 0
        i32.const 53
        call 1
        global.set 23
      end
      global.get 23
      struct.new 3
      throw 0
    end
    local.get 3
    ref.cast (ref 13)
    struct.get 13 30
    if (type 187) ;; label = @1
      i32.const 1
      i32.const 255
      i32.and
      return
    else
      local.get 0
      local.set 1
      local.get 1
      call 68
      i32.const 96
      i32.and
      i32.const 0
      i32.gt_u
      i32.const 255
      i32.and
      return
    end
    unreachable
  )
  (func (;66;) (type 188) (param eqref)
    (local eqref eqref i32 eqref eqref i32 i64 f64 externref)
    local.get 0
    local.set 1
    local.get 1
    call 68
    i32.const 96
    i32.and
    drop
    local.get 0
    local.get 0
    local.set 1
    local.get 1
    call 68
    i32.const -33
    i32.and
    local.set 3
    local.set 2
    local.get 2
    local.get 3
    call 71
    local.get 0
    local.get 0
    local.set 1
    local.get 1
    call 68
    i32.const -65
    i32.and
    local.set 3
    local.set 2
    local.get 2
    local.get 3
    call 71
    return
  )
  (func (;67;) (type 189) (param eqref f32 f32 f32 f32 i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 15
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 15
    i32.const 0
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 5
        local.get 1
        f64.promote_f32
        local.set 24
        local.set 7
        local.set 6
        local.get 6
        ref.is_null
        local.get 7
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 6
        local.get 6
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 7
          i32.const 0
          i32.add
        else
          local.get 7
          i32.const 0
          i32.add
        end
        local.get 24
        f32.demote_f64
        call 139
        local.get 0
        local.get 5
        local.get 2
        f64.promote_f32
        local.set 24
        local.set 7
        local.set 6
        local.get 6
        ref.is_null
        local.get 7
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 6
        local.get 6
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 7
          i32.const 4
          i32.add
        else
          local.get 7
          i32.const 1
          i32.add
        end
        local.get 24
        f32.demote_f64
        call 139
        local.get 0
        local.get 5
        local.get 3
        f64.promote_f32
        local.set 24
        local.set 7
        local.set 6
        local.get 6
        ref.is_null
        local.get 7
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 6
        local.get 6
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 7
          i32.const 8
          i32.add
        else
          local.get 7
          i32.const 2
          i32.add
        end
        local.get 24
        f32.demote_f64
        call 139
        local.get 0
        local.get 5
        local.get 4
        f64.promote_f32
        local.set 24
        local.set 7
        local.set 6
        local.get 6
        ref.is_null
        local.get 7
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 6
        local.get 6
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 7
          i32.const 12
          i32.add
        else
          local.get 7
          i32.const 3
          i32.add
        end
        local.get 24
        f32.demote_f64
        call 139
        local.get 15
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 10
    local.get 15
    global.set 27
    local.get 10
    throw_ref
  )
  (func (;68;) (type 190) (param eqref) (result i32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.tee 2
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 23
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 23
        i32.const 0
        i32.const 53
        call 1
        global.set 23
      end
      global.get 23
      struct.new 3
      throw 0
    end
    local.get 2
    ref.cast (ref 13)
    struct.get 13 29
    return
  )
  (func (;69;) (type 191) (param i32 i32) (result i32)
    (local f32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 8
    i32.const 1048528
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 8
    i32.const 48
    i32.add
    global.set 27
    local.get 8
    i32.const 0
    i32.add
    local.get 0
    i32.const 8
    memory.copy
    local.get 8
    i32.const 0
    i32.add
    local.set 0
    local.get 8
    i32.const 8
    i32.add
    local.get 1
    i32.const 8
    memory.copy
    local.get 8
    i32.const 8
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.set 9
        local.get 8
        i32.const 16
        i32.add
        local.get 9
        i32.const 8
        memory.copy
        local.get 8
        i32.const 16
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 9
        local.get 8
        i32.const 24
        i32.add
        local.get 9
        i32.const 8
        memory.copy
        local.get 8
        i32.const 24
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 2
        f32.demote_f64
        local.get 2
        f32.eq
        i32.eqz
        if (type 192) ;; label = @3
          i32.const 0
          i32.const 255
          i32.and
          local.get 8
          global.set 27
          return
        else
          local.get 0
          local.set 9
          local.get 8
          i32.const 32
          i32.add
          local.get 9
          i32.const 8
          memory.copy
          local.get 8
          i32.const 32
          i32.add
          i32.const 4
          i32.add
          f32.load align=1
          f64.promote_f32
          local.get 1
          local.set 9
          local.get 8
          i32.const 40
          i32.add
          local.get 9
          i32.const 8
          memory.copy
          local.get 8
          i32.const 40
          i32.add
          i32.const 4
          i32.add
          f32.load align=1
          f64.promote_f32
          f32.demote_f64
          local.set 2
          f32.demote_f64
          local.get 2
          f32.eq
          i32.const 255
          i32.and
          local.get 8
          global.set 27
          return
        end
        unreachable
        unreachable
      end
      unreachable
    end
    local.set 3
    local.get 8
    global.set 27
    local.get 3
    throw_ref
  )
  (func (;70;) (type 193) (param eqref i32 eqref i32) (result i32)
    (local i32 i32 eqref i32 eqref i32 f32 eqref f32 f32 i32 eqref i32 eqref i32 eqref eqref exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 26
    i32.const 1048496
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 26
    i32.const 80
    i32.add
    global.set 27
    local.get 26
    i32.const 8
    i32.add
    local.set 4
    local.get 4
    i32.const 0
    i32.const 8
    memory.fill
    local.get 26
    i32.const 0
    i32.add
    local.get 1
    i32.const 8
    memory.copy
    local.get 26
    i32.const 0
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        i32.const 0
        f32.convert_i32_s
        f64.promote_f32
        i32.const 0
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 13
        f32.demote_f64
        local.set 12
        local.get 26
        i32.const 16
        i32.add
        i32.const 0
        i32.const 8
        memory.fill
        ref.null eq
        local.set 11
        local.get 26
        i32.const 16
        i32.add
        local.set 14
        local.get 11
        local.get 12
        local.get 13
        local.get 14
        call 5
        local.get 26
        i32.const 16
        i32.add
        local.get 26
        i32.const 24
        i32.add
        call 6
        local.set 27
        local.get 4
        local.get 27
        i32.const 8
        memory.copy
        i32.const 0
        local.set 5
        block (type 194) ;; label = @3
          loop (type 194) ;; label = @4
            local.get 5
            local.get 0
            local.set 20
            local.get 20
            local.tee 32
            ref.is_null
            if ;; label = @5
              i32.const 4
              global.get 23
              ref.is_null
              if ;; label = @6
                i32.const 0
                i32.const 53
                array.new_data 0 23
                i32.const 0
                i32.const 53
                call 1
                global.set 23
              end
              global.get 23
              struct.new 3
              throw 0
            end
            local.get 32
            ref.cast (ref 13)
            local.get 20
            ref.cast (ref 10)
            struct.get 10 0
            ref.cast (ref 9)
            struct.get 9 2
            i32.const 23
            i32.add
            call_indirect (type 229)
            i32.lt_s
            if (type 195) ;; label = @5
              local.get 4
              local.set 27
              local.get 26
              i32.const 32
              i32.add
              local.get 27
              i32.const 8
              memory.copy
              local.get 26
              i32.const 32
              i32.add
              local.get 0
              local.get 5
              local.set 16
              local.set 15
              local.get 15
              local.tee 32
              ref.is_null
              if ;; label = @6
                i32.const 4
                global.get 23
                ref.is_null
                if ;; label = @7
                  i32.const 0
                  i32.const 53
                  array.new_data 0 23
                  i32.const 0
                  i32.const 53
                  call 1
                  global.set 23
                end
                global.get 23
                struct.new 3
                throw 0
              end
              local.get 32
              ref.cast (ref 13)
              local.get 16
              local.get 15
              ref.cast (ref 10)
              struct.get 10 0
              ref.cast (ref 9)
              struct.get 9 2
              i32.const 24
              i32.add
              call_indirect (type 228)
              local.get 1
              local.set 27
              local.get 26
              i32.const 40
              i32.add
              local.get 27
              i32.const 8
              memory.copy
              local.get 26
              i32.const 40
              i32.add
              local.get 2
              local.set 19
              local.set 18
              local.set 17
              local.get 17
              local.tee 32
              ref.is_null
              if ;; label = @6
                i32.const 4
                global.get 23
                ref.is_null
                if ;; label = @7
                  i32.const 0
                  i32.const 53
                  array.new_data 0 23
                  i32.const 0
                  i32.const 53
                  call 1
                  global.set 23
                end
                global.get 23
                struct.new 3
                throw 0
              end
              local.get 32
              ref.cast (ref 13)
              local.get 18
              local.get 19
              local.get 26
              i32.const 48
              i32.add
              call 7
              local.get 26
              i32.const 56
              i32.add
              call 30
              local.set 27
              local.get 4
              local.get 27
              i32.const 8
              memory.copy
              local.get 5
              i32.const 1
              i32.add
              local.set 5
              br 1 (;@4;)
            else
              br 2 (;@3;)
            end
            unreachable
          end
        end
        local.get 4
        local.set 27
        local.get 26
        i32.const 64
        i32.add
        local.get 27
        i32.const 8
        memory.copy
        local.get 26
        i32.const 64
        i32.add
        local.set 27
        local.get 3
        local.get 27
        i32.const 8
        memory.copy
        local.get 3
        local.get 26
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 21
    local.get 26
    global.set 27
    local.get 21
    throw_ref
  )
  (func (;71;) (type 196) (param eqref i32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.get 1
    local.set 4
    local.tee 3
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 23
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 23
        i32.const 0
        i32.const 53
        call 1
        global.set 23
      end
      global.get 23
      struct.new 3
      throw 0
    end
    local.get 3
    ref.cast (ref 13)
    local.set 3
    local.get 3
    ref.cast (ref 13)
    local.get 4
    struct.set 13 29
    return
  )
  (func (;72;) (type 197) (param i32 i32) (result i32)
    (local f32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 8
    i32.const 1048528
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 8
    i32.const 48
    i32.add
    global.set 27
    local.get 8
    i32.const 0
    i32.add
    local.get 0
    i32.const 8
    memory.copy
    local.get 8
    i32.const 0
    i32.add
    local.set 0
    local.get 8
    i32.const 8
    i32.add
    local.get 1
    i32.const 8
    memory.copy
    local.get 8
    i32.const 8
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.set 9
        local.get 8
        i32.const 16
        i32.add
        local.get 9
        i32.const 8
        memory.copy
        local.get 8
        i32.const 16
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 9
        local.get 8
        i32.const 24
        i32.add
        local.get 9
        i32.const 8
        memory.copy
        local.get 8
        i32.const 24
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 2
        f32.demote_f64
        local.get 2
        f32.eq
        i32.eqz
        if (type 198) ;; label = @3
          i32.const 0
          i32.const 255
          i32.and
          local.get 8
          global.set 27
          return
        else
          local.get 0
          local.set 9
          local.get 8
          i32.const 32
          i32.add
          local.get 9
          i32.const 8
          memory.copy
          local.get 8
          i32.const 32
          i32.add
          i32.const 4
          i32.add
          f32.load align=1
          f64.promote_f32
          local.get 1
          local.set 9
          local.get 8
          i32.const 40
          i32.add
          local.get 9
          i32.const 8
          memory.copy
          local.get 8
          i32.const 40
          i32.add
          i32.const 4
          i32.add
          f32.load align=1
          f64.promote_f32
          f32.demote_f64
          local.set 2
          f32.demote_f64
          local.get 2
          f32.eq
          i32.const 255
          i32.and
          local.get 8
          global.set 27
          return
        end
        unreachable
        unreachable
      end
      unreachable
    end
    local.set 3
    local.get 8
    global.set 27
    local.get 3
    throw_ref
  )
  (func (;73;) (type 199) (param eqref i32 eqref)
    (local i32 i32 eqref i32 eqref i32 eqref i32 eqref i32 eqref i32 eqref exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 21
    i32.const 1048496
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 21
    i32.const 80
    i32.add
    global.set 27
    local.get 21
    i32.const 16
    i32.add
    local.set 4
    local.get 4
    i32.const 0
    i32.const 12
    memory.fill
    local.get 21
    i32.const 0
    i32.add
    local.get 1
    i32.const 16
    memory.copy
    local.get 21
    i32.const 0
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        i32.const 0
        local.set 3
        block (type 200) ;; label = @3
          loop (type 200) ;; label = @4
            local.get 3
            local.get 0
            local.set 15
            local.get 15
            local.tee 27
            ref.is_null
            if ;; label = @5
              i32.const 4
              global.get 23
              ref.is_null
              if ;; label = @6
                i32.const 0
                i32.const 53
                array.new_data 0 23
                i32.const 0
                i32.const 53
                call 1
                global.set 23
              end
              global.get 23
              struct.new 3
              throw 0
            end
            local.get 27
            ref.cast (ref 13)
            local.get 15
            ref.cast (ref 10)
            struct.get 10 0
            ref.cast (ref 9)
            struct.get 9 2
            i32.const 23
            i32.add
            call_indirect (type 229)
            i32.lt_s
            if (type 201) ;; label = @5
              local.get 0
              local.get 3
              local.set 10
              local.set 9
              local.get 9
              local.tee 27
              ref.is_null
              if ;; label = @6
                i32.const 4
                global.get 23
                ref.is_null
                if ;; label = @7
                  i32.const 0
                  i32.const 53
                  array.new_data 0 23
                  i32.const 0
                  i32.const 53
                  call 1
                  global.set 23
                end
                global.get 23
                struct.new 3
                throw 0
              end
              local.get 27
              ref.cast (ref 13)
              local.get 10
              local.get 9
              ref.cast (ref 10)
              struct.get 10 0
              ref.cast (ref 9)
              struct.get 9 2
              i32.const 24
              i32.add
              call_indirect (type 228)
              local.get 1
              local.set 22
              local.get 21
              i32.const 28
              i32.add
              local.get 22
              i32.const 16
              memory.copy
              local.get 21
              i32.const 28
              i32.add
              local.get 2
              local.get 4
              local.set 6
              ref.null eq
              local.get 6
              local.set 6
              local.set 5
              local.get 5
              ref.is_null
              local.get 6
              i32.const -1
              i32.eq
              i32.and
              if ;; label = @6
                i32.const 4
                global.get 23
                ref.is_null
                if ;; label = @7
                  i32.const 0
                  i32.const 53
                  array.new_data 0 23
                  i32.const 0
                  i32.const 53
                  call 1
                  global.set 23
                end
                global.get 23
                struct.new 3
                throw 0
              end
              local.get 5
              ref.is_null
              local.get 6
              i32.const -1
              i32.eq
              i32.and
              if ;; label = @6
                i32.const 4
                global.get 23
                ref.is_null
                if ;; label = @7
                  i32.const 0
                  i32.const 53
                  array.new_data 0 23
                  i32.const 0
                  i32.const 53
                  call 1
                  global.set 23
                end
                global.get 23
                struct.new 3
                throw 0
              end
              local.get 5
              local.get 5
              ref.is_null
              if (type 59) (result i32) ;; label = @6
                local.get 6
                i32.const 0
                i32.add
              else
                local.get 6
                i32.const 0
                i32.add
              end
              i32.const 0
              call 145
              local.get 5
              ref.is_null
              local.get 6
              i32.const -1
              i32.eq
              i32.and
              if ;; label = @6
                i32.const 4
                global.get 23
                ref.is_null
                if ;; label = @7
                  i32.const 0
                  i32.const 53
                  array.new_data 0 23
                  i32.const 0
                  i32.const 53
                  call 1
                  global.set 23
                end
                global.get 23
                struct.new 3
                throw 0
              end
              local.get 5
              local.get 5
              ref.is_null
              if (type 59) (result i32) ;; label = @6
                local.get 6
                i32.const 4
                i32.add
              else
                local.get 6
                i32.const 1
                i32.add
              end
              f32.const 0x0p+0 (;=0;)
              call 139
              local.get 5
              ref.is_null
              local.get 6
              i32.const -1
              i32.eq
              i32.and
              if ;; label = @6
                i32.const 4
                global.get 23
                ref.is_null
                if ;; label = @7
                  i32.const 0
                  i32.const 53
                  array.new_data 0 23
                  i32.const 0
                  i32.const 53
                  call 1
                  global.set 23
                end
                global.get 23
                struct.new 3
                throw 0
              end
              local.get 5
              local.get 5
              ref.is_null
              if (type 59) (result i32) ;; label = @6
                local.get 6
                i32.const 8
                i32.add
              else
                local.get 6
                i32.const 2
                i32.add
              end
              f32.const 0x0p+0 (;=0;)
              call 139
              local.get 4
              local.set 22
              local.get 21
              i32.const 44
              i32.add
              local.get 22
              i32.const 12
              memory.copy
              local.get 21
              i32.const 44
              i32.add
              local.set 14
              local.set 13
              local.set 12
              local.set 11
              local.get 11
              local.tee 27
              ref.is_null
              if ;; label = @6
                i32.const 4
                global.get 23
                ref.is_null
                if ;; label = @7
                  i32.const 0
                  i32.const 53
                  array.new_data 0 23
                  i32.const 0
                  i32.const 53
                  call 1
                  global.set 23
                end
                global.get 23
                struct.new 3
                throw 0
              end
              local.get 27
              ref.cast (ref 13)
              local.get 12
              local.get 13
              local.get 14
              local.get 21
              i32.const 56
              i32.add
              call 11
              drop
              local.get 3
              i32.const 1
              i32.add
              local.set 3
              br 1 (;@4;)
            else
              br 2 (;@3;)
            end
            unreachable
          end
        end
        local.get 21
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 16
    local.get 21
    global.set 27
    local.get 16
    throw_ref
  )
  (func (;74;) (type 202) (param eqref i32) (result i32)
    (local eqref exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 8
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 8
    i32.const 16
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.set 2
        local.get 2
        local.get 8
        i32.const 0
        i32.add
        call 46
        local.set 9
        local.get 1
        local.get 9
        i32.const 16
        memory.copy
        local.get 1
        local.get 8
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 3
    local.get 8
    global.set 27
    local.get 3
    throw_ref
  )
  (func (;75;) (type 203) (param eqref i32) (result f32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 11
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 11
    i32.const 0
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 3
        local.set 2
        local.get 2
        ref.is_null
        local.get 3
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 2
        local.get 2
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 3
          i32.const 0
          i32.add
        else
          local.get 3
          i32.const 0
          i32.add
        end
        call 138
        f64.promote_f32
        f32.demote_f64
        local.get 11
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 6
    local.get 11
    global.set 27
    local.get 6
    throw_ref
  )
  (func (;76;) (type 204) (param eqref i32) (result f32)
    (local eqref i32 eqref i32 f32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 12
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 12
    i32.const 0
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 3
        local.set 2
        local.get 2
        ref.is_null
        local.get 3
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 2
        local.get 2
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 3
          i32.const 0
          i32.add
        else
          local.get 3
          i32.const 0
          i32.add
        end
        call 138
        f64.promote_f32
        local.get 0
        local.get 1
        local.set 3
        local.set 2
        local.get 2
        ref.is_null
        local.get 3
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 2
        local.get 2
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 3
          i32.const 8
          i32.add
        else
          local.get 3
          i32.const 2
          i32.add
        end
        call 138
        f64.promote_f32
        f32.demote_f64
        local.set 6
        f32.demote_f64
        local.get 6
        f32.add
        f64.promote_f32
        f32.demote_f64
        local.get 12
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 7
    local.get 12
    global.set 27
    local.get 7
    throw_ref
  )
  (func (;77;) (type 205) (param eqref i32) (result f32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 11
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 11
    i32.const 0
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 3
        local.set 2
        local.get 2
        ref.is_null
        local.get 3
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 2
        local.get 2
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 3
          i32.const 4
          i32.add
        else
          local.get 3
          i32.const 1
          i32.add
        end
        call 138
        f64.promote_f32
        f32.demote_f64
        local.get 11
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 6
    local.get 11
    global.set 27
    local.get 6
    throw_ref
  )
  (func (;78;) (type 206) (param eqref i32) (result f32)
    (local eqref i32 eqref i32 f32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 12
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 12
    i32.const 0
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 3
        local.set 2
        local.get 2
        ref.is_null
        local.get 3
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 2
        local.get 2
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 3
          i32.const 4
          i32.add
        else
          local.get 3
          i32.const 1
          i32.add
        end
        call 138
        f64.promote_f32
        local.get 0
        local.get 1
        local.set 3
        local.set 2
        local.get 2
        ref.is_null
        local.get 3
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 2
        local.get 2
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 3
          i32.const 12
          i32.add
        else
          local.get 3
          i32.const 3
          i32.add
        end
        call 138
        f64.promote_f32
        f32.demote_f64
        local.set 6
        f32.demote_f64
        local.get 6
        f32.add
        f64.promote_f32
        f32.demote_f64
        local.get 12
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 7
    local.get 12
    global.set 27
    local.get 7
    throw_ref
  )
  (func (;79;) (type 207) (param eqref i32 i32) (result i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 8
    i32.const 1048544
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 8
    i32.const 32
    i32.add
    global.set 27
    local.get 8
    i32.const 0
    i32.add
    local.get 1
    i32.const 16
    memory.copy
    local.get 8
    i32.const 0
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 1
        local.set 9
        local.get 8
        i32.const 16
        i32.add
        local.get 9
        i32.const 16
        memory.copy
        local.get 8
        i32.const 16
        i32.add
        local.set 9
        local.get 2
        local.get 9
        i32.const 16
        memory.copy
        local.get 2
        local.get 8
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 3
    local.get 8
    global.set 27
    local.get 3
    throw_ref
  )
  (func (;80;) (type 209) (param eqref)
    (local eqref eqref i32 i64 f64 externref)
    global.get 0
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 4
      array.new_data 0 0
      i32.const 0
      i32.const 4
      call 1
      global.set 0
    end
    global.get 0
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    global.get 1
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 0
      array.new_data 0 1
      i32.const 0
      i32.const 0
      call 1
      global.set 1
    end
    global.get 1
    call 0
    return
  )
  (func (;81;) (type 210) (param eqref eqref)
    (local i32 i32 i32 i32 i32 eqref i32 eqref i32 f32 eqref f32 eqref i32 eqref i32 eqref f32 eqref f32 eqref f32 eqref i32 eqref i32 eqref i32 eqref f32 externref i32 i32 i32 i32 i32 eqref i32 eqref i32 eqref i32 eqref exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 50
    i32.const 1048416
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 50
    i32.const 160
    i32.add
    global.set 27
    local.get 50
    i32.const 0
    i32.add
    local.set 2
    local.get 2
    i32.const 0
    i32.const 12
    memory.fill
    local.get 50
    i32.const 12
    i32.add
    local.set 3
    local.get 3
    i32.const 0
    i32.const 8
    memory.fill
    local.get 50
    i32.const 20
    i32.add
    local.set 4
    local.get 4
    i32.const 0
    i32.const 12
    memory.fill
    local.get 50
    i32.const 32
    i32.add
    local.set 5
    local.get 5
    i32.const 0
    i32.const 8
    memory.fill
    local.get 50
    i32.const 40
    i32.add
    local.set 6
    local.get 6
    i32.const 0
    i32.const 8
    memory.fill
    global.get 28
    local.tee 47
    i32.const 65533
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 47
    i32.const 3
    i32.add
    global.set 28
    local.get 50
    i32.const 0
    i32.add
    local.get 47
    i32.const 0
    i32.add
    i32.store align=1
    local.get 50
    i32.const 48
    i32.add
    local.get 47
    i32.const 1
    i32.add
    i32.store align=1
    local.get 50
    i32.const 128
    i32.add
    local.get 47
    i32.const 2
    i32.add
    i32.store align=1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 1
        i32.const 1
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 13
        local.set 12
        local.get 12
        local.tee 56
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 56
        ref.cast (ref 10)
        local.get 13
        local.get 12
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 2
        i32.add
        call_indirect (type 230)
        local.get 1
        i32.const 0
        local.set 15
        local.set 14
        local.get 14
        local.tee 56
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 56
        ref.cast (ref 10)
        local.get 15
        local.get 14
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 3
        i32.add
        call_indirect (type 232)
        local.get 1
        i32.const 0
        local.set 17
        local.set 16
        local.get 16
        local.tee 56
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 56
        ref.cast (ref 10)
        local.get 17
        local.get 16
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 4
        i32.add
        call_indirect (type 234)
        local.get 1
        i32.const 1
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 19
        local.set 18
        local.get 18
        local.tee 56
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 56
        ref.cast (ref 10)
        local.get 19
        local.get 18
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 5
        i32.add
        call_indirect (type 236)
        local.get 1
        i32.const 10
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 21
        local.set 20
        local.get 20
        local.tee 56
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 56
        ref.cast (ref 10)
        local.get 21
        local.get 20
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 6
        i32.add
        call_indirect (type 238)
        local.get 1
        i32.const 0
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 23
        local.set 22
        local.get 22
        local.tee 56
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 56
        ref.cast (ref 10)
        local.get 23
        local.get 22
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 7
        i32.add
        call_indirect (type 240)
        local.get 1
        local.get 2
        local.set 8
        ref.null eq
        local.get 8
        local.set 8
        local.set 7
        local.get 7
        ref.is_null
        local.get 8
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 7
        ref.is_null
        local.get 8
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 7
        local.get 7
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 8
          i32.const 0
          i32.add
          i32.load align=1
        else
          local.get 8
          i32.const 0
          i32.add
        end
        ref.null eq
        call 135
        local.get 7
        ref.is_null
        local.get 8
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 7
        local.get 7
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 8
          i32.const 4
          i32.add
        else
          local.get 8
          i32.const 1
          i32.add
        end
        i32.const 0
        call 147
        local.get 7
        ref.is_null
        local.get 8
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 7
        local.get 7
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 8
          i32.const 8
          i32.add
        else
          local.get 8
          i32.const 2
          i32.add
        end
        i32.const 0
        call 147
        local.get 2
        local.set 51
        local.get 50
        i32.const 48
        i32.add
        local.get 51
        local.set 48
        local.set 49
        local.get 49
        i32.const 0
        i32.add
        local.get 48
        i32.const 0
        i32.add
        i32.load align=1
        table.get 1
        local.set 46
        i32.load align=1
        local.get 46
        table.set 1
        local.get 49
        i32.const 4
        i32.add
        local.get 48
        i32.const 4
        i32.add
        i32.load align=1
        i32.store align=1
        local.get 49
        i32.const 8
        i32.add
        local.get 48
        i32.const 8
        i32.add
        i32.load align=1
        i32.store align=1
        local.get 50
        i32.const 48
        i32.add
        local.set 25
        local.set 24
        local.get 24
        local.tee 56
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 56
        ref.cast (ref 10)
        local.get 25
        local.get 24
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 8
        i32.add
        call_indirect (type 242)
        local.get 1
        i32.const 255
        local.get 50
        i32.const 60
        i32.add
        call 86
        local.set 27
        local.set 26
        local.get 26
        local.tee 56
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 56
        ref.cast (ref 10)
        local.get 27
        local.get 26
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 9
        i32.add
        call_indirect (type 246)
        local.get 1
        i32.const -2139062017
        local.get 50
        i32.const 76
        i32.add
        call 86
        local.set 29
        local.set 28
        local.get 28
        local.tee 56
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 56
        ref.cast (ref 10)
        local.get 29
        local.get 28
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 10
        i32.add
        call_indirect (type 248)
        local.get 1
        i32.const 12
        f32.convert_i32_s
        f64.promote_f32
        global.get 2
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 5
          array.new_data 0 2
          i32.const 0
          i32.const 5
          call 1
          global.set 2
        end
        global.get 2
        local.get 3
        local.set 8
        ref.null eq
        local.get 8
        local.set 8
        local.set 7
        local.get 7
        ref.is_null
        local.get 8
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 7
        ref.is_null
        local.get 8
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 7
        local.get 7
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 8
          i32.const 0
          i32.add
        else
          local.get 8
          i32.const 0
          i32.add
        end
        i32.const 0
        call 145
        local.get 7
        ref.is_null
        local.get 8
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 7
        local.get 7
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 8
          i32.const 4
          i32.add
        else
          local.get 8
          i32.const 1
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 139
        local.get 3
        local.set 51
        local.get 50
        i32.const 92
        i32.add
        local.get 51
        i32.const 8
        memory.copy
        local.get 50
        i32.const 92
        i32.add
        local.get 4
        local.set 8
        ref.null eq
        local.get 8
        local.set 8
        local.set 7
        local.get 7
        ref.is_null
        local.get 8
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 7
        ref.is_null
        local.get 8
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 7
        local.get 7
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 8
          i32.const 0
          i32.add
        else
          local.get 8
          i32.const 0
          i32.add
        end
        i32.const 0
        call 145
        local.get 7
        ref.is_null
        local.get 8
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 7
        local.get 7
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 8
          i32.const 4
          i32.add
        else
          local.get 8
          i32.const 1
          i32.add
        end
        i32.const 0
        call 145
        local.get 7
        ref.is_null
        local.get 8
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 7
        local.get 7
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 8
          i32.const 5
          i32.add
        else
          local.get 8
          i32.const 2
          i32.add
        end
        i32.const 0
        call 145
        local.get 7
        ref.is_null
        local.get 8
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 7
        local.get 7
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 8
          i32.const 8
          i32.add
        else
          local.get 8
          i32.const 3
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 139
        local.get 4
        local.set 51
        local.get 50
        i32.const 100
        i32.add
        local.get 51
        i32.const 12
        memory.copy
        local.get 50
        i32.const 100
        i32.add
        local.get 5
        local.set 8
        ref.null eq
        local.get 8
        local.set 8
        local.set 7
        local.get 7
        ref.is_null
        local.get 8
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 7
        ref.is_null
        local.get 8
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 7
        local.get 7
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 8
          i32.const 0
          i32.add
        else
          local.get 8
          i32.const 0
          i32.add
        end
        i32.const 0
        call 145
        local.get 7
        ref.is_null
        local.get 8
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 7
        local.get 7
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 8
          i32.const 4
          i32.add
        else
          local.get 8
          i32.const 1
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 139
        local.get 5
        local.set 51
        local.get 50
        i32.const 112
        i32.add
        local.get 51
        i32.const 8
        memory.copy
        local.get 50
        i32.const 112
        i32.add
        local.get 6
        local.set 8
        ref.null eq
        local.get 8
        local.set 8
        local.set 7
        local.get 7
        ref.is_null
        local.get 8
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 7
        ref.is_null
        local.get 8
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 7
        local.get 7
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 8
          i32.const 0
          i32.add
        else
          local.get 8
          i32.const 0
          i32.add
        end
        i32.const 0
        call 145
        local.get 7
        ref.is_null
        local.get 8
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 7
        local.get 7
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 8
          i32.const 4
          i32.add
        else
          local.get 8
          i32.const 1
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 139
        local.get 6
        local.set 51
        local.get 50
        i32.const 120
        i32.add
        local.get 51
        i32.const 8
        memory.copy
        local.get 50
        i32.const 120
        i32.add
        local.set 36
        local.set 35
        local.set 34
        local.set 33
        local.set 32
        f32.demote_f64
        local.set 31
        local.get 50
        i32.const 128
        i32.add
        local.set 49
        local.get 49
        i32.const 0
        i32.add
        ref.null extern
        local.set 60
        i32.load align=1
        local.get 60
        table.set 2
        local.get 49
        i32.const 4
        i32.add
        f32.const 0x0p+0 (;=0;)
        f32.store align=1
        local.get 49
        i32.const 8
        i32.add
        i32.const 0
        i32.store8
        local.get 49
        i32.const 9
        i32.add
        i32.const 0
        i32.store8
        local.get 49
        i32.const 12
        i32.add
        f32.const 0x0p+0 (;=0;)
        f32.store align=1
        local.get 49
        i32.const 16
        i32.add
        f32.const 0x0p+0 (;=0;)
        f32.store align=1
        local.get 49
        i32.const 20
        i32.add
        f32.const 0x0p+0 (;=0;)
        f32.store align=1
        local.get 49
        i32.const 24
        i32.add
        f32.const 0x0p+0 (;=0;)
        f32.store align=1
        ref.null eq
        local.set 30
        local.get 50
        i32.const 128
        i32.add
        local.set 37
        local.get 30
        local.get 31
        local.get 32
        local.get 33
        local.get 34
        local.get 35
        local.get 36
        local.get 37
        call 87
        local.get 50
        i32.const 128
        i32.add
        local.set 39
        local.set 38
        local.get 38
        local.tee 56
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 56
        ref.cast (ref 10)
        local.get 39
        local.get 38
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 11
        i32.add
        call_indirect (type 267)
        local.get 1
        i32.const 0
        local.set 41
        local.set 40
        local.get 40
        local.tee 56
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 56
        ref.cast (ref 10)
        local.get 41
        local.get 40
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 12
        i32.add
        call_indirect (type 269)
        local.get 1
        i32.const 3
        local.set 43
        local.set 42
        local.get 42
        local.tee 56
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 56
        ref.cast (ref 10)
        local.get 43
        local.get 42
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 13
        i32.add
        call_indirect (type 271)
        local.get 1
        local.set 44
        local.get 44
        local.tee 56
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 56
        ref.cast (ref 10)
        local.get 44
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 14
        i32.add
        call_indirect (type 273)
        local.get 47
        ref.null eq
        i32.const 3
        table.fill 1
        local.get 47
        ref.null extern
        i32.const 3
        table.fill 2
        local.get 47
        global.set 28
        local.get 50
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 45
    local.get 47
    ref.null eq
    i32.const 3
    table.fill 1
    local.get 47
    ref.null extern
    i32.const 3
    table.fill 2
    local.get 47
    global.set 28
    local.get 50
    global.set 27
    local.get 45
    throw_ref
  )
  (func (;82;) (type 211) (param eqref eqref)
    (local i32 eqref i32 eqref eqref eqref eqref eqref eqref i32 i64 f64 externref)
    i32.const 0
    local.set 2
    block (type 212) ;; label = @1
      loop (type 212) ;; label = @2
        local.get 2
        local.get 0
        local.set 8
        local.get 8
        local.tee 10
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 10
        ref.cast (ref 13)
        local.get 8
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 23
        i32.add
        call_indirect (type 229)
        i32.lt_s
        if (type 213) ;; label = @3
          local.get 0
          local.get 1
          local.get 0
          local.get 2
          local.set 4
          local.set 3
          local.get 3
          local.tee 10
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 10
          ref.cast (ref 13)
          local.get 4
          local.get 3
          ref.cast (ref 10)
          struct.get 10 0
          ref.cast (ref 9)
          struct.get 9 2
          i32.const 24
          i32.add
          call_indirect (type 228)
          local.set 7
          local.set 6
          local.set 5
          local.get 5
          local.get 6
          local.get 7
          call 88
          local.get 2
          i32.const 1
          i32.add
          local.set 2
          br 1 (;@2;)
        else
          br 2 (;@1;)
        end
        unreachable
      end
    end
    return
  )
  (func (;83;) (type 214) (param eqref eqref)
    (local f32 f32 eqref i32 eqref i32 f32 eqref eqref i32 eqref f32 f32 f32 f32 i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 25
    i32.const 1048384
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 25
    i32.const 192
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.set 9
        local.get 9
        local.get 25
        i32.const 0
        i32.add
        call 46
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 2
        local.get 0
        local.set 9
        local.get 9
        local.get 25
        i32.const 16
        i32.add
        call 46
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 3
        local.get 1
        i32.const -16776961
        local.get 25
        i32.const 32
        i32.add
        call 86
        local.set 11
        local.set 10
        local.get 10
        local.tee 31
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 31
        ref.cast (ref 10)
        local.get 11
        local.get 10
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 10
        i32.add
        call_indirect (type 248)
        local.get 1
        local.get 2
        f64.promote_f32
        i32.const 10
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 8
        f32.demote_f64
        local.get 8
        f32.add
        f64.promote_f32
        local.get 3
        f64.promote_f32
        i32.const 10
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 8
        f32.demote_f64
        local.get 8
        f32.add
        f64.promote_f32
        i32.const 100
        f32.convert_i32_s
        f64.promote_f32
        i32.const 80
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 16
        f32.demote_f64
        local.set 15
        f32.demote_f64
        local.set 14
        f32.demote_f64
        local.set 13
        local.get 25
        i32.const 48
        i32.add
        i32.const 0
        i32.const 16
        memory.fill
        ref.null eq
        local.set 12
        local.get 25
        i32.const 48
        i32.add
        local.set 17
        local.get 12
        local.get 13
        local.get 14
        local.get 15
        local.get 16
        local.get 17
        call 13
        local.get 25
        i32.const 48
        i32.add
        local.set 19
        local.set 18
        local.get 18
        local.tee 31
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 31
        ref.cast (ref 10)
        local.get 19
        local.get 18
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 15
        i32.add
        call_indirect (type 276)
        local.get 1
        i32.const 8388863
        local.get 25
        i32.const 64
        i32.add
        call 86
        local.set 11
        local.set 10
        local.get 10
        local.tee 31
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 31
        ref.cast (ref 10)
        local.get 11
        local.get 10
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 10
        i32.add
        call_indirect (type 248)
        local.get 1
        local.get 2
        f64.promote_f32
        i32.const 130
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 8
        f32.demote_f64
        local.get 8
        f32.add
        f64.promote_f32
        local.get 3
        f64.promote_f32
        i32.const 10
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 8
        f32.demote_f64
        local.get 8
        f32.add
        f64.promote_f32
        i32.const 100
        f32.convert_i32_s
        f64.promote_f32
        i32.const 80
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 16
        f32.demote_f64
        local.set 15
        f32.demote_f64
        local.set 14
        f32.demote_f64
        local.set 13
        local.get 25
        i32.const 80
        i32.add
        i32.const 0
        i32.const 16
        memory.fill
        ref.null eq
        local.set 12
        local.get 25
        i32.const 80
        i32.add
        local.set 17
        local.get 12
        local.get 13
        local.get 14
        local.get 15
        local.get 16
        local.get 17
        call 13
        local.get 25
        i32.const 80
        i32.add
        local.set 19
        local.set 18
        local.get 18
        local.tee 31
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 31
        ref.cast (ref 10)
        local.get 19
        local.get 18
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 15
        i32.add
        call_indirect (type 276)
        local.get 1
        i32.const 65535
        local.get 25
        i32.const 96
        i32.add
        call 86
        local.set 11
        local.set 10
        local.get 10
        local.tee 31
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 31
        ref.cast (ref 10)
        local.get 11
        local.get 10
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 10
        i32.add
        call_indirect (type 248)
        local.get 1
        local.get 2
        f64.promote_f32
        i32.const 250
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 8
        f32.demote_f64
        local.get 8
        f32.add
        f64.promote_f32
        local.get 3
        f64.promote_f32
        i32.const 10
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 8
        f32.demote_f64
        local.get 8
        f32.add
        f64.promote_f32
        i32.const 100
        f32.convert_i32_s
        f64.promote_f32
        i32.const 80
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 16
        f32.demote_f64
        local.set 15
        f32.demote_f64
        local.set 14
        f32.demote_f64
        local.set 13
        local.get 25
        i32.const 112
        i32.add
        i32.const 0
        i32.const 16
        memory.fill
        ref.null eq
        local.set 12
        local.get 25
        i32.const 112
        i32.add
        local.set 17
        local.get 12
        local.get 13
        local.get 14
        local.get 15
        local.get 16
        local.get 17
        call 13
        local.get 25
        i32.const 112
        i32.add
        local.set 19
        local.set 18
        local.get 18
        local.tee 31
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 31
        ref.cast (ref 10)
        local.get 19
        local.get 18
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 15
        i32.add
        call_indirect (type 276)
        local.get 1
        i32.const -5963521
        local.get 25
        i32.const 128
        i32.add
        call 86
        local.set 11
        local.set 10
        local.get 10
        local.tee 31
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 31
        ref.cast (ref 10)
        local.get 11
        local.get 10
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 10
        i32.add
        call_indirect (type 248)
        local.get 1
        local.get 2
        f64.promote_f32
        i32.const 60
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 8
        f32.demote_f64
        local.get 8
        f32.add
        f64.promote_f32
        local.get 3
        f64.promote_f32
        i32.const 50
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 8
        f32.demote_f64
        local.get 8
        f32.add
        f64.promote_f32
        i32.const 100
        f32.convert_i32_s
        f64.promote_f32
        i32.const 80
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 16
        f32.demote_f64
        local.set 15
        f32.demote_f64
        local.set 14
        f32.demote_f64
        local.set 13
        local.get 25
        i32.const 144
        i32.add
        i32.const 0
        i32.const 16
        memory.fill
        ref.null eq
        local.set 12
        local.get 25
        i32.const 144
        i32.add
        local.set 17
        local.get 12
        local.get 13
        local.get 14
        local.get 15
        local.get 16
        local.get 17
        call 13
        local.get 25
        i32.const 144
        i32.add
        local.set 19
        local.set 18
        local.get 18
        local.tee 31
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 31
        ref.cast (ref 10)
        local.get 19
        local.get 18
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 15
        i32.add
        call_indirect (type 276)
        local.get 1
        i32.const 255
        local.get 25
        i32.const 160
        i32.add
        call 86
        local.set 11
        local.set 10
        local.get 10
        local.tee 31
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 31
        ref.cast (ref 10)
        local.get 11
        local.get 10
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 10
        i32.add
        call_indirect (type 248)
        local.get 1
        local.get 2
        f64.promote_f32
        i32.const 10
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 8
        f32.demote_f64
        local.get 8
        f32.add
        f64.promote_f32
        local.get 3
        f64.promote_f32
        i32.const 150
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 8
        f32.demote_f64
        local.get 8
        f32.add
        f64.promote_f32
        i32.const 20
        f32.convert_i32_s
        f64.promote_f32
        i32.const 20
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 16
        f32.demote_f64
        local.set 15
        f32.demote_f64
        local.set 14
        f32.demote_f64
        local.set 13
        local.get 25
        i32.const 176
        i32.add
        i32.const 0
        i32.const 16
        memory.fill
        ref.null eq
        local.set 12
        local.get 25
        i32.const 176
        i32.add
        local.set 17
        local.get 12
        local.get 13
        local.get 14
        local.get 15
        local.get 16
        local.get 17
        call 13
        local.get 25
        i32.const 176
        i32.add
        local.set 19
        local.set 18
        local.get 18
        local.tee 31
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 31
        ref.cast (ref 10)
        local.get 19
        local.get 18
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 15
        i32.add
        call_indirect (type 276)
        local.get 25
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 20
    local.get 25
    global.set 27
    local.get 20
    throw_ref
  )
  (func (;84;) (type 216) (param eqref)
    (local eqref eqref i32 i64 f64 externref)
    global.get 3
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 7
      array.new_data 0 3
      i32.const 0
      i32.const 7
      call 1
      global.set 3
    end
    global.get 3
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    global.get 1
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 0
      array.new_data 0 1
      i32.const 0
      i32.const 0
      call 1
      global.set 1
    end
    global.get 1
    call 0
    return
  )
  (func (;85;) (type 217) (param eqref eqref)
    (local i32 i64 f64 externref eqref eqref i32 eqref eqref eqref eqref eqref i32 i64 f64 externref)
    local.get 0
    local.set 6
    local.get 6
    call 68
    i32.const 64
    i32.and
    i32.eqz
    if (type 218) ;; label = @1
      local.get 0
      local.get 0
      local.set 6
      local.get 6
      call 68
      i32.const 64
      i32.or
      local.set 8
      local.set 7
      local.get 7
      local.get 8
      call 71
      local.get 0
      local.set 9
      local.get 9
      call 60
      local.tee 12
      local.get 12
      ref.is_null
      i32.eqz
      if (type 219) (param eqref) ;; label = @2
        local.get 0
        local.set 11
        local.set 10
        local.get 10
        local.tee 13
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 13
        ref.cast (ref 13)
        local.get 11
        local.get 10
        ref.cast (ref 10)
        struct.get 10 0
        ref.cast (ref 9)
        struct.get 9 2
        i32.const 45
        i32.add
        call_indirect (type 217)
        return
      else
        drop
        return
      end
      unreachable
    else
      return
    end
    unreachable
  )
  (func (;86;) (type 220) (param i32 i32) (result i32)
    (local eqref i32 eqref i32 eqref i32 i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 14
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 14
    i32.const 16
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.set 7
        local.get 14
        i32.const 0
        i32.add
        i32.const 0
        i32.const 16
        memory.fill
        ref.null eq
        local.set 6
        local.get 14
        i32.const 0
        i32.add
        local.set 8
        local.get 6
        local.get 7
        local.get 8
        call 98
        local.get 14
        i32.const 0
        i32.add
        local.set 15
        local.get 1
        local.get 15
        i32.const 16
        memory.copy
        local.get 1
        local.get 14
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 9
    local.get 14
    global.set 27
    local.get 9
    throw_ref
  )
  (func (;87;) (type 221) (param eqref f32 externref i32 i32 i32 i32 i32)
    (local i32 i32 i32 i32 eqref i32 eqref i32 f32 eqref externref i32 eqref f32 i32 eqref i32 eqref f32 i32 eqref i32 eqref i32 i32 eqref i32 eqref i32 eqref i32 i32 eqref i32 eqref f32 i32 eqref i32 eqref i32 i32 eqref i32 eqref i32 eqref f32 i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 62
    i32.const 1048432
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 62
    i32.const 144
    i32.add
    global.set 27
    local.get 62
    i32.const 36
    i32.add
    local.set 8
    local.get 8
    i32.const 0
    i32.const 8
    memory.fill
    local.get 62
    i32.const 44
    i32.add
    local.set 9
    local.get 9
    i32.const 0
    i32.const 12
    memory.fill
    local.get 62
    i32.const 56
    i32.add
    local.set 10
    local.get 10
    i32.const 0
    i32.const 8
    memory.fill
    local.get 62
    i32.const 64
    i32.add
    local.set 11
    local.get 11
    i32.const 0
    i32.const 8
    memory.fill
    local.get 62
    i32.const 0
    i32.add
    local.get 3
    i32.const 8
    memory.copy
    local.get 62
    i32.const 0
    i32.add
    local.set 3
    local.get 62
    i32.const 8
    i32.add
    local.get 4
    i32.const 12
    memory.copy
    local.get 62
    i32.const 8
    i32.add
    local.set 4
    local.get 62
    i32.const 20
    i32.add
    local.get 5
    i32.const 8
    memory.copy
    local.get 62
    i32.const 20
    i32.add
    local.set 5
    local.get 62
    i32.const 28
    i32.add
    local.get 6
    i32.const 8
    memory.copy
    local.get 62
    i32.const 28
    i32.add
    local.set 6
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 7
        local.get 2
        local.set 18
        local.set 19
        local.set 17
        local.get 17
        local.get 18
        local.get 19
        call 101
        local.get 0
        local.get 7
        local.get 1
        f64.promote_f32
        f32.demote_f64
        local.set 21
        local.set 22
        local.set 20
        local.get 20
        local.get 21
        local.get 22
        call 102
        local.get 0
        local.get 7
        local.get 3
        local.set 63
        local.get 62
        i32.const 72
        i32.add
        local.get 63
        i32.const 8
        memory.copy
        local.get 62
        i32.const 72
        i32.add
        local.set 63
        local.get 8
        local.get 63
        i32.const 8
        memory.copy
        local.get 8
        local.set 13
        ref.null eq
        local.get 13
        local.set 24
        local.set 23
        local.get 23
        local.get 24
        call 103
        if (type 222) (result i32) ;; label = @3
          local.get 8
          local.set 13
          ref.null eq
          local.get 13
          local.set 29
          local.set 28
          local.get 28
          local.get 29
          local.get 62
          i32.const 84
          i32.add
          call 105
        else
          i32.const 400
          f32.convert_i32_s
          f64.promote_f32
          f32.demote_f64
          local.set 26
          local.get 62
          i32.const 80
          i32.add
          i32.const 0
          i32.const 4
          memory.fill
          ref.null eq
          local.set 25
          local.get 62
          i32.const 80
          i32.add
          local.set 27
          local.get 25
          local.get 26
          local.get 27
          call 104
          local.get 62
          i32.const 80
          i32.add
        end
        local.set 31
        local.set 32
        local.set 30
        local.get 30
        local.get 31
        local.get 32
        call 106
        local.get 0
        local.get 7
        local.get 4
        local.set 63
        local.get 62
        i32.const 88
        i32.add
        local.get 63
        i32.const 12
        memory.copy
        local.get 62
        i32.const 88
        i32.add
        local.set 63
        local.get 9
        local.get 63
        i32.const 12
        memory.copy
        local.get 9
        local.set 13
        ref.null eq
        local.get 13
        local.set 34
        local.set 33
        local.get 33
        local.get 34
        call 107
        if (type 223) (result i32) ;; label = @3
          local.get 9
          local.set 13
          ref.null eq
          local.get 13
          local.set 36
          local.set 35
          local.get 35
          local.get 36
          local.get 62
          i32.const 108
          i32.add
          call 109
        else
          local.get 62
          i32.const 100
          i32.add
          call 108
        end
        local.set 38
        local.set 39
        local.set 37
        local.get 37
        local.get 38
        local.get 39
        call 110
        local.get 0
        local.get 7
        local.get 5
        local.set 63
        local.get 62
        i32.const 116
        i32.add
        local.get 63
        i32.const 8
        memory.copy
        local.get 62
        i32.const 116
        i32.add
        local.set 63
        local.get 10
        local.get 63
        i32.const 8
        memory.copy
        local.get 10
        local.set 13
        ref.null eq
        local.get 13
        local.set 41
        local.set 40
        local.get 40
        local.get 41
        call 111
        if (type 224) (result i32) ;; label = @3
          local.get 10
          local.set 13
          ref.null eq
          local.get 13
          local.set 46
          local.set 45
          local.get 45
          local.get 46
          local.get 62
          i32.const 128
          i32.add
          call 113
        else
          i32.const 100
          f32.convert_i32_s
          f64.promote_f32
          f32.demote_f64
          local.set 43
          local.get 62
          i32.const 124
          i32.add
          i32.const 0
          i32.const 4
          memory.fill
          ref.null eq
          local.set 42
          local.get 62
          i32.const 124
          i32.add
          local.set 44
          local.get 42
          local.get 43
          local.get 44
          call 112
          local.get 62
          i32.const 124
          i32.add
        end
        local.set 48
        local.set 49
        local.set 47
        local.get 47
        local.get 48
        local.get 49
        call 114
        local.get 0
        local.get 7
        local.get 6
        local.set 63
        local.get 62
        i32.const 132
        i32.add
        local.get 63
        i32.const 8
        memory.copy
        local.get 62
        i32.const 132
        i32.add
        local.set 63
        local.get 11
        local.get 63
        i32.const 8
        memory.copy
        local.get 11
        local.set 13
        ref.null eq
        local.get 13
        local.set 51
        local.set 50
        local.get 50
        local.get 51
        call 115
        if (type 225) (result f64) ;; label = @3
          local.get 11
          local.set 13
          ref.null eq
          local.get 13
          local.set 53
          local.set 52
          local.get 52
          local.get 53
          call 116
          f64.promote_f32
        else
          local.get 1
          f64.promote_f32
          f32.const 0x1.333334p+0 (;=1.2;)
          f64.promote_f32
          f32.demote_f64
          f64.promote_f32
          f32.demote_f64
          local.set 16
          f32.demote_f64
          local.get 16
          f32.mul
          f64.promote_f32
        end
        f32.demote_f64
        local.set 55
        local.set 56
        local.set 54
        local.get 54
        local.get 55
        local.get 56
        call 117
        local.get 62
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 57
    local.get 62
    global.set 27
    local.get 57
    throw_ref
  )
  (func (;88;) (type 226) (param eqref eqref eqref)
    (local i32 i32 eqref i32 eqref i32 eqref i32 eqref i32 eqref eqref i32 eqref eqref exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 23
    i32.const 1048480
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 23
    i32.const 96
    i32.add
    global.set 27
    local.get 23
    i32.const 0
    i32.add
    local.set 3
    local.get 3
    i32.const 0
    i32.const 16
    memory.fill
    local.get 23
    i32.const 16
    i32.add
    local.set 4
    local.get 4
    i32.const 0
    i32.const 20
    memory.fill
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.tee 29
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 29
        ref.cast (ref 13)
        local.set 29
        local.get 23
        i32.const 36
        i32.add
        local.get 29
        ref.cast (ref 13)
        struct.get 13 23
        i32.store8
        local.get 23
        i32.const 40
        i32.add
        local.get 29
        ref.cast (ref 13)
        struct.get 13 24
        f32.store align=1
        local.get 23
        i32.const 44
        i32.add
        local.get 29
        ref.cast (ref 13)
        struct.get 13 25
        f32.store align=1
        local.get 23
        i32.const 48
        i32.add
        local.get 29
        ref.cast (ref 13)
        struct.get 13 26
        f32.store align=1
        local.get 23
        i32.const 52
        i32.add
        local.get 29
        ref.cast (ref 13)
        struct.get 13 27
        f32.store align=1
        local.get 23
        i32.const 36
        i32.add
        local.set 24
        local.get 4
        local.get 24
        i32.const 20
        memory.copy
        local.get 4
        local.set 6
        ref.null eq
        local.get 6
        local.set 10
        local.set 9
        local.get 9
        local.get 10
        call 59
        i32.eqz
        if (type 227) ;; label = @3
          local.get 2
          local.get 1
          local.set 17
          local.set 16
          local.get 16
          local.tee 29
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 29
          ref.cast (ref 13)
          local.get 17
          call 12
          local.get 23
          global.set 27
          return
        else
          local.get 4
          local.set 6
          ref.null eq
          local.get 6
          local.set 12
          local.set 11
          local.get 11
          local.get 12
          local.get 23
          i32.const 56
          i32.add
          call 62
          local.set 24
          local.get 3
          local.get 24
          i32.const 16
          memory.copy
          local.get 2
          local.get 1
          local.get 3
          local.set 24
          local.get 23
          i32.const 72
          i32.add
          local.get 24
          i32.const 16
          memory.copy
          local.get 23
          i32.const 72
          i32.add
          local.set 15
          local.set 14
          local.set 13
          local.get 13
          local.tee 29
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 29
          ref.cast (ref 13)
          local.get 14
          local.get 15
          call 122
          local.get 23
          global.set 27
          return
        end
        unreachable
        unreachable
      end
      unreachable
    end
    local.set 18
    local.get 23
    global.set 27
    local.get 18
    throw_ref
  )
  (func (;89;) (type 228) (param eqref i32) (result eqref)
    (local eqref eqref i32 i64 f64 externref)
    i32.const 7
    global.get 26
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 42
      array.new_data 0 26
      i32.const 0
      i32.const 42
      call 1
      global.set 26
    end
    global.get 26
    struct.new 3
    local.tee 3
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 23
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 23
        i32.const 0
        i32.const 53
        call 1
        global.set 23
      end
      global.get 23
      struct.new 3
      throw 0
    end
    local.get 3
    throw 0
  )
  (func (;90;) (type 229) (param eqref) (result i32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.tee 2
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 23
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 23
        i32.const 0
        i32.const 53
        call 1
        global.set 23
      end
      global.get 23
      struct.new 3
      throw 0
    end
    local.get 2
    ref.cast (ref 13)
    struct.get 13 2
    return
  )
  (func (;91;) (type 231) (param eqref f32)
    (local f32 eqref eqref i32 i64 f64 externref)
    global.get 4
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 11
      array.new_data 0 4
      i32.const 0
      i32.const 11
      call 1
      global.set 4
    end
    global.get 4
    local.get 1
    f64.promote_f32
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    global.get 1
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 0
      array.new_data 0 1
      i32.const 0
      i32.const 0
      call 1
      global.set 1
    end
    global.get 1
    call 0
    return
  )
  (func (;92;) (type 233) (param eqref i32)
    (local eqref eqref i32 i64 f64 externref)
    global.get 5
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 7
      array.new_data 0 5
      i32.const 0
      i32.const 7
      call 1
      global.set 5
    end
    global.get 5
    local.get 1
    f64.convert_i32_s
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    global.get 1
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 0
      array.new_data 0 1
      i32.const 0
      i32.const 0
      call 1
      global.set 1
    end
    global.get 1
    call 0
    return
  )
  (func (;93;) (type 235) (param eqref i32)
    (local eqref eqref i32 i64 f64 externref)
    global.get 6
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 8
      array.new_data 0 6
      i32.const 0
      i32.const 8
      call 1
      global.set 6
    end
    global.get 6
    local.get 1
    f64.convert_i32_s
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    global.get 1
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 0
      array.new_data 0 1
      i32.const 0
      i32.const 0
      call 1
      global.set 1
    end
    global.get 1
    call 0
    return
  )
  (func (;94;) (type 237) (param eqref f32)
    (local f32 eqref eqref i32 i64 f64 externref)
    global.get 7
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 9
      array.new_data 0 7
      i32.const 0
      i32.const 9
      call 1
      global.set 7
    end
    global.get 7
    local.get 1
    f64.promote_f32
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    global.get 1
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 0
      array.new_data 0 1
      i32.const 0
      i32.const 0
      call 1
      global.set 1
    end
    global.get 1
    call 0
    return
  )
  (func (;95;) (type 239) (param eqref f32)
    (local f32 eqref eqref i32 i64 f64 externref)
    global.get 8
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 10
      array.new_data 0 8
      i32.const 0
      i32.const 10
      call 1
      global.set 8
    end
    global.get 8
    local.get 1
    f64.promote_f32
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    global.get 1
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 0
      array.new_data 0 1
      i32.const 0
      i32.const 0
      call 1
      global.set 1
    end
    global.get 1
    call 0
    return
  )
  (func (;96;) (type 241) (param eqref f32)
    (local f32 eqref eqref i32 i64 f64 externref)
    global.get 9
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 14
      array.new_data 0 9
      i32.const 0
      i32.const 14
      call 1
      global.set 9
    end
    global.get 9
    local.get 1
    f64.promote_f32
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    global.get 1
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 0
      array.new_data 0 1
      i32.const 0
      i32.const 0
      call 1
      global.set 1
    end
    global.get 1
    call 0
    return
  )
  (func (;97;) (type 243) (param eqref i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 11
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 11
    i32.const 16
    i32.add
    global.set 27
    global.get 28
    local.tee 8
    i32.const 65535
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 8
    i32.const 1
    i32.add
    global.set 28
    local.get 11
    i32.const 0
    i32.add
    local.get 8
    i32.const 0
    i32.add
    i32.store align=1
    local.get 11
    i32.const 0
    i32.add
    local.get 1
    local.set 9
    local.set 10
    local.get 10
    i32.const 0
    i32.add
    local.get 9
    i32.const 0
    i32.add
    i32.load align=1
    table.get 1
    local.set 7
    i32.load align=1
    local.get 7
    table.set 1
    local.get 10
    i32.const 4
    i32.add
    local.get 9
    i32.const 4
    i32.add
    i32.load align=1
    i32.store align=1
    local.get 10
    i32.const 8
    i32.add
    local.get 9
    i32.const 8
    i32.add
    i32.load align=1
    i32.store align=1
    local.get 11
    i32.const 0
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 1
        local.set 3
        ref.null eq
        local.get 3
        local.set 3
        local.set 2
        local.get 2
        ref.is_null
        local.get 3
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 2
        ref.is_null
        i32.eqz
        if ;; label = @3
          unreachable
        end
        local.get 3
        i32.const 8
        i32.add
        i32.load align=1
        i32.eqz
        if (type 244) ;; label = @3
          global.get 11
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 11
            array.new_data 0 11
            i32.const 0
            i32.const 11
            call 1
            global.set 11
          end
          global.get 11
          f64.const 0x0p+0 (;=0;)
          f64.const 0x0p+0 (;=0;)
          f64.const 0x0p+0 (;=0;)
          f64.const 0x0p+0 (;=0;)
          global.get 1
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 0
            array.new_data 0 1
            i32.const 0
            i32.const 0
            call 1
            global.set 1
          end
          global.get 1
          call 0
          local.get 8
          ref.null eq
          i32.const 1
          table.fill 1
          local.get 8
          ref.null extern
          i32.const 1
          table.fill 2
          local.get 8
          global.set 28
          local.get 11
          global.set 27
          return
        else
          global.get 10
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 10
            i32.const 0
            i32.const 53
            call 1
            global.set 10
          end
          global.get 10
          local.set 21
          local.get 21
          ref.is_null
          if ;; label = @4
            global.get 24
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 62
              array.new_data 0 24
              i32.const 0
              i32.const 62
              call 1
              global.set 24
            end
            global.get 24
            local.set 21
          end
          i32.const 5
          local.get 21
          struct.new 3
          local.tee 17
          ref.is_null
          if ;; label = @4
            i32.const 4
            global.get 23
            ref.is_null
            if ;; label = @5
              i32.const 0
              i32.const 53
              array.new_data 0 23
              i32.const 0
              i32.const 53
              call 1
              global.set 23
            end
            global.get 23
            struct.new 3
            throw 0
          end
          local.get 17
          throw 0
        end
        unreachable
        unreachable
      end
      unreachable
    end
    local.set 6
    local.get 8
    ref.null eq
    i32.const 1
    table.fill 1
    local.get 8
    ref.null extern
    i32.const 1
    table.fill 2
    local.get 8
    global.set 28
    local.get 11
    global.set 27
    local.get 6
    throw_ref
  )
  (func (;98;) (type 245) (param eqref i32 i32)
    (local eqref i32 eqref i32 f32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 13
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 13
    i32.const 0
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 2
        local.get 1
        i32.const -16777216
        i32.and
        i32.const 24
        i32.shr_u
        f32.convert_i32_u
        f64.promote_f32
        i32.const 255
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 7
        f32.demote_f64
        local.get 7
        f32.div
        f64.promote_f32
        local.set 22
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
        else
          local.get 4
          i32.const 0
          i32.add
        end
        local.get 22
        f32.demote_f64
        call 139
        local.get 0
        local.get 2
        local.get 1
        i32.const 16711680
        i32.and
        i32.const 16
        i32.shr_u
        f32.convert_i32_u
        f64.promote_f32
        i32.const 255
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 7
        f32.demote_f64
        local.get 7
        f32.div
        f64.promote_f32
        local.set 22
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 4
          i32.add
        else
          local.get 4
          i32.const 1
          i32.add
        end
        local.get 22
        f32.demote_f64
        call 139
        local.get 0
        local.get 2
        local.get 1
        i32.const 65280
        i32.and
        i32.const 8
        i32.shr_u
        f32.convert_i32_u
        f64.promote_f32
        i32.const 255
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 7
        f32.demote_f64
        local.get 7
        f32.div
        f64.promote_f32
        local.set 22
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 8
          i32.add
        else
          local.get 4
          i32.const 2
          i32.add
        end
        local.get 22
        f32.demote_f64
        call 139
        local.get 0
        local.get 2
        local.get 1
        i32.const 255
        i32.and
        f32.convert_i32_u
        f64.promote_f32
        i32.const 255
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 7
        f32.demote_f64
        local.get 7
        f32.div
        f64.promote_f32
        local.set 22
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 12
          i32.add
        else
          local.get 4
          i32.const 3
          i32.add
        end
        local.get 22
        f32.demote_f64
        call 139
        local.get 13
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 8
    local.get 13
    global.set 27
    local.get 8
    throw_ref
  )
  (func (;99;) (type 247) (param eqref i32)
    (local f32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 8
    i32.const 1048496
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 8
    i32.const 80
    i32.add
    global.set 27
    local.get 8
    i32.const 0
    i32.add
    local.get 1
    i32.const 16
    memory.copy
    local.get 8
    i32.const 0
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        global.get 12
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 9
          array.new_data 0 12
          i32.const 0
          i32.const 9
          call 1
          global.set 12
        end
        global.get 12
        local.get 1
        local.set 9
        local.get 8
        i32.const 16
        i32.add
        local.get 9
        i32.const 16
        memory.copy
        local.get 8
        i32.const 16
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 9
        local.get 8
        i32.const 32
        i32.add
        local.get 9
        i32.const 16
        memory.copy
        local.get 8
        i32.const 32
        i32.add
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 9
        local.get 8
        i32.const 48
        i32.add
        local.get 9
        i32.const 16
        memory.copy
        local.get 8
        i32.const 48
        i32.add
        i32.const 8
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 9
        local.get 8
        i32.const 64
        i32.add
        local.get 9
        i32.const 16
        memory.copy
        local.get 8
        i32.const 64
        i32.add
        i32.const 12
        i32.add
        f32.load align=1
        f64.promote_f32
        global.get 1
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 0
          array.new_data 0 1
          i32.const 0
          i32.const 0
          call 1
          global.set 1
        end
        global.get 1
        call 0
        local.get 8
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 3
    local.get 8
    global.set 27
    local.get 3
    throw_ref
  )
  (func (;100;) (type 249) (param eqref i32)
    (local f32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 8
    i32.const 1048496
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 8
    i32.const 80
    i32.add
    global.set 27
    local.get 8
    i32.const 0
    i32.add
    local.get 1
    i32.const 16
    memory.copy
    local.get 8
    i32.const 0
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        global.get 13
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 7
          array.new_data 0 13
          i32.const 0
          i32.const 7
          call 1
          global.set 13
        end
        global.get 13
        local.get 1
        local.set 9
        local.get 8
        i32.const 16
        i32.add
        local.get 9
        i32.const 16
        memory.copy
        local.get 8
        i32.const 16
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 9
        local.get 8
        i32.const 32
        i32.add
        local.get 9
        i32.const 16
        memory.copy
        local.get 8
        i32.const 32
        i32.add
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 9
        local.get 8
        i32.const 48
        i32.add
        local.get 9
        i32.const 16
        memory.copy
        local.get 8
        i32.const 48
        i32.add
        i32.const 8
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 9
        local.get 8
        i32.const 64
        i32.add
        local.get 9
        i32.const 16
        memory.copy
        local.get 8
        i32.const 64
        i32.add
        i32.const 12
        i32.add
        f32.load align=1
        f64.promote_f32
        global.get 1
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 0
          array.new_data 0 1
          i32.const 0
          i32.const 0
          call 1
          global.set 1
        end
        global.get 1
        call 0
        local.get 8
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 3
    local.get 8
    global.set 27
    local.get 3
    throw_ref
  )
  (func (;101;) (type 250) (param eqref externref i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 12
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 12
    i32.const 0
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 2
        local.get 1
        local.set 22
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
          i32.load align=1
        else
          local.get 4
          i32.const 0
          i32.add
        end
        local.get 22
        call 137
        local.get 12
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 7
    local.get 12
    global.set 27
    local.get 7
    throw_ref
  )
  (func (;102;) (type 251) (param eqref f32 i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 12
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 12
    i32.const 0
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 2
        local.get 1
        f64.promote_f32
        local.set 21
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 4
          i32.add
        else
          local.get 4
          i32.const 1
          i32.add
        end
        local.get 21
        f32.demote_f64
        call 139
        local.get 12
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 7
    local.get 12
    global.set 27
    local.get 7
    throw_ref
  )
  (func (;103;) (type 252) (param eqref i32) (result i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 11
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 11
    i32.const 0
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 3
        local.set 2
        local.get 2
        ref.is_null
        local.get 3
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 2
        local.get 2
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 3
          i32.const 0
          i32.add
        else
          local.get 3
          i32.const 0
          i32.add
        end
        call 144
        i32.const 255
        i32.and
        local.get 11
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 6
    local.get 11
    global.set 27
    local.get 6
    throw_ref
  )
  (func (;104;) (type 253) (param eqref f32 i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 12
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 12
    i32.const 0
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 2
        local.get 1
        f64.promote_f32
        local.set 21
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
        else
          local.get 4
          i32.const 0
          i32.add
        end
        local.get 21
        f32.demote_f64
        call 139
        local.get 12
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 7
    local.get 12
    global.set 27
    local.get 7
    throw_ref
  )
  (func (;105;) (type 254) (param eqref i32 i32) (result i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 12
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 12
    i32.const 16
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 4
          i32.add
        else
          local.get 4
          i32.const 1
          i32.add
        end
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 12
        i32.const 0
        i32.add
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
        else
          local.get 4
          i32.const 0
          i32.add
        end
        call 138
        f32.store align=1
        local.get 12
        i32.const 0
        i32.add
        local.set 13
        local.get 2
        local.get 13
        i32.const 4
        memory.copy
        local.get 2
        local.get 12
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 7
    local.get 12
    global.set 27
    local.get 7
    throw_ref
  )
  (func (;106;) (type 255) (param eqref i32 i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 12
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 12
    i32.const 16
    i32.add
    global.set 27
    local.get 12
    i32.const 0
    i32.add
    local.get 1
    i32.const 4
    memory.copy
    local.get 12
    i32.const 0
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 2
        local.get 1
        local.set 13
        local.get 12
        i32.const 4
        i32.add
        local.get 13
        i32.const 4
        memory.copy
        local.get 12
        i32.const 4
        i32.add
        local.set 19
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 16
          i32.add
        else
          local.get 4
          i32.const 5
          i32.add
        end
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
        else
          local.get 4
          i32.const 0
          i32.add
        end
        local.get 19
        i32.const 0
        i32.add
        f32.load align=1
        call 139
        local.get 12
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 7
    local.get 12
    global.set 27
    local.get 7
    throw_ref
  )
  (func (;107;) (type 256) (param eqref i32) (result i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 11
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 11
    i32.const 0
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 3
        local.set 2
        local.get 2
        ref.is_null
        local.get 3
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 2
        local.get 2
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 3
          i32.const 0
          i32.add
        else
          local.get 3
          i32.const 0
          i32.add
        end
        call 144
        i32.const 255
        i32.and
        local.get 11
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 6
    local.get 11
    global.set 27
    local.get 6
    throw_ref
  )
  (func (;108;) (type 257) (param i32) (result i32)
    (local eqref i32 eqref i32 f32 eqref i32 i32 f32 i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 16
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 16
    i32.const 16
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        i32.const 0
        i32.const 0
        f32.const 0x1.cp+3 (;=14;)
        f64.promote_f32
        f32.demote_f64
        f64.promote_f32
        f32.demote_f64
        local.set 9
        i32.const 255
        i32.and
        local.set 8
        i32.const 255
        i32.and
        local.set 7
        local.get 16
        i32.const 0
        i32.add
        i32.const 0
        i32.const 8
        memory.fill
        ref.null eq
        local.set 6
        local.get 16
        i32.const 0
        i32.add
        local.set 10
        local.get 6
        local.get 7
        local.get 8
        local.get 9
        local.get 10
        call 124
        local.get 16
        i32.const 0
        i32.add
        local.set 17
        local.get 0
        local.get 17
        i32.const 8
        memory.copy
        local.get 0
        local.get 16
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 11
    local.get 16
    global.set 27
    local.get 11
    throw_ref
  )
  (func (;109;) (type 258) (param eqref i32 i32) (result i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 12
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 12
    i32.const 16
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 4
          i32.add
        else
          local.get 4
          i32.const 1
          i32.add
        end
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 12
        i32.const 0
        i32.add
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
        else
          local.get 4
          i32.const 0
          i32.add
        end
        call 144
        i32.store8
        local.get 12
        i32.const 1
        i32.add
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 1
          i32.add
        else
          local.get 4
          i32.const 1
          i32.add
        end
        call 144
        i32.store8
        local.get 12
        i32.const 4
        i32.add
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 4
          i32.add
        else
          local.get 4
          i32.const 2
          i32.add
        end
        call 138
        f32.store align=1
        local.get 12
        i32.const 0
        i32.add
        local.set 13
        local.get 2
        local.get 13
        i32.const 8
        memory.copy
        local.get 2
        local.get 12
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 7
    local.get 12
    global.set 27
    local.get 7
    throw_ref
  )
  (func (;110;) (type 259) (param eqref i32 i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 12
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 12
    i32.const 16
    i32.add
    global.set 27
    local.get 12
    i32.const 0
    i32.add
    local.get 1
    i32.const 8
    memory.copy
    local.get 12
    i32.const 0
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 2
        local.get 1
        local.set 13
        local.get 12
        i32.const 8
        i32.add
        local.get 13
        i32.const 8
        memory.copy
        local.get 12
        i32.const 8
        i32.add
        local.set 19
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 8
          i32.add
        else
          local.get 4
          i32.const 2
          i32.add
        end
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
        else
          local.get 4
          i32.const 0
          i32.add
        end
        local.get 19
        i32.const 0
        i32.add
        i32.load8_u
        call 145
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 1
          i32.add
        else
          local.get 4
          i32.const 1
          i32.add
        end
        local.get 19
        i32.const 1
        i32.add
        i32.load8_u
        call 145
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 4
          i32.add
        else
          local.get 4
          i32.const 2
          i32.add
        end
        local.get 19
        i32.const 4
        i32.add
        f32.load align=1
        call 139
        local.get 12
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 7
    local.get 12
    global.set 27
    local.get 7
    throw_ref
  )
  (func (;111;) (type 260) (param eqref i32) (result i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 11
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 11
    i32.const 0
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 3
        local.set 2
        local.get 2
        ref.is_null
        local.get 3
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 2
        local.get 2
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 3
          i32.const 0
          i32.add
        else
          local.get 3
          i32.const 0
          i32.add
        end
        call 144
        i32.const 255
        i32.and
        local.get 11
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 6
    local.get 11
    global.set 27
    local.get 6
    throw_ref
  )
  (func (;112;) (type 261) (param eqref f32 i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 12
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 12
    i32.const 0
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 2
        local.get 1
        f64.promote_f32
        local.set 21
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
        else
          local.get 4
          i32.const 0
          i32.add
        end
        local.get 21
        f32.demote_f64
        call 139
        local.get 12
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 7
    local.get 12
    global.set 27
    local.get 7
    throw_ref
  )
  (func (;113;) (type 262) (param eqref i32 i32) (result i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 12
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 12
    i32.const 16
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 4
          i32.add
        else
          local.get 4
          i32.const 1
          i32.add
        end
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 12
        i32.const 0
        i32.add
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
        else
          local.get 4
          i32.const 0
          i32.add
        end
        call 138
        f32.store align=1
        local.get 12
        i32.const 0
        i32.add
        local.set 13
        local.get 2
        local.get 13
        i32.const 4
        memory.copy
        local.get 2
        local.get 12
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 7
    local.get 12
    global.set 27
    local.get 7
    throw_ref
  )
  (func (;114;) (type 263) (param eqref i32 i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 12
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 12
    i32.const 16
    i32.add
    global.set 27
    local.get 12
    i32.const 0
    i32.add
    local.get 1
    i32.const 4
    memory.copy
    local.get 12
    i32.const 0
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 2
        local.get 1
        local.set 13
        local.get 12
        i32.const 4
        i32.add
        local.get 13
        i32.const 4
        memory.copy
        local.get 12
        i32.const 4
        i32.add
        local.set 19
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 20
          i32.add
        else
          local.get 4
          i32.const 6
          i32.add
        end
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
        else
          local.get 4
          i32.const 0
          i32.add
        end
        local.get 19
        i32.const 0
        i32.add
        f32.load align=1
        call 139
        local.get 12
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 7
    local.get 12
    global.set 27
    local.get 7
    throw_ref
  )
  (func (;115;) (type 264) (param eqref i32) (result i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 11
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 11
    i32.const 0
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 3
        local.set 2
        local.get 2
        ref.is_null
        local.get 3
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 2
        local.get 2
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 3
          i32.const 0
          i32.add
        else
          local.get 3
          i32.const 0
          i32.add
        end
        call 144
        i32.const 255
        i32.and
        local.get 11
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 6
    local.get 11
    global.set 27
    local.get 6
    throw_ref
  )
  (func (;116;) (type 265) (param eqref i32) (result f32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 11
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 11
    i32.const 0
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 3
        local.set 2
        local.get 2
        ref.is_null
        local.get 3
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 2
        local.get 2
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 3
          i32.const 4
          i32.add
        else
          local.get 3
          i32.const 1
          i32.add
        end
        call 138
        f64.promote_f32
        f32.demote_f64
        local.get 11
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 6
    local.get 11
    global.set 27
    local.get 6
    throw_ref
  )
  (func (;117;) (type 266) (param eqref f32 i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 12
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 12
    i32.const 0
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 2
        local.get 1
        f64.promote_f32
        local.set 21
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 24
          i32.add
        else
          local.get 4
          i32.const 7
          i32.add
        end
        local.get 21
        f32.demote_f64
        call 139
        local.get 12
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 7
    local.get 12
    global.set 27
    local.get 7
    throw_ref
  )
  (func (;118;) (type 268) (param eqref i32)
    (local i32 eqref i32 eqref i32 f32 eqref i32 eqref i32 eqref i32 eqref i32 eqref i32 eqref i32 eqref i32 eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 31
    i32.const 1048496
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 31
    i32.const 80
    i32.add
    global.set 27
    local.get 31
    i32.const 28
    i32.add
    local.set 2
    local.get 2
    i32.const 0
    i32.const 8
    memory.fill
    global.get 28
    local.tee 28
    i32.const 65535
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 28
    i32.const 1
    i32.add
    global.set 28
    local.get 31
    i32.const 0
    i32.add
    local.get 28
    i32.const 0
    i32.add
    i32.store align=1
    local.get 31
    i32.const 0
    i32.add
    local.get 1
    local.set 29
    local.set 30
    local.get 30
    i32.const 0
    i32.add
    local.get 29
    i32.const 0
    i32.add
    i32.load align=1
    table.get 2
    local.set 41
    i32.load align=1
    local.get 41
    table.set 2
    local.get 30
    i32.const 4
    i32.add
    local.get 29
    i32.const 4
    i32.add
    f32.load align=1
    f32.store align=1
    local.get 30
    i32.const 8
    i32.add
    local.get 29
    i32.const 8
    i32.add
    i32.load8_u
    i32.store8
    local.get 30
    i32.const 9
    i32.add
    local.get 29
    i32.const 9
    i32.add
    i32.load8_u
    i32.store8
    local.get 30
    i32.const 12
    i32.add
    local.get 29
    i32.const 12
    i32.add
    f32.load align=1
    f32.store align=1
    local.get 30
    i32.const 16
    i32.add
    local.get 29
    i32.const 16
    i32.add
    f32.load align=1
    f32.store align=1
    local.get 30
    i32.const 20
    i32.add
    local.get 29
    i32.const 20
    i32.add
    f32.load align=1
    f32.store align=1
    local.get 30
    i32.const 24
    i32.add
    local.get 29
    i32.const 24
    i32.add
    f32.load align=1
    f32.store align=1
    local.get 31
    i32.const 0
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        global.get 14
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 4
          array.new_data 0 14
          i32.const 0
          i32.const 4
          call 1
          global.set 14
        end
        global.get 14
        local.get 1
        local.set 4
        ref.null eq
        local.get 4
        local.set 9
        local.set 8
        local.get 8
        local.get 9
        call 125
        f64.promote_f32
        local.get 1
        local.set 4
        ref.null eq
        local.get 4
        local.set 11
        local.set 10
        local.get 10
        local.get 11
        local.get 31
        i32.const 36
        i32.add
        call 126
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 4
        ref.null eq
        local.get 4
        local.set 13
        local.set 12
        local.get 12
        local.get 13
        local.get 31
        i32.const 40
        i32.add
        call 127
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 4
        ref.null eq
        local.get 4
        local.set 15
        local.set 14
        local.get 14
        local.get 15
        call 128
        f64.promote_f32
        local.get 1
        local.set 4
        ref.null eq
        local.get 4
        local.set 17
        local.set 16
        local.get 16
        local.get 17
        call 129
        call 0
        global.get 15
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 9
          array.new_data 0 15
          i32.const 0
          i32.const 9
          call 1
          global.set 15
        end
        global.get 15
        local.get 1
        local.set 4
        ref.null eq
        local.get 4
        local.set 19
        local.set 18
        local.get 18
        local.get 19
        local.get 31
        i32.const 44
        i32.add
        call 130
        local.set 32
        local.get 2
        local.get 32
        i32.const 8
        memory.copy
        local.get 2
        local.set 4
        ref.null eq
        local.get 4
        local.set 21
        local.set 20
        local.get 20
        local.get 21
        call 131
        i32.const 0
        i32.gt_u
        f64.convert_i32_s
        local.get 1
        local.set 4
        ref.null eq
        local.get 4
        local.set 19
        local.set 18
        local.get 18
        local.get 19
        local.get 31
        i32.const 52
        i32.add
        call 130
        local.set 32
        local.get 2
        local.get 32
        i32.const 8
        memory.copy
        local.get 2
        local.set 4
        ref.null eq
        local.get 4
        local.set 23
        local.set 22
        local.get 22
        local.get 23
        call 132
        i32.const 0
        i32.gt_u
        f64.convert_i32_s
        local.get 1
        local.set 4
        ref.null eq
        local.get 4
        local.set 19
        local.set 18
        local.get 18
        local.get 19
        local.get 31
        i32.const 60
        i32.add
        call 130
        local.set 32
        local.get 2
        local.get 32
        i32.const 8
        memory.copy
        local.get 2
        local.set 4
        ref.null eq
        local.get 4
        local.set 25
        local.set 24
        local.get 24
        local.get 25
        call 133
        f64.promote_f32
        f64.const 0x0p+0 (;=0;)
        global.get 1
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 0
          array.new_data 0 1
          i32.const 0
          i32.const 0
          call 1
          global.set 1
        end
        global.get 1
        call 0
        local.get 28
        ref.null eq
        i32.const 1
        table.fill 1
        local.get 28
        ref.null extern
        i32.const 1
        table.fill 2
        local.get 28
        global.set 28
        local.get 31
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 26
    local.get 28
    ref.null eq
    i32.const 1
    table.fill 1
    local.get 28
    ref.null extern
    i32.const 1
    table.fill 2
    local.get 28
    global.set 28
    local.get 31
    global.set 27
    local.get 26
    throw_ref
  )
  (func (;119;) (type 270) (param eqref i32)
    (local eqref eqref i32 i64 f64 externref)
    global.get 16
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 9
      array.new_data 0 16
      i32.const 0
      i32.const 9
      call 1
      global.set 16
    end
    global.get 16
    local.get 1
    f64.convert_i32_s
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    global.get 1
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 0
      array.new_data 0 1
      i32.const 0
      i32.const 0
      call 1
      global.set 1
    end
    global.get 1
    call 0
    return
  )
  (func (;120;) (type 272) (param eqref i32)
    (local eqref eqref i32 i64 f64 externref)
    global.get 17
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 12
      array.new_data 0 17
      i32.const 0
      i32.const 12
      call 1
      global.set 17
    end
    global.get 17
    local.get 1
    f64.convert_i32_s
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    global.get 1
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 0
      array.new_data 0 1
      i32.const 0
      i32.const 0
      call 1
      global.set 1
    end
    global.get 1
    call 0
    return
  )
  (func (;121;) (type 274) (param eqref)
    (local eqref eqref i32 i64 f64 externref)
    global.get 18
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 9
      array.new_data 0 18
      i32.const 0
      i32.const 9
      call 1
      global.set 18
    end
    global.get 18
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    f64.const 0x0p+0 (;=0;)
    global.get 1
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 0
      array.new_data 0 1
      i32.const 0
      i32.const 0
      call 1
      global.set 1
    end
    global.get 1
    call 0
    return
  )
  (func (;122;) (type 275) (param eqref eqref i32)
    (local i32 eqref i32 eqref i32 eqref i32 i32 eqref eqref i32 i32 eqref i32 i32 eqref eqref i32 i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 27
    i32.const 1048464
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 27
    i32.const 112
    i32.add
    global.set 27
    local.get 27
    i32.const 16
    i32.add
    local.set 3
    local.get 3
    i32.const 0
    i32.const 32
    memory.fill
    global.get 28
    local.tee 24
    i32.const 65532
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 24
    i32.const 4
    i32.add
    global.set 28
    local.get 27
    i32.const 32
    i32.add
    local.get 24
    i32.const 0
    i32.add
    i32.store align=1
    local.get 27
    i32.const 36
    i32.add
    local.get 24
    i32.const 1
    i32.add
    i32.store align=1
    local.get 27
    i32.const 40
    i32.add
    local.get 24
    i32.const 2
    i32.add
    i32.store align=1
    local.get 27
    i32.const 64
    i32.add
    local.get 24
    i32.const 3
    i32.add
    i32.store align=1
    local.get 27
    i32.const 0
    i32.add
    local.get 2
    i32.const 16
    memory.copy
    local.get 27
    i32.const 0
    i32.add
    local.set 2
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 3
        local.set 5
        ref.null eq
        local.get 5
        local.get 27
        i32.const 48
        i32.add
        i32.const 0
        i32.const 8
        memory.fill
        local.get 27
        i32.const 48
        i32.add
        local.get 27
        i32.const 56
        i32.add
        i32.const 0
        i32.const 8
        memory.fill
        local.get 27
        i32.const 56
        i32.add
        local.get 1
        local.get 1
        local.get 0
        local.tee 33
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 33
        ref.cast (ref 13)
        local.set 33
        local.get 27
        i32.const 64
        i32.add
        local.get 33
        ref.cast (ref 13)
        struct.get 13 28
        local.set 23
        i32.load align=1
        local.get 23
        table.set 1
        local.get 27
        i32.const 64
        i32.add
        local.set 13
        local.set 12
        local.set 11
        local.set 10
        local.set 9
        local.set 14
        local.set 8
        local.get 8
        local.get 9
        local.get 10
        local.get 11
        local.get 12
        local.get 13
        local.get 14
        call 17
        local.get 0
        local.get 3
        local.set 5
        ref.null eq
        local.get 5
        local.get 2
        local.set 28
        local.get 27
        i32.const 68
        i32.add
        local.get 28
        i32.const 16
        memory.copy
        local.get 27
        i32.const 68
        i32.add
        local.set 16
        local.get 27
        i32.const 84
        i32.add
        i32.const 0
        i32.const 20
        memory.fill
        ref.null eq
        local.set 15
        local.get 27
        i32.const 84
        i32.add
        local.set 17
        local.get 15
        local.get 16
        local.get 17
        call 61
        local.get 27
        i32.const 84
        i32.add
        local.set 20
        local.set 21
        local.set 19
        local.set 18
        local.get 18
        local.get 19
        local.get 20
        local.get 21
        call 26
        local.get 24
        ref.null eq
        i32.const 4
        table.fill 1
        local.get 24
        ref.null extern
        i32.const 4
        table.fill 2
        local.get 24
        global.set 28
        local.get 27
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 22
    local.get 24
    ref.null eq
    i32.const 4
    table.fill 1
    local.get 24
    ref.null extern
    i32.const 4
    table.fill 2
    local.get 24
    global.set 28
    local.get 27
    global.set 27
    local.get 22
    throw_ref
  )
  (func (;123;) (type 277) (param eqref i32)
    (local f32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 8
    i32.const 1048496
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 8
    i32.const 80
    i32.add
    global.set 27
    local.get 8
    i32.const 0
    i32.add
    local.get 1
    i32.const 16
    memory.copy
    local.get 8
    i32.const 0
    i32.add
    local.set 1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        global.get 19
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 8
          array.new_data 0 19
          i32.const 0
          i32.const 8
          call 1
          global.set 19
        end
        global.get 19
        local.get 1
        local.set 9
        local.get 8
        i32.const 16
        i32.add
        local.get 9
        i32.const 16
        memory.copy
        local.get 8
        i32.const 16
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 9
        local.get 8
        i32.const 32
        i32.add
        local.get 9
        i32.const 16
        memory.copy
        local.get 8
        i32.const 32
        i32.add
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 9
        local.get 8
        i32.const 48
        i32.add
        local.get 9
        i32.const 16
        memory.copy
        local.get 8
        i32.const 48
        i32.add
        i32.const 8
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        local.set 9
        local.get 8
        i32.const 64
        i32.add
        local.get 9
        i32.const 16
        memory.copy
        local.get 8
        i32.const 64
        i32.add
        i32.const 12
        i32.add
        f32.load align=1
        f64.promote_f32
        global.get 1
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 0
          array.new_data 0 1
          i32.const 0
          i32.const 0
          call 1
          global.set 1
        end
        global.get 1
        call 0
        local.get 8
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 3
    local.get 8
    global.set 27
    local.get 3
    throw_ref
  )
  (func (;124;) (type 278) (param eqref i32 i32 f32 i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 14
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 14
    i32.const 0
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 4
        local.get 1
        local.set 21
        local.set 6
        local.set 5
        local.get 5
        ref.is_null
        local.get 6
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 5
        local.get 5
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 6
          i32.const 0
          i32.add
        else
          local.get 6
          i32.const 0
          i32.add
        end
        local.get 21
        i32.const 255
        i32.and
        call 145
        local.get 0
        local.get 4
        local.get 2
        local.set 21
        local.set 6
        local.set 5
        local.get 5
        ref.is_null
        local.get 6
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 5
        local.get 5
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 6
          i32.const 1
          i32.add
        else
          local.get 6
          i32.const 1
          i32.add
        end
        local.get 21
        i32.const 255
        i32.and
        call 145
        local.get 0
        local.get 4
        local.get 3
        f64.promote_f32
        local.set 23
        local.set 6
        local.set 5
        local.get 5
        ref.is_null
        local.get 6
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 5
        local.get 5
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 6
          i32.const 4
          i32.add
        else
          local.get 6
          i32.const 2
          i32.add
        end
        local.get 23
        f32.demote_f64
        call 139
        local.get 14
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 9
    local.get 14
    global.set 27
    local.get 9
    throw_ref
  )
  (func (;125;) (type 279) (param eqref i32) (result f32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 11
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 11
    i32.const 0
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 3
        local.set 2
        local.get 2
        ref.is_null
        local.get 3
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 2
        local.get 2
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 3
          i32.const 4
          i32.add
        else
          local.get 3
          i32.const 1
          i32.add
        end
        call 138
        f64.promote_f32
        f32.demote_f64
        local.get 11
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 6
    local.get 11
    global.set 27
    local.get 6
    throw_ref
  )
  (func (;126;) (type 280) (param eqref i32 i32) (result i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 12
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 12
    i32.const 16
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 16
          i32.add
        else
          local.get 4
          i32.const 5
          i32.add
        end
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 12
        i32.const 0
        i32.add
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
        else
          local.get 4
          i32.const 0
          i32.add
        end
        call 138
        f32.store align=1
        local.get 12
        i32.const 0
        i32.add
        local.set 13
        local.get 2
        local.get 13
        i32.const 4
        memory.copy
        local.get 2
        local.get 12
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 7
    local.get 12
    global.set 27
    local.get 7
    throw_ref
  )
  (func (;127;) (type 281) (param eqref i32 i32) (result i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 12
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 12
    i32.const 16
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 20
          i32.add
        else
          local.get 4
          i32.const 6
          i32.add
        end
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 12
        i32.const 0
        i32.add
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
        else
          local.get 4
          i32.const 0
          i32.add
        end
        call 138
        f32.store align=1
        local.get 12
        i32.const 0
        i32.add
        local.set 13
        local.get 2
        local.get 13
        i32.const 4
        memory.copy
        local.get 2
        local.get 12
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 7
    local.get 12
    global.set 27
    local.get 7
    throw_ref
  )
  (func (;128;) (type 282) (param eqref i32) (result f32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 11
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 11
    i32.const 0
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 3
        local.set 2
        local.get 2
        ref.is_null
        local.get 3
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 2
        local.get 2
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 3
          i32.const 24
          i32.add
        else
          local.get 3
          i32.const 7
          i32.add
        end
        call 138
        f64.promote_f32
        f32.demote_f64
        local.get 11
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 6
    local.get 11
    global.set 27
    local.get 6
    throw_ref
  )
  (func (;129;) (type 283) (param eqref i32) (result externref)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 11
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 11
    i32.const 0
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 3
        local.set 2
        local.get 2
        ref.is_null
        local.get 3
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 2
        local.get 2
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 3
          i32.const 0
          i32.add
          i32.load align=1
        else
          local.get 3
          i32.const 0
          i32.add
        end
        call 136
        local.get 11
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 6
    local.get 11
    global.set 27
    local.get 6
    throw_ref
  )
  (func (;130;) (type 284) (param eqref i32 i32) (result i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 12
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 12
    i32.const 16
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 8
          i32.add
        else
          local.get 4
          i32.const 2
          i32.add
        end
        local.set 4
        local.set 3
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 12
        i32.const 0
        i32.add
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
        else
          local.get 4
          i32.const 0
          i32.add
        end
        call 144
        i32.store8
        local.get 12
        i32.const 1
        i32.add
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 1
          i32.add
        else
          local.get 4
          i32.const 1
          i32.add
        end
        call 144
        i32.store8
        local.get 12
        i32.const 4
        i32.add
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 4
          i32.const 4
          i32.add
        else
          local.get 4
          i32.const 2
          i32.add
        end
        call 138
        f32.store align=1
        local.get 12
        i32.const 0
        i32.add
        local.set 13
        local.get 2
        local.get 13
        i32.const 8
        memory.copy
        local.get 2
        local.get 12
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 7
    local.get 12
    global.set 27
    local.get 7
    throw_ref
  )
  (func (;131;) (type 285) (param eqref i32) (result i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 11
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 11
    i32.const 0
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 3
        local.set 2
        local.get 2
        ref.is_null
        local.get 3
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 2
        local.get 2
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 3
          i32.const 0
          i32.add
        else
          local.get 3
          i32.const 0
          i32.add
        end
        call 144
        i32.const 255
        i32.and
        local.get 11
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 6
    local.get 11
    global.set 27
    local.get 6
    throw_ref
  )
  (func (;132;) (type 286) (param eqref i32) (result i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 11
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 11
    i32.const 0
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 3
        local.set 2
        local.get 2
        ref.is_null
        local.get 3
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 2
        local.get 2
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 3
          i32.const 1
          i32.add
        else
          local.get 3
          i32.const 1
          i32.add
        end
        call 144
        i32.const 255
        i32.and
        local.get 11
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 6
    local.get 11
    global.set 27
    local.get 6
    throw_ref
  )
  (func (;133;) (type 287) (param eqref i32) (result f32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 27
    local.tee 11
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 11
    i32.const 0
    i32.add
    global.set 27
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 3
        local.set 2
        local.get 2
        ref.is_null
        local.get 3
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 23
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 23
            i32.const 0
            i32.const 53
            call 1
            global.set 23
          end
          global.get 23
          struct.new 3
          throw 0
        end
        local.get 2
        local.get 2
        ref.is_null
        if (type 59) (result i32) ;; label = @3
          local.get 3
          i32.const 4
          i32.add
        else
          local.get 3
          i32.const 2
          i32.add
        end
        call 138
        f64.promote_f32
        f32.demote_f64
        local.get 11
        global.set 27
        return
        unreachable
      end
      unreachable
    end
    local.set 6
    local.get 11
    global.set 27
    local.get 6
    throw_ref
  )
  (func (;134;) (type 60) (param eqref i32) (result eqref)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 23
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 23
        i32.const 0
        i32.const 53
        call 1
        global.set 23
      end
      i32.const 4
      global.get 23
      struct.new 3
      throw 0
    end
    local.get 0
    ref.is_null
    if ;; label = @1
      local.get 1
      table.get 1
      return
    end
    unreachable
  )
  (func (;135;) (type 61) (param eqref i32 eqref)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 23
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 23
        i32.const 0
        i32.const 53
        call 1
        global.set 23
      end
      i32.const 4
      global.get 23
      struct.new 3
      throw 0
    end
    local.get 0
    ref.is_null
    if ;; label = @1
      local.get 1
      local.get 2
      table.set 1
      return
    end
    unreachable
  )
  (func (;136;) (type 62) (param eqref i32) (result externref)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 23
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 23
        i32.const 0
        i32.const 53
        call 1
        global.set 23
      end
      i32.const 4
      global.get 23
      struct.new 3
      throw 0
    end
    local.get 0
    ref.is_null
    if ;; label = @1
      local.get 1
      table.get 2
      return
    end
    unreachable
  )
  (func (;137;) (type 63) (param eqref i32 externref)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 23
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 23
        i32.const 0
        i32.const 53
        call 1
        global.set 23
      end
      i32.const 4
      global.get 23
      struct.new 3
      throw 0
    end
    local.get 0
    ref.is_null
    if ;; label = @1
      local.get 1
      local.get 2
      table.set 2
      return
    end
    unreachable
  )
  (func (;138;) (type 64) (param eqref i32) (result f32)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 23
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 23
        i32.const 0
        i32.const 53
        call 1
        global.set 23
      end
      i32.const 4
      global.get 23
      struct.new 3
      throw 0
    end
    local.get 0
    ref.is_null
    if ;; label = @1
      local.get 1
      f32.load align=1
      return
    end
    unreachable
  )
  (func (;139;) (type 65) (param eqref i32 f32)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 23
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 23
        i32.const 0
        i32.const 53
        call 1
        global.set 23
      end
      i32.const 4
      global.get 23
      struct.new 3
      throw 0
    end
    local.get 0
    ref.is_null
    if ;; label = @1
      local.get 1
      local.get 2
      f32.store align=1
      return
    end
    unreachable
  )
  (func (;140;) (type 66) (param eqref i32) (result i64)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 23
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 23
        i32.const 0
        i32.const 53
        call 1
        global.set 23
      end
      i32.const 4
      global.get 23
      struct.new 3
      throw 0
    end
    local.get 0
    ref.is_null
    if ;; label = @1
      local.get 1
      i64.load align=1
      return
    end
    unreachable
  )
  (func (;141;) (type 67) (param eqref i32 i64)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 23
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 23
        i32.const 0
        i32.const 53
        call 1
        global.set 23
      end
      i32.const 4
      global.get 23
      struct.new 3
      throw 0
    end
    local.get 0
    ref.is_null
    if ;; label = @1
      local.get 1
      local.get 2
      i64.store align=1
      return
    end
    unreachable
  )
  (func (;142;) (type 68) (param eqref i32) (result i32)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 23
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 23
        i32.const 0
        i32.const 53
        call 1
        global.set 23
      end
      i32.const 4
      global.get 23
      struct.new 3
      throw 0
    end
    local.get 0
    ref.is_null
    if ;; label = @1
      local.get 1
      i32.load8_u
      return
    end
    unreachable
  )
  (func (;143;) (type 69) (param eqref i32 i32)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 23
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 23
        i32.const 0
        i32.const 53
        call 1
        global.set 23
      end
      i32.const 4
      global.get 23
      struct.new 3
      throw 0
    end
    local.get 0
    ref.is_null
    if ;; label = @1
      local.get 1
      local.get 2
      i32.store8
      return
    end
    unreachable
  )
  (func (;144;) (type 70) (param eqref i32) (result i32)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 23
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 23
        i32.const 0
        i32.const 53
        call 1
        global.set 23
      end
      i32.const 4
      global.get 23
      struct.new 3
      throw 0
    end
    local.get 0
    ref.is_null
    if ;; label = @1
      local.get 1
      i32.load8_u
      return
    end
    unreachable
  )
  (func (;145;) (type 71) (param eqref i32 i32)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 23
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 23
        i32.const 0
        i32.const 53
        call 1
        global.set 23
      end
      i32.const 4
      global.get 23
      struct.new 3
      throw 0
    end
    local.get 0
    ref.is_null
    if ;; label = @1
      local.get 1
      local.get 2
      i32.store8
      return
    end
    unreachable
  )
  (func (;146;) (type 72) (param eqref i32) (result i32)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 23
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 23
        i32.const 0
        i32.const 53
        call 1
        global.set 23
      end
      i32.const 4
      global.get 23
      struct.new 3
      throw 0
    end
    local.get 0
    ref.is_null
    if ;; label = @1
      local.get 1
      i32.load align=1
      return
    end
    unreachable
  )
  (func (;147;) (type 73) (param eqref i32 i32)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 23
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 23
        i32.const 0
        i32.const 53
        call 1
        global.set 23
      end
      i32.const 4
      global.get 23
      struct.new 3
      throw 0
    end
    local.get 0
    ref.is_null
    if ;; label = @1
      local.get 1
      local.get 2
      i32.store align=1
      return
    end
    unreachable
  )
  (func (;148;) (type 11) (param eqref eqref) (result eqref)
    (local eqref)
    local.get 0
    ref.is_null
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 0
    ref.test (ref 10)
    i32.eqz
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 0
    ref.cast (ref 10)
    struct.get 10 0
    local.set 2
    local.get 1
    global.get 30
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 29
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 31
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 29
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 32
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 29
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 33
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 29
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 34
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 29
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 35
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 29
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 36
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 29
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 37
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 29
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 38
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 29
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 39
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 29
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 40
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 29
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 41
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 29
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 42
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 29
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 43
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 29
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 44
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 29
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 45
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 29
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 46
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 29
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 47
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 29
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 49
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 48
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      global.get 51
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 50
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 48
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      global.get 51
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 52
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 51
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 53
    ref.eq
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 1
    global.get 55
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 54
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 56
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 54
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 57
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 54
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      global.get 64
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      global.get 73
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      global.get 77
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 58
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 54
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 59
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 54
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 60
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 54
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 61
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 54
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      global.get 64
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 62
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 54
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      global.get 64
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 63
    ref.eq
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 1
    global.get 65
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 64
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 66
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 64
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 67
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 64
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 68
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 64
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 69
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 64
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      global.get 73
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      global.get 77
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 70
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 64
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 71
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 64
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 72
    ref.eq
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 1
    global.get 74
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 73
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 75
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 73
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 76
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 73
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 78
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 77
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 79
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 54
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      global.get 73
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      global.get 77
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    local.get 1
    global.get 80
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 54
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      global.get 73
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      global.get 77
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      ref.null eq
      return
    end
    loop ;; label = @1
      local.get 2
      local.get 1
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
      local.get 2
      ref.cast (ref 9)
      struct.get 9 1
      local.tee 2
      ref.is_null
      if ;; label = @2
        ref.null eq
        return
      end
      br 0 (;@1;)
    end
    unreachable
  )
  (data (;0;) "S\00a\00v\00e\00")
  (data (;1;) "")
  (data (;2;) "I\00n\00t\00e\00r\00")
  (data (;3;) "R\00e\00s\00t\00o\00r\00e\00")
  (data (;4;) "G\00l\00o\00b\00a\00l\00A\00l\00p\00h\00a\00")
  (data (;5;) "L\00i\00n\00e\00C\00a\00p\00")
  (data (;6;) "L\00i\00n\00e\00J\00o\00i\00n\00")
  (data (;7;) "L\00i\00n\00e\00W\00i\00d\00t\00h\00")
  (data (;8;) "M\00i\00t\00e\00r\00L\00i\00m\00i\00t\00")
  (data (;9;) "L\00i\00n\00e\00D\00a\00s\00h\00O\00f\00f\00s\00e\00t\00")
  (data (;10;) "U\00n\00s\00u\00p\00p\00o\00r\00t\00e\00d\00 \00C\00a\00n\00v\00a\00s\00 \00o\00p\00e\00r\00a\00t\00i\00o\00n\00 \00i\00n\00 \00t\00h\00e\00 \00f\00i\00r\00s\00t\00-\00f\00r\00a\00m\00e\00 \00h\00o\00s\00t\00.\00")
  (data (;11;) "S\00e\00t\00L\00i\00n\00e\00D\00a\00s\00h\00")
  (data (;12;) "S\00e\00t\00S\00t\00r\00o\00k\00e\00")
  (data (;13;) "S\00e\00t\00F\00i\00l\00l\00")
  (data (;14;) "F\00o\00n\00t\00")
  (data (;15;) "F\00o\00n\00t\00S\00t\00y\00l\00e\00")
  (data (;16;) "T\00e\00x\00t\00A\00l\00i\00g\00n\00")
  (data (;17;) "T\00e\00x\00t\00B\00a\00s\00e\00l\00i\00n\00e\00")
  (data (;18;) "B\00e\00g\00i\00n\00P\00a\00t\00h\00")
  (data (;19;) "F\00i\00l\00l\00R\00e\00c\00t\00")
  (data (;20;) "E\00x\00c\00e\00p\00t\00i\00o\00n\00 \00o\00f\00 \00t\00y\00p\00e\00 \00'\00S\00y\00s\00t\00e\00m\00.\00E\00x\00c\00e\00p\00t\00i\00o\00n\00'\00 \00w\00a\00s\00 \00t\00h\00r\00o\00w\00n\00.\00")
  (data (;21;) "S\00y\00s\00t\00e\00m\00 \00e\00r\00r\00o\00r\00.\00")
  (data (;22;) "S\00p\00e\00c\00i\00f\00i\00e\00d\00 \00c\00a\00s\00t\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00.\00")
  (data (;23;) "O\00b\00j\00e\00c\00t\00 \00r\00e\00f\00e\00r\00e\00n\00c\00e\00 \00n\00o\00t\00 \00s\00e\00t\00 \00t\00o\00 \00a\00n\00 \00i\00n\00s\00t\00a\00n\00c\00e\00 \00o\00f\00 \00a\00n\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;24;) "O\00p\00e\00r\00a\00t\00i\00o\00n\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00 \00d\00u\00e\00 \00t\00o\00 \00t\00h\00e\00 \00c\00u\00r\00r\00e\00n\00t\00 \00s\00t\00a\00t\00e\00 \00o\00f\00 \00t\00h\00e\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;25;) "V\00a\00l\00u\00e\00 \00d\00o\00e\00s\00 \00n\00o\00t\00 \00f\00a\00l\00l\00 \00w\00i\00t\00h\00i\00n\00 \00t\00h\00e\00 \00e\00x\00p\00e\00c\00t\00e\00d\00 \00r\00a\00n\00g\00e\00.\00")
  (data (;26;) "I\00n\00d\00e\00x\00 \00w\00a\00s\00 \00o\00u\00t\00s\00i\00d\00e\00 \00t\00h\00e\00 \00b\00o\00u\00n\00d\00s\00 \00o\00f\00 \00t\00h\00e\00 \00a\00r\00r\00a\00y\00.\00")
)
```
