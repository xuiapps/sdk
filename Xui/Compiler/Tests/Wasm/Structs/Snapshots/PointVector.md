# Point + Vector WASM

Source: [PointVector.cs](Fixtures/Structs/PointVector.cs)

## Source

```csharp
using nfloat = System.Runtime.InteropServices.NFloat;

public struct Point
{
    public nfloat X;
    public nfloat Y;
    public Point(nfloat x, nfloat y) { X = x; Y = y; }
    public static Point operator +(Point p, Vector v) => new(p.X + v.X, p.Y + v.Y);
}
public struct Vector
{
    public nfloat X;
    public nfloat Y;
    public Vector(nfloat x, nfloat y) { X = x; Y = y; }
}
public static class Program
{
    public static int Main()
    {
        var p = new Point(1, 2);
        var v = new Vector(3, 4);
        var result = p + v;
        System.Console.WriteLine((double)result.X);
        System.Console.WriteLine((double)result.Y);
        return 0;
    }
}
```

## CLR output

```text
4
6
```

## CLR exit code

```text
0
```

## WASM output

```text
4
6
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
  (type (;5;) (func (param i32 f32 f32)))
  (type (;6;) (func (param i32 i32 i32) (result i32)))
  (import "xui" "console-log" (func (;0;) (type 2) (param f64)))
  (import "wasm:js-string" "fromCharCodeArray" (func (;1;) (type 1) (param (ref null 0) i32 i32) (result (ref extern))))
  (memory (;0;) 16 16)
  (global (;0;) (mut i32) i32.const 16)
  (export "main" (func 2))
  (func (;2;) (type 3) (result i32)
    (local i32 f32 i32 i64 f64 externref i32 f32 f32 i32 f32 f32 i32 i32 i32 i64 f64)
    global.get 0
    local.tee 12
    i32.const 1048528
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 12
    i32.const 48
    i32.add
    global.set 0
    local.get 12
    i32.const 0
    i32.add
    local.set 0
    local.get 0
    i32.const 0
    i32.const 8
    memory.fill
    i32.const 1
    f32.convert_i32_s
    f64.promote_f32
    i32.const 2
    f32.convert_i32_s
    f64.promote_f32
    f32.demote_f64
    local.set 8
    f32.demote_f64
    local.set 7
    local.get 12
    i32.const 8
    i32.add
    local.tee 6
    i32.const 0
    i32.const 8
    memory.fill
    local.get 6
    local.get 7
    local.get 8
    call 3
    local.get 6
    local.get 0
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
    local.set 9
    local.get 9
    local.get 10
    local.get 11
    call 4
    local.get 0
    local.set 13
    local.get 12
    i32.const 16
    i32.add
    local.get 13
    i32.const 8
    memory.copy
    local.get 12
    i32.const 16
    i32.add
    local.get 12
    i32.const 24
    i32.add
    call 5
    local.tee 14
    local.get 14
    local.set 13
    local.get 12
    i32.const 32
    i32.add
    local.get 13
    i32.const 8
    memory.copy
    local.get 12
    i32.const 32
    i32.add
    i32.const 0
    i32.add
    f32.load align=1
    f64.promote_f32
    call 0
    i32.const 4
    i32.add
    f32.load align=1
    f64.promote_f32
    call 0
    i32.const 0
    local.get 12
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
  (func (;4;) (type 5) (param i32 f32 f32)
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
  (func (;5;) (type 6) (param i32 i32 i32) (result i32)
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
    call 3
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
)
```

## Status

Passed.
