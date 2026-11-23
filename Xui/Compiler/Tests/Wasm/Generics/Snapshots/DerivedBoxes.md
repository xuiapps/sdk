# Generic inheritance and virtual dispatch

Source: [DerivedBoxes.cs](Fixtures/Generics/DerivedBoxes.cs)

## Source

```csharp
using System;

public abstract class Box<T>
{
    public T Value;

    protected Box(T value) { Value = value; }
    public virtual T Read() => Value;
    public abstract T Echo(T value);
}

public class TaggedBox<TTag, T> : Box<T>
{
    public TTag Tag;

    public TaggedBox(T value, TTag tag) : base(value) { Tag = tag; }
    public override T Read() => base.Read();
    public override T Echo(T value) => value;
}

public sealed class IntBox : TaggedBox<string, int>
{
    public int Extra;

    public IntBox(int value, int extra) : base(value, "number") { Extra = extra; }
    public override int Read() => base.Read() + Extra;
}

public static class Program
{
    private static int ReadNumber(Box<int> box)
    {
        Console.WriteLine(box.Read());
        Console.WriteLine(box.Echo(7));
        Console.WriteLine(box is IntBox);
        Console.WriteLine(box is TaggedBox<string, int>);
        var tagged = box as TaggedBox<string, int>;
        Console.WriteLine(tagged.Tag);
        return box.Read();
    }

    private static void ReadText(Box<string> box)
    {
        Console.WriteLine(box.Read());
        Console.WriteLine(box.Echo("World"));
        Console.WriteLine(box is TaggedBox<int, string>);
        Console.WriteLine((box as TaggedBox<string, string>) == null);
    }

    public static int Main()
    {
        var number = new IntBox(40, 2);
        int result = ReadNumber(number);
        ReadText(new TaggedBox<int, string>("Hello", 3));
        number.Value = 50;
        Console.WriteLine(number.Read());
        return result;
    }
}
```

## Output

```text
42
7
True
True
number
Hello
World
True
True
52
```

## Exit Code

```text
42
```

## Runtime Types

### `Box<Int32>`

- Base: `System.Object`
- WASM type: `8`
- Table start: `0`

- Field `Box<Int32>.Value`: `Int32`, first `1`, count `1`

- Slot 0: `Box<Int32>.Read`
- Slot 1: `Box<Int32>.Echo`

### `TaggedBox<String, Int32>`

- Base: `Box<Int32>`
- WASM type: `9`
- Table start: `2`

- Field `Box<Int32>.Value`: `Int32`, first `1`, count `1`
- Field `TaggedBox<String, Int32>.Tag`: `String`, first `2`, count `1`

- Slot 0: `TaggedBox<String, Int32>.Read`
- Slot 1: `TaggedBox<String, Int32>.Echo`

### `IntBox`

- Base: `TaggedBox<String, Int32>`
- WASM type: `10`
- Table start: `4`

- Field `Box<Int32>.Value`: `Int32`, first `1`, count `1`
- Field `TaggedBox<String, Int32>.Tag`: `String`, first `2`, count `1`
- Field `IntBox.Extra`: `Int32`, first `3`, count `1`

- Slot 0: `IntBox.Read`
- Slot 1: `TaggedBox<String, Int32>.Echo`

### `Box<String>`

- Base: `System.Object`
- WASM type: `11`
- Table start: `6`

- Field `Box<String>.Value`: `String`, first `1`, count `1`

- Slot 0: `Box<String>.Read`
- Slot 1: `Box<String>.Echo`

### `TaggedBox<Int32, String>`

- Base: `Box<String>`
- WASM type: `12`
- Table start: `8`

- Field `Box<String>.Value`: `String`, first `1`, count `1`
- Field `TaggedBox<Int32, String>.Tag`: `Int32`, first `2`, count `1`

- Slot 0: `TaggedBox<Int32, String>.Read`
- Slot 1: `TaggedBox<Int32, String>.Echo`

### `TaggedBox<String, String>`

- Base: `Box<String>`
- WASM type: `13`
- Table start: `10`

- Field `Box<String>.Value`: `String`, first `1`, count `1`
- Field `TaggedBox<String, String>.Tag`: `String`, first `2`, count `1`

- Slot 0: `TaggedBox<String, String>.Read`
- Slot 1: `TaggedBox<String, String>.Echo`

## WAT

```wat
(module
  (type (;0;) (array (mut i16)))
  (type (;1;) (func (param (ref null 0) i32 i32) (result (ref extern))))
  (type (;2;) (func (param i32)))
  (type (;3;) (func (param i32)))
  (type (;4;) (func (param externref)))
  (type (;5;) (struct (field i32) (field eqref) (field i32) (field (ref null 0)) (field i32) (field i32)))
  (type (;6;) (sub (struct (field (mut eqref)))))
  (type (;7;) (func (param eqref eqref) (result eqref)))
  (type (;8;) (sub 6 (struct (field (mut eqref)) (field (mut i32)))))
  (type (;9;) (sub 8 (struct (field (mut eqref)) (field (mut i32)) (field (mut externref)))))
  (type (;10;) (sub 9 (struct (field (mut eqref)) (field (mut i32)) (field (mut externref)) (field (mut i32)))))
  (type (;11;) (sub 6 (struct (field (mut eqref)) (field (mut externref)))))
  (type (;12;) (sub 11 (struct (field (mut eqref)) (field (mut externref)) (field (mut i32)))))
  (type (;13;) (sub 11 (struct (field (mut eqref)) (field (mut externref)) (field (mut externref)))))
  (type (;14;) (func (result i32)))
  (type (;15;) (func (param eqref i32 i32)))
  (type (;16;) (func (param eqref) (result i32)))
  (type (;17;) (func (param eqref externref i32)))
  (type (;18;) (func (param eqref)))
  (type (;19;) (func (param eqref i32 externref)))
  (type (;20;) (func (param eqref externref)))
  (type (;21;) (func (param eqref) (result i32)))
  (type (;22;) (func (param eqref) (result i32)))
  (type (;23;) (func (param eqref i32)))
  (type (;24;) (func (param eqref i32) (result i32)))
  (type (;25;) (func (param eqref i32) (result i32)))
  (type (;26;) (func (param eqref) (result externref)))
  (type (;27;) (func (param eqref) (result externref)))
  (type (;28;) (func (param eqref externref) (result externref)))
  (type (;29;) (func (param eqref externref) (result externref)))
  (type (;30;) (func (param eqref) (result i32)))
  (import "xui" "console-log" (func (;0;) (type 2) (param i32)))
  (import "xui" "console-log-boolean" (func (;1;) (type 3) (param i32)))
  (import "xui" "console-log" (func (;2;) (type 4) (param externref)))
  (import "wasm:js-string" "fromCharCodeArray" (func (;3;) (type 1) (param (ref null 0) i32 i32) (result (ref extern))))
  (table (;0;) 12 12 funcref)
  (global (;0;) (mut externref) ref.null extern)
  (global (;1;) (mut externref) ref.null extern)
  (global (;2;) (mut externref) ref.null extern)
  (global (;3;) eqref i32.const 1 ref.null eq i32.const 0 i32.const 66 i32.const 111 i32.const 120 i32.const 60 i32.const 73 i32.const 110 i32.const 116 i32.const 51 i32.const 50 i32.const 62 array.new_fixed 0 10 i32.const 2 i32.const 2 struct.new 5)
  (global (;4;) eqref i32.const 2 global.get 3 i32.const 2 i32.const 84 i32.const 97 i32.const 103 i32.const 103 i32.const 101 i32.const 100 i32.const 66 i32.const 111 i32.const 120 i32.const 60 i32.const 83 i32.const 116 i32.const 114 i32.const 105 i32.const 110 i32.const 103 i32.const 44 i32.const 32 i32.const 73 i32.const 110 i32.const 116 i32.const 51 i32.const 50 i32.const 62 array.new_fixed 0 24 i32.const 2 i32.const 3 struct.new 5)
  (global (;5;) eqref i32.const 3 global.get 4 i32.const 4 i32.const 73 i32.const 110 i32.const 116 i32.const 66 i32.const 111 i32.const 120 array.new_fixed 0 6 i32.const 2 i32.const 4 struct.new 5)
  (global (;6;) eqref i32.const 4 ref.null eq i32.const 6 i32.const 66 i32.const 111 i32.const 120 i32.const 60 i32.const 83 i32.const 116 i32.const 114 i32.const 105 i32.const 110 i32.const 103 i32.const 62 array.new_fixed 0 11 i32.const 2 i32.const 2 struct.new 5)
  (global (;7;) eqref i32.const 5 global.get 6 i32.const 8 i32.const 84 i32.const 97 i32.const 103 i32.const 103 i32.const 101 i32.const 100 i32.const 66 i32.const 111 i32.const 120 i32.const 60 i32.const 73 i32.const 110 i32.const 116 i32.const 51 i32.const 50 i32.const 44 i32.const 32 i32.const 83 i32.const 116 i32.const 114 i32.const 105 i32.const 110 i32.const 103 i32.const 62 array.new_fixed 0 24 i32.const 2 i32.const 3 struct.new 5)
  (global (;8;) eqref i32.const 6 global.get 6 i32.const 10 i32.const 84 i32.const 97 i32.const 103 i32.const 103 i32.const 101 i32.const 100 i32.const 66 i32.const 111 i32.const 120 i32.const 60 i32.const 83 i32.const 116 i32.const 114 i32.const 105 i32.const 110 i32.const 103 i32.const 44 i32.const 32 i32.const 83 i32.const 116 i32.const 114 i32.const 105 i32.const 110 i32.const 103 i32.const 62 array.new_fixed 0 25 i32.const 2 i32.const 3 struct.new 5)
  (export "main" (func 4))
  (elem (;0;) (i32.const 0) func 11)
  (elem (;1;) (i32.const 2) func 18)
  (elem (;2;) (i32.const 3) func 14)
  (elem (;3;) (i32.const 4) func 12)
  (elem (;4;) (i32.const 5) func 14)
  (elem (;5;) (i32.const 6) func 15)
  (elem (;6;) (i32.const 8) func 16)
  (elem (;7;) (i32.const 9) func 17)
  (func (;4;) (type 14) (result i32)
    (local i32 i32 i64 f64 externref eqref i32 i32 eqref externref i32 eqref eqref eqref i32 i64 f64 externref)
    i32.const 40
    i32.const 2
    local.set 7
    local.set 6
    struct.new_default 10
    local.set 5
    local.get 5
    ref.cast (ref 10)
    global.get 5
    struct.set 10 0
    local.get 5
    local.get 6
    local.get 7
    call 5
    local.get 5
    local.tee 12
    local.get 12
    call 6
    local.set 0
    global.get 0
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 5
      array.new_data 0 0
      i32.const 0
      i32.const 5
      call 3
      global.set 0
    end
    global.get 0
    i32.const 3
    local.set 10
    local.set 9
    struct.new_default 12
    local.set 8
    local.get 8
    ref.cast (ref 12)
    global.get 7
    struct.set 12 0
    local.get 8
    local.get 9
    local.get 10
    call 7
    local.get 8
    call 8
    local.tee 12
    local.get 12
    i32.const 50
    local.set 14
    ref.cast (ref 10)
    local.set 13
    local.get 13
    ref.cast (ref 10)
    local.get 14
    struct.set 10 1
    local.set 11
    local.get 11
    ref.cast (ref 8)
    local.get 11
    ref.cast (ref 6)
    struct.get 6 0
    ref.cast (ref 5)
    struct.get 5 2
    i32.const 0
    i32.add
    call_indirect (type 21)
    call 0
    local.get 0
    return
  )
  (func (;5;) (type 15) (param eqref i32 i32)
    (local eqref i32 externref eqref eqref i32 i64 f64 externref)
    local.get 0
    local.get 1
    global.get 1
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 6
      array.new_data 0 1
      i32.const 0
      i32.const 6
      call 3
      global.set 1
    end
    global.get 1
    local.set 5
    local.set 4
    local.set 3
    local.get 3
    local.get 4
    local.get 5
    call 9
    local.get 0
    local.get 2
    local.set 8
    ref.cast (ref 10)
    local.set 7
    local.get 7
    ref.cast (ref 10)
    local.get 8
    struct.set 10 3
    return
  )
  (func (;6;) (type 16) (param eqref) (result i32)
    (local eqref eqref i32 eqref eqref i32 i64 f64 externref)
    local.get 0
    local.set 1
    local.get 1
    ref.cast (ref 8)
    local.get 1
    ref.cast (ref 6)
    struct.get 6 0
    ref.cast (ref 5)
    struct.get 5 2
    i32.const 0
    i32.add
    call_indirect (type 21)
    call 0
    local.get 0
    i32.const 7
    local.set 3
    local.set 2
    local.get 2
    ref.cast (ref 8)
    local.get 3
    local.get 2
    ref.cast (ref 6)
    struct.get 6 0
    ref.cast (ref 5)
    struct.get 5 2
    i32.const 1
    i32.add
    call_indirect (type 24)
    call 0
    local.get 0
    global.get 5
    call 19
    ref.null eq
    drop
    ref.is_null
    i32.eqz
    call 1
    local.get 0
    global.get 4
    call 19
    ref.null eq
    drop
    ref.is_null
    i32.eqz
    call 1
    local.get 0
    global.get 4
    call 19
    ref.cast (ref 9)
    struct.get 9 2
    call 2
    local.get 0
    local.set 1
    local.get 1
    ref.cast (ref 8)
    local.get 1
    ref.cast (ref 6)
    struct.get 6 0
    ref.cast (ref 5)
    struct.get 5 2
    i32.const 0
    i32.add
    call_indirect (type 21)
    return
  )
  (func (;7;) (type 17) (param eqref externref i32)
    (local eqref externref eqref eqref i32 i64 f64 externref)
    local.get 0
    local.get 1
    local.set 4
    local.set 3
    local.get 3
    local.get 4
    call 10
    local.get 0
    local.get 2
    local.set 7
    ref.cast (ref 12)
    local.set 6
    local.get 6
    ref.cast (ref 12)
    local.get 7
    struct.set 12 2
    return
  )
  (func (;8;) (type 18) (param eqref)
    (local eqref eqref externref eqref eqref i32 i64 f64 externref)
    local.get 0
    local.set 1
    local.get 1
    ref.cast (ref 11)
    local.get 1
    ref.cast (ref 6)
    struct.get 6 0
    ref.cast (ref 5)
    struct.get 5 2
    i32.const 0
    i32.add
    call_indirect (type 26)
    call 2
    local.get 0
    global.get 2
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 5
      array.new_data 0 2
      i32.const 0
      i32.const 5
      call 3
      global.set 2
    end
    global.get 2
    local.set 3
    local.set 2
    local.get 2
    ref.cast (ref 11)
    local.get 3
    local.get 2
    ref.cast (ref 6)
    struct.get 6 0
    ref.cast (ref 5)
    struct.get 5 2
    i32.const 1
    i32.add
    call_indirect (type 28)
    call 2
    local.get 0
    global.get 7
    call 19
    ref.null eq
    drop
    ref.is_null
    i32.eqz
    call 1
    local.get 0
    global.get 8
    call 19
    ref.null eq
    drop
    ref.is_null
    call 1
    return
  )
  (func (;9;) (type 19) (param eqref i32 externref)
    (local eqref i32 eqref eqref i32 i64 f64 externref)
    local.get 0
    local.get 1
    local.set 4
    local.set 3
    local.get 3
    local.get 4
    call 13
    local.get 0
    local.get 2
    local.set 10
    ref.cast (ref 9)
    local.set 6
    local.get 6
    ref.cast (ref 9)
    local.get 10
    struct.set 9 2
    return
  )
  (func (;10;) (type 20) (param eqref externref)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    drop
    local.get 0
    local.get 1
    local.set 7
    ref.cast (ref 11)
    local.set 3
    local.get 3
    ref.cast (ref 11)
    local.get 7
    struct.set 11 1
    return
  )
  (func (;11;) (type 21) (param eqref) (result i32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    ref.cast (ref 8)
    struct.get 8 1
    return
  )
  (func (;12;) (type 22) (param eqref) (result i32)
    (local eqref eqref eqref i32 i64 f64 externref)
    local.get 0
    local.set 1
    local.get 1
    call 18
    local.get 0
    ref.cast (ref 10)
    struct.get 10 3
    i32.add
    return
  )
  (func (;13;) (type 23) (param eqref i32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    drop
    local.get 0
    local.get 1
    local.set 4
    ref.cast (ref 8)
    local.set 3
    local.get 3
    ref.cast (ref 8)
    local.get 4
    struct.set 8 1
    return
  )
  (func (;14;) (type 25) (param eqref i32) (result i32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 1
    return
  )
  (func (;15;) (type 26) (param eqref) (result externref)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    ref.cast (ref 11)
    struct.get 11 1
    return
  )
  (func (;16;) (type 27) (param eqref) (result externref)
    (local eqref eqref eqref i32 i64 f64 externref)
    local.get 0
    local.set 1
    local.get 1
    call 15
    return
  )
  (func (;17;) (type 29) (param eqref externref) (result externref)
    (local eqref eqref i32 i64 f64 externref)
    local.get 1
    return
  )
  (func (;18;) (type 30) (param eqref) (result i32)
    (local eqref eqref eqref i32 i64 f64 externref)
    local.get 0
    local.set 1
    local.get 1
    call 11
    return
  )
  (func (;19;) (type 7) (param eqref eqref) (result eqref)
    (local eqref)
    local.get 0
    ref.is_null
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 0
    ref.cast (ref 6)
    struct.get 6 0
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
      ref.cast (ref 5)
      struct.get 5 1
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
  (data (;0;) "H\00e\00l\00l\00o\00")
  (data (;1;) "n\00u\00m\00b\00e\00r\00")
  (data (;2;) "W\00o\00r\00l\00d\00")
)
```
