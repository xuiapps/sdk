# Nested WASM

Source: [Nested.cs](Fixtures/CodeFlow/Nested.cs)

## Source

```csharp
namespace Samples.CodeFlow;

public static class Nested
{
    public static int Run(int value)
    {
        int total = 0;
        while (value > 0)
        {
            if (value > 2)
                total += value;
            else if (value == 2)
                total += 20;
            else
                total += 10;
            value--;
        }
        return total;
    }
}

public static class EntryPoint
{
    public static int Main()
    {
        System.Console.WriteLine(Nested.Run(-3));
        System.Console.WriteLine(Nested.Run(0));
        System.Console.WriteLine(Nested.Run(1));
        System.Console.WriteLine(Nested.Run(2));
        System.Console.WriteLine(Nested.Run(5));
        System.Console.WriteLine(Nested.Run(10));
        return 0;
    }
}
```

## Output

```text
0
0
10
30
42
82
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
  (type (;7;) (func))
  (type (;8;) (func))
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
        local.get 0
        i32.const 0
        i32.gt_s
        if (type 6) ;; label = @3
          local.get 0
          i32.const 2
          i32.gt_s
          i32.eqz
          if (type 7) ;; label = @4
            local.get 0
            i32.const 2
            i32.eq
            i32.eqz
            if (type 8) ;; label = @5
              local.get 1
              i32.const 10
              i32.add
              local.set 1
            else
              local.get 1
              i32.const 20
              i32.add
              local.set 1
            end
          else
            local.get 1
            local.get 0
            i32.add
            local.set 1
          end
          local.get 0
          i32.const 1
          i32.sub
          local.set 0
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
