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
  (type (;4;) (struct (field i32) (field externref)))
  (type (;5;) (func (param eqref)))
  (type (;6;) (func (result eqref)))
  (type (;7;) (func (result eqref exnref)))
  (type (;8;) (func (result exnref)))
  (type (;9;) (func))
  (type (;10;) (struct (field i32) (field eqref) (field i32) (field (ref null 0)) (field i32) (field i32)))
  (type (;11;) (sub (struct (field (mut eqref)))))
  (type (;12;) (func (param eqref eqref) (result eqref)))
  (type (;13;) (sub 11 (struct (field (mut eqref)) (field (mut i32)) (field (mut i32)) (field (mut externref)) (field (mut i32)) (field (mut eqref)))))
  (type (;14;) (struct (field (mut i32)) (field (mut i32))))
  (type (;15;) (struct (field (mut externref)) (field (mut i32)) (field (mut eqref))))
  (type (;16;) (struct (field (mut externref)) (field (mut i32))))
  (type (;17;) (func (result i32)))
  (type (;18;) (func (param eqref i32) (result eqref)))
  (type (;19;) (func (param eqref i32 eqref)))
  (type (;20;) (func (param eqref i32) (result externref)))
  (type (;21;) (func (param eqref i32 externref)))
  (type (;22;) (func (param eqref i32) (result i32)))
  (type (;23;) (func (param eqref i32 i32)))
  (type (;24;) (func (result i32)))
  (type (;25;) (func (param eqref i32 externref)))
  (import "xui" "console-log" (func (;0;) (type 2) (param i32)))
  (import "xui" "console-log" (func (;1;) (type 3) (param externref)))
  (import "wasm:js-string" "fromCharCodeArray" (func (;2;) (type 1) (param (ref null 0) i32 i32) (result (ref extern))))
  (table (;0;) 65536 65536 eqref)
  (table (;1;) 65536 65536 externref)
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
  (global (;8;) (mut externref) ref.null extern)
  (global (;9;) (mut externref) ref.null extern)
  (global (;10;) (mut i32) i32.const 16)
  (global (;11;) (mut i32) i32.const 0)
  (global (;12;) eqref i32.const 1 ref.null eq i32.const 0 i32.const 87 i32.const 105 i32.const 100 i32.const 103 i32.const 101 i32.const 116 array.new_fixed 0 6 i32.const 0 i32.const 6 struct.new 10)
  (export "main" (func 3))
  (func (;3;) (type 24) (result i32)
    (local eqref eqref i32 eqref i32 eqref i32 i32 i64 f64 externref eqref i32 externref exnref eqref i32 i32 i32 i32 i32 i32 i64 f64 eqref eqref i32 i64 f64 externref)
    global.get 10
    local.tee 19
    i32.const 1048544
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 19
    i32.const 32
    i32.add
    global.set 10
    local.get 19
    i32.const 0
    i32.add
    local.set 2
    local.get 2
    i32.const 0
    i32.const 8
    memory.fill
    struct.new_default 16
    local.set 1
    block (type 8) (result exnref) ;; label = @1
      try_table (catch_all_ref 0 (;@1;)) ;; label = @2
        i32.const 1
        global.get 0
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 5
          array.new_data 0 0
          i32.const 0
          i32.const 5
          call 2
          global.set 0
        end
        global.get 0
        local.set 13
        local.set 12
        struct.new_default 13
        local.set 11
        local.get 11
        ref.cast (ref 13)
        global.get 12
        struct.set 13 0
        local.get 11
        local.get 12
        local.get 13
        call 4
        local.get 11
        local.set 0
        local.get 0
        local.tee 25
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 7
            i32.const 0
            i32.const 53
            call 2
            global.set 7
          end
          global.get 7
          struct.new 4
          throw 0
        end
        local.get 25
        ref.cast (ref 13)
        i32.const 1
        i32.const 4
        local.set 26
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
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 7
            i32.const 0
            i32.const 53
            call 2
            global.set 7
          end
          global.get 7
          struct.new 4
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
        else
          local.get 4
          i32.const 0
          i32.add
        end
        local.get 26
        call 10
        local.get 0
        local.tee 25
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 7
            i32.const 0
            i32.const 53
            call 2
            global.set 7
          end
          global.get 7
          struct.new 4
          throw 0
        end
        local.get 25
        ref.cast (ref 13)
        i32.const 3
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
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 7
            i32.const 0
            i32.const 53
            call 2
            global.set 7
          end
          global.get 7
          struct.new 4
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
        else
          local.get 4
          i32.const 0
          i32.add
        end
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
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 7
            i32.const 0
            i32.const 53
            call 2
            global.set 7
          end
          global.get 7
          struct.new 4
          throw 0
        end
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 7
            i32.const 0
            i32.const 53
            call 2
            global.set 7
          end
          global.get 7
          struct.new 4
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
          i32.load align=1
        else
          local.get 4
          i32.const 0
          i32.add
        end
        call 7
        local.get 3
        ref.is_null
        local.get 4
        i32.const -1
        i32.eq
        i32.and
        if ;; label = @3
          i32.const 4
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 7
            i32.const 0
            i32.const 53
            call 2
            global.set 7
          end
          global.get 7
          struct.new 4
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
        else
          local.get 4
          i32.const 1
          i32.add
        end
        call 9
        struct.new 16
        local.set 24
        local.get 1
        local.set 25
        local.get 25
        ref.cast (ref 16)
        local.get 24
        ref.cast (ref 16)
        struct.get 16 0
        struct.set 16 0
        local.get 25
        ref.cast (ref 16)
        local.get 24
        ref.cast (ref 16)
        struct.get 16 1
        struct.set 16 1
        local.get 1
        i32.const 6
        global.get 1
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 4
          array.new_data 0 1
          i32.const 0
          i32.const 4
          call 2
          global.set 1
        end
        global.get 1
        local.set 29
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
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 7
            i32.const 0
            i32.const 53
            call 2
            global.set 7
          end
          global.get 7
          struct.new 4
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
          i32.load align=1
        else
          local.get 4
          i32.const 0
          i32.add
        end
        local.get 29
        call 8
        local.get 1
        i32.const 6
        i32.const 7
        local.set 26
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
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 7
            i32.const 0
            i32.const 53
            call 2
            global.set 7
          end
          global.get 7
          struct.new 4
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 4
          i32.const 4
          i32.add
        else
          local.get 4
          i32.const 1
          i32.add
        end
        local.get 26
        call 10
        local.get 0
        local.tee 25
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 7
            i32.const 0
            i32.const 53
            call 2
            global.set 7
          end
          global.get 7
          struct.new 4
          throw 0
        end
        local.get 25
        ref.cast (ref 13)
        i32.const 3
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
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 7
            i32.const 0
            i32.const 53
            call 2
            global.set 7
          end
          global.get 7
          struct.new 4
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
        else
          local.get 4
          i32.const 0
          i32.add
        end
        i32.const 2
        local.set 26
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
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 7
            i32.const 0
            i32.const 53
            call 2
            global.set 7
          end
          global.get 7
          struct.new 4
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 4
          i32.const 4
          i32.add
        else
          local.get 4
          i32.const 1
          i32.add
        end
        local.get 26
        call 10
        local.get 0
        local.tee 25
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 7
            i32.const 0
            i32.const 53
            call 2
            global.set 7
          end
          global.get 7
          struct.new 4
          throw 0
        end
        local.get 25
        ref.cast (ref 13)
        i32.const 3
        local.get 0
        local.set 24
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
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 7
            i32.const 0
            i32.const 53
            call 2
            global.set 7
          end
          global.get 7
          struct.new 4
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 4
          i32.const 8
          i32.add
          i32.load align=1
        else
          local.get 4
          i32.const 2
          i32.add
        end
        local.get 24
        call 6
        i32.const 9
        global.get 2
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 6
          array.new_data 0 2
          i32.const 0
          i32.const 6
          call 2
          global.set 2
        end
        global.get 2
        local.set 13
        local.set 12
        struct.new_default 13
        local.set 11
        local.get 11
        ref.cast (ref 13)
        global.get 12
        struct.set 13 0
        local.get 11
        local.get 12
        local.get 13
        call 4
        local.get 11
        local.tee 24
        local.get 24
        local.get 0
        local.tee 25
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 7
            i32.const 0
            i32.const 53
            call 2
            global.set 7
          end
          global.get 7
          struct.new 4
          throw 0
        end
        local.get 25
        ref.cast (ref 13)
        local.set 25
        local.get 25
        ref.cast (ref 13)
        struct.get 13 3
        local.get 25
        ref.cast (ref 13)
        struct.get 13 4
        local.get 25
        ref.cast (ref 13)
        struct.get 13 5
        struct.new 15
        local.set 24
        local.tee 25
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 7
            i32.const 0
            i32.const 53
            call 2
            global.set 7
          end
          global.get 7
          struct.new 4
          throw 0
        end
        local.get 25
        ref.cast (ref 13)
        local.set 25
        local.get 25
        ref.cast (ref 13)
        local.get 24
        ref.cast (ref 15)
        struct.get 15 0
        struct.set 13 3
        local.get 25
        ref.cast (ref 13)
        local.get 24
        ref.cast (ref 15)
        struct.get 15 1
        struct.set 13 4
        local.get 25
        ref.cast (ref 13)
        local.get 24
        ref.cast (ref 15)
        struct.get 15 2
        struct.set 13 5
        local.tee 24
        local.get 24
        local.tee 25
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 7
            i32.const 0
            i32.const 53
            call 2
            global.set 7
          end
          global.get 7
          struct.new 4
          throw 0
        end
        local.get 25
        ref.cast (ref 13)
        i32.const 3
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
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 7
            i32.const 0
            i32.const 53
            call 2
            global.set 7
          end
          global.get 7
          struct.new 4
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
        else
          local.get 4
          i32.const 0
          i32.add
        end
        global.get 3
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 5
          array.new_data 0 3
          i32.const 0
          i32.const 5
          call 2
          global.set 3
        end
        global.get 3
        local.set 29
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
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 7
            i32.const 0
            i32.const 53
            call 2
            global.set 7
          end
          global.get 7
          struct.new 4
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
          i32.load align=1
        else
          local.get 4
          i32.const 0
          i32.add
        end
        local.get 29
        call 8
        local.tee 24
        local.get 24
        local.tee 25
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 7
            i32.const 0
            i32.const 53
            call 2
            global.set 7
          end
          global.get 7
          struct.new 4
          throw 0
        end
        local.get 25
        ref.cast (ref 13)
        i32.const 3
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
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 7
            i32.const 0
            i32.const 53
            call 2
            global.set 7
          end
          global.get 7
          struct.new 4
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 4
          i32.const 8
          i32.add
          i32.load align=1
        else
          local.get 4
          i32.const 2
          i32.add
        end
        call 5
        local.tee 25
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 7
            i32.const 0
            i32.const 53
            call 2
            global.set 7
          end
          global.get 7
          struct.new 4
          throw 0
        end
        local.get 25
        ref.cast (ref 13)
        i32.const 1
        i32.const 6
        local.set 26
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
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 7
            i32.const 0
            i32.const 53
            call 2
            global.set 7
          end
          global.get 7
          struct.new 4
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 4
          i32.const 4
          i32.add
        else
          local.get 4
          i32.const 1
          i32.add
        end
        local.get 26
        call 10
        local.get 0
        local.tee 25
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 7
            i32.const 0
            i32.const 53
            call 2
            global.set 7
          end
          global.get 7
          struct.new 4
          throw 0
        end
        local.get 25
        ref.cast (ref 13)
        i32.const 1
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
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 7
            i32.const 0
            i32.const 53
            call 2
            global.set 7
          end
          global.get 7
          struct.new 4
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
        else
          local.get 4
          i32.const 0
          i32.add
        end
        call 9
        call 0
        local.get 0
        local.tee 25
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 7
            i32.const 0
            i32.const 53
            call 2
            global.set 7
          end
          global.get 7
          struct.new 4
          throw 0
        end
        local.get 25
        ref.cast (ref 13)
        i32.const 1
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
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 7
            i32.const 0
            i32.const 53
            call 2
            global.set 7
          end
          global.get 7
          struct.new 4
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 4
          i32.const 4
          i32.add
        else
          local.get 4
          i32.const 1
          i32.add
        end
        call 9
        call 0
        local.get 0
        local.tee 25
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 7
            i32.const 0
            i32.const 53
            call 2
            global.set 7
          end
          global.get 7
          struct.new 4
          throw 0
        end
        local.get 25
        ref.cast (ref 13)
        i32.const 3
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
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 7
            i32.const 0
            i32.const 53
            call 2
            global.set 7
          end
          global.get 7
          struct.new 4
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
        else
          local.get 4
          i32.const 0
          i32.add
        end
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
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 7
            i32.const 0
            i32.const 53
            call 2
            global.set 7
          end
          global.get 7
          struct.new 4
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
          i32.load align=1
        else
          local.get 4
          i32.const 0
          i32.add
        end
        call 7
        call 1
        local.get 1
        ref.cast (ref 16)
        local.set 24
        local.get 24
        ref.cast (ref 16)
        struct.get 16 0
        local.get 24
        ref.cast (ref 16)
        struct.get 16 1
        struct.new 16
        ref.cast (ref 16)
        struct.get 16 0
        call 1
        local.tee 25
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 7
            i32.const 0
            i32.const 53
            call 2
            global.set 7
          end
          global.get 7
          struct.new 4
          throw 0
        end
        local.get 25
        ref.cast (ref 13)
        i32.const 3
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
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 7
            i32.const 0
            i32.const 53
            call 2
            global.set 7
          end
          global.get 7
          struct.new 4
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
        else
          local.get 4
          i32.const 0
          i32.add
        end
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
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 7
            i32.const 0
            i32.const 53
            call 2
            global.set 7
          end
          global.get 7
          struct.new 4
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
          i32.load align=1
        else
          local.get 4
          i32.const 0
          i32.add
        end
        call 7
        call 1
        local.get 0
        local.tee 25
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 7
            i32.const 0
            i32.const 53
            call 2
            global.set 7
          end
          global.get 7
          struct.new 4
          throw 0
        end
        local.get 25
        ref.cast (ref 13)
        i32.const 3
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
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 7
            i32.const 0
            i32.const 53
            call 2
            global.set 7
          end
          global.get 7
          struct.new 4
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
        else
          local.get 4
          i32.const 0
          i32.add
        end
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
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 7
            i32.const 0
            i32.const 53
            call 2
            global.set 7
          end
          global.get 7
          struct.new 4
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 4
          i32.const 4
          i32.add
        else
          local.get 4
          i32.const 1
          i32.add
        end
        call 9
        call 0
        local.get 1
        ref.cast (ref 16)
        local.set 24
        local.get 24
        ref.cast (ref 16)
        struct.get 16 0
        local.get 24
        ref.cast (ref 16)
        struct.get 16 1
        struct.new 16
        ref.cast (ref 16)
        struct.get 16 1
        call 0
        local.get 0
        local.tee 25
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 7
            i32.const 0
            i32.const 53
            call 2
            global.set 7
          end
          global.get 7
          struct.new 4
          throw 0
        end
        local.get 25
        ref.cast (ref 13)
        local.set 25
        local.get 19
        i32.const 8
        i32.add
        local.get 25
        ref.cast (ref 13)
        struct.get 13 1
        i32.store align=1
        local.get 19
        i32.const 12
        i32.add
        local.get 25
        ref.cast (ref 13)
        struct.get 13 2
        i32.store align=1
        local.get 19
        i32.const 8
        i32.add
        local.set 20
        local.get 2
        local.get 20
        i32.const 8
        memory.copy
        local.get 2
        local.set 4
        ref.null eq
        local.get 4
        i32.const 20
        local.set 26
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
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 7
            i32.const 0
            i32.const 53
            call 2
            global.set 7
          end
          global.get 7
          struct.new 4
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
        else
          local.get 4
          i32.const 0
          i32.add
        end
        local.get 26
        call 10
        local.get 0
        local.get 2
        local.set 20
        local.get 19
        i32.const 16
        i32.add
        local.get 20
        i32.const 8
        memory.copy
        local.get 19
        i32.const 16
        i32.add
        local.set 26
        local.tee 25
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 7
            i32.const 0
            i32.const 53
            call 2
            global.set 7
          end
          global.get 7
          struct.new 4
          throw 0
        end
        local.get 25
        ref.cast (ref 13)
        local.set 25
        local.get 25
        ref.cast (ref 13)
        local.get 26
        i32.const 0
        i32.add
        i32.load align=1
        struct.set 13 1
        local.get 25
        ref.cast (ref 13)
        local.get 26
        i32.const 4
        i32.add
        i32.load align=1
        struct.set 13 2
        local.get 2
        local.set 4
        ref.null eq
        local.get 4
        i32.const 99
        local.set 26
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
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 7
            i32.const 0
            i32.const 53
            call 2
            global.set 7
          end
          global.get 7
          struct.new 4
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
        else
          local.get 4
          i32.const 0
          i32.add
        end
        local.get 26
        call 10
        local.get 0
        local.tee 25
        ref.is_null
        if ;; label = @3
          i32.const 4
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 7
            i32.const 0
            i32.const 53
            call 2
            global.set 7
          end
          global.get 7
          struct.new 4
          throw 0
        end
        local.get 25
        ref.cast (ref 13)
        i32.const 1
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
          global.get 7
          ref.is_null
          if ;; label = @4
            i32.const 0
            i32.const 53
            array.new_data 0 7
            i32.const 0
            i32.const 53
            call 2
            global.set 7
          end
          global.get 7
          struct.new 4
          throw 0
        end
        local.get 3
        local.get 3
        ref.is_null
        if (type 17) (result i32) ;; label = @3
          local.get 4
          i32.const 0
          i32.add
        else
          local.get 4
          i32.const 0
          i32.add
        end
        call 9
        local.get 19
        global.set 10
        return
        unreachable
      end
      unreachable
    end
    local.set 14
    local.get 19
    global.set 10
    local.get 14
    throw_ref
  )
  (func (;4;) (type 25) (param eqref i32 externref)
    (local eqref i32 eqref i32 eqref eqref i32 i64 f64 externref)
    local.get 0
    drop
    local.get 0
    local.tee 8
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 7
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 7
        i32.const 0
        i32.const 53
        call 2
        global.set 7
      end
      global.get 7
      struct.new 4
      throw 0
    end
    local.get 8
    ref.cast (ref 13)
    i32.const 1
    local.get 1
    local.set 9
    local.set 4
    local.set 3
    local.get 3
    ref.is_null
    local.get 4
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      i32.const 4
      global.get 7
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 7
        i32.const 0
        i32.const 53
        call 2
        global.set 7
      end
      global.get 7
      struct.new 4
      throw 0
    end
    local.get 3
    local.get 3
    ref.is_null
    if (type 17) (result i32) ;; label = @1
      local.get 4
      i32.const 0
      i32.add
    else
      local.get 4
      i32.const 0
      i32.add
    end
    local.get 9
    call 10
    local.get 0
    local.tee 8
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 7
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 7
        i32.const 0
        i32.const 53
        call 2
        global.set 7
      end
      global.get 7
      struct.new 4
      throw 0
    end
    local.get 8
    ref.cast (ref 13)
    i32.const 3
    local.set 4
    local.set 3
    local.get 3
    ref.is_null
    local.get 4
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      i32.const 4
      global.get 7
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 7
        i32.const 0
        i32.const 53
        call 2
        global.set 7
      end
      global.get 7
      struct.new 4
      throw 0
    end
    local.get 3
    local.get 3
    ref.is_null
    if (type 17) (result i32) ;; label = @1
      local.get 4
      i32.const 0
      i32.add
    else
      local.get 4
      i32.const 0
      i32.add
    end
    local.get 2
    local.set 12
    local.set 4
    local.set 3
    local.get 3
    ref.is_null
    local.get 4
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      i32.const 4
      global.get 7
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 7
        i32.const 0
        i32.const 53
        call 2
        global.set 7
      end
      global.get 7
      struct.new 4
      throw 0
    end
    local.get 3
    local.get 3
    ref.is_null
    if (type 17) (result i32) ;; label = @1
      local.get 4
      i32.const 0
      i32.add
      i32.load align=1
    else
      local.get 4
      i32.const 0
      i32.add
    end
    local.get 12
    call 8
    return
  )
  (func (;5;) (type 18) (param eqref i32) (result eqref)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 7
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 7
        i32.const 0
        i32.const 53
        call 2
        global.set 7
      end
      i32.const 4
      global.get 7
      struct.new 4
      throw 0
    end
    local.get 0
    ref.is_null
    if ;; label = @1
      local.get 1
      table.get 0
      return
    end
    local.get 1
    i32.const 0
    i32.eq
    if ;; label = @1
      local.get 0
      ref.cast (ref 13)
      struct.get 13 0
      return
    end
    local.get 1
    i32.const 5
    i32.eq
    if ;; label = @1
      local.get 0
      ref.cast (ref 13)
      struct.get 13 5
      return
    end
    unreachable
  )
  (func (;6;) (type 19) (param eqref i32 eqref)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 7
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 7
        i32.const 0
        i32.const 53
        call 2
        global.set 7
      end
      i32.const 4
      global.get 7
      struct.new 4
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
    local.get 1
    i32.const 0
    i32.eq
    if ;; label = @1
      local.get 0
      ref.cast (ref 13)
      local.get 2
      struct.set 13 0
      return
    end
    local.get 1
    i32.const 5
    i32.eq
    if ;; label = @1
      local.get 0
      ref.cast (ref 13)
      local.get 2
      struct.set 13 5
      return
    end
    unreachable
  )
  (func (;7;) (type 20) (param eqref i32) (result externref)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 7
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 7
        i32.const 0
        i32.const 53
        call 2
        global.set 7
      end
      i32.const 4
      global.get 7
      struct.new 4
      throw 0
    end
    local.get 0
    ref.is_null
    if ;; label = @1
      local.get 1
      table.get 1
      return
    end
    local.get 1
    i32.const 3
    i32.eq
    if ;; label = @1
      local.get 0
      ref.cast (ref 13)
      struct.get 13 3
      return
    end
    local.get 1
    i32.const 6
    i32.eq
    if ;; label = @1
      local.get 0
      ref.cast (ref 16)
      struct.get 16 0
      return
    end
    unreachable
  )
  (func (;8;) (type 21) (param eqref i32 externref)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 7
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 7
        i32.const 0
        i32.const 53
        call 2
        global.set 7
      end
      i32.const 4
      global.get 7
      struct.new 4
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
    local.get 1
    i32.const 3
    i32.eq
    if ;; label = @1
      local.get 0
      ref.cast (ref 13)
      local.get 2
      struct.set 13 3
      return
    end
    local.get 1
    i32.const 6
    i32.eq
    if ;; label = @1
      local.get 0
      ref.cast (ref 16)
      local.get 2
      struct.set 16 0
      return
    end
    unreachable
  )
  (func (;9;) (type 22) (param eqref i32) (result i32)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 7
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 7
        i32.const 0
        i32.const 53
        call 2
        global.set 7
      end
      i32.const 4
      global.get 7
      struct.new 4
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
      ref.cast (ref 13)
      struct.get 13 1
      return
    end
    local.get 1
    i32.const 2
    i32.eq
    if ;; label = @1
      local.get 0
      ref.cast (ref 13)
      struct.get 13 2
      return
    end
    local.get 1
    i32.const 4
    i32.eq
    if ;; label = @1
      local.get 0
      ref.cast (ref 13)
      struct.get 13 4
      return
    end
    local.get 1
    i32.const 7
    i32.eq
    if ;; label = @1
      local.get 0
      ref.cast (ref 16)
      struct.get 16 1
      return
    end
    unreachable
  )
  (func (;10;) (type 23) (param eqref i32 i32)
    local.get 0
    ref.is_null
    local.get 1
    i32.const -1
    i32.eq
    i32.and
    if ;; label = @1
      global.get 7
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 7
        i32.const 0
        i32.const 53
        call 2
        global.set 7
      end
      i32.const 4
      global.get 7
      struct.new 4
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
      ref.cast (ref 13)
      local.get 2
      struct.set 13 1
      return
    end
    local.get 1
    i32.const 2
    i32.eq
    if ;; label = @1
      local.get 0
      ref.cast (ref 13)
      local.get 2
      struct.set 13 2
      return
    end
    local.get 1
    i32.const 4
    i32.eq
    if ;; label = @1
      local.get 0
      ref.cast (ref 13)
      local.get 2
      struct.set 13 4
      return
    end
    local.get 1
    i32.const 7
    i32.eq
    if ;; label = @1
      local.get 0
      ref.cast (ref 16)
      local.get 2
      struct.set 16 1
      return
    end
    unreachable
  )
  (func (;11;) (type 12) (param eqref eqref) (result eqref)
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
  (data (;0;) "f\00i\00r\00s\00t\00")
  (data (;1;) "c\00o\00p\00y\00")
  (data (;2;) "s\00e\00c\00o\00n\00d\00")
  (data (;3;) "o\00t\00h\00e\00r\00")
  (data (;4;) "E\00x\00c\00e\00p\00t\00i\00o\00n\00 \00o\00f\00 \00t\00y\00p\00e\00 \00'\00S\00y\00s\00t\00e\00m\00.\00E\00x\00c\00e\00p\00t\00i\00o\00n\00'\00 \00w\00a\00s\00 \00t\00h\00r\00o\00w\00n\00.\00")
  (data (;5;) "S\00y\00s\00t\00e\00m\00 \00e\00r\00r\00o\00r\00.\00")
  (data (;6;) "S\00p\00e\00c\00i\00f\00i\00e\00d\00 \00c\00a\00s\00t\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00.\00")
  (data (;7;) "O\00b\00j\00e\00c\00t\00 \00r\00e\00f\00e\00r\00e\00n\00c\00e\00 \00n\00o\00t\00 \00s\00e\00t\00 \00t\00o\00 \00a\00n\00 \00i\00n\00s\00t\00a\00n\00c\00e\00 \00o\00f\00 \00a\00n\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;8;) "O\00p\00e\00r\00a\00t\00i\00o\00n\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00 \00d\00u\00e\00 \00t\00o\00 \00t\00h\00e\00 \00c\00u\00r\00r\00e\00n\00t\00 \00s\00t\00a\00t\00e\00 \00o\00f\00 \00t\00h\00e\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;9;) "V\00a\00l\00u\00e\00 \00d\00o\00e\00s\00 \00n\00o\00t\00 \00f\00a\00l\00l\00 \00w\00i\00t\00h\00i\00n\00 \00t\00h\00e\00 \00e\00x\00p\00e\00c\00t\00e\00d\00 \00r\00a\00n\00g\00e\00.\00")
)
```
