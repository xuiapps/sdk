# ElseIf instruction graph

Source: [ElseIf.cs](Fixtures/CodeFlow/ElseIf.cs)

```csharp
namespace Samples.CodeFlow;

public static class ElseIf
{
    public static int Run(int value)
    {
        int result;
        if (value < 0)
            result = -1;
        else if (value == 0)
            result = 0;
        else
            result = 1;
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
| IL_0002 | `bge.s` | IL_0004, IL_0008 | S2 | S0 | S0 |
| IL_0004 | `ldc.i4.m1` | IL_0005 | S0 | S1 | S0 |
| IL_0005 | `stloc.0` | IL_0006 | S1 | S0 | S0 |
| IL_0006 | `br.s` | IL_0011 | S0 | S0 | S0 |
| IL_0008 | `ldarg.0` | IL_0009 | S0 | S1 | S0 |
| IL_0009 | `brtrue.s` | IL_000b, IL_000f | S1 | S0 | S0 |
| IL_000b | `ldc.i4.0` | IL_000c | S0 | S1 | S0 |
| IL_000c | `stloc.0` | IL_000d | S1 | S0 | S0 |
| IL_000d | `br.s` | IL_0011 | S0 | S0 | S0 |
| IL_000f | `ldc.i4.1` | IL_0010 | S0 | S1 | S0 |
| IL_0010 | `stloc.0` | IL_0011 | S1 | S0 | S0 |
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
| IL_0001 | `ldarg.0` | IL_0002 | S0 | S1 | S0 |
| IL_0002 | `ldc.i4.0` | IL_0003 | S1 | S2 | S1 |
| IL_0003 | `clt` | IL_0005 | S2 | S1 | S0 |
| IL_0005 | `stloc.1` | IL_0006 | S1 | S0 | S0 |
| IL_0006 | `ldloc.1` | IL_0007 | S0 | S1 | S0 |
| IL_0007 | `brfalse.s` | IL_0009, IL_000d | S1 | S0 | S0 |
| IL_0009 | `ldc.i4.m1` | IL_000a | S0 | S1 | S0 |
| IL_000a | `stloc.0` | IL_000b | S1 | S0 | S0 |
| IL_000b | `br.s` | IL_001b | S0 | S0 | S0 |
| IL_000d | `ldarg.0` | IL_000e | S0 | S1 | S0 |
| IL_000e | `ldc.i4.0` | IL_000f | S1 | S2 | S1 |
| IL_000f | `ceq` | IL_0011 | S2 | S1 | S0 |
| IL_0011 | `stloc.2` | IL_0012 | S1 | S0 | S0 |
| IL_0012 | `ldloc.2` | IL_0013 | S0 | S1 | S0 |
| IL_0013 | `brfalse.s` | IL_0015, IL_0019 | S1 | S0 | S0 |
| IL_0015 | `ldc.i4.0` | IL_0016 | S0 | S1 | S0 |
| IL_0016 | `stloc.0` | IL_0017 | S1 | S0 | S0 |
| IL_0017 | `br.s` | IL_001b | S0 | S0 | S0 |
| IL_0019 | `ldc.i4.1` | IL_001a | S0 | S1 | S0 |
| IL_001a | `stloc.0` | IL_001b | S1 | S0 | S0 |
| IL_001b | `ldloc.0` | IL_001c | S0 | S1 | S0 |
| IL_001c | `stloc.3` | IL_001d | S1 | S0 | S0 |
| IL_001d | `br.s` | IL_001f | S0 | S0 | S0 |
| IL_001f | `ldloc.3` | IL_0020 | S0 | S1 | S0 |
| IL_0020 | `ret` |  | S1 | S0 | S0 |

### Stack tree

| State | Parent | Top type | Depth | Children |
| --- | --- | --- | --- | --- |
| S0 | — | empty | 0 | S1 |
| S1 | S0 | Int32 | 1 | S2 |
| S2 | S1 | Int32 | 2 |  |
