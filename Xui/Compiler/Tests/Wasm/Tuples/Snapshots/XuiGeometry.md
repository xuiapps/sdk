# Xui tuple geometry in WASM

Source: [XuiGeometry.cs](Fixtures/Tuples/XuiGeometry.cs)

## Source

```csharp
using System;
using Xui.Core.Math2D;

public static class Program
{
    public static int Main()
    {
        Point point = (0, 0);
        Vector step = (3, 4);
        point += step;
        Size size = (120, 80);
        Rect bounds = (10, 20, 120, 80);
        Frame margin = (5, 10);
        Rect expanded = bounds + margin;
        Console.WriteLine((double)point.X);
        Console.WriteLine((double)point.Y);
        Console.WriteLine((double)size.Width);
        Console.WriteLine((double)expanded.X);
        Console.WriteLine((double)expanded.Y);
        Console.WriteLine((double)expanded.Width);
        Console.WriteLine((double)expanded.Height);
        return 0;
    }
}
```

## Output

```text
3
4
120
0
15
140
90
```

## Exit Code

```text
0
```

## WAT

```wat
(module
  (type (;0;) (array (mut i16)))
  (type (;1;) (func (param (ref null 0) i32 i32) (result (ref extern))))
  (type (;2;) (func (param f64)))
  (type (;3;) (func (result i32)))
  (type (;4;) (func (param i32 f32 f32)))
  (type (;5;) (func (param i32 i32) (result i32)))
  (type (;6;) (func (param i32 i32) (result i32)))
  (type (;7;) (func (param i32 i32 i32) (result i32)))
  (type (;8;) (func (param i32 i32) (result i32)))
  (type (;9;) (func (param i32 f32 f32 f32 f32)))
  (type (;10;) (func (param i32 i32) (result i32)))
  (type (;11;) (func (param i32 i32) (result i32)))
  (type (;12;) (func (param i32 i32 i32) (result i32)))
  (type (;13;) (func (param i32 f32 f32)))
  (type (;14;) (func (param i32 f32 f32)))
  (type (;15;) (func (param i32 f32 f32)))
  (type (;16;) (func (param i32 f32 f32 f32 f32)))
  (type (;17;) (func (param i32 f32 f32 f32 f32)))
  (import "xui" "console-log" (func (;0;) (type 2) (param f64)))
  (import "wasm:js-string" "fromCharCodeArray" (func (;1;) (type 1) (param (ref null 0) i32 i32) (result (ref extern))))
  (memory (;0;) 16 16)
  (global (;0;) (mut i32) i32.const 16)
  (export "main" (func 2))
  (func (;2;) (type 3) (result i32)
    (local i32 i32 i32 i32 f32 i32 i64 f64 externref i32 f32 f32 i32 f32 f32 f32 f32 i32 i32 i32 i64 f64)
    global.get 0
    local.tee 17
    i32.const 1048304
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 17
    i32.const 272
    i32.add
    global.set 0
    local.get 17
    i32.const 0
    i32.add
    local.set 0
    local.get 0
    i32.const 0
    i32.const 8
    memory.fill
    local.get 17
    i32.const 8
    i32.add
    local.set 1
    local.get 1
    i32.const 0
    i32.const 8
    memory.fill
    local.get 17
    i32.const 16
    i32.add
    local.set 2
    local.get 2
    i32.const 0
    i32.const 8
    memory.fill
    local.get 17
    i32.const 24
    i32.add
    local.set 3
    local.get 3
    i32.const 0
    i32.const 16
    memory.fill
    i32.const 0
    f32.convert_i32_s
    f64.promote_f32
    i32.const 0
    f32.convert_i32_s
    f64.promote_f32
    f32.demote_f64
    local.set 11
    f32.demote_f64
    local.set 10
    local.get 17
    i32.const 40
    i32.add
    local.tee 9
    i32.const 0
    i32.const 8
    memory.fill
    local.get 9
    local.get 10
    local.get 11
    call 3
    local.get 9
    local.get 17
    i32.const 48
    i32.add
    call 4
    local.set 18
    local.get 0
    local.get 18
    i32.const 8
    memory.copy
    i32.const 3
    f32.convert_i32_s
    f64.promote_f32
    i32.const 4
    f32.convert_i32_s
    f64.promote_f32
    f32.demote_f64
    local.set 11
    f32.demote_f64
    local.set 10
    local.get 17
    i32.const 56
    i32.add
    local.tee 9
    i32.const 0
    i32.const 8
    memory.fill
    local.get 9
    local.get 10
    local.get 11
    call 3
    local.get 9
    local.get 17
    i32.const 64
    i32.add
    call 5
    local.set 18
    local.get 1
    local.get 18
    i32.const 8
    memory.copy
    local.get 0
    local.set 18
    local.get 17
    i32.const 72
    i32.add
    local.get 18
    i32.const 8
    memory.copy
    local.get 17
    i32.const 72
    i32.add
    local.get 1
    local.set 18
    local.get 17
    i32.const 80
    i32.add
    local.get 18
    i32.const 8
    memory.copy
    local.get 17
    i32.const 80
    i32.add
    local.get 17
    i32.const 88
    i32.add
    call 6
    local.set 18
    local.get 0
    local.get 18
    i32.const 8
    memory.copy
    i32.const 120
    f32.convert_i32_s
    f64.promote_f32
    i32.const 80
    f32.convert_i32_s
    f64.promote_f32
    f32.demote_f64
    local.set 11
    f32.demote_f64
    local.set 10
    local.get 17
    i32.const 96
    i32.add
    local.tee 9
    i32.const 0
    i32.const 8
    memory.fill
    local.get 9
    local.get 10
    local.get 11
    call 3
    local.get 9
    local.get 17
    i32.const 104
    i32.add
    call 7
    local.set 18
    local.get 2
    local.get 18
    i32.const 8
    memory.copy
    i32.const 10
    f32.convert_i32_s
    f64.promote_f32
    i32.const 20
    f32.convert_i32_s
    f64.promote_f32
    i32.const 120
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
    local.get 17
    i32.const 112
    i32.add
    local.tee 12
    i32.const 0
    i32.const 16
    memory.fill
    local.get 12
    local.get 13
    local.get 14
    local.get 15
    local.get 16
    call 8
    local.get 12
    local.get 17
    i32.const 128
    i32.add
    call 9
    i32.const 5
    f32.convert_i32_s
    f64.promote_f32
    i32.const 10
    f32.convert_i32_s
    f64.promote_f32
    f32.demote_f64
    local.set 11
    f32.demote_f64
    local.set 10
    local.get 17
    i32.const 144
    i32.add
    local.tee 9
    i32.const 0
    i32.const 8
    memory.fill
    local.get 9
    local.get 10
    local.get 11
    call 3
    local.get 9
    local.get 17
    i32.const 152
    i32.add
    call 10
    local.set 18
    local.get 3
    local.get 18
    i32.const 16
    memory.copy
    local.get 3
    local.set 18
    local.get 17
    i32.const 168
    i32.add
    local.get 18
    i32.const 16
    memory.copy
    local.get 17
    i32.const 168
    i32.add
    local.get 17
    i32.const 184
    i32.add
    call 11
    local.get 0
    local.set 18
    local.get 17
    i32.const 200
    i32.add
    local.get 18
    i32.const 8
    memory.copy
    local.get 17
    i32.const 200
    i32.add
    i32.const 0
    i32.add
    f32.load align=1
    f64.promote_f32
    call 0
    local.get 0
    local.set 18
    local.get 17
    i32.const 208
    i32.add
    local.get 18
    i32.const 8
    memory.copy
    local.get 17
    i32.const 208
    i32.add
    i32.const 4
    i32.add
    f32.load align=1
    f64.promote_f32
    call 0
    local.get 2
    local.set 18
    local.get 17
    i32.const 216
    i32.add
    local.get 18
    i32.const 8
    memory.copy
    local.get 17
    i32.const 216
    i32.add
    i32.const 0
    i32.add
    f32.load align=1
    f64.promote_f32
    call 0
    local.tee 19
    local.get 19
    local.set 18
    local.get 17
    i32.const 224
    i32.add
    local.get 18
    i32.const 16
    memory.copy
    local.get 17
    i32.const 224
    i32.add
    i32.const 0
    i32.add
    f32.load align=1
    f64.promote_f32
    call 0
    local.tee 19
    local.get 19
    local.set 18
    local.get 17
    i32.const 240
    i32.add
    local.get 18
    i32.const 16
    memory.copy
    local.get 17
    i32.const 240
    i32.add
    i32.const 4
    i32.add
    f32.load align=1
    f64.promote_f32
    call 0
    local.tee 19
    local.get 19
    local.set 18
    local.get 17
    i32.const 256
    i32.add
    local.get 18
    i32.const 16
    memory.copy
    local.get 17
    i32.const 256
    i32.add
    i32.const 8
    i32.add
    f32.load align=1
    f64.promote_f32
    call 0
    i32.const 12
    i32.add
    f32.load align=1
    f64.promote_f32
    call 0
    i32.const 0
    local.get 17
    global.set 0
    return
  )
  (func (;3;) (type 4) (param i32 f32 f32)
    (local i32 i32 i32 i64 f64)
    global.get 0
    local.tee 3
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 3
    i32.const 0
    i32.add
    global.set 0
    local.get 0
    local.get 1
    f64.promote_f32
    local.set 7
    i32.const 0
    i32.add
    local.get 7
    f32.demote_f64
    f32.store align=1
    local.get 0
    local.get 2
    f64.promote_f32
    local.set 7
    i32.const 4
    i32.add
    local.get 7
    f32.demote_f64
    f32.store align=1
    local.get 3
    global.set 0
    return
  )
  (func (;4;) (type 5) (param i32 i32) (result i32)
    (local i32 f32 f32 i32 i32 i32 i64 f64)
    global.get 0
    local.tee 5
    i32.const 1048544
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 5
    i32.const 32
    i32.add
    global.set 0
    local.get 5
    i32.const 0
    i32.add
    local.get 0
    i32.const 8
    memory.copy
    local.get 5
    i32.const 0
    i32.add
    local.set 0
    local.get 0
    local.set 6
    local.get 5
    i32.const 8
    i32.add
    local.get 6
    i32.const 8
    memory.copy
    local.get 5
    i32.const 8
    i32.add
    i32.const 0
    i32.add
    f32.load align=1
    f64.promote_f32
    local.get 0
    local.set 6
    local.get 5
    i32.const 16
    i32.add
    local.get 6
    i32.const 8
    memory.copy
    local.get 5
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
    local.get 5
    i32.const 24
    i32.add
    local.tee 2
    i32.const 0
    i32.const 8
    memory.fill
    local.get 2
    local.get 3
    local.get 4
    call 12
    local.get 2
    local.set 6
    local.get 1
    local.get 6
    i32.const 8
    memory.copy
    local.get 1
    local.get 5
    global.set 0
    return
  )
  (func (;5;) (type 6) (param i32 i32) (result i32)
    (local i32 f32 f32 i32 i32 i32 i64 f64)
    global.get 0
    local.tee 5
    i32.const 1048544
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 5
    i32.const 32
    i32.add
    global.set 0
    local.get 5
    i32.const 0
    i32.add
    local.get 0
    i32.const 8
    memory.copy
    local.get 5
    i32.const 0
    i32.add
    local.set 0
    local.get 0
    local.set 6
    local.get 5
    i32.const 8
    i32.add
    local.get 6
    i32.const 8
    memory.copy
    local.get 5
    i32.const 8
    i32.add
    i32.const 0
    i32.add
    f32.load align=1
    f64.promote_f32
    local.get 0
    local.set 6
    local.get 5
    i32.const 16
    i32.add
    local.get 6
    i32.const 8
    memory.copy
    local.get 5
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
    local.get 5
    i32.const 24
    i32.add
    local.tee 2
    i32.const 0
    i32.const 8
    memory.fill
    local.get 2
    local.get 3
    local.get 4
    call 13
    local.get 2
    local.set 6
    local.get 1
    local.get 6
    i32.const 8
    memory.copy
    local.get 1
    local.get 5
    global.set 0
    return
  )
  (func (;6;) (type 7) (param i32 i32 i32) (result i32)
    (local f32 i32 f32 f32 i32 i32 i32 i64 f64)
    global.get 0
    local.tee 7
    i32.const 1048512
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 7
    i32.const 64
    i32.add
    global.set 0
    local.get 7
    i32.const 0
    i32.add
    local.get 0
    i32.const 8
    memory.copy
    local.get 7
    i32.const 0
    i32.add
    local.set 0
    local.get 7
    i32.const 8
    i32.add
    local.get 1
    i32.const 8
    memory.copy
    local.get 7
    i32.const 8
    i32.add
    local.set 1
    local.get 0
    local.set 8
    local.get 7
    i32.const 16
    i32.add
    local.get 8
    i32.const 8
    memory.copy
    local.get 7
    i32.const 16
    i32.add
    i32.const 0
    i32.add
    f32.load align=1
    f64.promote_f32
    local.get 1
    local.set 8
    local.get 7
    i32.const 24
    i32.add
    local.get 8
    i32.const 8
    memory.copy
    local.get 7
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
    local.set 8
    local.get 7
    i32.const 32
    i32.add
    local.get 8
    i32.const 8
    memory.copy
    local.get 7
    i32.const 32
    i32.add
    i32.const 4
    i32.add
    f32.load align=1
    f64.promote_f32
    local.get 1
    local.set 8
    local.get 7
    i32.const 40
    i32.add
    local.get 8
    i32.const 8
    memory.copy
    local.get 7
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
    local.get 7
    i32.const 48
    i32.add
    local.tee 4
    i32.const 0
    i32.const 8
    memory.fill
    local.get 4
    local.get 5
    local.get 6
    call 12
    local.get 4
    local.set 8
    local.get 2
    local.get 8
    i32.const 8
    memory.copy
    local.get 2
    local.get 7
    global.set 0
    return
  )
  (func (;7;) (type 8) (param i32 i32) (result i32)
    (local i32 f32 f32 i32 i32 i32 i64 f64)
    global.get 0
    local.tee 5
    i32.const 1048544
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 5
    i32.const 32
    i32.add
    global.set 0
    local.get 5
    i32.const 0
    i32.add
    local.get 0
    i32.const 8
    memory.copy
    local.get 5
    i32.const 0
    i32.add
    local.set 0
    local.get 0
    local.set 6
    local.get 5
    i32.const 8
    i32.add
    local.get 6
    i32.const 8
    memory.copy
    local.get 5
    i32.const 8
    i32.add
    i32.const 0
    i32.add
    f32.load align=1
    f64.promote_f32
    local.get 0
    local.set 6
    local.get 5
    i32.const 16
    i32.add
    local.get 6
    i32.const 8
    memory.copy
    local.get 5
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
    local.get 5
    i32.const 24
    i32.add
    local.tee 2
    i32.const 0
    i32.const 8
    memory.fill
    local.get 2
    local.get 3
    local.get 4
    call 14
    local.get 2
    local.set 6
    local.get 1
    local.get 6
    i32.const 8
    memory.copy
    local.get 1
    local.get 5
    global.set 0
    return
  )
  (func (;8;) (type 9) (param i32 f32 f32 f32 f32)
    (local i32 i32 i32 i64 f64)
    global.get 0
    local.tee 5
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 5
    i32.const 0
    i32.add
    global.set 0
    local.get 0
    local.get 1
    f64.promote_f32
    local.set 9
    i32.const 0
    i32.add
    local.get 9
    f32.demote_f64
    f32.store align=1
    local.get 0
    local.get 2
    f64.promote_f32
    local.set 9
    i32.const 4
    i32.add
    local.get 9
    f32.demote_f64
    f32.store align=1
    local.get 0
    local.get 3
    f64.promote_f32
    local.set 9
    i32.const 8
    i32.add
    local.get 9
    f32.demote_f64
    f32.store align=1
    local.get 0
    local.get 4
    f64.promote_f32
    local.set 9
    i32.const 12
    i32.add
    local.get 9
    f32.demote_f64
    f32.store align=1
    local.get 5
    global.set 0
    return
  )
  (func (;9;) (type 10) (param i32 i32) (result i32)
    (local i32 f32 f32 f32 f32 i32 i32 i32 i64 f64)
    global.get 0
    local.tee 7
    i32.const 1048480
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 7
    i32.const 96
    i32.add
    global.set 0
    local.get 7
    i32.const 0
    i32.add
    local.get 0
    i32.const 16
    memory.copy
    local.get 7
    i32.const 0
    i32.add
    local.set 0
    local.get 0
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
    i32.const 0
    i32.add
    f32.load align=1
    f64.promote_f32
    local.get 0
    local.set 8
    local.get 7
    i32.const 32
    i32.add
    local.get 8
    i32.const 16
    memory.copy
    local.get 7
    i32.const 32
    i32.add
    i32.const 4
    i32.add
    f32.load align=1
    f64.promote_f32
    local.get 0
    local.set 8
    local.get 7
    i32.const 48
    i32.add
    local.get 8
    i32.const 16
    memory.copy
    local.get 7
    i32.const 48
    i32.add
    i32.const 8
    i32.add
    f32.load align=1
    f64.promote_f32
    local.get 0
    local.set 8
    local.get 7
    i32.const 64
    i32.add
    local.get 8
    i32.const 16
    memory.copy
    local.get 7
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
    local.get 7
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
    call 15
    local.get 2
    local.set 8
    local.get 1
    local.get 8
    i32.const 16
    memory.copy
    local.get 1
    local.get 7
    global.set 0
    return
  )
  (func (;10;) (type 11) (param i32 i32) (result i32)
    (local i32 f32 f32 f32 f32 i32 i32 i32 i64 f64)
    global.get 0
    local.tee 7
    i32.const 1048512
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 7
    i32.const 64
    i32.add
    global.set 0
    local.get 7
    i32.const 0
    i32.add
    local.get 0
    i32.const 8
    memory.copy
    local.get 7
    i32.const 0
    i32.add
    local.set 0
    local.get 0
    local.set 8
    local.get 7
    i32.const 8
    i32.add
    local.get 8
    i32.const 8
    memory.copy
    local.get 7
    i32.const 8
    i32.add
    i32.const 0
    i32.add
    f32.load align=1
    f64.promote_f32
    local.get 0
    local.set 8
    local.get 7
    i32.const 16
    i32.add
    local.get 8
    i32.const 8
    memory.copy
    local.get 7
    i32.const 16
    i32.add
    i32.const 4
    i32.add
    f32.load align=1
    f64.promote_f32
    local.get 0
    local.set 8
    local.get 7
    i32.const 24
    i32.add
    local.get 8
    i32.const 8
    memory.copy
    local.get 7
    i32.const 24
    i32.add
    i32.const 0
    i32.add
    f32.load align=1
    f64.promote_f32
    local.get 0
    local.set 8
    local.get 7
    i32.const 32
    i32.add
    local.get 8
    i32.const 8
    memory.copy
    local.get 7
    i32.const 32
    i32.add
    i32.const 4
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
    local.get 7
    i32.const 40
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
    call 16
    local.get 2
    local.set 8
    local.get 1
    local.get 8
    i32.const 16
    memory.copy
    local.get 1
    local.get 7
    global.set 0
    return
  )
  (func (;11;) (type 12) (param i32 i32 i32) (result i32)
    (local f32 i32 f32 f32 f32 f32 i32 i32 i32 i64 f64)
    global.get 0
    local.tee 9
    i32.const 1048368
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 9
    i32.const 208
    i32.add
    global.set 0
    local.get 9
    i32.const 0
    i32.add
    local.get 0
    i32.const 16
    memory.copy
    local.get 9
    i32.const 0
    i32.add
    local.set 0
    local.get 9
    i32.const 16
    i32.add
    local.get 1
    i32.const 16
    memory.copy
    local.get 9
    i32.const 16
    i32.add
    local.set 1
    local.get 0
    local.set 10
    local.get 9
    i32.const 32
    i32.add
    local.get 10
    i32.const 16
    memory.copy
    local.get 9
    i32.const 32
    i32.add
    i32.const 0
    i32.add
    f32.load align=1
    f64.promote_f32
    local.get 1
    local.set 10
    local.get 9
    i32.const 48
    i32.add
    local.get 10
    i32.const 16
    memory.copy
    local.get 9
    i32.const 48
    i32.add
    i32.const 12
    i32.add
    f32.load align=1
    f64.promote_f32
    f32.demote_f64
    local.set 3
    f32.demote_f64
    local.get 3
    f32.sub
    f64.promote_f32
    local.get 0
    local.set 10
    local.get 9
    i32.const 64
    i32.add
    local.get 10
    i32.const 16
    memory.copy
    local.get 9
    i32.const 64
    i32.add
    i32.const 4
    i32.add
    f32.load align=1
    f64.promote_f32
    local.get 1
    local.set 10
    local.get 9
    i32.const 80
    i32.add
    local.get 10
    i32.const 16
    memory.copy
    local.get 9
    i32.const 80
    i32.add
    i32.const 0
    i32.add
    f32.load align=1
    f64.promote_f32
    f32.demote_f64
    local.set 3
    f32.demote_f64
    local.get 3
    f32.sub
    f64.promote_f32
    local.get 0
    local.set 10
    local.get 9
    i32.const 96
    i32.add
    local.get 10
    i32.const 16
    memory.copy
    local.get 9
    i32.const 96
    i32.add
    i32.const 8
    i32.add
    f32.load align=1
    f64.promote_f32
    local.get 1
    local.set 10
    local.get 9
    i32.const 112
    i32.add
    local.get 10
    i32.const 16
    memory.copy
    local.get 9
    i32.const 112
    i32.add
    i32.const 12
    i32.add
    f32.load align=1
    f64.promote_f32
    f32.demote_f64
    local.set 3
    f32.demote_f64
    local.get 3
    f32.add
    f64.promote_f32
    local.get 1
    local.set 10
    local.get 9
    i32.const 128
    i32.add
    local.get 10
    i32.const 16
    memory.copy
    local.get 9
    i32.const 128
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
    local.get 0
    local.set 10
    local.get 9
    i32.const 144
    i32.add
    local.get 10
    i32.const 16
    memory.copy
    local.get 9
    i32.const 144
    i32.add
    i32.const 12
    i32.add
    f32.load align=1
    f64.promote_f32
    local.get 1
    local.set 10
    local.get 9
    i32.const 160
    i32.add
    local.get 10
    i32.const 16
    memory.copy
    local.get 9
    i32.const 160
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
    local.get 1
    local.set 10
    local.get 9
    i32.const 176
    i32.add
    local.get 10
    i32.const 16
    memory.copy
    local.get 9
    i32.const 176
    i32.add
    i32.const 8
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
    local.set 8
    f32.demote_f64
    local.set 7
    f32.demote_f64
    local.set 6
    f32.demote_f64
    local.set 5
    local.get 9
    i32.const 192
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
    call 15
    local.get 4
    local.set 10
    local.get 2
    local.get 10
    i32.const 16
    memory.copy
    local.get 2
    local.get 9
    global.set 0
    return
  )
  (func (;12;) (type 13) (param i32 f32 f32)
    (local i32 i32 i32 i64 f64)
    global.get 0
    local.tee 3
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 3
    i32.const 0
    i32.add
    global.set 0
    local.get 0
    local.get 1
    f64.promote_f32
    local.set 7
    i32.const 0
    i32.add
    local.get 7
    f32.demote_f64
    f32.store align=1
    local.get 0
    local.get 2
    f64.promote_f32
    local.set 7
    i32.const 4
    i32.add
    local.get 7
    f32.demote_f64
    f32.store align=1
    local.get 3
    global.set 0
    return
  )
  (func (;13;) (type 14) (param i32 f32 f32)
    (local i32 i32 i32 i64 f64)
    global.get 0
    local.tee 3
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 3
    i32.const 0
    i32.add
    global.set 0
    local.get 0
    local.get 1
    f64.promote_f32
    local.set 7
    i32.const 0
    i32.add
    local.get 7
    f32.demote_f64
    f32.store align=1
    local.get 0
    local.get 2
    f64.promote_f32
    local.set 7
    i32.const 4
    i32.add
    local.get 7
    f32.demote_f64
    f32.store align=1
    local.get 3
    global.set 0
    return
  )
  (func (;14;) (type 15) (param i32 f32 f32)
    (local i32 i32 i32 i64 f64)
    global.get 0
    local.tee 3
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 3
    i32.const 0
    i32.add
    global.set 0
    local.get 0
    local.get 1
    f64.promote_f32
    local.set 7
    i32.const 0
    i32.add
    local.get 7
    f32.demote_f64
    f32.store align=1
    local.get 0
    local.get 2
    f64.promote_f32
    local.set 7
    i32.const 4
    i32.add
    local.get 7
    f32.demote_f64
    f32.store align=1
    local.get 3
    global.set 0
    return
  )
  (func (;15;) (type 16) (param i32 f32 f32 f32 f32)
    (local i32 i32 i32 i64 f64)
    global.get 0
    local.tee 5
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 5
    i32.const 0
    i32.add
    global.set 0
    local.get 0
    local.get 1
    f64.promote_f32
    local.set 9
    i32.const 0
    i32.add
    local.get 9
    f32.demote_f64
    f32.store align=1
    local.get 0
    local.get 2
    f64.promote_f32
    local.set 9
    i32.const 4
    i32.add
    local.get 9
    f32.demote_f64
    f32.store align=1
    local.get 0
    local.get 3
    f64.promote_f32
    local.set 9
    i32.const 8
    i32.add
    local.get 9
    f32.demote_f64
    f32.store align=1
    local.get 0
    local.get 4
    f64.promote_f32
    local.set 9
    i32.const 12
    i32.add
    local.get 9
    f32.demote_f64
    f32.store align=1
    local.get 5
    global.set 0
    return
  )
  (func (;16;) (type 17) (param i32 f32 f32 f32 f32)
    (local i32 i32 i32 i64 f64)
    global.get 0
    local.tee 5
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 5
    i32.const 0
    i32.add
    global.set 0
    local.get 0
    local.get 1
    f64.promote_f32
    local.set 9
    i32.const 0
    i32.add
    local.get 9
    f32.demote_f64
    f32.store align=1
    local.get 0
    local.get 2
    f64.promote_f32
    local.set 9
    i32.const 4
    i32.add
    local.get 9
    f32.demote_f64
    f32.store align=1
    local.get 0
    local.get 3
    f64.promote_f32
    local.set 9
    i32.const 8
    i32.add
    local.get 9
    f32.demote_f64
    f32.store align=1
    local.get 0
    local.get 4
    f64.promote_f32
    local.set 9
    i32.const 12
    i32.add
    local.get 9
    f32.demote_f64
    f32.store align=1
    local.get 5
    global.set 0
    return
  )
)
```
