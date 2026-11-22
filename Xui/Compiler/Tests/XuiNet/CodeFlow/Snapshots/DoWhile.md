# DoWhile instruction graph

Source: [DoWhile.cs](Fixtures/CodeFlow/DoWhile.cs)

```csharp
namespace Samples.CodeFlow;

public static class DoWhile
{
    public static int Run(int value)
    {
        int total = 0;
        do
        {
            total += value;
            value--;
        }
        while (value > 0);
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
| IL_0002 | `ldloc.0` | IL_0003 | S0 | S1 | S0 |
| IL_0003 | `ldarg.0` | IL_0004 | S1 | S2 | S1 |
| IL_0004 | `add` | IL_0005 | S2 | S1 | S0 |
| IL_0005 | `stloc.0` | IL_0006 | S1 | S0 | S0 |
| IL_0006 | `ldarg.0` | IL_0007 | S0 | S1 | S0 |
| IL_0007 | `ldc.i4.1` | IL_0008 | S1 | S2 | S1 |
| IL_0008 | `sub` | IL_0009 | S2 | S1 | S0 |
| IL_0009 | `starg.s` | IL_000b | S1 | S0 | S0 |
| IL_000b | `ldarg.0` | IL_000c | S0 | S1 | S0 |
| IL_000c | `ldc.i4.0` | IL_000d | S1 | S2 | S1 |
| IL_000d | `bgt.s` | IL_000f, IL_0002 | S2 | S0 | S0 |
| IL_000f | `ldloc.0` | IL_0010 | S0 | S1 | S0 |
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
| IL_0001 | `ldc.i4.0` | IL_0002 | S0 | S1 | S0 |
| IL_0002 | `stloc.0` | IL_0003 | S1 | S0 | S0 |
| IL_0003 | `nop` | IL_0004 | S0 | S0 | S0 |
| IL_0004 | `ldloc.0` | IL_0005 | S0 | S1 | S0 |
| IL_0005 | `ldarg.0` | IL_0006 | S1 | S2 | S1 |
| IL_0006 | `add` | IL_0007 | S2 | S1 | S0 |
| IL_0007 | `stloc.0` | IL_0008 | S1 | S0 | S0 |
| IL_0008 | `ldarg.0` | IL_0009 | S0 | S1 | S0 |
| IL_0009 | `ldc.i4.1` | IL_000a | S1 | S2 | S1 |
| IL_000a | `sub` | IL_000b | S2 | S1 | S0 |
| IL_000b | `starg.s` | IL_000d | S1 | S0 | S0 |
| IL_000d | `nop` | IL_000e | S0 | S0 | S0 |
| IL_000e | `ldarg.0` | IL_000f | S0 | S1 | S0 |
| IL_000f | `ldc.i4.0` | IL_0010 | S1 | S2 | S1 |
| IL_0010 | `cgt` | IL_0012 | S2 | S1 | S0 |
| IL_0012 | `stloc.1` | IL_0013 | S1 | S0 | S0 |
| IL_0013 | `ldloc.1` | IL_0014 | S0 | S1 | S0 |
| IL_0014 | `brtrue.s` | IL_0016, IL_0003 | S1 | S0 | S0 |
| IL_0016 | `ldloc.0` | IL_0017 | S0 | S1 | S0 |
| IL_0017 | `stloc.2` | IL_0018 | S1 | S0 | S0 |
| IL_0018 | `br.s` | IL_001a | S0 | S0 | S0 |
| IL_001a | `ldloc.2` | IL_001b | S0 | S1 | S0 |
| IL_001b | `ret` |  | S1 | S0 | S0 |

### Stack tree

| State | Parent | Top type | Depth | Children |
| --- | --- | --- | --- | --- |
| S0 | — | empty | 0 | S1 |
| S1 | S0 | Int32 | 1 | S2 |
| S2 | S1 | Int32 | 2 |  |
