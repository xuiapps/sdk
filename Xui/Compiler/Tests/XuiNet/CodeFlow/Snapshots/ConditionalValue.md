# ConditionalValue instruction graph

Source: [ConditionalValue.cs](Fixtures/CodeFlow/ConditionalValue.cs)

```csharp
namespace Samples.CodeFlow;

public static class ConditionalValue
{
    public static int Run(int value) => value > 0 ? 10 : 20;
}
```

## Release

### Instructions

| Offset | Instruction | Successors | Before | After | Lowest |
| --- | --- | --- | --- | --- | --- |
| IL_0000 | `ldarg.0` | IL_0001 | S0 | S1 | S0 |
| IL_0001 | `ldc.i4.0` | IL_0002 | S1 | S2 | S1 |
| IL_0002 | `bgt.s` | IL_0004, IL_0007 | S2 | S0 | S0 |
| IL_0004 | `ldc.i4.s` | IL_0006 | S0 | S1 | S0 |
| IL_0006 | `ret` |  | S1 | S0 | S0 |
| IL_0007 | `ldc.i4.s` | IL_0009 | S0 | S1 | S0 |
| IL_0009 | `ret` |  | S1 | S0 | S0 |

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
| IL_0000 | `ldarg.0` | IL_0001 | S0 | S1 | S0 |
| IL_0001 | `ldc.i4.0` | IL_0002 | S1 | S2 | S1 |
| IL_0002 | `bgt.s` | IL_0004, IL_0008 | S2 | S0 | S0 |
| IL_0004 | `ldc.i4.s` | IL_0006 | S0 | S1 | S0 |
| IL_0006 | `br.s` | IL_000a | S1 | S1 | S1 |
| IL_0008 | `ldc.i4.s` | IL_000a | S0 | S1 | S0 |
| IL_000a | `ret` |  | S1 | S0 | S0 |

### Stack tree

| State | Parent | Top type | Depth | Children |
| --- | --- | --- | --- | --- |
| S0 | — | empty | 0 | S1 |
| S1 | S0 | Int32 | 1 | S2 |
| S2 | S1 | Int32 | 2 |  |
