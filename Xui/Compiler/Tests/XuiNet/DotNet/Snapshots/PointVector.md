# Assembly `XuiNet.PointVector`

Source: [PointVector.cs](Fixtures/Structs/PointVector.cs)

- Version: `0.0.0.0`
- Culture: `neutral`
- Flags: `0`
- Hash algorithm: `Sha1`
- Public key: `(empty)`

## Module `XuiNet.PointVector.exe`

- MVID: `<normalized>`
- Layout target: `WASM32`
- Entry point: `Program.Main [0x06000004]`
- Declarations: `4 types, 4 fields, 4 methods`

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
- 0x01000007: `System.Runtime.InteropServices.NFloat; scope: System.Private.CoreLib [0x23000001]`
- 0x01000008: `System.Object; scope: System.Private.CoreLib [0x23000001]`
- 0x01000009: `System.Console; scope: System.Console [0x23000002]`

### Member references

- 0x0a000001: `Method .ctor; parent: System.Runtime.CompilerServices.CompilationRelaxationsAttribute [0x01000001]; signature: 20 01 01 08`
- 0x0a000002: `Method .ctor; parent: System.Runtime.CompilerServices.RuntimeCompatibilityAttribute [0x01000002]; signature: 20 00 01`
- 0x0a000003: `Method .ctor; parent: System.Diagnostics.DebuggableAttribute [0x01000003]; signature: 20 01 01 11 11`
- 0x0a000004: `Method .ctor; parent: System.Runtime.CompilerServices.RefSafetyRulesAttribute [0x01000005]; signature: 20 01 01 08`
- 0x0a000005: `Method op_Addition; parent: System.Runtime.InteropServices.NFloat [0x01000007]; signature: 00 02 11 1D 11 1D 11 1D`
- 0x0a000006: `Method op_Implicit; parent: System.Runtime.InteropServices.NFloat [0x01000007]; signature: 00 01 11 1D 08`
- 0x0a000007: `Method op_Implicit; parent: System.Runtime.InteropServices.NFloat [0x01000007]; signature: 00 01 0D 11 1D`
- 0x0a000008: `Method WriteLine; parent: System.Console [0x01000009]; signature: 00 01 01 0D`

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
- Signature: `06 11 1D`
- Offset: `0`
- Size: `4`

#### Field `Y` [0x04000002]

- Declaring type: `Point [0x02000002]`
- Attributes: `Public`
- Signature: `06 11 1D`
- Offset: `4`
- Size: `4`

#### Method `.ctor` [0x06000001]

- Declaring type: `Point [0x02000002]`
- Attributes: `Public, HideBySig, SpecialName, RTSpecialName`
- Implementation: `IL`
- Signature: `20 02 01 11 1D 11 1D`
- RVA: `0x00002050`
- Max stack: `8`
- Init locals: `false`
- Local signature: `(empty)`

```il
IL_0000: ldarg.0
IL_0001: ldarg.1
IL_0002: stfld Point.X [0x04000001]
IL_0007: ldarg.0
IL_0008: ldarg.2
IL_0009: stfld Point.Y [0x04000002]
IL_000e: ret
```

#### Method `op_Addition` [0x06000002]

- Declaring type: `Point [0x02000002]`
- Attributes: `Public, Static, HideBySig, SpecialName`
- Implementation: `IL`
- Signature: `00 02 11 08 11 08 11 0C`
- RVA: `0x00002060`
- Max stack: `8`
- Init locals: `false`
- Local signature: `(empty)`

```il
IL_0000: ldarg.0
IL_0001: ldfld Point.X [0x04000001]
IL_0006: ldarg.1
IL_0007: ldfld Vector.X [0x04000003]
IL_000c: call System.Runtime.InteropServices.NFloat [0x01000007].op_Addition [0x0a000005]
IL_0011: ldarg.0
IL_0012: ldfld Point.Y [0x04000002]
IL_0017: ldarg.1
IL_0018: ldfld Vector.Y [0x04000004]
IL_001d: call System.Runtime.InteropServices.NFloat [0x01000007].op_Addition [0x0a000005]
IL_0022: newobj Point..ctor [0x06000001]
IL_0027: ret
```

### Struct `Vector` [0x02000003]

- Namespace: `(empty)`
- Attributes: `Public, SequentialLayout, Sealed, BeforeFieldInit`
- Base type: `System.ValueType [0x01000006]`
- Is interface: `false`
- Storage classification: `LinearMemory`
- Alignment: `4`
- Size: `8`

#### Field `X` [0x04000003]

- Declaring type: `Vector [0x02000003]`
- Attributes: `Public`
- Signature: `06 11 1D`
- Offset: `0`
- Size: `4`

#### Field `Y` [0x04000004]

- Declaring type: `Vector [0x02000003]`
- Attributes: `Public`
- Signature: `06 11 1D`
- Offset: `4`
- Size: `4`

#### Method `.ctor` [0x06000003]

- Declaring type: `Vector [0x02000003]`
- Attributes: `Public, HideBySig, SpecialName, RTSpecialName`
- Implementation: `IL`
- Signature: `20 02 01 11 1D 11 1D`
- RVA: `0x00002089`
- Max stack: `8`
- Init locals: `false`
- Local signature: `(empty)`

```il
IL_0000: ldarg.0
IL_0001: ldarg.1
IL_0002: stfld Vector.X [0x04000003]
IL_0007: ldarg.0
IL_0008: ldarg.2
IL_0009: stfld Vector.Y [0x04000004]
IL_000e: ret
```

### Class `Program` [0x02000004]

- Namespace: `(empty)`
- Attributes: `Public, Abstract, Sealed, BeforeFieldInit`
- Base type: `System.Object [0x01000008]`
- Is interface: `false`

#### Method `Main` [0x06000004]

- Declaring type: `Program [0x02000004]`
- Attributes: `Public, Static, HideBySig`
- Implementation: `IL`
- Signature: `00 00 08`
- RVA: `0x0000209c`
- Max stack: `4`
- Init locals: `true`
- Local signature: `07 01 11 0C`

```il
IL_0000: ldc.i4.1
IL_0001: call System.Runtime.InteropServices.NFloat [0x01000007].op_Implicit [0x0a000006]
IL_0006: ldc.i4.2
IL_0007: call System.Runtime.InteropServices.NFloat [0x01000007].op_Implicit [0x0a000006]
IL_000c: newobj Point..ctor [0x06000001]
IL_0011: ldloca.s 0
IL_0013: ldc.i4.3
IL_0014: call System.Runtime.InteropServices.NFloat [0x01000007].op_Implicit [0x0a000006]
IL_0019: ldc.i4.4
IL_001a: call System.Runtime.InteropServices.NFloat [0x01000007].op_Implicit [0x0a000006]
IL_001f: call Vector..ctor [0x06000003]
IL_0024: ldloc.0
IL_0025: call Point.op_Addition [0x06000002]
IL_002a: dup
IL_002b: ldfld Point.X [0x04000001]
IL_0030: call System.Runtime.InteropServices.NFloat [0x01000007].op_Implicit [0x0a000007]
IL_0035: conv.r8
IL_0036: call System.Console [0x01000009].WriteLine [0x0a000008]
IL_003b: ldfld Point.Y [0x04000002]
IL_0040: call System.Runtime.InteropServices.NFloat [0x01000007].op_Implicit [0x0a000007]
IL_0045: conv.r8
IL_0046: call System.Console [0x01000009].WriteLine [0x0a000008]
IL_004b: ldc.i4.0
IL_004c: ret
```

### String literals

None.
