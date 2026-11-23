# ConditionalValue WASM

Source: [ConditionalValue.cs](Fixtures/CodeFlow/ConditionalValue.cs)

## Source

```csharp
namespace Samples.CodeFlow;

public static class ConditionalValue
{
    public static int Run(int value) => value > 0 ? 10 : 20;
}

public static class EntryPoint
{
    public static int Main()
    {
        System.Console.WriteLine(ConditionalValue.Run(-3));
        System.Console.WriteLine(ConditionalValue.Run(0));
        System.Console.WriteLine(ConditionalValue.Run(1));
        System.Console.WriteLine(ConditionalValue.Run(2));
        System.Console.WriteLine(ConditionalValue.Run(5));
        System.Console.WriteLine(ConditionalValue.Run(10));
        return 0;
    }
}
```

## Output

```text
20
20
10
10
10
10
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
    i32.gt_s
    if (type 5) ;; label = @1
      i32.const 10
      return
    else
      i32.const 20
      return
    end
    unreachable
  )
)
```
