# Shapes: Xui geometry → WASM → Canvas

[Open standalone demo](Shapes.html)

Source: [Shapes.cs](Fixtures/Canvas/Shapes.cs), [Host.cs](Fixtures/Canvas/Host.cs)

## Source

```csharp
using System.Runtime.InteropServices;
using CanvasDemo;
using Xui.Core.Canvas;
using Xui.Core.Math2D;

public static class Program
{
    public static int Main()
    {
        NativeCanvas.Clear(0.0625, 0.125, 0.25, 1);
        Rect left = default;
        left.X = 40; left.Y = 100; left.Width = 240; left.Height = 180;
        Color blue = default;
        blue.Red = 0.125f; blue.Green = 0.5f; blue.Blue = 1; blue.Alpha = 1;
        Draw.Fill(ref left, ref blue);
        Rect right = left;
        right.X = left.Right + (NFloat)40;
        Color coral = default;
        coral.Red = 1; coral.Green = 0.375f; coral.Blue = 0.25f; coral.Alpha = 1;
        Draw.Fill(ref right, ref coral);
        Color white = default;
        white.Red = 1; white.Green = 1; white.Blue = 1; white.Alpha = 1;
        Draw.Text("Hello from Xui WASM", 40, 60, 28, ref white);
        return 0;
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
    240,
    180,
    0.125,
    0.5,
    1,
    1
  ],
  [
    "FillRect",
    320,
    100,
    240,
    180,
    1,
    0.375,
    0.25,
    1
  ],
  [
    "FillText",
    "Hello from Xui WASM",
    40,
    60,
    28,
    1,
    1,
    1,
    1
  ]
]
```

## Exit Code

```text
0
```

## Retained methods

- `CanvasDemo.exe`: `Program.Main`
- `CanvasDemo.exe`: `CanvasDemo.Draw.Fill`
- `Xui.Core.dll`: `Xui.Core.Math2D.Rect.get_Right`
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
  (type (;11;) (struct (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32))))
  (type (;12;) (struct (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32))))
  (type (;13;) (func (result i32)))
  (type (;14;) (func (param eqref i32) (result eqref)))
  (type (;15;) (func (param eqref i32 eqref)))
  (type (;16;) (func (param eqref i32) (result externref)))
  (type (;17;) (func (param eqref i32 externref)))
  (type (;18;) (func (param eqref i32) (result f32)))
  (type (;19;) (func (param eqref i32 f32)))
  (type (;20;) (func (result i32)))
  (type (;21;) (func (param eqref eqref i32 i32)))
  (type (;22;) (func (param eqref i32) (result f32)))
  (type (;23;) (func (param externref f32 f32 f32 eqref i32)))
  (import "canvas" "Clear" (func (;0;) (type 2) (param f64 f64 f64 f64)))
  (import "canvas" "FillRect" (func (;1;) (type 3) (param f64 f64 f64 f64 f64 f64 f64 f64)))
  (import "canvas" "FillText" (func (;2;) (type 4) (param externref f64 f64 f64 f64 f64 f64 f64)))
  (import "wasm:js-string" "fromCharCodeArray" (func (;3;) (type 1) (param (ref null 0) i32 i32) (result (ref extern))))
  (memory (;0;) 16 32768)
  (tag (;0;) (type 6) (param eqref))
  (global (;0;) (mut externref) ref.null extern)
  (global (;1;) (mut externref) ref.null extern)
  (global (;2;) (mut externref) ref.null extern)
  (global (;3;) (mut externref) ref.null extern)
  (global (;4;) (mut externref) ref.null extern)
  (global (;5;) (mut externref) ref.null extern)
  (global (;6;) (mut externref) ref.null extern)
  (global (;7;) (mut i32) i32.const 16)
  (export "main" (func 4))
  (func (;4;) (type 20) (result i32)
    (local i32 i32 i32 i32 i32 eqref i32 eqref i32 f32 eqref eqref i32 i32 eqref i32 externref f32 f32 f32 eqref i32 exnref i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 7
    local.tee 23
    i32.const 1048480
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 23
    i32.const 96
    i32.add
    global.set 7
    local.get 23
    i32.const 0
    i32.add
    local.set 0
    local.get 0
    i32.const 0
    i32.const 16
    memory.fill
    local.get 23
    i32.const 16
    i32.add
    local.set 1
    local.get 1
    i32.const 0
    i32.const 16
    memory.fill
    local.get 23
    i32.const 32
    i32.add
    local.set 2
    local.get 2
    i32.const 0
    i32.const 16
    memory.fill
    local.get 23
    i32.const 48
    i32.add
    local.set 3
    local.get 3
    i32.const 0
    i32.const 16
    memory.fill
    local.get 23
    i32.const 64
    i32.add
    local.set 4
    local.get 4
    i32.const 0
    i32.const 16
    memory.fill
    block (type 9) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        f64.const 0x1p-4 (;=0.0625;)
        f64.const 0x1p-3 (;=0.125;)
        f64.const 0x1p-2 (;=0.25;)
        f64.const 0x1p+0 (;=1;)
        call 0
        local.get 0
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
        local.get 5
        ref.is_null
        local.get 6
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
        local.get 5
        local.get 5
        ref.is_null
        if (type 13) (result i32) ;; label = @3
          local.get 6
          i32.const 0
          i32.add
        else
          local.get 6
          i32.const 0
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 13
        local.get 5
        ref.is_null
        local.get 6
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
        local.get 5
        local.get 5
        ref.is_null
        if (type 13) (result i32) ;; label = @3
          local.get 6
          i32.const 4
          i32.add
        else
          local.get 6
          i32.const 1
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 13
        local.get 5
        ref.is_null
        local.get 6
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
        local.get 5
        local.get 5
        ref.is_null
        if (type 13) (result i32) ;; label = @3
          local.get 6
          i32.const 8
          i32.add
        else
          local.get 6
          i32.const 2
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 13
        local.get 5
        ref.is_null
        local.get 6
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
        local.get 5
        local.get 5
        ref.is_null
        if (type 13) (result i32) ;; label = @3
          local.get 6
          i32.const 12
          i32.add
        else
          local.get 6
          i32.const 3
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 13
        local.get 0
        local.set 6
        ref.null eq
        local.get 6
        i32.const 40
        f32.convert_i32_s
        f64.promote_f32
        local.set 32
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
        local.get 5
        local.get 5
        ref.is_null
        if (type 13) (result i32) ;; label = @3
          local.get 6
          i32.const 0
          i32.add
        else
          local.get 6
          i32.const 0
          i32.add
        end
        local.get 32
        f32.demote_f64
        call 13
        local.get 0
        local.set 6
        ref.null eq
        local.get 6
        i32.const 100
        f32.convert_i32_s
        f64.promote_f32
        local.set 32
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
        local.get 5
        local.get 5
        ref.is_null
        if (type 13) (result i32) ;; label = @3
          local.get 6
          i32.const 4
          i32.add
        else
          local.get 6
          i32.const 1
          i32.add
        end
        local.get 32
        f32.demote_f64
        call 13
        local.get 0
        local.set 6
        ref.null eq
        local.get 6
        i32.const 240
        f32.convert_i32_s
        f64.promote_f32
        local.set 32
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
        local.get 5
        local.get 5
        ref.is_null
        if (type 13) (result i32) ;; label = @3
          local.get 6
          i32.const 8
          i32.add
        else
          local.get 6
          i32.const 2
          i32.add
        end
        local.get 32
        f32.demote_f64
        call 13
        local.get 0
        local.set 6
        ref.null eq
        local.get 6
        i32.const 180
        f32.convert_i32_s
        f64.promote_f32
        local.set 32
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
        local.get 5
        local.get 5
        ref.is_null
        if (type 13) (result i32) ;; label = @3
          local.get 6
          i32.const 12
          i32.add
        else
          local.get 6
          i32.const 3
          i32.add
        end
        local.get 32
        f32.demote_f64
        call 13
        local.get 1
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
        local.get 5
        ref.is_null
        local.get 6
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
        local.get 5
        local.get 5
        ref.is_null
        if (type 13) (result i32) ;; label = @3
          local.get 6
          i32.const 0
          i32.add
        else
          local.get 6
          i32.const 0
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 13
        local.get 5
        ref.is_null
        local.get 6
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
        local.get 5
        local.get 5
        ref.is_null
        if (type 13) (result i32) ;; label = @3
          local.get 6
          i32.const 4
          i32.add
        else
          local.get 6
          i32.const 1
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 13
        local.get 5
        ref.is_null
        local.get 6
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
        local.get 5
        local.get 5
        ref.is_null
        if (type 13) (result i32) ;; label = @3
          local.get 6
          i32.const 8
          i32.add
        else
          local.get 6
          i32.const 2
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 13
        local.get 5
        ref.is_null
        local.get 6
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
        local.get 5
        local.get 5
        ref.is_null
        if (type 13) (result i32) ;; label = @3
          local.get 6
          i32.const 12
          i32.add
        else
          local.get 6
          i32.const 3
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 13
        local.get 1
        local.set 6
        ref.null eq
        local.get 6
        f32.const 0x1p-3 (;=0.125;)
        f64.promote_f32
        f32.demote_f64
        f64.promote_f32
        local.set 32
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
        local.get 5
        local.get 5
        ref.is_null
        if (type 13) (result i32) ;; label = @3
          local.get 6
          i32.const 0
          i32.add
        else
          local.get 6
          i32.const 0
          i32.add
        end
        local.get 32
        f32.demote_f64
        call 13
        local.get 1
        local.set 6
        ref.null eq
        local.get 6
        f32.const 0x1p-1 (;=0.5;)
        f64.promote_f32
        f32.demote_f64
        f64.promote_f32
        local.set 32
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
        local.get 5
        local.get 5
        ref.is_null
        if (type 13) (result i32) ;; label = @3
          local.get 6
          i32.const 4
          i32.add
        else
          local.get 6
          i32.const 1
          i32.add
        end
        local.get 32
        f32.demote_f64
        call 13
        local.get 1
        local.set 6
        ref.null eq
        local.get 6
        i32.const 1
        f32.convert_i32_s
        f64.promote_f32
        local.set 32
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
        local.get 5
        local.get 5
        ref.is_null
        if (type 13) (result i32) ;; label = @3
          local.get 6
          i32.const 8
          i32.add
        else
          local.get 6
          i32.const 2
          i32.add
        end
        local.get 32
        f32.demote_f64
        call 13
        local.get 1
        local.set 6
        ref.null eq
        local.get 6
        i32.const 1
        f32.convert_i32_s
        f64.promote_f32
        local.set 32
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
        local.get 5
        local.get 5
        ref.is_null
        if (type 13) (result i32) ;; label = @3
          local.get 6
          i32.const 12
          i32.add
        else
          local.get 6
          i32.const 3
          i32.add
        end
        local.get 32
        f32.demote_f64
        call 13
        local.get 0
        local.set 6
        ref.null eq
        local.get 6
        local.get 1
        local.set 6
        ref.null eq
        local.get 6
        local.set 13
        local.set 11
        local.set 12
        local.set 10
        local.get 10
        local.get 11
        local.get 12
        local.get 13
        call 5
        local.get 0
        local.set 24
        local.get 23
        i32.const 80
        i32.add
        local.get 24
        i32.const 16
        memory.copy
        local.get 23
        i32.const 80
        i32.add
        local.set 24
        local.get 2
        local.get 24
        i32.const 16
        memory.copy
        local.get 2
        local.set 6
        ref.null eq
        local.get 6
        local.get 0
        local.set 6
        ref.null eq
        local.get 6
        local.set 15
        local.set 14
        local.get 14
        local.get 15
        call 6
        f64.promote_f32
        i32.const 40
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 9
        f32.demote_f64
        local.get 9
        f32.add
        f64.promote_f32
        local.set 32
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
        local.get 5
        local.get 5
        ref.is_null
        if (type 13) (result i32) ;; label = @3
          local.get 6
          i32.const 0
          i32.add
        else
          local.get 6
          i32.const 0
          i32.add
        end
        local.get 32
        f32.demote_f64
        call 13
        local.get 3
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
        local.get 5
        ref.is_null
        local.get 6
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
        local.get 5
        local.get 5
        ref.is_null
        if (type 13) (result i32) ;; label = @3
          local.get 6
          i32.const 0
          i32.add
        else
          local.get 6
          i32.const 0
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 13
        local.get 5
        ref.is_null
        local.get 6
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
        local.get 5
        local.get 5
        ref.is_null
        if (type 13) (result i32) ;; label = @3
          local.get 6
          i32.const 4
          i32.add
        else
          local.get 6
          i32.const 1
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 13
        local.get 5
        ref.is_null
        local.get 6
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
        local.get 5
        local.get 5
        ref.is_null
        if (type 13) (result i32) ;; label = @3
          local.get 6
          i32.const 8
          i32.add
        else
          local.get 6
          i32.const 2
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 13
        local.get 5
        ref.is_null
        local.get 6
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
        local.get 5
        local.get 5
        ref.is_null
        if (type 13) (result i32) ;; label = @3
          local.get 6
          i32.const 12
          i32.add
        else
          local.get 6
          i32.const 3
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 13
        local.get 3
        local.set 6
        ref.null eq
        local.get 6
        i32.const 1
        f32.convert_i32_s
        f64.promote_f32
        local.set 32
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
        local.get 5
        local.get 5
        ref.is_null
        if (type 13) (result i32) ;; label = @3
          local.get 6
          i32.const 0
          i32.add
        else
          local.get 6
          i32.const 0
          i32.add
        end
        local.get 32
        f32.demote_f64
        call 13
        local.get 3
        local.set 6
        ref.null eq
        local.get 6
        f32.const 0x1.8p-2 (;=0.375;)
        f64.promote_f32
        f32.demote_f64
        f64.promote_f32
        local.set 32
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
        local.get 5
        local.get 5
        ref.is_null
        if (type 13) (result i32) ;; label = @3
          local.get 6
          i32.const 4
          i32.add
        else
          local.get 6
          i32.const 1
          i32.add
        end
        local.get 32
        f32.demote_f64
        call 13
        local.get 3
        local.set 6
        ref.null eq
        local.get 6
        f32.const 0x1p-2 (;=0.25;)
        f64.promote_f32
        f32.demote_f64
        f64.promote_f32
        local.set 32
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
        local.get 5
        local.get 5
        ref.is_null
        if (type 13) (result i32) ;; label = @3
          local.get 6
          i32.const 8
          i32.add
        else
          local.get 6
          i32.const 2
          i32.add
        end
        local.get 32
        f32.demote_f64
        call 13
        local.get 3
        local.set 6
        ref.null eq
        local.get 6
        i32.const 1
        f32.convert_i32_s
        f64.promote_f32
        local.set 32
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
        local.get 5
        local.get 5
        ref.is_null
        if (type 13) (result i32) ;; label = @3
          local.get 6
          i32.const 12
          i32.add
        else
          local.get 6
          i32.const 3
          i32.add
        end
        local.get 32
        f32.demote_f64
        call 13
        local.get 2
        local.set 6
        ref.null eq
        local.get 6
        local.get 3
        local.set 6
        ref.null eq
        local.get 6
        local.set 13
        local.set 11
        local.set 12
        local.set 10
        local.get 10
        local.get 11
        local.get 12
        local.get 13
        call 5
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
        local.get 5
        ref.is_null
        local.get 6
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
        local.get 5
        local.get 5
        ref.is_null
        if (type 13) (result i32) ;; label = @3
          local.get 6
          i32.const 0
          i32.add
        else
          local.get 6
          i32.const 0
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 13
        local.get 5
        ref.is_null
        local.get 6
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
        local.get 5
        local.get 5
        ref.is_null
        if (type 13) (result i32) ;; label = @3
          local.get 6
          i32.const 4
          i32.add
        else
          local.get 6
          i32.const 1
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 13
        local.get 5
        ref.is_null
        local.get 6
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
        local.get 5
        local.get 5
        ref.is_null
        if (type 13) (result i32) ;; label = @3
          local.get 6
          i32.const 8
          i32.add
        else
          local.get 6
          i32.const 2
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 13
        local.get 5
        ref.is_null
        local.get 6
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
        local.get 5
        local.get 5
        ref.is_null
        if (type 13) (result i32) ;; label = @3
          local.get 6
          i32.const 12
          i32.add
        else
          local.get 6
          i32.const 3
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 13
        local.get 4
        local.set 6
        ref.null eq
        local.get 6
        i32.const 1
        f32.convert_i32_s
        f64.promote_f32
        local.set 32
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
        local.get 5
        local.get 5
        ref.is_null
        if (type 13) (result i32) ;; label = @3
          local.get 6
          i32.const 0
          i32.add
        else
          local.get 6
          i32.const 0
          i32.add
        end
        local.get 32
        f32.demote_f64
        call 13
        local.get 4
        local.set 6
        ref.null eq
        local.get 6
        i32.const 1
        f32.convert_i32_s
        f64.promote_f32
        local.set 32
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
        local.get 5
        local.get 5
        ref.is_null
        if (type 13) (result i32) ;; label = @3
          local.get 6
          i32.const 4
          i32.add
        else
          local.get 6
          i32.const 1
          i32.add
        end
        local.get 32
        f32.demote_f64
        call 13
        local.get 4
        local.set 6
        ref.null eq
        local.get 6
        i32.const 1
        f32.convert_i32_s
        f64.promote_f32
        local.set 32
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
        local.get 5
        local.get 5
        ref.is_null
        if (type 13) (result i32) ;; label = @3
          local.get 6
          i32.const 8
          i32.add
        else
          local.get 6
          i32.const 2
          i32.add
        end
        local.get 32
        f32.demote_f64
        call 13
        local.get 4
        local.set 6
        ref.null eq
        local.get 6
        i32.const 1
        f32.convert_i32_s
        f64.promote_f32
        local.set 32
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
        local.get 5
        local.get 5
        ref.is_null
        if (type 13) (result i32) ;; label = @3
          local.get 6
          i32.const 12
          i32.add
        else
          local.get 6
          i32.const 3
          i32.add
        end
        local.get 32
        f32.demote_f64
        call 13
        global.get 0
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 19
          array.new_data 0 0
          i32.const 0
          i32.const 19
          call 3
          global.set 0
        end
        global.get 0
        i32.const 40
        f32.convert_i32_s
        f64.promote_f32
        i32.const 60
        f32.convert_i32_s
        f64.promote_f32
        i32.const 28
        f32.convert_i32_s
        f64.promote_f32
        local.get 4
        local.set 6
        ref.null eq
        local.get 6
        local.set 21
        local.set 20
        f32.demote_f64
        local.set 19
        f32.demote_f64
        local.set 18
        f32.demote_f64
        local.set 17
        local.set 16
        local.get 16
        local.get 17
        local.get 18
        local.get 19
        local.get 20
        local.get 21
        call 7
        i32.const 0
        local.get 23
        global.set 7
        return
        unreachable
      end
      unreachable
    end
    local.set 22
    local.get 23
    global.set 7
    local.get 22
    throw_ref
  )
  (func (;5;) (type 21) (param eqref eqref i32 i32)
    (local eqref i32 eqref i32 f32 exnref i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 7
    local.tee 10
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 10
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
        if (type 13) (result i32) ;; label = @3
          local.get 5
          i32.const 0
          i32.add
        else
          local.get 5
          i32.const 0
          i32.add
        end
        call 12
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
        if (type 13) (result i32) ;; label = @3
          local.get 5
          i32.const 4
          i32.add
        else
          local.get 5
          i32.const 1
          i32.add
        end
        call 12
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
        if (type 13) (result i32) ;; label = @3
          local.get 5
          i32.const 8
          i32.add
        else
          local.get 5
          i32.const 2
          i32.add
        end
        call 12
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
        if (type 13) (result i32) ;; label = @3
          local.get 5
          i32.const 12
          i32.add
        else
          local.get 5
          i32.const 3
          i32.add
        end
        call 12
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
        if (type 13) (result i32) ;; label = @3
          local.get 5
          i32.const 0
          i32.add
        else
          local.get 5
          i32.const 0
          i32.add
        end
        call 12
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
        if (type 13) (result i32) ;; label = @3
          local.get 5
          i32.const 4
          i32.add
        else
          local.get 5
          i32.const 1
          i32.add
        end
        call 12
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
        if (type 13) (result i32) ;; label = @3
          local.get 5
          i32.const 8
          i32.add
        else
          local.get 5
          i32.const 2
          i32.add
        end
        call 12
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
        if (type 13) (result i32) ;; label = @3
          local.get 5
          i32.const 12
          i32.add
        else
          local.get 5
          i32.const 3
          i32.add
        end
        call 12
        f64.promote_f32
        call 1
        local.get 10
        global.set 7
        return
        unreachable
      end
      unreachable
    end
    local.set 9
    local.get 10
    global.set 7
    local.get 9
    throw_ref
  )
  (func (;6;) (type 22) (param eqref i32) (result f32)
    (local eqref i32 eqref i32 f32 exnref i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 7
    local.tee 8
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 8
    i32.const 0
    i32.add
    global.set 7
    block (type 9) (result exnref) ;; label = @1
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
        local.get 2
        local.get 2
        ref.is_null
        if (type 13) (result i32) ;; label = @3
          local.get 3
          i32.const 0
          i32.add
        else
          local.get 3
          i32.const 0
          i32.add
        end
        call 12
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
        local.get 2
        local.get 2
        ref.is_null
        if (type 13) (result i32) ;; label = @3
          local.get 3
          i32.const 8
          i32.add
        else
          local.get 3
          i32.const 2
          i32.add
        end
        call 12
        f64.promote_f32
        f32.demote_f64
        local.set 6
        f32.demote_f64
        local.get 6
        f32.add
        f64.promote_f32
        f32.demote_f64
        local.get 8
        global.set 7
        return
        unreachable
      end
      unreachable
    end
    local.set 7
    local.get 8
    global.set 7
    local.get 7
    throw_ref
  )
  (func (;7;) (type 23) (param externref f32 f32 f32 eqref i32)
    (local eqref i32 eqref i32 f32 exnref i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 7
    local.tee 12
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 12
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
        if (type 13) (result i32) ;; label = @3
          local.get 7
          i32.const 0
          i32.add
        else
          local.get 7
          i32.const 0
          i32.add
        end
        call 12
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
        if (type 13) (result i32) ;; label = @3
          local.get 7
          i32.const 4
          i32.add
        else
          local.get 7
          i32.const 1
          i32.add
        end
        call 12
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
        if (type 13) (result i32) ;; label = @3
          local.get 7
          i32.const 8
          i32.add
        else
          local.get 7
          i32.const 2
          i32.add
        end
        call 12
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
        if (type 13) (result i32) ;; label = @3
          local.get 7
          i32.const 12
          i32.add
        else
          local.get 7
          i32.const 3
          i32.add
        end
        call 12
        f64.promote_f32
        call 2
        local.get 12
        global.set 7
        return
        unreachable
      end
      unreachable
    end
    local.set 11
    local.get 12
    global.set 7
    local.get 11
    throw_ref
  )
  (func (;8;) (type 14) (param eqref i32) (result eqref)
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
    unreachable
  )
  (func (;9;) (type 15) (param eqref i32 eqref)
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
    unreachable
  )
  (func (;10;) (type 16) (param eqref i32) (result externref)
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
    unreachable
  )
  (func (;11;) (type 17) (param eqref i32 externref)
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
    unreachable
  )
  (func (;12;) (type 18) (param eqref i32) (result f32)
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
  (func (;13;) (type 19) (param eqref i32 f32)
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
  (data (;0;) "H\00e\00l\00l\00o\00 \00f\00r\00o\00m\00 \00X\00u\00i\00 \00W\00A\00S\00M\00")
  (data (;1;) "E\00x\00c\00e\00p\00t\00i\00o\00n\00 \00o\00f\00 \00t\00y\00p\00e\00 \00'\00S\00y\00s\00t\00e\00m\00.\00E\00x\00c\00e\00p\00t\00i\00o\00n\00'\00 \00w\00a\00s\00 \00t\00h\00r\00o\00w\00n\00.\00")
  (data (;2;) "S\00y\00s\00t\00e\00m\00 \00e\00r\00r\00o\00r\00.\00")
  (data (;3;) "S\00p\00e\00c\00i\00f\00i\00e\00d\00 \00c\00a\00s\00t\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00.\00")
  (data (;4;) "O\00b\00j\00e\00c\00t\00 \00r\00e\00f\00e\00r\00e\00n\00c\00e\00 \00n\00o\00t\00 \00s\00e\00t\00 \00t\00o\00 \00a\00n\00 \00i\00n\00s\00t\00a\00n\00c\00e\00 \00o\00f\00 \00a\00n\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;5;) "O\00p\00e\00r\00a\00t\00i\00o\00n\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00 \00d\00u\00e\00 \00t\00o\00 \00t\00h\00e\00 \00c\00u\00r\00r\00e\00n\00t\00 \00s\00t\00a\00t\00e\00 \00o\00f\00 \00t\00h\00e\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;6;) "V\00a\00l\00u\00e\00 \00d\00o\00e\00s\00 \00n\00o\00t\00 \00f\00a\00l\00l\00 \00w\00i\00t\00h\00i\00n\00 \00t\00h\00e\00 \00e\00x\00p\00e\00c\00t\00e\00d\00 \00r\00a\00n\00g\00e\00.\00")
)
```
