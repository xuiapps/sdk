# SharedElseIfChain instruction graph

Source: [SharedElseIfChain.cs](Fixtures/CodeFlow/SharedElseIfChain.cs)

```csharp
namespace Samples.CodeFlow;

public static class SharedElseIfChain
{
    private static int checks;
    private static bool Check(int value) { checks++; return (value & 1) != 0; }

    public static int Run(int value)
    {
        checks = 0;
        int result;
        if (value > 10 && Check(value)) result = 1;
        else if (value > 8 && Check(value + 1)) result = 2;
        else if (value > 6 && Check(value)) result = 3;
        else if (value > 4 && Check(value + 1)) result = 4;
        else if (value > 2 && Check(value)) result = 5;
        else if (value > 0 && Check(value + 1)) result = 6;
        else result = value < 0 ? 7 : 8;
        int other = 12;
        ref int selected = ref (value < 0 ? ref result : ref other);
        selected += 1;
        int extra = value switch { 0 => 10, 1 => 20, 2 => 30, 3 => 40, 4 => 50, _ => 60 };
        return checks * 100 + (result > 4 ? result : result + 10) + other + extra;
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
| IL_0007 | `ldc.i4.s` | IL_0009 | S1 | S2 | S1 |
| IL_0009 | `ble.s` | IL_000b, IL_0017 | S2 | S0 | S0 |
| IL_000b | `ldarg.0` | IL_000c | S0 | S1 | S0 |
| IL_000c | `call` | IL_0011 | S1 | S1 | S0 |
| IL_0011 | `brfalse.s` | IL_0013, IL_0017 | S1 | S0 | S0 |
| IL_0013 | `ldc.i4.1` | IL_0014 | S0 | S1 | S0 |
| IL_0014 | `stloc.0` | IL_0015 | S1 | S0 | S0 |
| IL_0015 | `br.s` | IL_0076 | S0 | S0 | S0 |
| IL_0017 | `ldarg.0` | IL_0018 | S0 | S1 | S0 |
| IL_0018 | `ldc.i4.8` | IL_0019 | S1 | S2 | S1 |
| IL_0019 | `ble.s` | IL_001b, IL_0029 | S2 | S0 | S0 |
| IL_001b | `ldarg.0` | IL_001c | S0 | S1 | S0 |
| IL_001c | `ldc.i4.1` | IL_001d | S1 | S2 | S1 |
| IL_001d | `add` | IL_001e | S2 | S1 | S0 |
| IL_001e | `call` | IL_0023 | S1 | S1 | S0 |
| IL_0023 | `brfalse.s` | IL_0025, IL_0029 | S1 | S0 | S0 |
| IL_0025 | `ldc.i4.2` | IL_0026 | S0 | S1 | S0 |
| IL_0026 | `stloc.0` | IL_0027 | S1 | S0 | S0 |
| IL_0027 | `br.s` | IL_0076 | S0 | S0 | S0 |
| IL_0029 | `ldarg.0` | IL_002a | S0 | S1 | S0 |
| IL_002a | `ldc.i4.6` | IL_002b | S1 | S2 | S1 |
| IL_002b | `ble.s` | IL_002d, IL_0039 | S2 | S0 | S0 |
| IL_002d | `ldarg.0` | IL_002e | S0 | S1 | S0 |
| IL_002e | `call` | IL_0033 | S1 | S1 | S0 |
| IL_0033 | `brfalse.s` | IL_0035, IL_0039 | S1 | S0 | S0 |
| IL_0035 | `ldc.i4.3` | IL_0036 | S0 | S1 | S0 |
| IL_0036 | `stloc.0` | IL_0037 | S1 | S0 | S0 |
| IL_0037 | `br.s` | IL_0076 | S0 | S0 | S0 |
| IL_0039 | `ldarg.0` | IL_003a | S0 | S1 | S0 |
| IL_003a | `ldc.i4.4` | IL_003b | S1 | S2 | S1 |
| IL_003b | `ble.s` | IL_003d, IL_004b | S2 | S0 | S0 |
| IL_003d | `ldarg.0` | IL_003e | S0 | S1 | S0 |
| IL_003e | `ldc.i4.1` | IL_003f | S1 | S2 | S1 |
| IL_003f | `add` | IL_0040 | S2 | S1 | S0 |
| IL_0040 | `call` | IL_0045 | S1 | S1 | S0 |
| IL_0045 | `brfalse.s` | IL_0047, IL_004b | S1 | S0 | S0 |
| IL_0047 | `ldc.i4.4` | IL_0048 | S0 | S1 | S0 |
| IL_0048 | `stloc.0` | IL_0049 | S1 | S0 | S0 |
| IL_0049 | `br.s` | IL_0076 | S0 | S0 | S0 |
| IL_004b | `ldarg.0` | IL_004c | S0 | S1 | S0 |
| IL_004c | `ldc.i4.2` | IL_004d | S1 | S2 | S1 |
| IL_004d | `ble.s` | IL_004f, IL_005b | S2 | S0 | S0 |
| IL_004f | `ldarg.0` | IL_0050 | S0 | S1 | S0 |
| IL_0050 | `call` | IL_0055 | S1 | S1 | S0 |
| IL_0055 | `brfalse.s` | IL_0057, IL_005b | S1 | S0 | S0 |
| IL_0057 | `ldc.i4.5` | IL_0058 | S0 | S1 | S0 |
| IL_0058 | `stloc.0` | IL_0059 | S1 | S0 | S0 |
| IL_0059 | `br.s` | IL_0076 | S0 | S0 | S0 |
| IL_005b | `ldarg.0` | IL_005c | S0 | S1 | S0 |
| IL_005c | `ldc.i4.0` | IL_005d | S1 | S2 | S1 |
| IL_005d | `ble.s` | IL_005f, IL_006d | S2 | S0 | S0 |
| IL_005f | `ldarg.0` | IL_0060 | S0 | S1 | S0 |
| IL_0060 | `ldc.i4.1` | IL_0061 | S1 | S2 | S1 |
| IL_0061 | `add` | IL_0062 | S2 | S1 | S0 |
| IL_0062 | `call` | IL_0067 | S1 | S1 | S0 |
| IL_0067 | `brfalse.s` | IL_0069, IL_006d | S1 | S0 | S0 |
| IL_0069 | `ldc.i4.6` | IL_006a | S0 | S1 | S0 |
| IL_006a | `stloc.0` | IL_006b | S1 | S0 | S0 |
| IL_006b | `br.s` | IL_0076 | S0 | S0 | S0 |
| IL_006d | `ldarg.0` | IL_006e | S0 | S1 | S0 |
| IL_006e | `ldc.i4.0` | IL_006f | S1 | S2 | S1 |
| IL_006f | `blt.s` | IL_0071, IL_0074 | S2 | S0 | S0 |
| IL_0071 | `ldc.i4.8` | IL_0072 | S0 | S1 | S0 |
| IL_0072 | `br.s` | IL_0075 | S1 | S1 | S1 |
| IL_0074 | `ldc.i4.7` | IL_0075 | S0 | S1 | S0 |
| IL_0075 | `stloc.0` | IL_0076 | S1 | S0 | S0 |
| IL_0076 | `ldc.i4.s` | IL_0078 | S0 | S1 | S0 |
| IL_0078 | `stloc.1` | IL_0079 | S1 | S0 | S0 |
| IL_0079 | `ldarg.0` | IL_007a | S0 | S1 | S0 |
| IL_007a | `ldc.i4.0` | IL_007b | S1 | S2 | S1 |
| IL_007b | `blt.s` | IL_007d, IL_0081 | S2 | S0 | S0 |
| IL_007d | `ldloca.s` | IL_007f | S0 | S3 | S0 |
| IL_007f | `br.s` | IL_0083 | S3 | S3 | S3 |
| IL_0081 | `ldloca.s` | IL_0083 | S0 | S3 | S0 |
| IL_0083 | `dup` | IL_0084 | S3 | S4 | S0 |
| IL_0084 | `ldind.i4` | IL_0085 | S4 | S5 | S3 |
| IL_0085 | `ldc.i4.1` | IL_0086 | S5 | S6 | S5 |
| IL_0086 | `add` | IL_0087 | S6 | S5 | S3 |
| IL_0087 | `stind.i4` | IL_0088 | S5 | S0 | S0 |
| IL_0088 | `ldarg.0` | IL_0089 | S0 | S1 | S0 |
| IL_0089 | `switch` | IL_00a2, IL_00a4, IL_00a9, IL_00ae, IL_00b3, IL_00b8 | S1 | S0 | S0 |
| IL_00a2 | `br.s` | IL_00bd | S0 | S0 | S0 |
| IL_00a4 | `ldc.i4.s` | IL_00a6 | S0 | S1 | S0 |
| IL_00a6 | `stloc.3` | IL_00a7 | S1 | S0 | S0 |
| IL_00a7 | `br.s` | IL_00c0 | S0 | S0 | S0 |
| IL_00a9 | `ldc.i4.s` | IL_00ab | S0 | S1 | S0 |
| IL_00ab | `stloc.3` | IL_00ac | S1 | S0 | S0 |
| IL_00ac | `br.s` | IL_00c0 | S0 | S0 | S0 |
| IL_00ae | `ldc.i4.s` | IL_00b0 | S0 | S1 | S0 |
| IL_00b0 | `stloc.3` | IL_00b1 | S1 | S0 | S0 |
| IL_00b1 | `br.s` | IL_00c0 | S0 | S0 | S0 |
| IL_00b3 | `ldc.i4.s` | IL_00b5 | S0 | S1 | S0 |
| IL_00b5 | `stloc.3` | IL_00b6 | S1 | S0 | S0 |
| IL_00b6 | `br.s` | IL_00c0 | S0 | S0 | S0 |
| IL_00b8 | `ldc.i4.s` | IL_00ba | S0 | S1 | S0 |
| IL_00ba | `stloc.3` | IL_00bb | S1 | S0 | S0 |
| IL_00bb | `br.s` | IL_00c0 | S0 | S0 | S0 |
| IL_00bd | `ldc.i4.s` | IL_00bf | S0 | S1 | S0 |
| IL_00bf | `stloc.3` | IL_00c0 | S1 | S0 | S0 |
| IL_00c0 | `ldloc.3` | IL_00c1 | S0 | S1 | S0 |
| IL_00c1 | `stloc.2` | IL_00c2 | S1 | S0 | S0 |
| IL_00c2 | `ldsfld` | IL_00c7 | S0 | S1 | S0 |
| IL_00c7 | `ldc.i4.s` | IL_00c9 | S1 | S2 | S1 |
| IL_00c9 | `mul` | IL_00ca | S2 | S1 | S0 |
| IL_00ca | `ldloc.0` | IL_00cb | S1 | S2 | S1 |
| IL_00cb | `ldc.i4.4` | IL_00cc | S2 | S7 | S2 |
| IL_00cc | `bgt.s` | IL_00ce, IL_00d4 | S7 | S1 | S1 |
| IL_00ce | `ldloc.0` | IL_00cf | S1 | S2 | S1 |
| IL_00cf | `ldc.i4.s` | IL_00d1 | S2 | S7 | S2 |
| IL_00d1 | `add` | IL_00d2 | S7 | S2 | S1 |
| IL_00d2 | `br.s` | IL_00d5 | S2 | S2 | S2 |
| IL_00d4 | `ldloc.0` | IL_00d5 | S1 | S2 | S1 |
| IL_00d5 | `add` | IL_00d6 | S2 | S1 | S0 |
| IL_00d6 | `ldloc.1` | IL_00d7 | S1 | S2 | S1 |
| IL_00d7 | `add` | IL_00d8 | S2 | S1 | S0 |
| IL_00d8 | `ldloc.2` | IL_00d9 | S1 | S2 | S1 |
| IL_00d9 | `add` | IL_00da | S2 | S1 | S0 |
| IL_00da | `ret` |  | S1 | S0 | S0 |

### Stack tree

| State | Parent | Top type | Depth | Children |
| --- | --- | --- | --- | --- |
| S0 | — | empty | 0 | S1, S3 |
| S1 | S0 | Int32 | 1 | S2 |
| S2 | S1 | Int32 | 2 | S7 |
| S3 | S0 | Int32& | 1 | S4, S5 |
| S4 | S3 | Int32& | 2 |  |
| S5 | S3 | Int32 | 2 | S6 |
| S6 | S5 | Int32 | 3 |  |
| S7 | S2 | Int32 | 3 |  |

## Debug

### Instructions

| Offset | Instruction | Successors | Before | After | Lowest |
| --- | --- | --- | --- | --- | --- |
| IL_0000 | `nop` | IL_0001 | S0 | S0 | S0 |
| IL_0001 | `ldc.i4.0` | IL_0002 | S0 | S1 | S0 |
| IL_0002 | `stsfld` | IL_0007 | S1 | S0 | S0 |
| IL_0007 | `ldarg.0` | IL_0008 | S0 | S1 | S0 |
| IL_0008 | `ldc.i4.s` | IL_000a | S1 | S2 | S1 |
| IL_000a | `ble.s` | IL_000c, IL_0014 | S2 | S0 | S0 |
| IL_000c | `ldarg.0` | IL_000d | S0 | S1 | S0 |
| IL_000d | `call` | IL_0012 | S1 | S1 | S0 |
| IL_0012 | `br.s` | IL_0015 | S1 | S1 | S1 |
| IL_0014 | `ldc.i4.0` | IL_0015 | S0 | S1 | S0 |
| IL_0015 | `stloc.s` | IL_0017 | S1 | S0 | S0 |
| IL_0017 | `ldloc.s` | IL_0019 | S0 | S1 | S0 |
| IL_0019 | `brfalse.s` | IL_001b, IL_0022 | S1 | S0 | S0 |
| IL_001b | `ldc.i4.1` | IL_001c | S0 | S1 | S0 |
| IL_001c | `stloc.0` | IL_001d | S1 | S0 | S0 |
| IL_001d | `br` | IL_00a4 | S0 | S0 | S0 |
| IL_0022 | `ldarg.0` | IL_0023 | S0 | S1 | S0 |
| IL_0023 | `ldc.i4.8` | IL_0024 | S1 | S2 | S1 |
| IL_0024 | `ble.s` | IL_0026, IL_0030 | S2 | S0 | S0 |
| IL_0026 | `ldarg.0` | IL_0027 | S0 | S1 | S0 |
| IL_0027 | `ldc.i4.1` | IL_0028 | S1 | S2 | S1 |
| IL_0028 | `add` | IL_0029 | S2 | S1 | S0 |
| IL_0029 | `call` | IL_002e | S1 | S1 | S0 |
| IL_002e | `br.s` | IL_0031 | S1 | S1 | S1 |
| IL_0030 | `ldc.i4.0` | IL_0031 | S0 | S1 | S0 |
| IL_0031 | `stloc.s` | IL_0033 | S1 | S0 | S0 |
| IL_0033 | `ldloc.s` | IL_0035 | S0 | S1 | S0 |
| IL_0035 | `brfalse.s` | IL_0037, IL_003b | S1 | S0 | S0 |
| IL_0037 | `ldc.i4.2` | IL_0038 | S0 | S1 | S0 |
| IL_0038 | `stloc.0` | IL_0039 | S1 | S0 | S0 |
| IL_0039 | `br.s` | IL_00a4 | S0 | S0 | S0 |
| IL_003b | `ldarg.0` | IL_003c | S0 | S1 | S0 |
| IL_003c | `ldc.i4.6` | IL_003d | S1 | S2 | S1 |
| IL_003d | `ble.s` | IL_003f, IL_0047 | S2 | S0 | S0 |
| IL_003f | `ldarg.0` | IL_0040 | S0 | S1 | S0 |
| IL_0040 | `call` | IL_0045 | S1 | S1 | S0 |
| IL_0045 | `br.s` | IL_0048 | S1 | S1 | S1 |
| IL_0047 | `ldc.i4.0` | IL_0048 | S0 | S1 | S0 |
| IL_0048 | `stloc.s` | IL_004a | S1 | S0 | S0 |
| IL_004a | `ldloc.s` | IL_004c | S0 | S1 | S0 |
| IL_004c | `brfalse.s` | IL_004e, IL_0052 | S1 | S0 | S0 |
| IL_004e | `ldc.i4.3` | IL_004f | S0 | S1 | S0 |
| IL_004f | `stloc.0` | IL_0050 | S1 | S0 | S0 |
| IL_0050 | `br.s` | IL_00a4 | S0 | S0 | S0 |
| IL_0052 | `ldarg.0` | IL_0053 | S0 | S1 | S0 |
| IL_0053 | `ldc.i4.4` | IL_0054 | S1 | S2 | S1 |
| IL_0054 | `ble.s` | IL_0056, IL_0060 | S2 | S0 | S0 |
| IL_0056 | `ldarg.0` | IL_0057 | S0 | S1 | S0 |
| IL_0057 | `ldc.i4.1` | IL_0058 | S1 | S2 | S1 |
| IL_0058 | `add` | IL_0059 | S2 | S1 | S0 |
| IL_0059 | `call` | IL_005e | S1 | S1 | S0 |
| IL_005e | `br.s` | IL_0061 | S1 | S1 | S1 |
| IL_0060 | `ldc.i4.0` | IL_0061 | S0 | S1 | S0 |
| IL_0061 | `stloc.s` | IL_0063 | S1 | S0 | S0 |
| IL_0063 | `ldloc.s` | IL_0065 | S0 | S1 | S0 |
| IL_0065 | `brfalse.s` | IL_0067, IL_006b | S1 | S0 | S0 |
| IL_0067 | `ldc.i4.4` | IL_0068 | S0 | S1 | S0 |
| IL_0068 | `stloc.0` | IL_0069 | S1 | S0 | S0 |
| IL_0069 | `br.s` | IL_00a4 | S0 | S0 | S0 |
| IL_006b | `ldarg.0` | IL_006c | S0 | S1 | S0 |
| IL_006c | `ldc.i4.2` | IL_006d | S1 | S2 | S1 |
| IL_006d | `ble.s` | IL_006f, IL_0077 | S2 | S0 | S0 |
| IL_006f | `ldarg.0` | IL_0070 | S0 | S1 | S0 |
| IL_0070 | `call` | IL_0075 | S1 | S1 | S0 |
| IL_0075 | `br.s` | IL_0078 | S1 | S1 | S1 |
| IL_0077 | `ldc.i4.0` | IL_0078 | S0 | S1 | S0 |
| IL_0078 | `stloc.s` | IL_007a | S1 | S0 | S0 |
| IL_007a | `ldloc.s` | IL_007c | S0 | S1 | S0 |
| IL_007c | `brfalse.s` | IL_007e, IL_0082 | S1 | S0 | S0 |
| IL_007e | `ldc.i4.5` | IL_007f | S0 | S1 | S0 |
| IL_007f | `stloc.0` | IL_0080 | S1 | S0 | S0 |
| IL_0080 | `br.s` | IL_00a4 | S0 | S0 | S0 |
| IL_0082 | `ldarg.0` | IL_0083 | S0 | S1 | S0 |
| IL_0083 | `ldc.i4.0` | IL_0084 | S1 | S2 | S1 |
| IL_0084 | `ble.s` | IL_0086, IL_0090 | S2 | S0 | S0 |
| IL_0086 | `ldarg.0` | IL_0087 | S0 | S1 | S0 |
| IL_0087 | `ldc.i4.1` | IL_0088 | S1 | S2 | S1 |
| IL_0088 | `add` | IL_0089 | S2 | S1 | S0 |
| IL_0089 | `call` | IL_008e | S1 | S1 | S0 |
| IL_008e | `br.s` | IL_0091 | S1 | S1 | S1 |
| IL_0090 | `ldc.i4.0` | IL_0091 | S0 | S1 | S0 |
| IL_0091 | `stloc.s` | IL_0093 | S1 | S0 | S0 |
| IL_0093 | `ldloc.s` | IL_0095 | S0 | S1 | S0 |
| IL_0095 | `brfalse.s` | IL_0097, IL_009b | S1 | S0 | S0 |
| IL_0097 | `ldc.i4.6` | IL_0098 | S0 | S1 | S0 |
| IL_0098 | `stloc.0` | IL_0099 | S1 | S0 | S0 |
| IL_0099 | `br.s` | IL_00a4 | S0 | S0 | S0 |
| IL_009b | `ldarg.0` | IL_009c | S0 | S1 | S0 |
| IL_009c | `ldc.i4.0` | IL_009d | S1 | S2 | S1 |
| IL_009d | `blt.s` | IL_009f, IL_00a2 | S2 | S0 | S0 |
| IL_009f | `ldc.i4.8` | IL_00a0 | S0 | S1 | S0 |
| IL_00a0 | `br.s` | IL_00a3 | S1 | S1 | S1 |
| IL_00a2 | `ldc.i4.7` | IL_00a3 | S0 | S1 | S0 |
| IL_00a3 | `stloc.0` | IL_00a4 | S1 | S0 | S0 |
| IL_00a4 | `ldc.i4.s` | IL_00a6 | S0 | S1 | S0 |
| IL_00a6 | `stloc.1` | IL_00a7 | S1 | S0 | S0 |
| IL_00a7 | `ldarg.0` | IL_00a8 | S0 | S1 | S0 |
| IL_00a8 | `ldc.i4.0` | IL_00a9 | S1 | S2 | S1 |
| IL_00a9 | `blt.s` | IL_00ab, IL_00af | S2 | S0 | S0 |
| IL_00ab | `ldloca.s` | IL_00ad | S0 | S3 | S0 |
| IL_00ad | `br.s` | IL_00b1 | S3 | S3 | S3 |
| IL_00af | `ldloca.s` | IL_00b1 | S0 | S3 | S0 |
| IL_00b1 | `stloc.2` | IL_00b2 | S3 | S0 | S0 |
| IL_00b2 | `ldloc.2` | IL_00b3 | S0 | S3 | S0 |
| IL_00b3 | `ldloc.2` | IL_00b4 | S3 | S4 | S3 |
| IL_00b4 | `ldind.i4` | IL_00b5 | S4 | S5 | S3 |
| IL_00b5 | `ldc.i4.1` | IL_00b6 | S5 | S6 | S5 |
| IL_00b6 | `add` | IL_00b7 | S6 | S5 | S3 |
| IL_00b7 | `stind.i4` | IL_00b8 | S5 | S0 | S0 |
| IL_00b8 | `ldarg.0` | IL_00b9 | S0 | S1 | S0 |
| IL_00b9 | `switch` | IL_00d2, IL_00d4, IL_00da, IL_00e0, IL_00e6, IL_00ec | S1 | S0 | S0 |
| IL_00d2 | `br.s` | IL_00f2 | S0 | S0 | S0 |
| IL_00d4 | `ldc.i4.s` | IL_00d6 | S0 | S1 | S0 |
| IL_00d6 | `stloc.s` | IL_00d8 | S1 | S0 | S0 |
| IL_00d8 | `br.s` | IL_00f8 | S0 | S0 | S0 |
| IL_00da | `ldc.i4.s` | IL_00dc | S0 | S1 | S0 |
| IL_00dc | `stloc.s` | IL_00de | S1 | S0 | S0 |
| IL_00de | `br.s` | IL_00f8 | S0 | S0 | S0 |
| IL_00e0 | `ldc.i4.s` | IL_00e2 | S0 | S1 | S0 |
| IL_00e2 | `stloc.s` | IL_00e4 | S1 | S0 | S0 |
| IL_00e4 | `br.s` | IL_00f8 | S0 | S0 | S0 |
| IL_00e6 | `ldc.i4.s` | IL_00e8 | S0 | S1 | S0 |
| IL_00e8 | `stloc.s` | IL_00ea | S1 | S0 | S0 |
| IL_00ea | `br.s` | IL_00f8 | S0 | S0 | S0 |
| IL_00ec | `ldc.i4.s` | IL_00ee | S0 | S1 | S0 |
| IL_00ee | `stloc.s` | IL_00f0 | S1 | S0 | S0 |
| IL_00f0 | `br.s` | IL_00f8 | S0 | S0 | S0 |
| IL_00f2 | `ldc.i4.s` | IL_00f4 | S0 | S1 | S0 |
| IL_00f4 | `stloc.s` | IL_00f6 | S1 | S0 | S0 |
| IL_00f6 | `br.s` | IL_00f8 | S0 | S0 | S0 |
| IL_00f8 | `ldloc.s` | IL_00fa | S0 | S1 | S0 |
| IL_00fa | `stloc.3` | IL_00fb | S1 | S0 | S0 |
| IL_00fb | `ldsfld` | IL_0100 | S0 | S1 | S0 |
| IL_0100 | `ldc.i4.s` | IL_0102 | S1 | S2 | S1 |
| IL_0102 | `mul` | IL_0103 | S2 | S1 | S0 |
| IL_0103 | `ldloc.0` | IL_0104 | S1 | S2 | S1 |
| IL_0104 | `ldc.i4.4` | IL_0105 | S2 | S7 | S2 |
| IL_0105 | `bgt.s` | IL_0107, IL_010d | S7 | S1 | S1 |
| IL_0107 | `ldloc.0` | IL_0108 | S1 | S2 | S1 |
| IL_0108 | `ldc.i4.s` | IL_010a | S2 | S7 | S2 |
| IL_010a | `add` | IL_010b | S7 | S2 | S1 |
| IL_010b | `br.s` | IL_010e | S2 | S2 | S2 |
| IL_010d | `ldloc.0` | IL_010e | S1 | S2 | S1 |
| IL_010e | `add` | IL_010f | S2 | S1 | S0 |
| IL_010f | `ldloc.1` | IL_0110 | S1 | S2 | S1 |
| IL_0110 | `add` | IL_0111 | S2 | S1 | S0 |
| IL_0111 | `ldloc.3` | IL_0112 | S1 | S2 | S1 |
| IL_0112 | `add` | IL_0113 | S2 | S1 | S0 |
| IL_0113 | `stloc.s` | IL_0115 | S1 | S0 | S0 |
| IL_0115 | `br.s` | IL_0117 | S0 | S0 | S0 |
| IL_0117 | `ldloc.s` | IL_0119 | S0 | S1 | S0 |
| IL_0119 | `ret` |  | S1 | S0 | S0 |

### Stack tree

| State | Parent | Top type | Depth | Children |
| --- | --- | --- | --- | --- |
| S0 | — | empty | 0 | S1, S3 |
| S1 | S0 | Int32 | 1 | S2 |
| S2 | S1 | Int32 | 2 | S7 |
| S3 | S0 | Int32& | 1 | S4, S5 |
| S4 | S3 | Int32& | 2 |  |
| S5 | S3 | Int32 | 2 | S6 |
| S6 | S5 | Int32 | 3 |  |
| S7 | S2 | Int32 | 3 |  |
