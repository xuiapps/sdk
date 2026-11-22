# Assembly `XuiNet.ControlFlow`

Source: [ControlFlow.cs](Fixtures/Assemblies/ControlFlow.cs)

- Version: `2.0.0.0`
- Culture: `neutral`
- Flags: `0`
- Hash algorithm: `Sha1`
- Public key: `(empty)`

## Module `XuiNet.ControlFlow.dll`

- MVID: `<normalized>`
- Layout target: `WASM32`
- Entry point: `none`
- Declarations: `5 types, 2 fields, 10 methods`

### Assembly references


#### `System.Private.CoreLib` [0x23000001]

- Version: `10.0.0.0`
- Culture: `neutral`
- Flags: `0`
- Public key or token: `7C EC 85 D7 BE A7 79 8E`
- Hash: `(empty)`

#### `System.Console` [0x23000002]

- Version: `10.0.0.0`
- Culture: `neutral`
- Flags: `0`
- Public key or token: `B0 3F 5F 7F 11 D5 0A 3A`
- Hash: `(empty)`

### Type references

- 0x01000001: `System.Runtime.CompilerServices.CompilationRelaxationsAttribute; scope: System.Private.CoreLib [0x23000001]`
- 0x01000002: `System.Runtime.CompilerServices.RuntimeCompatibilityAttribute; scope: System.Private.CoreLib [0x23000001]`
- 0x01000003: `System.Diagnostics.DebuggableAttribute; scope: System.Private.CoreLib [0x23000001]`
- 0x01000004: `DebuggingModes; scope: System.Diagnostics.DebuggableAttribute [0x01000003]`
- 0x01000005: `System.Runtime.CompilerServices.RefSafetyRulesAttribute; scope: System.Private.CoreLib [0x23000001]`
- 0x01000006: `System.Runtime.CompilerServices.IsReadOnlyAttribute; scope: System.Private.CoreLib [0x23000001]`
- 0x01000007: `System.ValueType; scope: System.Private.CoreLib [0x23000001]`
- 0x01000008: `System.Object; scope: System.Private.CoreLib [0x23000001]`
- 0x01000009: `System.DivideByZeroException; scope: System.Private.CoreLib [0x23000001]`
- 0x0100000a: `System.Console; scope: System.Console [0x23000002]`

### Member references

- 0x0a000001: `Method .ctor; parent: System.Runtime.CompilerServices.CompilationRelaxationsAttribute [0x01000001]; signature: 20 01 01 08`
- 0x0a000002: `Method .ctor; parent: System.Runtime.CompilerServices.RuntimeCompatibilityAttribute [0x01000002]; signature: 20 00 01`
- 0x0a000003: `Method .ctor; parent: System.Diagnostics.DebuggableAttribute [0x01000003]; signature: 20 01 01 11 11`
- 0x0a000004: `Method .ctor; parent: System.Runtime.CompilerServices.RefSafetyRulesAttribute [0x01000005]; signature: 20 01 01 08`
- 0x0a000005: `Method .ctor; parent: System.Runtime.CompilerServices.IsReadOnlyAttribute [0x01000006]; signature: 20 00 01`
- 0x0a000006: `Method WriteLine; parent: System.Console [0x0100000a]; signature: 00 01 01 0E`
- 0x0a000007: `Method .ctor; parent: System.Object [0x01000008]; signature: 20 00 01`

### Module type `<Module>` [0x02000001]

- Namespace: `(empty)`
- Attributes: `NotPublic`
- Base type: `none`
- Is interface: `false`

### Interface `Samples.ControlFlow.IClassifier` [0x02000002]

- Namespace: `Samples.ControlFlow`
- Attributes: `Public, ClassSemanticsMask, Abstract, BeforeFieldInit`
- Base type: `none`
- Is interface: `true`

#### Method `Classify` [0x06000001]

- Declaring type: `Samples.ControlFlow.IClassifier [0x02000002]`
- Attributes: `Public, Virtual, HideBySig, VtableLayoutMask, Abstract`
- Implementation: `IL`
- Signature: `20 01 0E 08`
- Body: `none`

### Struct `Samples.ControlFlow.Range` [0x02000003]

- Namespace: `Samples.ControlFlow`
- Attributes: `Public, SequentialLayout, Sealed, BeforeFieldInit`
- Base type: `System.ValueType [0x01000007]`
- Is interface: `false`
- Storage classification: `LinearMemory`
- Alignment: `4`
- Size: `8`

#### Field `Min` [0x04000001]

- Declaring type: `Samples.ControlFlow.Range [0x02000003]`
- Attributes: `Public, InitOnly`
- Signature: `06 08`
- Offset: `0`
- Size: `4`

#### Field `Max` [0x04000002]

- Declaring type: `Samples.ControlFlow.Range [0x02000003]`
- Attributes: `Public, InitOnly`
- Signature: `06 08`
- Offset: `4`
- Size: `4`

#### Method `.ctor` [0x06000002]

- Declaring type: `Samples.ControlFlow.Range [0x02000003]`
- Attributes: `Public, HideBySig, SpecialName, RTSpecialName`
- Implementation: `IL`
- Signature: `20 02 01 08 08`
- RVA: `0x00002050`
- Max stack: `8`
- Init locals: `false`
- Local signature: `(empty)`

```il
IL_0000: ldarg.0
IL_0001: ldarg.1
IL_0002: stfld Samples.ControlFlow.Range.Min [0x04000001]
IL_0007: ldarg.0
IL_0008: ldarg.2
IL_0009: stfld Samples.ControlFlow.Range.Max [0x04000002]
IL_000e: ret
```

### Class `Samples.ControlFlow.Classifier` [0x02000004]

- Namespace: `Samples.ControlFlow`
- Attributes: `Public, Sealed, BeforeFieldInit`
- Base type: `System.Object [0x01000008]`
- Is interface: `false`
- Interface [0x09000001]: `Samples.ControlFlow.IClassifier [0x02000002]`

#### Method `Classify` [0x06000003]

- Declaring type: `Samples.ControlFlow.Classifier [0x02000004]`
- Attributes: `Public, Final, Virtual, HideBySig, VtableLayoutMask`
- Implementation: `IL`
- Signature: `20 01 0E 08`
- RVA: `0x00002060`
- Max stack: `1`
- Init locals: `true`
- Local signature: `07 01 0E`

```il
IL_0000: ldarg.1
IL_0001: switch (IL_001c, IL_0024, IL_002c, IL_0034, IL_003c)
IL_001a: br.s IL_0044
IL_001c: ldstr "zero" [0x70000001]
IL_0021: stloc.0
IL_0022: leave.s IL_0057
IL_0024: ldstr "one" [0x7000000b]
IL_0029: stloc.0
IL_002a: leave.s IL_0057
IL_002c: ldstr "two" [0x70000013]
IL_0031: stloc.0
IL_0032: leave.s IL_0057
IL_0034: ldstr "three" [0x7000001b]
IL_0039: stloc.0
IL_003a: leave.s IL_0057
IL_003c: ldstr "four" [0x70000027]
IL_0041: stloc.0
IL_0042: leave.s IL_0057
IL_0044: ldstr "other" [0x70000031]
IL_0049: stloc.0
IL_004a: leave.s IL_0057
IL_004c: ldstr "classified" [0x7000003d]
IL_0051: call System.Console [0x0100000a].WriteLine [0x0a000006]
IL_0056: endfinally
IL_0057: ldloc.0
IL_0058: ret
```

Exception regions:

- Finally: `try [IL_0000, IL_004c); handler [IL_004c, IL_0057)`

#### Method `Sum` [0x06000004]

- Declaring type: `Samples.ControlFlow.Classifier [0x02000004]`
- Attributes: `Public, HideBySig`
- Implementation: `IL`
- Signature: `20 01 08 08`
- RVA: `0x000020d8`
- Max stack: `2`
- Init locals: `true`
- Local signature: `07 02 08 08`

```il
IL_0000: ldc.i4.0
IL_0001: stloc.0
IL_0002: ldc.i4.0
IL_0003: stloc.1
IL_0004: br.s IL_000e
IL_0006: ldloc.0
IL_0007: ldloc.1
IL_0008: add
IL_0009: stloc.0
IL_000a: ldloc.1
IL_000b: ldc.i4.1
IL_000c: add
IL_000d: stloc.1
IL_000e: ldloc.1
IL_000f: ldarg.1
IL_0010: blt.s IL_0006
IL_0012: ldloc.0
IL_0013: ret
```

#### Method `Divide` [0x06000005]

- Declaring type: `Samples.ControlFlow.Classifier [0x02000004]`
- Attributes: `Public, HideBySig`
- Implementation: `IL`
- Signature: `20 01 08 08`
- RVA: `0x000020f8`
- Max stack: `2`
- Init locals: `true`
- Local signature: `07 01 08`

```il
IL_0000: ldc.i4.s 100
IL_0002: ldarg.1
IL_0003: div
IL_0004: stloc.0
IL_0005: leave.s IL_000c
IL_0007: pop
IL_0008: ldc.i4.m1
IL_0009: stloc.0
IL_000a: leave.s IL_000c
IL_000c: ldloc.0
IL_000d: ret
```

Exception regions:

- Catch: `try [IL_0000, IL_0007); handler [IL_0007, IL_000c); catch: System.DivideByZeroException [0x01000009]`

#### Method `.ctor` [0x06000006]

- Declaring type: `Samples.ControlFlow.Classifier [0x02000004]`
- Attributes: `Public, HideBySig, SpecialName, RTSpecialName`
- Implementation: `IL`
- Signature: `20 00 01`
- RVA: `0x00002124`
- Max stack: `8`
- Init locals: `false`
- Local signature: `(empty)`

```il
IL_0000: ldarg.0
IL_0001: call System.Object [0x01000008]..ctor [0x0a000007]
IL_0006: ret
```

### Class `Samples.ControlFlow.Numbers` [0x02000005]

- Namespace: `Samples.ControlFlow`
- Attributes: `Public, Abstract, Sealed, BeforeFieldInit`
- Base type: `System.Object [0x01000008]`
- Is interface: `false`

#### Method `Negative` [0x06000007]

- Declaring type: `Samples.ControlFlow.Numbers [0x02000005]`
- Attributes: `Public, Static, HideBySig`
- Implementation: `IL`
- Signature: `00 00 08`
- RVA: `0x0000212c`
- Max stack: `8`
- Init locals: `false`
- Local signature: `(empty)`

```il
IL_0000: ldc.i4 -123456
IL_0005: ret
```

#### Method `Large` [0x06000008]

- Declaring type: `Samples.ControlFlow.Numbers [0x02000005]`
- Attributes: `Public, Static, HideBySig`
- Implementation: `IL`
- Signature: `00 00 0A`
- RVA: `0x00002133`
- Max stack: `8`
- Init locals: `false`
- Local signature: `(empty)`

```il
IL_0000: ldc.i8 1234605616436508552
IL_0009: ret
```

#### Method `Single` [0x06000009]

- Declaring type: `Samples.ControlFlow.Numbers [0x02000005]`
- Attributes: `Public, Static, HideBySig`
- Implementation: `IL`
- Signature: `00 00 0C`
- RVA: `0x0000213e`
- Max stack: `8`
- Init locals: `false`
- Local signature: `(empty)`

```il
IL_0000: ldc.r4 1.5
IL_0005: ret
```

#### Method `Double` [0x0600000a]

- Declaring type: `Samples.ControlFlow.Numbers [0x02000005]`
- Attributes: `Public, Static, HideBySig`
- Implementation: `IL`
- Signature: `00 00 0D`
- RVA: `0x00002145`
- Max stack: `8`
- Init locals: `false`
- Local signature: `(empty)`

```il
IL_0000: ldc.r8 -1.25
IL_0009: ret
```

### String literals

- 0x70000001: `"zero"`
- 0x7000000b: `"one"`
- 0x70000013: `"two"`
- 0x7000001b: `"three"`
- 0x70000027: `"four"`
- 0x70000031: `"other"`
- 0x7000003d: `"classified"`
