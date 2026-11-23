# EarlyReturn WASM

Source: [EarlyReturn.cs](Fixtures/CodeFlow/EarlyReturn.cs)

## Source

```csharp
namespace Samples.CodeFlow;

public static class EarlyReturn
{
    public static int Run(int value)
    {
        if (value < 0)
            return -value;
        if (value == 0)
            return 10;
        return value + 1;
    }
}

public static class EntryPoint
{
    public static int Main()
    {
        System.Console.WriteLine(EarlyReturn.Run(-3));
        System.Console.WriteLine(EarlyReturn.Run(0));
        System.Console.WriteLine(EarlyReturn.Run(1));
        System.Console.WriteLine(EarlyReturn.Run(2));
        System.Console.WriteLine(EarlyReturn.Run(5));
        System.Console.WriteLine(EarlyReturn.Run(10));
        return 0;
    }
}
```

## Output

```text
3
10
2
3
6
11
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
  (type (;3;) (func (result i32)))
  (type (;4;) (func (param i32) (result i32)))
  (type (;5;) (func))
  (type (;6;) (func))
  (import "xui" "console-log" (func (;0;) (type 2) (param i32)))
  (import "wasm:js-string" "fromCharCodeArray" (func (;1;) (type 1) (param (ref null 0) i32 i32) (result (ref extern))))
  (export "main" (func 2))
  (func (;2;) (type 3) (result i32)
    i32.const -3
    call 3
    call 0
    i32.const 0
    call 3
    call 0
    i32.const 1
    call 3
    call 0
    i32.const 2
    call 3
    call 0
    i32.const 5
    call 3
    call 0
    i32.const 10
    call 3
    call 0
    i32.const 0
    return
  )
  (func (;3;) (type 4) (param i32) (result i32)
    local.get 0
    i32.const 0
    i32.lt_s
    i32.eqz
    if (type 5) ;; label = @1
      local.get 0
      if (type 6) ;; label = @2
        local.get 0
        i32.const 1
        i32.add
        return
      else
        i32.const 10
        return
      end
      unreachable
    else
      local.get 0
      i32.const -1
      i32.mul
      return
    end
    unreachable
  )
)
```
