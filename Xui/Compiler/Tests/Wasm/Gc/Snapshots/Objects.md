# Classes and GC struct values

Source: [Objects.cs](Fixtures/Gc/Objects.cs)

## Source

```csharp
using System;

public struct Point { public int X; public int Y; }
public struct Label { public string Text; public int Number; }
public struct Caption { public Label Value; public Widget Target; }

public class Widget
{
    public Point Position;
    public Caption Caption;
    public Widget(int x, string text)
    {
        Position.X = x;
        Caption.Value.Text = text;
    }
}

public static class Program
{
    public static int Main()
    {
        var original = new Widget(1, "first");
        var alias = original;
        alias.Position.X = 4;
        var label = original.Caption.Value;
        label.Text = "copy";
        label.Number = 7;
        original.Caption.Value.Number = 2;
        original.Caption.Target = original;
        var other = new Widget(9, "second");
        other.Caption = original.Caption;
        other.Caption.Value.Text = "other";
        other.Caption.Target.Position.Y = 6;
        Console.WriteLine(original.Position.X);
        Console.WriteLine(original.Position.Y);
        Console.WriteLine(original.Caption.Value.Text);
        Console.WriteLine(label.Text);
        Console.WriteLine(other.Caption.Value.Text);
        Console.WriteLine(original.Caption.Value.Number);
        Console.WriteLine(label.Number);
        var point = original.Position;
        point.X = 20;
        original.Position = point;
        point.X = 99;
        return original.Position.X;
    }
}
```

## Output

```text
4
6
first
copy
other
2
7
```

## Exit Code

```text
20
```

## WAT

```wat
(module
  (type (;0;) (array (mut i16)))
  (type (;1;) (func (param (ref null 0) i32 i32) (result (ref extern))))
  (type (;2;) (func (param i32)))
  (type (;3;) (func (param externref)))
  (type (;4;) (struct (field i32) (field eqref) (field i32) (field (ref null 0)) (field i32) (field i32)))
  (type (;5;) (sub (struct (field (mut eqref)))))
  (type (;6;) (func (param eqref eqref) (result eqref)))
  (type (;7;) (sub 5 (struct (field (mut eqref)) (field (mut i32)) (field (mut i32)) (field (mut externref)) (field (mut i32)) (field (mut eqref)))))
  (type (;8;) (struct (field (mut i32)) (field (mut i32))))
  (type (;9;) (struct (field (mut externref)) (field (mut i32)) (field (mut eqref))))
  (type (;10;) (struct (field (mut externref)) (field (mut i32))))
  (type (;11;) (func (result i32)))
  (type (;12;) (func (param eqref i32 externref)))
  (import "xui" "console-log" (func (;0;) (type 2) (param i32)))
  (import "xui" "console-log" (func (;1;) (type 3) (param externref)))
  (import "wasm:js-string" "fromCharCodeArray" (func (;2;) (type 1) (param (ref null 0) i32 i32) (result (ref extern))))
  (memory (;0;) 16 16)
  (global (;0;) (mut externref) ref.null extern)
  (global (;1;) (mut externref) ref.null extern)
  (global (;2;) (mut externref) ref.null extern)
  (global (;3;) (mut externref) ref.null extern)
  (global (;4;) (mut i32) i32.const 16)
  (global (;5;) eqref i32.const 1 ref.null eq i32.const 0 i32.const 87 i32.const 105 i32.const 100 i32.const 103 i32.const 101 i32.const 116 array.new_fixed 0 6 i32.const 0 i32.const 6 struct.new 4)
  (export "main" (func 3))
  (func (;3;) (type 11) (result i32)
    (local eqref eqref i32 i32 i64 f64 externref eqref i32 externref i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 4
    local.tee 10
    i32.const 1048544
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 10
    i32.const 32
    i32.add
    global.set 4
    local.get 10
    i32.const 0
    i32.add
    local.set 2
    local.get 2
    i32.const 0
    i32.const 8
    memory.fill
    struct.new_default 10
    local.set 1
    i32.const 1
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
    local.set 9
    local.set 8
    struct.new_default 7
    local.set 7
    local.get 7
    ref.cast (ref 7)
    global.get 5
    struct.set 7 0
    local.get 7
    local.get 8
    local.get 9
    call 4
    local.get 7
    local.set 0
    local.get 0
    ref.cast (ref 7)
    i32.const 4
    local.set 17
    ref.cast (ref 7)
    local.set 16
    local.get 16
    ref.cast (ref 7)
    local.get 17
    struct.set 7 1
    local.get 0
    ref.cast (ref 7)
    ref.cast (ref 7)
    local.set 16
    local.get 16
    ref.cast (ref 7)
    struct.get 7 3
    local.get 16
    ref.cast (ref 7)
    struct.get 7 4
    struct.new 10
    local.set 15
    local.get 1
    local.set 16
    local.get 16
    ref.cast (ref 10)
    local.get 15
    ref.cast (ref 10)
    struct.get 10 0
    struct.set 10 0
    local.get 16
    ref.cast (ref 10)
    local.get 15
    ref.cast (ref 10)
    struct.get 10 1
    struct.set 10 1
    local.get 1
    global.get 1
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 4
      array.new_data 0 1
      i32.const 0
      i32.const 4
      call 2
      global.set 1
    end
    global.get 1
    local.set 20
    ref.cast (ref 10)
    local.set 16
    local.get 16
    ref.cast (ref 10)
    local.get 20
    struct.set 10 0
    local.get 1
    i32.const 7
    local.set 17
    ref.cast (ref 10)
    local.set 16
    local.get 16
    ref.cast (ref 10)
    local.get 17
    struct.set 10 1
    local.get 0
    ref.cast (ref 7)
    ref.cast (ref 7)
    i32.const 2
    local.set 17
    ref.cast (ref 7)
    local.set 16
    local.get 16
    ref.cast (ref 7)
    local.get 17
    struct.set 7 4
    local.get 0
    ref.cast (ref 7)
    local.get 0
    local.set 15
    ref.cast (ref 7)
    local.set 16
    local.get 16
    ref.cast (ref 7)
    local.get 15
    struct.set 7 5
    i32.const 9
    global.get 2
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 6
      array.new_data 0 2
      i32.const 0
      i32.const 6
      call 2
      global.set 2
    end
    global.get 2
    local.set 9
    local.set 8
    struct.new_default 7
    local.set 7
    local.get 7
    ref.cast (ref 7)
    global.get 5
    struct.set 7 0
    local.get 7
    local.get 8
    local.get 9
    call 4
    local.get 7
    local.tee 15
    local.get 15
    local.get 0
    ref.cast (ref 7)
    local.set 16
    local.get 16
    ref.cast (ref 7)
    struct.get 7 3
    local.get 16
    ref.cast (ref 7)
    struct.get 7 4
    local.get 16
    ref.cast (ref 7)
    struct.get 7 5
    struct.new 9
    local.set 15
    ref.cast (ref 7)
    local.set 16
    local.get 16
    ref.cast (ref 7)
    local.get 15
    ref.cast (ref 9)
    struct.get 9 0
    struct.set 7 3
    local.get 16
    ref.cast (ref 7)
    local.get 15
    ref.cast (ref 9)
    struct.get 9 1
    struct.set 7 4
    local.get 16
    ref.cast (ref 7)
    local.get 15
    ref.cast (ref 9)
    struct.get 9 2
    struct.set 7 5
    local.tee 15
    local.get 15
    ref.cast (ref 7)
    ref.cast (ref 7)
    global.get 3
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 5
      array.new_data 0 3
      i32.const 0
      i32.const 5
      call 2
      global.set 3
    end
    global.get 3
    local.set 20
    ref.cast (ref 7)
    local.set 16
    local.get 16
    ref.cast (ref 7)
    local.get 20
    struct.set 7 3
    local.tee 15
    local.get 15
    ref.cast (ref 7)
    ref.cast (ref 7)
    struct.get 7 5
    ref.cast (ref 7)
    i32.const 6
    local.set 17
    ref.cast (ref 7)
    local.set 16
    local.get 16
    ref.cast (ref 7)
    local.get 17
    struct.set 7 2
    local.get 0
    ref.cast (ref 7)
    ref.cast (ref 7)
    struct.get 7 1
    call 0
    local.get 0
    ref.cast (ref 7)
    ref.cast (ref 7)
    struct.get 7 2
    call 0
    local.get 0
    ref.cast (ref 7)
    ref.cast (ref 7)
    ref.cast (ref 7)
    struct.get 7 3
    call 1
    local.get 1
    ref.cast (ref 10)
    local.set 15
    local.get 15
    ref.cast (ref 10)
    struct.get 10 0
    local.get 15
    ref.cast (ref 10)
    struct.get 10 1
    struct.new 10
    ref.cast (ref 10)
    struct.get 10 0
    call 1
    ref.cast (ref 7)
    ref.cast (ref 7)
    ref.cast (ref 7)
    struct.get 7 3
    call 1
    local.get 0
    ref.cast (ref 7)
    ref.cast (ref 7)
    ref.cast (ref 7)
    struct.get 7 4
    call 0
    local.get 1
    ref.cast (ref 10)
    local.set 15
    local.get 15
    ref.cast (ref 10)
    struct.get 10 0
    local.get 15
    ref.cast (ref 10)
    struct.get 10 1
    struct.new 10
    ref.cast (ref 10)
    struct.get 10 1
    call 0
    local.get 0
    ref.cast (ref 7)
    local.set 16
    local.get 10
    i32.const 8
    i32.add
    local.get 16
    ref.cast (ref 7)
    struct.get 7 1
    i32.store align=1
    local.get 10
    i32.const 12
    i32.add
    local.get 16
    ref.cast (ref 7)
    struct.get 7 2
    i32.store align=1
    local.get 10
    i32.const 8
    i32.add
    local.set 11
    local.get 2
    local.get 11
    i32.const 8
    memory.copy
    local.get 2
    i32.const 20
    local.set 12
    i32.const 0
    i32.add
    local.get 12
    i32.store align=1
    local.get 0
    local.get 2
    local.set 11
    local.get 10
    i32.const 16
    i32.add
    local.get 11
    i32.const 8
    memory.copy
    local.get 10
    i32.const 16
    i32.add
    local.set 17
    ref.cast (ref 7)
    local.set 16
    local.get 16
    ref.cast (ref 7)
    local.get 17
    i32.const 0
    i32.add
    i32.load align=1
    struct.set 7 1
    local.get 16
    ref.cast (ref 7)
    local.get 17
    i32.const 4
    i32.add
    i32.load align=1
    struct.set 7 2
    local.get 2
    i32.const 99
    local.set 12
    i32.const 0
    i32.add
    local.get 12
    i32.store align=1
    local.get 0
    ref.cast (ref 7)
    ref.cast (ref 7)
    struct.get 7 1
    local.get 10
    global.set 4
    return
  )
  (func (;4;) (type 12) (param eqref i32 externref)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    drop
    local.get 0
    ref.cast (ref 7)
    local.get 1
    local.set 5
    ref.cast (ref 7)
    local.set 4
    local.get 4
    ref.cast (ref 7)
    local.get 5
    struct.set 7 1
    local.get 0
    ref.cast (ref 7)
    ref.cast (ref 7)
    local.get 2
    local.set 8
    ref.cast (ref 7)
    local.set 4
    local.get 4
    ref.cast (ref 7)
    local.get 8
    struct.set 7 3
    return
  )
  (func (;5;) (type 6) (param eqref eqref) (result eqref)
    (local eqref)
    local.get 0
    ref.is_null
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 0
    ref.cast (ref 5)
    struct.get 5 0
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
      ref.cast (ref 4)
      struct.get 4 1
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
  (data (;0;) "f\00i\00r\00s\00t\00")
  (data (;1;) "c\00o\00p\00y\00")
  (data (;2;) "s\00e\00c\00o\00n\00d\00")
  (data (;3;) "o\00t\00h\00e\00r\00")
)
```
