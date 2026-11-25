# Events: Xui geometry → WASM → Canvas

[Open standalone demo](Events.html)

Source: [Events.cs](Fixtures/Canvas/Events.cs), [Host.cs](Fixtures/Canvas/Host.cs)

## Source

```csharp
using CanvasDemo;
using Xui.Core.Canvas;
using Xui.Core.Math2D;

public interface IPainter { void Paint(ref PaintEvent e); }
public ref struct PaintEvent
{
    public Rect Bounds;
    public Color Color;
    public string Label;
    public IPainter Painter;
}
public class Painter : IPainter
{
    public void Paint(ref PaintEvent e)
    {
        Draw.Fill(ref e.Bounds, ref e.Color);
        e.Bounds.X = 40; e.Bounds.Y = 60;
        Draw.Text(e.Label, e.Bounds.X, e.Bounds.Y, 26, ref e.Color);
    }
}
public static class Program
{
    public static int Main()
    {
        NativeCanvas.Clear(0.0625, 0.125, 0.25, 1);
        PaintEvent e = default;
        e.Bounds.X = 40; e.Bounds.Y = 100; e.Bounds.Width = 520; e.Bounds.Height = 180;
        e.Color.Red = 0.5f; e.Color.Green = 1; e.Color.Blue = 0.75f; e.Color.Alpha = 1;
        e.Label = "Stack event → interface → canvas";
        e.Painter = new Painter();
        e.Painter.Paint(ref e);
        return 42;
    }
}
```

## Drawing commands

```json
[
  [
    "Clear",
    0.0625,
    0.125,
    0.25,
    1
  ],
  [
    "FillRect",
    40,
    100,
    520,
    180,
    0.5,
    1,
    0.75,
    1
  ],
  [
    "FillText",
    "Stack event \u2192 interface \u2192 canvas",
    40,
    60,
    26,
    0.5,
    1,
    0.75,
    1
  ]
]
```

## Exit Code

```text
42
```

## Retained methods

- `CanvasDemo.exe`: `Program.Main`
- `CanvasDemo.exe`: `Painter..ctor`
- `CanvasDemo.exe`: `IPainter.Paint`
- `CanvasDemo.exe`: `Painter.Paint`
- `CanvasDemo.exe`: `CanvasDemo.Draw.Fill`
- `CanvasDemo.exe`: `CanvasDemo.Draw.Text`

## WAT

```wat
(module
  (type (;0;) (array (mut i16)))
  (type (;1;) (func (param (ref null 0) i32 i32) (result (ref extern))))
  (type (;2;) (func (param f64 f64 f64 f64)))
  (type (;3;) (func (param f64 f64 f64 f64 f64 f64 f64 f64)))
  (type (;4;) (func (param externref f64 f64 f64 f64 f64 f64 f64)))
  (type (;5;) (struct (field i32) (field externref)))
  (type (;6;) (func (param eqref)))
  (type (;7;) (func (result eqref)))
  (type (;8;) (func (result eqref exnref)))
  (type (;9;) (func (result exnref)))
  (type (;10;) (func))
  (type (;11;) (struct (field i32) (field eqref) (field i32) (field (ref null 0)) (field i32) (field i32)))
  (type (;12;) (sub (struct (field (mut eqref)))))
  (type (;13;) (func (param eqref eqref) (result eqref)))
  (type (;14;) (struct (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut externref)) (field (mut eqref))))
  (type (;15;) (struct (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32))))
  (type (;16;) (struct (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32))))
  (type (;17;) (sub 12 (struct (field (mut eqref)))))
  (type (;18;) (func (result i32)))
  (type (;19;) (func (param eqref i32) (result eqref)))
  (type (;20;) (func (param eqref i32 eqref)))
  (type (;21;) (func (param eqref i32) (result externref)))
  (type (;22;) (func (param eqref i32 externref)))
  (type (;23;) (func (param eqref i32) (result f32)))
  (type (;24;) (func (param eqref i32 f32)))
  (type (;25;) (func (result i32)))
  (type (;26;) (func (param eqref)))
  (type (;27;) (func (param eqref eqref i32)))
  (type (;28;) (func (param eqref eqref i32)))
  (type (;29;) (func (param eqref eqref i32 i32)))
  (type (;30;) (func (param externref f32 f32 f32 eqref i32)))
  (import "canvas" "Clear" (func (;0;) (type 2) (param f64 f64 f64 f64)))
  (import "canvas" "FillRect" (func (;1;) (type 3) (param f64 f64 f64 f64 f64 f64 f64 f64)))
  (import "canvas" "FillText" (func (;2;) (type 4) (param externref f64 f64 f64 f64 f64 f64 f64)))
  (import "wasm:js-string" "fromCharCodeArray" (func (;3;) (type 1) (param (ref null 0) i32 i32) (result (ref extern))))
  (table (;0;) 2 2 funcref)
  (table (;1;) 65536 65536 eqref)
  (table (;2;) 65536 65536 externref)
  (memory (;0;) 16 16)
  (tag (;0;) (type 6) (param eqref))
  (global (;0;) (mut externref) ref.null extern)
  (global (;1;) (mut externref) ref.null extern)
  (global (;2;) (mut externref) ref.null extern)
  (global (;3;) (mut externref) ref.null extern)
  (global (;4;) (mut externref) ref.null extern)
  (global (;5;) (mut externref) ref.null extern)
  (global (;6;) (mut externref) ref.null extern)
  (global (;7;) (mut i32) i32.const 16)
  (global (;8;) (mut i32) i32.const 0)
  (global (;9;) eqref i32.const 1 ref.null eq i32.const 0 i32.const 73 i32.const 80 i32.const 97 i32.const 105 i32.const 110 i32.const 116 i32.const 101 i32.const 114 array.new_fixed 0 8 i32.const 0 i32.const 0 struct.new 11)
  (global (;10;) eqref i32.const 2 ref.null eq i32.const 0 i32.const 80 i32.const 97 i32.const 105 i32.const 110 i32.const 116 i32.const 101 i32.const 114 array.new_fixed 0 7 i32.const 1 i32.const 1 struct.new 11)
  (export "main" (func 4))
  (elem (;0;) (i32.const 1) func 6)
  (elem (;1;) (i32.const 0) func 6)
  (func (;4;) (type 25) (result i32)
    (local i32 eqref i32 eqref i32 f32 eqref eqref eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 7
    local.tee 15
    i32.const 1048496
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 15
    i32.const 80
    i32.add
    global.set 7
    local.get 15
    i32.const 0
    i32.add
    local.set 0
    local.get 0
    i32.const 0
    i32.const 40
    memory.fill
    global.get 8
    local.tee 12
    i32.const 65532
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 12
    i32.const 4
    i32.add
    global.set 8
    local.get 15
    i32.const 32
    i32.add
    local.get 12
    i32.const 0
    i32.add
    i32.store align=1
    local.get 15
    i32.const 36
    i32.add
    local.get 12
    i32.const 1
    i32.add
    i32.store align=1
    local.get 15
    i32.const 72
    i32.add
    local.get 12
    i32.const 2
    i32.add
    i32.store align=1
    local.get 15
    i32.const 76
    i32.add
    local.get 12
    i32.const 3
    i32.add
    i32.store align=1
    block (type 9) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        f64.const 0x1p-4 (;=0.0625;)
        f64.const 0x1p-3 (;=0.125;)
        f64.const 0x1p-2 (;=0.25;)
        f64.const 0x1p+0 (;=1;)
        call 0
        local.get 0
        local.set 2
        ref.null eq
        local.get 2
        local.set 2
        local.set 1
        local.get 1
        ref.is_null
        local.get 2
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 1
        ref.is_null
        local.get 2
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 1
        local.get 1
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 2
          i32.const 0
          i32.add
        else
          local.get 2
          i32.const 0
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 14
        local.get 1
        ref.is_null
        local.get 2
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 1
        local.get 1
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 2
          i32.const 4
          i32.add
        else
          local.get 2
          i32.const 1
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 14
        local.get 1
        ref.is_null
        local.get 2
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 1
        local.get 1
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 2
          i32.const 8
          i32.add
        else
          local.get 2
          i32.const 2
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 14
        local.get 1
        ref.is_null
        local.get 2
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 1
        local.get 1
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 2
          i32.const 12
          i32.add
        else
          local.get 2
          i32.const 3
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 14
        local.get 1
        ref.is_null
        local.get 2
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 1
        local.get 1
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 2
          i32.const 16
          i32.add
        else
          local.get 2
          i32.const 4
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 14
        local.get 1
        ref.is_null
        local.get 2
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 1
        local.get 1
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 2
          i32.const 20
          i32.add
        else
          local.get 2
          i32.const 5
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 14
        local.get 1
        ref.is_null
        local.get 2
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 1
        local.get 1
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 2
          i32.const 24
          i32.add
        else
          local.get 2
          i32.const 6
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 14
        local.get 1
        ref.is_null
        local.get 2
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 1
        local.get 1
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 2
          i32.const 28
          i32.add
        else
          local.get 2
          i32.const 7
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 14
        local.get 1
        ref.is_null
        local.get 2
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 1
        local.get 1
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 2
          i32.const 32
          i32.add
          i32.load align=1
        else
          local.get 2
          i32.const 8
          i32.add
        end
        ref.null extern
        call 12
        local.get 1
        ref.is_null
        local.get 2
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 1
        local.get 1
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 2
          i32.const 36
          i32.add
          i32.load align=1
        else
          local.get 2
          i32.const 9
          i32.add
        end
        ref.null eq
        call 10
        local.get 0
        local.set 2
        ref.null eq
        local.get 2
        local.set 2
        local.set 1
        local.get 1
        ref.is_null
        local.get 2
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 1
        local.get 1
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 2
          i32.const 0
          i32.add
        else
          local.get 2
          i32.const 0
          i32.add
        end
        i32.const 40
        f32.convert_i32_s
        f64.promote_f32
        local.set 24
        local.set 2
        local.set 1
        local.get 1
        ref.is_null
        local.get 2
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 1
        local.get 1
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 2
          i32.const 0
          i32.add
        else
          local.get 2
          i32.const 0
          i32.add
        end
        local.get 24
        f32.demote_f64
        call 14
        local.get 0
        local.set 2
        ref.null eq
        local.get 2
        local.set 2
        local.set 1
        local.get 1
        ref.is_null
        local.get 2
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 1
        local.get 1
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 2
          i32.const 0
          i32.add
        else
          local.get 2
          i32.const 0
          i32.add
        end
        i32.const 100
        f32.convert_i32_s
        f64.promote_f32
        local.set 24
        local.set 2
        local.set 1
        local.get 1
        ref.is_null
        local.get 2
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 1
        local.get 1
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 2
          i32.const 4
          i32.add
        else
          local.get 2
          i32.const 1
          i32.add
        end
        local.get 24
        f32.demote_f64
        call 14
        local.get 0
        local.set 2
        ref.null eq
        local.get 2
        local.set 2
        local.set 1
        local.get 1
        ref.is_null
        local.get 2
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 1
        local.get 1
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 2
          i32.const 0
          i32.add
        else
          local.get 2
          i32.const 0
          i32.add
        end
        i32.const 520
        f32.convert_i32_s
        f64.promote_f32
        local.set 24
        local.set 2
        local.set 1
        local.get 1
        ref.is_null
        local.get 2
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 1
        local.get 1
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 2
          i32.const 8
          i32.add
        else
          local.get 2
          i32.const 2
          i32.add
        end
        local.get 24
        f32.demote_f64
        call 14
        local.get 0
        local.set 2
        ref.null eq
        local.get 2
        local.set 2
        local.set 1
        local.get 1
        ref.is_null
        local.get 2
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 1
        local.get 1
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 2
          i32.const 0
          i32.add
        else
          local.get 2
          i32.const 0
          i32.add
        end
        i32.const 180
        f32.convert_i32_s
        f64.promote_f32
        local.set 24
        local.set 2
        local.set 1
        local.get 1
        ref.is_null
        local.get 2
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 1
        local.get 1
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 2
          i32.const 12
          i32.add
        else
          local.get 2
          i32.const 3
          i32.add
        end
        local.get 24
        f32.demote_f64
        call 14
        local.get 0
        local.set 2
        ref.null eq
        local.get 2
        local.set 2
        local.set 1
        local.get 1
        ref.is_null
        local.get 2
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 1
        local.get 1
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 2
          i32.const 16
          i32.add
        else
          local.get 2
          i32.const 4
          i32.add
        end
        f32.const 0x1p-1 (;=0.5;)
        f64.promote_f32
        f32.demote_f64
        f64.promote_f32
        local.set 24
        local.set 2
        local.set 1
        local.get 1
        ref.is_null
        local.get 2
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 1
        local.get 1
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 2
          i32.const 0
          i32.add
        else
          local.get 2
          i32.const 0
          i32.add
        end
        local.get 24
        f32.demote_f64
        call 14
        local.get 0
        local.set 2
        ref.null eq
        local.get 2
        local.set 2
        local.set 1
        local.get 1
        ref.is_null
        local.get 2
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 1
        local.get 1
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 2
          i32.const 16
          i32.add
        else
          local.get 2
          i32.const 4
          i32.add
        end
        i32.const 1
        f32.convert_i32_s
        f64.promote_f32
        local.set 24
        local.set 2
        local.set 1
        local.get 1
        ref.is_null
        local.get 2
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 1
        local.get 1
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 2
          i32.const 4
          i32.add
        else
          local.get 2
          i32.const 1
          i32.add
        end
        local.get 24
        f32.demote_f64
        call 14
        local.get 0
        local.set 2
        ref.null eq
        local.get 2
        local.set 2
        local.set 1
        local.get 1
        ref.is_null
        local.get 2
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 1
        local.get 1
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 2
          i32.const 16
          i32.add
        else
          local.get 2
          i32.const 4
          i32.add
        end
        f32.const 0x1.8p-1 (;=0.75;)
        f64.promote_f32
        f32.demote_f64
        f64.promote_f32
        local.set 24
        local.set 2
        local.set 1
        local.get 1
        ref.is_null
        local.get 2
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 1
        local.get 1
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 2
          i32.const 8
          i32.add
        else
          local.get 2
          i32.const 2
          i32.add
        end
        local.get 24
        f32.demote_f64
        call 14
        local.get 0
        local.set 2
        ref.null eq
        local.get 2
        local.set 2
        local.set 1
        local.get 1
        ref.is_null
        local.get 2
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 1
        local.get 1
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 2
          i32.const 16
          i32.add
        else
          local.get 2
          i32.const 4
          i32.add
        end
        i32.const 1
        f32.convert_i32_s
        f64.promote_f32
        local.set 24
        local.set 2
        local.set 1
        local.get 1
        ref.is_null
        local.get 2
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 1
        local.get 1
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 2
          i32.const 12
          i32.add
        else
          local.get 2
          i32.const 3
          i32.add
        end
        local.get 24
        f32.demote_f64
        call 14
        local.get 0
        local.set 2
        ref.null eq
        local.get 2
        global.get 0
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 32
          array.new_data 0 0
          i32.const 0
          i32.const 32
          call 3
          global.set 0
        end
        global.get 0
        local.set 25
        local.set 2
        local.set 1
        local.get 1
        ref.is_null
        local.get 2
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 1
        local.get 1
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 2
          i32.const 32
          i32.add
          i32.load align=1
        else
          local.get 2
          i32.const 8
          i32.add
        end
        local.get 25
        call 12
        local.get 0
        local.set 2
        ref.null eq
        local.get 2
        struct.new_default 17
        local.set 6
        local.get 6
        ref.cast (ref 17)
        global.get 10
        struct.set 17 0
        local.get 6
        call 5
        local.get 6
        local.set 20
        local.set 2
        local.set 1
        local.get 1
        ref.is_null
        local.get 2
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 1
        local.get 1
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 2
          i32.const 36
          i32.add
          i32.load align=1
        else
          local.get 2
          i32.const 9
          i32.add
        end
        local.get 20
        call 10
        local.get 0
        local.set 16
        local.get 15
        i32.const 40
        i32.add
        local.get 16
        local.set 13
        local.set 14
        local.get 14
        i32.const 0
        i32.add
        local.get 13
        i32.const 0
        i32.add
        f32.load align=1
        f32.store align=1
        local.get 14
        i32.const 4
        i32.add
        local.get 13
        i32.const 4
        i32.add
        f32.load align=1
        f32.store align=1
        local.get 14
        i32.const 8
        i32.add
        local.get 13
        i32.const 8
        i32.add
        f32.load align=1
        f32.store align=1
        local.get 14
        i32.const 12
        i32.add
        local.get 13
        i32.const 12
        i32.add
        f32.load align=1
        f32.store align=1
        local.get 14
        i32.const 16
        i32.add
        local.get 13
        i32.const 16
        i32.add
        f32.load align=1
        f32.store align=1
        local.get 14
        i32.const 20
        i32.add
        local.get 13
        i32.const 20
        i32.add
        f32.load align=1
        f32.store align=1
        local.get 14
        i32.const 24
        i32.add
        local.get 13
        i32.const 24
        i32.add
        f32.load align=1
        f32.store align=1
        local.get 14
        i32.const 28
        i32.add
        local.get 13
        i32.const 28
        i32.add
        f32.load align=1
        f32.store align=1
        local.get 14
        i32.const 32
        i32.add
        local.get 13
        i32.const 32
        i32.add
        i32.load align=1
        table.get 2
        local.set 25
        i32.load align=1
        local.get 25
        table.set 2
        local.get 14
        i32.const 36
        i32.add
        local.get 13
        i32.const 36
        i32.add
        i32.load align=1
        table.get 1
        local.set 11
        i32.load align=1
        local.get 11
        table.set 1
        local.get 15
        i32.const 40
        i32.add
        i32.const 36
        i32.add
        i32.load align=1
        table.get 1
        local.get 0
        local.set 2
        ref.null eq
        local.get 2
        local.set 9
        local.set 8
        local.set 7
        local.get 7
        local.tee 21
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 21
        ref.cast (ref 12)
        local.get 8
        local.get 9
        local.get 7
        ref.cast (ref 12)
        struct.get 12 0
        ref.cast (ref 11)
        struct.get 11 2
        i32.const 0
        i32.add
        call_indirect (type 27)
        i32.const 42
        local.get 12
        ref.null eq
        i32.const 4
        table.fill 1
        local.get 12
        ref.null extern
        i32.const 4
        table.fill 2
        local.get 12
        global.set 8
        local.get 15
        global.set 7
        return
        unreachable
      end
      unreachable
    end
    local.set 10
    local.get 12
    ref.null eq
    i32.const 4
    table.fill 1
    local.get 12
    ref.null extern
    i32.const 4
    table.fill 2
    local.get 12
    global.set 8
    local.get 15
    global.set 7
    local.get 10
    throw_ref
  )
  (func (;5;) (type 26) (param eqref)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    drop
    return
  )
  (func (;6;) (type 28) (param eqref eqref i32)
    (local eqref i32 eqref i32 f32 eqref eqref i32 i32 externref f32 f32 f32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 7
    local.tee 23
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 23
    i32.const 0
    i32.add
    global.set 7
    block (type 9) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 1
        local.get 2
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
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
        else
          local.get 4
          i32.const 0
          i32.add
        end
        local.get 1
        local.get 2
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
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 4
          i32.const 16
          i32.add
        else
          local.get 4
          i32.const 4
          i32.add
        end
        local.set 11
        local.set 9
        local.set 10
        local.set 8
        local.get 8
        local.get 9
        local.get 10
        local.get 11
        call 7
        local.get 1
        local.get 2
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
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
        else
          local.get 4
          i32.const 0
          i32.add
        end
        i32.const 40
        f32.convert_i32_s
        f64.promote_f32
        local.set 32
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
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
        else
          local.get 4
          i32.const 0
          i32.add
        end
        local.get 32
        f32.demote_f64
        call 14
        local.get 1
        local.get 2
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
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
        else
          local.get 4
          i32.const 0
          i32.add
        end
        i32.const 60
        f32.convert_i32_s
        f64.promote_f32
        local.set 32
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
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 4
          i32.const 4
          i32.add
        else
          local.get 4
          i32.const 1
          i32.add
        end
        local.get 32
        f32.demote_f64
        call 14
        local.get 1
        local.get 2
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
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 4
          i32.const 32
          i32.add
          i32.load align=1
        else
          local.get 4
          i32.const 8
          i32.add
        end
        call 11
        local.get 1
        local.get 2
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
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 18) (result i32) ;; label = @3
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
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
        else
          local.get 4
          i32.const 0
          i32.add
        end
        call 13
        f64.promote_f32
        local.get 1
        local.get 2
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
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 18) (result i32) ;; label = @3
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
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 4
          i32.const 4
          i32.add
        else
          local.get 4
          i32.const 1
          i32.add
        end
        call 13
        f64.promote_f32
        i32.const 26
        f32.convert_i32_s
        f64.promote_f32
        local.get 1
        local.get 2
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
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 4
          i32.const 16
          i32.add
        else
          local.get 4
          i32.const 4
          i32.add
        end
        local.set 17
        local.set 16
        f32.demote_f64
        local.set 15
        f32.demote_f64
        local.set 14
        f32.demote_f64
        local.set 13
        local.set 12
        local.get 12
        local.get 13
        local.get 14
        local.get 15
        local.get 16
        local.get 17
        call 8
        local.get 23
        global.set 7
        return
        unreachable
      end
      unreachable
    end
    local.set 18
    local.get 23
    global.set 7
    local.get 18
    throw_ref
  )
  (func (;7;) (type 29) (param eqref eqref i32 i32)
    (local eqref i32 eqref i32 f32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 7
    local.tee 14
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 14
    i32.const 0
    i32.add
    global.set 7
    block (type 9) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 2
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
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 4
        local.get 4
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 5
          i32.const 0
          i32.add
        else
          local.get 5
          i32.const 0
          i32.add
        end
        call 13
        f64.promote_f32
        local.get 0
        local.get 2
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
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 4
        local.get 4
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 5
          i32.const 4
          i32.add
        else
          local.get 5
          i32.const 1
          i32.add
        end
        call 13
        f64.promote_f32
        local.get 0
        local.get 2
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
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 4
        local.get 4
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 5
          i32.const 8
          i32.add
        else
          local.get 5
          i32.const 2
          i32.add
        end
        call 13
        f64.promote_f32
        local.get 0
        local.get 2
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
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 4
        local.get 4
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 5
          i32.const 12
          i32.add
        else
          local.get 5
          i32.const 3
          i32.add
        end
        call 13
        f64.promote_f32
        local.get 1
        local.get 3
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
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 4
        local.get 4
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 5
          i32.const 0
          i32.add
        else
          local.get 5
          i32.const 0
          i32.add
        end
        call 13
        f64.promote_f32
        local.get 1
        local.get 3
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
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 4
        local.get 4
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 5
          i32.const 4
          i32.add
        else
          local.get 5
          i32.const 1
          i32.add
        end
        call 13
        f64.promote_f32
        local.get 1
        local.get 3
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
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 4
        local.get 4
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 5
          i32.const 8
          i32.add
        else
          local.get 5
          i32.const 2
          i32.add
        end
        call 13
        f64.promote_f32
        local.get 1
        local.get 3
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
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 4
        local.get 4
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 5
          i32.const 12
          i32.add
        else
          local.get 5
          i32.const 3
          i32.add
        end
        call 13
        f64.promote_f32
        call 1
        local.get 14
        global.set 7
        return
        unreachable
      end
      unreachable
    end
    local.set 9
    local.get 14
    global.set 7
    local.get 9
    throw_ref
  )
  (func (;8;) (type 30) (param externref f32 f32 f32 eqref i32)
    (local eqref i32 eqref i32 f32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 7
    local.tee 16
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 16
    i32.const 0
    i32.add
    global.set 7
    block (type 9) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        f64.promote_f32
        local.get 2
        f64.promote_f32
        local.get 3
        f64.promote_f32
        local.get 4
        local.get 5
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
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 6
        local.get 6
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 7
          i32.const 0
          i32.add
        else
          local.get 7
          i32.const 0
          i32.add
        end
        call 13
        f64.promote_f32
        local.get 4
        local.get 5
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
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 6
        local.get 6
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 7
          i32.const 4
          i32.add
        else
          local.get 7
          i32.const 1
          i32.add
        end
        call 13
        f64.promote_f32
        local.get 4
        local.get 5
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
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 6
        local.get 6
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 7
          i32.const 8
          i32.add
        else
          local.get 7
          i32.const 2
          i32.add
        end
        call 13
        f64.promote_f32
        local.get 4
        local.get 5
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
          global.get 4
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 4
            i32.const 0
            i32.const 53
            call 3
            global.set 4
          end
          global.get 4
          struct.new 5
          throw 0
        end
        local.get 6
        local.get 6
        ref.is_null
        if (type 18) (result i32) ;; label = @3
          local.get 7
          i32.const 12
          i32.add
        else
          local.get 7
          i32.const 3
          i32.add
        end
        call 13
        f64.promote_f32
        call 2
        local.get 16
        global.set 7
        return
        unreachable
      end
      unreachable
    end
    local.set 11
    local.get 16
    global.set 7
    local.get 11
    throw_ref
  )
  (func (;9;) (type 19) (param eqref i32) (result eqref)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 4
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 4
        i32.const 0
        i32.const 53
        call 3
        global.set 4
      end
      i32.const 4
      global.get 4
      struct.new 5
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
  (func (;10;) (type 20) (param eqref i32 eqref)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 4
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 4
        i32.const 0
        i32.const 53
        call 3
        global.set 4
      end
      i32.const 4
      global.get 4
      struct.new 5
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
  (func (;11;) (type 21) (param eqref i32) (result externref)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 4
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 4
        i32.const 0
        i32.const 53
        call 3
        global.set 4
      end
      i32.const 4
      global.get 4
      struct.new 5
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
  (func (;12;) (type 22) (param eqref i32 externref)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 4
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 4
        i32.const 0
        i32.const 53
        call 3
        global.set 4
      end
      i32.const 4
      global.get 4
      struct.new 5
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
  (func (;13;) (type 23) (param eqref i32) (result f32)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 4
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 4
        i32.const 0
        i32.const 53
        call 3
        global.set 4
      end
      i32.const 4
      global.get 4
      struct.new 5
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
  (func (;14;) (type 24) (param eqref i32 f32)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 4
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 4
        i32.const 0
        i32.const 53
        call 3
        global.set 4
      end
      i32.const 4
      global.get 4
      struct.new 5
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
  (func (;15;) (type 13) (param eqref eqref) (result eqref)
    (local eqref)
    local.get 0
    ref.is_null
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 0
    ref.test (ref 12)
    i32.eqz
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 0
    ref.cast (ref 12)
    struct.get 12 0
    local.set 2
    local.get 1
    global.get 9
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 10
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
      ref.cast (ref 11)
      struct.get 11 1
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
  (data (;0;) "S\00t\00a\00c\00k\00 \00e\00v\00e\00n\00t\00 \00\92! \00i\00n\00t\00e\00r\00f\00a\00c\00e\00 \00\92! \00c\00a\00n\00v\00a\00s\00")
  (data (;1;) "E\00x\00c\00e\00p\00t\00i\00o\00n\00 \00o\00f\00 \00t\00y\00p\00e\00 \00'\00S\00y\00s\00t\00e\00m\00.\00E\00x\00c\00e\00p\00t\00i\00o\00n\00'\00 \00w\00a\00s\00 \00t\00h\00r\00o\00w\00n\00.\00")
  (data (;2;) "S\00y\00s\00t\00e\00m\00 \00e\00r\00r\00o\00r\00.\00")
  (data (;3;) "S\00p\00e\00c\00i\00f\00i\00e\00d\00 \00c\00a\00s\00t\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00.\00")
  (data (;4;) "O\00b\00j\00e\00c\00t\00 \00r\00e\00f\00e\00r\00e\00n\00c\00e\00 \00n\00o\00t\00 \00s\00e\00t\00 \00t\00o\00 \00a\00n\00 \00i\00n\00s\00t\00a\00n\00c\00e\00 \00o\00f\00 \00a\00n\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;5;) "O\00p\00e\00r\00a\00t\00i\00o\00n\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00 \00d\00u\00e\00 \00t\00o\00 \00t\00h\00e\00 \00c\00u\00r\00r\00e\00n\00t\00 \00s\00t\00a\00t\00e\00 \00o\00f\00 \00t\00h\00e\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;6;) "V\00a\00l\00u\00e\00 \00d\00o\00e\00s\00 \00n\00o\00t\00 \00f\00a\00l\00l\00 \00w\00i\00t\00h\00i\00n\00 \00t\00h\00e\00 \00e\00x\00p\00e\00c\00t\00e\00d\00 \00r\00a\00n\00g\00e\00.\00")
)
```
