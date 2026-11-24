# Specialized generic classes

Source: [Boxes.cs](Fixtures/Generics/Boxes.cs)

## Source

```csharp
using System;

public class Box<T>
{
    public T Value;
    public Box(T value) { Value = value; }
    public T Read() => Value;
    public void Write(T value) { Value = value; }
    public T Exchange(T value)
    {
        T previous = Value;
        Value = value;
        return previous;
    }
    public static T Echo(T value) => value;
}

public static class Program
{
    public static int Main()
    {
        var number = new Box<int>(41);
        number.Write(number.Read() + 1);
        var text = new Box<string>("Hello");
        Console.WriteLine(number.Read());
        Console.WriteLine(text.Read());
        Console.WriteLine(number.Exchange(7));
        Console.WriteLine(number.Value);
        Console.WriteLine(text.Exchange("World"));
        Console.WriteLine(text.Read());
        return Box<int>.Echo(0);
    }
}
```

## Output

```text
42
Hello
42
7
Hello
World
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
  (type (;13;) (sub 11 (struct (field (mut eqref)) (field (mut i32)))))
  (type (;14;) (sub 11 (struct (field (mut eqref)) (field (mut externref)))))
  (type (;15;) (func (result i32)))
  (type (;16;) (func (param eqref i32)))
  (type (;17;) (func (param eqref) (result i32)))
  (type (;18;) (func (param eqref i32)))
  (type (;19;) (func (param eqref externref)))
  (type (;20;) (func (param eqref) (result externref)))
  (type (;21;) (func (param eqref i32) (result i32)))
  (type (;22;) (func (param eqref externref) (result externref)))
  (type (;23;) (func (param i32) (result i32)))
  (import "xui" "console-log" (func (;0;) (type 2) (param i32)))
  (import "xui" "console-log" (func (;1;) (type 3) (param externref)))
  (import "wasm:js-string" "fromCharCodeArray" (func (;2;) (type 1) (param (ref null 0) i32 i32) (result (ref extern))))
  (tag (;0;) (type 5) (param eqref))
  (global (;0;) (mut externref) ref.null extern)
  (global (;1;) (mut externref) ref.null extern)
  (global (;2;) (mut externref) ref.null extern)
  (global (;3;) (mut externref) ref.null extern)
  (global (;4;) (mut externref) ref.null extern)
  (global (;5;) (mut externref) ref.null extern)
  (global (;6;) (mut externref) ref.null extern)
  (global (;7;) (mut externref) ref.null extern)
  (global (;8;) eqref i32.const 1 ref.null eq i32.const 0 i32.const 66 i32.const 111 i32.const 120 i32.const 60 i32.const 73 i32.const 110 i32.const 116 i32.const 51 i32.const 50 i32.const 62 array.new_fixed 0 10 i32.const 0 i32.const 2 struct.new 10)
  (global (;9;) eqref i32.const 2 ref.null eq i32.const 0 i32.const 66 i32.const 111 i32.const 120 i32.const 60 i32.const 83 i32.const 116 i32.const 114 i32.const 105 i32.const 110 i32.const 103 i32.const 62 array.new_fixed 0 11 i32.const 0 i32.const 2 struct.new 10)
  (export "main" (func 3))
  (func (;3;) (type 15) (result i32)
    (local eqref i32 i64 f64 externref eqref i32 eqref eqref i32 eqref externref eqref eqref i32 eqref externref eqref eqref i32 i64 f64 externref)
    i32.const 41
    local.set 6
    struct.new_default 13
    local.set 5
    local.get 5
    ref.cast (ref 13)
    global.get 8
    struct.set 13 0
    local.get 5
    local.get 6
    call 4
    local.get 5
    local.set 0
    local.get 0
    local.get 0
    local.set 7
    local.get 7
    local.tee 18
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
    local.get 18
    ref.cast (ref 13)
    call 5
    i32.const 1
    i32.add
    local.set 9
    local.set 8
    local.get 8
    local.tee 18
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
    local.get 18
    ref.cast (ref 13)
    local.get 9
    call 6
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
    local.set 11
    struct.new_default 14
    local.set 10
    local.get 10
    ref.cast (ref 14)
    global.get 9
    struct.set 14 0
    local.get 10
    local.get 11
    call 7
    local.get 10
    local.get 0
    local.set 7
    local.get 7
    local.tee 18
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
    local.get 18
    ref.cast (ref 13)
    call 5
    call 0
    local.tee 17
    local.get 17
    local.set 12
    local.get 12
    local.tee 18
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
    local.get 18
    ref.cast (ref 14)
    call 8
    call 1
    local.get 0
    i32.const 7
    local.set 14
    local.set 13
    local.get 13
    local.tee 18
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
    local.get 18
    ref.cast (ref 13)
    local.get 14
    call 9
    call 0
    local.get 0
    local.tee 18
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
    local.get 18
    ref.cast (ref 13)
    struct.get 13 1
    call 0
    local.tee 17
    local.get 17
    global.get 1
    ref.is_null
    if ;; label = @1
      i32.const 0
      i32.const 5
      array.new_data 0 1
      i32.const 0
      i32.const 5
      call 2
      global.set 1
    end
    global.get 1
    local.set 16
    local.set 15
    local.get 15
    local.tee 18
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
    local.get 18
    ref.cast (ref 14)
    local.get 16
    call 10
    call 1
    local.set 12
    local.get 12
    local.tee 18
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
    local.get 18
    ref.cast (ref 14)
    call 8
    call 1
    i32.const 0
    call 11
    return
  )
  (func (;4;) (type 16) (param eqref i32)
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
    local.get 3
    ref.cast (ref 13)
    local.set 3
    local.get 3
    ref.cast (ref 13)
    local.get 4
    struct.set 13 1
    return
  )
  (func (;5;) (type 17) (param eqref) (result i32)
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
    ref.cast (ref 13)
    struct.get 13 1
    return
  )
  (func (;6;) (type 18) (param eqref i32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.get 1
    local.set 4
    local.tee 3
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
    local.get 3
    ref.cast (ref 13)
    local.set 3
    local.get 3
    ref.cast (ref 13)
    local.get 4
    struct.set 13 1
    return
  )
  (func (;7;) (type 19) (param eqref externref)
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
    local.get 3
    ref.cast (ref 14)
    local.set 3
    local.get 3
    ref.cast (ref 14)
    local.get 7
    struct.set 14 1
    return
  )
  (func (;8;) (type 20) (param eqref) (result externref)
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
    ref.cast (ref 14)
    struct.get 14 1
    return
  )
  (func (;9;) (type 21) (param eqref i32) (result i32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.tee 3
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
    local.get 3
    ref.cast (ref 13)
    struct.get 13 1
    local.get 0
    local.get 1
    local.set 4
    local.tee 3
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
    local.get 3
    ref.cast (ref 13)
    local.set 3
    local.get 3
    ref.cast (ref 13)
    local.get 4
    struct.set 13 1
    return
  )
  (func (;10;) (type 22) (param eqref externref) (result externref)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.tee 3
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
    local.get 3
    ref.cast (ref 14)
    struct.get 14 1
    local.get 0
    local.get 1
    local.set 7
    local.tee 3
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
    local.get 3
    ref.cast (ref 14)
    local.set 3
    local.get 3
    ref.cast (ref 14)
    local.get 7
    struct.set 14 1
    return
  )
  (func (;11;) (type 23) (param i32) (result i32)
    local.get 0
    return
  )
  (func (;12;) (type 12) (param eqref eqref) (result eqref)
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
  (data (;0;) "H\00e\00l\00l\00o\00")
  (data (;1;) "W\00o\00r\00l\00d\00")
  (data (;2;) "E\00x\00c\00e\00p\00t\00i\00o\00n\00 \00o\00f\00 \00t\00y\00p\00e\00 \00'\00S\00y\00s\00t\00e\00m\00.\00E\00x\00c\00e\00p\00t\00i\00o\00n\00'\00 \00w\00a\00s\00 \00t\00h\00r\00o\00w\00n\00.\00")
  (data (;3;) "S\00y\00s\00t\00e\00m\00 \00e\00r\00r\00o\00r\00.\00")
  (data (;4;) "S\00p\00e\00c\00i\00f\00i\00e\00d\00 \00c\00a\00s\00t\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00.\00")
  (data (;5;) "O\00b\00j\00e\00c\00t\00 \00r\00e\00f\00e\00r\00e\00n\00c\00e\00 \00n\00o\00t\00 \00s\00e\00t\00 \00t\00o\00 \00a\00n\00 \00i\00n\00s\00t\00a\00n\00c\00e\00 \00o\00f\00 \00a\00n\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;6;) "O\00p\00e\00r\00a\00t\00i\00o\00n\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00 \00d\00u\00e\00 \00t\00o\00 \00t\00h\00e\00 \00c\00u\00r\00r\00e\00n\00t\00 \00s\00t\00a\00t\00e\00 \00o\00f\00 \00t\00h\00e\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;7;) "V\00a\00l\00u\00e\00 \00d\00o\00e\00s\00 \00n\00o\00t\00 \00f\00a\00l\00l\00 \00w\00i\00t\00h\00i\00n\00 \00t\00h\00e\00 \00e\00x\00p\00e\00c\00t\00e\00d\00 \00r\00a\00n\00g\00e\00.\00")
)
```
