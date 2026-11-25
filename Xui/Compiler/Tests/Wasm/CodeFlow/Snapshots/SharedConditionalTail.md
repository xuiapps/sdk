# SharedConditionalTail WASM

Source: [SharedConditionalTail.cs](Fixtures/CodeFlow/SharedConditionalTail.cs)

## Source

```csharp
namespace Samples.CodeFlow;

public static class SharedConditionalTail
{
    private static int checks;

    private static bool Before(int now, int showAt)
    {
        checks++;
        return now < showAt;
    }

    public static int Run(int value)
    {
        checks = 0;
        bool tapped = (value & 1) != 0;
        int now = value, showAt = 2, fadeAt = 0;
        bool active = true;
        if (!tapped && Before(now, showAt)) active = false;
        else
        {
            showAt = now < showAt ? now : showAt;
            fadeAt = now + (tapped ? 20 : 0);
        }
        if (tapped) fadeAt += 3;
        return (active ? 1000 : 0) + showAt + fadeAt + checks * 100;
    }
}

public static class EntryPoint
{
    public static int Main()
    {
        System.Console.WriteLine(SharedConditionalTail.Run(-3));
        System.Console.WriteLine(SharedConditionalTail.Run(0));
        System.Console.WriteLine(SharedConditionalTail.Run(1));
        System.Console.WriteLine(SharedConditionalTail.Run(2));
        System.Console.WriteLine(SharedConditionalTail.Run(5));
        System.Console.WriteLine(SharedConditionalTail.Run(10));
        return 0;
    }
}
```

## Output

```text
1017
102
1025
1104
1030
1112
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
  (type (;3;) (struct (field (mut i32))))
  (type (;4;) (func (result i32)))
  (type (;5;) (func (param i32) (result i32)))
  (type (;6;) (func))
  (type (;7;) (func (result i32)))
  (type (;8;) (func (result i32)))
  (type (;9;) (func))
  (type (;10;) (func (result i32)))
  (type (;11;) (func (result i32)))
  (type (;12;) (func))
  (type (;13;) (func (result i32)))
  (type (;14;) (func (param i32 i32) (result i32)))
  (import "xui" "console-log" (func (;0;) (type 2) (param i32)))
  (import "wasm:js-string" "fromCharCodeArray" (func (;1;) (type 1) (param (ref null 0) i32 i32) (result (ref extern))))
  (global (;0;) eqref struct.new_default 3)
  (export "main" (func 2))
  (func (;2;) (type 4) (result i32)
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
  (func (;3;) (type 5) (param i32) (result i32)
    (local i32 i32 i32 i32 i32 eqref eqref i32 i64 f64 externref)
    i32.const 0
    local.set 8
    global.get 0
    local.set 7
    local.get 7
    ref.cast (ref 3)
    local.get 8
    struct.set 3 0
    local.get 0
    i32.const 1
    i32.and
    i32.const 0
    i32.gt_u
    i32.const 255
    i32.and
    local.set 1
    local.get 0
    local.set 2
    i32.const 2
    local.set 3
    i32.const 0
    local.set 4
    i32.const 1
    i32.const 255
    i32.and
    local.set 5
    local.get 1
    if (type 6) ;; label = @1
      local.get 2
      local.get 3
      i32.lt_s
      if (type 7) (result i32) ;; label = @2
        local.get 2
      else
        local.get 3
      end
      local.set 3
      local.get 2
      local.get 1
      if (type 8) (result i32) ;; label = @2
        i32.const 20
      else
        i32.const 0
      end
      i32.add
      local.set 4
    else
      local.get 2
      local.get 3
      call 4
      i32.eqz
      if (type 9) ;; label = @2
        local.get 2
        local.get 3
        i32.lt_s
        if (type 10) (result i32) ;; label = @3
          local.get 2
        else
          local.get 3
        end
        local.set 3
        local.get 2
        local.get 1
        if (type 11) (result i32) ;; label = @3
          i32.const 20
        else
          i32.const 0
        end
        i32.add
        local.set 4
      else
        i32.const 0
        i32.const 255
        i32.and
        local.set 5
      end
    end
    local.get 1
    i32.eqz
    if (type 12) ;; label = @1
    else
      local.get 4
      i32.const 3
      i32.add
      local.set 4
    end
    local.get 5
    if (type 13) (result i32) ;; label = @1
      i32.const 1000
    else
      i32.const 0
    end
    local.get 3
    i32.add
    local.get 4
    i32.add
    global.get 0
    ref.cast (ref 3)
    struct.get 3 0
    i32.const 100
    i32.mul
    i32.add
    return
  )
  (func (;4;) (type 14) (param i32 i32) (result i32)
    (local eqref eqref i32 i64 f64 externref)
    global.get 0
    ref.cast (ref 3)
    struct.get 3 0
    i32.const 1
    i32.add
    local.set 4
    global.get 0
    local.set 3
    local.get 3
    ref.cast (ref 3)
    local.get 4
    struct.set 3 0
    local.get 0
    local.get 1
    i32.lt_s
    i32.const 255
    i32.and
    return
  )
)
```
