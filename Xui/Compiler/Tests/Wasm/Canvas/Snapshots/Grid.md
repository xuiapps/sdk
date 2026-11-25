# Grid: Xui geometry → WASM → Canvas

[Open standalone demo](Grid.html)

Source: [Grid.cs](Fixtures/Canvas/Grid.cs), [Host.cs](Fixtures/Canvas/Host.cs)

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
        Color color = default;
        color.Red = 0.25f; color.Green = 0.75f; color.Blue = 1; color.Alpha = 1;
        Rect cell = default; cell.Width = 72; cell.Height = 56;
        for (int row = 0; row < 3; row++)
        {
            for (int column = 0; column < 6; column++)
            {
                cell.X = (NFloat)40 + (NFloat)column * (NFloat)88;
                cell.Y = (NFloat)96 + (NFloat)row * (NFloat)72;
                Draw.Fill(ref cell, ref color);
            }
        }
        Draw.Text("Geometry calculated in C#", 40, 56, 24, ref color);
        return 18;
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
    96,
    72,
    56,
    0.25,
    0.75,
    1,
    1
  ],
  [
    "FillRect",
    128,
    96,
    72,
    56,
    0.25,
    0.75,
    1,
    1
  ],
  [
    "FillRect",
    216,
    96,
    72,
    56,
    0.25,
    0.75,
    1,
    1
  ],
  [
    "FillRect",
    304,
    96,
    72,
    56,
    0.25,
    0.75,
    1,
    1
  ],
  [
    "FillRect",
    392,
    96,
    72,
    56,
    0.25,
    0.75,
    1,
    1
  ],
  [
    "FillRect",
    480,
    96,
    72,
    56,
    0.25,
    0.75,
    1,
    1
  ],
  [
    "FillRect",
    40,
    168,
    72,
    56,
    0.25,
    0.75,
    1,
    1
  ],
  [
    "FillRect",
    128,
    168,
    72,
    56,
    0.25,
    0.75,
    1,
    1
  ],
  [
    "FillRect",
    216,
    168,
    72,
    56,
    0.25,
    0.75,
    1,
    1
  ],
  [
    "FillRect",
    304,
    168,
    72,
    56,
    0.25,
    0.75,
    1,
    1
  ],
  [
    "FillRect",
    392,
    168,
    72,
    56,
    0.25,
    0.75,
    1,
    1
  ],
  [
    "FillRect",
    480,
    168,
    72,
    56,
    0.25,
    0.75,
    1,
    1
  ],
  [
    "FillRect",
    40,
    240,
    72,
    56,
    0.25,
    0.75,
    1,
    1
  ],
  [
    "FillRect",
    128,
    240,
    72,
    56,
    0.25,
    0.75,
    1,
    1
  ],
  [
    "FillRect",
    216,
    240,
    72,
    56,
    0.25,
    0.75,
    1,
    1
  ],
  [
    "FillRect",
    304,
    240,
    72,
    56,
    0.25,
    0.75,
    1,
    1
  ],
  [
    "FillRect",
    392,
    240,
    72,
    56,
    0.25,
    0.75,
    1,
    1
  ],
  [
    "FillRect",
    480,
    240,
    72,
    56,
    0.25,
    0.75,
    1,
    1
  ],
  [
    "FillText",
    "Geometry calculated in C#",
    40,
    56,
    24,
    0.25,
    0.75,
    1,
    1
  ]
]
```

## Exit Code

```text
18
```

## Retained methods

- `CanvasDemo.exe`: `Program.Main`
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
  (type (;21;) (func))
  (type (;22;) (func))
  (type (;23;) (func))
  (type (;24;) (func))
  (type (;25;) (func (param eqref eqref i32 i32)))
  (type (;26;) (func (param externref f32 f32 f32 eqref i32)))
  (import "canvas" "Clear" (func (;0;) (type 2) (param f64 f64 f64 f64)))
  (import "canvas" "FillRect" (func (;1;) (type 3) (param f64 f64 f64 f64 f64 f64 f64 f64)))
  (import "canvas" "FillText" (func (;2;) (type 4) (param externref f64 f64 f64 f64 f64 f64 f64)))
  (import "wasm:js-string" "fromCharCodeArray" (func (;3;) (type 1) (param (ref null 0) i32 i32) (result (ref extern))))
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
  (export "main" (func 4))
  (func (;4;) (type 20) (result i32)
    (local i32 i32 i32 i32 eqref i32 eqref i32 f32 eqref eqref i32 i32 externref f32 f32 f32 eqref i32 exnref i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 7
    local.tee 20
    i32.const 1048544
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 20
    i32.const 32
    i32.add
    global.set 7
    local.get 20
    i32.const 0
    i32.add
    local.set 0
    local.get 0
    i32.const 0
    i32.const 16
    memory.fill
    local.get 20
    i32.const 16
    i32.add
    local.set 1
    local.get 1
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
        f32.const 0x0p+0 (;=0;)
        call 12
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
        f32.const 0x0p+0 (;=0;)
        call 12
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
        f32.const 0x0p+0 (;=0;)
        call 12
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
        f32.const 0x0p+0 (;=0;)
        call 12
        local.get 0
        local.set 5
        ref.null eq
        local.get 5
        f32.const 0x1p-2 (;=0.25;)
        f64.promote_f32
        f32.demote_f64
        f64.promote_f32
        local.set 29
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
        local.get 29
        f32.demote_f64
        call 12
        local.get 0
        local.set 5
        ref.null eq
        local.get 5
        f32.const 0x1.8p-1 (;=0.75;)
        f64.promote_f32
        f32.demote_f64
        f64.promote_f32
        local.set 29
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
        local.get 29
        f32.demote_f64
        call 12
        local.get 0
        local.set 5
        ref.null eq
        local.get 5
        i32.const 1
        f32.convert_i32_s
        f64.promote_f32
        local.set 29
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
        local.get 29
        f32.demote_f64
        call 12
        local.get 0
        local.set 5
        ref.null eq
        local.get 5
        i32.const 1
        f32.convert_i32_s
        f64.promote_f32
        local.set 29
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
        local.get 29
        f32.demote_f64
        call 12
        local.get 1
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
        f32.const 0x0p+0 (;=0;)
        call 12
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
        f32.const 0x0p+0 (;=0;)
        call 12
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
        f32.const 0x0p+0 (;=0;)
        call 12
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
        f32.const 0x0p+0 (;=0;)
        call 12
        local.get 1
        local.set 5
        ref.null eq
        local.get 5
        i32.const 72
        f32.convert_i32_s
        f64.promote_f32
        local.set 29
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
        local.get 29
        f32.demote_f64
        call 12
        local.get 1
        local.set 5
        ref.null eq
        local.get 5
        i32.const 56
        f32.convert_i32_s
        f64.promote_f32
        local.set 29
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
        local.get 29
        f32.demote_f64
        call 12
        i32.const 0
        local.set 2
        block (type 21) ;; label = @3
          loop (type 21) ;; label = @4
            local.get 2
            i32.const 3
            i32.lt_s
            if (type 22) ;; label = @5
              i32.const 0
              local.set 3
              block (type 23) ;; label = @6
                loop (type 23) ;; label = @7
                  local.get 3
                  i32.const 6
                  i32.lt_s
                  if (type 24) ;; label = @8
                    local.get 1
                    local.set 5
                    ref.null eq
                    local.get 5
                    i32.const 40
                    f32.convert_i32_s
                    f64.promote_f32
                    local.get 3
                    f32.convert_i32_s
                    f64.promote_f32
                    i32.const 88
                    f32.convert_i32_s
                    f64.promote_f32
                    f32.demote_f64
                    local.set 8
                    f32.demote_f64
                    local.get 8
                    f32.mul
                    f64.promote_f32
                    f32.demote_f64
                    local.set 8
                    f32.demote_f64
                    local.get 8
                    f32.add
                    f64.promote_f32
                    local.set 29
                    local.set 5
                    local.set 4
                    local.get 4
                    ref.is_null
                    local.get 5
                    i32.const -1
                    i32.eq
                    i32.and
                    if ;; label = @9
                      i32.const 4
                      global.get 4
                      ref.is_null
                      if ;; label = @10
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
                    if (type 13) (result i32) ;; label = @9
                      local.get 5
                      i32.const 0
                      i32.add
                    else
                      local.get 5
                      i32.const 0
                      i32.add
                    end
                    local.get 29
                    f32.demote_f64
                    call 12
                    local.get 1
                    local.set 5
                    ref.null eq
                    local.get 5
                    i32.const 96
                    f32.convert_i32_s
                    f64.promote_f32
                    local.get 2
                    f32.convert_i32_s
                    f64.promote_f32
                    i32.const 72
                    f32.convert_i32_s
                    f64.promote_f32
                    f32.demote_f64
                    local.set 8
                    f32.demote_f64
                    local.get 8
                    f32.mul
                    f64.promote_f32
                    f32.demote_f64
                    local.set 8
                    f32.demote_f64
                    local.get 8
                    f32.add
                    f64.promote_f32
                    local.set 29
                    local.set 5
                    local.set 4
                    local.get 4
                    ref.is_null
                    local.get 5
                    i32.const -1
                    i32.eq
                    i32.and
                    if ;; label = @9
                      i32.const 4
                      global.get 4
                      ref.is_null
                      if ;; label = @10
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
                    if (type 13) (result i32) ;; label = @9
                      local.get 5
                      i32.const 4
                      i32.add
                    else
                      local.get 5
                      i32.const 1
                      i32.add
                    end
                    local.get 29
                    f32.demote_f64
                    call 12
                    local.get 1
                    local.set 5
                    ref.null eq
                    local.get 5
                    local.get 0
                    local.set 5
                    ref.null eq
                    local.get 5
                    local.set 12
                    local.set 10
                    local.set 11
                    local.set 9
                    local.get 9
                    local.get 10
                    local.get 11
                    local.get 12
                    call 5
                    local.get 3
                    i32.const 1
                    i32.add
                    local.set 3
                    br 1 (;@7;)
                  else
                    br 2 (;@6;)
                  end
                  unreachable
                end
              end
              local.get 2
              i32.const 1
              i32.add
              local.set 2
              br 1 (;@4;)
            else
              br 2 (;@3;)
            end
            unreachable
          end
        end
        global.get 0
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 25
          array.new_data 0 0
          i32.const 0
          i32.const 25
          call 3
          global.set 0
        end
        global.get 0
        i32.const 40
        f32.convert_i32_s
        f64.promote_f32
        i32.const 56
        f32.convert_i32_s
        f64.promote_f32
        i32.const 24
        f32.convert_i32_s
        f64.promote_f32
        local.get 0
        local.set 5
        ref.null eq
        local.get 5
        local.set 18
        local.set 17
        f32.demote_f64
        local.set 16
        f32.demote_f64
        local.set 15
        f32.demote_f64
        local.set 14
        local.set 13
        local.get 13
        local.get 14
        local.get 15
        local.get 16
        local.get 17
        local.get 18
        call 6
        i32.const 18
        local.get 20
        global.set 7
        return
        unreachable
      end
      unreachable
    end
    local.set 19
    local.get 20
    global.set 7
    local.get 19
    throw_ref
  )
  (func (;5;) (type 25) (param eqref eqref i32 i32)
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
        call 11
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
        call 11
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
        call 11
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
        call 11
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
        call 11
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
        call 11
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
        call 11
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
        call 11
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
  (func (;6;) (type 26) (param externref f32 f32 f32 eqref i32)
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
        call 11
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
        call 11
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
        call 11
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
        call 11
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
  (func (;7;) (type 14) (param eqref i32) (result eqref)
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
  (func (;8;) (type 15) (param eqref i32 eqref)
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
  (func (;9;) (type 16) (param eqref i32) (result externref)
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
  (func (;10;) (type 17) (param eqref i32 externref)
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
  (func (;11;) (type 18) (param eqref i32) (result f32)
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
  (func (;12;) (type 19) (param eqref i32 f32)
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
  (data (;0;) "G\00e\00o\00m\00e\00t\00r\00y\00 \00c\00a\00l\00c\00u\00l\00a\00t\00e\00d\00 \00i\00n\00 \00C\00#\00")
  (data (;1;) "E\00x\00c\00e\00p\00t\00i\00o\00n\00 \00o\00f\00 \00t\00y\00p\00e\00 \00'\00S\00y\00s\00t\00e\00m\00.\00E\00x\00c\00e\00p\00t\00i\00o\00n\00'\00 \00w\00a\00s\00 \00t\00h\00r\00o\00w\00n\00.\00")
  (data (;2;) "S\00y\00s\00t\00e\00m\00 \00e\00r\00r\00o\00r\00.\00")
  (data (;3;) "S\00p\00e\00c\00i\00f\00i\00e\00d\00 \00c\00a\00s\00t\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00.\00")
  (data (;4;) "O\00b\00j\00e\00c\00t\00 \00r\00e\00f\00e\00r\00e\00n\00c\00e\00 \00n\00o\00t\00 \00s\00e\00t\00 \00t\00o\00 \00a\00n\00 \00i\00n\00s\00t\00a\00n\00c\00e\00 \00o\00f\00 \00a\00n\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;5;) "O\00p\00e\00r\00a\00t\00i\00o\00n\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00 \00d\00u\00e\00 \00t\00o\00 \00t\00h\00e\00 \00c\00u\00r\00r\00e\00n\00t\00 \00s\00t\00a\00t\00e\00 \00o\00f\00 \00t\00h\00e\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;6;) "V\00a\00l\00u\00e\00 \00d\00o\00e\00s\00 \00n\00o\00t\00 \00f\00a\00l\00l\00 \00w\00i\00t\00h\00i\00n\00 \00t\00h\00e\00 \00e\00x\00p\00e\00c\00t\00e\00d\00 \00r\00a\00n\00g\00e\00.\00")
)
```
