# Nested instruction graph

Source: [Nested.cs](Fixtures/CodeFlow/Nested.cs)

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
```

## Release

### Instructions

| Offset | Instruction | Successors | Before | After | Lowest |
| --- | --- | --- | --- | --- | --- |
| IL_0000 | `ldc.i4.0` | IL_0001 | S0 | S1 | S0 |
| IL_0001 | `stloc.0` | IL_0002 | S1 | S0 | S0 |
| IL_0002 | `br.s` | IL_0023 | S0 | S0 | S0 |
| IL_0004 | `ldarg.0` | IL_0005 | S0 | S1 | S0 |
| IL_0005 | `ldc.i4.2` | IL_0006 | S1 | S2 | S1 |
| IL_0006 | `ble.s` | IL_0008, IL_000e | S2 | S0 | S0 |
| IL_0008 | `ldloc.0` | IL_0009 | S0 | S1 | S0 |
| IL_0009 | `ldarg.0` | IL_000a | S1 | S2 | S1 |
| IL_000a | `add` | IL_000b | S2 | S1 | S0 |
| IL_000b | `stloc.0` | IL_000c | S1 | S0 | S0 |
| IL_000c | `br.s` | IL_001e | S0 | S0 | S0 |
| IL_000e | `ldarg.0` | IL_000f | S0 | S1 | S0 |
| IL_000f | `ldc.i4.2` | IL_0010 | S1 | S2 | S1 |
| IL_0010 | `bne.un.s` | IL_0012, IL_0019 | S2 | S0 | S0 |
| IL_0012 | `ldloc.0` | IL_0013 | S0 | S1 | S0 |
| IL_0013 | `ldc.i4.s` | IL_0015 | S1 | S2 | S1 |
| IL_0015 | `add` | IL_0016 | S2 | S1 | S0 |
| IL_0016 | `stloc.0` | IL_0017 | S1 | S0 | S0 |
| IL_0017 | `br.s` | IL_001e | S0 | S0 | S0 |
| IL_0019 | `ldloc.0` | IL_001a | S0 | S1 | S0 |
| IL_001a | `ldc.i4.s` | IL_001c | S1 | S2 | S1 |
| IL_001c | `add` | IL_001d | S2 | S1 | S0 |
| IL_001d | `stloc.0` | IL_001e | S1 | S0 | S0 |
| IL_001e | `ldarg.0` | IL_001f | S0 | S1 | S0 |
| IL_001f | `ldc.i4.1` | IL_0020 | S1 | S2 | S1 |
| IL_0020 | `sub` | IL_0021 | S2 | S1 | S0 |
| IL_0021 | `starg.s` | IL_0023 | S1 | S0 | S0 |
| IL_0023 | `ldarg.0` | IL_0024 | S0 | S1 | S0 |
| IL_0024 | `ldc.i4.0` | IL_0025 | S1 | S2 | S1 |
| IL_0025 | `bgt.s` | IL_0027, IL_0004 | S2 | S0 | S0 |
| IL_0027 | `ldloc.0` | IL_0028 | S0 | S1 | S0 |
| IL_0028 | `ret` |  | S1 | S0 | S0 |

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
| IL_0003 | `br.s` | IL_002e | S0 | S0 | S0 |
| IL_0005 | `nop` | IL_0006 | S0 | S0 | S0 |
| IL_0006 | `ldarg.0` | IL_0007 | S0 | S1 | S0 |
| IL_0007 | `ldc.i4.2` | IL_0008 | S1 | S2 | S1 |
| IL_0008 | `cgt` | IL_000a | S2 | S1 | S0 |
| IL_000a | `stloc.1` | IL_000b | S1 | S0 | S0 |
| IL_000b | `ldloc.1` | IL_000c | S0 | S1 | S0 |
| IL_000c | `brfalse.s` | IL_000e, IL_0014 | S1 | S0 | S0 |
| IL_000e | `ldloc.0` | IL_000f | S0 | S1 | S0 |
| IL_000f | `ldarg.0` | IL_0010 | S1 | S2 | S1 |
| IL_0010 | `add` | IL_0011 | S2 | S1 | S0 |
| IL_0011 | `stloc.0` | IL_0012 | S1 | S0 | S0 |
| IL_0012 | `br.s` | IL_0028 | S0 | S0 | S0 |
| IL_0014 | `ldarg.0` | IL_0015 | S0 | S1 | S0 |
| IL_0015 | `ldc.i4.2` | IL_0016 | S1 | S2 | S1 |
| IL_0016 | `ceq` | IL_0018 | S2 | S1 | S0 |
| IL_0018 | `stloc.2` | IL_0019 | S1 | S0 | S0 |
| IL_0019 | `ldloc.2` | IL_001a | S0 | S1 | S0 |
| IL_001a | `brfalse.s` | IL_001c, IL_0023 | S1 | S0 | S0 |
| IL_001c | `ldloc.0` | IL_001d | S0 | S1 | S0 |
| IL_001d | `ldc.i4.s` | IL_001f | S1 | S2 | S1 |
| IL_001f | `add` | IL_0020 | S2 | S1 | S0 |
| IL_0020 | `stloc.0` | IL_0021 | S1 | S0 | S0 |
| IL_0021 | `br.s` | IL_0028 | S0 | S0 | S0 |
| IL_0023 | `ldloc.0` | IL_0024 | S0 | S1 | S0 |
| IL_0024 | `ldc.i4.s` | IL_0026 | S1 | S2 | S1 |
| IL_0026 | `add` | IL_0027 | S2 | S1 | S0 |
| IL_0027 | `stloc.0` | IL_0028 | S1 | S0 | S0 |
| IL_0028 | `ldarg.0` | IL_0029 | S0 | S1 | S0 |
| IL_0029 | `ldc.i4.1` | IL_002a | S1 | S2 | S1 |
| IL_002a | `sub` | IL_002b | S2 | S1 | S0 |
| IL_002b | `starg.s` | IL_002d | S1 | S0 | S0 |
| IL_002d | `nop` | IL_002e | S0 | S0 | S0 |
| IL_002e | `ldarg.0` | IL_002f | S0 | S1 | S0 |
| IL_002f | `ldc.i4.0` | IL_0030 | S1 | S2 | S1 |
| IL_0030 | `cgt` | IL_0032 | S2 | S1 | S0 |
| IL_0032 | `stloc.3` | IL_0033 | S1 | S0 | S0 |
| IL_0033 | `ldloc.3` | IL_0034 | S0 | S1 | S0 |
| IL_0034 | `brtrue.s` | IL_0036, IL_0005 | S1 | S0 | S0 |
| IL_0036 | `ldloc.0` | IL_0037 | S0 | S1 | S0 |
| IL_0037 | `stloc.s` | IL_0039 | S1 | S0 | S0 |
| IL_0039 | `br.s` | IL_003b | S0 | S0 | S0 |
| IL_003b | `ldloc.s` | IL_003d | S0 | S1 | S0 |
| IL_003d | `ret` |  | S1 | S0 | S0 |

### Stack tree

| State | Parent | Top type | Depth | Children |
| --- | --- | --- | --- | --- |
| S0 | — | empty | 0 | S1 |
| S1 | S0 | Int32 | 1 | S2 |
| S2 | S1 | Int32 | 2 |  |
