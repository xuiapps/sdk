# Catching failed interface casts

Source: [Casts.cs](Fixtures/Exceptions/Casts.cs)

## Source

```csharp
using System;

public interface IValue { int Read(); }
public class Value : IValue { public int Read() => 42; }
public class Other { }

public static class Program
{
    private static int Read(object value)
    {
        try
        {
            return ((IValue)value).Read();
        }
        catch (InvalidCastException error)
        {
            Console.WriteLine(error.Message);
            return -1;
        }
        finally
        {
            Console.WriteLine("finished");
        }
    }

    public static int Main()
    {
        Console.WriteLine(Read(new Other()));
        return Read(new Value());
    }
}
```

## Output

```text
Specified cast is not valid.
finished
-1
finished
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
  (type (;13;) (sub 11 (struct (field (mut eqref)))))
  (type (;14;) (sub 11 (struct (field (mut eqref)))))
  (type (;15;) (func (result i32)))
  (type (;16;) (func (param eqref)))
  (type (;17;) (func (param eqref) (result i32)))
  (type (;18;) (func (param eqref)))
  (type (;19;) (func (param eqref) (result i32)))
  (type (;20;) (func (param eqref) (result i32)))
  (import "xui" "console-log" (func (;0;) (type 2) (param i32)))
  (import "xui" "console-log" (func (;1;) (type 3) (param externref)))
  (import "wasm:js-string" "fromCharCodeArray" (func (;2;) (type 1) (param (ref null 0) i32 i32) (result (ref extern))))
  (table (;0;) 3 3 funcref)
  (tag (;0;) (type 5) (param eqref))
  (global (;0;) (mut externref) ref.null extern)
  (global (;1;) (mut externref) ref.null extern)
  (global (;2;) (mut externref) ref.null extern)
  (global (;3;) (mut externref) ref.null extern)
  (global (;4;) (mut externref) ref.null extern)
  (global (;5;) (mut externref) ref.null extern)
  (global (;6;) (mut externref) ref.null extern)
  (global (;7;) eqref i32.const 1 ref.null eq i32.const 0 i32.const 79 i32.const 116 i32.const 104 i32.const 101 i32.const 114 array.new_fixed 0 5 i32.const 0 i32.const 1 struct.new 10)
  (global (;8;) eqref i32.const 2 ref.null eq i32.const 1 i32.const 86 i32.const 97 i32.const 108 i32.const 117 i32.const 101 array.new_fixed 0 5 i32.const 1 i32.const 1 struct.new 10)
  (global (;9;) eqref i32.const 3 ref.null eq i32.const 0 i32.const 73 i32.const 86 i32.const 97 i32.const 108 i32.const 117 i32.const 101 array.new_fixed 0 6 i32.const 0 i32.const 0 struct.new 10)
  (export "main" (func 3))
  (elem (;0;) (i32.const 2) func 7)
  (elem (;1;) (i32.const 1) func 7)
  (func (;3;) (type 15) (result i32)
    (local eqref eqref eqref eqref i32 i64 f64 externref)
    struct.new_default 13
    local.set 0
    local.get 0
    ref.cast (ref 13)
    global.get 7
    struct.set 13 0
    local.get 0
    call 4
    local.get 0
    call 5
    call 0
    struct.new_default 14
    local.set 1
    local.get 1
    ref.cast (ref 14)
    global.get 8
    struct.set 14 0
    local.get 1
    call 6
    local.get 1
    call 5
    return
  )
  (func (;4;) (type 16) (param eqref)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    drop
    return
  )
  (func (;5;) (type 17) (param eqref) (result i32)
    (local i32 eqref eqref exnref exnref eqref eqref i32 i64 f64 externref)
    block (type 9) ;; label = @1
      block (type 8) (result exnref) ;; label = @2
        try_table (catch_all_ref 0 (;@2;)) ;; label = @3
          block (type 9) ;; label = @4
            block (type 7) (result eqref exnref) ;; label = @5
              try_table (catch_ref 0 0 (;@5;)) ;; label = @6
                local.get 0
                local.tee 6
                global.get 9
                call 8
                local.set 7
                local.get 6
                ref.is_null
                i32.eqz
                local.get 7
                ref.is_null
                i32.and
                if ;; label = @7
                  i32.const 3
                  global.get 3
                  ref.is_null
                  if ;; label = @8
                    i32.const 0
                    i32.const 28
                    array.new_data 0 3
                    i32.const 0
                    i32.const 28
                    call 2
                    global.set 3
                  end
                  global.get 3
                  struct.new 4
                  throw 0
                end
                local.get 7
                local.set 2
                local.get 2
                local.tee 7
                ref.is_null
                if ;; label = @7
                  i32.const 4
                  global.get 4
                  ref.is_null
                  if ;; label = @8
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
                local.get 7
                ref.cast (ref 11)
                local.get 2
                ref.cast (ref 11)
                struct.get 11 0
                ref.cast (ref 10)
                struct.get 10 2
                i32.const 0
                i32.add
                call_indirect (type 19)
                local.set 1
                br 2 (;@4;)
              end
              br 1 (;@4;)
            end
            local.set 4
            local.set 3
            local.get 3
            ref.cast (ref 4)
            struct.get 4 0
            i32.const 3
            i32.eq
            if ;; label = @5
              local.get 3
              local.tee 3
              local.get 4
              local.set 4
              local.tee 7
              ref.is_null
              if ;; label = @6
                i32.const 4
                global.get 4
                ref.is_null
                if ;; label = @7
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
              local.get 7
              ref.cast (ref 4)
              struct.get 4 1
              call 1
              i32.const -1
              local.set 1
              br 1 (;@4;)
              br 1 (;@4;)
            end
            local.get 4
            throw_ref
          end
          br 2 (;@1;)
        end
        br 1 (;@1;)
      end
      local.set 5
      block (type 9) ;; label = @2
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
        call 1
        br 0 (;@2;)
      end
      local.get 5
      throw_ref
    end
    block (type 9) ;; label = @1
      global.get 0
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 8
        array.new_data 0 0
        i32.const 0
        i32.const 8
        call 2
        global.set 0
      end
      global.get 0
      call 1
      br 0 (;@1;)
    end
    local.get 1
    return
  )
  (func (;6;) (type 18) (param eqref)
    (local eqref eqref i32 i64 f64 externref)
    local.get 0
    drop
    return
  )
  (func (;7;) (type 20) (param eqref) (result i32)
    (local eqref eqref i32 i64 f64 externref)
    i32.const 42
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
  (data (;0;) "f\00i\00n\00i\00s\00h\00e\00d\00")
  (data (;1;) "E\00x\00c\00e\00p\00t\00i\00o\00n\00 \00o\00f\00 \00t\00y\00p\00e\00 \00'\00S\00y\00s\00t\00e\00m\00.\00E\00x\00c\00e\00p\00t\00i\00o\00n\00'\00 \00w\00a\00s\00 \00t\00h\00r\00o\00w\00n\00.\00")
  (data (;2;) "S\00y\00s\00t\00e\00m\00 \00e\00r\00r\00o\00r\00.\00")
  (data (;3;) "S\00p\00e\00c\00i\00f\00i\00e\00d\00 \00c\00a\00s\00t\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00.\00")
  (data (;4;) "O\00b\00j\00e\00c\00t\00 \00r\00e\00f\00e\00r\00e\00n\00c\00e\00 \00n\00o\00t\00 \00s\00e\00t\00 \00t\00o\00 \00a\00n\00 \00i\00n\00s\00t\00a\00n\00c\00e\00 \00o\00f\00 \00a\00n\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;5;) "O\00p\00e\00r\00a\00t\00i\00o\00n\00 \00i\00s\00 \00n\00o\00t\00 \00v\00a\00l\00i\00d\00 \00d\00u\00e\00 \00t\00o\00 \00t\00h\00e\00 \00c\00u\00r\00r\00e\00n\00t\00 \00s\00t\00a\00t\00e\00 \00o\00f\00 \00t\00h\00e\00 \00o\00b\00j\00e\00c\00t\00.\00")
  (data (;6;) "V\00a\00l\00u\00e\00 \00d\00o\00e\00s\00 \00n\00o\00t\00 \00f\00a\00l\00l\00 \00w\00i\00t\00h\00i\00n\00 \00t\00h\00e\00 \00e\00x\00p\00e\00c\00t\00e\00d\00 \00r\00a\00n\00g\00e\00.\00")
)
```
