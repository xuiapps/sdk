# Stack events with reference slots

Source: [Events.cs](Fixtures/ReferenceSlots/Events.cs)

## Source

```csharp
using System;

public interface IContext { int Read(); }
public class Context : IContext
{
    public int Value;
    public int Read() => Value;
}
public struct Point { public int X; public int Y; }
public ref struct Event
{
    public Point Position;
    public string Name;
    public IContext Context;
}
public static class Program
{
    static void Dispatch(ref Event e)
    {
        e.Position.X += e.Context.Read();
        e.Name = "handled";
        Forward(ref e);
    }
    static void Forward(ref Event e) { e.Position.Y = 2; }
    public static int Main()
    {
        Event e = default;
        e.Name = "original";
        e.Context = new Context { Value = 40 };
        Event copy = e;
        Dispatch(ref e);
        Console.WriteLine(e.Name);
        Console.WriteLine(copy.Name);
        Console.WriteLine(copy.Context == e.Context);
        return e.Position.X + e.Position.Y;
    }
}
```

## Output

```text
handled
original
True
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
  (type (;2;) (func (param externref)))
  (type (;3;) (func (param i32)))
  (type (;4;) (struct (field i32) (field externref)))
  (type (;5;) (func (param eqref)))
  (type (;6;) (func (result eqref)))
  (type (;7;) (func (result eqref exnref)))
  (type (;8;) (func (result exnref)))
  (type (;9;) (func))
  (type (;10;) (struct (field i32) (field eqref) (field i32) (field (ref null 0)) (field i32) (field i32)))
  (type (;11;) (sub (struct (field (mut eqref)))))
  (type (;12;) (func (param eqref eqref) (result eqref)))
  (type (;13;) (struct (field (mut i32)) (field (mut i32)) (field (mut externref)) (field (mut eqref))))
  (type (;14;) (struct (field (mut i32)) (field (mut i32))))
  (type (;15;) (sub 11 (struct (field (mut eqref)) (field (mut i32)))))
  (type (;16;) (func (result i32)))
  (type (;17;) (func (param eqref)))
  (type (;18;) (func (param i32)))
  (type (;19;) (func (param i32)))
  (type (;20;) (func (param eqref) (result i32)))
  (type (;21;) (func (param eqref) (result i32)))
  (import "xui" "console-log" (func (;0;) (type 2) (param externref)))
  (import "xui" "console-log-boolean" (func (;1;) (type 3) (param i32)))
  (import "wasm:js-string" "fromCharCodeArray" (func (;2;) (type 1) (param (ref null 0) i32 i32) (result (ref extern))))
  (table (;0;) 2 2 funcref)
  (table (;1;) 65536 65536 eqref)
  (table (;2;) 65536 65536 externref)
  (memory (;0;) 16 16)
  (tag (;0;) (type 5) (param eqref))
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
  (global (;10;) eqref i32.const 1 ref.null eq i32.const 0 i32.const 73 i32.const 67 i32.const 111 i32.const 110 i32.const 116 i32.const 101 i32.const 120 i32.const 116 array.new_fixed 0 8 i32.const 0 i32.const 0 struct.new 10)
  (global (;11;) eqref i32.const 2 ref.null eq i32.const 0 i32.const 67 i32.const 111 i32.const 110 i32.const 116 i32.const 101 i32.const 120 i32.const 116 array.new_fixed 0 7 i32.const 1 i32.const 2 struct.new 10)
  (export "main" (func 3))
  (elem (;0;) (i32.const 1) func 7)
  (elem (;1;) (i32.const 0) func 7)
  (func (;3;) (type 16) (result i32)
    (local i32 i32 i64 f64 externref eqref exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 8
    local.tee 11
    i32.const 1048448
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 11
    i32.const 128
    i32.add
    global.set 8
    local.get 11
    i32.const 0
    i32.add
    local.set 0
    local.get 0
    i32.const 0
    i32.const 16
    memory.fill
    global.get 9
    local.tee 8
    i32.const 65522
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 8
    i32.const 14
    i32.add
    global.set 9
    local.get 11
    i32.const 8
    i32.add
    local.get 8
    i32.const 0
    i32.add
    i32.store align=1
    local.get 11
    i32.const 12
    i32.add
    local.get 8
    i32.const 1
    i32.add
    i32.store align=1
    local.get 11
    i32.const 24
    i32.add
    local.get 8
    i32.const 2
    i32.add
    i32.store align=1
    local.get 11
    i32.const 28
    i32.add
    local.get 8
    i32.const 3
    i32.add
    i32.store align=1
    local.get 11
    i32.const 40
    i32.add
    local.get 8
    i32.const 4
    i32.add
    i32.store align=1
    local.get 11
    i32.const 44
    i32.add
    local.get 8
    i32.const 5
    i32.add
    i32.store align=1
    local.get 11
    i32.const 56
    i32.add
    local.get 8
    i32.const 6
    i32.add
    i32.store align=1
    local.get 11
    i32.const 60
    i32.add
    local.get 8
    i32.const 7
    i32.add
    i32.store align=1
    local.get 11
    i32.const 72
    i32.add
    local.get 8
    i32.const 8
    i32.add
    i32.store align=1
    local.get 11
    i32.const 76
    i32.add
    local.get 8
    i32.const 9
    i32.add
    i32.store align=1
    local.get 11
    i32.const 88
    i32.add
    local.get 8
    i32.const 10
    i32.add
    i32.store align=1
    local.get 11
    i32.const 92
    i32.add
    local.get 8
    i32.const 11
    i32.add
    i32.store align=1
    local.get 11
    i32.const 112
    i32.add
    local.get 8
    i32.const 12
    i32.add
    i32.store align=1
    local.get 11
    i32.const 116
    i32.add
    local.get 8
    i32.const 13
    i32.add
    i32.store align=1
    block (type 8) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.set 10
        local.get 10
        i32.const 0
        i32.add
        i32.const 0
        i32.store align=1
        local.get 10
        i32.const 4
        i32.add
        i32.const 0
        i32.store align=1
        local.get 10
        i32.const 8
        i32.add
        ref.null extern
        local.set 21
        i32.load align=1
        local.get 21
        table.set 2
        local.get 10
        i32.const 12
        i32.add
        ref.null eq
        local.set 7
        i32.load align=1
        local.get 7
        table.set 1
        local.get 0
        global.get 0
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 8
          array.new_data 0 0
          i32.const 0
          i32.const 8
          call 2
          global.set 0
        end
        global.get 0
        local.set 21
        i32.const 8
        i32.add
        local.get 21
        local.set 21
        i32.load align=1
        local.get 21
        table.set 2
        local.get 0
        struct.new_default 15
        local.set 5
        local.get 5
        ref.cast (ref 15)
        global.get 11
        struct.set 15 0
        local.get 5
        call 4
        local.get 5
        local.tee 16
        local.get 16
        i32.const 40
        local.set 18
        local.tee 17
        ref.is_null
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
            call 2
            global.set 5
          end
          global.get 5
          struct.new 4
          throw 0
        end
        local.get 17
        ref.cast (ref 15)
        local.set 17
        local.get 17
        ref.cast (ref 15)
        local.get 18
        struct.set 15 1
        local.set 16
        i32.const 12
        i32.add
        local.get 16
        local.set 7
        i32.load align=1
        local.get 7
        table.set 1
        local.get 0
        local.set 12
        local.get 11
        i32.const 16
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
        i32.store align=1
        local.get 10
        i32.const 4
        i32.add
        local.get 9
        i32.const 4
        i32.add
        i32.load align=1
        i32.store align=1
        local.get 10
        i32.const 8
        i32.add
        local.get 9
        i32.const 8
        i32.add
        i32.load align=1
        table.get 2
        local.set 21
        i32.load align=1
        local.get 21
        table.set 2
        local.get 10
        i32.const 12
        i32.add
        local.get 9
        i32.const 12
        i32.add
        i32.load align=1
        table.get 1
        local.set 7
        i32.load align=1
        local.get 7
        table.set 1
        local.get 11
        i32.const 16
        i32.add
        local.get 0
        call 5
        local.get 0
        local.set 12
        local.get 11
        i32.const 32
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
        i32.store align=1
        local.get 10
        i32.const 4
        i32.add
        local.get 9
        i32.const 4
        i32.add
        i32.load align=1
        i32.store align=1
        local.get 10
        i32.const 8
        i32.add
        local.get 9
        i32.const 8
        i32.add
        i32.load align=1
        table.get 2
        local.set 21
        i32.load align=1
        local.get 21
        table.set 2
        local.get 10
        i32.const 12
        i32.add
        local.get 9
        i32.const 12
        i32.add
        i32.load align=1
        table.get 1
        local.set 7
        i32.load align=1
        local.get 7
        table.set 1
        local.get 11
        i32.const 32
        i32.add
        i32.const 8
        i32.add
        i32.load align=1
        table.get 2
        call 0
        local.tee 13
        local.get 13
        local.set 12
        local.get 11
        i32.const 48
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
        i32.store align=1
        local.get 10
        i32.const 4
        i32.add
        local.get 9
        i32.const 4
        i32.add
        i32.load align=1
        i32.store align=1
        local.get 10
        i32.const 8
        i32.add
        local.get 9
        i32.const 8
        i32.add
        i32.load align=1
        table.get 2
        local.set 21
        i32.load align=1
        local.get 21
        table.set 2
        local.get 10
        i32.const 12
        i32.add
        local.get 9
        i32.const 12
        i32.add
        i32.load align=1
        table.get 1
        local.set 7
        i32.load align=1
        local.get 7
        table.set 1
        local.get 11
        i32.const 48
        i32.add
        i32.const 8
        i32.add
        i32.load align=1
        table.get 2
        call 0
        i32.const 12
        i32.add
        i32.load align=1
        table.get 1
        local.get 0
        local.set 12
        local.get 11
        i32.const 64
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
        i32.store align=1
        local.get 10
        i32.const 4
        i32.add
        local.get 9
        i32.const 4
        i32.add
        i32.load align=1
        i32.store align=1
        local.get 10
        i32.const 8
        i32.add
        local.get 9
        i32.const 8
        i32.add
        i32.load align=1
        table.get 2
        local.set 21
        i32.load align=1
        local.get 21
        table.set 2
        local.get 10
        i32.const 12
        i32.add
        local.get 9
        i32.const 12
        i32.add
        i32.load align=1
        table.get 1
        local.set 7
        i32.load align=1
        local.get 7
        table.set 1
        local.get 11
        i32.const 64
        i32.add
        i32.const 12
        i32.add
        i32.load align=1
        table.get 1
        ref.eq
        call 1
        local.get 0
        local.set 12
        local.get 11
        i32.const 80
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
        i32.store align=1
        local.get 10
        i32.const 4
        i32.add
        local.get 9
        i32.const 4
        i32.add
        i32.load align=1
        i32.store align=1
        local.get 10
        i32.const 8
        i32.add
        local.get 9
        i32.const 8
        i32.add
        i32.load align=1
        table.get 2
        local.set 21
        i32.load align=1
        local.get 21
        table.set 2
        local.get 10
        i32.const 12
        i32.add
        local.get 9
        i32.const 12
        i32.add
        i32.load align=1
        table.get 1
        local.set 7
        i32.load align=1
        local.get 7
        table.set 1
        local.get 11
        i32.const 80
        i32.add
        i32.const 0
        i32.add
        local.set 12
        local.get 11
        i32.const 96
        i32.add
        local.get 12
        i32.const 8
        memory.copy
        local.get 11
        i32.const 96
        i32.add
        i32.const 0
        i32.add
        i32.load align=1
        local.get 0
        local.set 12
        local.get 11
        i32.const 104
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
        i32.store align=1
        local.get 10
        i32.const 4
        i32.add
        local.get 9
        i32.const 4
        i32.add
        i32.load align=1
        i32.store align=1
        local.get 10
        i32.const 8
        i32.add
        local.get 9
        i32.const 8
        i32.add
        i32.load align=1
        table.get 2
        local.set 21
        i32.load align=1
        local.get 21
        table.set 2
        local.get 10
        i32.const 12
        i32.add
        local.get 9
        i32.const 12
        i32.add
        i32.load align=1
        table.get 1
        local.set 7
        i32.load align=1
        local.get 7
        table.set 1
        local.get 11
        i32.const 104
        i32.add
        i32.const 0
        i32.add
        local.set 12
        local.get 11
        i32.const 120
        i32.add
        local.get 12
        i32.const 8
        memory.copy
        local.get 11
        i32.const 120
        i32.add
        i32.const 4
        i32.add
        i32.load align=1
        i32.add
        local.get 8
        ref.null eq
        i32.const 14
        table.fill 1
        local.get 8
        ref.null extern
        i32.const 14
        table.fill 2
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
    i32.const 14
    table.fill 1
    local.get 8
    ref.null extern
    i32.const 14
    table.fill 2
    local.get 8
    global.set 9
    local.get 11
    global.set 8
    local.get 6
    throw_ref
  )
  (func (;4;) (type 17) (param eqref)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    drop
    return
  )
  (func (;5;) (type 18) (param i32)
    (local i32 i64 f64 externref eqref exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 8
    local.tee 11
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 11
    i32.const 0
    i32.add
    global.set 8
    block (type 8) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 0
        i32.add
        i32.const 0
        i32.add
        local.tee 1
        local.get 1
        i32.load align=1
        local.get 0
        i32.const 12
        i32.add
        i32.load align=1
        table.get 1
        local.set 5
        local.get 5
        local.tee 17
        ref.is_null
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
            call 2
            global.set 5
          end
          global.get 5
          struct.new 4
          throw 0
        end
        local.get 17
        ref.cast (ref 11)
        local.get 5
        ref.cast (ref 11)
        struct.get 11 0
        ref.cast (ref 10)
        struct.get 10 2
        i32.const 0
        i32.add
        call_indirect (type 20)
        i32.add
        i32.store align=1
        local.get 0
        global.get 1
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 7
          array.new_data 0 1
          i32.const 0
          i32.const 7
          call 2
          global.set 1
        end
        global.get 1
        local.set 21
        i32.const 8
        i32.add
        local.get 21
        local.set 21
        i32.load align=1
        local.get 21
        table.set 2
        local.get 0
        call 6
        local.get 11
        global.set 8
        return
        unreachable
      end
      unreachable
    end
    local.set 6
    local.get 11
    global.set 8
    local.get 6
    throw_ref
  )
  (func (;6;) (type 19) (param i32)
    (local exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 8
    local.tee 6
    i32.const 1048576
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 6
    i32.const 0
    i32.add
    global.set 8
    block (type 8) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        i32.const 0
        i32.add
        i32.const 2
        local.set 8
        i32.const 4
        i32.add
        local.get 8
        i32.store align=1
        local.get 6
        global.set 8
        return
        unreachable
      end
      unreachable
    end
    local.set 1
    local.get 6
    global.set 8
    local.get 1
    throw_ref
  )
  (func (;7;) (type 21) (param eqref) (result i32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.tee 2
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 5
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 5
        i32.const 0
        i32.const 53
        call 2
        global.set 5
      end
      global.get 5
      struct.new 4
      throw 0
    end
    local.get 2
    ref.cast (ref 15)
    struct.get 15 1
    return
  )
  (func (;8;) (type 12) (param eqref eqref) (result eqref)
    (local eqref)
    local.get 0
    ref.is_null
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 0
    ref.test (ref 11)
    i32.eqz
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 0
    ref.cast (ref 11)
    struct.get 11 0
    local.set 2
    local.get 1
    global.get 10
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 11
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
      ref.cast (ref 10)
      struct.get 10 1
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
  (data (;0;) "o\00r\00i\00g\00i\00n\00a\00l\00")
  (data (;1;) "h\00a\00n\00d\00l\00e\00d\00")
  (data (;2;) "E\00x\00c\00e\00p\00t\00i\00o\00n\00 \00o\00f\00 \00t\00y\00p\00e\00 \00'\00S\00y\00s\00t\00e\00m\00.\00E\00x\00c\00e\00p\00t\00i\00o\00n\00'\00 \00w\00a\00s\00 \00t\00h\00r\00o\00w\00n\00.\00")
  (data (;3;) "S\00y\00s\00t\00e\00m\00 \00e\00r\00r\00o\00r\00.\00")
  (data (;4;) "S\00p\00e\00c\00i\00f\00i\00e\00d\00 \00c\00a\00s\00t\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00.\00")
  (data (;5;) "O\00b\00j\00e\00c\00t\00 \00r\00e\00f\00e\00r\00e\00n\00c\00e\00 \00n\00o\00t\00 \00s\00e\00t\00 \00t\00o\00 \00a\00n\00 \00i\00n\00s\00t\00a\00n\00c\00e\00 \00o\00f\00 \00a\00n\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;6;) "O\00p\00e\00r\00a\00t\00i\00o\00n\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00 \00d\00u\00e\00 \00t\00o\00 \00t\00h\00e\00 \00c\00u\00r\00r\00e\00n\00t\00 \00s\00t\00a\00t\00e\00 \00o\00f\00 \00t\00h\00e\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;7;) "V\00a\00l\00u\00e\00 \00d\00o\00e\00s\00 \00n\00o\00t\00 \00f\00a\00l\00l\00 \00w\00i\00t\00h\00i\00n\00 \00t\00h\00e\00 \00e\00x\00p\00e\00c\00t\00e\00d\00 \00r\00a\00n\00g\00e\00.\00")
)
```
