# WhileWithBreak instruction graph

Source: [WhileWithBreak.cs](Fixtures/CodeFlow/WhileWithBreak.cs)

```csharp
namespace Samples.CodeFlow;

public static class WhileWithBreak
{
    public static int Run(int value)
    {
        int total = 0;
        while (value > 0)
        {
            if (value == 3) break;
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
| IL_0002 | `br.s` | IL_0011 | S0 | S0 | S0 |
| IL_0004 | `ldarg.0` | IL_0005 | S0 | S1 | S0 |
| IL_0005 | `ldc.i4.3` | IL_0006 | S1 | S2 | S1 |
| IL_0006 | `beq.s` | IL_0008, IL_0015 | S2 | S0 | S0 |
| IL_0008 | `ldloc.0` | IL_0009 | S0 | S1 | S0 |
| IL_0009 | `ldarg.0` | IL_000a | S1 | S2 | S1 |
| IL_000a | `add` | IL_000b | S2 | S1 | S0 |
| IL_000b | `stloc.0` | IL_000c | S1 | S0 | S0 |
| IL_000c | `ldarg.0` | IL_000d | S0 | S1 | S0 |
| IL_000d | `ldc.i4.1` | IL_000e | S1 | S2 | S1 |
| IL_000e | `sub` | IL_000f | S2 | S1 | S0 |
| IL_000f | `starg.s` | IL_0011 | S1 | S0 | S0 |
| IL_0011 | `ldarg.0` | IL_0012 | S0 | S1 | S0 |
| IL_0012 | `ldc.i4.0` | IL_0013 | S1 | S2 | S1 |
| IL_0013 | `bgt.s` | IL_0015, IL_0004 | S2 | S0 | S0 |
| IL_0015 | `ldloc.0` | IL_0016 | S0 | S1 | S0 |
| IL_0016 | `ret` |  | S1 | S0 | S0 |

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
| IL_0003 | `br.s` | IL_001a | S0 | S0 | S0 |
| IL_0005 | `nop` | IL_0006 | S0 | S0 | S0 |
| IL_0006 | `ldarg.0` | IL_0007 | S0 | S1 | S0 |
| IL_0007 | `ldc.i4.3` | IL_0008 | S1 | S2 | S1 |
| IL_0008 | `ceq` | IL_000a | S2 | S1 | S0 |
| IL_000a | `stloc.1` | IL_000b | S1 | S0 | S0 |
| IL_000b | `ldloc.1` | IL_000c | S0 | S1 | S0 |
| IL_000c | `brfalse.s` | IL_000e, IL_0010 | S1 | S0 | S0 |
| IL_000e | `br.s` | IL_0022 | S0 | S0 | S0 |
| IL_0010 | `ldloc.0` | IL_0011 | S0 | S1 | S0 |
| IL_0011 | `ldarg.0` | IL_0012 | S1 | S2 | S1 |
| IL_0012 | `add` | IL_0013 | S2 | S1 | S0 |
| IL_0013 | `stloc.0` | IL_0014 | S1 | S0 | S0 |
| IL_0014 | `ldarg.0` | IL_0015 | S0 | S1 | S0 |
| IL_0015 | `ldc.i4.1` | IL_0016 | S1 | S2 | S1 |
| IL_0016 | `sub` | IL_0017 | S2 | S1 | S0 |
| IL_0017 | `starg.s` | IL_0019 | S1 | S0 | S0 |
| IL_0019 | `nop` | IL_001a | S0 | S0 | S0 |
| IL_001a | `ldarg.0` | IL_001b | S0 | S1 | S0 |
| IL_001b | `ldc.i4.0` | IL_001c | S1 | S2 | S1 |
| IL_001c | `cgt` | IL_001e | S2 | S1 | S0 |
| IL_001e | `stloc.2` | IL_001f | S1 | S0 | S0 |
| IL_001f | `ldloc.2` | IL_0020 | S0 | S1 | S0 |
| IL_0020 | `brtrue.s` | IL_0022, IL_0005 | S1 | S0 | S0 |
| IL_0022 | `ldloc.0` | IL_0023 | S0 | S1 | S0 |
| IL_0023 | `stloc.3` | IL_0024 | S1 | S0 | S0 |
| IL_0024 | `br.s` | IL_0026 | S0 | S0 | S0 |
| IL_0026 | `ldloc.3` | IL_0027 | S0 | S1 | S0 |
| IL_0027 | `ret` |  | S1 | S0 | S0 |

### Stack tree

| State | Parent | Top type | Depth | Children |
| --- | --- | --- | --- | --- |
| S0 | — | empty | 0 | S1 |
| S1 | S0 | Int32 | 1 | S2 |
| S2 | S1 | Int32 | 2 |  |
