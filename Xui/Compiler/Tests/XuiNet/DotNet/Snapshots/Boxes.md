# Assembly `XuiNet.Boxes`

Source: [Boxes.cs](Fixtures/Generics/Boxes.cs)

- Version: `0.0.0.0`
- Culture: `neutral`
- Flags: `0`
- Hash algorithm: `Sha1`
- Public key: `(empty)`

## Module `XuiNet.Boxes.exe`

- MVID: `<normalized>`
- Layout target: `WASM32`
- Entry point: `Program.Main [0x06000006]`
- Declarations: `3 types, 1 fields, 6 methods`

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
- 0x01000006: `System.Object; scope: System.Private.CoreLib [0x23000001]`
- 0x01000007: `System.Console; scope: System.Console [0x23000002]`

### Member references

- 0x0a000001: `Method .ctor; parent: System.Runtime.CompilerServices.CompilationRelaxationsAttribute [0x01000001]; signature: 20 01 01 08`
- 0x0a000002: `Method .ctor; parent: System.Runtime.CompilerServices.RuntimeCompatibilityAttribute [0x01000002]; signature: 20 00 01`
- 0x0a000003: `Method .ctor; parent: System.Diagnostics.DebuggableAttribute [0x01000003]; signature: 20 01 01 11 11`
- 0x0a000004: `Method .ctor; parent: System.Runtime.CompilerServices.RefSafetyRulesAttribute [0x01000005]; signature: 20 01 01 08`
- 0x0a000005: `Method .ctor; parent: System.Object [0x01000006]; signature: 20 00 01`
- 0x0a000006: `Field Value; parent: Box<!0> [0x1b000001]; signature: 06 13 00`
- 0x0a000007: `Method .ctor; parent: Box<Int32> [0x1b000002]; signature: 20 01 01 13 00`
- 0x0a000008: `Method Read; parent: Box<Int32> [0x1b000002]; signature: 20 00 13 00`
- 0x0a000009: `Method Write; parent: Box<Int32> [0x1b000002]; signature: 20 01 01 13 00`
- 0x0a00000a: `Method .ctor; parent: Box<String> [0x1b000003]; signature: 20 01 01 13 00`
- 0x0a00000b: `Method WriteLine; parent: System.Console [0x01000007]; signature: 00 01 01 08`
- 0x0a00000c: `Method Read; parent: Box<String> [0x1b000003]; signature: 20 00 13 00`
- 0x0a00000d: `Method WriteLine; parent: System.Console [0x01000007]; signature: 00 01 01 0E`
- 0x0a00000e: `Method Exchange; parent: Box<Int32> [0x1b000002]; signature: 20 01 13 00 13 00`
- 0x0a00000f: `Field Value; parent: Box<Int32> [0x1b000002]; signature: 06 13 00`
- 0x0a000010: `Method Exchange; parent: Box<String> [0x1b000003]; signature: 20 01 13 00 13 00`
- 0x0a000011: `Method Echo; parent: Box<Int32> [0x1b000002]; signature: 00 01 13 00 13 00`

### Type specifications

- 0x1b000001: `Box<!0>; signature: 15 12 08 01 13 00`
- 0x1b000002: `Box<Int32>; signature: 15 12 08 01 08`
- 0x1b000003: `Box<String>; signature: 15 12 08 01 0E`

### Module type `<Module>` [0x02000001]

- Namespace: `(empty)`
- Attributes: `NotPublic`
- Base type: `none`
- Is interface: `false`

### Class `` Box`1 `` [0x02000002]

- Namespace: `(empty)`
- Attributes: `Public, BeforeFieldInit`
- Base type: `System.Object [0x01000006]`
- Is interface: `false`
- Generic parameter 0: `T; attributes: None`

#### Field `Value` [0x04000001]

- Declaring type: `` Box`1 [0x02000002] ``
- Attributes: `Public`
- Signature: `06 13 00`

#### Method `.ctor` [0x06000001]

- Declaring type: `` Box`1 [0x02000002] ``
- Attributes: `Public, HideBySig, SpecialName, RTSpecialName`
- Implementation: `IL`
- Signature: `20 01 01 13 00`
- RVA: `0x00002050`
- Max stack: `8`
- Init locals: `false`
- Local signature: `(empty)`

```il
IL_0000: ldarg.0
IL_0001: call System.Object [0x01000006]..ctor [0x0a000005]
IL_0006: ldarg.0
IL_0007: ldarg.1
IL_0008: stfld Box<!0> [0x1b000001].Value [0x0a000006]
IL_000d: ret
```

#### Method `Read` [0x06000002]

- Declaring type: `` Box`1 [0x02000002] ``
- Attributes: `Public, HideBySig`
- Implementation: `IL`
- Signature: `20 00 13 00`
- RVA: `0x0000205f`
- Max stack: `8`
- Init locals: `false`
- Local signature: `(empty)`

```il
IL_0000: ldarg.0
IL_0001: ldfld Box<!0> [0x1b000001].Value [0x0a000006]
IL_0006: ret
```

#### Method `Write` [0x06000003]

- Declaring type: `` Box`1 [0x02000002] ``
- Attributes: `Public, HideBySig`
- Implementation: `IL`
- Signature: `20 01 01 13 00`
- RVA: `0x00002067`
- Max stack: `8`
- Init locals: `false`
- Local signature: `(empty)`

```il
IL_0000: ldarg.0
IL_0001: ldarg.1
IL_0002: stfld Box<!0> [0x1b000001].Value [0x0a000006]
IL_0007: ret
```

#### Method `Exchange` [0x06000004]

- Declaring type: `` Box`1 [0x02000002] ``
- Attributes: `Public, HideBySig`
- Implementation: `IL`
- Signature: `20 01 13 00 13 00`
- RVA: `0x00002070`
- Max stack: `8`
- Init locals: `false`
- Local signature: `(empty)`

```il
IL_0000: ldarg.0
IL_0001: ldfld Box<!0> [0x1b000001].Value [0x0a000006]
IL_0006: ldarg.0
IL_0007: ldarg.1
IL_0008: stfld Box<!0> [0x1b000001].Value [0x0a000006]
IL_000d: ret
```

#### Method `Echo` [0x06000005]

- Declaring type: `` Box`1 [0x02000002] ``
- Attributes: `Public, Static, HideBySig`
- Implementation: `IL`
- Signature: `00 01 13 00 13 00`
- RVA: `0x0000207f`
- Max stack: `8`
- Init locals: `false`
- Local signature: `(empty)`

```il
IL_0000: ldarg.0
IL_0001: ret
```

### Class `Program` [0x02000003]

- Namespace: `(empty)`
- Attributes: `Public, Abstract, Sealed, BeforeFieldInit`
- Base type: `System.Object [0x01000006]`
- Is interface: `false`

#### Method `Main` [0x06000006]

- Declaring type: `Program [0x02000003]`
- Attributes: `Public, Static, HideBySig`
- Implementation: `IL`
- Signature: `00 00 08`
- RVA: `0x00002084`
- Max stack: `3`
- Init locals: `true`
- Local signature: `07 01 15 12 08 01 08`

```il
IL_0000: ldc.i4.s 41
IL_0002: newobj Box<Int32> [0x1b000002]..ctor [0x0a000007]
IL_0007: stloc.0
IL_0008: ldloc.0
IL_0009: ldloc.0
IL_000a: callvirt Box<Int32> [0x1b000002].Read [0x0a000008]
IL_000f: ldc.i4.1
IL_0010: add
IL_0011: callvirt Box<Int32> [0x1b000002].Write [0x0a000009]
IL_0016: ldstr "Hello" [0x70000001]
IL_001b: newobj Box<String> [0x1b000003]..ctor [0x0a00000a]
IL_0020: ldloc.0
IL_0021: callvirt Box<Int32> [0x1b000002].Read [0x0a000008]
IL_0026: call System.Console [0x01000007].WriteLine [0x0a00000b]
IL_002b: dup
IL_002c: callvirt Box<String> [0x1b000003].Read [0x0a00000c]
IL_0031: call System.Console [0x01000007].WriteLine [0x0a00000d]
IL_0036: ldloc.0
IL_0037: ldc.i4.7
IL_0038: callvirt Box<Int32> [0x1b000002].Exchange [0x0a00000e]
IL_003d: call System.Console [0x01000007].WriteLine [0x0a00000b]
IL_0042: ldloc.0
IL_0043: ldfld Box<Int32> [0x1b000002].Value [0x0a00000f]
IL_0048: call System.Console [0x01000007].WriteLine [0x0a00000b]
IL_004d: dup
IL_004e: ldstr "World" [0x7000000d]
IL_0053: callvirt Box<String> [0x1b000003].Exchange [0x0a000010]
IL_0058: call System.Console [0x01000007].WriteLine [0x0a00000d]
IL_005d: callvirt Box<String> [0x1b000003].Read [0x0a00000c]
IL_0062: call System.Console [0x01000007].WriteLine [0x0a00000d]
IL_0067: ldc.i4.0
IL_0068: call Box<Int32> [0x1b000002].Echo [0x0a000011]
IL_006d: ret
```

### String literals

- 0x70000001: `"Hello"`
- 0x7000000d: `"World"`
