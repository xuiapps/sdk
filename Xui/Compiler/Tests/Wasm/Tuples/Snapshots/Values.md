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
  (type (;5;) (struct (field i32) (field externref)))
  (type (;6;) (func (param eqref)))
  (type (;7;) (func (result eqref)))
  (type (;8;) (func (result eqref exnref)))
  (type (;9;) (func (result exnref)))
  (type (;10;) (func))
  (type (;11;) (struct (field (mut i32)) (field (mut f64))))
  (type (;12;) (struct (field (mut externref)) (field (mut i32))))
  (type (;13;) (struct (field (mut i32)) (field (mut f64)) (field (mut i32)) (field (mut f64)) (field (mut i32))))
  (type (;14;) (func (result i32)))
  (type (;15;) (func (param eqref i32) (result eqref)))
  (type (;16;) (func (param eqref i32 eqref)))
  (type (;17;) (func (param eqref i32) (result externref)))
  (type (;18;) (func (param eqref i32 externref)))
  (type (;19;) (func (param eqref i32) (result i32)))
  (type (;20;) (func (param eqref i32 i32)))
  (type (;21;) (func (param eqref i32) (result f64)))
  (type (;22;) (func (param eqref i32 f64)))
  (type (;23;) (func (param eqref i32) (result i32)))
  (type (;24;) (func (param eqref i32 i32)))
  (type (;25;) (func (result i32)))
  (type (;26;) (func (param eqref i32 f64 i32)))
  (type (;27;) (func (param i32 i32) (result i32)))
  (type (;28;) (func (param eqref i32 i32 i32 i32)))
  (type (;29;) (func (param eqref i32 i32) (result i32)))
  (type (;30;) (func (param eqref externref i32 i32)))
  (type (;31;) (func (param i32 i32) (result i32)))
  (import "xui" "console-log" (func (;0;) (type 2) (param i32)))
  (import "xui" "console-log" (func (;1;) (type 3) (param f64)))
  (import "xui" "console-log" (func (;2;) (type 4) (param externref)))
  (import "wasm:js-string" "fromCharCodeArray" (func (;3;) (type 1) (param (ref null 0) i32 i32) (result (ref extern))))
  (table (;0;) 65536 65536 eqref)
  (table (;1;) 65536 65536 externref)
  (memory (;0;) 16 32768)
  (tag (;0;) (type 6) (param eqref))
  (global (;0;) (mut externref) ref.null extern)
  (global (;1;) (mut externref) ref.null extern)
  (global (;2;) (mut externref) ref.null extern)
  (global (;3;) (mut externref) ref.null extern)
  (global (;4;) (mut externref) ref.null extern)
  (global (;5;) (mut externref) ref.null extern)
  (global (;6;) (mut externref) ref.null extern)
  (global (;7;) (mut externref) ref.null extern)
  (global (;8;) (mut i32) i32.const 16)
  (global (;9;) (mut i32) i32.const 0)
  (export "main" (func 4))
  (func (;4;) (type 25) (result i32)
    (local i32 i32 i32 i32 eqref i32 eqref i32 i32 i64 f64 externref eqref i32 f64 i32 eqref i32 i32 i32 i32 eqref i32 eqref externref i32 i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 8
    local.tee 32
    i32.const 1048304
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 32
    i32.const 272
    i32.add
    global.set 8
    local.get 32
    i32.const 0
    i32.add
    local.set 0
    local.get 0
    i32.const 0
    i32.const 16
    memory.fill
    local.get 32
    i32.const 16
    i32.add
    local.set 1
    local.get 1
    i32.const 0
    i32.const 16
    memory.fill
    local.get 32
    i32.const 32
    i32.add
    local.set 3
    local.get 3
    i32.const 0
    i32.const 8
    memory.fill
    global.get 9
    local.tee 29
    i32.const 65530
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 29
    i32.const 6
    i32.add
    global.set 9
    local.get 32
    i32.const 32
    i32.add
    local.get 29
    i32.const 0
    i32.add
    i32.store align=1
    local.get 32
    i32.const 224
    i32.add
    local.get 29
    i32.const 1
    i32.add
    i32.store align=1
    local.get 32
    i32.const 232
    i32.add
    local.get 29
    i32.const 2
    i32.add
    i32.store align=1
    local.get 32
    i32.const 240
    i32.add
    local.get 29
    i32.const 3
    i32.add
    i32.store align=1
    local.get 32
    i32.const 248
    i32.add
    local.get 29
    i32.const 4
    i32.add
    i32.store align=1
    local.get 32
    i32.const 256
    i32.add
    local.get 29
    i32.const 5
    i32.add
    i32.store align=1
    block (type 9) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.set 5
        ref.null eq
        local.get 5
        i32.const 1
        f64.const 0x1.4p+1 (;=2.5;)
        local.set 14
        local.set 13
        local.set 15
        local.set 12
        local.get 12
        local.get 13
        local.get 14
        local.get 15
        call 5
        local.get 0
        local.set 33
        local.get 32
        i32.const 40
        i32.add
        local.get 33
        i32.const 16
        memory.copy
        local.get 32
        i32.const 40
        i32.add
        local.get 32
        i32.const 56
        i32.add
        call 6
        local.set 33
        local.get 1
        local.get 33
        i32.const 16
        memory.copy
        local.get 1
        local.set 33
        local.get 32
        i32.const 72
        i32.add
        local.get 33
        i32.const 16
        memory.copy
        local.get 32
        i32.const 72
        i32.add
        local.tee 34
        local.get 34
        local.set 33
        local.get 32
        i32.const 88
        i32.add
        local.get 33
        i32.const 16
        memory.copy
        local.get 32
        i32.const 88
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
        local.set 33
        local.get 32
        i32.const 104
        i32.add
        local.get 33
        i32.const 16
        memory.copy
        local.get 32
        i32.const 104
        i32.add
        i32.const 0
        i32.add
        i32.load align=1
        call 0
        local.get 0
        local.set 33
        local.get 32
        i32.const 120
        i32.add
        local.get 33
        i32.const 16
        memory.copy
        local.get 32
        i32.const 120
        i32.add
        local.get 1
        local.set 33
        local.get 32
        i32.const 136
        i32.add
        local.get 33
        i32.const 16
        memory.copy
        local.get 32
        i32.const 136
        i32.add
        i32.const 1
        i32.const 255
        i32.and
        local.set 19
        local.set 18
        local.set 17
        local.get 32
        i32.const 152
        i32.add
        i32.const 0
        i32.const 40
        memory.fill
        ref.null eq
        local.set 16
        local.get 32
        i32.const 152
        i32.add
        local.set 20
        local.get 16
        local.get 17
        local.get 18
        local.get 19
        local.get 20
        call 7
        local.get 32
        i32.const 152
        i32.add
        i32.const 16
        i32.add
        local.set 33
        local.get 32
        i32.const 192
        i32.add
        local.get 33
        i32.const 16
        memory.copy
        local.get 32
        i32.const 192
        i32.add
        i32.const 0
        i32.add
        i32.load align=1
        call 0
        local.get 0
        local.set 5
        ref.null eq
        local.get 5
        local.set 22
        local.set 21
        local.get 21
        local.get 22
        local.get 32
        i32.const 208
        i32.add
        call 8
        i32.const 0
        i32.add
        i32.load align=1
        call 0
        global.get 0
        ref.is_null
        if ;; label = @3
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
        local.set 25
        local.set 24
        local.get 32
        i32.const 224
        i32.add
        local.set 31
        local.get 31
        i32.const 0
        i32.add
        ref.null extern
        local.set 42
        i32.load align=1
        local.get 42
        table.set 1
        local.get 31
        i32.const 4
        i32.add
        i32.const 0
        i32.store align=1
        ref.null eq
        local.set 23
        local.get 32
        i32.const 224
        i32.add
        local.set 26
        local.get 23
        local.get 24
        local.get 25
        local.get 26
        call 9
        local.get 32
        i32.const 224
        i32.add
        local.tee 34
        local.get 34
        local.set 33
        local.get 32
        i32.const 232
        i32.add
        local.get 33
        local.set 30
        local.set 31
        local.get 31
        i32.const 0
        i32.add
        local.get 30
        i32.const 0
        i32.add
        i32.load align=1
        table.get 1
        local.set 42
        i32.load align=1
        local.get 42
        table.set 1
        local.get 31
        i32.const 4
        i32.add
        local.get 30
        i32.const 4
        i32.add
        i32.load align=1
        i32.store align=1
        local.get 32
        i32.const 232
        i32.add
        local.get 32
        i32.const 240
        i32.add
        call 10
        local.set 33
        local.get 3
        local.get 33
        local.set 30
        local.set 31
        local.get 31
        i32.const 0
        i32.add
        local.get 30
        i32.const 0
        i32.add
        i32.load align=1
        table.get 1
        local.set 42
        i32.load align=1
        local.get 42
        table.set 1
        local.get 31
        i32.const 4
        i32.add
        local.get 30
        i32.const 4
        i32.add
        i32.load align=1
        i32.store align=1
        local.tee 34
        local.get 34
        local.set 33
        local.get 32
        i32.const 248
        i32.add
        local.get 33
        local.set 30
        local.set 31
        local.get 31
        i32.const 0
        i32.add
        local.get 30
        i32.const 0
        i32.add
        i32.load align=1
        table.get 1
        local.set 42
        i32.load align=1
        local.get 42
        table.set 1
        local.get 31
        i32.const 4
        i32.add
        local.get 30
        i32.const 4
        i32.add
        i32.load align=1
        i32.store align=1
        local.get 32
        i32.const 248
        i32.add
        i32.const 0
        i32.add
        i32.load align=1
        table.get 1
        call 2
        local.get 3
        local.set 33
        local.get 32
        i32.const 256
        i32.add
        local.get 33
        local.set 30
        local.set 31
        local.get 31
        i32.const 0
        i32.add
        local.get 30
        i32.const 0
        i32.add
        i32.load align=1
        table.get 1
        local.set 42
        i32.load align=1
        local.get 42
        table.set 1
        local.get 31
        i32.const 4
        i32.add
        local.get 30
        i32.const 4
        i32.add
        i32.load align=1
        i32.store align=1
        local.get 32
        i32.const 256
        i32.add
        i32.const 0
        i32.add
        i32.load align=1
        table.get 1
        call 2
        i32.const 4
        i32.add
        i32.load align=1
        local.get 29
        ref.null eq
        i32.const 6
        table.fill 0
        local.get 29
        ref.null extern
        i32.const 6
        table.fill 1
        local.get 29
        global.set 9
        local.get 32
        global.set 8
        return
        unreachable
      end
      unreachable
    end
    local.set 27
    local.get 29
    ref.null eq
    i32.const 6
    table.fill 0
    local.get 29
    ref.null extern
    i32.const 6
    table.fill 1
    local.get 29
    global.set 9
    local.get 32
    global.set 8
    local.get 27
    throw_ref
  )
  (func (;5;) (type 26) (param eqref i32 f64 i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 8
    local.tee 13
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 13
    i32.const 0
    i32.add
    global.set 8
    block (type 9) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 3
        local.get 1
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
          global.get 5
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 5
            i32.const 0
            i32.const 53
            call 3
            global.set 5
          end
          global.get 5
          struct.new 5
          throw 0
        end
        local.get 4
        local.get 4
        ref.is_null
        if (type 14) (result i32) ;; label = @3
          local.get 5
          i32.const 0
          i32.add
        else
          local.get 5
          i32.const 0
          i32.add
        end
        local.get 20
        call 16
        local.get 0
        local.get 3
        local.get 2
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
          global.get 5
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 5
            i32.const 0
            i32.const 53
            call 3
            global.set 5
          end
          global.get 5
          struct.new 5
          throw 0
        end
        local.get 4
        local.get 4
        ref.is_null
        if (type 14) (result i32) ;; label = @3
          local.get 5
          i32.const 8
          i32.add
        else
          local.get 5
          i32.const 1
          i32.add
        end
        local.get 22
        call 18
        local.get 13
        global.set 8
        return
        unreachable
      end
      unreachable
    end
    local.set 8
    local.get 13
    global.set 8
    local.get 8
    throw_ref
  )
  (func (;6;) (type 27) (param i32 i32) (result i32)
    (local eqref i32 eqref i32 eqref i32 f64 i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 8
    local.tee 15
    i32.const 1048512
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 15
    i32.const 64
    i32.add
    global.set 8
    local.get 15
    i32.const 0
    i32.add
    local.get 0
    i32.const 16
    memory.copy
    local.get 15
    i32.const 0
    i32.add
    local.set 0
    block (type 9) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.set 16
        local.get 15
        i32.const 16
        i32.add
        local.get 16
        i32.const 16
        memory.copy
        local.get 15
        i32.const 16
        i32.add
        i32.const 0
        i32.add
        i32.load align=1
        i32.const 20
        i32.add
        local.get 0
        local.set 16
        local.get 15
        i32.const 32
        i32.add
        local.get 16
        i32.const 16
        memory.copy
        local.get 15
        i32.const 32
        i32.add
        i32.const 8
        i32.add
        f64.load align=1
        f64.const 0x1.6p+4 (;=22;)
        f64.add
        local.set 8
        local.set 7
        local.get 15
        i32.const 48
        i32.add
        i32.const 0
        i32.const 16
        memory.fill
        ref.null eq
        local.set 6
        local.get 15
        i32.const 48
        i32.add
        local.set 9
        local.get 6
        local.get 7
        local.get 8
        local.get 9
        call 5
        local.get 15
        i32.const 48
        i32.add
        local.set 16
        local.get 1
        local.get 16
        i32.const 16
        memory.copy
        local.get 1
        local.get 15
        global.set 8
        return
        unreachable
      end
      unreachable
    end
    local.set 10
    local.get 15
    global.set 8
    local.get 10
    throw_ref
  )
  (func (;7;) (type 28) (param eqref i32 i32 i32 i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 8
    local.tee 14
    i32.const 1048512
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 14
    i32.const 64
    i32.add
    global.set 8
    local.get 14
    i32.const 0
    i32.add
    local.get 1
    i32.const 16
    memory.copy
    local.get 14
    i32.const 0
    i32.add
    local.set 1
    local.get 14
    i32.const 16
    i32.add
    local.get 2
    i32.const 16
    memory.copy
    local.get 14
    i32.const 16
    i32.add
    local.set 2
    block (type 9) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 4
        local.get 1
        local.set 15
        local.get 14
        i32.const 32
        i32.add
        local.get 15
        i32.const 16
        memory.copy
        local.get 14
        i32.const 32
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
          global.get 5
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 5
            i32.const 0
            i32.const 53
            call 3
            global.set 5
          end
          global.get 5
          struct.new 5
          throw 0
        end
        local.get 5
        local.get 5
        ref.is_null
        if (type 14) (result i32) ;; label = @3
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
          global.get 5
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 5
            i32.const 0
            i32.const 53
            call 3
            global.set 5
          end
          global.get 5
          struct.new 5
          throw 0
        end
        local.get 5
        local.get 5
        ref.is_null
        if (type 14) (result i32) ;; label = @3
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
        i32.load align=1
        call 16
        local.get 5
        ref.is_null
        local.get 6
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 5
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 5
            i32.const 0
            i32.const 53
            call 3
            global.set 5
          end
          global.get 5
          struct.new 5
          throw 0
        end
        local.get 5
        local.get 5
        ref.is_null
        if (type 14) (result i32) ;; label = @3
          local.get 6
          i32.const 8
          i32.add
        else
          local.get 6
          i32.const 1
          i32.add
        end
        local.get 21
        i32.const 8
        i32.add
        f64.load align=1
        call 18
        local.get 0
        local.get 4
        local.get 2
        local.set 15
        local.get 14
        i32.const 48
        i32.add
        local.get 15
        i32.const 16
        memory.copy
        local.get 14
        i32.const 48
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
          global.get 5
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 5
            i32.const 0
            i32.const 53
            call 3
            global.set 5
          end
          global.get 5
          struct.new 5
          throw 0
        end
        local.get 5
        local.get 5
        ref.is_null
        if (type 14) (result i32) ;; label = @3
          local.get 6
          i32.const 16
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
          global.get 5
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 5
            i32.const 0
            i32.const 53
            call 3
            global.set 5
          end
          global.get 5
          struct.new 5
          throw 0
        end
        local.get 5
        local.get 5
        ref.is_null
        if (type 14) (result i32) ;; label = @3
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
        i32.load align=1
        call 16
        local.get 5
        ref.is_null
        local.get 6
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 5
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 5
            i32.const 0
            i32.const 53
            call 3
            global.set 5
          end
          global.get 5
          struct.new 5
          throw 0
        end
        local.get 5
        local.get 5
        ref.is_null
        if (type 14) (result i32) ;; label = @3
          local.get 6
          i32.const 8
          i32.add
        else
          local.get 6
          i32.const 1
          i32.add
        end
        local.get 21
        i32.const 8
        i32.add
        f64.load align=1
        call 18
        local.get 0
        local.get 4
        local.get 3
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
          global.get 5
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 5
            i32.const 0
            i32.const 53
            call 3
            global.set 5
          end
          global.get 5
          struct.new 5
          throw 0
        end
        local.get 5
        local.get 5
        ref.is_null
        if (type 14) (result i32) ;; label = @3
          local.get 6
          i32.const 32
          i32.add
        else
          local.get 6
          i32.const 4
          i32.add
        end
        local.get 21
        i32.const 255
        i32.and
        call 20
        local.get 14
        global.set 8
        return
        unreachable
      end
      unreachable
    end
    local.set 9
    local.get 14
    global.set 8
    local.get 9
    throw_ref
  )
  (func (;8;) (type 29) (param eqref i32 i32) (result i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 8
    local.tee 12
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 12
    i32.const 16
    i32.add
    global.set 8
    block (type 9) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        i32.const 9
        local.set 19
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
          global.get 5
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 5
            i32.const 0
            i32.const 53
            call 3
            global.set 5
          end
          global.get 5
          struct.new 5
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 14) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
        else
          local.get 4
          i32.const 0
          i32.add
        end
        local.get 19
        call 16
        local.get 0
        local.get 1
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
          global.get 5
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 5
            i32.const 0
            i32.const 53
            call 3
            global.set 5
          end
          global.get 5
          struct.new 5
          throw 0
        end
        local.get 12
        i32.const 0
        i32.add
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 5
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 5
            i32.const 0
            i32.const 53
            call 3
            global.set 5
          end
          global.get 5
          struct.new 5
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 14) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
        else
          local.get 4
          i32.const 0
          i32.add
        end
        call 15
        i32.store align=1
        local.get 12
        i32.const 8
        i32.add
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 5
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 5
            i32.const 0
            i32.const 53
            call 3
            global.set 5
          end
          global.get 5
          struct.new 5
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 14) (result i32) ;; label = @3
          local.get 4
          i32.const 8
          i32.add
        else
          local.get 4
          i32.const 1
          i32.add
        end
        call 17
        f64.store align=1
        local.get 12
        i32.const 0
        i32.add
        local.set 13
        local.get 2
        local.get 13
        i32.const 16
        memory.copy
        local.get 2
        local.get 12
        global.set 8
        return
        unreachable
      end
      unreachable
    end
    local.set 7
    local.get 12
    global.set 8
    local.get 7
    throw_ref
  )
  (func (;9;) (type 30) (param eqref externref i32 i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 8
    local.tee 13
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 13
    i32.const 0
    i32.add
    global.set 8
    block (type 9) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 3
        local.get 1
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
          global.get 5
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 5
            i32.const 0
            i32.const 53
            call 3
            global.set 5
          end
          global.get 5
          struct.new 5
          throw 0
        end
        local.get 4
        local.get 4
        ref.is_null
        if (type 14) (result i32) ;; label = @3
          local.get 5
          i32.const 0
          i32.add
          i32.load align=1
        else
          local.get 5
          i32.const 0
          i32.add
        end
        local.get 23
        call 14
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
          global.get 5
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 5
            i32.const 0
            i32.const 53
            call 3
            global.set 5
          end
          global.get 5
          struct.new 5
          throw 0
        end
        local.get 4
        local.get 4
        ref.is_null
        if (type 14) (result i32) ;; label = @3
          local.get 5
          i32.const 4
          i32.add
        else
          local.get 5
          i32.const 1
          i32.add
        end
        local.get 20
        call 16
        local.get 13
        global.set 8
        return
        unreachable
      end
      unreachable
    end
    local.set 8
    local.get 13
    global.set 8
    local.get 8
    throw_ref
  )
  (func (;10;) (type 31) (param i32 i32) (result i32)
    (local eqref i32 eqref i32 exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 8
    local.tee 11
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 11
    i32.const 16
    i32.add
    global.set 8
    global.get 9
    local.tee 8
    i32.const 65534
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 8
    i32.const 2
    i32.add
    global.set 9
    local.get 11
    i32.const 0
    i32.add
    local.get 8
    i32.const 0
    i32.add
    i32.store align=1
    local.get 11
    i32.const 8
    i32.add
    local.get 8
    i32.const 1
    i32.add
    i32.store align=1
    local.get 11
    i32.const 0
    i32.add
    local.get 0
    local.set 9
    local.set 10
    local.get 10
    i32.const 0
    i32.add
    local.get 9
    i32.const 0
    i32.add
    i32.load align=1
    table.get 1
    local.set 21
    i32.load align=1
    local.get 21
    table.set 1
    local.get 10
    i32.const 4
    i32.add
    local.get 9
    i32.const 4
    i32.add
    i32.load align=1
    i32.store align=1
    local.get 11
    i32.const 0
    i32.add
    local.set 0
    block (type 9) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.set 3
        ref.null eq
        local.get 3
        global.get 1
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 4
          array.new_data 0 1
          i32.const 0
          i32.const 4
          call 3
          global.set 1
        end
        global.get 1
        local.set 21
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
          global.get 5
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 5
            i32.const 0
            i32.const 53
            call 3
            global.set 5
          end
          global.get 5
          struct.new 5
          throw 0
        end
        local.get 2
        local.get 2
        ref.is_null
        if (type 14) (result i32) ;; label = @3
          local.get 3
          i32.const 0
          i32.add
          i32.load align=1
        else
          local.get 3
          i32.const 0
          i32.add
        end
        local.get 21
        call 14
        local.get 0
        local.set 12
        local.get 11
        i32.const 8
        i32.add
        local.get 12
        local.set 9
        local.set 10
        local.get 10
        i32.const 0
        i32.add
        local.get 9
        i32.const 0
        i32.add
        i32.load align=1
        table.get 1
        local.set 21
        i32.load align=1
        local.get 21
        table.set 1
        local.get 10
        i32.const 4
        i32.add
        local.get 9
        i32.const 4
        i32.add
        i32.load align=1
        i32.store align=1
        local.get 11
        i32.const 8
        i32.add
        local.set 12
        local.get 1
        local.get 12
        local.set 9
        local.set 10
        local.get 10
        i32.const 0
        i32.add
        local.get 9
        i32.const 0
        i32.add
        i32.load align=1
        table.get 1
        local.set 21
        i32.load align=1
        local.get 21
        table.set 1
        local.get 10
        i32.const 4
        i32.add
        local.get 9
        i32.const 4
        i32.add
        i32.load align=1
        i32.store align=1
        local.get 1
        local.get 8
        ref.null eq
        i32.const 2
        table.fill 0
        local.get 8
        ref.null extern
        i32.const 2
        table.fill 1
        local.get 8
        global.set 9
        local.get 11
        global.set 8
        return
        unreachable
      end
      unreachable
    end
    local.set 6
    local.get 8
    ref.null eq
    i32.const 2
    table.fill 0
    local.get 8
    ref.null extern
    i32.const 2
    table.fill 1
    local.get 8
    global.set 9
    local.get 11
    global.set 8
    local.get 6
    throw_ref
  )
  (func (;11;) (type 15) (param eqref i32) (result eqref)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 5
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 5
        i32.const 0
        i32.const 53
        call 3
        global.set 5
      end
      i32.const 4
      global.get 5
      struct.new 5
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
  (func (;12;) (type 16) (param eqref i32 eqref)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 5
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 5
        i32.const 0
        i32.const 53
        call 3
        global.set 5
      end
      i32.const 4
      global.get 5
      struct.new 5
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
  (func (;13;) (type 17) (param eqref i32) (result externref)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 5
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 5
        i32.const 0
        i32.const 53
        call 3
        global.set 5
      end
      i32.const 4
      global.get 5
      struct.new 5
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
  (func (;14;) (type 18) (param eqref i32 externref)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 5
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 5
        i32.const 0
        i32.const 53
        call 3
        global.set 5
      end
      i32.const 4
      global.get 5
      struct.new 5
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
  (func (;15;) (type 19) (param eqref i32) (result i32)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 5
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 5
        i32.const 0
        i32.const 53
        call 3
        global.set 5
      end
      i32.const 4
      global.get 5
      struct.new 5
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
  (func (;16;) (type 20) (param eqref i32 i32)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 5
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 5
        i32.const 0
        i32.const 53
        call 3
        global.set 5
      end
      i32.const 4
      global.get 5
      struct.new 5
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
  (func (;17;) (type 21) (param eqref i32) (result f64)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 5
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 5
        i32.const 0
        i32.const 53
        call 3
        global.set 5
      end
      i32.const 4
      global.get 5
      struct.new 5
      throw 0
    end
    local.get 0
    ref.is_null
    if ;; label = @1
      local.get 1
      f64.load align=1
      return
    end
    unreachable
  )
  (func (;18;) (type 22) (param eqref i32 f64)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 5
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 5
        i32.const 0
        i32.const 53
        call 3
        global.set 5
      end
      i32.const 4
      global.get 5
      struct.new 5
      throw 0
    end
    local.get 0
    ref.is_null
    if ;; label = @1
      local.get 1
      local.get 2
      f64.store align=1
      return
    end
    unreachable
  )
  (func (;19;) (type 23) (param eqref i32) (result i32)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 5
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 5
        i32.const 0
        i32.const 53
        call 3
        global.set 5
      end
      i32.const 4
      global.get 5
      struct.new 5
      throw 0
    end
    local.get 0
    ref.is_null
    if ;; label = @1
      local.get 1
      i32.load8_u
      return
    end
    unreachable
  )
  (func (;20;) (type 24) (param eqref i32 i32)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 5
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 5
        i32.const 0
        i32.const 53
        call 3
        global.set 5
      end
      i32.const 4
      global.get 5
      struct.new 5
      throw 0
    end
    local.get 0
    ref.is_null
    if ;; label = @1
      local.get 1
      local.get 2
      i32.store8
      return
    end
    unreachable
  )
  (data (;0;) "o\00r\00i\00g\00i\00n\00a\00l\00")
  (data (;1;) "c\00o\00p\00y\00")
  (data (;2;) "E\00x\00c\00e\00p\00t\00i\00o\00n\00 \00o\00f\00 \00t\00y\00p\00e\00 \00'\00S\00y\00s\00t\00e\00m\00.\00E\00x\00c\00e\00p\00t\00i\00o\00n\00'\00 \00w\00a\00s\00 \00t\00h\00r\00o\00w\00n\00.\00")
  (data (;3;) "S\00y\00s\00t\00e\00m\00 \00e\00r\00r\00o\00r\00.\00")
  (data (;4;) "S\00p\00e\00c\00i\00f\00i\00e\00d\00 \00c\00a\00s\00t\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00.\00")
  (data (;5;) "O\00b\00j\00e\00c\00t\00 \00r\00e\00f\00e\00r\00e\00n\00c\00e\00 \00n\00o\00t\00 \00s\00e\00t\00 \00t\00o\00 \00a\00n\00 \00i\00n\00s\00t\00a\00n\00c\00e\00 \00o\00f\00 \00a\00n\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;6;) "O\00p\00e\00r\00a\00t\00i\00o\00n\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00 \00d\00u\00e\00 \00t\00o\00 \00t\00h\00e\00 \00c\00u\00r\00r\00e\00n\00t\00 \00s\00t\00a\00t\00e\00 \00o\00f\00 \00t\00h\00e\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;7;) "V\00a\00l\00u\00e\00 \00d\00o\00e\00s\00 \00n\00o\00t\00 \00f\00a\00l\00l\00 \00w\00i\00t\00h\00i\00n\00 \00t\00h\00e\00 \00e\00x\00p\00e\00c\00t\00e\00d\00 \00r\00a\00n\00g\00e\00.\00")
)
```
