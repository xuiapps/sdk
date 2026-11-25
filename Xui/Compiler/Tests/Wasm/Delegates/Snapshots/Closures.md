# Delegates and escaping closures

Source: [Closures.cs](Fixtures/Delegates/Closures.cs)

Each Create call owns a separate captured count. Its two callbacks share
that captured variable after Create returns. The delegate keeps the
compiler-generated closure object reachable through a GC reference.

## Source

```csharp
using System;

public delegate int Read();
public delegate void Add(int amount);

public class Counter
{
    public Read Read;
    public Add Add;
}

public static class Program
{
    private static Counter Create(int start)
    {
        int count = start;
        return new Counter
        {
            Read = () => count,
            Add = amount => count += amount
        };
    }

    public static int Main()
    {
        var first = Create(10);
        var second = Create(100);
        first.Add(2);
        first.Add(3);
        second.Add(7);
        Console.WriteLine(first.Read());
        Console.WriteLine(second.Read());
        return first.Read() + second.Read();
    }
}
```

## Output

```text
15
107
```

## Exit Code

```text
122
```

## WAT

```wat
(module
  (type (;0;) (array (mut i16)))
  (type (;1;) (func (param (ref null 0) i32 i32) (result (ref extern))))
  (type (;2;) (func (param i32)))
  (type (;3;) (struct (field i32) (field externref)))
  (type (;4;) (func (param eqref)))
  (type (;5;) (func (result eqref)))
  (type (;6;) (func (result eqref exnref)))
  (type (;7;) (func (result exnref)))
  (type (;8;) (func))
  (type (;9;) (struct (field i32) (field eqref) (field i32) (field (ref null 0)) (field i32) (field i32)))
  (type (;10;) (sub (struct (field (mut eqref)))))
  (type (;11;) (func (param eqref eqref) (result eqref)))
  (type (;12;) (sub final 10 (struct (field (mut eqref)) (field (mut eqref)) (field (mut i32)) (field (mut i32)) (field (mut eqref)))))
  (type (;13;) (array (mut eqref)))
  (type (;14;) (array i32))
  (type (;15;) (sub 10 (struct (field (mut eqref)) (field (mut eqref)) (field (mut eqref)))))
  (type (;16;) (sub 10 (struct (field (mut eqref)) (field (mut i32)))))
  (type (;17;) (func (param eqref) (result i32)))
  (type (;18;) (func (param eqref i32) (result eqref)))
  (type (;19;) (func (param eqref) (result i32)))
  (type (;20;) (func (param eqref i32)))
  (type (;21;) (func (result i32)))
  (type (;22;) (func (param i32) (result eqref)))
  (type (;23;) (func (param eqref)))
  (type (;24;) (func (param eqref)))
  (type (;25;) (func (param eqref) (result i32)))
  (type (;26;) (func (param eqref i32)))
  (type (;27;) (func (param eqref i32)))
  (type (;28;) (func (param eqref) (result i32)))
  (import "xui" "console-log" (func (;0;) (type 2) (param i32)))
  (import "wasm:js-string" "fromCharCodeArray" (func (;1;) (type 1) (param (ref null 0) i32 i32) (result (ref extern))))
  (table (;0;) 2 2 funcref)
  (tag (;0;) (type 4) (param eqref))
  (global (;0;) eqref i32.const 6 i32.const 7 array.new_fixed 14 2)
  (global (;1;) (mut externref) ref.null extern)
  (global (;2;) (mut externref) ref.null extern)
  (global (;3;) (mut externref) ref.null extern)
  (global (;4;) (mut externref) ref.null extern)
  (global (;5;) (mut externref) ref.null extern)
  (global (;6;) (mut externref) ref.null extern)
  (global (;7;) eqref i32.const 1 ref.null eq i32.const 0 i32.const 67 i32.const 111 i32.const 117 i32.const 110 i32.const 116 i32.const 101 i32.const 114 array.new_fixed 0 7 i32.const 0 i32.const 3 struct.new 9)
  (global (;8;) eqref i32.const 2 ref.null eq i32.const 0 i32.const 82 i32.const 101 i32.const 97 i32.const 100 array.new_fixed 0 4 i32.const 0 i32.const 5 struct.new 9)
  (global (;9;) eqref i32.const 3 ref.null eq i32.const 0 i32.const 65 i32.const 100 i32.const 100 array.new_fixed 0 3 i32.const 0 i32.const 5 struct.new 9)
  (global (;10;) eqref i32.const 4 ref.null eq i32.const 0 i32.const 60 i32.const 62 i32.const 99 i32.const 95 i32.const 95 i32.const 68 i32.const 105 i32.const 115 i32.const 112 i32.const 108 i32.const 97 i32.const 121 i32.const 67 i32.const 108 i32.const 97 i32.const 115 i32.const 115 i32.const 48 i32.const 95 i32.const 48 array.new_fixed 0 20 i32.const 0 i32.const 2 struct.new 9)
  (export "main" (func 2))
  (elem (;0;) (i32.const 0) func 11)
  (elem (;1;) (i32.const 1) func 12)
  (func (;2;) (type 21) (result i32)
    (local eqref i32 i64 f64 externref eqref i32 eqref eqref eqref i32 i64 f64 externref)
    i32.const 10
    call 3
    i32.const 100
    call 3
    local.set 0
    local.tee 8
    local.get 8
    local.tee 9
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 4
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 3
        i32.const 0
        i32.const 53
        call 1
        global.set 4
      end
      global.get 4
      struct.new 3
      throw 0
    end
    local.get 9
    ref.cast (ref 15)
    struct.get 15 2
    i32.const 2
    local.set 6
    local.set 5
    local.get 5
    local.tee 9
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 4
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 3
        i32.const 0
        i32.const 53
        call 1
        global.set 4
      end
      global.get 4
      struct.new 3
      throw 0
    end
    local.get 9
    ref.cast (ref 12)
    local.get 6
    call 8
    local.tee 8
    local.get 8
    local.tee 9
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 4
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 3
        i32.const 0
        i32.const 53
        call 1
        global.set 4
      end
      global.get 4
      struct.new 3
      throw 0
    end
    local.get 9
    ref.cast (ref 15)
    struct.get 15 2
    i32.const 3
    local.set 6
    local.set 5
    local.get 5
    local.tee 9
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 4
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 3
        i32.const 0
        i32.const 53
        call 1
        global.set 4
      end
      global.get 4
      struct.new 3
      throw 0
    end
    local.get 9
    ref.cast (ref 12)
    local.get 6
    call 8
    local.get 0
    local.tee 9
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 4
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 3
        i32.const 0
        i32.const 53
        call 1
        global.set 4
      end
      global.get 4
      struct.new 3
      throw 0
    end
    local.get 9
    ref.cast (ref 15)
    struct.get 15 2
    i32.const 7
    local.set 6
    local.set 5
    local.get 5
    local.tee 9
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 4
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 3
        i32.const 0
        i32.const 53
        call 1
        global.set 4
      end
      global.get 4
      struct.new 3
      throw 0
    end
    local.get 9
    ref.cast (ref 12)
    local.get 6
    call 8
    local.tee 8
    local.get 8
    local.tee 9
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 4
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 3
        i32.const 0
        i32.const 53
        call 1
        global.set 4
      end
      global.get 4
      struct.new 3
      throw 0
    end
    local.get 9
    ref.cast (ref 15)
    struct.get 15 1
    local.set 7
    local.get 7
    local.tee 9
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 4
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 3
        i32.const 0
        i32.const 53
        call 1
        global.set 4
      end
      global.get 4
      struct.new 3
      throw 0
    end
    local.get 9
    ref.cast (ref 12)
    call 9
    call 0
    local.get 0
    local.tee 9
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 4
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 3
        i32.const 0
        i32.const 53
        call 1
        global.set 4
      end
      global.get 4
      struct.new 3
      throw 0
    end
    local.get 9
    ref.cast (ref 15)
    struct.get 15 1
    local.set 7
    local.get 7
    local.tee 9
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 4
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 3
        i32.const 0
        i32.const 53
        call 1
        global.set 4
      end
      global.get 4
      struct.new 3
      throw 0
    end
    local.get 9
    ref.cast (ref 12)
    call 9
    call 0
    local.tee 9
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 4
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 3
        i32.const 0
        i32.const 53
        call 1
        global.set 4
      end
      global.get 4
      struct.new 3
      throw 0
    end
    local.get 9
    ref.cast (ref 15)
    struct.get 15 1
    local.set 7
    local.get 7
    local.tee 9
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 4
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 3
        i32.const 0
        i32.const 53
        call 1
        global.set 4
      end
      global.get 4
      struct.new 3
      throw 0
    end
    local.get 9
    ref.cast (ref 12)
    call 9
    local.get 0
    local.tee 9
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 4
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 3
        i32.const 0
        i32.const 53
        call 1
        global.set 4
      end
      global.get 4
      struct.new 3
      throw 0
    end
    local.get 9
    ref.cast (ref 15)
    struct.get 15 1
    local.set 7
    local.get 7
    local.tee 9
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 4
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 3
        i32.const 0
        i32.const 53
        call 1
        global.set 4
      end
      global.get 4
      struct.new 3
      throw 0
    end
    local.get 9
    ref.cast (ref 12)
    call 9
    i32.add
    return
  )
  (func (;3;) (type 22) (param i32) (result eqref)
    (local eqref i32 i64 f64 externref eqref eqref eqref eqref i32 eqref eqref i32 i64 f64 externref)
    struct.new_default 16
    local.set 6
    local.get 6
    ref.cast (ref 16)
    global.get 10
    struct.set 16 0
    local.get 6
    call 4
    local.get 6
    local.set 1
    local.get 1
    local.get 0
    local.set 13
    local.tee 12
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 4
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 3
        i32.const 0
        i32.const 53
        call 1
        global.set 4
      end
      global.get 4
      struct.new 3
      throw 0
    end
    local.get 12
    ref.cast (ref 16)
    local.set 12
    local.get 12
    ref.cast (ref 16)
    local.get 13
    struct.set 16 1
    struct.new_default 15
    local.set 7
    local.get 7
    ref.cast (ref 15)
    global.get 7
    struct.set 15 0
    local.get 7
    call 5
    local.get 7
    local.tee 11
    local.get 11
    local.get 1
    i32.const 0
    local.set 13
    local.tee 11
    ref.is_null
    if ;; label = @1
      i32.const 6
      global.get 6
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 46
        array.new_data 0 5
        i32.const 0
        i32.const 46
        call 1
        global.set 6
      end
      global.get 6
      struct.new 3
      throw 0
    end
    global.get 8
    local.get 11
    local.get 13
    global.get 0
    ref.cast (ref 14)
    local.get 13
    array.get 14
    ref.null eq
    struct.new 12
    local.set 11
    local.tee 12
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 4
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 3
        i32.const 0
        i32.const 53
        call 1
        global.set 4
      end
      global.get 4
      struct.new 3
      throw 0
    end
    local.get 12
    ref.cast (ref 15)
    local.set 12
    local.get 12
    ref.cast (ref 15)
    local.get 11
    struct.set 15 1
    local.tee 11
    local.get 11
    local.get 1
    i32.const 1
    local.set 13
    local.tee 11
    ref.is_null
    if ;; label = @1
      i32.const 6
      global.get 6
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 46
        array.new_data 0 5
        i32.const 0
        i32.const 46
        call 1
        global.set 6
      end
      global.get 6
      struct.new 3
      throw 0
    end
    global.get 9
    local.get 11
    local.get 13
    global.get 0
    ref.cast (ref 14)
    local.get 13
    array.get 14
    ref.null eq
    struct.new 12
    local.set 11
    local.tee 12
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 4
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 3
        i32.const 0
        i32.const 53
        call 1
        global.set 4
      end
      global.get 4
      struct.new 3
      throw 0
    end
    local.get 12
    ref.cast (ref 15)
    local.set 12
    local.get 12
    ref.cast (ref 15)
    local.get 11
    struct.set 15 2
    return
  )
  (func (;4;) (type 23) (param eqref)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    drop
    return
  )
  (func (;5;) (type 24) (param eqref)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    drop
    return
  )
  (func (;6;) (type 25) (param eqref) (result i32)
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
        array.new_data 0 3
        i32.const 0
        i32.const 53
        call 1
        global.set 4
      end
      global.get 4
      struct.new 3
      throw 0
    end
    local.get 2
    ref.cast (ref 16)
    struct.get 16 1
    return
  )
  (func (;7;) (type 26) (param eqref i32)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    local.get 0
    local.tee 3
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 4
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 3
        i32.const 0
        i32.const 53
        call 1
        global.set 4
      end
      global.get 4
      struct.new 3
      throw 0
    end
    local.get 3
    ref.cast (ref 16)
    struct.get 16 1
    local.get 1
    i32.add
    local.set 4
    local.tee 3
    ref.is_null
    if ;; label = @1
      i32.const 4
      global.get 4
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 53
        array.new_data 0 3
        i32.const 0
        i32.const 53
        call 1
        global.set 4
      end
      global.get 4
      struct.new 3
      throw 0
    end
    local.get 3
    ref.cast (ref 16)
    local.set 3
    local.get 3
    ref.cast (ref 16)
    local.get 4
    struct.set 16 1
    return
  )
  (func (;8;) (type 27) (param eqref i32)
    (local eqref i32 i32)
    local.get 0
    call 13
    local.set 4
    loop ;; label = @1
      local.get 0
      local.get 3
      call 14
      local.set 2
      local.get 2
      ref.cast (ref 12)
      struct.get 12 1
      local.get 1
      local.get 2
      ref.cast (ref 12)
      struct.get 12 2
      call_indirect (type 27)
      local.get 3
      i32.const 1
      i32.add
      local.tee 3
      local.get 4
      i32.lt_u
      br_if 0 (;@1;)
    end
  )
  (func (;9;) (type 28) (param eqref) (result i32)
    (local eqref i32 i32 i32)
    local.get 0
    call 13
    local.set 3
    loop ;; label = @1
      local.get 0
      local.get 2
      call 14
      local.set 1
      local.get 1
      ref.cast (ref 12)
      struct.get 12 1
      local.get 1
      ref.cast (ref 12)
      struct.get 12 2
      call_indirect (type 28)
      local.set 4
      local.get 2
      i32.const 1
      i32.add
      local.tee 2
      local.get 3
      i32.lt_u
      br_if 0 (;@1;)
    end
    local.get 4
  )
  (func (;10;) (type 11) (param eqref eqref) (result eqref)
    (local eqref)
    local.get 0
    ref.is_null
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 0
    ref.test (ref 10)
    i32.eqz
    if ;; label = @1
      ref.null eq
      return
    end
    local.get 0
    ref.cast (ref 10)
    struct.get 10 0
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
      ref.cast (ref 9)
      struct.get 9 1
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
  (func (;11;) (type 19) (param eqref) (result i32)
    local.get 0
    call 6
  )
  (func (;12;) (type 20) (param eqref i32)
    local.get 0
    local.get 1
    call 7
  )
  (func (;13;) (type 17) (param eqref) (result i32)
    local.get 0
    ref.cast (ref 12)
    struct.get 12 4
    ref.is_null
    if ;; label = @1
      i32.const 1
      return
    end
    local.get 0
    ref.cast (ref 12)
    struct.get 12 4
    ref.cast (ref 13)
    array.len
  )
  (func (;14;) (type 18) (param eqref i32) (result eqref)
    local.get 0
    ref.cast (ref 12)
    struct.get 12 4
    ref.is_null
    if ;; label = @1
      local.get 0
      return
    end
    local.get 0
    ref.cast (ref 12)
    struct.get 12 4
    ref.cast (ref 13)
    local.get 1
    array.get 13
  )
  (data (;0;) "E\00x\00c\00e\00p\00t\00i\00o\00n\00 \00o\00f\00 \00t\00y\00p\00e\00 \00'\00S\00y\00s\00t\00e\00m\00.\00E\00x\00c\00e\00p\00t\00i\00o\00n\00'\00 \00w\00a\00s\00 \00t\00h\00r\00o\00w\00n\00.\00")
  (data (;1;) "S\00y\00s\00t\00e\00m\00 \00e\00r\00r\00o\00r\00.\00")
  (data (;2;) "S\00p\00e\00c\00i\00f\00i\00e\00d\00 \00c\00a\00s\00t\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00.\00")
  (data (;3;) "O\00b\00j\00e\00c\00t\00 \00r\00e\00f\00e\00r\00e\00n\00c\00e\00 \00n\00o\00t\00 \00s\00e\00t\00 \00t\00o\00 \00a\00n\00 \00i\00n\00s\00t\00a\00n\00c\00e\00 \00o\00f\00 \00a\00n\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;4;) "O\00p\00e\00r\00a\00t\00i\00o\00n\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00 \00d\00u\00e\00 \00t\00o\00 \00t\00h\00e\00 \00c\00u\00r\00r\00e\00n\00t\00 \00s\00t\00a\00t\00e\00 \00o\00f\00 \00t\00h\00e\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;5;) "V\00a\00l\00u\00e\00 \00d\00o\00e\00s\00 \00n\00o\00t\00 \00f\00a\00l\00l\00 \00w\00i\00t\00h\00i\00n\00 \00t\00h\00e\00 \00e\00x\00p\00e\00c\00t\00e\00d\00 \00r\00a\00n\00g\00e\00.\00")
)
```
