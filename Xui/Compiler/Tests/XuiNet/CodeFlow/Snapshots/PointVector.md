# Point + Vector typed stacks

Source: [PointVector.cs](Fixtures/Structs/PointVector.cs)

```csharp
using nfloat = System.Runtime.InteropServices.NFloat;

public struct Point
{
    public nfloat X;
    public nfloat Y;
    public Point(nfloat x, nfloat y) { X = x; Y = y; }
    public static Point operator +(Point p, Vector v) => new(p.X + v.X, p.Y + v.Y);
}
public struct Vector
{
    public nfloat X;
    public nfloat Y;
    public Vector(nfloat x, nfloat y) { X = x; Y = y; }
}
public static class Program
{
    public static int Main()
    {
        var p = new Point(1, 2);
        var v = new Vector(3, 4);
        var result = p + v;
        System.Console.WriteLine((double)result.X);
        System.Console.WriteLine((double)result.Y);
        return 0;
    }
}
```

## Release

### Point..ctor

- Parameters: `System.Runtime.InteropServices.NFloat, System.Runtime.InteropServices.NFloat`
- Result: `Void`
- Receiver: `Point&`

| Offset | Instruction | Before | After | Lowest |
| --- | --- | --- | --- | --- |
| IL_0000 | `ldarg.0` | S0 | S1 | S0 |
| IL_0001 | `ldarg.1` | S1 | S2 | S1 |
| IL_0002 | `stfld` | S2 | S0 | S0 |
| IL_0007 | `ldarg.0` | S0 | S1 | S0 |
| IL_0008 | `ldarg.2` | S1 | S2 | S1 |
| IL_0009 | `stfld` | S2 | S0 | S0 |
| IL_000e | `ret` | S0 | S0 | S0 |

| State | Stack (bottom to top) |
| --- | --- |
| S0 | empty |
| S1 | Point& |
| S2 | Point&, System.Runtime.InteropServices.NFloat |

### Point.op_Addition

- Parameters: `Point, Vector`
- Result: `Point`

| Offset | Instruction | Before | After | Lowest |
| --- | --- | --- | --- | --- |
| IL_0000 | `ldarg.0` | S0 | S1 | S0 |
| IL_0001 | `ldfld` | S1 | S2 | S0 |
| IL_0006 | `ldarg.1` | S2 | S3 | S2 |
| IL_0007 | `ldfld` | S3 | S4 | S2 |
| IL_000c | `call` | S4 | S2 | S0 |
| IL_0011 | `ldarg.0` | S2 | S5 | S2 |
| IL_0012 | `ldfld` | S5 | S4 | S2 |
| IL_0017 | `ldarg.1` | S4 | S6 | S4 |
| IL_0018 | `ldfld` | S6 | S7 | S4 |
| IL_001d | `call` | S7 | S4 | S2 |
| IL_0022 | `newobj` | S4 | S1 | S0 |
| IL_0027 | `ret` | S1 | S0 | S0 |

| State | Stack (bottom to top) |
| --- | --- |
| S0 | empty |
| S1 | Point |
| S2 | System.Runtime.InteropServices.NFloat |
| S3 | System.Runtime.InteropServices.NFloat, Vector |
| S4 | System.Runtime.InteropServices.NFloat, System.Runtime.InteropServices.NFloat |
| S5 | System.Runtime.InteropServices.NFloat, Point |
| S6 | System.Runtime.InteropServices.NFloat, System.Runtime.InteropServices.NFloat, Vector |
| S7 | System.Runtime.InteropServices.NFloat, System.Runtime.InteropServices.NFloat, System.Runtime.InteropServices.NFloat |

### Vector..ctor

- Parameters: `System.Runtime.InteropServices.NFloat, System.Runtime.InteropServices.NFloat`
- Result: `Void`
- Receiver: `Vector&`

| Offset | Instruction | Before | After | Lowest |
| --- | --- | --- | --- | --- |
| IL_0000 | `ldarg.0` | S0 | S1 | S0 |
| IL_0001 | `ldarg.1` | S1 | S2 | S1 |
| IL_0002 | `stfld` | S2 | S0 | S0 |
| IL_0007 | `ldarg.0` | S0 | S1 | S0 |
| IL_0008 | `ldarg.2` | S1 | S2 | S1 |
| IL_0009 | `stfld` | S2 | S0 | S0 |
| IL_000e | `ret` | S0 | S0 | S0 |

| State | Stack (bottom to top) |
| --- | --- |
| S0 | empty |
| S1 | Vector& |
| S2 | Vector&, System.Runtime.InteropServices.NFloat |

### Program.Main

- Parameters: ``
- Result: `Int32`

| Offset | Instruction | Before | After | Lowest |
| --- | --- | --- | --- | --- |
| IL_0000 | `ldc.i4.1` | S0 | S1 | S0 |
| IL_0001 | `call` | S1 | S2 | S0 |
| IL_0006 | `ldc.i4.2` | S2 | S3 | S2 |
| IL_0007 | `call` | S3 | S4 | S2 |
| IL_000c | `newobj` | S4 | S5 | S0 |
| IL_0011 | `ldloca.s` | S5 | S6 | S5 |
| IL_0013 | `ldc.i4.3` | S6 | S7 | S6 |
| IL_0014 | `call` | S7 | S8 | S6 |
| IL_0019 | `ldc.i4.4` | S8 | S9 | S8 |
| IL_001a | `call` | S9 | S10 | S8 |
| IL_001f | `call` | S10 | S5 | S5 |
| IL_0024 | `ldloc.0` | S5 | S11 | S5 |
| IL_0025 | `call` | S11 | S5 | S0 |
| IL_002a | `dup` | S5 | S12 | S0 |
| IL_002b | `ldfld` | S12 | S13 | S5 |
| IL_0030 | `call` | S13 | S14 | S5 |
| IL_0035 | `conv.r8` | S14 | S14 | S5 |
| IL_0036 | `call` | S14 | S5 | S5 |
| IL_003b | `ldfld` | S5 | S2 | S0 |
| IL_0040 | `call` | S2 | S15 | S0 |
| IL_0045 | `conv.r8` | S15 | S15 | S0 |
| IL_0046 | `call` | S15 | S0 | S0 |
| IL_004b | `ldc.i4.0` | S0 | S1 | S0 |
| IL_004c | `ret` | S1 | S0 | S0 |

| State | Stack (bottom to top) |
| --- | --- |
| S0 | empty |
| S1 | Int32 |
| S2 | System.Runtime.InteropServices.NFloat |
| S3 | System.Runtime.InteropServices.NFloat, Int32 |
| S4 | System.Runtime.InteropServices.NFloat, System.Runtime.InteropServices.NFloat |
| S5 | Point |
| S6 | Point, Vector& |
| S7 | Point, Vector&, Int32 |
| S8 | Point, Vector&, System.Runtime.InteropServices.NFloat |
| S9 | Point, Vector&, System.Runtime.InteropServices.NFloat, Int32 |
| S10 | Point, Vector&, System.Runtime.InteropServices.NFloat, System.Runtime.InteropServices.NFloat |
| S11 | Point, Vector |
| S12 | Point, Point |
| S13 | Point, System.Runtime.InteropServices.NFloat |
| S14 | Point, Float |
| S15 | Float |

## Debug

### Point..ctor

- Parameters: `System.Runtime.InteropServices.NFloat, System.Runtime.InteropServices.NFloat`
- Result: `Void`
- Receiver: `Point&`

| Offset | Instruction | Before | After | Lowest |
| --- | --- | --- | --- | --- |
| IL_0000 | `nop` | S0 | S0 | S0 |
| IL_0001 | `ldarg.0` | S0 | S1 | S0 |
| IL_0002 | `ldarg.1` | S1 | S2 | S1 |
| IL_0003 | `stfld` | S2 | S0 | S0 |
| IL_0008 | `ldarg.0` | S0 | S1 | S0 |
| IL_0009 | `ldarg.2` | S1 | S2 | S1 |
| IL_000a | `stfld` | S2 | S0 | S0 |
| IL_000f | `ret` | S0 | S0 | S0 |

| State | Stack (bottom to top) |
| --- | --- |
| S0 | empty |
| S1 | Point& |
| S2 | Point&, System.Runtime.InteropServices.NFloat |

### Point.op_Addition

- Parameters: `Point, Vector`
- Result: `Point`

| Offset | Instruction | Before | After | Lowest |
| --- | --- | --- | --- | --- |
| IL_0000 | `ldarg.0` | S0 | S1 | S0 |
| IL_0001 | `ldfld` | S1 | S2 | S0 |
| IL_0006 | `ldarg.1` | S2 | S3 | S2 |
| IL_0007 | `ldfld` | S3 | S4 | S2 |
| IL_000c | `call` | S4 | S2 | S0 |
| IL_0011 | `ldarg.0` | S2 | S5 | S2 |
| IL_0012 | `ldfld` | S5 | S4 | S2 |
| IL_0017 | `ldarg.1` | S4 | S6 | S4 |
| IL_0018 | `ldfld` | S6 | S7 | S4 |
| IL_001d | `call` | S7 | S4 | S2 |
| IL_0022 | `newobj` | S4 | S1 | S0 |
| IL_0027 | `ret` | S1 | S0 | S0 |

| State | Stack (bottom to top) |
| --- | --- |
| S0 | empty |
| S1 | Point |
| S2 | System.Runtime.InteropServices.NFloat |
| S3 | System.Runtime.InteropServices.NFloat, Vector |
| S4 | System.Runtime.InteropServices.NFloat, System.Runtime.InteropServices.NFloat |
| S5 | System.Runtime.InteropServices.NFloat, Point |
| S6 | System.Runtime.InteropServices.NFloat, System.Runtime.InteropServices.NFloat, Vector |
| S7 | System.Runtime.InteropServices.NFloat, System.Runtime.InteropServices.NFloat, System.Runtime.InteropServices.NFloat |

### Vector..ctor

- Parameters: `System.Runtime.InteropServices.NFloat, System.Runtime.InteropServices.NFloat`
- Result: `Void`
- Receiver: `Vector&`

| Offset | Instruction | Before | After | Lowest |
| --- | --- | --- | --- | --- |
| IL_0000 | `nop` | S0 | S0 | S0 |
| IL_0001 | `ldarg.0` | S0 | S1 | S0 |
| IL_0002 | `ldarg.1` | S1 | S2 | S1 |
| IL_0003 | `stfld` | S2 | S0 | S0 |
| IL_0008 | `ldarg.0` | S0 | S1 | S0 |
| IL_0009 | `ldarg.2` | S1 | S2 | S1 |
| IL_000a | `stfld` | S2 | S0 | S0 |
| IL_000f | `ret` | S0 | S0 | S0 |

| State | Stack (bottom to top) |
| --- | --- |
| S0 | empty |
| S1 | Vector& |
| S2 | Vector&, System.Runtime.InteropServices.NFloat |

### Program.Main

- Parameters: ``
- Result: `Int32`

| Offset | Instruction | Before | After | Lowest |
| --- | --- | --- | --- | --- |
| IL_0000 | `nop` | S0 | S0 | S0 |
| IL_0001 | `ldloca.s` | S0 | S1 | S0 |
| IL_0003 | `ldc.i4.1` | S1 | S2 | S1 |
| IL_0004 | `call` | S2 | S3 | S1 |
| IL_0009 | `ldc.i4.2` | S3 | S4 | S3 |
| IL_000a | `call` | S4 | S5 | S3 |
| IL_000f | `call` | S5 | S0 | S0 |
| IL_0014 | `ldloca.s` | S0 | S6 | S0 |
| IL_0016 | `ldc.i4.3` | S6 | S7 | S6 |
| IL_0017 | `call` | S7 | S8 | S6 |
| IL_001c | `ldc.i4.4` | S8 | S9 | S8 |
| IL_001d | `call` | S9 | S10 | S8 |
| IL_0022 | `call` | S10 | S0 | S0 |
| IL_0027 | `ldloc.0` | S0 | S11 | S0 |
| IL_0028 | `ldloc.1` | S11 | S12 | S11 |
| IL_0029 | `call` | S12 | S11 | S0 |
| IL_002e | `stloc.2` | S11 | S0 | S0 |
| IL_002f | `ldloc.2` | S0 | S11 | S0 |
| IL_0030 | `ldfld` | S11 | S13 | S0 |
| IL_0035 | `call` | S13 | S14 | S0 |
| IL_003a | `conv.r8` | S14 | S14 | S0 |
| IL_003b | `call` | S14 | S0 | S0 |
| IL_0040 | `nop` | S0 | S0 | S0 |
| IL_0041 | `ldloc.2` | S0 | S11 | S0 |
| IL_0042 | `ldfld` | S11 | S13 | S0 |
| IL_0047 | `call` | S13 | S14 | S0 |
| IL_004c | `conv.r8` | S14 | S14 | S0 |
| IL_004d | `call` | S14 | S0 | S0 |
| IL_0052 | `nop` | S0 | S0 | S0 |
| IL_0053 | `ldc.i4.0` | S0 | S15 | S0 |
| IL_0054 | `stloc.3` | S15 | S0 | S0 |
| IL_0055 | `br.s` | S0 | S0 | S0 |
| IL_0057 | `ldloc.3` | S0 | S15 | S0 |
| IL_0058 | `ret` | S15 | S0 | S0 |

| State | Stack (bottom to top) |
| --- | --- |
| S0 | empty |
| S1 | Point& |
| S2 | Point&, Int32 |
| S3 | Point&, System.Runtime.InteropServices.NFloat |
| S4 | Point&, System.Runtime.InteropServices.NFloat, Int32 |
| S5 | Point&, System.Runtime.InteropServices.NFloat, System.Runtime.InteropServices.NFloat |
| S6 | Vector& |
| S7 | Vector&, Int32 |
| S8 | Vector&, System.Runtime.InteropServices.NFloat |
| S9 | Vector&, System.Runtime.InteropServices.NFloat, Int32 |
| S10 | Vector&, System.Runtime.InteropServices.NFloat, System.Runtime.InteropServices.NFloat |
| S11 | Point |
| S12 | Point, Vector |
| S13 | System.Runtime.InteropServices.NFloat |
| S14 | Float |
| S15 | Int32 |
