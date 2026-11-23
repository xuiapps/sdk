# For instruction graph

Source: [For.cs](Fixtures/CodeFlow/For.cs)

```csharp
namespace Samples.CodeFlow;

public static class For
{
    public static int Run(int value)
    {
        int total = 0;
        for (int i = 1; i <= value; i++) total += i;
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
| IL_0002 | `ldc.i4.1` | IL_0003 | S0 | S1 | S0 |
| IL_0003 | `stloc.1` | IL_0004 | S1 | S0 | S0 |
| IL_0004 | `br.s` | IL_000e | S0 | S0 | S0 |
| IL_0006 | `ldloc.0` | IL_0007 | S0 | S1 | S0 |
| IL_0007 | `ldloc.1` | IL_0008 | S1 | S2 | S1 |
| IL_0008 | `add` | IL_0009 | S2 | S1 | S0 |
| IL_0009 | `stloc.0` | IL_000a | S1 | S0 | S0 |
| IL_000a | `ldloc.1` | IL_000b | S0 | S1 | S0 |
| IL_000b | `ldc.i4.1` | IL_000c | S1 | S2 | S1 |
| IL_000c | `add` | IL_000d | S2 | S1 | S0 |
| IL_000d | `stloc.1` | IL_000e | S1 | S0 | S0 |
| IL_000e | `ldloc.1` | IL_000f | S0 | S1 | S0 |
| IL_000f | `ldarg.0` | IL_0010 | S1 | S2 | S1 |
| IL_0010 | `ble.s` | IL_0012, IL_0006 | S2 | S0 | S0 |
| IL_0012 | `ldloc.0` | IL_0013 | S0 | S1 | S0 |
| IL_0013 | `ret` |  | S1 | S0 | S0 |

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
| IL_0003 | `ldc.i4.1` | IL_0004 | S0 | S1 | S0 |
| IL_0004 | `stloc.1` | IL_0005 | S1 | S0 | S0 |
| IL_0005 | `br.s` | IL_000f | S0 | S0 | S0 |
| IL_0007 | `ldloc.0` | IL_0008 | S0 | S1 | S0 |
| IL_0008 | `ldloc.1` | IL_0009 | S1 | S2 | S1 |
| IL_0009 | `add` | IL_000a | S2 | S1 | S0 |
| IL_000a | `stloc.0` | IL_000b | S1 | S0 | S0 |
| IL_000b | `ldloc.1` | IL_000c | S0 | S1 | S0 |
| IL_000c | `ldc.i4.1` | IL_000d | S1 | S2 | S1 |
| IL_000d | `add` | IL_000e | S2 | S1 | S0 |
| IL_000e | `stloc.1` | IL_000f | S1 | S0 | S0 |
| IL_000f | `ldloc.1` | IL_0010 | S0 | S1 | S0 |
| IL_0010 | `ldarg.0` | IL_0011 | S1 | S2 | S1 |
| IL_0011 | `cgt` | IL_0013 | S2 | S1 | S0 |
| IL_0013 | `ldc.i4.0` | IL_0014 | S1 | S2 | S1 |
| IL_0014 | `ceq` | IL_0016 | S2 | S1 | S0 |
| IL_0016 | `stloc.2` | IL_0017 | S1 | S0 | S0 |
| IL_0017 | `ldloc.2` | IL_0018 | S0 | S1 | S0 |
| IL_0018 | `brtrue.s` | IL_001a, IL_0007 | S1 | S0 | S0 |
| IL_001a | `ldloc.0` | IL_001b | S0 | S1 | S0 |
| IL_001b | `stloc.3` | IL_001c | S1 | S0 | S0 |
| IL_001c | `br.s` | IL_001e | S0 | S0 | S0 |
| IL_001e | `ldloc.3` | IL_001f | S0 | S1 | S0 |
| IL_001f | `ret` |  | S1 | S0 | S0 |

### Stack tree

| State | Parent | Top type | Depth | Children |
| --- | --- | --- | --- | --- |
| S0 | — | empty | 0 | S1 |
| S1 | S0 | Int32 | 1 | S2 |
| S2 | S1 | Int32 | 2 |  |
