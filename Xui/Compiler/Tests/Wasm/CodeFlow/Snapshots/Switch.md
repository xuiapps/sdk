# Switch WASM

Source: [Switch.cs](Fixtures/CodeFlow/Switch.cs)

## Source

```csharp
namespace Samples.CodeFlow;

public static class Switch
{
    public static int Run(int value)
    {
        int result;
        switch (value)
        {
            case 0: result = 10; break;
            case 1:
            case 2: result = 20; break;
            case 3: result = 30; break;
            case 4: result = 40; break;
            default: result = -1; break;
        }
        return result + 1;
    }
}

public static class EntryPoint
{
    public static int Main()
    {
        System.Console.WriteLine(Switch.Run(-3));
        System.Console.WriteLine(Switch.Run(0));
        System.Console.WriteLine(Switch.Run(1));
        System.Console.WriteLine(Switch.Run(2));
        System.Console.WriteLine(Switch.Run(5));
        System.Console.WriteLine(Switch.Run(10));
        return 0;
    }
}
```

## Output

```text
0
11
21
21
0
0
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
    (local i32 i32)
    local.get 0
    local.set 2
    block (type 5) ;; label = @1
      block (type 6) ;; label = @2
        block (type 6) ;; label = @3
          block (type 6) ;; label = @4
            block (type 6) ;; label = @5
              block (type 6) ;; label = @6
                local.get 2
                br_table 1 (;@5;) 2 (;@4;) 2 (;@4;) 3 (;@3;) 4 (;@2;) 0 (;@6;)
              end
              i32.const -1
              local.set 1
              br 4 (;@1;)
            end
            i32.const 10
            local.set 1
            br 3 (;@1;)
          end
          i32.const 20
          local.set 1
          br 2 (;@1;)
        end
        i32.const 30
        local.set 1
        br 1 (;@1;)
      end
      i32.const 40
      local.set 1
      br 0 (;@1;)
    end
    local.get 1
    i32.const 1
    i32.add
    return
  )
)
```
