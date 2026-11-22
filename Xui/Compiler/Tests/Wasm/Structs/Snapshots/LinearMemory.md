# Linear-memory struct copies

Source: [LinearMemory.cs](Fixtures/Structs/LinearMemory.cs)

## Source

```csharp
using System;

public struct Point
{
    public double X;
    public double Y;
}

public struct LabelledPoint
{
    public byte Tag;
    public Point Position;
}

public static class Program
{
    public static int Main()
    {
        LabelledPoint original = default;
        original.Tag = 7;
        original.Position.X = 1;
        original.Position.Y = 2;
        var copy = original;
        original.Position.X = 10;
        copy.Position.Y = 20;
        Console.WriteLine(original.Position.X);
        Console.WriteLine(original.Position.Y);
        Console.WriteLine(copy.Position.X);
        Console.WriteLine(copy.Position.Y);
        return copy.Tag;
    }
}
```

## Output

```text
10
2
1
20
```

## Exit Code

```text
7
```

## WAT

```wat
(module
  (type (;0;) (array (mut i16)))
  (type (;1;) (func (param (ref null 0) i32 i32) (result (ref extern))))
  (type (;2;) (func (param f64)))
  (type (;3;) (func (result i32)))
  (import "xui" "console-log" (func (;0;) (type 2) (param f64)))
  (import "wasm:js-string" "fromCharCodeArray" (func (;1;) (type 1) (param (ref null 0) i32 i32) (result (ref extern))))
  (memory (;0;) 16 16)
  (global (;0;) (mut i32) i32.const 16)
  (export "main" (func 2))
  (func (;2;) (type 3) (result i32)
    (local i32 i32 i32 i32 i32 i64 f64)
    global.get 0
    local.tee 2
    i32.const 1048320
    i32.gt_u
    if ;; label = @1
      unreachable
    end
    local.get 2
    i32.const 256
    i32.add
    global.set 0
    local.get 2
    i32.const 0
    i32.add
    local.set 0
    local.get 0
    i32.const 0
    i32.const 24
    memory.fill
    local.get 2
    i32.const 24
    i32.add
    local.set 1
    local.get 1
    i32.const 0
    i32.const 24
    memory.fill
    local.get 0
    i32.const 0
    i32.const 24
    memory.fill
    local.get 0
    i32.const 7
    local.set 4
    i32.const 0
    i32.add
    local.get 4
    i32.store8
    local.get 0
    i32.const 8
    i32.add
    f64.const 0x1p+0 (;=1;)
    local.set 6
    i32.const 0
    i32.add
    local.get 6
    f64.store align=1
    local.get 0
    i32.const 8
    i32.add
    f64.const 0x1p+1 (;=2;)
    local.set 6
    i32.const 8
    i32.add
    local.get 6
    f64.store align=1
    local.get 0
    local.set 3
    local.get 2
    i32.const 48
    i32.add
    local.get 3
    i32.const 24
    memory.copy
    local.get 2
    i32.const 48
    i32.add
    local.set 3
    local.get 1
    local.get 3
    i32.const 24
    memory.copy
    local.get 0
    i32.const 8
    i32.add
    f64.const 0x1.4p+3 (;=10;)
    local.set 6
    i32.const 0
    i32.add
    local.get 6
    f64.store align=1
    local.get 1
    i32.const 8
    i32.add
    f64.const 0x1.4p+4 (;=20;)
    local.set 6
    i32.const 8
    i32.add
    local.get 6
    f64.store align=1
    local.get 0
    local.set 3
    local.get 2
    i32.const 72
    i32.add
    local.get 3
    i32.const 24
    memory.copy
    local.get 2
    i32.const 72
    i32.add
    i32.const 8
    i32.add
    local.set 3
    local.get 2
    i32.const 96
    i32.add
    local.get 3
    i32.const 16
    memory.copy
    local.get 2
    i32.const 96
    i32.add
    i32.const 0
    i32.add
    f64.load align=1
    call 0
    local.get 0
    local.set 3
    local.get 2
    i32.const 112
    i32.add
    local.get 3
    i32.const 24
    memory.copy
    local.get 2
    i32.const 112
    i32.add
    i32.const 8
    i32.add
    local.set 3
    local.get 2
    i32.const 136
    i32.add
    local.get 3
    i32.const 16
    memory.copy
    local.get 2
    i32.const 136
    i32.add
    i32.const 8
    i32.add
    f64.load align=1
    call 0
    local.get 1
    local.set 3
    local.get 2
    i32.const 152
    i32.add
    local.get 3
    i32.const 24
    memory.copy
    local.get 2
    i32.const 152
    i32.add
    i32.const 8
    i32.add
    local.set 3
    local.get 2
    i32.const 176
    i32.add
    local.get 3
    i32.const 16
    memory.copy
    local.get 2
    i32.const 176
    i32.add
    i32.const 0
    i32.add
    f64.load align=1
    call 0
    local.get 1
    local.set 3
    local.get 2
    i32.const 192
    i32.add
    local.get 3
    i32.const 24
    memory.copy
    local.get 2
    i32.const 192
    i32.add
    i32.const 8
    i32.add
    local.set 3
    local.get 2
    i32.const 216
    i32.add
    local.get 3
    i32.const 16
    memory.copy
    local.get 2
    i32.const 216
    i32.add
    i32.const 8
    i32.add
    f64.load align=1
    call 0
    local.get 1
    local.set 3
    local.get 2
    i32.const 232
    i32.add
    local.get 3
    i32.const 24
    memory.copy
    local.get 2
    i32.const 232
    i32.add
    i32.const 0
    i32.add
    i32.load8_u
    local.get 2
    global.set 0
    return
  )
)
```
