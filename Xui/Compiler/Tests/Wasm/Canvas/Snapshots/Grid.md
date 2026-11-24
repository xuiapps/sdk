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
  (type (;5;) (func (result i32)))
  (type (;6;) (func))
  (type (;7;) (func))
  (type (;8;) (func))
  (type (;9;) (func))
  (type (;10;) (func (param i32 i32)))
  (type (;11;) (func (param externref f32 f32 f32 i32)))
  (import "canvas" "Clear" (func (;0;) (type 2) (param f64 f64 f64 f64)))
  (import "canvas" "FillRect" (func (;1;) (type 3) (param f64 f64 f64 f64 f64 f64 f64 f64)))
  (import "canvas" "FillText" (func (;2;) (type 4) (param externref f64 f64 f64 f64 f64 f64 f64)))
  (import "wasm:js-string" "fromCharCodeArray" (func (;3;) (type 1) (param (ref null 0) i32 i32) (result (ref extern))))
  (memory (;0;) 16 16)
  (global (;0;) (mut externref) ref.null extern)
  (global (;1;) (mut i32) i32.const 16)
  (export "main" (func 4))
  (func (;4;) (type 5) (result i32)
    (local i32 i32 i32 i32 f32 externref f32 f32 f32 i32 i32 i32 i32 i64 f64)
    global.get 1
    local.tee 10
    i32.const 1048544
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 10
    i32.const 32
    i32.add
    global.set 1
    local.get 10
    i32.const 0
    i32.add
    local.set 0
    local.get 0
    i32.const 0
    i32.const 16
    memory.fill
    local.get 10
    i32.const 16
    i32.add
    local.set 1
    local.get 1
    i32.const 0
    i32.const 16
    memory.fill
    f64.const 0x1p-4 (;=0.0625;)
    f64.const 0x1p-3 (;=0.125;)
    f64.const 0x1p-2 (;=0.25;)
    f64.const 0x1p+0 (;=1;)
    call 0
    local.get 0
    i32.const 0
    i32.const 16
    memory.fill
    local.get 0
    f32.const 0x1p-2 (;=0.25;)
    f64.promote_f32
    f32.demote_f64
    f64.promote_f32
    local.set 14
    i32.const 0
    i32.add
    local.get 14
    f32.demote_f64
    f32.store align=1
    local.get 0
    f32.const 0x1.8p-1 (;=0.75;)
    f64.promote_f32
    f32.demote_f64
    f64.promote_f32
    local.set 14
    i32.const 4
    i32.add
    local.get 14
    f32.demote_f64
    f32.store align=1
    local.get 0
    i32.const 1
    f32.convert_i32_s
    f64.promote_f32
    local.set 14
    i32.const 8
    i32.add
    local.get 14
    f32.demote_f64
    f32.store align=1
    local.get 0
    i32.const 1
    f32.convert_i32_s
    f64.promote_f32
    local.set 14
    i32.const 12
    i32.add
    local.get 14
    f32.demote_f64
    f32.store align=1
    local.get 1
    i32.const 0
    i32.const 16
    memory.fill
    local.get 1
    i32.const 72
    f32.convert_i32_s
    f64.promote_f32
    local.set 14
    i32.const 8
    i32.add
    local.get 14
    f32.demote_f64
    f32.store align=1
    local.get 1
    i32.const 56
    f32.convert_i32_s
    f64.promote_f32
    local.set 14
    i32.const 12
    i32.add
    local.get 14
    f32.demote_f64
    f32.store align=1
    i32.const 0
    local.set 2
    block (type 6) ;; label = @1
      loop (type 6) ;; label = @2
        local.get 2
        i32.const 3
        i32.lt_s
        if (type 7) ;; label = @3
          i32.const 0
          local.set 3
          block (type 8) ;; label = @4
            loop (type 8) ;; label = @5
              local.get 3
              i32.const 6
              i32.lt_s
              if (type 9) ;; label = @6
                local.get 1
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
                local.set 4
                f32.demote_f64
                local.get 4
                f32.mul
                f64.promote_f32
                f32.demote_f64
                local.set 4
                f32.demote_f64
                local.get 4
                f32.add
                f64.promote_f32
                local.set 14
                i32.const 0
                i32.add
                local.get 14
                f32.demote_f64
                f32.store align=1
                local.get 1
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
                local.set 4
                f32.demote_f64
                local.get 4
                f32.mul
                f64.promote_f32
                f32.demote_f64
                local.set 4
                f32.demote_f64
                local.get 4
                f32.add
                f64.promote_f32
                local.set 14
                i32.const 4
                i32.add
                local.get 14
                f32.demote_f64
                f32.store align=1
                local.get 1
                local.get 0
                call 5
                local.get 3
                i32.const 1
                i32.add
                local.set 3
                br 1 (;@5;)
              else
                br 2 (;@4;)
              end
              unreachable
            end
          end
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
    global.get 0
    ref.is_null
    if ;; label = @1
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
    local.set 9
    f32.demote_f64
    local.set 8
    f32.demote_f64
    local.set 7
    f32.demote_f64
    local.set 6
    local.set 5
    local.get 5
    local.get 6
    local.get 7
    local.get 8
    local.get 9
    call 6
    i32.const 18
    local.get 10
    global.set 1
    return
  )
  (func (;5;) (type 10) (param i32 i32)
    (local f32 i32 i32 i32 i64 f64)
    global.get 1
    local.tee 3
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 3
    i32.const 0
    i32.add
    global.set 1
    local.get 0
    i32.const 0
    i32.add
    f32.load align=1
    f64.promote_f32
    local.get 0
    i32.const 4
    i32.add
    f32.load align=1
    f64.promote_f32
    local.get 0
    i32.const 8
    i32.add
    f32.load align=1
    f64.promote_f32
    local.get 0
    i32.const 12
    i32.add
    f32.load align=1
    f64.promote_f32
    local.get 1
    i32.const 0
    i32.add
    f32.load align=1
    f64.promote_f32
    local.get 1
    i32.const 4
    i32.add
    f32.load align=1
    f64.promote_f32
    local.get 1
    i32.const 8
    i32.add
    f32.load align=1
    f64.promote_f32
    local.get 1
    i32.const 12
    i32.add
    f32.load align=1
    f64.promote_f32
    call 1
    local.get 3
    global.set 1
    return
  )
  (func (;6;) (type 11) (param externref f32 f32 f32 i32)
    (local f32 i32 i32 i32 i64 f64)
    global.get 1
    local.tee 6
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 6
    i32.const 0
    i32.add
    global.set 1
    local.get 0
    local.get 1
    f64.promote_f32
    local.get 2
    f64.promote_f32
    local.get 3
    f64.promote_f32
    local.get 4
    i32.const 0
    i32.add
    f32.load align=1
    f64.promote_f32
    local.get 4
    i32.const 4
    i32.add
    f32.load align=1
    f64.promote_f32
    local.get 4
    i32.const 8
    i32.add
    f32.load align=1
    f64.promote_f32
    local.get 4
    i32.const 12
    i32.add
    f32.load align=1
    f64.promote_f32
    call 2
    local.get 6
    global.set 1
    return
  )
  (data (;0;) "G\00e\00o\00m\00e\00t\00r\00y\00 \00c\00a\00l\00c\00u\00l\00a\00t\00e\00d\00 \00i\00n\00 \00C\00#\00")
)
```
