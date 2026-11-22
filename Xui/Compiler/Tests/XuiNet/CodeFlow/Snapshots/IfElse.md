# IfElse instruction graph

Source: [IfElse.cs](Fixtures/CodeFlow/IfElse.cs)

```csharp
namespace Samples.CodeFlow;

public static class IfElse
{
    public static int Run(int value)
    {
        int result;
        if (value > 0)
            result = value + 10;
        else
            result = value - 10;
        return result;
    }
}
```

## Release

### Instructions

| Offset | Instruction | Successors | Before | After | Lowest |
| --- | --- | --- | --- | --- | --- |
| IL_0000 | `ldarg.0` | IL_0001 | S0 | S1 | S0 |
| IL_0001 | `ldc.i4.0` | IL_0002 | S1 | S2 | S1 |
| IL_0002 | `ble.s` | IL_0004, IL_000b | S2 | S0 | S0 |
| IL_0004 | `ldarg.0` | IL_0005 | S0 | S1 | S0 |
| IL_0005 | `ldc.i4.s` | IL_0007 | S1 | S2 | S1 |
| IL_0007 | `add` | IL_0008 | S2 | S1 | S0 |
| IL_0008 | `stloc.0` | IL_0009 | S1 | S0 | S0 |
| IL_0009 | `br.s` | IL_0010 | S0 | S0 | S0 |
| IL_000b | `ldarg.0` | IL_000c | S0 | S1 | S0 |
| IL_000c | `ldc.i4.s` | IL_000e | S1 | S2 | S1 |
| IL_000e | `sub` | IL_000f | S2 | S1 | S0 |
| IL_000f | `stloc.0` | IL_0010 | S1 | S0 | S0 |
| IL_0010 | `ldloc.0` | IL_0011 | S0 | S1 | S0 |
| IL_0011 | `ret` |  | S1 | S0 | S0 |

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
| IL_0003 | `cgt` | IL_0005 | S2 | S1 | S0 |
| IL_0005 | `stloc.1` | IL_0006 | S1 | S0 | S0 |
| IL_0006 | `ldloc.1` | IL_0007 | S0 | S1 | S0 |
| IL_0007 | `brfalse.s` | IL_0009, IL_0010 | S1 | S0 | S0 |
| IL_0009 | `ldarg.0` | IL_000a | S0 | S1 | S0 |
| IL_000a | `ldc.i4.s` | IL_000c | S1 | S2 | S1 |
| IL_000c | `add` | IL_000d | S2 | S1 | S0 |
| IL_000d | `stloc.0` | IL_000e | S1 | S0 | S0 |
| IL_000e | `br.s` | IL_0015 | S0 | S0 | S0 |
| IL_0010 | `ldarg.0` | IL_0011 | S0 | S1 | S0 |
| IL_0011 | `ldc.i4.s` | IL_0013 | S1 | S2 | S1 |
| IL_0013 | `sub` | IL_0014 | S2 | S1 | S0 |
| IL_0014 | `stloc.0` | IL_0015 | S1 | S0 | S0 |
| IL_0015 | `ldloc.0` | IL_0016 | S0 | S1 | S0 |
| IL_0016 | `stloc.2` | IL_0017 | S1 | S0 | S0 |
| IL_0017 | `br.s` | IL_0019 | S0 | S0 | S0 |
| IL_0019 | `ldloc.2` | IL_001a | S0 | S1 | S0 |
| IL_001a | `ret` |  | S1 | S0 | S0 |

### Stack tree

| State | Parent | Top type | Depth | Children |
| --- | --- | --- | --- | --- |
| S0 | — | empty | 0 | S1 |
| S1 | S0 | Int32 | 1 | S2 |
| S2 | S1 | Int32 | 2 |  |
