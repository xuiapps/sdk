# NestedConditionals instruction graph

Source: [NestedConditionals.cs](Fixtures/CodeFlow/NestedConditionals.cs)

```csharp
namespace Samples.CodeFlow;

public static class NestedConditionals
{
    public static int Run(int value)
    {
        if (value > 0)
        {
            if (value > 1) return 20;
            return 10;
        }
        else
        {
            if (value < 0) return -10;
            return 0;
        }
    }
}
```

## Release

### Instructions

| Offset | Instruction | Successors | Before | After | Lowest |
| --- | --- | --- | --- | --- | --- |
| IL_0000 | `ldarg.0` | IL_0001 | S0 | S1 | S0 |
| IL_0001 | `ldc.i4.0` | IL_0002 | S1 | S2 | S1 |
| IL_0002 | `ble.s` | IL_0004, IL_000e | S2 | S0 | S0 |
| IL_0004 | `ldarg.0` | IL_0005 | S0 | S1 | S0 |
| IL_0005 | `ldc.i4.1` | IL_0006 | S1 | S2 | S1 |
| IL_0006 | `ble.s` | IL_0008, IL_000b | S2 | S0 | S0 |
| IL_0008 | `ldc.i4.s` | IL_000a | S0 | S1 | S0 |
| IL_000a | `ret` |  | S1 | S0 | S0 |
| IL_000b | `ldc.i4.s` | IL_000d | S0 | S1 | S0 |
| IL_000d | `ret` |  | S1 | S0 | S0 |
| IL_000e | `ldarg.0` | IL_000f | S0 | S1 | S0 |
| IL_000f | `ldc.i4.0` | IL_0010 | S1 | S2 | S1 |
| IL_0010 | `bge.s` | IL_0012, IL_0015 | S2 | S0 | S0 |
| IL_0012 | `ldc.i4.s` | IL_0014 | S0 | S1 | S0 |
| IL_0014 | `ret` |  | S1 | S0 | S0 |
| IL_0015 | `ldc.i4.0` | IL_0016 | S0 | S1 | S0 |
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
| IL_0001 | `ldarg.0` | IL_0002 | S0 | S1 | S0 |
| IL_0002 | `ldc.i4.0` | IL_0003 | S1 | S2 | S1 |
| IL_0003 | `cgt` | IL_0005 | S2 | S1 | S0 |
| IL_0005 | `stloc.0` | IL_0006 | S1 | S0 | S0 |
| IL_0006 | `ldloc.0` | IL_0007 | S0 | S1 | S0 |
| IL_0007 | `brfalse.s` | IL_0009, IL_001c | S1 | S0 | S0 |
| IL_0009 | `nop` | IL_000a | S0 | S0 | S0 |
| IL_000a | `ldarg.0` | IL_000b | S0 | S1 | S0 |
| IL_000b | `ldc.i4.1` | IL_000c | S1 | S2 | S1 |
| IL_000c | `cgt` | IL_000e | S2 | S1 | S0 |
| IL_000e | `stloc.1` | IL_000f | S1 | S0 | S0 |
| IL_000f | `ldloc.1` | IL_0010 | S0 | S1 | S0 |
| IL_0010 | `brfalse.s` | IL_0012, IL_0017 | S1 | S0 | S0 |
| IL_0012 | `ldc.i4.s` | IL_0014 | S0 | S1 | S0 |
| IL_0014 | `stloc.2` | IL_0015 | S1 | S0 | S0 |
| IL_0015 | `br.s` | IL_002e | S0 | S0 | S0 |
| IL_0017 | `ldc.i4.s` | IL_0019 | S0 | S1 | S0 |
| IL_0019 | `stloc.2` | IL_001a | S1 | S0 | S0 |
| IL_001a | `br.s` | IL_002e | S0 | S0 | S0 |
| IL_001c | `nop` | IL_001d | S0 | S0 | S0 |
| IL_001d | `ldarg.0` | IL_001e | S0 | S1 | S0 |
| IL_001e | `ldc.i4.0` | IL_001f | S1 | S2 | S1 |
| IL_001f | `clt` | IL_0021 | S2 | S1 | S0 |
| IL_0021 | `stloc.3` | IL_0022 | S1 | S0 | S0 |
| IL_0022 | `ldloc.3` | IL_0023 | S0 | S1 | S0 |
| IL_0023 | `brfalse.s` | IL_0025, IL_002a | S1 | S0 | S0 |
| IL_0025 | `ldc.i4.s` | IL_0027 | S0 | S1 | S0 |
| IL_0027 | `stloc.2` | IL_0028 | S1 | S0 | S0 |
| IL_0028 | `br.s` | IL_002e | S0 | S0 | S0 |
| IL_002a | `ldc.i4.0` | IL_002b | S0 | S1 | S0 |
| IL_002b | `stloc.2` | IL_002c | S1 | S0 | S0 |
| IL_002c | `br.s` | IL_002e | S0 | S0 | S0 |
| IL_002e | `ldloc.2` | IL_002f | S0 | S1 | S0 |
| IL_002f | `ret` |  | S1 | S0 | S0 |

### Stack tree

| State | Parent | Top type | Depth | Children |
| --- | --- | --- | --- | --- |
| S0 | — | empty | 0 | S1 |
| S1 | S0 | Int32 | 1 | S2 |
| S2 | S1 | Int32 | 2 |  |
