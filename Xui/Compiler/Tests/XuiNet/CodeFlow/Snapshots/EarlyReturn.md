# EarlyReturn instruction graph

Source: [EarlyReturn.cs](Fixtures/CodeFlow/EarlyReturn.cs)

```csharp
namespace Samples.CodeFlow;

public static class EarlyReturn
{
    public static int Run(int value)
    {
        if (value < 0)
            return -value;
        if (value == 0)
            return 10;
        return value + 1;
    }
}
```

## Release

### Instructions

| Offset | Instruction | Successors | Before | After | Lowest |
| --- | --- | --- | --- | --- | --- |
| IL_0000 | `ldarg.0` | IL_0001 | S0 | S1 | S0 |
| IL_0001 | `ldc.i4.0` | IL_0002 | S1 | S2 | S1 |
| IL_0002 | `bge.s` | IL_0004, IL_0007 | S2 | S0 | S0 |
| IL_0004 | `ldarg.0` | IL_0005 | S0 | S1 | S0 |
| IL_0005 | `neg` | IL_0006 | S1 | S1 | S0 |
| IL_0006 | `ret` |  | S1 | S0 | S0 |
| IL_0007 | `ldarg.0` | IL_0008 | S0 | S1 | S0 |
| IL_0008 | `brtrue.s` | IL_000a, IL_000d | S1 | S0 | S0 |
| IL_000a | `ldc.i4.s` | IL_000c | S0 | S1 | S0 |
| IL_000c | `ret` |  | S1 | S0 | S0 |
| IL_000d | `ldarg.0` | IL_000e | S0 | S1 | S0 |
| IL_000e | `ldc.i4.1` | IL_000f | S1 | S2 | S1 |
| IL_000f | `add` | IL_0010 | S2 | S1 | S0 |
| IL_0010 | `ret` |  | S1 | S0 | S0 |

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
| IL_0001 | `ldarg.0` | IL_0002 | S0 | S1 | S0 |
| IL_0002 | `ldc.i4.0` | IL_0003 | S1 | S2 | S1 |
| IL_0003 | `clt` | IL_0005 | S2 | S1 | S0 |
| IL_0005 | `stloc.0` | IL_0006 | S1 | S0 | S0 |
| IL_0006 | `ldloc.0` | IL_0007 | S0 | S1 | S0 |
| IL_0007 | `brfalse.s` | IL_0009, IL_000e | S1 | S0 | S0 |
| IL_0009 | `ldarg.0` | IL_000a | S0 | S1 | S0 |
| IL_000a | `neg` | IL_000b | S1 | S1 | S0 |
| IL_000b | `stloc.1` | IL_000c | S1 | S0 | S0 |
| IL_000c | `br.s` | IL_0021 | S0 | S0 | S0 |
| IL_000e | `ldarg.0` | IL_000f | S0 | S1 | S0 |
| IL_000f | `ldc.i4.0` | IL_0010 | S1 | S2 | S1 |
| IL_0010 | `ceq` | IL_0012 | S2 | S1 | S0 |
| IL_0012 | `stloc.2` | IL_0013 | S1 | S0 | S0 |
| IL_0013 | `ldloc.2` | IL_0014 | S0 | S1 | S0 |
| IL_0014 | `brfalse.s` | IL_0016, IL_001b | S1 | S0 | S0 |
| IL_0016 | `ldc.i4.s` | IL_0018 | S0 | S1 | S0 |
| IL_0018 | `stloc.1` | IL_0019 | S1 | S0 | S0 |
| IL_0019 | `br.s` | IL_0021 | S0 | S0 | S0 |
| IL_001b | `ldarg.0` | IL_001c | S0 | S1 | S0 |
| IL_001c | `ldc.i4.1` | IL_001d | S1 | S2 | S1 |
| IL_001d | `add` | IL_001e | S2 | S1 | S0 |
| IL_001e | `stloc.1` | IL_001f | S1 | S0 | S0 |
| IL_001f | `br.s` | IL_0021 | S0 | S0 | S0 |
| IL_0021 | `ldloc.1` | IL_0022 | S0 | S1 | S0 |
| IL_0022 | `ret` |  | S1 | S0 | S0 |

### Stack tree

| State | Parent | Top type | Depth | Children |
| --- | --- | --- | --- | --- |
| S0 | — | empty | 0 | S1 |
| S1 | S0 | Int32 | 1 | S2 |
| S2 | S1 | Int32 | 2 |  |
