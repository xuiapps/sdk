# Linear-memory struct copies

Source: [LinearMemory.cs](Fixtures/Structs/LinearMemory.cs)

## Source

```csharp
using System;

public struct Point
{
    public double X;
    public double Y;
}

public struct LabelledPoint
{
    public byte Tag;
    public Point Position;
}

public static class Program
{
    public static int Main()
    {
        LabelledPoint original = default;
        original.Tag = 7;
        original.Position.X = 1;
        original.Position.Y = 2;
        var copy = original;
        original.Position.X = 10;
        copy.Position.Y = 20;
        Console.WriteLine(original.Position.X);
        Console.WriteLine(original.Position.Y);
        Console.WriteLine(copy.Position.X);
        Console.WriteLine(copy.Position.Y);
        return copy.Tag;
    }
}
```

## Output

```text
10
2
1
20
```

## Exit Code

```text
7
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
  (type (;9;) (struct (field (mut i32)) (field (mut f64)) (field (mut f64))))
  (type (;10;) (struct (field (mut f64)) (field (mut f64))))
  (type (;11;) (func (result i32)))
  (type (;12;) (func (param eqref i32) (result eqref)))
  (type (;13;) (func (param eqref i32 eqref)))
  (type (;14;) (func (param eqref i32) (result externref)))
  (type (;15;) (func (param eqref i32 externref)))
  (type (;16;) (func (param eqref i32) (result i32)))
  (type (;17;) (func (param eqref i32 i32)))
  (type (;18;) (func (param eqref i32) (result f64)))
  (type (;19;) (func (param eqref i32 f64)))
  (type (;20;) (func (result i32)))
  (import "xui" "console-log" (func (;0;) (type 2) (param f64)))
  (import "wasm:js-string" "fromCharCodeArray" (func (;1;) (type 1) (param (ref null 0) i32 i32) (result (ref extern))))
  (memory (;0;) 16 32768)
  (tag (;0;) (type 4) (param eqref))
  (global (;0;) (mut externref) ref.null extern)
  (global (;1;) (mut externref) ref.null extern)
  (global (;2;) (mut externref) ref.null extern)
  (global (;3;) (mut externref) ref.null extern)
  (global (;4;) (mut externref) ref.null extern)
  (global (;5;) (mut externref) ref.null extern)
  (global (;6;) (mut i32) i32.const 16)
  (export "main" (func 2))
  (func (;2;) (type 20) (result i32)
    (local i32 i32 eqref i32 eqref i32 exnref i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 6
    local.tee 7
    i32.const 1048320
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 7
    i32.const 256
    i32.add
    global.set 6
    local.get 7
    i32.const 0
    i32.add
    local.set 0
    local.get 0
    i32.const 0
    i32.const 24
    memory.fill
    local.get 7
    i32.const 24
    i32.add
    local.set 1
    local.get 1
    i32.const 0
    i32.const 24
    memory.fill
    block (type 7) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
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
        if (type 11) (result i32) ;; label = @3
          local.get 3
          i32.const 0
          i32.add
        else
          local.get 3
          i32.const 0
          i32.add
        end
        i32.const 0
        call 8
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
        if (type 11) (result i32) ;; label = @3
          local.get 3
          i32.const 8
          i32.add
        else
          local.get 3
          i32.const 1
          i32.add
        end
        f64.const 0x0p+0 (;=0;)
        call 10
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
        if (type 11) (result i32) ;; label = @3
          local.get 3
          i32.const 16
          i32.add
        else
          local.get 3
          i32.const 2
          i32.add
        end
        f64.const 0x0p+0 (;=0;)
        call 10
        local.get 0
        local.set 3
        ref.null eq
        local.get 3
        i32.const 7
        local.set 14
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
        if (type 11) (result i32) ;; label = @3
          local.get 3
          i32.const 0
          i32.add
        else
          local.get 3
          i32.const 0
          i32.add
        end
        local.get 14
        i32.const 255
        i32.and
        call 8
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
        if (type 11) (result i32) ;; label = @3
          local.get 3
          i32.const 8
          i32.add
        else
          local.get 3
          i32.const 1
          i32.add
        end
        f64.const 0x1p+0 (;=1;)
        local.set 16
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
        if (type 11) (result i32) ;; label = @3
          local.get 3
          i32.const 0
          i32.add
        else
          local.get 3
          i32.const 0
          i32.add
        end
        local.get 16
        call 10
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
        if (type 11) (result i32) ;; label = @3
          local.get 3
          i32.const 8
          i32.add
        else
          local.get 3
          i32.const 1
          i32.add
        end
        f64.const 0x1p+1 (;=2;)
        local.set 16
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
        if (type 11) (result i32) ;; label = @3
          local.get 3
          i32.const 8
          i32.add
        else
          local.get 3
          i32.const 1
          i32.add
        end
        local.get 16
        call 10
        local.get 0
        local.set 8
        local.get 7
        i32.const 48
        i32.add
        local.get 8
        i32.const 24
        memory.copy
        local.get 7
        i32.const 48
        i32.add
        local.set 8
        local.get 1
        local.get 8
        i32.const 24
        memory.copy
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
        if (type 11) (result i32) ;; label = @3
          local.get 3
          i32.const 8
          i32.add
        else
          local.get 3
          i32.const 1
          i32.add
        end
        f64.const 0x1.4p+3 (;=10;)
        local.set 16
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
        if (type 11) (result i32) ;; label = @3
          local.get 3
          i32.const 0
          i32.add
        else
          local.get 3
          i32.const 0
          i32.add
        end
        local.get 16
        call 10
        local.get 1
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
        if (type 11) (result i32) ;; label = @3
          local.get 3
          i32.const 8
          i32.add
        else
          local.get 3
          i32.const 1
          i32.add
        end
        f64.const 0x1.4p+4 (;=20;)
        local.set 16
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
        if (type 11) (result i32) ;; label = @3
          local.get 3
          i32.const 8
          i32.add
        else
          local.get 3
          i32.const 1
          i32.add
        end
        local.get 16
        call 10
        local.get 0
        local.set 8
        local.get 7
        i32.const 72
        i32.add
        local.get 8
        i32.const 24
        memory.copy
        local.get 7
        i32.const 72
        i32.add
        i32.const 8
        i32.add
        local.set 8
        local.get 7
        i32.const 96
        i32.add
        local.get 8
        i32.const 16
        memory.copy
        local.get 7
        i32.const 96
        i32.add
        i32.const 0
        i32.add
        f64.load align=1
        call 0
        local.get 0
        local.set 8
        local.get 7
        i32.const 112
        i32.add
        local.get 8
        i32.const 24
        memory.copy
        local.get 7
        i32.const 112
        i32.add
        i32.const 8
        i32.add
        local.set 8
        local.get 7
        i32.const 136
        i32.add
        local.get 8
        i32.const 16
        memory.copy
        local.get 7
        i32.const 136
        i32.add
        i32.const 8
        i32.add
        f64.load align=1
        call 0
        local.get 1
        local.set 8
        local.get 7
        i32.const 152
        i32.add
        local.get 8
        i32.const 24
        memory.copy
        local.get 7
        i32.const 152
        i32.add
        i32.const 8
        i32.add
        local.set 8
        local.get 7
        i32.const 176
        i32.add
        local.get 8
        i32.const 16
        memory.copy
        local.get 7
        i32.const 176
        i32.add
        i32.const 0
        i32.add
        f64.load align=1
        call 0
        local.get 1
        local.set 8
        local.get 7
        i32.const 192
        i32.add
        local.get 8
        i32.const 24
        memory.copy
        local.get 7
        i32.const 192
        i32.add
        i32.const 8
        i32.add
        local.set 8
        local.get 7
        i32.const 216
        i32.add
        local.get 8
        i32.const 16
        memory.copy
        local.get 7
        i32.const 216
        i32.add
        i32.const 8
        i32.add
        f64.load align=1
        call 0
        local.get 1
        local.set 8
        local.get 7
        i32.const 232
        i32.add
        local.get 8
        i32.const 24
        memory.copy
        local.get 7
        i32.const 232
        i32.add
        i32.const 0
        i32.add
        i32.load8_u
        local.get 7
        global.set 6
        return
        unreachable
      end
      unreachable
    end
    local.set 6
    local.get 7
    global.set 6
    local.get 6
    throw_ref
  )
  (func (;3;) (type 12) (param eqref i32) (result eqref)
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
    unreachable
  )
  (func (;4;) (type 13) (param eqref i32 eqref)
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
    unreachable
  )
  (func (;5;) (type 14) (param eqref i32) (result externref)
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
    unreachable
  )
  (func (;6;) (type 15) (param eqref i32 externref)
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
    unreachable
  )
  (func (;7;) (type 16) (param eqref i32) (result i32)
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
      i32.load8_u
      return
    end
    unreachable
  )
  (func (;8;) (type 17) (param eqref i32 i32)
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
      i32.store8
      return
    end
    unreachable
  )
  (func (;9;) (type 18) (param eqref i32) (result f64)
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
      f64.load align=1
      return
    end
    unreachable
  )
  (func (;10;) (type 19) (param eqref i32 f64)
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
      f64.store align=1
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
)
```
