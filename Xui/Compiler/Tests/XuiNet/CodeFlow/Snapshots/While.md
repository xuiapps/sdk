# While instruction graph

Source: [While.cs](Fixtures/CodeFlow/While.cs)

```csharp
namespace Samples.CodeFlow;

public static class While
{
    public static int Run(int value)
    {
        int total = 0;
        while (value > 0)
        {
            total += value;
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
| IL_0002 | `br.s` | IL_000d | S0 | S0 | S0 |
| IL_0004 | `ldloc.0` | IL_0005 | S0 | S1 | S0 |
| IL_0005 | `ldarg.0` | IL_0006 | S1 | S2 | S1 |
| IL_0006 | `add` | IL_0007 | S2 | S1 | S0 |
| IL_0007 | `stloc.0` | IL_0008 | S1 | S0 | S0 |
| IL_0008 | `ldarg.0` | IL_0009 | S0 | S1 | S0 |
| IL_0009 | `ldc.i4.1` | IL_000a | S1 | S2 | S1 |
| IL_000a | `sub` | IL_000b | S2 | S1 | S0 |
| IL_000b | `starg.s` | IL_000d | S1 | S0 | S0 |
| IL_000d | `ldarg.0` | IL_000e | S0 | S1 | S0 |
| IL_000e | `ldc.i4.0` | IL_000f | S1 | S2 | S1 |
| IL_000f | `bgt.s` | IL_0011, IL_0004 | S2 | S0 | S0 |
| IL_0011 | `ldloc.0` | IL_0012 | S0 | S1 | S0 |
| IL_0012 | `ret` |  | S1 | S0 | S0 |

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
| IL_0003 | `br.s` | IL_0010 | S0 | S0 | S0 |
| IL_0005 | `nop` | IL_0006 | S0 | S0 | S0 |
| IL_0006 | `ldloc.0` | IL_0007 | S0 | S1 | S0 |
| IL_0007 | `ldarg.0` | IL_0008 | S1 | S2 | S1 |
| IL_0008 | `add` | IL_0009 | S2 | S1 | S0 |
| IL_0009 | `stloc.0` | IL_000a | S1 | S0 | S0 |
| IL_000a | `ldarg.0` | IL_000b | S0 | S1 | S0 |
| IL_000b | `ldc.i4.1` | IL_000c | S1 | S2 | S1 |
| IL_000c | `sub` | IL_000d | S2 | S1 | S0 |
| IL_000d | `starg.s` | IL_000f | S1 | S0 | S0 |
| IL_000f | `nop` | IL_0010 | S0 | S0 | S0 |
| IL_0010 | `ldarg.0` | IL_0011 | S0 | S1 | S0 |
| IL_0011 | `ldc.i4.0` | IL_0012 | S1 | S2 | S1 |
| IL_0012 | `cgt` | IL_0014 | S2 | S1 | S0 |
| IL_0014 | `stloc.1` | IL_0015 | S1 | S0 | S0 |
| IL_0015 | `ldloc.1` | IL_0016 | S0 | S1 | S0 |
| IL_0016 | `brtrue.s` | IL_0018, IL_0005 | S1 | S0 | S0 |
| IL_0018 | `ldloc.0` | IL_0019 | S0 | S1 | S0 |
| IL_0019 | `stloc.2` | IL_001a | S1 | S0 | S0 |
| IL_001a | `br.s` | IL_001c | S0 | S0 | S0 |
| IL_001c | `ldloc.2` | IL_001d | S0 | S1 | S0 |
| IL_001d | `ret` |  | S1 | S0 | S0 |

### Stack tree

| State | Parent | Top type | Depth | Children |
| --- | --- | --- | --- | --- |
| S0 | — | empty | 0 | S1 |
| S1 | S0 | Int32 | 1 | S2 |
| S2 | S1 | Int32 | 2 |  |
