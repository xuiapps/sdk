# Framework tuples in WASM

Source: [Values.cs](Fixtures/Tuples/Values.cs)

## Source

```csharp
using System;

public static class Program
{
    static (int X, double Y) Move((int X, double Y) value) => (value.X + 20, value.Y + 22);
    static (int X, double Y) Change(ref (int X, double Y) value) { value.X = 9; return value; }
    static (string Name, int Count) Rename((string Name, int Count) value) { value.Name = "copy"; return value; }
    public static int Main()
    {
        var pair = (X: 1, Y: 2.5);
        var moved = Move(pair);
        var (x, y) = moved;
        Console.WriteLine(x);
        Console.WriteLine(y);
        Console.WriteLine(pair.X);
        var nested = (pair, moved, Enabled: true);
        Console.WriteLine(nested.Item2.X);
        var copy = Change(ref pair);
        Console.WriteLine(copy.X);
        var named = (Name: "original", Count: 42);
        var renamed = Rename(named);
        Console.WriteLine(named.Name);
        Console.WriteLine(renamed.Name);
        return named.Count;
    }
}
```

## Output

```text
21
24.5
1
21
9
original
copy
```

## Exit Code

```text
42
```

## WAT

```wat
(module
  (type (;0;) (array (mut i16)))
  (type (;1;) (func (param (ref null 0) i32 i32) (result (ref extern))))
  (type (;2;) (func (param i32)))
  (type (;3;) (func (param f64)))
  (type (;4;) (func (param externref)))
  (type (;5;) (struct (field (mut externref)) (field (mut i32))))
  (type (;6;) (func (result i32)))
  (type (;7;) (func (param i32 i32 f64)))
  (type (;8;) (func (param i32 i32) (result i32)))
  (type (;9;) (func (param i32 i32 i32 i32)))
  (type (;10;) (func (param i32 i32) (result i32)))
  (type (;11;) (func (param eqref externref i32)))
  (type (;12;) (func (param eqref) (result eqref)))
  (import "xui" "console-log" (func (;0;) (type 2) (param i32)))
  (import "xui" "console-log" (func (;1;) (type 3) (param f64)))
  (import "xui" "console-log" (func (;2;) (type 4) (param externref)))
  (import "wasm:js-string" "fromCharCodeArray" (func (;3;) (type 1) (param (ref null 0) i32 i32) (result (ref extern))))
  (memory (;0;) 16 16)
  (global (;0;) (mut externref) ref.null extern)
  (global (;1;) (mut externref) ref.null extern)
  (global (;2;) (mut i32) i32.const 16)
  (export "main" (func 4))
  (func (;4;) (type 6) (result i32)
    (local i32 i32 i32 eqref i32 i64 f64 externref i32 i32 f64 i32 i32 i32 i32 eqref externref i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 2
    local.tee 18
    i32.const 1048352
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 18
    i32.const 224
    i32.add
    global.set 2
    local.get 18
    i32.const 0
    i32.add
    local.set 0
    local.get 0
    i32.const 0
    i32.const 16
    memory.fill
    local.get 18
    i32.const 16
    i32.add
    local.set 1
    local.get 1
    i32.const 0
    i32.const 16
    memory.fill
    struct.new_default 5
    local.set 3
    local.get 0
    i32.const 1
    f64.const 0x1.4p+1 (;=2.5;)
    local.set 10
    local.set 9
    local.set 8
    local.get 8
    local.get 9
    local.get 10
    call 5
    local.get 0
    local.set 19
    local.get 18
    i32.const 32
    i32.add
    local.get 19
    i32.const 16
    memory.copy
    local.get 18
    i32.const 32
    i32.add
    local.get 18
    i32.const 48
    i32.add
    call 6
    local.set 19
    local.get 1
    local.get 19
    i32.const 16
    memory.copy
    local.get 1
    local.set 19
    local.get 18
    i32.const 64
    i32.add
    local.get 19
    i32.const 16
    memory.copy
    local.get 18
    i32.const 64
    i32.add
    local.tee 20
    local.get 20
    local.set 19
    local.get 18
    i32.const 80
    i32.add
    local.get 19
    i32.const 16
    memory.copy
    local.get 18
    i32.const 80
    i32.add
    i32.const 0
    i32.add
    i32.load align=1
    local.set 2
    i32.const 8
    i32.add
    f64.load align=1
    local.get 2
    call 0
    call 1
    local.get 0
    local.set 19
    local.get 18
    i32.const 96
    i32.add
    local.get 19
    i32.const 16
    memory.copy
    local.get 18
    i32.const 96
    i32.add
    i32.const 0
    i32.add
    i32.load align=1
    call 0
    local.get 0
    local.set 19
    local.get 18
    i32.const 112
    i32.add
    local.get 19
    i32.const 16
    memory.copy
    local.get 18
    i32.const 112
    i32.add
    local.get 1
    local.set 19
    local.get 18
    i32.const 128
    i32.add
    local.get 19
    i32.const 16
    memory.copy
    local.get 18
    i32.const 128
    i32.add
    i32.const 1
    i32.const 255
    i32.and
    local.set 14
    local.set 13
    local.set 12
    local.get 18
    i32.const 144
    i32.add
    local.tee 11
    i32.const 0
    i32.const 40
    memory.fill
    local.get 11
    local.get 12
    local.get 13
    local.get 14
    call 7
    local.get 11
    i32.const 16
    i32.add
    local.set 19
    local.get 18
    i32.const 184
    i32.add
    local.get 19
    i32.const 16
    memory.copy
    local.get 18
    i32.const 184
    i32.add
    i32.const 0
    i32.add
    i32.load align=1
    call 0
    local.get 0
    local.get 18
    i32.const 200
    i32.add
    call 8
    i32.const 0
    i32.add
    i32.load align=1
    call 0
    global.get 0
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 8
      array.new_data 0 0
      i32.const 0
      i32.const 8
      call 3
      global.set 0
    end
    global.get 0
    i32.const 42
    local.set 17
    local.set 16
    struct.new_default 5
    local.set 15
    local.get 15
    local.get 16
    local.get 17
    call 9
    local.get 15
    local.tee 23
    local.get 23
    ref.cast (ref 5)
    local.set 23
    local.get 23
    ref.cast (ref 5)
    struct.get 5 0
    local.get 23
    ref.cast (ref 5)
    struct.get 5 1
    struct.new 5
    call 10
    local.set 23
    local.get 3
    local.set 24
    local.get 24
    ref.cast (ref 5)
    local.get 23
    ref.cast (ref 5)
    struct.get 5 0
    struct.set 5 0
    local.get 24
    ref.cast (ref 5)
    local.get 23
    ref.cast (ref 5)
    struct.get 5 1
    struct.set 5 1
    local.tee 23
    local.get 23
    ref.cast (ref 5)
    local.set 23
    local.get 23
    ref.cast (ref 5)
    struct.get 5 0
    local.get 23
    ref.cast (ref 5)
    struct.get 5 1
    struct.new 5
    ref.cast (ref 5)
    struct.get 5 0
    call 2
    local.get 3
    ref.cast (ref 5)
    local.set 23
    local.get 23
    ref.cast (ref 5)
    struct.get 5 0
    local.get 23
    ref.cast (ref 5)
    struct.get 5 1
    struct.new 5
    ref.cast (ref 5)
    struct.get 5 0
    call 2
    ref.cast (ref 5)
    struct.get 5 1
    local.get 18
    global.set 2
    return
  )
  (func (;5;) (type 7) (param i32 i32 f64)
    (local i32 i32 i32 i64 f64)
    global.get 2
    local.tee 3
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 3
    i32.const 0
    i32.add
    global.set 2
    local.get 0
    local.get 1
    local.set 5
    i32.const 0
    i32.add
    local.get 5
    i32.store align=1
    local.get 0
    local.get 2
    local.set 7
    i32.const 8
    i32.add
    local.get 7
    f64.store align=1
    local.get 3
    global.set 2
    return
  )
  (func (;6;) (type 8) (param i32 i32) (result i32)
    (local i32 i32 f64 i32 i32 i32 i64 f64)
    global.get 2
    local.tee 5
    i32.const 1048512
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 5
    i32.const 64
    i32.add
    global.set 2
    local.get 5
    i32.const 0
    i32.add
    local.get 0
    i32.const 16
    memory.copy
    local.get 5
    i32.const 0
    i32.add
    local.set 0
    local.get 0
    local.set 6
    local.get 5
    i32.const 16
    i32.add
    local.get 6
    i32.const 16
    memory.copy
    local.get 5
    i32.const 16
    i32.add
    i32.const 0
    i32.add
    i32.load align=1
    i32.const 20
    i32.add
    local.get 0
    local.set 6
    local.get 5
    i32.const 32
    i32.add
    local.get 6
    i32.const 16
    memory.copy
    local.get 5
    i32.const 32
    i32.add
    i32.const 8
    i32.add
    f64.load align=1
    f64.const 0x1.6p+4 (;=22;)
    f64.add
    local.set 4
    local.set 3
    local.get 5
    i32.const 48
    i32.add
    local.tee 2
    i32.const 0
    i32.const 16
    memory.fill
    local.get 2
    local.get 3
    local.get 4
    call 5
    local.get 2
    local.set 6
    local.get 1
    local.get 6
    i32.const 16
    memory.copy
    local.get 1
    local.get 5
    global.set 2
    return
  )
  (func (;7;) (type 9) (param i32 i32 i32 i32)
    (local i32 i32 i32 i64 f64)
    global.get 2
    local.tee 4
    i32.const 1048512
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 4
    i32.const 64
    i32.add
    global.set 2
    local.get 4
    i32.const 0
    i32.add
    local.get 1
    i32.const 16
    memory.copy
    local.get 4
    i32.const 0
    i32.add
    local.set 1
    local.get 4
    i32.const 16
    i32.add
    local.get 2
    i32.const 16
    memory.copy
    local.get 4
    i32.const 16
    i32.add
    local.set 2
    local.get 0
    local.get 1
    local.set 5
    local.get 4
    i32.const 32
    i32.add
    local.get 5
    i32.const 16
    memory.copy
    local.get 4
    i32.const 32
    i32.add
    local.set 6
    i32.const 0
    i32.add
    local.get 6
    i32.const 16
    memory.copy
    local.get 0
    local.get 2
    local.set 5
    local.get 4
    i32.const 48
    i32.add
    local.get 5
    i32.const 16
    memory.copy
    local.get 4
    i32.const 48
    i32.add
    local.set 6
    i32.const 16
    i32.add
    local.get 6
    i32.const 16
    memory.copy
    local.get 0
    local.get 3
    local.set 6
    i32.const 32
    i32.add
    local.get 6
    i32.store8
    local.get 4
    global.set 2
    return
  )
  (func (;8;) (type 10) (param i32 i32) (result i32)
    (local i32 i32 i32 i64 f64)
    global.get 2
    local.tee 2
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 2
    i32.const 16
    i32.add
    global.set 2
    local.get 0
    i32.const 9
    local.set 4
    i32.const 0
    i32.add
    local.get 4
    i32.store align=1
    local.get 0
    local.set 3
    local.get 2
    i32.const 0
    i32.add
    local.get 3
    i32.const 16
    memory.copy
    local.get 2
    i32.const 0
    i32.add
    local.set 3
    local.get 1
    local.get 3
    i32.const 16
    memory.copy
    local.get 1
    local.get 2
    global.set 2
    return
  )
  (func (;9;) (type 11) (param eqref externref i32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.get 1
    local.set 8
    ref.cast (ref 5)
    local.set 4
    local.get 4
    ref.cast (ref 5)
    local.get 8
    struct.set 5 0
    local.get 0
    local.get 2
    local.set 5
    ref.cast (ref 5)
    local.set 4
    local.get 4
    ref.cast (ref 5)
    local.get 5
    struct.set 5 1
    return
  )
  (func (;10;) (type 12) (param eqref) (result eqref)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    global.get 1
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 4
      array.new_data 0 1
      i32.const 0
      i32.const 4
      call 3
      global.set 1
    end
    global.get 1
    local.set 6
    ref.cast (ref 5)
    local.set 2
    local.get 2
    ref.cast (ref 5)
    local.get 6
    struct.set 5 0
    local.get 0
    ref.cast (ref 5)
    local.set 1
    local.get 1
    ref.cast (ref 5)
    struct.get 5 0
    local.get 1
    ref.cast (ref 5)
    struct.get 5 1
    struct.new 5
    return
  )
  (data (;0;) "o\00r\00i\00g\00i\00n\00a\00l\00")
  (data (;1;) "c\00o\00p\00y\00")
)
```
