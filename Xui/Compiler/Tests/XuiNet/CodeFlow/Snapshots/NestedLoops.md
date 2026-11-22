# NestedLoops instruction graph

Source: [NestedLoops.cs](Fixtures/CodeFlow/NestedLoops.cs)

```csharp
namespace Samples.CodeFlow;

public static class NestedLoops
{
    public static int Run(int value)
    {
        int total = 0;
        while (value > 0)
        {
            int inner = 2;
            do
            {
                total += value;
                inner--;
            }
            while (inner > 0);
            value--;
        }
        return total;
    }
}
```

## Release

### Instructions

| Offset | Instruction | Successors | Before | After | Lowest |
| --- | --- | --- | --- | --- | --- |
| IL_0000 | `ldc.i4.0` | IL_0001 | S0 | S1 | S0 |
| IL_0001 | `stloc.0` | IL_0002 | S1 | S0 | S0 |
| IL_0002 | `br.s` | IL_0017 | S0 | S0 | S0 |
| IL_0004 | `ldc.i4.2` | IL_0005 | S0 | S1 | S0 |
| IL_0005 | `stloc.1` | IL_0006 | S1 | S0 | S0 |
| IL_0006 | `ldloc.0` | IL_0007 | S0 | S1 | S0 |
| IL_0007 | `ldarg.0` | IL_0008 | S1 | S2 | S1 |
| IL_0008 | `add` | IL_0009 | S2 | S1 | S0 |
| IL_0009 | `stloc.0` | IL_000a | S1 | S0 | S0 |
| IL_000a | `ldloc.1` | IL_000b | S0 | S1 | S0 |
| IL_000b | `ldc.i4.1` | IL_000c | S1 | S2 | S1 |
| IL_000c | `sub` | IL_000d | S2 | S1 | S0 |
| IL_000d | `stloc.1` | IL_000e | S1 | S0 | S0 |
| IL_000e | `ldloc.1` | IL_000f | S0 | S1 | S0 |
| IL_000f | `ldc.i4.0` | IL_0010 | S1 | S2 | S1 |
| IL_0010 | `bgt.s` | IL_0012, IL_0006 | S2 | S0 | S0 |
| IL_0012 | `ldarg.0` | IL_0013 | S0 | S1 | S0 |
| IL_0013 | `ldc.i4.1` | IL_0014 | S1 | S2 | S1 |
| IL_0014 | `sub` | IL_0015 | S2 | S1 | S0 |
| IL_0015 | `starg.s` | IL_0017 | S1 | S0 | S0 |
| IL_0017 | `ldarg.0` | IL_0018 | S0 | S1 | S0 |
| IL_0018 | `ldc.i4.0` | IL_0019 | S1 | S2 | S1 |
| IL_0019 | `bgt.s` | IL_001b, IL_0004 | S2 | S0 | S0 |
| IL_001b | `ldloc.0` | IL_001c | S0 | S1 | S0 |
| IL_001c | `ret` |  | S1 | S0 | S0 |

### Stack tree

| State | Parent | Top type | Depth | Children |
| --- | --- | --- | --- | --- |
| S0 | — | empty | 0 | S1 |
| S1 | S0 | Int32 | 1 | S2 |
| S2 | S1 | Int32 | 2 |  |

## Debug

### Instructions

| Offset | Instruction | Successors | Before | After | Lowest |
| --- | --- | --- | --- | --- | --- |
| IL_0000 | `nop` | IL_0001 | S0 | S0 | S0 |
| IL_0001 | `ldc.i4.0` | IL_0002 | S0 | S1 | S0 |
| IL_0002 | `stloc.0` | IL_0003 | S1 | S0 | S0 |
| IL_0003 | `br.s` | IL_0020 | S0 | S0 | S0 |
| IL_0005 | `nop` | IL_0006 | S0 | S0 | S0 |
| IL_0006 | `ldc.i4.2` | IL_0007 | S0 | S1 | S0 |
| IL_0007 | `stloc.1` | IL_0008 | S1 | S0 | S0 |
| IL_0008 | `nop` | IL_0009 | S0 | S0 | S0 |
| IL_0009 | `ldloc.0` | IL_000a | S0 | S1 | S0 |
| IL_000a | `ldarg.0` | IL_000b | S1 | S2 | S1 |
| IL_000b | `add` | IL_000c | S2 | S1 | S0 |
| IL_000c | `stloc.0` | IL_000d | S1 | S0 | S0 |
| IL_000d | `ldloc.1` | IL_000e | S0 | S1 | S0 |
| IL_000e | `ldc.i4.1` | IL_000f | S1 | S2 | S1 |
| IL_000f | `sub` | IL_0010 | S2 | S1 | S0 |
| IL_0010 | `stloc.1` | IL_0011 | S1 | S0 | S0 |
| IL_0011 | `nop` | IL_0012 | S0 | S0 | S0 |
| IL_0012 | `ldloc.1` | IL_0013 | S0 | S1 | S0 |
| IL_0013 | `ldc.i4.0` | IL_0014 | S1 | S2 | S1 |
| IL_0014 | `cgt` | IL_0016 | S2 | S1 | S0 |
| IL_0016 | `stloc.2` | IL_0017 | S1 | S0 | S0 |
| IL_0017 | `ldloc.2` | IL_0018 | S0 | S1 | S0 |
| IL_0018 | `brtrue.s` | IL_001a, IL_0008 | S1 | S0 | S0 |
| IL_001a | `ldarg.0` | IL_001b | S0 | S1 | S0 |
| IL_001b | `ldc.i4.1` | IL_001c | S1 | S2 | S1 |
| IL_001c | `sub` | IL_001d | S2 | S1 | S0 |
| IL_001d | `starg.s` | IL_001f | S1 | S0 | S0 |
| IL_001f | `nop` | IL_0020 | S0 | S0 | S0 |
| IL_0020 | `ldarg.0` | IL_0021 | S0 | S1 | S0 |
| IL_0021 | `ldc.i4.0` | IL_0022 | S1 | S2 | S1 |
| IL_0022 | `cgt` | IL_0024 | S2 | S1 | S0 |
| IL_0024 | `stloc.3` | IL_0025 | S1 | S0 | S0 |
| IL_0025 | `ldloc.3` | IL_0026 | S0 | S1 | S0 |
| IL_0026 | `brtrue.s` | IL_0028, IL_0005 | S1 | S0 | S0 |
| IL_0028 | `ldloc.0` | IL_0029 | S0 | S1 | S0 |
| IL_0029 | `stloc.s` | IL_002b | S1 | S0 | S0 |
| IL_002b | `br.s` | IL_002d | S0 | S0 | S0 |
| IL_002d | `ldloc.s` | IL_002f | S0 | S1 | S0 |
| IL_002f | `ret` |  | S1 | S0 | S0 |

### Stack tree

| State | Parent | Top type | Depth | Children |
| --- | --- | --- | --- | --- |
| S0 | — | empty | 0 | S1 |
| S1 | S0 | Int32 | 1 | S2 |
| S2 | S1 | Int32 | 2 |  |
