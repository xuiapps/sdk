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
- WASM type: `14`
- Table start: `0`

- Field `Box<Int32>.Value`: `Int32`, first `1`, count `1`

- Slot 0: `Box<Int32>.Read`
- Slot 1: `Box<Int32>.Echo`

### `TaggedBox<String, Int32>`

- Base: `Box<Int32>`
- WASM type: `15`
- Table start: `2`

- Field `Box<Int32>.Value`: `Int32`, first `1`, count `1`
- Field `TaggedBox<String, Int32>.Tag`: `String`, first `2`, count `1`

- Slot 0: `TaggedBox<String, Int32>.Read`
- Slot 1: `TaggedBox<String, Int32>.Echo`

### `IntBox`

- Base: `TaggedBox<String, Int32>`
- WASM type: `16`
- Table start: `4`

- Field `Box<Int32>.Value`: `Int32`, first `1`, count `1`
- Field `TaggedBox<String, Int32>.Tag`: `String`, first `2`, count `1`
- Field `IntBox.Extra`: `Int32`, first `3`, count `1`

- Slot 0: `IntBox.Read`
- Slot 1: `TaggedBox<String, Int32>.Echo`

### `Box<String>`

- Base: `System.Object`
- WASM type: `17`
- Table start: `6`

- Field `Box<String>.Value`: `String`, first `1`, count `1`

- Slot 0: `Box<String>.Read`
- Slot 1: `Box<String>.Echo`

### `TaggedBox<Int32, String>`

- Base: `Box<String>`
- WASM type: `18`
- Table start: `8`

- Field `Box<String>.Value`: `String`, first `1`, count `1`
- Field `TaggedBox<Int32, String>.Tag`: `Int32`, first `2`, count `1`

- Slot 0: `TaggedBox<Int32, String>.Read`
- Slot 1: `TaggedBox<Int32, String>.Echo`

### `TaggedBox<String, String>`

- Base: `Box<String>`
- WASM type: `19`
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
  (type (;5;) (struct (field i32) (field externref)))
  (type (;6;) (func (param eqref)))
  (type (;7;) (func (result eqref)))
  (type (;8;) (func (result eqref exnref)))
  (type (;9;) (func (result exnref)))
  (type (;10;) (func))
  (type (;11;) (struct (field i32) (field eqref) (field i32) (field (ref null 0)) (field i32) (field i32)))
  (type (;12;) (sub (struct (field (mut eqref)))))
  (type (;13;) (func (param eqref eqref) (result eqref)))
  (type (;14;) (sub 12 (struct (field (mut eqref)) (field (mut i32)))))
  (type (;15;) (sub 14 (struct (field (mut eqref)) (field (mut i32)) (field (mut externref)))))
  (type (;16;) (sub 15 (struct (field (mut eqref)) (field (mut i32)) (field (mut externref)) (field (mut i32)))))
  (type (;17;) (sub 12 (struct (field (mut eqref)) (field (mut externref)))))
  (type (;18;) (sub 17 (struct (field (mut eqref)) (field (mut externref)) (field (mut i32)))))
  (type (;19;) (sub 17 (struct (field (mut eqref)) (field (mut externref)) (field (mut externref)))))
  (type (;20;) (func (result i32)))
  (type (;21;) (func (param eqref i32 i32)))
  (type (;22;) (func (param eqref) (result i32)))
  (type (;23;) (func (param eqref externref i32)))
  (type (;24;) (func (param eqref)))
  (type (;25;) (func (param eqref i32 externref)))
  (type (;26;) (func (param eqref externref)))
  (type (;27;) (func (param eqref) (result i32)))
  (type (;28;) (func (param eqref) (result i32)))
  (type (;29;) (func (param eqref i32)))
  (type (;30;) (func (param eqref i32) (result i32)))
  (type (;31;) (func (param eqref i32) (result i32)))
  (type (;32;) (func (param eqref) (result externref)))
  (type (;33;) (func (param eqref) (result externref)))
  (type (;34;) (func (param eqref externref) (result externref)))
  (type (;35;) (func (param eqref externref) (result externref)))
  (type (;36;) (func (param eqref) (result i32)))
  (import "xui" "console-log" (func (;0;) (type 2) (param i32)))
  (import "xui" "console-log-boolean" (func (;1;) (type 3) (param i32)))
  (import "xui" "console-log" (func (;2;) (type 4) (param externref)))
  (import "wasm:js-string" "fromCharCodeArray" (func (;3;) (type 1) (param (ref null 0) i32 i32) (result (ref extern))))
  (table (;0;) 12 12 funcref)
  (tag (;0;) (type 6) (param eqref))
  (global (;0;) (mut externref) ref.null extern)
  (global (;1;) (mut externref) ref.null extern)
  (global (;2;) (mut externref) ref.null extern)
  (global (;3;) (mut externref) ref.null extern)
  (global (;4;) (mut externref) ref.null extern)
  (global (;5;) (mut externref) ref.null extern)
  (global (;6;) (mut externref) ref.null extern)
  (global (;7;) (mut externref) ref.null extern)
  (global (;8;) (mut externref) ref.null extern)
  (global (;9;) eqref i32.const 1 ref.null eq i32.const 0 i32.const 66 i32.const 111 i32.const 120 i32.const 60 i32.const 73 i32.const 110 i32.const 116 i32.const 51 i32.const 50 i32.const 62 array.new_fixed 0 10 i32.const 2 i32.const 2 struct.new 11)
  (global (;10;) eqref i32.const 2 global.get 9 i32.const 2 i32.const 84 i32.const 97 i32.const 103 i32.const 103 i32.const 101 i32.const 100 i32.const 66 i32.const 111 i32.const 120 i32.const 60 i32.const 83 i32.const 116 i32.const 114 i32.const 105 i32.const 110 i32.const 103 i32.const 44 i32.const 32 i32.const 73 i32.const 110 i32.const 116 i32.const 51 i32.const 50 i32.const 62 array.new_fixed 0 24 i32.const 2 i32.const 3 struct.new 11)
  (global (;11;) eqref i32.const 3 global.get 10 i32.const 4 i32.const 73 i32.const 110 i32.const 116 i32.const 66 i32.const 111 i32.const 120 array.new_fixed 0 6 i32.const 2 i32.const 4 struct.new 11)
  (global (;12;) eqref i32.const 4 ref.null eq i32.const 6 i32.const 66 i32.const 111 i32.const 120 i32.const 60 i32.const 83 i32.const 116 i32.const 114 i32.const 105 i32.const 110 i32.const 103 i32.const 62 array.new_fixed 0 11 i32.const 2 i32.const 2 struct.new 11)
  (global (;13;) eqref i32.const 5 global.get 12 i32.const 8 i32.const 84 i32.const 97 i32.const 103 i32.const 103 i32.const 101 i32.const 100 i32.const 66 i32.const 111 i32.const 120 i32.const 60 i32.const 73 i32.const 110 i32.const 116 i32.const 51 i32.const 50 i32.const 44 i32.const 32 i32.const 83 i32.const 116 i32.const 114 i32.const 105 i32.const 110 i32.const 103 i32.const 62 array.new_fixed 0 24 i32.const 2 i32.const 3 struct.new 11)
  (global (;14;) eqref i32.const 6 global.get 12 i32.const 10 i32.const 84 i32.const 97 i32.const 103 i32.const 103 i32.const 101 i32.const 100 i32.const 66 i32.const 111 i32.const 120 i32.const 60 i32.const 83 i32.const 116 i32.const 114 i32.const 105 i32.const 110 i32.const 103 i32.const 44 i32.const 32 i32.const 83 i32.const 116 i32.const 114 i32.const 105 i32.const 110 i32.const 103 i32.const 62 array.new_fixed 0 25 i32.const 2 i32.const 3 struct.new 11)
  (export "main" (func 4))
  (elem (;0;) (i32.const 0) func 11)
  (elem (;1;) (i32.const 2) func 18)
  (elem (;2;) (i32.const 3) func 14)
  (elem (;3;) (i32.const 4) func 12)
  (elem (;4;) (i32.const 5) func 14)
  (elem (;5;) (i32.const 6) func 15)
  (elem (;6;) (i32.const 8) func 16)
  (elem (;7;) (i32.const 9) func 17)
  (func (;4;) (type 20) (result i32)
    (local i32 i32 i64 f64 externref eqref i32 i32 eqref externref i32 eqref eqref eqref i32 i64 f64 externref)
    i32.const 40
    i32.const 2
    local.set 7
    local.set 6
    struct.new_default 16
    local.set 5
    local.get 5
    ref.cast (ref 16)
    global.get 11
    struct.set 16 0
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
    struct.new_default 18
    local.set 8
    local.get 8
    ref.cast (ref 18)
    global.get 13
    struct.set 18 0
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
    local.tee 13
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 6
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 6
        i32.const 0
        i32.const 53
        call 3
        global.set 6
      end
      global.get 6
      struct.new 5
      throw 0
    end
    local.get 13
    ref.cast (ref 16)
    local.set 13
    local.get 13
    ref.cast (ref 16)
    local.get 14
    struct.set 16 1
    local.set 11
    local.get 11
    local.tee 13
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 6
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 6
        i32.const 0
        i32.const 53
        call 3
        global.set 6
      end
      global.get 6
      struct.new 5
      throw 0
    end
    local.get 13
    ref.cast (ref 14)
    local.get 11
    ref.cast (ref 12)
    struct.get 12 0
    ref.cast (ref 11)
    struct.get 11 2
    i32.const 0
    i32.add
    call_indirect (type 27)
    call 0
    local.get 0
    return
  )
  (func (;5;) (type 21) (param eqref i32 i32)
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
    local.tee 7
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 6
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 6
        i32.const 0
        i32.const 53
        call 3
        global.set 6
      end
      global.get 6
      struct.new 5
      throw 0
    end
    local.get 7
    ref.cast (ref 16)
    local.set 7
    local.get 7
    ref.cast (ref 16)
    local.get 8
    struct.set 16 3
    return
  )
  (func (;6;) (type 22) (param eqref) (result i32)
    (local eqref eqref i32 eqref eqref i32 i64 f64 externref)
    local.get 0
    local.set 1
    local.get 1
    local.tee 5
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 6
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 6
        i32.const 0
        i32.const 53
        call 3
        global.set 6
      end
      global.get 6
      struct.new 5
      throw 0
    end
    local.get 5
    ref.cast (ref 14)
    local.get 1
    ref.cast (ref 12)
    struct.get 12 0
    ref.cast (ref 11)
    struct.get 11 2
    i32.const 0
    i32.add
    call_indirect (type 27)
    call 0
    local.get 0
    i32.const 7
    local.set 3
    local.set 2
    local.get 2
    local.tee 5
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 6
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 6
        i32.const 0
        i32.const 53
        call 3
        global.set 6
      end
      global.get 6
      struct.new 5
      throw 0
    end
    local.get 5
    ref.cast (ref 14)
    local.get 3
    local.get 2
    ref.cast (ref 12)
    struct.get 12 0
    ref.cast (ref 11)
    struct.get 11 2
    i32.const 1
    i32.add
    call_indirect (type 30)
    call 0
    local.get 0
    global.get 11
    call 19
    ref.null eq
    drop
    ref.is_null
    i32.eqz
    call 1
    local.get 0
    global.get 10
    call 19
    ref.null eq
    drop
    ref.is_null
    i32.eqz
    call 1
    local.get 0
    global.get 10
    call 19
    local.tee 5
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 6
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 6
        i32.const 0
        i32.const 53
        call 3
        global.set 6
      end
      global.get 6
      struct.new 5
      throw 0
    end
    local.get 5
    ref.cast (ref 15)
    struct.get 15 2
    call 2
    local.get 0
    local.set 1
    local.get 1
    local.tee 5
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 6
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 6
        i32.const 0
        i32.const 53
        call 3
        global.set 6
      end
      global.get 6
      struct.new 5
      throw 0
    end
    local.get 5
    ref.cast (ref 14)
    local.get 1
    ref.cast (ref 12)
    struct.get 12 0
    ref.cast (ref 11)
    struct.get 11 2
    i32.const 0
    i32.add
    call_indirect (type 27)
    return
  )
  (func (;7;) (type 23) (param eqref externref i32)
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
    local.tee 6
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 6
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 6
        i32.const 0
        i32.const 53
        call 3
        global.set 6
      end
      global.get 6
      struct.new 5
      throw 0
    end
    local.get 6
    ref.cast (ref 18)
    local.set 6
    local.get 6
    ref.cast (ref 18)
    local.get 7
    struct.set 18 2
    return
  )
  (func (;8;) (type 24) (param eqref)
    (local eqref eqref externref eqref eqref i32 i64 f64 externref)
    local.get 0
    local.set 1
    local.get 1
    local.tee 5
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 6
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 6
        i32.const 0
        i32.const 53
        call 3
        global.set 6
      end
      global.get 6
      struct.new 5
      throw 0
    end
    local.get 5
    ref.cast (ref 17)
    local.get 1
    ref.cast (ref 12)
    struct.get 12 0
    ref.cast (ref 11)
    struct.get 11 2
    i32.const 0
    i32.add
    call_indirect (type 32)
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
    local.tee 5
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 6
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 6
        i32.const 0
        i32.const 53
        call 3
        global.set 6
      end
      global.get 6
      struct.new 5
      throw 0
    end
    local.get 5
    ref.cast (ref 17)
    local.get 3
    local.get 2
    ref.cast (ref 12)
    struct.get 12 0
    ref.cast (ref 11)
    struct.get 11 2
    i32.const 1
    i32.add
    call_indirect (type 34)
    call 2
    local.get 0
    global.get 13
    call 19
    ref.null eq
    drop
    ref.is_null
    i32.eqz
    call 1
    local.get 0
    global.get 14
    call 19
    ref.null eq
    drop
    ref.is_null
    call 1
    return
  )
  (func (;9;) (type 25) (param eqref i32 externref)
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
    local.tee 6
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 6
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 6
        i32.const 0
        i32.const 53
        call 3
        global.set 6
      end
      global.get 6
      struct.new 5
      throw 0
    end
    local.get 6
    ref.cast (ref 15)
    local.set 6
    local.get 6
    ref.cast (ref 15)
    local.get 10
    struct.set 15 2
    return
  )
  (func (;10;) (type 26) (param eqref externref)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    drop
    local.get 0
    local.get 1
    local.set 7
    local.tee 3
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 6
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 6
        i32.const 0
        i32.const 53
        call 3
        global.set 6
      end
      global.get 6
      struct.new 5
      throw 0
    end
    local.get 3
    ref.cast (ref 17)
    local.set 3
    local.get 3
    ref.cast (ref 17)
    local.get 7
    struct.set 17 1
    return
  )
  (func (;11;) (type 27) (param eqref) (result i32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.tee 2
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 6
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 6
        i32.const 0
        i32.const 53
        call 3
        global.set 6
      end
      global.get 6
      struct.new 5
      throw 0
    end
    local.get 2
    ref.cast (ref 14)
    struct.get 14 1
    return
  )
  (func (;12;) (type 28) (param eqref) (result i32)
    (local eqref eqref eqref i32 i64 f64 externref)
    local.get 0
    local.set 1
    local.get 1
    call 18
    local.get 0
    local.tee 3
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 6
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 6
        i32.const 0
        i32.const 53
        call 3
        global.set 6
      end
      global.get 6
      struct.new 5
      throw 0
    end
    local.get 3
    ref.cast (ref 16)
    struct.get 16 3
    i32.add
    return
  )
  (func (;13;) (type 29) (param eqref i32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    drop
    local.get 0
    local.get 1
    local.set 4
    local.tee 3
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 6
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 6
        i32.const 0
        i32.const 53
        call 3
        global.set 6
      end
      global.get 6
      struct.new 5
      throw 0
    end
    local.get 3
    ref.cast (ref 14)
    local.set 3
    local.get 3
    ref.cast (ref 14)
    local.get 4
    struct.set 14 1
    return
  )
  (func (;14;) (type 31) (param eqref i32) (result i32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 1
    return
  )
  (func (;15;) (type 32) (param eqref) (result externref)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.tee 2
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 6
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 6
        i32.const 0
        i32.const 53
        call 3
        global.set 6
      end
      global.get 6
      struct.new 5
      throw 0
    end
    local.get 2
    ref.cast (ref 17)
    struct.get 17 1
    return
  )
  (func (;16;) (type 33) (param eqref) (result externref)
    (local eqref eqref eqref i32 i64 f64 externref)
    local.get 0
    local.set 1
    local.get 1
    call 15
    return
  )
  (func (;17;) (type 35) (param eqref externref) (result externref)
    (local eqref eqref i32 i64 f64 externref)
    local.get 1
    return
  )
  (func (;18;) (type 36) (param eqref) (result i32)
    (local eqref eqref eqref i32 i64 f64 externref)
    local.get 0
    local.set 1
    local.get 1
    call 11
    return
  )
  (func (;19;) (type 13) (param eqref eqref) (result eqref)
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
  (data (;0;) "H\00e\00l\00l\00o\00")
  (data (;1;) "n\00u\00m\00b\00e\00r\00")
  (data (;2;) "W\00o\00r\00l\00d\00")
  (data (;3;) "E\00x\00c\00e\00p\00t\00i\00o\00n\00 \00o\00f\00 \00t\00y\00p\00e\00 \00'\00S\00y\00s\00t\00e\00m\00.\00E\00x\00c\00e\00p\00t\00i\00o\00n\00'\00 \00w\00a\00s\00 \00t\00h\00r\00o\00w\00n\00.\00")
  (data (;4;) "S\00y\00s\00t\00e\00m\00 \00e\00r\00r\00o\00r\00.\00")
  (data (;5;) "S\00p\00e\00c\00i\00f\00i\00e\00d\00 \00c\00a\00s\00t\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00.\00")
  (data (;6;) "O\00b\00j\00e\00c\00t\00 \00r\00e\00f\00e\00r\00e\00n\00c\00e\00 \00n\00o\00t\00 \00s\00e\00t\00 \00t\00o\00 \00a\00n\00 \00i\00n\00s\00t\00a\00n\00c\00e\00 \00o\00f\00 \00a\00n\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;7;) "O\00p\00e\00r\00a\00t\00i\00o\00n\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00 \00d\00u\00e\00 \00t\00o\00 \00t\00h\00e\00 \00c\00u\00r\00r\00e\00n\00t\00 \00s\00t\00a\00t\00e\00 \00o\00f\00 \00t\00h\00e\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;8;) "V\00a\00l\00u\00e\00 \00d\00o\00e\00s\00 \00n\00o\00t\00 \00f\00a\00l\00l\00 \00w\00i\00t\00h\00i\00n\00 \00t\00h\00e\00 \00e\00x\00p\00e\00c\00t\00e\00d\00 \00r\00a\00n\00g\00e\00.\00")
)
```
