# Assembly `XuiNet.Units`

Source: [Units.cs](Fixtures/Polymorphism/Units.cs)

- Version: `0.0.0.0`
- Culture: `neutral`
- Flags: `0`
- Hash algorithm: `Sha1`
- Public key: `(empty)`

## Module `XuiNet.Units.exe`

- MVID: `<normalized>`
- Layout target: `WASM32`
- Entry point: `Program.Main [0x0600000b]`
- Declarations: `7 types, 5 fields, 11 methods`

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
- 0x0a000006: `Method WriteLine; parent: System.Console [0x01000008]; signature: 00 01 01 0E`
- 0x0a000007: `Method WriteLine; parent: System.Console [0x01000008]; signature: 00 01 01 08`
- 0x0a000008: `Method WriteLine; parent: System.Console [0x01000008]; signature: 00 01 01 02`

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

### Class `Unit` [0x02000003]

- Namespace: `(empty)`
- Attributes: `Public, BeforeFieldInit`
- Base type: `System.Object [0x01000007]`
- Is interface: `false`

#### Field `Name` [0x04000003]

- Declaring type: `Unit [0x02000003]`
- Attributes: `Public`
- Signature: `06 0E`

#### Field `Position` [0x04000004]

- Declaring type: `Unit [0x02000003]`
- Attributes: `Public`
- Signature: `06 11 08`

#### Method `.ctor` [0x06000001]

- Declaring type: `Unit [0x02000003]`
- Attributes: `Public, HideBySig, SpecialName, RTSpecialName`
- Implementation: `IL`
- Signature: `20 01 01 0E`
- RVA: `0x00002050`
- Max stack: `8`
- Init locals: `false`
- Local signature: `(empty)`

```il
IL_0000: ldarg.0
IL_0001: call System.Object [0x01000007]..ctor [0x0a000005]
IL_0006: ldarg.0
IL_0007: ldarg.1
IL_0008: stfld Unit.Name [0x04000003]
IL_000d: ret
```

#### Method `Power` [0x06000002]

- Declaring type: `Unit [0x02000003]`
- Attributes: `Public, Virtual, HideBySig, VtableLayoutMask`
- Implementation: `IL`
- Signature: `20 00 08`
- RVA: `0x0000205f`
- Max stack: `8`
- Init locals: `false`
- Local signature: `(empty)`

```il
IL_0000: ldc.i4.1
IL_0001: ret
```

#### Method `Kind` [0x06000003]

- Declaring type: `Unit [0x02000003]`
- Attributes: `Public, Virtual, HideBySig, VtableLayoutMask`
- Implementation: `IL`
- Signature: `20 00 0E`
- RVA: `0x00002062`
- Max stack: `8`
- Init locals: `false`
- Local signature: `(empty)`

```il
IL_0000: ldstr "unit" [0x70000001]
IL_0005: ret
```

### Class `Scout` [0x02000004]

- Namespace: `(empty)`
- Attributes: `Public, BeforeFieldInit`
- Base type: `Unit [0x02000003]`
- Is interface: `false`

#### Field `Bonus` [0x04000005]

- Declaring type: `Scout [0x02000004]`
- Attributes: `Public`
- Signature: `06 08`

#### Method `.ctor` [0x06000004]

- Declaring type: `Scout [0x02000004]`
- Attributes: `Public, HideBySig, SpecialName, RTSpecialName`
- Implementation: `IL`
- Signature: `20 02 01 0E 08`
- RVA: `0x00002069`
- Max stack: `8`
- Init locals: `false`
- Local signature: `(empty)`

```il
IL_0000: ldarg.0
IL_0001: ldarg.1
IL_0002: call Unit..ctor [0x06000001]
IL_0007: ldarg.0
IL_0008: ldarg.2
IL_0009: stfld Scout.Bonus [0x04000005]
IL_000e: ret
```

#### Method `Power` [0x06000005]

- Declaring type: `Scout [0x02000004]`
- Attributes: `Public, Virtual, HideBySig`
- Implementation: `IL`
- Signature: `20 00 08`
- RVA: `0x00002079`
- Max stack: `8`
- Init locals: `false`
- Local signature: `(empty)`

```il
IL_0000: ldarg.0
IL_0001: call Unit.Power [0x06000002]
IL_0006: ldarg.0
IL_0007: ldfld Scout.Bonus [0x04000005]
IL_000c: add
IL_000d: ret
```

#### Method `Kind` [0x06000006]

- Declaring type: `Scout [0x02000004]`
- Attributes: `Public, Virtual, HideBySig`
- Implementation: `IL`
- Signature: `20 00 0E`
- RVA: `0x00002088`
- Max stack: `8`
- Init locals: `false`
- Local signature: `(empty)`

```il
IL_0000: ldstr "scout" [0x7000000b]
IL_0005: ret
```

### Class `Veteran` [0x02000005]

- Namespace: `(empty)`
- Attributes: `Public, BeforeFieldInit`
- Base type: `Scout [0x02000004]`
- Is interface: `false`

#### Method `.ctor` [0x06000007]

- Declaring type: `Veteran [0x02000005]`
- Attributes: `Public, HideBySig, SpecialName, RTSpecialName`
- Implementation: `IL`
- Signature: `20 02 01 0E 08`
- RVA: `0x0000208f`
- Max stack: `8`
- Init locals: `false`
- Local signature: `(empty)`

```il
IL_0000: ldarg.0
IL_0001: ldarg.1
IL_0002: ldarg.2
IL_0003: call Scout..ctor [0x06000004]
IL_0008: ret
```

#### Method `Power` [0x06000008]

- Declaring type: `Veteran [0x02000005]`
- Attributes: `Public, Virtual, HideBySig`
- Implementation: `IL`
- Signature: `20 00 08`
- RVA: `0x00002099`
- Max stack: `8`
- Init locals: `false`
- Local signature: `(empty)`

```il
IL_0000: ldarg.0
IL_0001: call Scout.Power [0x06000005]
IL_0006: ldc.i4.s 10
IL_0008: add
IL_0009: ret
```

### Class `Medic` [0x02000006]

- Namespace: `(empty)`
- Attributes: `Public, BeforeFieldInit`
- Base type: `Unit [0x02000003]`
- Is interface: `false`

#### Method `.ctor` [0x06000009]

- Declaring type: `Medic [0x02000006]`
- Attributes: `Public, HideBySig, SpecialName, RTSpecialName`
- Implementation: `IL`
- Signature: `20 01 01 0E`
- RVA: `0x000020a4`
- Max stack: `8`
- Init locals: `false`
- Local signature: `(empty)`

```il
IL_0000: ldarg.0
IL_0001: ldarg.1
IL_0002: call Unit..ctor [0x06000001]
IL_0007: ret
```

### Class `Program` [0x02000007]

- Namespace: `(empty)`
- Attributes: `Public, Abstract, Sealed, BeforeFieldInit`
- Base type: `System.Object [0x01000007]`
- Is interface: `false`

#### Method `Inspect` [0x0600000a]

- Declaring type: `Program [0x02000007]`
- Attributes: `Private, Static, HideBySig`
- Implementation: `IL`
- Signature: `00 01 08 12 0C`
- RVA: `0x000020b0`
- Max stack: `2`
- Init locals: `true`
- Local signature: `07 01 12 10`

```il
IL_0000: ldarg.0
IL_0001: ldfld Unit.Name [0x04000003]
IL_0006: call System.Console [0x01000008].WriteLine [0x0a000006]
IL_000b: ldarg.0
IL_000c: callvirt Unit.Kind [0x06000003]
IL_0011: call System.Console [0x01000008].WriteLine [0x0a000006]
IL_0016: ldarg.0
IL_0017: callvirt Unit.Power [0x06000002]
IL_001c: call System.Console [0x01000008].WriteLine [0x0a000007]
IL_0021: ldarg.0
IL_0022: ldflda Unit.Position [0x04000004]
IL_0027: ldfld Point.X [0x04000001]
IL_002c: call System.Console [0x01000008].WriteLine [0x0a000007]
IL_0031: ldarg.0
IL_0032: isinst Scout [0x02000004]
IL_0037: ldnull
IL_0038: cgt.un
IL_003a: call System.Console [0x01000008].WriteLine [0x0a000008]
IL_003f: ldarg.0
IL_0040: isinst Scout [0x02000004]
IL_0045: stloc.0
IL_0046: ldloc.0
IL_0047: brfalse.s IL_0054
IL_0049: ldloc.0
IL_004a: ldfld Scout.Bonus [0x04000005]
IL_004f: call System.Console [0x01000008].WriteLine [0x0a000007]
IL_0054: ldarg.0
IL_0055: isinst Medic [0x02000006]
IL_005a: ldnull
IL_005b: cgt.un
IL_005d: call System.Console [0x01000008].WriteLine [0x0a000008]
IL_0062: ldarg.0
IL_0063: isinst Medic [0x02000006]
IL_0068: ldnull
IL_0069: ceq
IL_006b: call System.Console [0x01000008].WriteLine [0x0a000008]
IL_0070: ldarg.0
IL_0071: callvirt Unit.Power [0x06000002]
IL_0076: ret
```

#### Method `Main` [0x0600000b]

- Declaring type: `Program [0x02000007]`
- Attributes: `Public, Static, HideBySig`
- Implementation: `IL`
- Signature: `00 00 08`
- RVA: `0x00002133`
- Max stack: `8`
- Init locals: `false`
- Local signature: `(empty)`

```il
IL_0000: ldstr "Ada" [0x70000017]
IL_0005: ldc.i4.3
IL_0006: newobj Veteran..ctor [0x06000007]
IL_000b: dup
IL_000c: ldflda Unit.Position [0x04000004]
IL_0011: ldc.i4.4
IL_0012: stfld Point.X [0x04000001]
IL_0017: dup
IL_0018: ldflda Unit.Position [0x04000004]
IL_001d: ldc.i4.6
IL_001e: stfld Point.Y [0x04000002]
IL_0023: call Program.Inspect [0x0600000a]
IL_0028: ret
```

### String literals

- 0x70000001: `"unit"`
- 0x7000000b: `"scout"`
- 0x70000017: `"Ada"`
