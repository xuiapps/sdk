# Unit with a string and inline Point

Source: [Unit.cs](Fixtures/Gc/Unit.cs)

## Source

```csharp
using System;

public struct Point
{
    public int X;
    public int Y;
}

public class Unit
{
    string name;
    Point position;

    public Unit(string name, int x, int y)
    {
        this.name = name;
        position.X = x;
        position.Y = y;
    }

    public void MoveTo(int x, int y)
    {
        position.X = x;
        position.Y = y;
    }

    public static int Main()
    {
        var unit = new Unit("Scout", 1, 2);
        var alias = unit;
        alias.MoveTo(4, 6);

        var copy = unit.position;
        copy.X = 20;
        Console.WriteLine(unit.name);
        Console.WriteLine(unit.position.X);
        Console.WriteLine(unit.position.Y);
        Console.WriteLine(copy.X);

        unit.position = copy;
        copy.X = 99;
        Console.WriteLine(alias.position.X);
        Console.WriteLine(unit == alias);
        return 0;
    }
}
```

## Output

```text
Scout
4
6
20
20
True
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
  (type (;2;) (func (param externref)))
  (type (;3;) (func (param i32)))
  (type (;4;) (func (param i32)))
  (type (;5;) (struct (field i32) (field externref)))
  (type (;6;) (func (param eqref)))
  (type (;7;) (func (result eqref)))
  (type (;8;) (func (result eqref exnref)))
  (type (;9;) (func (result exnref)))
  (type (;10;) (func))
  (type (;11;) (struct (field i32) (field eqref) (field i32) (field (ref null 0)) (field i32) (field i32)))
  (type (;12;) (sub (struct (field (mut eqref)))))
  (type (;13;) (func (param eqref eqref) (result eqref)))
  (type (;14;) (sub 12 (struct (field (mut eqref)) (field (mut externref)) (field (mut i32)) (field (mut i32)))))
  (type (;15;) (struct (field (mut i32)) (field (mut i32))))
  (type (;16;) (func (result i32)))
  (type (;17;) (func (param eqref externref i32 i32)))
  (type (;18;) (func (param eqref i32 i32)))
  (import "xui" "console-log" (func (;0;) (type 2) (param externref)))
  (import "xui" "console-log" (func (;1;) (type 3) (param i32)))
  (import "xui" "console-log-boolean" (func (;2;) (type 4) (param i32)))
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
  (global (;8;) eqref i32.const 1 ref.null eq i32.const 0 i32.const 85 i32.const 110 i32.const 105 i32.const 116 array.new_fixed 0 4 i32.const 0 i32.const 4 struct.new 11)
  (export "main" (func 4))
  (func (;4;) (type 16) (result i32)
    (local eqref i32 i32 i64 f64 externref eqref externref i32 i32 eqref i32 i32 exnref i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 7
    local.tee 14
    i32.const 1048544
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 14
    i32.const 32
    i32.add
    global.set 7
    local.get 14
    i32.const 0
    i32.add
    local.set 1
    local.get 1
    i32.const 0
    i32.const 8
    memory.fill
    block (type 9) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        global.get 0
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 5
          array.new_data 0 0
          i32.const 0
          i32.const 5
          call 3
          global.set 0
        end
        global.get 0
        i32.const 1
        i32.const 2
        local.set 9
        local.set 8
        local.set 7
        struct.new_default 14
        local.set 6
        local.get 6
        ref.cast (ref 14)
        global.get 8
        struct.set 14 0
        local.get 6
        local.get 7
        local.get 8
        local.get 9
        call 5
        local.get 6
        local.tee 19
        local.get 19
        local.set 0
        local.get 0
        i32.const 4
        i32.const 6
        local.set 12
        local.set 11
        local.set 10
        local.get 10
        local.tee 20
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
        local.get 20
        ref.cast (ref 14)
        local.get 11
        local.get 12
        call 6
        local.tee 19
        local.get 19
        local.tee 20
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
        local.get 20
        ref.cast (ref 14)
        local.set 20
        local.get 14
        i32.const 8
        i32.add
        local.get 20
        ref.cast (ref 14)
        struct.get 14 2
        i32.store align=1
        local.get 14
        i32.const 12
        i32.add
        local.get 20
        ref.cast (ref 14)
        struct.get 14 3
        i32.store align=1
        local.get 14
        i32.const 8
        i32.add
        local.set 15
        local.get 1
        local.get 15
        i32.const 8
        memory.copy
        local.get 1
        i32.const 20
        local.set 16
        i32.const 0
        i32.add
        local.get 16
        i32.store align=1
        local.tee 19
        local.get 19
        local.tee 20
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
        local.get 20
        ref.cast (ref 14)
        struct.get 14 1
        call 0
        local.tee 19
        local.get 19
        local.tee 20
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
        local.get 20
        ref.cast (ref 14)
        ref.cast (ref 14)
        struct.get 14 2
        call 1
        local.tee 19
        local.get 19
        local.tee 20
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
        local.get 20
        ref.cast (ref 14)
        ref.cast (ref 14)
        struct.get 14 3
        call 1
        local.get 1
        local.set 15
        local.get 14
        i32.const 16
        i32.add
        local.get 15
        i32.const 8
        memory.copy
        local.get 14
        i32.const 16
        i32.add
        i32.const 0
        i32.add
        i32.load align=1
        call 1
        local.tee 19
        local.get 19
        local.get 1
        local.set 15
        local.get 14
        i32.const 24
        i32.add
        local.get 15
        i32.const 8
        memory.copy
        local.get 14
        i32.const 24
        i32.add
        local.set 21
        local.tee 20
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
        local.get 20
        ref.cast (ref 14)
        local.set 20
        local.get 20
        ref.cast (ref 14)
        local.get 21
        i32.const 0
        i32.add
        i32.load align=1
        struct.set 14 2
        local.get 20
        ref.cast (ref 14)
        local.get 21
        i32.const 4
        i32.add
        i32.load align=1
        struct.set 14 3
        local.get 1
        i32.const 99
        local.set 16
        i32.const 0
        i32.add
        local.get 16
        i32.store align=1
        local.get 0
        local.tee 20
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
        local.get 20
        ref.cast (ref 14)
        ref.cast (ref 14)
        struct.get 14 2
        call 1
        local.get 0
        ref.eq
        call 2
        i32.const 0
        local.get 14
        global.set 7
        return
        unreachable
      end
      unreachable
    end
    local.set 13
    local.get 14
    global.set 7
    local.get 13
    throw_ref
  )
  (func (;5;) (type 17) (param eqref externref i32 i32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    drop
    local.get 0
    local.get 1
    local.set 9
    local.tee 5
    ref.is_null
    if ;; label = @1
      i32.const 4
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
      global.get 4
      struct.new 5
      throw 0
    end
    local.get 5
    ref.cast (ref 14)
    local.set 5
    local.get 5
    ref.cast (ref 14)
    local.get 9
    struct.set 14 1
    local.get 0
    local.tee 5
    ref.is_null
    if ;; label = @1
      i32.const 4
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
      global.get 4
      struct.new 5
      throw 0
    end
    local.get 5
    ref.cast (ref 14)
    local.get 2
    local.set 6
    ref.cast (ref 14)
    local.set 5
    local.get 5
    ref.cast (ref 14)
    local.get 6
    struct.set 14 2
    local.get 0
    local.tee 5
    ref.is_null
    if ;; label = @1
      i32.const 4
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
      global.get 4
      struct.new 5
      throw 0
    end
    local.get 5
    ref.cast (ref 14)
    local.get 3
    local.set 6
    ref.cast (ref 14)
    local.set 5
    local.get 5
    ref.cast (ref 14)
    local.get 6
    struct.set 14 3
    return
  )
  (func (;6;) (type 18) (param eqref i32 i32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.tee 4
    ref.is_null
    if ;; label = @1
      i32.const 4
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
      global.get 4
      struct.new 5
      throw 0
    end
    local.get 4
    ref.cast (ref 14)
    local.get 1
    local.set 5
    ref.cast (ref 14)
    local.set 4
    local.get 4
    ref.cast (ref 14)
    local.get 5
    struct.set 14 2
    local.get 0
    local.tee 4
    ref.is_null
    if ;; label = @1
      i32.const 4
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
      global.get 4
      struct.new 5
      throw 0
    end
    local.get 4
    ref.cast (ref 14)
    local.get 2
    local.set 5
    ref.cast (ref 14)
    local.set 4
    local.get 4
    ref.cast (ref 14)
    local.get 5
    struct.set 14 3
    return
  )
  (func (;7;) (type 13) (param eqref eqref) (result eqref)
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
  (data (;0;) "S\00c\00o\00u\00t\00")
  (data (;1;) "E\00x\00c\00e\00p\00t\00i\00o\00n\00 \00o\00f\00 \00t\00y\00p\00e\00 \00'\00S\00y\00s\00t\00e\00m\00.\00E\00x\00c\00e\00p\00t\00i\00o\00n\00'\00 \00w\00a\00s\00 \00t\00h\00r\00o\00w\00n\00.\00")
  (data (;2;) "S\00y\00s\00t\00e\00m\00 \00e\00r\00r\00o\00r\00.\00")
  (data (;3;) "S\00p\00e\00c\00i\00f\00i\00e\00d\00 \00c\00a\00s\00t\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00.\00")
  (data (;4;) "O\00b\00j\00e\00c\00t\00 \00r\00e\00f\00e\00r\00e\00n\00c\00e\00 \00n\00o\00t\00 \00s\00e\00t\00 \00t\00o\00 \00a\00n\00 \00i\00n\00s\00t\00a\00n\00c\00e\00 \00o\00f\00 \00a\00n\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;5;) "O\00p\00e\00r\00a\00t\00i\00o\00n\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00 \00d\00u\00e\00 \00t\00o\00 \00t\00h\00e\00 \00c\00u\00r\00r\00e\00n\00t\00 \00s\00t\00a\00t\00e\00 \00o\00f\00 \00t\00h\00e\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;6;) "V\00a\00l\00u\00e\00 \00d\00o\00e\00s\00 \00n\00o\00t\00 \00f\00a\00l\00l\00 \00w\00i\00t\00h\00i\00n\00 \00t\00h\00e\00 \00e\00x\00p\00e\00c\00t\00e\00d\00 \00r\00a\00n\00g\00e\00.\00")
)
```
