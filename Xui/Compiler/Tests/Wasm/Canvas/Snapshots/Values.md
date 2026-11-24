# Values: Xui geometry → WASM → Canvas

[Open standalone demo](Values.html)

Source: [Values.cs](Fixtures/Canvas/Values.cs), [Host.cs](Fixtures/Canvas/Host.cs)

## Source

```csharp
using System.Runtime.InteropServices;
using CanvasDemo;
using Xui.Core.Canvas;
using Xui.Core.Math2D;

public interface ILayout { Rect Arrange(Rect bounds); }
public class InsetLayout : ILayout
{
    public Rect Arrange(Rect bounds) => bounds.Inset((NFloat)20);
}
public static class Program
{
    static void Paint(Rect bounds, Color color) => Draw.Fill(ref bounds, ref color);

    public static int Main()
    {
        NativeCanvas.Clear(0.0625, 0.125, 0.25, 1);
        Point origin = (0, 0);
        Vector shift = (40, 100);
        origin += shift;
        Rect outer = (origin.X, origin.Y, 520, 180);
        ILayout layout = new InsetLayout();
        Rect inner = layout.Arrange(outer);
        Color blue = new Color((NFloat)0.125, (NFloat)0.5, (NFloat)1, (NFloat)1);
        Color coral = new Color((NFloat)1, (NFloat)0.375, (NFloat)0.25, (NFloat)1);
        Paint(outer, blue);
        Paint(inner, coral);
        Color white = new Color((NFloat)1, (NFloat)1, (NFloat)1, (NFloat)1);
        Draw.Text("Rect values through interface calls", 40, 60, 26, ref white);
        return inner.Width == (NFloat)480 ? 42 : 1;
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
    0.125,
    0.5,
    1,
    1
  ],
  [
    "FillRect",
    60,
    120,
    480,
    140,
    1,
    0.375,
    0.25,
    1
  ],
  [
    "FillText",
    "Rect values through interface calls",
    40,
    60,
    26,
    1,
    1,
    1,
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
- `System.Private.CoreLib.dll`: `System.ValueTuple<System.Runtime.InteropServices.NFloat, System.Runtime.InteropServices.NFloat>..ctor`
- `Xui.Core.dll`: `Xui.Core.Math2D.Point.op_Implicit`
- `Xui.Core.dll`: `Xui.Core.Math2D.Vector.op_Implicit`
- `Xui.Core.dll`: `Xui.Core.Math2D.Point.op_Addition`
- `System.Private.CoreLib.dll`: `System.ValueTuple<System.Runtime.InteropServices.NFloat, System.Runtime.InteropServices.NFloat, System.Runtime.InteropServices.NFloat, System.Runtime.InteropServices.NFloat>..ctor`
- `Xui.Core.dll`: `Xui.Core.Math2D.Rect.op_Implicit`
- `CanvasDemo.exe`: `InsetLayout..ctor`
- `Xui.Core.dll`: `Xui.Core.Canvas.Color..ctor`
- `CanvasDemo.exe`: `Program.Paint`
- `CanvasDemo.exe`: `CanvasDemo.Draw.Text`
- `Xui.Core.dll`: `Xui.Core.Math2D.Point..ctor`
- `Xui.Core.dll`: `Xui.Core.Math2D.Vector..ctor`
- `Xui.Core.dll`: `Xui.Core.Math2D.Rect..ctor`
- `CanvasDemo.exe`: `ILayout.Arrange`
- `CanvasDemo.exe`: `InsetLayout.Arrange`
- `CanvasDemo.exe`: `CanvasDemo.Draw.Fill`
- `Xui.Core.dll`: `Xui.Core.Math2D.Rect.Inset`

## WAT

```wat
(module
  (type (;0;) (array (mut i16)))
  (type (;1;) (func (param (ref null 0) i32 i32) (result (ref extern))))
  (type (;2;) (func (param f64 f64 f64 f64)))
  (type (;3;) (func (param externref f64 f64 f64 f64 f64 f64 f64)))
  (type (;4;) (func (param f64 f64 f64 f64 f64 f64 f64 f64)))
  (type (;5;) (struct (field i32) (field externref)))
  (type (;6;) (func (param eqref)))
  (type (;7;) (func (result eqref)))
  (type (;8;) (func (result eqref exnref)))
  (type (;9;) (func (result exnref)))
  (type (;10;) (func))
  (type (;11;) (struct (field i32) (field eqref) (field i32) (field (ref null 0)) (field i32) (field i32)))
  (type (;12;) (sub (struct (field (mut eqref)))))
  (type (;13;) (func (param eqref eqref) (result eqref)))
  (type (;14;) (sub 12 (struct (field (mut eqref)))))
  (type (;15;) (func (result i32)))
  (type (;16;) (func))
  (type (;17;) (func (param i32 f32 f32)))
  (type (;18;) (func (param i32 i32) (result i32)))
  (type (;19;) (func (param i32 i32) (result i32)))
  (type (;20;) (func (param i32 i32 i32) (result i32)))
  (type (;21;) (func (param i32 f32 f32 f32 f32)))
  (type (;22;) (func (param i32 i32) (result i32)))
  (type (;23;) (func (param eqref)))
  (type (;24;) (func (param i32 f32 f32 f32 f32)))
  (type (;25;) (func (param i32 i32)))
  (type (;26;) (func (param externref f32 f32 f32 i32)))
  (type (;27;) (func (param i32 f32 f32)))
  (type (;28;) (func (param i32 f32 f32)))
  (type (;29;) (func (param i32 f32 f32 f32 f32)))
  (type (;30;) (func (param eqref i32 i32) (result i32)))
  (type (;31;) (func (param eqref i32 i32) (result i32)))
  (type (;32;) (func (param i32 i32)))
  (type (;33;) (func (param i32 f32 i32) (result i32)))
  (import "canvas" "Clear" (func (;0;) (type 2) (param f64 f64 f64 f64)))
  (import "canvas" "FillText" (func (;1;) (type 3) (param externref f64 f64 f64 f64 f64 f64 f64)))
  (import "canvas" "FillRect" (func (;2;) (type 4) (param f64 f64 f64 f64 f64 f64 f64 f64)))
  (import "wasm:js-string" "fromCharCodeArray" (func (;3;) (type 1) (param (ref null 0) i32 i32) (result (ref extern))))
  (table (;0;) 2 2 funcref)
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
  (global (;8;) eqref i32.const 1 ref.null eq i32.const 0 i32.const 73 i32.const 110 i32.const 115 i32.const 101 i32.const 116 i32.const 76 i32.const 97 i32.const 121 i32.const 111 i32.const 117 i32.const 116 array.new_fixed 0 11 i32.const 1 i32.const 1 struct.new 11)
  (global (;9;) eqref i32.const 2 ref.null eq i32.const 0 i32.const 73 i32.const 76 i32.const 97 i32.const 121 i32.const 111 i32.const 117 i32.const 116 array.new_fixed 0 7 i32.const 0 i32.const 0 struct.new 11)
  (export "main" (func 4))
  (elem (;0;) (i32.const 1) func 18)
  (elem (;1;) (i32.const 0) func 18)
  (func (;4;) (type 15) (result i32)
    (local i32 i32 i32 i32 i32 i32 f32 i32 i64 f64 externref i32 f32 f32 i32 f32 f32 f32 f32 eqref eqref i32 i32 f32 f32 f32 f32 externref f32 f32 f32 i32 exnref i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 7
    local.tee 33
    i32.const 1048288
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 33
    i32.const 288
    i32.add
    global.set 7
    local.get 33
    i32.const 0
    i32.add
    local.set 0
    local.get 0
    i32.const 0
    i32.const 8
    memory.fill
    local.get 33
    i32.const 8
    i32.add
    local.set 1
    local.get 1
    i32.const 0
    i32.const 8
    memory.fill
    local.get 33
    i32.const 16
    i32.add
    local.set 2
    local.get 2
    i32.const 0
    i32.const 16
    memory.fill
    local.get 33
    i32.const 32
    i32.add
    local.set 3
    local.get 3
    i32.const 0
    i32.const 16
    memory.fill
    local.get 33
    i32.const 48
    i32.add
    local.set 4
    local.get 4
    i32.const 0
    i32.const 16
    memory.fill
    local.get 33
    i32.const 64
    i32.add
    local.set 5
    local.get 5
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
        local.get 33
        i32.const 80
        i32.add
        local.tee 11
        i32.const 0
        i32.const 8
        memory.fill
        local.get 11
        local.get 12
        local.get 13
        call 5
        local.get 11
        local.get 33
        i32.const 88
        i32.add
        call 6
        local.set 34
        local.get 0
        local.get 34
        i32.const 8
        memory.copy
        i32.const 40
        f32.convert_i32_s
        f64.promote_f32
        i32.const 100
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 13
        f32.demote_f64
        local.set 12
        local.get 33
        i32.const 96
        i32.add
        local.tee 11
        i32.const 0
        i32.const 8
        memory.fill
        local.get 11
        local.get 12
        local.get 13
        call 5
        local.get 11
        local.get 33
        i32.const 104
        i32.add
        call 7
        local.set 34
        local.get 1
        local.get 34
        i32.const 8
        memory.copy
        local.get 0
        local.set 34
        local.get 33
        i32.const 112
        i32.add
        local.get 34
        i32.const 8
        memory.copy
        local.get 33
        i32.const 112
        i32.add
        local.get 1
        local.set 34
        local.get 33
        i32.const 120
        i32.add
        local.get 34
        i32.const 8
        memory.copy
        local.get 33
        i32.const 120
        i32.add
        local.get 33
        i32.const 128
        i32.add
        call 8
        local.set 34
        local.get 0
        local.get 34
        i32.const 8
        memory.copy
        local.get 0
        local.set 34
        local.get 33
        i32.const 136
        i32.add
        local.get 34
        i32.const 8
        memory.copy
        local.get 33
        i32.const 136
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 0
        local.set 34
        local.get 33
        i32.const 144
        i32.add
        local.get 34
        i32.const 8
        memory.copy
        local.get 33
        i32.const 144
        i32.add
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        i32.const 520
        f32.convert_i32_s
        f64.promote_f32
        i32.const 180
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 18
        f32.demote_f64
        local.set 17
        f32.demote_f64
        local.set 16
        f32.demote_f64
        local.set 15
        local.get 33
        i32.const 152
        i32.add
        local.tee 14
        i32.const 0
        i32.const 16
        memory.fill
        local.get 14
        local.get 15
        local.get 16
        local.get 17
        local.get 18
        call 9
        local.get 14
        local.get 33
        i32.const 168
        i32.add
        call 10
        local.set 34
        local.get 2
        local.get 34
        i32.const 16
        memory.copy
        struct.new_default 14
        local.set 19
        local.get 19
        ref.cast (ref 14)
        global.get 8
        struct.set 14 0
        local.get 19
        call 11
        local.get 19
        local.get 2
        local.set 34
        local.get 33
        i32.const 184
        i32.add
        local.get 34
        i32.const 16
        memory.copy
        local.get 33
        i32.const 184
        i32.add
        local.set 21
        local.set 20
        local.get 20
        local.tee 39
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
        local.get 39
        ref.cast (ref 12)
        local.get 21
        local.get 33
        i32.const 200
        i32.add
        local.get 20
        ref.cast (ref 12)
        struct.get 12 0
        ref.cast (ref 11)
        struct.get 11 2
        i32.const 0
        i32.add
        call_indirect (type 30)
        local.get 3
        f64.const 0x1p-3 (;=0.125;)
        f32.demote_f64
        f64.promote_f32
        f64.const 0x1p-1 (;=0.5;)
        f32.demote_f64
        f64.promote_f32
        i32.const 1
        f32.convert_i32_s
        f64.promote_f32
        i32.const 1
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 26
        f32.demote_f64
        local.set 25
        f32.demote_f64
        local.set 24
        f32.demote_f64
        local.set 23
        local.set 22
        local.get 22
        local.get 23
        local.get 24
        local.get 25
        local.get 26
        call 12
        local.get 4
        i32.const 1
        f32.convert_i32_s
        f64.promote_f32
        f64.const 0x1.8p-2 (;=0.375;)
        f32.demote_f64
        f64.promote_f32
        f64.const 0x1p-2 (;=0.25;)
        f32.demote_f64
        f64.promote_f32
        i32.const 1
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 26
        f32.demote_f64
        local.set 25
        f32.demote_f64
        local.set 24
        f32.demote_f64
        local.set 23
        local.set 22
        local.get 22
        local.get 23
        local.get 24
        local.get 25
        local.get 26
        call 12
        local.get 2
        local.set 34
        local.get 33
        i32.const 216
        i32.add
        local.get 34
        i32.const 16
        memory.copy
        local.get 33
        i32.const 216
        i32.add
        local.get 3
        local.set 34
        local.get 33
        i32.const 232
        i32.add
        local.get 34
        i32.const 16
        memory.copy
        local.get 33
        i32.const 232
        i32.add
        call 13
        local.tee 35
        local.get 35
        local.set 34
        local.get 33
        i32.const 248
        i32.add
        local.get 34
        i32.const 16
        memory.copy
        local.get 33
        i32.const 248
        i32.add
        local.get 4
        local.set 34
        local.get 33
        i32.const 264
        i32.add
        local.get 34
        i32.const 16
        memory.copy
        local.get 33
        i32.const 264
        i32.add
        call 13
        local.get 5
        i32.const 1
        f32.convert_i32_s
        f64.promote_f32
        i32.const 1
        f32.convert_i32_s
        f64.promote_f32
        i32.const 1
        f32.convert_i32_s
        f64.promote_f32
        i32.const 1
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 26
        f32.demote_f64
        local.set 25
        f32.demote_f64
        local.set 24
        f32.demote_f64
        local.set 23
        local.set 22
        local.get 22
        local.get 23
        local.get 24
        local.get 25
        local.get 26
        call 12
        global.get 0
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 35
          array.new_data 0 0
          i32.const 0
          i32.const 35
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
        i32.const 26
        f32.convert_i32_s
        f64.promote_f32
        local.get 5
        local.set 31
        f32.demote_f64
        local.set 30
        f32.demote_f64
        local.set 29
        f32.demote_f64
        local.set 28
        local.set 27
        local.get 27
        local.get 28
        local.get 29
        local.get 30
        local.get 31
        call 14
        i32.const 8
        i32.add
        f32.load align=1
        f64.promote_f32
        i32.const 480
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 6
        f32.demote_f64
        local.get 6
        f32.eq
        if (type 16) ;; label = @3
          i32.const 42
          local.get 33
          global.set 7
          return
        else
          i32.const 1
          local.get 33
          global.set 7
          return
        end
        unreachable
        unreachable
      end
      unreachable
    end
    local.set 32
    local.get 33
    global.set 7
    local.get 32
    throw_ref
  )
  (func (;5;) (type 17) (param i32 f32 f32)
    (local exnref i32 i32 i32 i64 f64)
    global.get 7
    local.tee 4
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 4
    i32.const 0
    i32.add
    global.set 7
    block (type 9) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        f64.promote_f32
        local.set 8
        i32.const 0
        i32.add
        local.get 8
        f32.demote_f64
        f32.store align=1
        local.get 0
        local.get 2
        f64.promote_f32
        local.set 8
        i32.const 4
        i32.add
        local.get 8
        f32.demote_f64
        f32.store align=1
        local.get 4
        global.set 7
        return
        unreachable
      end
      unreachable
    end
    local.set 3
    local.get 4
    global.set 7
    local.get 3
    throw_ref
  )
  (func (;6;) (type 18) (param i32 i32) (result i32)
    (local i32 f32 f32 exnref i32 i32 i32 i64 f64)
    global.get 7
    local.tee 6
    i32.const 1048544
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 6
    i32.const 32
    i32.add
    global.set 7
    local.get 6
    i32.const 0
    i32.add
    local.get 0
    i32.const 8
    memory.copy
    local.get 6
    i32.const 0
    i32.add
    local.set 0
    block (type 9) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.set 7
        local.get 6
        i32.const 8
        i32.add
        local.get 7
        i32.const 8
        memory.copy
        local.get 6
        i32.const 8
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 0
        local.set 7
        local.get 6
        i32.const 16
        i32.add
        local.get 7
        i32.const 8
        memory.copy
        local.get 6
        i32.const 16
        i32.add
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 4
        f32.demote_f64
        local.set 3
        local.get 6
        i32.const 24
        i32.add
        local.tee 2
        i32.const 0
        i32.const 8
        memory.fill
        local.get 2
        local.get 3
        local.get 4
        call 15
        local.get 2
        local.set 7
        local.get 1
        local.get 7
        i32.const 8
        memory.copy
        local.get 1
        local.get 6
        global.set 7
        return
        unreachable
      end
      unreachable
    end
    local.set 5
    local.get 6
    global.set 7
    local.get 5
    throw_ref
  )
  (func (;7;) (type 19) (param i32 i32) (result i32)
    (local i32 f32 f32 exnref i32 i32 i32 i64 f64)
    global.get 7
    local.tee 6
    i32.const 1048544
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 6
    i32.const 32
    i32.add
    global.set 7
    local.get 6
    i32.const 0
    i32.add
    local.get 0
    i32.const 8
    memory.copy
    local.get 6
    i32.const 0
    i32.add
    local.set 0
    block (type 9) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.set 7
        local.get 6
        i32.const 8
        i32.add
        local.get 7
        i32.const 8
        memory.copy
        local.get 6
        i32.const 8
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 0
        local.set 7
        local.get 6
        i32.const 16
        i32.add
        local.get 7
        i32.const 8
        memory.copy
        local.get 6
        i32.const 16
        i32.add
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 4
        f32.demote_f64
        local.set 3
        local.get 6
        i32.const 24
        i32.add
        local.tee 2
        i32.const 0
        i32.const 8
        memory.fill
        local.get 2
        local.get 3
        local.get 4
        call 16
        local.get 2
        local.set 7
        local.get 1
        local.get 7
        i32.const 8
        memory.copy
        local.get 1
        local.get 6
        global.set 7
        return
        unreachable
      end
      unreachable
    end
    local.set 5
    local.get 6
    global.set 7
    local.get 5
    throw_ref
  )
  (func (;8;) (type 20) (param i32 i32 i32) (result i32)
    (local f32 i32 f32 f32 exnref i32 i32 i32 i64 f64)
    global.get 7
    local.tee 8
    i32.const 1048512
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 8
    i32.const 64
    i32.add
    global.set 7
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
    block (type 9) (result exnref) ;; label = @1
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
        local.set 3
        f32.demote_f64
        local.get 3
        f32.add
        f64.promote_f32
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
        local.set 3
        f32.demote_f64
        local.get 3
        f32.add
        f64.promote_f32
        f32.demote_f64
        local.set 6
        f32.demote_f64
        local.set 5
        local.get 8
        i32.const 48
        i32.add
        local.tee 4
        i32.const 0
        i32.const 8
        memory.fill
        local.get 4
        local.get 5
        local.get 6
        call 15
        local.get 4
        local.set 9
        local.get 2
        local.get 9
        i32.const 8
        memory.copy
        local.get 2
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
  (func (;9;) (type 21) (param i32 f32 f32 f32 f32)
    (local exnref i32 i32 i32 i64 f64)
    global.get 7
    local.tee 6
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 6
    i32.const 0
    i32.add
    global.set 7
    block (type 9) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        f64.promote_f32
        local.set 10
        i32.const 0
        i32.add
        local.get 10
        f32.demote_f64
        f32.store align=1
        local.get 0
        local.get 2
        f64.promote_f32
        local.set 10
        i32.const 4
        i32.add
        local.get 10
        f32.demote_f64
        f32.store align=1
        local.get 0
        local.get 3
        f64.promote_f32
        local.set 10
        i32.const 8
        i32.add
        local.get 10
        f32.demote_f64
        f32.store align=1
        local.get 0
        local.get 4
        f64.promote_f32
        local.set 10
        i32.const 12
        i32.add
        local.get 10
        f32.demote_f64
        f32.store align=1
        local.get 6
        global.set 7
        return
        unreachable
      end
      unreachable
    end
    local.set 5
    local.get 6
    global.set 7
    local.get 5
    throw_ref
  )
  (func (;10;) (type 22) (param i32 i32) (result i32)
    (local i32 f32 f32 f32 f32 exnref i32 i32 i32 i64 f64)
    global.get 7
    local.tee 8
    i32.const 1048480
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 8
    i32.const 96
    i32.add
    global.set 7
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
    block (type 9) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
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
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 0
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
        i32.const 12
        i32.add
        f32.load align=1
        f64.promote_f32
        f32.demote_f64
        local.set 6
        f32.demote_f64
        local.set 5
        f32.demote_f64
        local.set 4
        f32.demote_f64
        local.set 3
        local.get 8
        i32.const 80
        i32.add
        local.tee 2
        i32.const 0
        i32.const 16
        memory.fill
        local.get 2
        local.get 3
        local.get 4
        local.get 5
        local.get 6
        call 17
        local.get 2
        local.set 9
        local.get 1
        local.get 9
        i32.const 16
        memory.copy
        local.get 1
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
  (func (;11;) (type 23) (param eqref)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    drop
    return
  )
  (func (;12;) (type 24) (param i32 f32 f32 f32 f32)
    (local exnref i32 i32 i32 i64 f64)
    global.get 7
    local.tee 6
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 6
    i32.const 0
    i32.add
    global.set 7
    block (type 9) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        f64.promote_f32
        local.set 10
        i32.const 0
        i32.add
        local.get 10
        f32.demote_f64
        f32.store align=1
        local.get 0
        local.get 2
        f64.promote_f32
        local.set 10
        i32.const 4
        i32.add
        local.get 10
        f32.demote_f64
        f32.store align=1
        local.get 0
        local.get 3
        f64.promote_f32
        local.set 10
        i32.const 8
        i32.add
        local.get 10
        f32.demote_f64
        f32.store align=1
        local.get 0
        local.get 4
        f64.promote_f32
        local.set 10
        i32.const 12
        i32.add
        local.get 10
        f32.demote_f64
        f32.store align=1
        local.get 6
        global.set 7
        return
        unreachable
      end
      unreachable
    end
    local.set 5
    local.get 6
    global.set 7
    local.get 5
    throw_ref
  )
  (func (;13;) (type 25) (param i32 i32)
    (local exnref i32 i32 i32 i64 f64)
    global.get 7
    local.tee 3
    i32.const 1048544
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 3
    i32.const 32
    i32.add
    global.set 7
    local.get 3
    i32.const 0
    i32.add
    local.get 0
    i32.const 16
    memory.copy
    local.get 3
    i32.const 0
    i32.add
    local.set 0
    local.get 3
    i32.const 16
    i32.add
    local.get 1
    i32.const 16
    memory.copy
    local.get 3
    i32.const 16
    i32.add
    local.set 1
    block (type 9) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        call 19
        local.get 3
        global.set 7
        return
        unreachable
      end
      unreachable
    end
    local.set 2
    local.get 3
    global.set 7
    local.get 2
    throw_ref
  )
  (func (;14;) (type 26) (param externref f32 f32 f32 i32)
    (local f32 exnref i32 i32 i32 i64 f64)
    global.get 7
    local.tee 7
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 7
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
        call 1
        local.get 7
        global.set 7
        return
        unreachable
      end
      unreachable
    end
    local.set 6
    local.get 7
    global.set 7
    local.get 6
    throw_ref
  )
  (func (;15;) (type 27) (param i32 f32 f32)
    (local exnref i32 i32 i32 i64 f64)
    global.get 7
    local.tee 4
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 4
    i32.const 0
    i32.add
    global.set 7
    block (type 9) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        f64.promote_f32
        local.set 8
        i32.const 0
        i32.add
        local.get 8
        f32.demote_f64
        f32.store align=1
        local.get 0
        local.get 2
        f64.promote_f32
        local.set 8
        i32.const 4
        i32.add
        local.get 8
        f32.demote_f64
        f32.store align=1
        local.get 4
        global.set 7
        return
        unreachable
      end
      unreachable
    end
    local.set 3
    local.get 4
    global.set 7
    local.get 3
    throw_ref
  )
  (func (;16;) (type 28) (param i32 f32 f32)
    (local exnref i32 i32 i32 i64 f64)
    global.get 7
    local.tee 4
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 4
    i32.const 0
    i32.add
    global.set 7
    block (type 9) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        f64.promote_f32
        local.set 8
        i32.const 0
        i32.add
        local.get 8
        f32.demote_f64
        f32.store align=1
        local.get 0
        local.get 2
        f64.promote_f32
        local.set 8
        i32.const 4
        i32.add
        local.get 8
        f32.demote_f64
        f32.store align=1
        local.get 4
        global.set 7
        return
        unreachable
      end
      unreachable
    end
    local.set 3
    local.get 4
    global.set 7
    local.get 3
    throw_ref
  )
  (func (;17;) (type 29) (param i32 f32 f32 f32 f32)
    (local exnref i32 i32 i32 i64 f64)
    global.get 7
    local.tee 6
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 6
    i32.const 0
    i32.add
    global.set 7
    block (type 9) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        f64.promote_f32
        local.set 10
        i32.const 0
        i32.add
        local.get 10
        f32.demote_f64
        f32.store align=1
        local.get 0
        local.get 2
        f64.promote_f32
        local.set 10
        i32.const 4
        i32.add
        local.get 10
        f32.demote_f64
        f32.store align=1
        local.get 0
        local.get 3
        f64.promote_f32
        local.set 10
        i32.const 8
        i32.add
        local.get 10
        f32.demote_f64
        f32.store align=1
        local.get 0
        local.get 4
        f64.promote_f32
        local.set 10
        i32.const 12
        i32.add
        local.get 10
        f32.demote_f64
        f32.store align=1
        local.get 6
        global.set 7
        return
        unreachable
      end
      unreachable
    end
    local.set 5
    local.get 6
    global.set 7
    local.get 5
    throw_ref
  )
  (func (;18;) (type 31) (param eqref i32 i32) (result i32)
    (local f32 i32 f32 exnref i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 7
    local.tee 7
    i32.const 1048544
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 7
    i32.const 32
    i32.add
    global.set 7
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
    block (type 9) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 1
        i32.const 20
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 5
        local.set 4
        local.get 4
        local.get 5
        local.get 7
        i32.const 16
        i32.add
        call 20
        local.set 8
        local.get 2
        local.get 8
        i32.const 16
        memory.copy
        local.get 2
        local.get 7
        global.set 7
        return
        unreachable
      end
      unreachable
    end
    local.set 6
    local.get 7
    global.set 7
    local.get 6
    throw_ref
  )
  (func (;19;) (type 32) (param i32 i32)
    (local f32 exnref i32 i32 i32 i64 f64)
    global.get 7
    local.tee 4
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 4
    i32.const 0
    i32.add
    global.set 7
    block (type 9) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
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
        call 2
        local.get 4
        global.set 7
        return
        unreachable
      end
      unreachable
    end
    local.set 3
    local.get 4
    global.set 7
    local.get 3
    throw_ref
  )
  (func (;20;) (type 33) (param i32 f32 i32) (result i32)
    (local f32 i32 f32 f32 f32 f32 exnref i32 i32 i32 i64 f64)
    global.get 7
    local.tee 10
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 10
    i32.const 16
    i32.add
    global.set 7
    block (type 9) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        f64.promote_f32
        f32.demote_f64
        local.set 3
        f32.demote_f64
        local.get 3
        f32.add
        f64.promote_f32
        local.get 0
        i32.const 4
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        f64.promote_f32
        f32.demote_f64
        local.set 3
        f32.demote_f64
        local.get 3
        f32.add
        f64.promote_f32
        local.get 0
        i32.const 8
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        f64.promote_f32
        f32.demote_f64
        local.set 3
        f32.demote_f64
        local.get 3
        f32.sub
        f64.promote_f32
        local.get 1
        f64.promote_f32
        f32.demote_f64
        local.set 3
        f32.demote_f64
        local.get 3
        f32.sub
        f64.promote_f32
        local.get 0
        i32.const 12
        i32.add
        f32.load align=1
        f64.promote_f32
        local.get 1
        f64.promote_f32
        f32.demote_f64
        local.set 3
        f32.demote_f64
        local.get 3
        f32.sub
        f64.promote_f32
        local.get 1
        f64.promote_f32
        f32.demote_f64
        local.set 3
        f32.demote_f64
        local.get 3
        f32.sub
        f64.promote_f32
        f32.demote_f64
        local.set 8
        f32.demote_f64
        local.set 7
        f32.demote_f64
        local.set 6
        f32.demote_f64
        local.set 5
        local.get 10
        i32.const 0
        i32.add
        local.tee 4
        i32.const 0
        i32.const 16
        memory.fill
        local.get 4
        local.get 5
        local.get 6
        local.get 7
        local.get 8
        call 17
        local.get 4
        local.set 11
        local.get 2
        local.get 11
        i32.const 16
        memory.copy
        local.get 2
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
  (func (;21;) (type 13) (param eqref eqref) (result eqref)
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
      global.get 8
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
  (data (;0;) "R\00e\00c\00t\00 \00v\00a\00l\00u\00e\00s\00 \00t\00h\00r\00o\00u\00g\00h\00 \00i\00n\00t\00e\00r\00f\00a\00c\00e\00 \00c\00a\00l\00l\00s\00")
  (data (;1;) "E\00x\00c\00e\00p\00t\00i\00o\00n\00 \00o\00f\00 \00t\00y\00p\00e\00 \00'\00S\00y\00s\00t\00e\00m\00.\00E\00x\00c\00e\00p\00t\00i\00o\00n\00'\00 \00w\00a\00s\00 \00t\00h\00r\00o\00w\00n\00.\00")
  (data (;2;) "S\00y\00s\00t\00e\00m\00 \00e\00r\00r\00o\00r\00.\00")
  (data (;3;) "S\00p\00e\00c\00i\00f\00i\00e\00d\00 \00c\00a\00s\00t\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00.\00")
  (data (;4;) "O\00b\00j\00e\00c\00t\00 \00r\00e\00f\00e\00r\00e\00n\00c\00e\00 \00n\00o\00t\00 \00s\00e\00t\00 \00t\00o\00 \00a\00n\00 \00i\00n\00s\00t\00a\00n\00c\00e\00 \00o\00f\00 \00a\00n\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;5;) "O\00p\00e\00r\00a\00t\00i\00o\00n\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00 \00d\00u\00e\00 \00t\00o\00 \00t\00h\00e\00 \00c\00u\00r\00r\00e\00n\00t\00 \00s\00t\00a\00t\00e\00 \00o\00f\00 \00t\00h\00e\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;6;) "V\00a\00l\00u\00e\00 \00d\00o\00e\00s\00 \00n\00o\00t\00 \00f\00a\00l\00l\00 \00w\00i\00t\00h\00i\00n\00 \00t\00h\00e\00 \00e\00x\00p\00e\00c\00t\00e\00d\00 \00r\00a\00n\00g\00e\00.\00")
)
```
