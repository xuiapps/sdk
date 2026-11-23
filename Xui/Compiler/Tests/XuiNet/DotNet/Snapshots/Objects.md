# Assembly `XuiNet.Objects`

Source: [Objects.cs](Fixtures/Gc/Objects.cs)

- Version: `0.0.0.0`
- Culture: `neutral`
- Flags: `0`
- Hash algorithm: `Sha1`
- Public key: `(empty)`

## Module `XuiNet.Objects.exe`

- MVID: `<normalized>`
- Layout target: `WASM32`
- Entry point: `Program.Main [0x06000002]`
- Declarations: `6 types, 8 fields, 2 methods`

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
- 0x01000006: `System.ValueType; scope: System.Private.CoreLib [0x23000001]`
- 0x01000007: `System.Object; scope: System.Private.CoreLib [0x23000001]`
- 0x01000008: `System.Console; scope: System.Console [0x23000002]`

### Member references

- 0x0a000001: `Method .ctor; parent: System.Runtime.CompilerServices.CompilationRelaxationsAttribute [0x01000001]; signature: 20 01 01 08`
- 0x0a000002: `Method .ctor; parent: System.Runtime.CompilerServices.RuntimeCompatibilityAttribute [0x01000002]; signature: 20 00 01`
- 0x0a000003: `Method .ctor; parent: System.Diagnostics.DebuggableAttribute [0x01000003]; signature: 20 01 01 11 11`
- 0x0a000004: `Method .ctor; parent: System.Runtime.CompilerServices.RefSafetyRulesAttribute [0x01000005]; signature: 20 01 01 08`
- 0x0a000005: `Method .ctor; parent: System.Object [0x01000007]; signature: 20 00 01`
- 0x0a000006: `Method WriteLine; parent: System.Console [0x01000008]; signature: 00 01 01 08`
- 0x0a000007: `Method WriteLine; parent: System.Console [0x01000008]; signature: 00 01 01 0E`

### Module type `<Module>` [0x02000001]

- Namespace: `(empty)`
- Attributes: `NotPublic`
- Base type: `none`
- Is interface: `false`

### Struct `Point` [0x02000002]

- Namespace: `(empty)`
- Attributes: `Public, SequentialLayout, Sealed, BeforeFieldInit`
- Base type: `System.ValueType [0x01000006]`
- Is interface: `false`
- Storage classification: `LinearMemory`
- Alignment: `4`
- Size: `8`

#### Field `X` [0x04000001]

- Declaring type: `Point [0x02000002]`
- Attributes: `Public`
- Signature: `06 08`
- Offset: `0`
- Size: `4`

#### Field `Y` [0x04000002]

- Declaring type: `Point [0x02000002]`
- Attributes: `Public`
- Signature: `06 08`
- Offset: `4`
- Size: `4`

### Struct `Label` [0x02000003]

- Namespace: `(empty)`
- Attributes: `Public, SequentialLayout, Sealed, BeforeFieldInit`
- Base type: `System.ValueType [0x01000006]`
- Is interface: `false`
- Storage classification: `ContainsManagedReferences`
- Layout: `Unsupported: Cannot lay out Label.Text: Field storage type String is not supported.`

#### Field `Text` [0x04000003]

- Declaring type: `Label [0x02000003]`
- Attributes: `Public`
- Signature: `06 0E`

#### Field `Number` [0x04000004]

- Declaring type: `Label [0x02000003]`
- Attributes: `Public`
- Signature: `06 08`

### Struct `Caption` [0x02000004]

- Namespace: `(empty)`
- Attributes: `Public, SequentialLayout, Sealed, BeforeFieldInit`
- Base type: `System.ValueType [0x01000006]`
- Is interface: `false`
- Storage classification: `ContainsManagedReferences`
- Layout: `Unsupported: Cannot lay out Caption.Value: Cannot lay out Label.Text: Field storage type String is not supported.`

#### Field `Value` [0x04000005]

- Declaring type: `Caption [0x02000004]`
- Attributes: `Public`
- Signature: `06 11 0C`

#### Field `Target` [0x04000006]

- Declaring type: `Caption [0x02000004]`
- Attributes: `Public`
- Signature: `06 12 14`

### Class `Widget` [0x02000005]

- Namespace: `(empty)`
- Attributes: `Public, BeforeFieldInit`
- Base type: `System.Object [0x01000007]`
- Is interface: `false`

#### Field `Position` [0x04000007]

- Declaring type: `Widget [0x02000005]`
- Attributes: `Public`
- Signature: `06 11 08`

#### Field `Caption` [0x04000008]

- Declaring type: `Widget [0x02000005]`
- Attributes: `Public`
- Signature: `06 11 10`

#### Method `.ctor` [0x06000001]

- Declaring type: `Widget [0x02000005]`
- Attributes: `Public, HideBySig, SpecialName, RTSpecialName`
- Implementation: `IL`
- Signature: `20 02 01 08 0E`
- RVA: `0x00002050`
- Max stack: `8`
- Init locals: `false`
- Local signature: `(empty)`

```il
IL_0000: ldarg.0
IL_0001: call System.Object [0x01000007]..ctor [0x0a000005]
IL_0006: ldarg.0
IL_0007: ldflda Widget.Position [0x04000007]
IL_000c: ldarg.1
IL_000d: stfld Point.X [0x04000001]
IL_0012: ldarg.0
IL_0013: ldflda Widget.Caption [0x04000008]
IL_0018: ldflda Caption.Value [0x04000005]
IL_001d: ldarg.2
IL_001e: stfld Label.Text [0x04000003]
IL_0023: ret
```

### Class `Program` [0x02000006]

- Namespace: `(empty)`
- Attributes: `Public, Abstract, Sealed, BeforeFieldInit`
- Base type: `System.Object [0x01000007]`
- Is interface: `false`

#### Method `Main` [0x06000002]

- Declaring type: `Program [0x02000006]`
- Attributes: `Public, Static, HideBySig`
- Implementation: `IL`
- Signature: `00 00 08`
- RVA: `0x00002078`
- Max stack: `3`
- Init locals: `true`
- Local signature: `07 03 12 14 11 0C 11 08`

```il
IL_0000: ldc.i4.1
IL_0001: ldstr "first" [0x70000001]
IL_0006: newobj Widget..ctor [0x06000001]
IL_000b: stloc.0
IL_000c: ldloc.0
IL_000d: ldflda Widget.Position [0x04000007]
IL_0012: ldc.i4.4
IL_0013: stfld Point.X [0x04000001]
IL_0018: ldloc.0
IL_0019: ldflda Widget.Caption [0x04000008]
IL_001e: ldfld Caption.Value [0x04000005]
IL_0023: stloc.1
IL_0024: ldloca.s 1
IL_0026: ldstr "copy" [0x7000000d]
IL_002b: stfld Label.Text [0x04000003]
IL_0030: ldloca.s 1
IL_0032: ldc.i4.7
IL_0033: stfld Label.Number [0x04000004]
IL_0038: ldloc.0
IL_0039: ldflda Widget.Caption [0x04000008]
IL_003e: ldflda Caption.Value [0x04000005]
IL_0043: ldc.i4.2
IL_0044: stfld Label.Number [0x04000004]
IL_0049: ldloc.0
IL_004a: ldflda Widget.Caption [0x04000008]
IL_004f: ldloc.0
IL_0050: stfld Caption.Target [0x04000006]
IL_0055: ldc.i4.s 9
IL_0057: ldstr "second" [0x70000017]
IL_005c: newobj Widget..ctor [0x06000001]
IL_0061: dup
IL_0062: ldloc.0
IL_0063: ldfld Widget.Caption [0x04000008]
IL_0068: stfld Widget.Caption [0x04000008]
IL_006d: dup
IL_006e: ldflda Widget.Caption [0x04000008]
IL_0073: ldflda Caption.Value [0x04000005]
IL_0078: ldstr "other" [0x70000025]
IL_007d: stfld Label.Text [0x04000003]
IL_0082: dup
IL_0083: ldflda Widget.Caption [0x04000008]
IL_0088: ldfld Caption.Target [0x04000006]
IL_008d: ldflda Widget.Position [0x04000007]
IL_0092: ldc.i4.6
IL_0093: stfld Point.Y [0x04000002]
IL_0098: ldloc.0
IL_0099: ldflda Widget.Position [0x04000007]
IL_009e: ldfld Point.X [0x04000001]
IL_00a3: call System.Console [0x01000008].WriteLine [0x0a000006]
IL_00a8: ldloc.0
IL_00a9: ldflda Widget.Position [0x04000007]
IL_00ae: ldfld Point.Y [0x04000002]
IL_00b3: call System.Console [0x01000008].WriteLine [0x0a000006]
IL_00b8: ldloc.0
IL_00b9: ldflda Widget.Caption [0x04000008]
IL_00be: ldflda Caption.Value [0x04000005]
IL_00c3: ldfld Label.Text [0x04000003]
IL_00c8: call System.Console [0x01000008].WriteLine [0x0a000007]
IL_00cd: ldloc.1
IL_00ce: ldfld Label.Text [0x04000003]
IL_00d3: call System.Console [0x01000008].WriteLine [0x0a000007]
IL_00d8: ldflda Widget.Caption [0x04000008]
IL_00dd: ldflda Caption.Value [0x04000005]
IL_00e2: ldfld Label.Text [0x04000003]
IL_00e7: call System.Console [0x01000008].WriteLine [0x0a000007]
IL_00ec: ldloc.0
IL_00ed: ldflda Widget.Caption [0x04000008]
IL_00f2: ldflda Caption.Value [0x04000005]
IL_00f7: ldfld Label.Number [0x04000004]
IL_00fc: call System.Console [0x01000008].WriteLine [0x0a000006]
IL_0101: ldloc.1
IL_0102: ldfld Label.Number [0x04000004]
IL_0107: call System.Console [0x01000008].WriteLine [0x0a000006]
IL_010c: ldloc.0
IL_010d: ldfld Widget.Position [0x04000007]
IL_0112: stloc.2
IL_0113: ldloca.s 2
IL_0115: ldc.i4.s 20
IL_0117: stfld Point.X [0x04000001]
IL_011c: ldloc.0
IL_011d: ldloc.2
IL_011e: stfld Widget.Position [0x04000007]
IL_0123: ldloca.s 2
IL_0125: ldc.i4.s 99
IL_0127: stfld Point.X [0x04000001]
IL_012c: ldloc.0
IL_012d: ldflda Widget.Position [0x04000007]
IL_0132: ldfld Point.X [0x04000001]
IL_0137: ret
```

### String literals

- 0x70000001: `"first"`
- 0x7000000d: `"copy"`
- 0x70000017: `"second"`
- 0x70000025: `"other"`
