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
  (type (;9;) (func (result i32)))
  (type (;10;) (func (param i32 f32 i32)))
  (type (;11;) (func (param i32 i32) (result i32)))
  (type (;12;) (func (param i32 f32 f32)))
  (type (;13;) (func (param i32 i32) (result i32)))
  (type (;14;) (func (param i32 i32 i32 i32)))
  (type (;15;) (func (param i32) (result i32)))
  (type (;16;) (func))
  (type (;17;) (func))
  (type (;18;) (func (param i32 f32 i32)))
  (type (;19;) (func (param i32 f32 f32)))
  (type (;20;) (func (param i32 i32)))
  (import "xui" "console-log" (func (;0;) (type 2) (param f64)))
  (import "wasm:js-string" "fromCharCodeArray" (func (;1;) (type 1) (param (ref null 0) i32 i32) (result (ref extern))))
  (memory (;0;) 16 16)
  (tag (;0;) (type 4) (param eqref))
  (global (;0;) (mut externref) ref.null extern)
  (global (;1;) (mut externref) ref.null extern)
  (global (;2;) (mut externref) ref.null extern)
  (global (;3;) (mut externref) ref.null extern)
  (global (;4;) (mut externref) ref.null extern)
  (global (;5;) (mut externref) ref.null extern)
  (global (;6;) (mut externref) ref.null extern)
  (global (;7;) (mut i32) i32.const 16)
  (export "main" (func 2))
  (func (;2;) (type 9) (result i32)
    (local i32 i32 i32 f32 i32 f32 i32 i32 f32 f32 i32 i32 i32 i32 exnref i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 7
    local.tee 15
    i32.const 1048384
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 15
    i32.const 192
    i32.add
    global.set 7
    local.get 15
    i32.const 0
    i32.add
    local.set 0
    local.get 0
    i32.const 0
    i32.const 8
    memory.fill
    local.get 15
    i32.const 8
    i32.add
    local.set 1
    local.get 1
    i32.const 0
    i32.const 8
    memory.fill
    local.get 15
    i32.const 16
    i32.add
    local.set 2
    local.get 2
    i32.const 0
    i32.const 40
    memory.fill
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 2
        i32.const 0
        i32.const 40
        memory.fill
        local.get 2
        i32.const 0
        local.set 17
        local.get 17
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
        local.get 17
        i32.const 20
        i32.mul
        i32.add
        i32.const 0
        f32.convert_i32_s
        f64.promote_f32
        i32.const -16776961
        local.set 6
        f32.demote_f64
        local.set 5
        local.get 15
        i32.const 56
        i32.add
        local.tee 4
        i32.const 0
        i32.const 8
        memory.fill
        local.get 4
        local.get 5
        local.get 6
        call 3
        local.get 4
        local.get 15
        i32.const 64
        i32.add
        call 4
        i32.const 20
        memory.copy
        local.get 2
        i32.const 1
        local.set 17
        local.get 17
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
        local.get 17
        i32.const 20
        i32.mul
        i32.add
        i32.const 1
        f32.convert_i32_s
        f64.promote_f32
        i32.const 65535
        local.set 6
        f32.demote_f64
        local.set 5
        local.get 15
        i32.const 84
        i32.add
        local.tee 4
        i32.const 0
        i32.const 8
        memory.fill
        local.get 4
        local.get 5
        local.get 6
        call 3
        local.get 4
        local.get 15
        i32.const 92
        i32.add
        call 4
        i32.const 20
        memory.copy
        local.get 2
        i32.const 2
        local.set 17
        local.get 17
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
        local.set 16
        local.get 15
        i32.const 112
        i32.add
        local.get 16
        i32.store align=1
        local.get 15
        i32.const 116
        i32.add
        local.get 17
        i32.store align=1
        local.get 15
        i32.const 112
        i32.add
        local.set 16
        local.get 0
        local.get 16
        i32.const 8
        memory.copy
        i32.const 0
        f32.convert_i32_s
        f64.promote_f32
        i32.const 0
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 9
        f32.demote_f64
        local.set 8
        local.get 15
        i32.const 120
        i32.add
        local.tee 7
        i32.const 0
        i32.const 8
        memory.fill
        local.get 7
        local.get 8
        local.get 9
        call 5
        local.get 7
        local.get 15
        i32.const 128
        i32.add
        call 6
        i32.const 100
        f32.convert_i32_s
        f64.promote_f32
        i32.const 0
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 9
        f32.demote_f64
        local.set 8
        local.get 15
        i32.const 136
        i32.add
        local.tee 7
        i32.const 0
        i32.const 8
        memory.fill
        local.get 7
        local.get 8
        local.get 9
        call 5
        local.get 7
        local.get 15
        i32.const 144
        i32.add
        call 6
        local.set 16
        local.get 1
        local.get 16
        i32.const 8
        memory.copy
        local.get 1
        local.set 16
        local.get 15
        i32.const 152
        i32.add
        local.get 16
        i32.const 8
        memory.copy
        local.get 15
        i32.const 152
        i32.add
        local.get 0
        local.set 16
        local.get 15
        i32.const 160
        i32.add
        local.get 16
        i32.const 8
        memory.copy
        local.get 15
        i32.const 160
        i32.add
        local.set 13
        local.set 12
        local.set 11
        local.get 15
        i32.const 168
        i32.add
        local.tee 10
        i32.const 0
        i32.const 24
        memory.fill
        local.get 10
        local.get 11
        local.get 12
        local.get 13
        call 7
        local.get 10
        call 8
        local.get 15
        global.set 7
        return
        unreachable
      end
      unreachable
    end
    local.set 14
    local.get 15
    global.set 7
    local.get 14
    throw_ref
  )
  (func (;3;) (type 10) (param i32 f32 i32)
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
    block (type 7) (result exnref) ;; label = @1
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
        local.set 6
        i32.const 4
        i32.add
        local.get 6
        i32.store align=1
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
  (func (;4;) (type 11) (param i32 i32) (result i32)
    (local i32 f32 i32 exnref i32 i32 i32 i64 f64)
    global.get 7
    local.tee 6
    i32.const 1048528
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 6
    i32.const 48
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
    block (type 7) (result exnref) ;; label = @1
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
        i32.load align=1
        local.set 4
        f32.demote_f64
        local.set 3
        local.get 6
        i32.const 24
        i32.add
        local.tee 2
        i32.const 0
        i32.const 20
        memory.fill
        local.get 2
        local.get 3
        local.get 4
        call 9
        local.get 2
        local.set 7
        local.get 1
        local.get 7
        i32.const 20
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
  (func (;5;) (type 12) (param i32 f32 f32)
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
    block (type 7) (result exnref) ;; label = @1
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
  (func (;6;) (type 13) (param i32 i32) (result i32)
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
    block (type 7) (result exnref) ;; label = @1
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
        call 10
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
  (func (;7;) (type 14) (param i32 i32 i32 i32)
    (local exnref i32 i32 i32 i64 f64)
    global.get 7
    local.tee 5
    i32.const 1048528
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 5
    i32.const 48
    i32.add
    global.set 7
    local.get 5
    i32.const 0
    i32.add
    local.get 1
    i32.const 8
    memory.copy
    local.get 5
    i32.const 0
    i32.add
    local.set 1
    local.get 5
    i32.const 8
    i32.add
    local.get 2
    i32.const 8
    memory.copy
    local.get 5
    i32.const 8
    i32.add
    local.set 2
    local.get 5
    i32.const 16
    i32.add
    local.get 3
    i32.const 8
    memory.copy
    local.get 5
    i32.const 16
    i32.add
    local.set 3
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 6
        local.get 5
        i32.const 24
        i32.add
        local.get 6
        i32.const 8
        memory.copy
        local.get 5
        i32.const 24
        i32.add
        local.set 7
        i32.const 0
        i32.add
        local.get 7
        i32.const 8
        memory.copy
        local.get 0
        local.get 2
        local.set 6
        local.get 5
        i32.const 32
        i32.add
        local.get 6
        i32.const 8
        memory.copy
        local.get 5
        i32.const 32
        i32.add
        local.set 7
        i32.const 8
        i32.add
        local.get 7
        i32.const 8
        memory.copy
        local.get 0
        local.get 3
        local.set 6
        local.get 5
        i32.const 40
        i32.add
        local.get 6
        i32.const 8
        memory.copy
        local.get 5
        i32.const 40
        i32.add
        local.set 7
        i32.const 16
        i32.add
        local.get 7
        i32.const 8
        memory.copy
        local.get 5
        global.set 7
        return
        unreachable
      end
      unreachable
    end
    local.set 4
    local.get 5
    global.set 7
    local.get 4
    throw_ref
  )
  (func (;8;) (type 15) (param i32) (result i32)
    (local i32 f32 i32 i64 f64 externref exnref i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
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
    i32.const 24
    memory.copy
    local.get 8
    i32.const 0
    i32.add
    local.set 0
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.set 9
        local.get 8
        i32.const 24
        i32.add
        local.get 9
        i32.const 24
        memory.copy
        local.get 8
        i32.const 24
        i32.add
        i32.const 0
        i32.add
        local.set 9
        local.get 8
        i32.const 48
        i32.add
        local.get 9
        i32.const 8
        memory.copy
        local.get 8
        i32.const 48
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        call 0
        local.get 0
        local.set 9
        local.get 8
        i32.const 56
        i32.add
        local.get 9
        i32.const 24
        memory.copy
        local.get 8
        i32.const 56
        i32.add
        i32.const 8
        i32.add
        local.set 9
        local.get 8
        i32.const 80
        i32.add
        local.get 9
        i32.const 8
        memory.copy
        local.get 8
        i32.const 80
        i32.add
        i32.const 0
        i32.add
        f32.load align=1
        f64.promote_f32
        call 0
        i32.const 0
        local.set 1
        block (type 16) ;; label = @3
          loop (type 16) ;; label = @4
            local.get 1
            local.get 0
            i32.const 16
            i32.add
            i32.const 4
            i32.add
            i32.load align=1
            i32.lt_s
            if (type 17) ;; label = @5
              local.get 0
              i32.const 16
              i32.add
              local.get 1
              local.set 10
              local.set 9
              local.get 10
              local.get 9
              i32.const 4
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
              local.get 9
              i32.load align=1
              local.get 10
              i32.const 20
              i32.mul
              i32.add
              local.tee 3
              local.get 3
              i32.const 0
              i32.add
              f32.load align=1
              f64.promote_f32
              call 0
              local.tee 3
              local.get 3
              i32.const 4
              i32.add
              i32.const 0
              i32.add
              f32.load align=1
              f64.promote_f32
              call 0
              i32.const 4
              i32.add
              i32.const 8
              i32.add
              f32.load align=1
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
        i32.const 16
        i32.add
        i32.const 4
        i32.add
        i32.load align=1
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
  (func (;9;) (type 18) (param i32 f32 i32)
    (local i32 i32 exnref i32 i32 i32 i64 f64)
    global.get 7
    local.tee 6
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 6
    i32.const 16
    i32.add
    global.set 7
    block (type 7) (result exnref) ;; label = @1
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
        local.set 4
        local.get 6
        i32.const 0
        i32.add
        local.tee 3
        i32.const 0
        i32.const 16
        memory.fill
        local.get 3
        local.get 4
        call 11
        local.get 3
        local.set 8
        i32.const 4
        i32.add
        local.get 8
        i32.const 16
        memory.copy
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
  (func (;10;) (type 19) (param i32 f32 f32)
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
    block (type 7) (result exnref) ;; label = @1
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
  (func (;11;) (type 20) (param i32 i32)
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
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        i32.const -16777216
        i32.and
        i32.const 24
        i32.shr_u
        f64.convert_i32_u
        f32.demote_f64
        f64.promote_f32
        i32.const 255
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 2
        f32.demote_f64
        local.get 2
        f32.div
        f64.promote_f32
        local.set 8
        i32.const 0
        i32.add
        local.get 8
        f32.demote_f64
        f32.store align=1
        local.get 0
        local.get 1
        i32.const 16711680
        i32.and
        i32.const 16
        i32.shr_u
        f64.convert_i32_u
        f32.demote_f64
        f64.promote_f32
        i32.const 255
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 2
        f32.demote_f64
        local.get 2
        f32.div
        f64.promote_f32
        local.set 8
        i32.const 4
        i32.add
        local.get 8
        f32.demote_f64
        f32.store align=1
        local.get 0
        local.get 1
        i32.const 65280
        i32.and
        i32.const 8
        i32.shr_u
        f64.convert_i32_u
        f32.demote_f64
        f64.promote_f32
        i32.const 255
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 2
        f32.demote_f64
        local.get 2
        f32.div
        f64.promote_f32
        local.set 8
        i32.const 8
        i32.add
        local.get 8
        f32.demote_f64
        f32.store align=1
        local.get 0
        local.get 1
        i32.const 255
        i32.and
        f64.convert_i32_u
        f32.demote_f64
        f64.promote_f32
        i32.const 255
        f32.convert_i32_s
        f64.promote_f32
        f32.demote_f64
        local.set 2
        f32.demote_f64
        local.get 2
        f32.div
        f64.promote_f32
        local.set 8
        i32.const 12
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
  (data (;0;) "E\00x\00c\00e\00p\00t\00i\00o\00n\00 \00o\00f\00 \00t\00y\00p\00e\00 \00'\00S\00y\00s\00t\00e\00m\00.\00E\00x\00c\00e\00p\00t\00i\00o\00n\00'\00 \00w\00a\00s\00 \00t\00h\00r\00o\00w\00n\00.\00")
  (data (;1;) "S\00y\00s\00t\00e\00m\00 \00e\00r\00r\00o\00r\00.\00")
  (data (;2;) "S\00p\00e\00c\00i\00f\00i\00e\00d\00 \00c\00a\00s\00t\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00.\00")
  (data (;3;) "O\00b\00j\00e\00c\00t\00 \00r\00e\00f\00e\00r\00e\00n\00c\00e\00 \00n\00o\00t\00 \00s\00e\00t\00 \00t\00o\00 \00a\00n\00 \00i\00n\00s\00t\00a\00n\00c\00e\00 \00o\00f\00 \00a\00n\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;4;) "O\00p\00e\00r\00a\00t\00i\00o\00n\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00 \00d\00u\00e\00 \00t\00o\00 \00t\00h\00e\00 \00c\00u\00r\00r\00e\00n\00t\00 \00s\00t\00a\00t\00e\00 \00o\00f\00 \00t\00h\00e\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;5;) "V\00a\00l\00u\00e\00 \00d\00o\00e\00s\00 \00n\00o\00t\00 \00f\00a\00l\00l\00 \00w\00i\00t\00h\00i\00n\00 \00t\00h\00e\00 \00e\00x\00p\00e\00c\00t\00e\00d\00 \00r\00a\00n\00g\00e\00.\00")
  (data (;6;) "I\00n\00d\00e\00x\00 \00w\00a\00s\00 \00o\00u\00t\00s\00i\00d\00e\00 \00t\00h\00e\00 \00b\00o\00u\00n\00d\00s\00 \00o\00f\00 \00t\00h\00e\00 \00a\00r\00r\00a\00y\00.\00")
)
```
