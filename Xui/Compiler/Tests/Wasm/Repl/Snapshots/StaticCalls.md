# Static Calls WASM

## Source

```csharp
using System;

public static class Program
{
    static int Add(int a, int b) => a + b;

    public static int Main()
    {
        Console.WriteLine(Add(1, 1));
        return 0;
    }
}
```

## Output

```text
2
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
  (type (;4;) (func (param i32 i32) (result i32)))
  (import "xui" "console-log" (func (;0;) (type 2) (param i32)))
  (import "wasm:js-string" "fromCharCodeArray" (func (;1;) (type 1) (param (ref null 0) i32 i32) (result (ref extern))))
  (export "main" (func 2))
  (func (;2;) (type 3) (result i32)
    i32.const 1
    i32.const 1
    call 3
    call 0
    i32.const 0
    return
  )
  (func (;3;) (type 4) (param i32 i32) (result i32)
    local.get 0
    local.get 1
    i32.add
    return
  )
)
```
