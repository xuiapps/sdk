# StackPrefix instruction graph

Source: [StackPrefix.cs](Fixtures/CodeFlow/StackPrefix.cs)

```csharp
namespace Samples.CodeFlow;

public static class StackPrefix
{
    private static int Add(int left, int right) => left + right;

    public static int Run(int value) => Add(5, value > 0 ? 10 : 20);
}
```

## Release

### Instructions

| Offset | Instruction | Successors | Before | After | Lowest |
| --- | --- | --- | --- | --- | --- |
| IL_0000 | `ldc.i4.5` | IL_0001 | S0 | S1 | S0 |
| IL_0001 | `ldarg.0` | IL_0002 | S1 | S2 | S1 |
| IL_0002 | `ldc.i4.0` | IL_0003 | S2 | S3 | S2 |
| IL_0003 | `bgt.s` | IL_0005, IL_0009 | S3 | S1 | S1 |
| IL_0005 | `ldc.i4.s` | IL_0007 | S1 | S2 | S1 |
| IL_0007 | `br.s` | IL_000b | S2 | S2 | S2 |
| IL_0009 | `ldc.i4.s` | IL_000b | S1 | S2 | S1 |
| IL_000b | `call` | IL_0010 | S2 | S1 | S0 |
| IL_0010 | `ret` |  | S1 | S0 | S0 |

### Stack tree

| State | Parent | Top type | Depth | Children |
| --- | --- | --- | --- | --- |
| S0 | — | empty | 0 | S1 |
| S1 | S0 | Int32 | 1 | S2 |
| S2 | S1 | Int32 | 2 | S3 |
| S3 | S2 | Int32 | 3 |  |

## Debug

### Instructions

| Offset | Instruction | Successors | Before | After | Lowest |
| --- | --- | --- | --- | --- | --- |
| IL_0000 | `ldc.i4.5` | IL_0001 | S0 | S1 | S0 |
| IL_0001 | `ldarg.0` | IL_0002 | S1 | S2 | S1 |
| IL_0002 | `ldc.i4.0` | IL_0003 | S2 | S3 | S2 |
| IL_0003 | `bgt.s` | IL_0005, IL_0009 | S3 | S1 | S1 |
| IL_0005 | `ldc.i4.s` | IL_0007 | S1 | S2 | S1 |
| IL_0007 | `br.s` | IL_000b | S2 | S2 | S2 |
| IL_0009 | `ldc.i4.s` | IL_000b | S1 | S2 | S1 |
| IL_000b | `call` | IL_0010 | S2 | S1 | S0 |
| IL_0010 | `ret` |  | S1 | S0 | S0 |

### Stack tree

| State | Parent | Top type | Depth | Children |
| --- | --- | --- | --- | --- |
| S0 | — | empty | 0 | S1 |
| S1 | S0 | Int32 | 1 | S2 |
| S2 | S1 | Int32 | 2 | S3 |
| S3 | S2 | Int32 | 3 |  |
