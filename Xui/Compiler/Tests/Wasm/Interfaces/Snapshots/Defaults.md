# Default interface layout methods

Source: [Defaults.cs](Fixtures/Interfaces/Defaults.cs)

## Source

```csharp
using System;

public struct Bounds { public int X, Y; }
public interface ILayout
{
    string Name { get; }
    Bounds Arrange(Bounds bounds) => bounds;
    void Show(Bounds bounds)
    {
        Bounds arranged = Arrange(bounds);
        Console.WriteLine(Name);
        Console.WriteLine(arranged.X);
        Console.WriteLine(arranged.Y);
    }
}
public interface IInsetLayout : ILayout
{
    Bounds ILayout.Arrange(Bounds bounds) => new Bounds { X = bounds.X + 10, Y = bounds.Y + 20 };
}
public class Panel : IInsetLayout { public string Name => "panel"; }
public class CustomPanel : Panel, ILayout
{
    public Bounds Arrange(Bounds bounds) => new Bounds { X = bounds.X + 30, Y = bounds.Y + 40 };
}
public static class Program
{
    public static int Main()
    {
        ILayout panel = new Panel();
        panel.Show(new Bounds { X = 1, Y = 2 });
        panel = new CustomPanel();
        panel.Show(new Bounds { X = 3, Y = 4 });
        return 0;
    }
}
```

## Output

```text
panel
11
22
panel
33
44
```

## Exit Code

```text
0
```

## Dispatch

| Receiver | Declaration | Selected implementation |
| --- | --- | --- |
| `Panel` | `ILayout.Show` | `ILayout.Show` |
| `Panel` | `ILayout.Arrange` | `IInsetLayout.ILayout.Arrange` |
| `Panel` | `ILayout.get_Name` | `Panel.get_Name` |
| `CustomPanel` | `ILayout.Show` | `ILayout.Show` |
| `CustomPanel` | `ILayout.Arrange` | `CustomPanel.Arrange` |
| `CustomPanel` | `ILayout.get_Name` | `Panel.get_Name` |

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
  (type (;13;) (sub 11 (struct (field (mut eqref)))))
  (type (;14;) (sub 13 (struct (field (mut eqref)))))
  (type (;15;) (func (result i32)))
  (type (;16;) (func (param eqref)))
  (type (;17;) (func (param eqref)))
  (type (;18;) (func (param eqref i32)))
  (type (;19;) (func (param eqref i32 i32) (result i32)))
  (type (;20;) (func (param eqref i32 i32) (result i32)))
  (type (;21;) (func (param eqref) (result externref)))
  (type (;22;) (func (param eqref) (result externref)))
  (type (;23;) (func (param eqref i32 i32) (result i32)))
  (import "xui" "console-log" (func (;0;) (type 2) (param externref)))
  (import "xui" "console-log" (func (;1;) (type 3) (param i32)))
  (import "wasm:js-string" "fromCharCodeArray" (func (;2;) (type 1) (param (ref null 0) i32 i32) (result (ref extern))))
  (table (;0;) 9 9 funcref)
  (memory (;0;) 16 16)
  (tag (;0;) (type 5) (param eqref))
  (global (;0;) (mut externref) ref.null extern)
  (global (;1;) (mut externref) ref.null extern)
  (global (;2;) (mut externref) ref.null extern)
  (global (;3;) (mut externref) ref.null extern)
  (global (;4;) (mut externref) ref.null extern)
  (global (;5;) (mut externref) ref.null extern)
  (global (;6;) (mut externref) ref.null extern)
  (global (;7;) (mut i32) i32.const 16)
  (global (;8;) eqref i32.const 1 ref.null eq i32.const 0 i32.const 80 i32.const 97 i32.const 110 i32.const 101 i32.const 108 array.new_fixed 0 5 i32.const 1 i32.const 1 struct.new 10)
  (global (;9;) eqref i32.const 2 ref.null eq i32.const 0 i32.const 73 i32.const 73 i32.const 110 i32.const 115 i32.const 101 i32.const 116 i32.const 76 i32.const 97 i32.const 121 i32.const 111 i32.const 117 i32.const 116 array.new_fixed 0 12 i32.const 0 i32.const 0 struct.new 10)
  (global (;10;) eqref i32.const 3 ref.null eq i32.const 0 i32.const 73 i32.const 76 i32.const 97 i32.const 121 i32.const 111 i32.const 117 i32.const 116 array.new_fixed 0 7 i32.const 0 i32.const 0 struct.new 10)
  (global (;11;) eqref i32.const 4 global.get 8 i32.const 4 i32.const 67 i32.const 117 i32.const 115 i32.const 116 i32.const 111 i32.const 109 i32.const 80 i32.const 97 i32.const 110 i32.const 101 i32.const 108 array.new_fixed 0 11 i32.const 2 i32.const 1 struct.new 10)
  (export "main" (func 3))
  (elem (;0;) (i32.const 3) func 9)
  (elem (;1;) (i32.const 0) func 6)
  (elem (;2;) (i32.const 1) func 7)
  (elem (;3;) (i32.const 2) func 9)
  (elem (;4;) (i32.const 7) func 9)
  (elem (;5;) (i32.const 8) func 8)
  (elem (;6;) (i32.const 4) func 6)
  (elem (;7;) (i32.const 5) func 8)
  (elem (;8;) (i32.const 6) func 9)
  (func (;3;) (type 15) (result i32)
    (local i32 eqref eqref i32 eqref exnref i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
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
    local.set 0
    local.get 0
    i32.const 0
    i32.const 8
    memory.fill
    block (type 8) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        struct.new_default 13
        local.set 1
        local.get 1
        ref.cast (ref 13)
        global.get 8
        struct.set 13 0
        local.get 1
        call 4
        local.get 1
        local.get 0
        i32.const 0
        i32.const 8
        memory.fill
        local.get 0
        i32.const 1
        local.set 8
        i32.const 0
        i32.add
        local.get 8
        i32.store align=1
        local.get 0
        i32.const 2
        local.set 8
        i32.const 4
        i32.add
        local.get 8
        i32.store align=1
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
        local.set 3
        local.set 2
        local.get 2
        local.tee 12
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
            call 2
            global.set 4
          end
          global.get 4
          struct.new 4
          throw 0
        end
        local.get 12
        ref.cast (ref 11)
        local.get 3
        local.get 2
        ref.cast (ref 11)
        struct.get 11 0
        ref.cast (ref 10)
        struct.get 10 2
        i32.const 0
        i32.add
        call_indirect (type 18)
        struct.new_default 14
        local.set 4
        local.get 4
        ref.cast (ref 14)
        global.get 11
        struct.set 14 0
        local.get 4
        call 5
        local.get 4
        local.get 0
        i32.const 0
        i32.const 8
        memory.fill
        local.get 0
        i32.const 3
        local.set 8
        i32.const 0
        i32.add
        local.get 8
        i32.store align=1
        local.get 0
        i32.const 4
        local.set 8
        i32.const 4
        i32.add
        local.get 8
        i32.store align=1
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
        local.set 3
        local.set 2
        local.get 2
        local.tee 12
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
            call 2
            global.set 4
          end
          global.get 4
          struct.new 4
          throw 0
        end
        local.get 12
        ref.cast (ref 11)
        local.get 3
        local.get 2
        ref.cast (ref 11)
        struct.get 11 0
        ref.cast (ref 10)
        struct.get 10 2
        i32.const 0
        i32.add
        call_indirect (type 18)
        i32.const 0
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
  (func (;4;) (type 16) (param eqref)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    drop
    return
  )
  (func (;5;) (type 17) (param eqref)
    (local eqref eqref eqref i32 i64 f64 externref)
    local.get 0
    local.set 1
    local.get 1
    call 4
    return
  )
  (func (;6;) (type 18) (param eqref i32)
    (local i32 i64 f64 externref eqref i32 eqref exnref i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 7
    local.tee 10
    i32.const 1048544
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 10
    i32.const 32
    i32.add
    global.set 7
    local.get 10
    i32.const 0
    i32.add
    local.get 1
    i32.const 8
    memory.copy
    local.get 10
    i32.const 0
    i32.add
    local.set 1
    block (type 8) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 0
        local.get 1
        local.set 11
        local.get 10
        i32.const 8
        i32.add
        local.get 11
        i32.const 8
        memory.copy
        local.get 10
        i32.const 8
        i32.add
        local.set 7
        local.set 6
        local.get 6
        local.tee 16
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
            call 2
            global.set 4
          end
          global.get 4
          struct.new 4
          throw 0
        end
        local.get 16
        ref.cast (ref 11)
        local.get 7
        local.get 10
        i32.const 16
        i32.add
        local.get 6
        ref.cast (ref 11)
        struct.get 11 0
        ref.cast (ref 10)
        struct.get 10 2
        i32.const 1
        i32.add
        call_indirect (type 23)
        local.get 0
        local.set 8
        local.get 8
        local.tee 16
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
            call 2
            global.set 4
          end
          global.get 4
          struct.new 4
          throw 0
        end
        local.get 16
        ref.cast (ref 11)
        local.get 8
        ref.cast (ref 11)
        struct.get 11 0
        ref.cast (ref 10)
        struct.get 10 2
        i32.const 2
        i32.add
        call_indirect (type 21)
        call 0
        local.tee 12
        local.get 12
        local.set 11
        local.get 10
        i32.const 24
        i32.add
        local.get 11
        i32.const 8
        memory.copy
        local.get 10
        i32.const 24
        i32.add
        i32.const 0
        i32.add
        i32.load align=1
        call 1
        i32.const 4
        i32.add
        i32.load align=1
        call 1
        local.get 10
        global.set 7
        return
        unreachable
      end
      unreachable
    end
    local.set 9
    local.get 10
    global.set 7
    local.get 9
    throw_ref
  )
  (func (;7;) (type 19) (param eqref i32 i32) (result i32)
    (local i32 exnref i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
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
    i32.const 8
    i32.add
    local.set 3
    local.get 3
    i32.const 0
    i32.const 8
    memory.fill
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
    block (type 8) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 3
        i32.const 0
        i32.const 8
        memory.fill
        local.get 3
        local.get 1
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
        i32.const 0
        i32.add
        i32.load align=1
        i32.const 10
        i32.add
        local.set 7
        i32.const 0
        i32.add
        local.get 7
        i32.store align=1
        local.get 3
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
        i32.const 4
        i32.add
        i32.load align=1
        i32.const 20
        i32.add
        local.set 7
        i32.const 4
        i32.add
        local.get 7
        i32.store align=1
        local.get 3
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
        local.set 6
        local.get 2
        local.get 6
        i32.const 8
        memory.copy
        local.get 2
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
  (func (;8;) (type 20) (param eqref i32 i32) (result i32)
    (local i32 exnref i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
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
    i32.const 8
    i32.add
    local.set 3
    local.get 3
    i32.const 0
    i32.const 8
    memory.fill
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
    block (type 8) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        local.get 3
        i32.const 0
        i32.const 8
        memory.fill
        local.get 3
        local.get 1
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
        i32.const 0
        i32.add
        i32.load align=1
        i32.const 30
        i32.add
        local.set 7
        i32.const 0
        i32.add
        local.get 7
        i32.store align=1
        local.get 3
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
        i32.const 4
        i32.add
        i32.load align=1
        i32.const 40
        i32.add
        local.set 7
        i32.const 4
        i32.add
        local.get 7
        i32.store align=1
        local.get 3
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
        local.set 6
        local.get 2
        local.get 6
        i32.const 8
        memory.copy
        local.get 2
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
  (func (;9;) (type 22) (param eqref) (result externref)
    (local eqref eqref i32 i64 f64 externref)
    global.get 0
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 5
      array.new_data 0 0
      i32.const 0
      i32.const 5
      call 2
      global.set 0
    end
    global.get 0
    return
  )
  (func (;10;) (type 12) (param eqref eqref) (result eqref)
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
    global.get 9
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 8
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
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
    local.get 1
    global.get 10
    ref.eq
    if ;; label = @1
      local.get 2
      global.get 8
      ref.eq
      if ;; label = @2
        local.get 0
        return
      end
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
  (data (;0;) "p\00a\00n\00e\00l\00")
  (data (;1;) "E\00x\00c\00e\00p\00t\00i\00o\00n\00 \00o\00f\00 \00t\00y\00p\00e\00 \00'\00S\00y\00s\00t\00e\00m\00.\00E\00x\00c\00e\00p\00t\00i\00o\00n\00'\00 \00w\00a\00s\00 \00t\00h\00r\00o\00w\00n\00.\00")
  (data (;2;) "S\00y\00s\00t\00e\00m\00 \00e\00r\00r\00o\00r\00.\00")
  (data (;3;) "S\00p\00e\00c\00i\00f\00i\00e\00d\00 \00c\00a\00s\00t\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00.\00")
  (data (;4;) "O\00b\00j\00e\00c\00t\00 \00r\00e\00f\00e\00r\00e\00n\00c\00e\00 \00n\00o\00t\00 \00s\00e\00t\00 \00t\00o\00 \00a\00n\00 \00i\00n\00s\00t\00a\00n\00c\00e\00 \00o\00f\00 \00a\00n\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;5;) "O\00p\00e\00r\00a\00t\00i\00o\00n\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00 \00d\00u\00e\00 \00t\00o\00 \00t\00h\00e\00 \00c\00u\00r\00r\00e\00n\00t\00 \00s\00t\00a\00t\00e\00 \00o\00f\00 \00t\00h\00e\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;6;) "V\00a\00l\00u\00e\00 \00d\00o\00e\00s\00 \00n\00o\00t\00 \00f\00a\00l\00l\00 \00w\00i\00t\00h\00i\00n\00 \00t\00h\00e\00 \00e\00x\00p\00e\00c\00t\00e\00d\00 \00r\00a\00n\00g\00e\00.\00")
)
```
