# DoWhile WASM

Source: [DoWhile.cs](Fixtures/CodeFlow/DoWhile.cs)

## Source

```csharp
namespace Samples.CodeFlow;

public static class DoWhile
{
    public static int Run(int value)
    {
        int total = 0;
        do
        {
            total += value;
            value--;
        }
        while (value > 0);
        return total;
    }
}

public static class EntryPoint
{
    public static int Main()
    {
        System.Console.WriteLine(DoWhile.Run(-3));
        System.Console.WriteLine(DoWhile.Run(0));
        System.Console.WriteLine(DoWhile.Run(1));
        System.Console.WriteLine(DoWhile.Run(2));
        System.Console.WriteLine(DoWhile.Run(5));
        System.Console.WriteLine(DoWhile.Run(10));
        return 0;
    }
}
```

## Output

```text
-3
0
1
3
15
55
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
    (local i32)
    i32.const 0
    local.set 1
    block (type 5) ;; label = @1
      loop (type 5) ;; label = @2
        local.get 1
        local.get 0
        i32.add
        local.set 1
        local.get 0
        i32.const 1
        i32.sub
        local.set 0
        local.get 0
        i32.const 0
        i32.gt_s
        if (type 6) ;; label = @3
          br 1 (;@2;)
        else
          br 2 (;@1;)
        end
        unreachable
      end
    end
    local.get 1
    return
  )
)
```
