# Assembly `XuiNet.Unit`

Source: [Unit.cs](Fixtures/Gc/Unit.cs)

- Version: `0.0.0.0`
- Culture: `neutral`
- Flags: `0`
- Hash algorithm: `Sha1`
- Public key: `(empty)`

## Module `XuiNet.Unit.exe`

- MVID: `<normalized>`
- Layout target: `WASM32`
- Entry point: `Unit.Main [0x06000003]`
- Declarations: `3 types, 4 fields, 3 methods`

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

#### Field `name` [0x04000003]

- Declaring type: `Unit [0x02000003]`
- Attributes: `Private`
- Signature: `06 0E`

#### Field `position` [0x04000004]

- Declaring type: `Unit [0x02000003]`
- Attributes: `Private`
- Signature: `06 11 08`

#### Method `.ctor` [0x06000001]

- Declaring type: `Unit [0x02000003]`
- Attributes: `Public, HideBySig, SpecialName, RTSpecialName`
- Implementation: `IL`
- Signature: `20 03 01 0E 08 08`
- RVA: `0x00002050`
- Max stack: `8`
- Init locals: `false`
- Local signature: `(empty)`

```il
IL_0000: ldarg.0
IL_0001: call System.Object [0x01000007]..ctor [0x0a000005]
IL_0006: ldarg.0
IL_0007: ldarg.1
IL_0008: stfld Unit.name [0x04000003]
IL_000d: ldarg.0
IL_000e: ldflda Unit.position [0x04000004]
IL_0013: ldarg.2
IL_0014: stfld Point.X [0x04000001]
IL_0019: ldarg.0
IL_001a: ldflda Unit.position [0x04000004]
IL_001f: ldarg.3
IL_0020: stfld Point.Y [0x04000002]
IL_0025: ret
```

#### Method `MoveTo` [0x06000002]

- Declaring type: `Unit [0x02000003]`
- Attributes: `Public, HideBySig`
- Implementation: `IL`
- Signature: `20 02 01 08 08`
- RVA: `0x00002077`
- Max stack: `8`
- Init locals: `false`
- Local signature: `(empty)`

```il
IL_0000: ldarg.0
IL_0001: ldflda Unit.position [0x04000004]
IL_0006: ldarg.1
IL_0007: stfld Point.X [0x04000001]
IL_000c: ldarg.0
IL_000d: ldflda Unit.position [0x04000004]
IL_0012: ldarg.2
IL_0013: stfld Point.Y [0x04000002]
IL_0018: ret
```

#### Method `Main` [0x06000003]

- Declaring type: `Unit [0x02000003]`
- Attributes: `Public, Static, HideBySig`
- Implementation: `IL`
- Signature: `00 00 08`
- RVA: `0x00002094`
- Max stack: `4`
- Init locals: `true`
- Local signature: `07 02 12 0C 11 08`

```il
IL_0000: ldstr "Scout" [0x70000001]
IL_0005: ldc.i4.1
IL_0006: ldc.i4.2
IL_0007: newobj Unit..ctor [0x06000001]
IL_000c: dup
IL_000d: stloc.0
IL_000e: ldloc.0
IL_000f: ldc.i4.4
IL_0010: ldc.i4.6
IL_0011: callvirt Unit.MoveTo [0x06000002]
IL_0016: dup
IL_0017: ldfld Unit.position [0x04000004]
IL_001c: stloc.1
IL_001d: ldloca.s 1
IL_001f: ldc.i4.s 20
IL_0021: stfld Point.X [0x04000001]
IL_0026: dup
IL_0027: ldfld Unit.name [0x04000003]
IL_002c: call System.Console [0x01000008].WriteLine [0x0a000006]
IL_0031: dup
IL_0032: ldflda Unit.position [0x04000004]
IL_0037: ldfld Point.X [0x04000001]
IL_003c: call System.Console [0x01000008].WriteLine [0x0a000007]
IL_0041: dup
IL_0042: ldflda Unit.position [0x04000004]
IL_0047: ldfld Point.Y [0x04000002]
IL_004c: call System.Console [0x01000008].WriteLine [0x0a000007]
IL_0051: ldloc.1
IL_0052: ldfld Point.X [0x04000001]
IL_0057: call System.Console [0x01000008].WriteLine [0x0a000007]
IL_005c: dup
IL_005d: ldloc.1
IL_005e: stfld Unit.position [0x04000004]
IL_0063: ldloca.s 1
IL_0065: ldc.i4.s 99
IL_0067: stfld Point.X [0x04000001]
IL_006c: ldloc.0
IL_006d: ldflda Unit.position [0x04000004]
IL_0072: ldfld Point.X [0x04000001]
IL_0077: call System.Console [0x01000008].WriteLine [0x0a000007]
IL_007c: ldloc.0
IL_007d: ceq
IL_007f: call System.Console [0x01000008].WriteLine [0x0a000008]
IL_0084: ldc.i4.0
IL_0085: ret
```

### String literals

- 0x70000001: `"Scout"`
