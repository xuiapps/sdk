# SharedConditionalTail instruction graph

Source: [SharedConditionalTail.cs](Fixtures/CodeFlow/SharedConditionalTail.cs)

```csharp
namespace Samples.CodeFlow;

public static class SharedConditionalTail
{
    private static int checks;

    private static bool Before(int now, int showAt)
    {
        checks++;
        return now < showAt;
    }

    public static int Run(int value)
    {
        checks = 0;
        bool tapped = (value & 1) != 0;
        int now = value, showAt = 2, fadeAt = 0;
        bool active = true;
        if (!tapped && Before(now, showAt)) active = false;
        else
        {
            showAt = now < showAt ? now : showAt;
            fadeAt = now + (tapped ? 20 : 0);
        }
        if (tapped) fadeAt += 3;
        return (active ? 1000 : 0) + showAt + fadeAt + checks * 100;
    }
}
```

## Release

### Instructions

| Offset | Instruction | Successors | Before | After | Lowest |
| --- | --- | --- | --- | --- | --- |
| IL_0000 | `ldc.i4.0` | IL_0001 | S0 | S1 | S0 |
| IL_0001 | `stsfld` | IL_0006 | S1 | S0 | S0 |
| IL_0006 | `ldarg.0` | IL_0007 | S0 | S1 | S0 |
| IL_0007 | `ldc.i4.1` | IL_0008 | S1 | S2 | S1 |
| IL_0008 | `and` | IL_0009 | S2 | S1 | S0 |
| IL_0009 | `ldc.i4.0` | IL_000a | S1 | S2 | S1 |
| IL_000a | `cgt.un` | IL_000c | S2 | S1 | S0 |
| IL_000c | `stloc.0` | IL_000d | S1 | S0 | S0 |
| IL_000d | `ldarg.0` | IL_000e | S0 | S1 | S0 |
| IL_000e | `stloc.1` | IL_000f | S1 | S0 | S0 |
| IL_000f | `ldc.i4.2` | IL_0010 | S0 | S1 | S0 |
| IL_0010 | `stloc.2` | IL_0011 | S1 | S0 | S0 |
| IL_0011 | `ldc.i4.0` | IL_0012 | S0 | S1 | S0 |
| IL_0012 | `stloc.3` | IL_0013 | S1 | S0 | S0 |
| IL_0013 | `ldc.i4.1` | IL_0014 | S0 | S1 | S0 |
| IL_0014 | `stloc.s` | IL_0016 | S1 | S0 | S0 |
| IL_0016 | `ldloc.0` | IL_0017 | S0 | S1 | S0 |
| IL_0017 | `brtrue.s` | IL_0019, IL_0027 | S1 | S0 | S0 |
| IL_0019 | `ldloc.1` | IL_001a | S0 | S1 | S0 |
| IL_001a | `ldloc.2` | IL_001b | S1 | S2 | S1 |
| IL_001b | `call` | IL_0020 | S2 | S1 | S0 |
| IL_0020 | `brfalse.s` | IL_0022, IL_0027 | S1 | S0 | S0 |
| IL_0022 | `ldc.i4.0` | IL_0023 | S0 | S1 | S0 |
| IL_0023 | `stloc.s` | IL_0025 | S1 | S0 | S0 |
| IL_0025 | `br.s` | IL_003b | S0 | S0 | S0 |
| IL_0027 | `ldloc.1` | IL_0028 | S0 | S1 | S0 |
| IL_0028 | `ldloc.2` | IL_0029 | S1 | S2 | S1 |
| IL_0029 | `blt.s` | IL_002b, IL_002e | S2 | S0 | S0 |
| IL_002b | `ldloc.2` | IL_002c | S0 | S1 | S0 |
| IL_002c | `br.s` | IL_002f | S1 | S1 | S1 |
| IL_002e | `ldloc.1` | IL_002f | S0 | S1 | S0 |
| IL_002f | `stloc.2` | IL_0030 | S1 | S0 | S0 |
| IL_0030 | `ldloc.1` | IL_0031 | S0 | S1 | S0 |
| IL_0031 | `ldloc.0` | IL_0032 | S1 | S2 | S1 |
| IL_0032 | `brtrue.s` | IL_0034, IL_0037 | S2 | S1 | S1 |
| IL_0034 | `ldc.i4.0` | IL_0035 | S1 | S2 | S1 |
| IL_0035 | `br.s` | IL_0039 | S2 | S2 | S2 |
| IL_0037 | `ldc.i4.s` | IL_0039 | S1 | S2 | S1 |
| IL_0039 | `add` | IL_003a | S2 | S1 | S0 |
| IL_003a | `stloc.3` | IL_003b | S1 | S0 | S0 |
| IL_003b | `ldloc.0` | IL_003c | S0 | S1 | S0 |
| IL_003c | `brfalse.s` | IL_003e, IL_0042 | S1 | S0 | S0 |
| IL_003e | `ldloc.3` | IL_003f | S0 | S1 | S0 |
| IL_003f | `ldc.i4.3` | IL_0040 | S1 | S2 | S1 |
| IL_0040 | `add` | IL_0041 | S2 | S1 | S0 |
| IL_0041 | `stloc.3` | IL_0042 | S1 | S0 | S0 |
| IL_0042 | `ldloc.s` | IL_0044 | S0 | S1 | S0 |
| IL_0044 | `brtrue.s` | IL_0046, IL_0049 | S1 | S0 | S0 |
| IL_0046 | `ldc.i4.0` | IL_0047 | S0 | S1 | S0 |
| IL_0047 | `br.s` | IL_004e | S1 | S1 | S1 |
| IL_0049 | `ldc.i4` | IL_004e | S0 | S1 | S0 |
| IL_004e | `ldloc.2` | IL_004f | S1 | S2 | S1 |
| IL_004f | `add` | IL_0050 | S2 | S1 | S0 |
| IL_0050 | `ldloc.3` | IL_0051 | S1 | S2 | S1 |
| IL_0051 | `add` | IL_0052 | S2 | S1 | S0 |
| IL_0052 | `ldsfld` | IL_0057 | S1 | S2 | S1 |
| IL_0057 | `ldc.i4.s` | IL_0059 | S2 | S3 | S2 |
| IL_0059 | `mul` | IL_005a | S3 | S2 | S1 |
| IL_005a | `add` | IL_005b | S2 | S1 | S0 |
| IL_005b | `ret` |  | S1 | S0 | S0 |

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
| IL_0000 | `nop` | IL_0001 | S0 | S0 | S0 |
| IL_0001 | `ldc.i4.0` | IL_0002 | S0 | S1 | S0 |
| IL_0002 | `stsfld` | IL_0007 | S1 | S0 | S0 |
| IL_0007 | `ldarg.0` | IL_0008 | S0 | S1 | S0 |
| IL_0008 | `ldc.i4.1` | IL_0009 | S1 | S2 | S1 |
| IL_0009 | `and` | IL_000a | S2 | S1 | S0 |
| IL_000a | `ldc.i4.0` | IL_000b | S1 | S2 | S1 |
| IL_000b | `cgt.un` | IL_000d | S2 | S1 | S0 |
| IL_000d | `stloc.0` | IL_000e | S1 | S0 | S0 |
| IL_000e | `ldarg.0` | IL_000f | S0 | S1 | S0 |
| IL_000f | `stloc.1` | IL_0010 | S1 | S0 | S0 |
| IL_0010 | `ldc.i4.2` | IL_0011 | S0 | S1 | S0 |
| IL_0011 | `stloc.2` | IL_0012 | S1 | S0 | S0 |
| IL_0012 | `ldc.i4.0` | IL_0013 | S0 | S1 | S0 |
| IL_0013 | `stloc.3` | IL_0014 | S1 | S0 | S0 |
| IL_0014 | `ldc.i4.1` | IL_0015 | S0 | S1 | S0 |
| IL_0015 | `stloc.s` | IL_0017 | S1 | S0 | S0 |
| IL_0017 | `ldloc.0` | IL_0018 | S0 | S1 | S0 |
| IL_0018 | `brtrue.s` | IL_001a, IL_0023 | S1 | S0 | S0 |
| IL_001a | `ldloc.1` | IL_001b | S0 | S1 | S0 |
| IL_001b | `ldloc.2` | IL_001c | S1 | S2 | S1 |
| IL_001c | `call` | IL_0021 | S2 | S1 | S0 |
| IL_0021 | `br.s` | IL_0024 | S1 | S1 | S1 |
| IL_0023 | `ldc.i4.0` | IL_0024 | S0 | S1 | S0 |
| IL_0024 | `stloc.s` | IL_0026 | S1 | S0 | S0 |
| IL_0026 | `ldloc.s` | IL_0028 | S0 | S1 | S0 |
| IL_0028 | `brfalse.s` | IL_002a, IL_002f | S1 | S0 | S0 |
| IL_002a | `ldc.i4.0` | IL_002b | S0 | S1 | S0 |
| IL_002b | `stloc.s` | IL_002d | S1 | S0 | S0 |
| IL_002d | `br.s` | IL_0045 | S0 | S0 | S0 |
| IL_002f | `nop` | IL_0030 | S0 | S0 | S0 |
| IL_0030 | `ldloc.1` | IL_0031 | S0 | S1 | S0 |
| IL_0031 | `ldloc.2` | IL_0032 | S1 | S2 | S1 |
| IL_0032 | `blt.s` | IL_0034, IL_0037 | S2 | S0 | S0 |
| IL_0034 | `ldloc.2` | IL_0035 | S0 | S1 | S0 |
| IL_0035 | `br.s` | IL_0038 | S1 | S1 | S1 |
| IL_0037 | `ldloc.1` | IL_0038 | S0 | S1 | S0 |
| IL_0038 | `stloc.2` | IL_0039 | S1 | S0 | S0 |
| IL_0039 | `ldloc.1` | IL_003a | S0 | S1 | S0 |
| IL_003a | `ldloc.0` | IL_003b | S1 | S2 | S1 |
| IL_003b | `brtrue.s` | IL_003d, IL_0040 | S2 | S1 | S1 |
| IL_003d | `ldc.i4.0` | IL_003e | S1 | S2 | S1 |
| IL_003e | `br.s` | IL_0042 | S2 | S2 | S2 |
| IL_0040 | `ldc.i4.s` | IL_0042 | S1 | S2 | S1 |
| IL_0042 | `add` | IL_0043 | S2 | S1 | S0 |
| IL_0043 | `stloc.3` | IL_0044 | S1 | S0 | S0 |
| IL_0044 | `nop` | IL_0045 | S0 | S0 | S0 |
| IL_0045 | `ldloc.0` | IL_0046 | S0 | S1 | S0 |
| IL_0046 | `stloc.s` | IL_0048 | S1 | S0 | S0 |
| IL_0048 | `ldloc.s` | IL_004a | S0 | S1 | S0 |
| IL_004a | `brfalse.s` | IL_004c, IL_0050 | S1 | S0 | S0 |
| IL_004c | `ldloc.3` | IL_004d | S0 | S1 | S0 |
| IL_004d | `ldc.i4.3` | IL_004e | S1 | S2 | S1 |
| IL_004e | `add` | IL_004f | S2 | S1 | S0 |
| IL_004f | `stloc.3` | IL_0050 | S1 | S0 | S0 |
| IL_0050 | `ldloc.s` | IL_0052 | S0 | S1 | S0 |
| IL_0052 | `brtrue.s` | IL_0054, IL_0057 | S1 | S0 | S0 |
| IL_0054 | `ldc.i4.0` | IL_0055 | S0 | S1 | S0 |
| IL_0055 | `br.s` | IL_005c | S1 | S1 | S1 |
| IL_0057 | `ldc.i4` | IL_005c | S0 | S1 | S0 |
| IL_005c | `ldloc.2` | IL_005d | S1 | S2 | S1 |
| IL_005d | `add` | IL_005e | S2 | S1 | S0 |
| IL_005e | `ldloc.3` | IL_005f | S1 | S2 | S1 |
| IL_005f | `add` | IL_0060 | S2 | S1 | S0 |
| IL_0060 | `ldsfld` | IL_0065 | S1 | S2 | S1 |
| IL_0065 | `ldc.i4.s` | IL_0067 | S2 | S3 | S2 |
| IL_0067 | `mul` | IL_0068 | S3 | S2 | S1 |
| IL_0068 | `add` | IL_0069 | S2 | S1 | S0 |
| IL_0069 | `stloc.s` | IL_006b | S1 | S0 | S0 |
| IL_006b | `br.s` | IL_006d | S0 | S0 | S0 |
| IL_006d | `ldloc.s` | IL_006f | S0 | S1 | S0 |
| IL_006f | `ret` |  | S1 | S0 | S0 |

### Stack tree

| State | Parent | Top type | Depth | Children |
| --- | --- | --- | --- | --- |
| S0 | — | empty | 0 | S1 |
| S1 | S0 | Int32 | 1 | S2 |
| S2 | S1 | Int32 | 2 | S3 |
| S3 | S2 | Int32 | 3 |  |
