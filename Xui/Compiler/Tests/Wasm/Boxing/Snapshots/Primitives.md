# Primitive boxing and unboxing

Source: [Primitives.cs](Fixtures/Boxing/Primitives.cs)

## Source

```csharp
using System;

public static class Program
{
    static object Box<T>(T value) => value;
    static T Read<T>(object value) => (T)value;

    public static int Main()
    {
        int original = 42;
        object boxed = Box(original);
        object alias = boxed;
        original = 99;

        Console.WriteLine(boxed.GetType().Name);
        Console.WriteLine(boxed is int);
        Console.WriteLine(Read<int>(boxed));
        Console.WriteLine(boxed == alias);

        try { Read<uint>(boxed); }
        catch (InvalidCastException) { Console.WriteLine("Wrong type caught"); }

        return Read<int>(boxed);
    }
}
```

## Output

```text
Int32
True
42
True
Wrong type caught
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
  (type (;4;) (func (param i32)))
  (type (;5;) (struct (field i32) (field externref)))
  (type (;6;) (func (param eqref)))
  (type (;7;) (func (result eqref)))
  (type (;8;) (func (result eqref exnref)))
  (type (;9;) (func (result exnref)))
  (type (;10;) (func))
  (type (;11;) (struct (field i32) (field eqref) (field i32) (field (ref null 0)) (field i32) (field i32) (field (ref null 0))))
  (type (;12;) (sub (struct (field (mut eqref)))))
  (type (;13;) (func (param eqref eqref) (result eqref)))
  (type (;14;) (sub final 12 (struct (field (mut eqref)) (field (mut i32)))))
  (type (;15;) (sub final 12 (struct (field (mut eqref)) (field (mut i32)))))
  (type (;16;) (func (result i32)))
  (type (;17;) (func (param i32) (result eqref)))
  (type (;18;) (func (param eqref) (result i32)))
  (type (;19;) (func (param eqref) (result i32)))
  (import "xui" "console-log" (func (;0;) (type 2) (param externref)))
  (import "xui" "console-log-boolean" (func (;1;) (type 3) (param i32)))
  (import "xui" "console-log" (func (;2;) (type 4) (param i32)))
  (import "wasm:js-string" "fromCharCodeArray" (func (;3;) (type 1) (param (ref null 0) i32 i32) (result (ref extern))))
  (tag (;0;) (type 6) (param eqref))
  (global (;0;) (mut externref) ref.null extern)
  (global (;1;) (mut externref) ref.null extern)
  (global (;2;) (mut externref) ref.null extern)
  (global (;3;) (mut externref) ref.null extern)
  (global (;4;) (mut externref) ref.null extern)
  (global (;5;) (mut externref) ref.null extern)
  (global (;6;) (mut externref) ref.null extern)
  (global (;7;) eqref i32.const 1 ref.null eq i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 86 i32.const 97 i32.const 108 i32.const 117 i32.const 101 i32.const 84 i32.const 121 i32.const 112 i32.const 101 array.new_fixed 0 16 i32.const 0 i32.const 0 i32.const 86 i32.const 97 i32.const 108 i32.const 117 i32.const 101 i32.const 84 i32.const 121 i32.const 112 i32.const 101 array.new_fixed 0 9 struct.new 11)
  (global (;8;) eqref i32.const 2 global.get 7 i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 73 i32.const 110 i32.const 116 i32.const 51 i32.const 50 array.new_fixed 0 12 i32.const 0 i32.const 2 i32.const 73 i32.const 110 i32.const 116 i32.const 51 i32.const 50 array.new_fixed 0 5 struct.new 11)
  (global (;9;) eqref i32.const 3 global.get 7 i32.const 0 i32.const 83 i32.const 121 i32.const 115 i32.const 116 i32.const 101 i32.const 109 i32.const 46 i32.const 85 i32.const 73 i32.const 110 i32.const 116 i32.const 51 i32.const 50 array.new_fixed 0 13 i32.const 0 i32.const 2 i32.const 85 i32.const 73 i32.const 110 i32.const 116 i32.const 51 i32.const 50 array.new_fixed 0 6 struct.new 11)
  (export "main" (func 4))
  (func (;4;) (type 16) (result i32)
    (local eqref eqref eqref exnref eqref eqref i32 i64 f64 externref)
    i32.const 42
    call 5
    local.set 0
    local.get 0
    local.set 1
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
    ref.cast (ref 12)
    struct.get 12 0
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
    ref.cast (ref 11)
    struct.get 11 6
    local.tee 4
    ref.cast (ref 0)
    i32.const 0
    local.get 4
    ref.cast (ref 0)
    array.len
    call 3
    call 0
    local.get 0
    local.tee 5
    ref.is_null
    local.set 6
    local.get 5
    local.set 5
    local.get 5
    ref.test (ref 14)
    if (type 7) (result eqref) ;; label = @1
      local.get 5
      ref.cast (ref 12)
      struct.get 12 0
      local.set 4
      i32.const 0
      local.get 4
      global.get 8
      ref.eq
      i32.or
      if (type 7) (result eqref) ;; label = @2
        local.get 5
      else
        ref.null eq
      end
    else
      ref.null eq
    end
    ref.null eq
    drop
    ref.is_null
    i32.eqz
    call 1
    local.get 0
    call 6
    call 2
    local.get 0
    local.get 1
    ref.eq
    call 1
    block (type 10) ;; label = @1
      block (type 8) (result eqref exnref) ;; label = @2
        try_table (catch_ref 0 0 (;@2;)) ;; label = @3
          local.get 0
          call 7
          drop
          br 2 (;@1;)
        end
        br 1 (;@1;)
      end
      local.set 3
      local.set 2
      local.get 2
      ref.cast (ref 5)
      struct.get 5 0
      i32.const 3
      i32.eq
      if ;; label = @2
        local.get 2
        local.tee 2
        local.get 3
        local.set 3
        drop
        global.get 0
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 17
          array.new_data 0 0
          i32.const 0
          i32.const 17
          call 3
          global.set 0
        end
        global.get 0
        call 0
        br 1 (;@1;)
        br 1 (;@1;)
      end
      local.get 3
      throw_ref
    end
    local.get 0
    call 6
    return
  )
  (func (;5;) (type 17) (param i32) (result eqref)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.set 3
    global.get 8
    local.get 3
    struct.new 14
    return
  )
  (func (;6;) (type 18) (param eqref) (result i32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.tee 2
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
    local.get 2
    local.set 2
    local.get 2
    ref.test (ref 14)
    if (type 7) (result eqref) ;; label = @1
      local.get 2
      ref.cast (ref 12)
      struct.get 12 0
      local.set 1
      i32.const 0
      local.get 1
      global.get 8
      ref.eq
      i32.or
      if (type 7) (result eqref) ;; label = @2
        local.get 2
      else
        ref.null eq
      end
    else
      ref.null eq
    end
    local.tee 2
    ref.is_null
    if ;; label = @1
      i32.const 3
      global.get 3
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 28
        array.new_data 0 3
        i32.const 0
        i32.const 28
        call 3
        global.set 3
      end
      global.get 3
      struct.new 5
      throw 0
    end
    local.get 2
    ref.cast (ref 14)
    struct.get 14 1
    return
  )
  (func (;7;) (type 19) (param eqref) (result i32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.tee 2
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
    local.get 2
    local.set 2
    local.get 2
    ref.test (ref 15)
    if (type 7) (result eqref) ;; label = @1
      local.get 2
      ref.cast (ref 12)
      struct.get 12 0
      local.set 1
      i32.const 0
      local.get 1
      global.get 9
      ref.eq
      i32.or
      if (type 7) (result eqref) ;; label = @2
        local.get 2
      else
        ref.null eq
      end
    else
      ref.null eq
    end
    local.tee 2
    ref.is_null
    if ;; label = @1
      i32.const 3
      global.get 3
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 28
        array.new_data 0 3
        i32.const 0
        i32.const 28
        call 3
        global.set 3
      end
      global.get 3
      struct.new 5
      throw 0
    end
    local.get 2
    ref.cast (ref 15)
    struct.get 15 1
    return
  )
  (func (;8;) (type 13) (param eqref eqref) (result eqref)
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
  (data (;0;) "W\00r\00o\00n\00g\00 \00t\00y\00p\00e\00 \00c\00a\00u\00g\00h\00t\00")
  (data (;1;) "E\00x\00c\00e\00p\00t\00i\00o\00n\00 \00o\00f\00 \00t\00y\00p\00e\00 \00'\00S\00y\00s\00t\00e\00m\00.\00E\00x\00c\00e\00p\00t\00i\00o\00n\00'\00 \00w\00a\00s\00 \00t\00h\00r\00o\00w\00n\00.\00")
  (data (;2;) "S\00y\00s\00t\00e\00m\00 \00e\00r\00r\00o\00r\00.\00")
  (data (;3;) "S\00p\00e\00c\00i\00f\00i\00e\00d\00 \00c\00a\00s\00t\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00.\00")
  (data (;4;) "O\00b\00j\00e\00c\00t\00 \00r\00e\00f\00e\00r\00e\00n\00c\00e\00 \00n\00o\00t\00 \00s\00e\00t\00 \00t\00o\00 \00a\00n\00 \00i\00n\00s\00t\00a\00n\00c\00e\00 \00o\00f\00 \00a\00n\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;5;) "O\00p\00e\00r\00a\00t\00i\00o\00n\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00 \00d\00u\00e\00 \00t\00o\00 \00t\00h\00e\00 \00c\00u\00r\00r\00e\00n\00t\00 \00s\00t\00a\00t\00e\00 \00o\00f\00 \00t\00h\00e\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;6;) "V\00a\00l\00u\00e\00 \00d\00o\00e\00s\00 \00n\00o\00t\00 \00f\00a\00l\00l\00 \00w\00i\00t\00h\00i\00n\00 \00t\00h\00e\00 \00e\00x\00p\00e\00c\00t\00e\00d\00 \00r\00a\00n\00g\00e\00.\00")
)
```
