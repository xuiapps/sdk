# Xui gradient stops in inline-array storage

Source: [Gradients.cs](Fixtures/InlineArrays/Gradients.cs)

## Source

```csharp
using System;
using Xui.Core.Canvas;
using Xui.Core.Math2D;

public static class Program
{
    private static int Inspect(LinearGradient gradient)
    {
        Console.WriteLine((double)gradient.StartPoint.X);
        Console.WriteLine((double)gradient.EndPoint.X);
        for (int i = 0; i < gradient.GradientStops.Length; i++)
        {
            ref readonly GradientStop stop = ref gradient.GradientStops[i];
            Console.WriteLine((double)stop.Offset);
            Console.WriteLine((double)stop.Color.Red);
            Console.WriteLine((double)stop.Color.Blue);
        }
        return gradient.GradientStops.Length;
    }

    public static int Main()
    {
        ReadOnlySpan<GradientStop> stops = [(0, 0xff0000ffu), (1, 0x0000ffffu)];
        Point start = (0, 0);
        Point end = (100, 0);
        return Inspect(new LinearGradient(start, end, stops));
    }
}
```

## Output

```text
0
100
0
1
0
1
0
1
```

## Exit Code

```text
2
```

## WAT

```wat
(module
  (type (;0;) (array (mut i16)))
  (type (;1;) (func (param (ref null 0) i32 i32) (result (ref extern))))
  (type (;2;) (func (param f64)))
  (type (;3;) (struct (field i32) (field externref)))
  (type (;4;) (func (param eqref)))
  (type (;5;) (func (result eqref)))
  (type (;6;) (func (result eqref exnref)))
  (type (;7;) (func (result exnref)))
  (type (;8;) (func))
  (type (;9;) (struct (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32))))
  (type (;10;) (struct (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32))))
  (type (;11;) (struct (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32))))
  (type (;12;) (struct (field (mut eqref)) (field (mut i32)) (field (mut i32))))
  (type (;13;) (struct (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut f32)) (field (mut eqref)) (field (mut i32)) (field (mut i32))))
  (type (;14;) (struct (field (mut f32)) (field (mut f32))))
  (type (;15;) (struct (field (mut f32)) (field (mut i32))))
  (type (;16;) (struct (field (mut f32)) (field (mut f32))))
  (type (;17;) (func (result i32)))
  (type (;18;) (func (param eqref i32) (result eqref)))
  (type (;19;) (func (param eqref i32 eqref)))
  (type (;20;) (func (param eqref i32) (result externref)))
  (type (;21;) (func (param eqref i32 externref)))
  (type (;22;) (func (param eqref i32) (result f32)))
  (type (;23;) (func (param eqref i32 f32)))
  (type (;24;) (func (param eqref i32) (result i32)))
  (type (;25;) (func (param eqref i32 i32)))
  (type (;26;) (func (param eqref i32) (result i32)))
  (type (;27;) (func (param eqref i32 i32)))
  (type (;28;) (func (result i32)))
  (type (;29;) (func (param eqref f32 i32 i32)))
  (type (;30;) (func (param i32 i32) (result i32)))
  (type (;31;) (func (param eqref f32 f32 i32)))
  (type (;32;) (func (param i32 i32) (result i32)))
  (type (;33;) (func (param eqref i32 i32 i32 i32)))
  (type (;34;) (func (param i32) (result i32)))
  (type (;35;) (func))
  (type (;36;) (func))
  (type (;37;) (func (param eqref f32 i32 i32)))
  (type (;38;) (func (param eqref f32 f32 i32)))
  (type (;39;) (func (param eqref i32 i32)))
  (import "xui" "console-log" (func (;0;) (type 2) (param f64)))
  (import "wasm:js-string" "fromCharCodeArray" (func (;1;) (type 1) (param (ref null 0) i32 i32) (result (ref extern))))
  (table (;0;) 65536 65536 eqref)
  (table (;1;) 65536 65536 externref)
  (memory (;0;) 16 32768)
  (tag (;0;) (type 4) (param eqref))
  (global (;0;) (mut externref) ref.null extern)
  (global (;1;) (mut externref) ref.null extern)
  (global (;2;) (mut externref) ref.null extern)
  (global (;3;) (mut externref) ref.null extern)
  (global (;4;) (mut externref) ref.null extern)
  (global (;5;) (mut externref) ref.null extern)
  (global (;6;) (mut externref) ref.null extern)
  (global (;7;) (mut i32) i32.const 16)
  (global (;8;) (mut i32) i32.const 0)
  (export "main" (func 2))
  (func (;2;) (type 28) (result i32)
    (local i32 i32 i32 eqref i32 eqref i32 f32 eqref f32 i32 i32 eqref f32 f32 i32 eqref i32 i32 i32 i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 7
    local.tee 26
    i32.const 1048368
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 26
    i32.const 208
    i32.add
    global.set 7
    local.get 26
    i32.const 0
    i32.add
    local.set 0
    local.get 0
    i32.const 0
    i32.const 12
    memory.fill
    local.get 26
    i32.const 12
    i32.add
    local.set 1
    local.get 1
    i32.const 0
    i32.const 8
    memory.fill
    local.get 26
    i32.const 20
    i32.add
    local.set 2
    local.get 2
    i32.const 0
    i32.const 40
    memory.fill
    global.get 8
    local.tee 23
    i32.const 65532
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 23
    i32.const 4
    i32.add
    global.set 8
    local.get 26
    i32.const 0
    i32.add
    local.get 23
    i32.const 0
    i32.add
    i32.store align=1
    local.get 26
    i32.const 116
    i32.add
    local.get 23
    i32.const 1
    i32.add
    i32.store align=1
    local.get 26
    i32.const 168
    i32.add
    local.get 23
    i32.const 2
    i32.add
    i32.store align=1
    local.get 26
    i32.const 196
    i32.add
    local.get 23
    i32.const 3
    i32.add
    i32.store align=1
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 2
        local.set 4
        ref.null eq
        local.get 4
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
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
        else
          local.get 4
          i32.const 0
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 17
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 4
          i32.const 4
          i32.add
        else
          local.get 4
          i32.const 1
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 17
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 4
          i32.const 8
          i32.add
        else
          local.get 4
          i32.const 2
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 17
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 4
          i32.const 12
          i32.add
        else
          local.get 4
          i32.const 3
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 17
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 4
          i32.const 16
          i32.add
        else
          local.get 4
          i32.const 4
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 17
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 4
          i32.const 20
          i32.add
        else
          local.get 4
          i32.const 5
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 17
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 4
          i32.const 24
          i32.add
        else
          local.get 4
          i32.const 6
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 17
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 4
          i32.const 28
          i32.add
        else
          local.get 4
          i32.const 7
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 17
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 4
          i32.const 32
          i32.add
        else
          local.get 4
          i32.const 8
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 17
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 4
          i32.const 36
          i32.add
        else
          local.get 4
          i32.const 9
          i32.add
        end
        f32.const 0x0p+0 (;=0;)
        call 17
        local.get 2
        local.set 4
        ref.null eq
        local.get 4
        i32.const 0
        local.set 28
        local.get 28
        i32.const 2
        i32.lt_u
        i32.eqz
        if ;; label = @3
          i32.const 7
          global.get 6
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 42
            array.new_data 0 6
            i32.const 0
            i32.const 42
            call 1
            global.set 6
          end
          global.get 6
          struct.new 3
          throw 0
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
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 4
          local.get 28
          i32.const 20
          i32.mul
          i32.add
        else
          local.get 4
          local.get 28
          i32.const 5
          i32.mul
          i32.add
        end
        i32.const 0
        f32.convert_i32_s
        f64.promote_f32
        i32.const -16776961
        local.set 10
        f32.demote_f64
        local.set 9
        local.get 26
        i32.const 60
        i32.add
        i32.const 0
        i32.const 8
        memory.fill
        ref.null eq
        local.set 8
        local.get 26
        i32.const 60
        i32.add
        local.set 11
        local.get 8
        local.get 9
        local.get 10
        local.get 11
        call 3
        local.get 26
        i32.const 60
        i32.add
        local.get 26
        i32.const 68
        i32.add
        call 4
        local.set 33
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
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
        else
          local.get 4
          i32.const 0
          i32.add
        end
        local.get 33
        i32.const 0
        i32.add
        f32.load align=1
        call 17
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 4
          i32.const 4
          i32.add
        else
          local.get 4
          i32.const 1
          i32.add
        end
        local.get 33
        i32.const 4
        i32.add
        f32.load align=1
        call 17
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 4
          i32.const 8
          i32.add
        else
          local.get 4
          i32.const 2
          i32.add
        end
        local.get 33
        i32.const 8
        i32.add
        f32.load align=1
        call 17
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 4
          i32.const 12
          i32.add
        else
          local.get 4
          i32.const 3
          i32.add
        end
        local.get 33
        i32.const 12
        i32.add
        f32.load align=1
        call 17
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 4
          i32.const 16
          i32.add
        else
          local.get 4
          i32.const 4
          i32.add
        end
        local.get 33
        i32.const 16
        i32.add
        f32.load align=1
        call 17
        local.get 2
        local.set 4
        ref.null eq
        local.get 4
        i32.const 1
        local.set 28
        local.get 28
        i32.const 2
        i32.lt_u
        i32.eqz
        if ;; label = @3
          i32.const 7
          global.get 6
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 42
            array.new_data 0 6
            i32.const 0
            i32.const 42
            call 1
            global.set 6
          end
          global.get 6
          struct.new 3
          throw 0
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
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 4
          local.get 28
          i32.const 20
          i32.mul
          i32.add
        else
          local.get 4
          local.get 28
          i32.const 5
          i32.mul
          i32.add
        end
        i32.const 1
        f32.convert_i32_s
        f64.promote_f32
        i32.const 65535
        local.set 10
        f32.demote_f64
        local.set 9
        local.get 26
        i32.const 88
        i32.add
        i32.const 0
        i32.const 8
        memory.fill
        ref.null eq
        local.set 8
        local.get 26
        i32.const 88
        i32.add
        local.set 11
        local.get 8
        local.get 9
        local.get 10
        local.get 11
        call 3
        local.get 26
        i32.const 88
        i32.add
        local.get 26
        i32.const 96
        i32.add
        call 4
        local.set 33
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
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
        else
          local.get 4
          i32.const 0
          i32.add
        end
        local.get 33
        i32.const 0
        i32.add
        f32.load align=1
        call 17
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 4
          i32.const 4
          i32.add
        else
          local.get 4
          i32.const 1
          i32.add
        end
        local.get 33
        i32.const 4
        i32.add
        f32.load align=1
        call 17
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 4
          i32.const 8
          i32.add
        else
          local.get 4
          i32.const 2
          i32.add
        end
        local.get 33
        i32.const 8
        i32.add
        f32.load align=1
        call 17
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 4
          i32.const 12
          i32.add
        else
          local.get 4
          i32.const 3
          i32.add
        end
        local.get 33
        i32.const 12
        i32.add
        f32.load align=1
        call 17
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 4
          i32.const 16
          i32.add
        else
          local.get 4
          i32.const 4
          i32.add
        end
        local.get 33
        i32.const 16
        i32.add
        f32.load align=1
        call 17
        local.get 2
        local.set 4
        ref.null eq
        local.get 4
        i32.const 2
        local.set 28
        local.get 28
        i32.const 3
        i32.lt_u
        i32.eqz
        if ;; label = @3
          i32.const 7
          global.get 6
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 42
            array.new_data 0 6
            i32.const 0
            i32.const 42
            call 1
            global.set 6
          end
          global.get 6
          struct.new 3
          throw 0
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
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 26
        i32.const 116
        i32.add
        i32.load align=1
        local.get 3
        table.set 0
        local.get 26
        i32.const 120
        i32.add
        local.get 4
        i32.store align=1
        local.get 26
        i32.const 124
        i32.add
        local.get 28
        i32.store align=1
        local.get 26
        i32.const 116
        i32.add
        local.set 27
        local.get 0
        local.get 27
        local.set 24
        local.set 25
        local.get 25
        i32.const 0
        i32.add
        local.get 24
        i32.const 0
        i32.add
        i32.load align=1
        table.get 0
        local.set 22
        i32.load align=1
        local.get 22
        table.set 0
        local.get 25
        i32.const 4
        i32.add
        local.get 24
        i32.const 4
        i32.add
        i32.load align=1
        i32.store align=1
        local.get 25
        i32.const 8
        i32.add
        local.get 24
        i32.const 8
        i32.add
        i32.load align=1
        i32.store align=1
        i32.const 0
        f32.convert_i32_s
        f64.promote_f32
        i32.const 0
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 14
        f32.demote_f64
        local.set 13
        local.get 26
        i32.const 128
        i32.add
        i32.const 0
        i32.const 8
        memory.fill
        ref.null eq
        local.set 12
        local.get 26
        i32.const 128
        i32.add
        local.set 15
        local.get 12
        local.get 13
        local.get 14
        local.get 15
        call 5
        local.get 26
        i32.const 128
        i32.add
        local.get 26
        i32.const 136
        i32.add
        call 6
        i32.const 100
        f32.convert_i32_s
        f64.promote_f32
        i32.const 0
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 14
        f32.demote_f64
        local.set 13
        local.get 26
        i32.const 144
        i32.add
        i32.const 0
        i32.const 8
        memory.fill
        ref.null eq
        local.set 12
        local.get 26
        i32.const 144
        i32.add
        local.set 15
        local.get 12
        local.get 13
        local.get 14
        local.get 15
        call 5
        local.get 26
        i32.const 144
        i32.add
        local.get 26
        i32.const 152
        i32.add
        call 6
        local.set 27
        local.get 1
        local.get 27
        i32.const 8
        memory.copy
        local.get 1
        local.set 27
        local.get 26
        i32.const 160
        i32.add
        local.get 27
        i32.const 8
        memory.copy
        local.get 26
        i32.const 160
        i32.add
        local.get 0
        local.set 27
        local.get 26
        i32.const 168
        i32.add
        local.get 27
        local.set 24
        local.set 25
        local.get 25
        i32.const 0
        i32.add
        local.get 24
        i32.const 0
        i32.add
        i32.load align=1
        table.get 0
        local.set 22
        i32.load align=1
        local.get 22
        table.set 0
        local.get 25
        i32.const 4
        i32.add
        local.get 24
        i32.const 4
        i32.add
        i32.load align=1
        i32.store align=1
        local.get 25
        i32.const 8
        i32.add
        local.get 24
        i32.const 8
        i32.add
        i32.load align=1
        i32.store align=1
        local.get 26
        i32.const 168
        i32.add
        local.set 19
        local.set 18
        local.set 17
        local.get 26
        i32.const 180
        i32.add
        local.set 25
        local.get 25
        i32.const 0
        i32.add
        f32.const 0x0p+0 (;=0;)
        f32.store align=1
        local.get 25
        i32.const 4
        i32.add
        f32.const 0x0p+0 (;=0;)
        f32.store align=1
        local.get 25
        i32.const 8
        i32.add
        f32.const 0x0p+0 (;=0;)
        f32.store align=1
        local.get 25
        i32.const 12
        i32.add
        f32.const 0x0p+0 (;=0;)
        f32.store align=1
        local.get 25
        i32.const 16
        i32.add
        ref.null eq
        local.set 22
        i32.load align=1
        local.get 22
        table.set 0
        local.get 25
        i32.const 20
        i32.add
        i32.const 0
        i32.store align=1
        local.get 25
        i32.const 24
        i32.add
        i32.const 0
        i32.store align=1
        ref.null eq
        local.set 16
        local.get 26
        i32.const 180
        i32.add
        local.set 20
        local.get 16
        local.get 17
        local.get 18
        local.get 19
        local.get 20
        call 7
        local.get 26
        i32.const 180
        i32.add
        call 8
        local.get 23
        ref.null eq
        i32.const 4
        table.fill 0
        local.get 23
        ref.null extern
        i32.const 4
        table.fill 1
        local.get 23
        global.set 8
        local.get 26
        global.set 7
        return
        unreachable
      end
      unreachable
    end
    local.set 21
    local.get 23
    ref.null eq
    i32.const 4
    table.fill 0
    local.get 23
    ref.null extern
    i32.const 4
    table.fill 1
    local.get 23
    global.set 8
    local.get 26
    global.set 7
    local.get 21
    throw_ref
  )
  (func (;3;) (type 29) (param eqref f32 i32 i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 7
    local.tee 13
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 13
    i32.const 0
    i32.add
    global.set 7
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
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 4
        local.get 4
        ref.is_null
        if (type 17) (result i32) ;; label = @3
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
        call 17
        local.get 0
        local.get 3
        local.get 2
        local.set 20
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
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 4
        local.get 4
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 5
          i32.const 4
          i32.add
        else
          local.get 5
          i32.const 1
          i32.add
        end
        local.get 20
        call 19
        local.get 13
        global.set 7
        return
        unreachable
      end
      unreachable
    end
    local.set 8
    local.get 13
    global.set 7
    local.get 8
    throw_ref
  )
  (func (;4;) (type 30) (param i32 i32) (result i32)
    (local eqref i32 eqref i32 eqref f32 i32 i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 7
    local.tee 15
    i32.const 1048528
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 15
    i32.const 48
    i32.add
    global.set 7
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
        i32.load align=1
        local.set 8
        f32.demote_f64
        local.set 7
        local.get 15
        i32.const 24
        i32.add
        i32.const 0
        i32.const 20
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
        call 9
        local.get 15
        i32.const 24
        i32.add
        local.set 16
        local.get 1
        local.get 16
        i32.const 20
        memory.copy
        local.get 1
        local.get 15
        global.set 7
        return
        unreachable
      end
      unreachable
    end
    local.set 10
    local.get 15
    global.set 7
    local.get 10
    throw_ref
  )
  (func (;5;) (type 31) (param eqref f32 f32 i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 7
    local.tee 13
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 13
    i32.const 0
    i32.add
    global.set 7
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
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 4
        local.get 4
        ref.is_null
        if (type 17) (result i32) ;; label = @3
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
        call 17
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
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 4
        local.get 4
        ref.is_null
        if (type 17) (result i32) ;; label = @3
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
        call 17
        local.get 13
        global.set 7
        return
        unreachable
      end
      unreachable
    end
    local.set 8
    local.get 13
    global.set 7
    local.get 8
    throw_ref
  )
  (func (;6;) (type 32) (param i32 i32) (result i32)
    (local eqref i32 eqref i32 eqref f32 f32 i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 7
    local.tee 15
    i32.const 1048544
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 15
    i32.const 32
    i32.add
    global.set 7
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
        call 10
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
        global.set 7
        return
        unreachable
      end
      unreachable
    end
    local.set 10
    local.get 15
    global.set 7
    local.get 10
    throw_ref
  )
  (func (;7;) (type 33) (param eqref i32 i32 i32 i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 7
    local.tee 14
    i32.const 1048512
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 14
    i32.const 64
    i32.add
    global.set 7
    global.get 8
    local.tee 11
    i32.const 65534
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 11
    i32.const 2
    i32.add
    global.set 8
    local.get 14
    i32.const 16
    i32.add
    local.get 11
    i32.const 0
    i32.add
    i32.store align=1
    local.get 14
    i32.const 44
    i32.add
    local.get 11
    i32.const 1
    i32.add
    i32.store align=1
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
    local.get 14
    i32.const 8
    i32.add
    local.get 2
    i32.const 8
    memory.copy
    local.get 14
    i32.const 8
    i32.add
    local.set 2
    local.get 14
    i32.const 16
    i32.add
    local.get 3
    local.set 12
    local.set 13
    local.get 13
    i32.const 0
    i32.add
    local.get 12
    i32.const 0
    i32.add
    i32.load align=1
    table.get 0
    local.set 10
    i32.load align=1
    local.get 10
    table.set 0
    local.get 13
    i32.const 4
    i32.add
    local.get 12
    i32.const 4
    i32.add
    i32.load align=1
    i32.store align=1
    local.get 13
    i32.const 8
    i32.add
    local.get 12
    i32.const 8
    i32.add
    i32.load align=1
    i32.store align=1
    local.get 14
    i32.const 16
    i32.add
    local.set 3
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 4
        local.get 1
        local.set 15
        local.get 14
        i32.const 28
        i32.add
        local.get 15
        i32.const 8
        memory.copy
        local.get 14
        i32.const 28
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
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 5
        local.get 5
        ref.is_null
        if (type 17) (result i32) ;; label = @3
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
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 5
        local.get 5
        ref.is_null
        if (type 17) (result i32) ;; label = @3
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
        call 17
        local.get 5
        ref.is_null
        local.get 6
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 5
        local.get 5
        ref.is_null
        if (type 17) (result i32) ;; label = @3
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
        call 17
        local.get 0
        local.get 4
        local.get 2
        local.set 15
        local.get 14
        i32.const 36
        i32.add
        local.get 15
        i32.const 8
        memory.copy
        local.get 14
        i32.const 36
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
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 5
        local.get 5
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 6
          i32.const 8
          i32.add
        else
          local.get 6
          i32.const 2
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
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 5
        local.get 5
        ref.is_null
        if (type 17) (result i32) ;; label = @3
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
        call 17
        local.get 5
        ref.is_null
        local.get 6
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 5
        local.get 5
        ref.is_null
        if (type 17) (result i32) ;; label = @3
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
        call 17
        local.get 0
        local.get 4
        local.get 3
        local.set 15
        local.get 14
        i32.const 44
        i32.add
        local.get 15
        local.set 12
        local.set 13
        local.get 13
        i32.const 0
        i32.add
        local.get 12
        i32.const 0
        i32.add
        i32.load align=1
        table.get 0
        local.set 10
        i32.load align=1
        local.get 10
        table.set 0
        local.get 13
        i32.const 4
        i32.add
        local.get 12
        i32.const 4
        i32.add
        i32.load align=1
        i32.store align=1
        local.get 13
        i32.const 8
        i32.add
        local.get 12
        i32.const 8
        i32.add
        i32.load align=1
        i32.store align=1
        local.get 14
        i32.const 44
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
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 5
        local.get 5
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 6
          i32.const 16
          i32.add
        else
          local.get 6
          i32.const 4
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
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 5
        local.get 5
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 6
          i32.const 0
          i32.add
          i32.load align=1
        else
          local.get 6
          i32.const 0
          i32.add
        end
        local.get 21
        i32.const 0
        i32.add
        i32.load align=1
        table.get 0
        call 13
        local.get 5
        ref.is_null
        local.get 6
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 5
        local.get 5
        ref.is_null
        if (type 17) (result i32) ;; label = @3
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
        i32.load align=1
        call 21
        local.get 5
        ref.is_null
        local.get 6
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 5
        local.get 5
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 6
          i32.const 8
          i32.add
        else
          local.get 6
          i32.const 2
          i32.add
        end
        local.get 21
        i32.const 8
        i32.add
        i32.load align=1
        call 21
        local.get 11
        ref.null eq
        i32.const 2
        table.fill 0
        local.get 11
        ref.null extern
        i32.const 2
        table.fill 1
        local.get 11
        global.set 8
        local.get 14
        global.set 7
        return
        unreachable
      end
      unreachable
    end
    local.set 9
    local.get 11
    ref.null eq
    i32.const 2
    table.fill 0
    local.get 11
    ref.null extern
    i32.const 2
    table.fill 1
    local.get 11
    global.set 8
    local.get 14
    global.set 7
    local.get 9
    throw_ref
  )
  (func (;8;) (type 34) (param i32) (result i32)
    (local i32 eqref i32 eqref i32 f32 i32 i64 f64 externref exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 7
    local.tee 16
    i32.const 1048464
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 16
    i32.const 112
    i32.add
    global.set 7
    global.get 8
    local.tee 13
    i32.const 65533
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 13
    i32.const 3
    i32.add
    global.set 8
    local.get 16
    i32.const 16
    i32.add
    local.get 13
    i32.const 0
    i32.add
    i32.store align=1
    local.get 16
    i32.const 44
    i32.add
    local.get 13
    i32.const 1
    i32.add
    i32.store align=1
    local.get 16
    i32.const 80
    i32.add
    local.get 13
    i32.const 2
    i32.add
    i32.store align=1
    local.get 16
    i32.const 0
    i32.add
    local.get 0
    local.set 14
    local.set 15
    local.get 15
    i32.const 0
    i32.add
    local.get 14
    i32.const 0
    i32.add
    f32.load align=1
    f32.store align=1
    local.get 15
    i32.const 4
    i32.add
    local.get 14
    i32.const 4
    i32.add
    f32.load align=1
    f32.store align=1
    local.get 15
    i32.const 8
    i32.add
    local.get 14
    i32.const 8
    i32.add
    f32.load align=1
    f32.store align=1
    local.get 15
    i32.const 12
    i32.add
    local.get 14
    i32.const 12
    i32.add
    f32.load align=1
    f32.store align=1
    local.get 15
    i32.const 16
    i32.add
    local.get 14
    i32.const 16
    i32.add
    i32.load align=1
    table.get 0
    local.set 12
    i32.load align=1
    local.get 12
    table.set 0
    local.get 15
    i32.const 20
    i32.add
    local.get 14
    i32.const 20
    i32.add
    i32.load align=1
    i32.store align=1
    local.get 15
    i32.const 24
    i32.add
    local.get 14
    i32.const 24
    i32.add
    i32.load align=1
    i32.store align=1
    local.get 16
    i32.const 0
    i32.add
    local.set 0
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.set 17
        local.get 16
        i32.const 28
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
        f32.load align=1
        f32.store align=1
        local.get 15
        i32.const 4
        i32.add
        local.get 14
        i32.const 4
        i32.add
        f32.load align=1
        f32.store align=1
        local.get 15
        i32.const 8
        i32.add
        local.get 14
        i32.const 8
        i32.add
        f32.load align=1
        f32.store align=1
        local.get 15
        i32.const 12
        i32.add
        local.get 14
        i32.const 12
        i32.add
        f32.load align=1
        f32.store align=1
        local.get 15
        i32.const 16
        i32.add
        local.get 14
        i32.const 16
        i32.add
        i32.load align=1
        table.get 0
        local.set 12
        i32.load align=1
        local.get 12
        table.set 0
        local.get 15
        i32.const 20
        i32.add
        local.get 14
        i32.const 20
        i32.add
        i32.load align=1
        i32.store align=1
        local.get 15
        i32.const 24
        i32.add
        local.get 14
        i32.const 24
        i32.add
        i32.load align=1
        i32.store align=1
        local.get 16
        i32.const 28
        i32.add
        i32.const 0
        i32.add
        local.set 17
        local.get 16
        i32.const 56
        i32.add
        local.get 17
        i32.const 8
        memory.copy
        local.get 16
        i32.const 56
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        call 0
        local.get 0
        local.set 17
        local.get 16
        i32.const 64
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
        f32.load align=1
        f32.store align=1
        local.get 15
        i32.const 4
        i32.add
        local.get 14
        i32.const 4
        i32.add
        f32.load align=1
        f32.store align=1
        local.get 15
        i32.const 8
        i32.add
        local.get 14
        i32.const 8
        i32.add
        f32.load align=1
        f32.store align=1
        local.get 15
        i32.const 12
        i32.add
        local.get 14
        i32.const 12
        i32.add
        f32.load align=1
        f32.store align=1
        local.get 15
        i32.const 16
        i32.add
        local.get 14
        i32.const 16
        i32.add
        i32.load align=1
        table.get 0
        local.set 12
        i32.load align=1
        local.get 12
        table.set 0
        local.get 15
        i32.const 20
        i32.add
        local.get 14
        i32.const 20
        i32.add
        i32.load align=1
        i32.store align=1
        local.get 15
        i32.const 24
        i32.add
        local.get 14
        i32.const 24
        i32.add
        i32.load align=1
        i32.store align=1
        local.get 16
        i32.const 64
        i32.add
        i32.const 8
        i32.add
        local.set 17
        local.get 16
        i32.const 92
        i32.add
        local.get 17
        i32.const 8
        memory.copy
        local.get 16
        i32.const 92
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        call 0
        i32.const 0
        local.set 1
        block (type 35) ;; label = @3
          loop (type 35) ;; label = @4
            local.get 1
            local.get 0
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
            if ;; label = @5
              i32.const 4
              global.get 3
              ref.is_null
              if ;; label = @6
                i32.const 0
                i32.const 53
                array.new_data 0 3
                i32.const 0
                i32.const 53
                call 1
                global.set 3
              end
              global.get 3
              struct.new 3
              throw 0
            end
            local.get 2
            local.get 2
            ref.is_null
            if (type 17) (result i32) ;; label = @5
              local.get 3
              i32.const 16
              i32.add
            else
              local.get 3
              i32.const 4
              i32.add
            end
            local.set 3
            local.set 2
            local.get 2
            ref.is_null
            local.get 3
            i32.const -1
            i32.eq
            i32.and
            if ;; label = @5
              i32.const 4
              global.get 3
              ref.is_null
              if ;; label = @6
                i32.const 0
                i32.const 53
                array.new_data 0 3
                i32.const 0
                i32.const 53
                call 1
                global.set 3
              end
              global.get 3
              struct.new 3
              throw 0
            end
            local.get 2
            ref.is_null
            i32.eqz
            if ;; label = @5
              unreachable
            end
            local.get 3
            i32.const 8
            i32.add
            i32.load align=1
            i32.lt_s
            if (type 36) ;; label = @5
              local.get 0
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
              if ;; label = @6
                i32.const 4
                global.get 3
                ref.is_null
                if ;; label = @7
                  i32.const 0
                  i32.const 53
                  array.new_data 0 3
                  i32.const 0
                  i32.const 53
                  call 1
                  global.set 3
                end
                global.get 3
                struct.new 3
                throw 0
              end
              local.get 2
              local.get 2
              ref.is_null
              if (type 17) (result i32) ;; label = @6
                local.get 3
                i32.const 16
                i32.add
              else
                local.get 3
                i32.const 4
                i32.add
              end
              local.get 1
              local.set 18
              local.set 3
              local.set 2
              local.get 2
              ref.is_null
              local.get 3
              i32.const -1
              i32.eq
              i32.and
              if ;; label = @6
                i32.const 4
                global.get 3
                ref.is_null
                if ;; label = @7
                  i32.const 0
                  i32.const 53
                  array.new_data 0 3
                  i32.const 0
                  i32.const 53
                  call 1
                  global.set 3
                end
                global.get 3
                struct.new 3
                throw 0
              end
              local.get 2
              ref.is_null
              i32.eqz
              if ;; label = @6
                unreachable
              end
              local.get 3
              local.set 17
              local.get 18
              local.get 17
              i32.const 8
              i32.add
              i32.load align=1
              i32.lt_u
              i32.eqz
              if ;; label = @6
                i32.const 7
                global.get 6
                ref.is_null
                if ;; label = @7
                  i32.const 0
                  i32.const 42
                  array.new_data 0 6
                  i32.const 0
                  i32.const 42
                  call 1
                  global.set 6
                end
                global.get 6
                struct.new 3
                throw 0
              end
              local.get 17
              i32.load align=1
              table.get 0
              local.set 2
              local.get 17
              i32.const 4
              i32.add
              i32.load align=1
              local.set 3
              local.get 2
              local.get 2
              ref.is_null
              if (type 17) (result i32) ;; label = @6
                local.get 3
                local.get 18
                i32.const 20
                i32.mul
                i32.add
                i32.const 0
                i32.add
              else
                local.get 3
                local.get 18
                i32.const 5
                i32.mul
                i32.add
                i32.const 0
                i32.add
              end
              local.set 23
              local.set 22
              local.get 22
              local.get 23
              local.get 22
              local.get 23
              local.set 3
              local.set 2
              local.get 2
              ref.is_null
              local.get 3
              i32.const -1
              i32.eq
              i32.and
              if ;; label = @6
                i32.const 4
                global.get 3
                ref.is_null
                if ;; label = @7
                  i32.const 0
                  i32.const 53
                  array.new_data 0 3
                  i32.const 0
                  i32.const 53
                  call 1
                  global.set 3
                end
                global.get 3
                struct.new 3
                throw 0
              end
              local.get 2
              local.get 2
              ref.is_null
              if (type 17) (result i32) ;; label = @6
                local.get 3
                i32.const 0
                i32.add
              else
                local.get 3
                i32.const 0
                i32.add
              end
              call 16
              f64.promote_f32
              call 0
              local.set 23
              local.set 22
              local.get 22
              local.get 23
              local.get 22
              local.get 23
              local.set 3
              local.set 2
              local.get 2
              ref.is_null
              local.get 3
              i32.const -1
              i32.eq
              i32.and
              if ;; label = @6
                i32.const 4
                global.get 3
                ref.is_null
                if ;; label = @7
                  i32.const 0
                  i32.const 53
                  array.new_data 0 3
                  i32.const 0
                  i32.const 53
                  call 1
                  global.set 3
                end
                global.get 3
                struct.new 3
                throw 0
              end
              local.get 2
              local.get 2
              ref.is_null
              if (type 17) (result i32) ;; label = @6
                local.get 3
                i32.const 4
                i32.add
              else
                local.get 3
                i32.const 1
                i32.add
              end
              local.set 3
              local.set 2
              local.get 2
              ref.is_null
              local.get 3
              i32.const -1
              i32.eq
              i32.and
              if ;; label = @6
                i32.const 4
                global.get 3
                ref.is_null
                if ;; label = @7
                  i32.const 0
                  i32.const 53
                  array.new_data 0 3
                  i32.const 0
                  i32.const 53
                  call 1
                  global.set 3
                end
                global.get 3
                struct.new 3
                throw 0
              end
              local.get 2
              local.get 2
              ref.is_null
              if (type 17) (result i32) ;; label = @6
                local.get 3
                i32.const 0
                i32.add
              else
                local.get 3
                i32.const 0
                i32.add
              end
              call 16
              f64.promote_f32
              call 0
              local.set 3
              local.set 2
              local.get 2
              ref.is_null
              local.get 3
              i32.const -1
              i32.eq
              i32.and
              if ;; label = @6
                i32.const 4
                global.get 3
                ref.is_null
                if ;; label = @7
                  i32.const 0
                  i32.const 53
                  array.new_data 0 3
                  i32.const 0
                  i32.const 53
                  call 1
                  global.set 3
                end
                global.get 3
                struct.new 3
                throw 0
              end
              local.get 2
              local.get 2
              ref.is_null
              if (type 17) (result i32) ;; label = @6
                local.get 3
                i32.const 4
                i32.add
              else
                local.get 3
                i32.const 1
                i32.add
              end
              local.set 3
              local.set 2
              local.get 2
              ref.is_null
              local.get 3
              i32.const -1
              i32.eq
              i32.and
              if ;; label = @6
                i32.const 4
                global.get 3
                ref.is_null
                if ;; label = @7
                  i32.const 0
                  i32.const 53
                  array.new_data 0 3
                  i32.const 0
                  i32.const 53
                  call 1
                  global.set 3
                end
                global.get 3
                struct.new 3
                throw 0
              end
              local.get 2
              local.get 2
              ref.is_null
              if (type 17) (result i32) ;; label = @6
                local.get 3
                i32.const 8
                i32.add
              else
                local.get 3
                i32.const 2
                i32.add
              end
              call 16
              f64.promote_f32
              call 0
              local.get 1
              i32.const 1
              i32.add
              local.set 1
              br 1 (;@4;)
            else
              br 2 (;@3;)
            end
            unreachable
          end
        end
        local.get 0
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
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 2
        local.get 2
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 3
          i32.const 16
          i32.add
        else
          local.get 3
          i32.const 4
          i32.add
        end
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
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
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
        local.get 13
        ref.null eq
        i32.const 3
        table.fill 0
        local.get 13
        ref.null extern
        i32.const 3
        table.fill 1
        local.get 13
        global.set 8
        local.get 16
        global.set 7
        return
        unreachable
      end
      unreachable
    end
    local.set 11
    local.get 13
    ref.null eq
    i32.const 3
    table.fill 0
    local.get 13
    ref.null extern
    i32.const 3
    table.fill 1
    local.get 13
    global.set 8
    local.get 16
    global.set 7
    local.get 11
    throw_ref
  )
  (func (;9;) (type 37) (param eqref f32 i32 i32)
    (local eqref i32 eqref i32 eqref i32 i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 7
    local.tee 16
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 16
    i32.const 16
    i32.add
    global.set 7
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 3
        local.get 1
        f64.promote_f32
        local.set 25
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
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 4
        local.get 4
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 5
          i32.const 0
          i32.add
        else
          local.get 5
          i32.const 0
          i32.add
        end
        local.get 25
        f32.demote_f64
        call 17
        local.get 0
        local.get 3
        local.get 2
        local.set 9
        local.get 16
        i32.const 0
        i32.add
        i32.const 0
        i32.const 16
        memory.fill
        ref.null eq
        local.set 8
        local.get 16
        i32.const 0
        i32.add
        local.set 10
        local.get 8
        local.get 9
        local.get 10
        call 11
        local.get 16
        i32.const 0
        i32.add
        local.set 23
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
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 4
        local.get 4
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 5
          i32.const 4
          i32.add
        else
          local.get 5
          i32.const 1
          i32.add
        end
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
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 4
        local.get 4
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 5
          i32.const 0
          i32.add
        else
          local.get 5
          i32.const 0
          i32.add
        end
        local.get 23
        i32.const 0
        i32.add
        f32.load align=1
        call 17
        local.get 4
        ref.is_null
        local.get 5
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 4
        local.get 4
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 5
          i32.const 4
          i32.add
        else
          local.get 5
          i32.const 1
          i32.add
        end
        local.get 23
        i32.const 4
        i32.add
        f32.load align=1
        call 17
        local.get 4
        ref.is_null
        local.get 5
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 4
        local.get 4
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 5
          i32.const 8
          i32.add
        else
          local.get 5
          i32.const 2
          i32.add
        end
        local.get 23
        i32.const 8
        i32.add
        f32.load align=1
        call 17
        local.get 4
        ref.is_null
        local.get 5
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 4
        local.get 4
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 5
          i32.const 12
          i32.add
        else
          local.get 5
          i32.const 3
          i32.add
        end
        local.get 23
        i32.const 12
        i32.add
        f32.load align=1
        call 17
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
  (func (;10;) (type 38) (param eqref f32 f32 i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 7
    local.tee 13
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 13
    i32.const 0
    i32.add
    global.set 7
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
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 4
        local.get 4
        ref.is_null
        if (type 17) (result i32) ;; label = @3
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
        call 17
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
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 4
        local.get 4
        ref.is_null
        if (type 17) (result i32) ;; label = @3
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
        call 17
        local.get 13
        global.set 7
        return
        unreachable
      end
      unreachable
    end
    local.set 8
    local.get 13
    global.set 7
    local.get 8
    throw_ref
  )
  (func (;11;) (type 39) (param eqref i32 i32)
    (local eqref i32 eqref i32 f32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 7
    local.tee 13
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 13
    i32.const 0
    i32.add
    global.set 7
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
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
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
        call 17
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
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
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
        call 17
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
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
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
        call 17
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
          global.get 3
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 3
            i32.const 0
            i32.const 53
            call 1
            global.set 3
          end
          global.get 3
          struct.new 3
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
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
        call 17
        local.get 13
        global.set 7
        return
        unreachable
      end
      unreachable
    end
    local.set 8
    local.get 13
    global.set 7
    local.get 8
    throw_ref
  )
  (func (;12;) (type 18) (param eqref i32) (result eqref)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 3
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 3
        i32.const 0
        i32.const 53
        call 1
        global.set 3
      end
      i32.const 4
      global.get 3
      struct.new 3
      throw 0
    end
    local.get 0
    ref.is_null
    if ;; label = @1
      local.get 1
      table.get 0
      return
    end
    unreachable
  )
  (func (;13;) (type 19) (param eqref i32 eqref)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 3
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 3
        i32.const 0
        i32.const 53
        call 1
        global.set 3
      end
      i32.const 4
      global.get 3
      struct.new 3
      throw 0
    end
    local.get 0
    ref.is_null
    if ;; label = @1
      local.get 1
      local.get 2
      table.set 0
      return
    end
    unreachable
  )
  (func (;14;) (type 20) (param eqref i32) (result externref)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 3
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 3
        i32.const 0
        i32.const 53
        call 1
        global.set 3
      end
      i32.const 4
      global.get 3
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
  (func (;15;) (type 21) (param eqref i32 externref)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 3
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 3
        i32.const 0
        i32.const 53
        call 1
        global.set 3
      end
      i32.const 4
      global.get 3
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
  (func (;16;) (type 22) (param eqref i32) (result f32)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 3
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 3
        i32.const 0
        i32.const 53
        call 1
        global.set 3
      end
      i32.const 4
      global.get 3
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
  (func (;17;) (type 23) (param eqref i32 f32)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 3
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 3
        i32.const 0
        i32.const 53
        call 1
        global.set 3
      end
      i32.const 4
      global.get 3
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
  (func (;18;) (type 24) (param eqref i32) (result i32)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 3
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 3
        i32.const 0
        i32.const 53
        call 1
        global.set 3
      end
      i32.const 4
      global.get 3
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
  (func (;19;) (type 25) (param eqref i32 i32)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 3
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 3
        i32.const 0
        i32.const 53
        call 1
        global.set 3
      end
      i32.const 4
      global.get 3
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
  (func (;20;) (type 26) (param eqref i32) (result i32)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 3
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 3
        i32.const 0
        i32.const 53
        call 1
        global.set 3
      end
      i32.const 4
      global.get 3
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
  (func (;21;) (type 27) (param eqref i32 i32)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 3
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 3
        i32.const 0
        i32.const 53
        call 1
        global.set 3
      end
      i32.const 4
      global.get 3
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
  (data (;0;) "E\00x\00c\00e\00p\00t\00i\00o\00n\00 \00o\00f\00 \00t\00y\00p\00e\00 \00'\00S\00y\00s\00t\00e\00m\00.\00E\00x\00c\00e\00p\00t\00i\00o\00n\00'\00 \00w\00a\00s\00 \00t\00h\00r\00o\00w\00n\00.\00")
  (data (;1;) "S\00y\00s\00t\00e\00m\00 \00e\00r\00r\00o\00r\00.\00")
  (data (;2;) "S\00p\00e\00c\00i\00f\00i\00e\00d\00 \00c\00a\00s\00t\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00.\00")
  (data (;3;) "O\00b\00j\00e\00c\00t\00 \00r\00e\00f\00e\00r\00e\00n\00c\00e\00 \00n\00o\00t\00 \00s\00e\00t\00 \00t\00o\00 \00a\00n\00 \00i\00n\00s\00t\00a\00n\00c\00e\00 \00o\00f\00 \00a\00n\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;4;) "O\00p\00e\00r\00a\00t\00i\00o\00n\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00 \00d\00u\00e\00 \00t\00o\00 \00t\00h\00e\00 \00c\00u\00r\00r\00e\00n\00t\00 \00s\00t\00a\00t\00e\00 \00o\00f\00 \00t\00h\00e\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;5;) "V\00a\00l\00u\00e\00 \00d\00o\00e\00s\00 \00n\00o\00t\00 \00f\00a\00l\00l\00 \00w\00i\00t\00h\00i\00n\00 \00t\00h\00e\00 \00e\00x\00p\00e\00c\00t\00e\00d\00 \00r\00a\00n\00g\00e\00.\00")
  (data (;6;) "I\00n\00d\00e\00x\00 \00w\00a\00s\00 \00o\00u\00t\00s\00i\00d\00e\00 \00t\00h\00e\00 \00b\00o\00u\00n\00d\00s\00 \00o\00f\00 \00t\00h\00e\00 \00a\00r\00r\00a\00y\00.\00")
)
```
