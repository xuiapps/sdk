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
  (type (;5;) (func (result i32)))
  (type (;6;) (func (param i32 i32)))
  (type (;7;) (func (param i32) (result f32)))
  (type (;8;) (func (param externref f32 f32 f32 i32)))
  (import "canvas" "Clear" (func (;0;) (type 2) (param f64 f64 f64 f64)))
  (import "canvas" "FillRect" (func (;1;) (type 3) (param f64 f64 f64 f64 f64 f64 f64 f64)))
  (import "canvas" "FillText" (func (;2;) (type 4) (param externref f64 f64 f64 f64 f64 f64 f64)))
  (import "wasm:js-string" "fromCharCodeArray" (func (;3;) (type 1) (param (ref null 0) i32 i32) (result (ref extern))))
  (memory (;0;) 16 16)
  (global (;0;) (mut externref) ref.null extern)
  (global (;1;) (mut i32) i32.const 16)
  (export "main" (func 4))
  (func (;4;) (type 5) (result i32)
    (local i32 i32 i32 i32 i32 f32 i32 externref f32 f32 f32 i32 i32 i32 i32 i64 f64)
    global.get 1
    local.tee 12
    i32.const 1048480
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 12
    i32.const 96
    i32.add
    global.set 1
    local.get 12
    i32.const 0
    i32.add
    local.set 0
    local.get 0
    i32.const 0
    i32.const 16
    memory.fill
    local.get 12
    i32.const 16
    i32.add
    local.set 1
    local.get 1
    i32.const 0
    i32.const 16
    memory.fill
    local.get 12
    i32.const 32
    i32.add
    local.set 2
    local.get 2
    i32.const 0
    i32.const 16
    memory.fill
    local.get 12
    i32.const 48
    i32.add
    local.set 3
    local.get 3
    i32.const 0
    i32.const 16
    memory.fill
    local.get 12
    i32.const 64
    i32.add
    local.set 4
    local.get 4
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
    i32.const 40
    f32.convert_i32_s
    f64.promote_f32
    local.set 16
    i32.const 0
    i32.add
    local.get 16
    f32.demote_f64
    f32.store align=1
    local.get 0
    i32.const 100
    f32.convert_i32_s
    f64.promote_f32
    local.set 16
    i32.const 4
    i32.add
    local.get 16
    f32.demote_f64
    f32.store align=1
    local.get 0
    i32.const 240
    f32.convert_i32_s
    f64.promote_f32
    local.set 16
    i32.const 8
    i32.add
    local.get 16
    f32.demote_f64
    f32.store align=1
    local.get 0
    i32.const 180
    f32.convert_i32_s
    f64.promote_f32
    local.set 16
    i32.const 12
    i32.add
    local.get 16
    f32.demote_f64
    f32.store align=1
    local.get 1
    i32.const 0
    i32.const 16
    memory.fill
    local.get 1
    f32.const 0x1p-3 (;=0.125;)
    f64.promote_f32
    f32.demote_f64
    f64.promote_f32
    local.set 16
    i32.const 0
    i32.add
    local.get 16
    f32.demote_f64
    f32.store align=1
    local.get 1
    f32.const 0x1p-1 (;=0.5;)
    f64.promote_f32
    f32.demote_f64
    f64.promote_f32
    local.set 16
    i32.const 4
    i32.add
    local.get 16
    f32.demote_f64
    f32.store align=1
    local.get 1
    i32.const 1
    f32.convert_i32_s
    f64.promote_f32
    local.set 16
    i32.const 8
    i32.add
    local.get 16
    f32.demote_f64
    f32.store align=1
    local.get 1
    i32.const 1
    f32.convert_i32_s
    f64.promote_f32
    local.set 16
    i32.const 12
    i32.add
    local.get 16
    f32.demote_f64
    f32.store align=1
    local.get 0
    local.get 1
    call 5
    local.get 0
    local.set 13
    local.get 12
    i32.const 80
    i32.add
    local.get 13
    i32.const 16
    memory.copy
    local.get 12
    i32.const 80
    i32.add
    local.set 13
    local.get 2
    local.get 13
    i32.const 16
    memory.copy
    local.get 2
    local.get 0
    local.set 6
    local.get 6
    call 6
    f64.promote_f32
    i32.const 40
    f32.convert_i32_s
    f64.promote_f32
    f32.demote_f64
    local.set 5
    f32.demote_f64
    local.get 5
    f32.add
    f64.promote_f32
    local.set 16
    i32.const 0
    i32.add
    local.get 16
    f32.demote_f64
    f32.store align=1
    local.get 3
    i32.const 0
    i32.const 16
    memory.fill
    local.get 3
    i32.const 1
    f32.convert_i32_s
    f64.promote_f32
    local.set 16
    i32.const 0
    i32.add
    local.get 16
    f32.demote_f64
    f32.store align=1
    local.get 3
    f32.const 0x1.8p-2 (;=0.375;)
    f64.promote_f32
    f32.demote_f64
    f64.promote_f32
    local.set 16
    i32.const 4
    i32.add
    local.get 16
    f32.demote_f64
    f32.store align=1
    local.get 3
    f32.const 0x1p-2 (;=0.25;)
    f64.promote_f32
    f32.demote_f64
    f64.promote_f32
    local.set 16
    i32.const 8
    i32.add
    local.get 16
    f32.demote_f64
    f32.store align=1
    local.get 3
    i32.const 1
    f32.convert_i32_s
    f64.promote_f32
    local.set 16
    i32.const 12
    i32.add
    local.get 16
    f32.demote_f64
    f32.store align=1
    local.get 2
    local.get 3
    call 5
    local.get 4
    i32.const 0
    i32.const 16
    memory.fill
    local.get 4
    i32.const 1
    f32.convert_i32_s
    f64.promote_f32
    local.set 16
    i32.const 0
    i32.add
    local.get 16
    f32.demote_f64
    f32.store align=1
    local.get 4
    i32.const 1
    f32.convert_i32_s
    f64.promote_f32
    local.set 16
    i32.const 4
    i32.add
    local.get 16
    f32.demote_f64
    f32.store align=1
    local.get 4
    i32.const 1
    f32.convert_i32_s
    f64.promote_f32
    local.set 16
    i32.const 8
    i32.add
    local.get 16
    f32.demote_f64
    f32.store align=1
    local.get 4
    i32.const 1
    f32.convert_i32_s
    f64.promote_f32
    local.set 16
    i32.const 12
    i32.add
    local.get 16
    f32.demote_f64
    f32.store align=1
    global.get 0
    ref.is_null
    if ;; label = @1
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
    local.set 11
    f32.demote_f64
    local.set 10
    f32.demote_f64
    local.set 9
    f32.demote_f64
    local.set 8
    local.set 7
    local.get 7
    local.get 8
    local.get 9
    local.get 10
    local.get 11
    call 7
    i32.const 0
    local.get 12
    global.set 1
    return
  )
  (func (;5;) (type 6) (param i32 i32)
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
  (func (;6;) (type 7) (param i32) (result f32)
    (local f32 i32 i32 i32 i64 f64)
    global.get 1
    local.tee 2
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 2
    i32.const 0
    i32.add
    global.set 1
    local.get 0
    i32.const 0
    i32.add
    f32.load align=1
    f64.promote_f32
    local.get 0
    i32.const 8
    i32.add
    f32.load align=1
    f64.promote_f32
    f32.demote_f64
    local.set 1
    f32.demote_f64
    local.get 1
    f32.add
    f64.promote_f32
    f32.demote_f64
    local.get 2
    global.set 1
    return
  )
  (func (;7;) (type 8) (param externref f32 f32 f32 i32)
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
  (data (;0;) "H\00e\00l\00l\00o\00 \00f\00r\00o\00m\00 \00X\00u\00i\00 \00W\00A\00S\00M\00")
)
```
