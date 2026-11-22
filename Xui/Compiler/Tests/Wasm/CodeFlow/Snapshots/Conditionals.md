# Typed Conditionals WASM

## Source

```csharp
using System;

public static class Program
{
    private static int Add(int left, int right) => left + right;
    private static int Choose(int value) => Add(5, value > 0 ? 10 : 20);
    private static double Number(int value) => 0.5 + (value > 0 ? 1.25 : 2.5);
    private static string Message(int value) => value < 0 ? "negative" : value == 0 ? "zero" : "positive";

    private static void PrintMessage(int value) => Console.WriteLine(value > 0 ? "yes" : "no");

    public static int Main()
    {
        Console.WriteLine(Choose(1));
        Console.WriteLine(Choose(-1));
        Console.WriteLine(Number(1));
        Console.WriteLine(Number(-1));
        Console.WriteLine(Message(-1));
        Console.WriteLine(Message(0));
        Console.WriteLine(Message(1));
        PrintMessage(1);
        PrintMessage(-1);
        return Choose(1);
    }
}
```

## Output

```text
15
25
1.75
3
negative
zero
positive
yes
no
```

## Exit Code

```text
15
```

## WAT

```wat
(module
  (type (;0;) (array (mut i16)))
  (type (;1;) (func (param (ref null 0) i32 i32) (result (ref extern))))
  (type (;2;) (func (param i32)))
  (type (;3;) (func (param f64)))
  (type (;4;) (func (param externref)))
  (type (;5;) (func (result i32)))
  (type (;6;) (func (param i32) (result i32)))
  (type (;7;) (func (result i32)))
  (type (;8;) (func (param i32) (result f64)))
  (type (;9;) (func (result f64)))
  (type (;10;) (func (param i32) (result externref)))
  (type (;11;) (func))
  (type (;12;) (func))
  (type (;13;) (func (param i32)))
  (type (;14;) (func (result externref)))
  (type (;15;) (func (param i32 i32) (result i32)))
  (import "xui" "console-log" (func (;0;) (type 2) (param i32)))
  (import "xui" "console-log" (func (;1;) (type 3) (param f64)))
  (import "xui" "console-log" (func (;2;) (type 4) (param externref)))
  (import "wasm:js-string" "fromCharCodeArray" (func (;3;) (type 1) (param (ref null 0) i32 i32) (result (ref extern))))
  (global (;0;) (mut externref) ref.null extern)
  (global (;1;) (mut externref) ref.null extern)
  (global (;2;) (mut externref) ref.null extern)
  (global (;3;) (mut externref) ref.null extern)
  (global (;4;) (mut externref) ref.null extern)
  (export "main" (func 4))
  (func (;4;) (type 5) (result i32)
    i32.const 1
    call 5
    call 0
    i32.const -1
    call 5
    call 0
    i32.const 1
    call 6
    call 1
    i32.const -1
    call 6
    call 1
    i32.const -1
    call 7
    call 2
    i32.const 0
    call 7
    call 2
    i32.const 1
    call 7
    call 2
    i32.const 1
    call 8
    i32.const -1
    call 8
    i32.const 1
    call 5
    return
  )
  (func (;5;) (type 6) (param i32) (result i32)
    i32.const 5
    local.get 0
    i32.const 0
    i32.gt_s
    if (type 7) (result i32) ;; label = @1
      i32.const 10
    else
      i32.const 20
    end
    call 9
    return
  )
  (func (;6;) (type 8) (param i32) (result f64)
    f64.const 0x1p-1 (;=0.5;)
    local.get 0
    i32.const 0
    i32.gt_s
    if (type 9) (result f64) ;; label = @1
      f64.const 0x1.4p+0 (;=1.25;)
    else
      f64.const 0x1.4p+1 (;=2.5;)
    end
    f64.add
    return
  )
  (func (;7;) (type 10) (param i32) (result externref)
    local.get 0
    i32.const 0
    i32.lt_s
    if (type 11) ;; label = @1
      global.get 2
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 8
        array.new_data 0 2
        i32.const 0
        i32.const 8
        call 3
        global.set 2
      end
      global.get 2
      return
    else
      local.get 0
      i32.eqz
      if (type 12) ;; label = @2
        global.get 1
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 4
          array.new_data 0 1
          i32.const 0
          i32.const 4
          call 3
          global.set 1
        end
        global.get 1
        return
      else
        global.get 0
        ref.is_null
        if ;; label = @3
          i32.const 0
          i32.const 8
          array.new_data 0 0
          i32.const 0
          i32.const 8
          call 3
          global.set 0
        end
        global.get 0
        return
      end
      unreachable
    end
    unreachable
  )
  (func (;8;) (type 13) (param i32)
    local.get 0
    i32.const 0
    i32.gt_s
    if (type 14) (result externref) ;; label = @1
      global.get 4
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 3
        array.new_data 0 4
        i32.const 0
        i32.const 3
        call 3
        global.set 4
      end
      global.get 4
    else
      global.get 3
      ref.is_null
      if ;; label = @2
        i32.const 0
        i32.const 2
        array.new_data 0 3
        i32.const 0
        i32.const 2
        call 3
        global.set 3
      end
      global.get 3
    end
    call 2
    return
  )
  (func (;9;) (type 15) (param i32 i32) (result i32)
    local.get 0
    local.get 1
    i32.add
    return
  )
  (data (;0;) "p\00o\00s\00i\00t\00i\00v\00e\00")
  (data (;1;) "z\00e\00r\00o\00")
  (data (;2;) "n\00e\00g\00a\00t\00i\00v\00e\00")
  (data (;3;) "n\00o\00")
  (data (;4;) "y\00e\00s\00")
)
```
