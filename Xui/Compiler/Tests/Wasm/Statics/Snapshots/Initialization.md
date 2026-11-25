# Static fields and type initialization

Source: [Initialization.cs](Fixtures/Statics/Initialization.cs)

## Source

```csharp
using System;

public struct Point
{
    public int X, Y;
}

public static class Settings
{
    public static readonly Point Origin = new Point { X = 20, Y = 22 };
    public static string Name = "Xui";

    static Settings()
    {
        Console.WriteLine("Settings initialized");
    }
}

public static class Program
{
    public static int Main()
    {
        Console.WriteLine("Main");
        Console.WriteLine(Settings.Name);
        Point copy = Settings.Origin;
        copy.X = 99;
        Console.WriteLine(Settings.Origin.X);
        return Settings.Origin.X + Settings.Origin.Y;
    }
}
```

## Output

```text
Main
Settings initialized
Xui
20
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
  (type (;2;) (func (param externref externref) (result (ref extern))))
  (type (;3;) (func (param externref) (result i32)))
  (type (;4;) (func (param externref)))
  (type (;5;) (func (param i32)))
  (type (;6;) (struct (field i32) (field externref) (field externref) (field eqref)))
  (type (;7;) (func (param eqref)))
  (type (;8;) (func (result eqref)))
  (type (;9;) (func (result eqref exnref)))
  (type (;10;) (func (result exnref)))
  (type (;11;) (func))
  (type (;12;) (struct (field (mut i32)) (field (mut i32))))
  (type (;13;) (func))
  (type (;14;) (struct (field (mut externref)) (field (mut i32)) (field (mut i32))))
  (type (;15;) (func (result i32)))
  (type (;16;) (func (param eqref i32) (result eqref)))
  (type (;17;) (func (param eqref i32 eqref)))
  (type (;18;) (func (param eqref i32) (result externref)))
  (type (;19;) (func (param eqref i32 externref)))
  (type (;20;) (func (param eqref i32) (result i32)))
  (type (;21;) (func (param eqref i32 i32)))
  (type (;22;) (func (result i32)))
  (type (;23;) (func))
  (import "xui" "console-log" (func (;0;) (type 4) (param externref)))
  (import "xui" "console-log" (func (;1;) (type 5) (param i32)))
  (import "wasm:js-string" "fromCharCodeArray" (func (;2;) (type 1) (param (ref null 0) i32 i32) (result (ref extern))))
  (import "wasm:js-string" "concat" (func (;3;) (type 2) (param externref externref) (result (ref extern))))
  (import "wasm:js-string" "length" (func (;4;) (type 3) (param externref) (result i32)))
  (memory (;0;) 16 16)
  (tag (;0;) (type 7) (param eqref))
  (global (;0;) (mut externref) ref.null extern)
  (global (;1;) (mut externref) ref.null extern)
  (global (;2;) (mut externref) ref.null extern)
  (global (;3;) (mut externref) ref.null extern)
  (global (;4;) (mut externref) ref.null extern)
  (global (;5;) (mut externref) ref.null extern)
  (global (;6;) (mut externref) ref.null extern)
  (global (;7;) (mut externref) ref.null extern)
  (global (;8;) (mut externref) ref.null extern)
  (global (;9;) (mut externref) ref.null extern)
  (global (;10;) (mut externref) ref.null extern)
  (global (;11;) (mut externref) ref.null extern)
  (global (;12;) (mut externref) ref.null extern)
  (global (;13;) (mut externref) ref.null extern)
  (global (;14;) (mut i32) i32.const 16)
  (global (;15;) eqref struct.new_default 14)
  (global (;16;) (mut i32) i32.const 0)
  (global (;17;) (mut exnref) ref.null exn)
  (export "main" (func 5))
  (func (;5;) (type 22) (result i32)
    (local i32 eqref i32 eqref i32 exnref i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref externref)
    global.get 14
    local.tee 6
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 6
    i32.const 16
    i32.add
    global.set 14
    local.get 6
    i32.const 0
    i32.add
    local.set 0
    local.get 0
    i32.const 0
    i32.const 8
    memory.fill
    block (type 10) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        global.get 0
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 4
          array.new_data 0 0
          i32.const 0
          i32.const 4
          call 2
          global.set 0
        end
        global.get 0
        call 0
        call 13
        global.get 15
        ref.cast (ref 14)
        struct.get 14 0
        call 0
        call 13
        global.get 15
        ref.cast (ref 14)
        local.set 12
        local.get 6
        i32.const 8
        i32.add
        local.get 12
        ref.cast (ref 14)
        struct.get 14 1
        i32.store align=1
        local.get 6
        i32.const 12
        i32.add
        local.get 12
        ref.cast (ref 14)
        struct.get 14 2
        i32.store align=1
        local.get 6
        i32.const 8
        i32.add
        local.set 7
        local.get 0
        local.get 7
        i32.const 8
        memory.copy
        local.get 0
        local.set 2
        ref.null eq
        local.get 2
        i32.const 99
        local.set 13
        local.set 2
        local.set 1
        local.get 1
        ref.is_null
        local.get 2
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 6
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 6
            i32.const 0
            i32.const 53
            call 2
            global.set 6
          end
          global.get 6
          ref.null extern
          ref.null eq
          struct.new 6
          throw 0
        end
        local.get 1
        local.get 1
        ref.is_null
        if (type 15) (result i32) ;; label = @3
          local.get 2
          i32.const 0
          i32.add
        else
          local.get 2
          i32.const 0
          i32.add
        end
        local.get 13
        call 12
        call 13
        global.get 15
        i32.const 1
        local.set 2
        local.set 1
        local.get 1
        ref.is_null
        local.get 2
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 6
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 6
            i32.const 0
            i32.const 53
            call 2
            global.set 6
          end
          global.get 6
          ref.null extern
          ref.null eq
          struct.new 6
          throw 0
        end
        local.get 1
        local.get 1
        ref.is_null
        if (type 15) (result i32) ;; label = @3
          local.get 2
          i32.const 0
          i32.add
        else
          local.get 2
          i32.const 0
          i32.add
        end
        call 11
        call 1
        call 13
        global.get 15
        i32.const 1
        local.set 2
        local.set 1
        local.get 1
        ref.is_null
        local.get 2
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 6
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 6
            i32.const 0
            i32.const 53
            call 2
            global.set 6
          end
          global.get 6
          ref.null extern
          ref.null eq
          struct.new 6
          throw 0
        end
        local.get 1
        local.get 1
        ref.is_null
        if (type 15) (result i32) ;; label = @3
          local.get 2
          i32.const 0
          i32.add
        else
          local.get 2
          i32.const 0
          i32.add
        end
        call 11
        call 13
        global.get 15
        i32.const 1
        local.set 2
        local.set 1
        local.get 1
        ref.is_null
        local.get 2
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 6
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 6
            i32.const 0
            i32.const 53
            call 2
            global.set 6
          end
          global.get 6
          ref.null extern
          ref.null eq
          struct.new 6
          throw 0
        end
        local.get 1
        local.get 1
        ref.is_null
        if (type 15) (result i32) ;; label = @3
          local.get 2
          i32.const 4
          i32.add
        else
          local.get 2
          i32.const 1
          i32.add
        end
        call 11
        i32.add
        local.get 6
        global.set 14
        return
        unreachable
      end
      unreachable
    end
    local.set 5
    local.get 6
    global.set 14
    local.get 5
    throw_ref
  )
  (func (;6;) (type 23)
    (local i32 eqref i32 eqref i32 exnref i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref externref)
    global.get 14
    local.tee 6
    i32.const 1048560
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 6
    i32.const 16
    i32.add
    global.set 14
    local.get 6
    i32.const 0
    i32.add
    local.set 0
    local.get 0
    i32.const 0
    i32.const 8
    memory.fill
    block (type 10) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.set 2
        ref.null eq
        local.get 2
        local.set 2
        local.set 1
        local.get 1
        ref.is_null
        local.get 2
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 6
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 6
            i32.const 0
            i32.const 53
            call 2
            global.set 6
          end
          global.get 6
          ref.null extern
          ref.null eq
          struct.new 6
          throw 0
        end
        local.get 1
        ref.is_null
        local.get 2
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 6
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 6
            i32.const 0
            i32.const 53
            call 2
            global.set 6
          end
          global.get 6
          ref.null extern
          ref.null eq
          struct.new 6
          throw 0
        end
        local.get 1
        local.get 1
        ref.is_null
        if (type 15) (result i32) ;; label = @3
          local.get 2
          i32.const 0
          i32.add
        else
          local.get 2
          i32.const 0
          i32.add
        end
        i32.const 0
        call 12
        local.get 1
        ref.is_null
        local.get 2
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 6
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 6
            i32.const 0
            i32.const 53
            call 2
            global.set 6
          end
          global.get 6
          ref.null extern
          ref.null eq
          struct.new 6
          throw 0
        end
        local.get 1
        local.get 1
        ref.is_null
        if (type 15) (result i32) ;; label = @3
          local.get 2
          i32.const 4
          i32.add
        else
          local.get 2
          i32.const 1
          i32.add
        end
        i32.const 0
        call 12
        local.get 0
        local.set 2
        ref.null eq
        local.get 2
        i32.const 20
        local.set 13
        local.set 2
        local.set 1
        local.get 1
        ref.is_null
        local.get 2
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 6
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 6
            i32.const 0
            i32.const 53
            call 2
            global.set 6
          end
          global.get 6
          ref.null extern
          ref.null eq
          struct.new 6
          throw 0
        end
        local.get 1
        local.get 1
        ref.is_null
        if (type 15) (result i32) ;; label = @3
          local.get 2
          i32.const 0
          i32.add
        else
          local.get 2
          i32.const 0
          i32.add
        end
        local.get 13
        call 12
        local.get 0
        local.set 2
        ref.null eq
        local.get 2
        i32.const 22
        local.set 13
        local.set 2
        local.set 1
        local.get 1
        ref.is_null
        local.get 2
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 6
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 6
            i32.const 0
            i32.const 53
            call 2
            global.set 6
          end
          global.get 6
          ref.null extern
          ref.null eq
          struct.new 6
          throw 0
        end
        local.get 1
        local.get 1
        ref.is_null
        if (type 15) (result i32) ;; label = @3
          local.get 2
          i32.const 4
          i32.add
        else
          local.get 2
          i32.const 1
          i32.add
        end
        local.get 13
        call 12
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
        call 13
        local.set 13
        global.get 15
        local.set 12
        local.get 12
        ref.cast (ref 14)
        local.get 13
        i32.const 0
        i32.add
        i32.load align=1
        struct.set 14 1
        local.get 12
        ref.cast (ref 14)
        local.get 13
        i32.const 4
        i32.add
        i32.load align=1
        struct.set 14 2
        global.get 1
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 3
          array.new_data 0 1
          i32.const 0
          i32.const 3
          call 2
          global.set 1
        end
        global.get 1
        call 13
        local.set 16
        global.get 15
        local.set 12
        local.get 12
        ref.cast (ref 14)
        local.get 16
        struct.set 14 0
        global.get 2
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 20
          array.new_data 0 2
          i32.const 0
          i32.const 20
          call 2
          global.set 2
        end
        global.get 2
        call 0
        local.get 6
        global.set 14
        return
        unreachable
      end
      unreachable
    end
    local.set 5
    local.get 6
    global.set 14
    local.get 5
    throw_ref
  )
  (func (;7;) (type 16) (param eqref i32) (result eqref)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 6
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 6
        i32.const 0
        i32.const 53
        call 2
        global.set 6
      end
      i32.const 4
      global.get 6
      ref.null extern
      ref.null eq
      struct.new 6
      throw 0
    end
    unreachable
  )
  (func (;8;) (type 17) (param eqref i32 eqref)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 6
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 6
        i32.const 0
        i32.const 53
        call 2
        global.set 6
      end
      i32.const 4
      global.get 6
      ref.null extern
      ref.null eq
      struct.new 6
      throw 0
    end
    unreachable
  )
  (func (;9;) (type 18) (param eqref i32) (result externref)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 6
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 6
        i32.const 0
        i32.const 53
        call 2
        global.set 6
      end
      i32.const 4
      global.get 6
      ref.null extern
      ref.null eq
      struct.new 6
      throw 0
    end
    local.get 1
    i32.const 0
    i32.eq
    if ;; label = @1
      local.get 0
      ref.cast (ref 14)
      struct.get 14 0
      return
    end
    unreachable
  )
  (func (;10;) (type 19) (param eqref i32 externref)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 6
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 6
        i32.const 0
        i32.const 53
        call 2
        global.set 6
      end
      i32.const 4
      global.get 6
      ref.null extern
      ref.null eq
      struct.new 6
      throw 0
    end
    local.get 1
    i32.const 0
    i32.eq
    if ;; label = @1
      local.get 0
      ref.cast (ref 14)
      local.get 2
      struct.set 14 0
      return
    end
    unreachable
  )
  (func (;11;) (type 20) (param eqref i32) (result i32)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 6
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 6
        i32.const 0
        i32.const 53
        call 2
        global.set 6
      end
      i32.const 4
      global.get 6
      ref.null extern
      ref.null eq
      struct.new 6
      throw 0
    end
    local.get 0
    ref.is_null
    if ;; label = @1
      local.get 1
      i32.load align=1
      return
    end
    local.get 1
    i32.const 1
    i32.eq
    if ;; label = @1
      local.get 0
      ref.cast (ref 14)
      struct.get 14 1
      return
    end
    local.get 1
    i32.const 2
    i32.eq
    if ;; label = @1
      local.get 0
      ref.cast (ref 14)
      struct.get 14 2
      return
    end
    unreachable
  )
  (func (;12;) (type 21) (param eqref i32 i32)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 6
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 6
        i32.const 0
        i32.const 53
        call 2
        global.set 6
      end
      i32.const 4
      global.get 6
      ref.null extern
      ref.null eq
      struct.new 6
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
    local.get 1
    i32.const 1
    i32.eq
    if ;; label = @1
      local.get 0
      ref.cast (ref 14)
      local.get 2
      struct.set 14 1
      return
    end
    local.get 1
    i32.const 2
    i32.eq
    if ;; label = @1
      local.get 0
      ref.cast (ref 14)
      local.get 2
      struct.set 14 2
      return
    end
    unreachable
  )
  (func (;13;) (type 13)
    (local eqref)
    global.get 16
    i32.const 2
    i32.eq
    if ;; label = @1
      return
    end
    global.get 16
    i32.const 1
    i32.eq
    if ;; label = @1
      return
    end
    global.get 16
    i32.const 3
    i32.eq
    if ;; label = @1
      global.get 17
      throw_ref
    end
    i32.const 1
    global.set 16
    block (type 10) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        block (type 8) (result eqref) ;; label = @3
          try_table (catch 0 0 (;@3;)) ;; label = @4
            call 6
            i32.const 2
            global.set 16
            return
          end
          unreachable
        end
        local.set 0
        i32.const 18
        global.get 13
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 55
          array.new_data 0 13
          i32.const 0
          i32.const 55
          call 2
          global.set 13
        end
        global.get 13
        global.get 12
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 8
          array.new_data 0 12
          i32.const 0
          i32.const 8
          call 2
          global.set 12
        end
        global.get 12
        local.get 0
        struct.new 6
        throw 0
      end
      unreachable
    end
    global.set 17
    i32.const 3
    global.set 16
    global.get 17
    throw_ref
  )
  (data (;0;) "M\00a\00i\00n\00")
  (data (;1;) "X\00u\00i\00")
  (data (;2;) "S\00e\00t\00t\00i\00n\00g\00s\00 \00i\00n\00i\00t\00i\00a\00l\00i\00z\00e\00d\00")
  (data (;3;) "E\00x\00c\00e\00p\00t\00i\00o\00n\00 \00o\00f\00 \00t\00y\00p\00e\00 \00'\00S\00y\00s\00t\00e\00m\00.\00E\00x\00c\00e\00p\00t\00i\00o\00n\00'\00 \00w\00a\00s\00 \00t\00h\00r\00o\00w\00n\00.\00")
  (data (;4;) "S\00y\00s\00t\00e\00m\00 \00e\00r\00r\00o\00r\00.\00")
  (data (;5;) "S\00p\00e\00c\00i\00f\00i\00e\00d\00 \00c\00a\00s\00t\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00.\00")
  (data (;6;) "O\00b\00j\00e\00c\00t\00 \00r\00e\00f\00e\00r\00e\00n\00c\00e\00 \00n\00o\00t\00 \00s\00e\00t\00 \00t\00o\00 \00a\00n\00 \00i\00n\00s\00t\00a\00n\00c\00e\00 \00o\00f\00 \00a\00n\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;7;) "O\00p\00e\00r\00a\00t\00i\00o\00n\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00 \00d\00u\00e\00 \00t\00o\00 \00t\00h\00e\00 \00c\00u\00r\00r\00e\00n\00t\00 \00s\00t\00a\00t\00e\00 \00o\00f\00 \00t\00h\00e\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;8;) "V\00a\00l\00u\00e\00 \00d\00o\00e\00s\00 \00n\00o\00t\00 \00f\00a\00l\00l\00 \00w\00i\00t\00h\00i\00n\00 \00t\00h\00e\00 \00e\00x\00p\00e\00c\00t\00e\00d\00 \00r\00a\00n\00g\00e\00.\00")
  (data (;9;) "T\00h\00e\00 \00t\00y\00p\00e\00 \00i\00n\00i\00t\00i\00a\00l\00i\00z\00e\00r\00 \00t\00h\00r\00e\00w\00 \00a\00n\00 \00e\00x\00c\00e\00p\00t\00i\00o\00n\00.\00")
  (data (;10;) " \00(\00P\00a\00r\00a\00m\00e\00t\00e\00r\00 \00'\00")
  (data (;11;) "'\00)\00")
  (data (;12;) "S\00e\00t\00t\00i\00n\00g\00s\00")
  (data (;13;) "T\00h\00e\00 \00t\00y\00p\00e\00 \00i\00n\00i\00t\00i\00a\00l\00i\00z\00e\00r\00 \00f\00o\00r\00 \00'\00S\00e\00t\00t\00i\00n\00g\00s\00'\00 \00t\00h\00r\00e\00w\00 \00a\00n\00 \00e\00x\00c\00e\00p\00t\00i\00o\00n\00.\00")
)
```
