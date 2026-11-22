# If instruction graph

Source: [If.cs](Fixtures/CodeFlow/If.cs)

```csharp
namespace Samples.CodeFlow;

public static class If
{
    public static int Run(int value)
    {
        int result = value;
        if (value > 0)
            result = value + 10;
        return result;
    }
}
```

## Release

### Instructions

| Offset | Instruction | Successors | Before | After | Lowest |
| --- | --- | --- | --- | --- | --- |
| IL_0000 | `ldarg.0` | IL_0001 | S0 | S1 | S0 |
| IL_0001 | `stloc.0` | IL_0002 | S1 | S0 | S0 |
| IL_0002 | `ldarg.0` | IL_0003 | S0 | S1 | S0 |
| IL_0003 | `ldc.i4.0` | IL_0004 | S1 | S2 | S1 |
| IL_0004 | `ble.s` | IL_0006, IL_000b | S2 | S0 | S0 |
| IL_0006 | `ldarg.0` | IL_0007 | S0 | S1 | S0 |
| IL_0007 | `ldc.i4.s` | IL_0009 | S1 | S2 | S1 |
| IL_0009 | `add` | IL_000a | S2 | S1 | S0 |
| IL_000a | `stloc.0` | IL_000b | S1 | S0 | S0 |
| IL_000b | `ldloc.0` | IL_000c | S0 | S1 | S0 |
| IL_000c | `ret` |  | S1 | S0 | S0 |

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
| IL_0002 | `stloc.0` | IL_0003 | S1 | S0 | S0 |
| IL_0003 | `ldarg.0` | IL_0004 | S0 | S1 | S0 |
| IL_0004 | `ldc.i4.0` | IL_0005 | S1 | S2 | S1 |
| IL_0005 | `cgt` | IL_0007 | S2 | S1 | S0 |
| IL_0007 | `stloc.1` | IL_0008 | S1 | S0 | S0 |
| IL_0008 | `ldloc.1` | IL_0009 | S0 | S1 | S0 |
| IL_0009 | `brfalse.s` | IL_000b, IL_0010 | S1 | S0 | S0 |
| IL_000b | `ldarg.0` | IL_000c | S0 | S1 | S0 |
| IL_000c | `ldc.i4.s` | IL_000e | S1 | S2 | S1 |
| IL_000e | `add` | IL_000f | S2 | S1 | S0 |
| IL_000f | `stloc.0` | IL_0010 | S1 | S0 | S0 |
| IL_0010 | `ldloc.0` | IL_0011 | S0 | S1 | S0 |
| IL_0011 | `stloc.2` | IL_0012 | S1 | S0 | S0 |
| IL_0012 | `br.s` | IL_0014 | S0 | S0 | S0 |
| IL_0014 | `ldloc.2` | IL_0015 | S0 | S1 | S0 |
| IL_0015 | `ret` |  | S1 | S0 | S0 |

### Stack tree

| State | Parent | Top type | Depth | Children |
| --- | --- | --- | --- | --- |
| S0 | — | empty | 0 | S1 |
| S1 | S0 | Int32 | 1 | S2 |
| S2 | S1 | Int32 | 2 |  |
