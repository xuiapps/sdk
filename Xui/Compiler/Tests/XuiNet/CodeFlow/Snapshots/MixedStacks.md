# MixedStacks instruction graph

Source: [MixedStacks.cs](Fixtures/CodeFlow/MixedStacks.cs)

```csharp
namespace Samples.CodeFlow;

public static class MixedStacks
{
    public static int Run(int value)
    {
        double number = value + 0.5;
        string message = value > 0 ? "positive" : "other";
        System.Console.WriteLine(message);
        return (int)number;
    }
}
```

## Release

### Instructions

| Offset | Instruction | Successors | Before | After | Lowest |
| --- | --- | --- | --- | --- | --- |
| IL_0000 | `ldarg.0` | IL_0001 | S0 | S1 | S0 |
| IL_0001 | `conv.r8` | IL_0002 | S1 | S2 | S0 |
| IL_0002 | `ldc.r8` | IL_000b | S2 | S3 | S2 |
| IL_000b | `add` | IL_000c | S3 | S2 | S0 |
| IL_000c | `ldarg.0` | IL_000d | S2 | S4 | S2 |
| IL_000d | `ldc.i4.0` | IL_000e | S4 | S5 | S4 |
| IL_000e | `bgt.s` | IL_0010, IL_0017 | S5 | S2 | S2 |
| IL_0010 | `ldstr` | IL_0015 | S2 | S6 | S2 |
| IL_0015 | `br.s` | IL_001c | S6 | S6 | S6 |
| IL_0017 | `ldstr` | IL_001c | S2 | S6 | S2 |
| IL_001c | `call` | IL_0021 | S6 | S2 | S2 |
| IL_0021 | `conv.i4` | IL_0022 | S2 | S1 | S0 |
| IL_0022 | `ret` |  | S1 | S0 | S0 |

### Stack tree

| State | Parent | Top type | Depth | Children |
| --- | --- | --- | --- | --- |
| S0 | — | empty | 0 | S1, S2 |
| S1 | S0 | Int32 | 1 |  |
| S2 | S0 | Float | 1 | S3, S4, S6 |
| S3 | S2 | Float | 2 |  |
| S4 | S2 | Int32 | 2 | S5 |
| S5 | S4 | Int32 | 3 |  |
| S6 | S2 | String | 2 |  |

## Debug

### Instructions

| Offset | Instruction | Successors | Before | After | Lowest |
| --- | --- | --- | --- | --- | --- |
| IL_0000 | `nop` | IL_0001 | S0 | S0 | S0 |
| IL_0001 | `ldarg.0` | IL_0002 | S0 | S1 | S0 |
| IL_0002 | `conv.r8` | IL_0003 | S1 | S2 | S0 |
| IL_0003 | `ldc.r8` | IL_000c | S2 | S3 | S2 |
| IL_000c | `add` | IL_000d | S3 | S2 | S0 |
| IL_000d | `stloc.0` | IL_000e | S2 | S0 | S0 |
| IL_000e | `ldarg.0` | IL_000f | S0 | S1 | S0 |
| IL_000f | `ldc.i4.0` | IL_0010 | S1 | S4 | S1 |
| IL_0010 | `bgt.s` | IL_0012, IL_0019 | S4 | S0 | S0 |
| IL_0012 | `ldstr` | IL_0017 | S0 | S5 | S0 |
| IL_0017 | `br.s` | IL_001e | S5 | S5 | S5 |
| IL_0019 | `ldstr` | IL_001e | S0 | S5 | S0 |
| IL_001e | `stloc.1` | IL_001f | S5 | S0 | S0 |
| IL_001f | `ldloc.1` | IL_0020 | S0 | S5 | S0 |
| IL_0020 | `call` | IL_0025 | S5 | S0 | S0 |
| IL_0025 | `nop` | IL_0026 | S0 | S0 | S0 |
| IL_0026 | `ldloc.0` | IL_0027 | S0 | S2 | S0 |
| IL_0027 | `conv.i4` | IL_0028 | S2 | S1 | S0 |
| IL_0028 | `stloc.2` | IL_0029 | S1 | S0 | S0 |
| IL_0029 | `br.s` | IL_002b | S0 | S0 | S0 |
| IL_002b | `ldloc.2` | IL_002c | S0 | S1 | S0 |
| IL_002c | `ret` |  | S1 | S0 | S0 |

### Stack tree

| State | Parent | Top type | Depth | Children |
| --- | --- | --- | --- | --- |
| S0 | — | empty | 0 | S1, S2, S5 |
| S1 | S0 | Int32 | 1 | S4 |
| S2 | S0 | Float | 1 | S3 |
| S3 | S2 | Float | 2 |  |
| S4 | S1 | Int32 | 2 |  |
| S5 | S0 | String | 1 |  |
