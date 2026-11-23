# Assembly `XuiNet.DerivedBoxes`

Source: [DerivedBoxes.cs](Fixtures/Generics/DerivedBoxes.cs)

- Version: `0.0.0.0`
- Culture: `neutral`
- Flags: `0`
- Hash algorithm: `Sha1`
- Public key: `(empty)`

## Module `XuiNet.DerivedBoxes.exe`

- MVID: `<normalized>`
- Layout target: `WASM32`
- Entry point: `Program.Main [0x0600000b]`
- Declarations: `5 types, 3 fields, 11 methods`

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
- 0x0a000006: `Field Value; parent: Box<!0> [0x1b000003]; signature: 06 13 00`
- 0x0a000007: `Method .ctor; parent: Box<!1> [0x1b000001]; signature: 20 01 01 13 00`
- 0x0a000008: `Field Tag; parent: TaggedBox<!0, !1> [0x1b000004]; signature: 06 13 00`
- 0x0a000009: `Method Read; parent: Box<!1> [0x1b000001]; signature: 20 00 13 00`
- 0x0a00000a: `Method .ctor; parent: TaggedBox<String, Int32> [0x1b000002]; signature: 20 02 01 13 01 13 00`
- 0x0a00000b: `Method Read; parent: TaggedBox<String, Int32> [0x1b000002]; signature: 20 00 13 01`
- 0x0a00000c: `Method Read; parent: Box<Int32> [0x1b000005]; signature: 20 00 13 00`
- 0x0a00000d: `Method WriteLine; parent: System.Console [0x01000007]; signature: 00 01 01 08`
- 0x0a00000e: `Method Echo; parent: Box<Int32> [0x1b000005]; signature: 20 01 13 00 13 00`
- 0x0a00000f: `Method WriteLine; parent: System.Console [0x01000007]; signature: 00 01 01 02`
- 0x0a000010: `Field Tag; parent: TaggedBox<String, Int32> [0x1b000002]; signature: 06 13 00`
- 0x0a000011: `Method WriteLine; parent: System.Console [0x01000007]; signature: 00 01 01 0E`
- 0x0a000012: `Method Read; parent: Box<String> [0x1b000006]; signature: 20 00 13 00`
- 0x0a000013: `Method Echo; parent: Box<String> [0x1b000006]; signature: 20 01 13 00 13 00`
- 0x0a000014: `Method .ctor; parent: TaggedBox<Int32, String> [0x1b000007]; signature: 20 02 01 13 01 13 00`
- 0x0a000015: `Field Value; parent: Box<Int32> [0x1b000005]; signature: 06 13 00`

### Type specifications

- 0x1b000001: `Box<!1>; signature: 15 12 08 01 13 01`
- 0x1b000002: `TaggedBox<String, Int32>; signature: 15 12 0C 02 0E 08`
- 0x1b000003: `Box<!0>; signature: 15 12 08 01 13 00`
- 0x1b000004: `TaggedBox<!0, !1>; signature: 15 12 0C 02 13 00 13 01`
- 0x1b000005: `Box<Int32>; signature: 15 12 08 01 08`
- 0x1b000006: `Box<String>; signature: 15 12 08 01 0E`
- 0x1b000007: `TaggedBox<Int32, String>; signature: 15 12 0C 02 08 0E`
- 0x1b000008: `TaggedBox<String, String>; signature: 15 12 0C 02 0E 0E`

### Module type `<Module>` [0x02000001]

- Namespace: `(empty)`
- Attributes: `NotPublic`
- Base type: `none`
- Is interface: `false`

### Class `` Box`1 `` [0x02000002]

- Namespace: `(empty)`
- Attributes: `Public, Abstract, BeforeFieldInit`
- Base type: `System.Object [0x01000006]`
- Is interface: `false`
- Generic parameter 0: `T; attributes: None`

#### Field `Value` [0x04000001]

- Declaring type: `` Box`1 [0x02000002] ``
- Attributes: `Public`
- Signature: `06 13 00`

#### Method `.ctor` [0x06000001]

- Declaring type: `` Box`1 [0x02000002] ``
- Attributes: `Family, HideBySig, SpecialName, RTSpecialName`
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
IL_0008: stfld Box<!0> [0x1b000003].Value [0x0a000006]
IL_000d: ret
```

#### Method `Read` [0x06000002]

- Declaring type: `` Box`1 [0x02000002] ``
- Attributes: `Public, Virtual, HideBySig, VtableLayoutMask`
- Implementation: `IL`
- Signature: `20 00 13 00`
- RVA: `0x0000205f`
- Max stack: `8`
- Init locals: `false`
- Local signature: `(empty)`

```il
IL_0000: ldarg.0
IL_0001: ldfld Box<!0> [0x1b000003].Value [0x0a000006]
IL_0006: ret
```

#### Method `Echo` [0x06000003]

- Declaring type: `` Box`1 [0x02000002] ``
- Attributes: `Public, Virtual, HideBySig, VtableLayoutMask, Abstract`
- Implementation: `IL`
- Signature: `20 01 13 00 13 00`
- Body: `none`

### Class `` TaggedBox`2 `` [0x02000003]

- Namespace: `(empty)`
- Attributes: `Public, BeforeFieldInit`
- Base type: `Box<!1> [0x1b000001]`
- Is interface: `false`
- Generic parameter 0: `TTag; attributes: None`
- Generic parameter 1: `T; attributes: None`

#### Field `Tag` [0x04000002]

- Declaring type: `` TaggedBox`2 [0x02000003] ``
- Attributes: `Public`
- Signature: `06 13 00`

#### Method `.ctor` [0x06000004]

- Declaring type: `` TaggedBox`2 [0x02000003] ``
- Attributes: `Public, HideBySig, SpecialName, RTSpecialName`
- Implementation: `IL`
- Signature: `20 02 01 13 01 13 00`
- RVA: `0x00002067`
- Max stack: `8`
- Init locals: `false`
- Local signature: `(empty)`

```il
IL_0000: ldarg.0
IL_0001: ldarg.1
IL_0002: call Box<!1> [0x1b000001]..ctor [0x0a000007]
IL_0007: ldarg.0
IL_0008: ldarg.2
IL_0009: stfld TaggedBox<!0, !1> [0x1b000004].Tag [0x0a000008]
IL_000e: ret
```

#### Method `Read` [0x06000005]

- Declaring type: `` TaggedBox`2 [0x02000003] ``
- Attributes: `Public, Virtual, HideBySig`
- Implementation: `IL`
- Signature: `20 00 13 01`
- RVA: `0x00002077`
- Max stack: `8`
- Init locals: `false`
- Local signature: `(empty)`

```il
IL_0000: ldarg.0
IL_0001: call Box<!1> [0x1b000001].Read [0x0a000009]
IL_0006: ret
```

#### Method `Echo` [0x06000006]

- Declaring type: `` TaggedBox`2 [0x02000003] ``
- Attributes: `Public, Virtual, HideBySig`
- Implementation: `IL`
- Signature: `20 01 13 01 13 01`
- RVA: `0x0000207f`
- Max stack: `8`
- Init locals: `false`
- Local signature: `(empty)`

```il
IL_0000: ldarg.1
IL_0001: ret
```

### Class `IntBox` [0x02000004]

- Namespace: `(empty)`
- Attributes: `Public, Sealed, BeforeFieldInit`
- Base type: `TaggedBox<String, Int32> [0x1b000002]`
- Is interface: `false`

#### Field `Extra` [0x04000003]

- Declaring type: `IntBox [0x02000004]`
- Attributes: `Public`
- Signature: `06 08`

#### Method `.ctor` [0x06000007]

- Declaring type: `IntBox [0x02000004]`
- Attributes: `Public, HideBySig, SpecialName, RTSpecialName`
- Implementation: `IL`
- Signature: `20 02 01 08 08`
- RVA: `0x00002082`
- Max stack: `8`
- Init locals: `false`
- Local signature: `(empty)`

```il
IL_0000: ldarg.0
IL_0001: ldarg.1
IL_0002: ldstr "number" [0x70000001]
IL_0007: call TaggedBox<String, Int32> [0x1b000002]..ctor [0x0a00000a]
IL_000c: ldarg.0
IL_000d: ldarg.2
IL_000e: stfld IntBox.Extra [0x04000003]
IL_0013: ret
```

#### Method `Read` [0x06000008]

- Declaring type: `IntBox [0x02000004]`
- Attributes: `Public, Virtual, HideBySig`
- Implementation: `IL`
- Signature: `20 00 08`
- RVA: `0x00002097`
- Max stack: `8`
- Init locals: `false`
- Local signature: `(empty)`

```il
IL_0000: ldarg.0
IL_0001: call TaggedBox<String, Int32> [0x1b000002].Read [0x0a00000b]
IL_0006: ldarg.0
IL_0007: ldfld IntBox.Extra [0x04000003]
IL_000c: add
IL_000d: ret
```

### Class `Program` [0x02000005]

- Namespace: `(empty)`
- Attributes: `Public, Abstract, Sealed, BeforeFieldInit`
- Base type: `System.Object [0x01000006]`
- Is interface: `false`

#### Method `ReadNumber` [0x06000009]

- Declaring type: `Program [0x02000005]`
- Attributes: `Private, Static, HideBySig`
- Implementation: `IL`
- Signature: `00 01 08 15 12 08 01 08`
- RVA: `0x000020a8`
- Max stack: `2`
- Init locals: `true`
- Local signature: `(empty)`

```il
IL_0000: ldarg.0
IL_0001: callvirt Box<Int32> [0x1b000005].Read [0x0a00000c]
IL_0006: call System.Console [0x01000007].WriteLine [0x0a00000d]
IL_000b: ldarg.0
IL_000c: ldc.i4.7
IL_000d: callvirt Box<Int32> [0x1b000005].Echo [0x0a00000e]
IL_0012: call System.Console [0x01000007].WriteLine [0x0a00000d]
IL_0017: ldarg.0
IL_0018: isinst IntBox [0x02000004]
IL_001d: ldnull
IL_001e: cgt.un
IL_0020: call System.Console [0x01000007].WriteLine [0x0a00000f]
IL_0025: ldarg.0
IL_0026: isinst TaggedBox<String, Int32> [0x1b000002]
IL_002b: ldnull
IL_002c: cgt.un
IL_002e: call System.Console [0x01000007].WriteLine [0x0a00000f]
IL_0033: ldarg.0
IL_0034: isinst TaggedBox<String, Int32> [0x1b000002]
IL_0039: ldfld TaggedBox<String, Int32> [0x1b000002].Tag [0x0a000010]
IL_003e: call System.Console [0x01000007].WriteLine [0x0a000011]
IL_0043: ldarg.0
IL_0044: callvirt Box<Int32> [0x1b000005].Read [0x0a00000c]
IL_0049: ret
```

#### Method `ReadText` [0x0600000a]

- Declaring type: `Program [0x02000005]`
- Attributes: `Private, Static, HideBySig`
- Implementation: `IL`
- Signature: `00 01 01 15 12 08 01 0E`
- RVA: `0x000020fe`
- Max stack: `8`
- Init locals: `false`
- Local signature: `(empty)`

```il
IL_0000: ldarg.0
IL_0001: callvirt Box<String> [0x1b000006].Read [0x0a000012]
IL_0006: call System.Console [0x01000007].WriteLine [0x0a000011]
IL_000b: ldarg.0
IL_000c: ldstr "World" [0x7000000f]
IL_0011: callvirt Box<String> [0x1b000006].Echo [0x0a000013]
IL_0016: call System.Console [0x01000007].WriteLine [0x0a000011]
IL_001b: ldarg.0
IL_001c: isinst TaggedBox<Int32, String> [0x1b000007]
IL_0021: ldnull
IL_0022: cgt.un
IL_0024: call System.Console [0x01000007].WriteLine [0x0a00000f]
IL_0029: ldarg.0
IL_002a: isinst TaggedBox<String, String> [0x1b000008]
IL_002f: ldnull
IL_0030: ceq
IL_0032: call System.Console [0x01000007].WriteLine [0x0a00000f]
IL_0037: ret
```

#### Method `Main` [0x0600000b]

- Declaring type: `Program [0x02000005]`
- Attributes: `Public, Static, HideBySig`
- Implementation: `IL`
- Signature: `00 00 08`
- RVA: `0x00002138`
- Max stack: `3`
- Init locals: `true`
- Local signature: `07 01 08`

```il
IL_0000: ldc.i4.s 40
IL_0002: ldc.i4.2
IL_0003: newobj IntBox..ctor [0x06000007]
IL_0008: dup
IL_0009: call Program.ReadNumber [0x06000009]
IL_000e: stloc.0
IL_000f: ldstr "Hello" [0x7000001b]
IL_0014: ldc.i4.3
IL_0015: newobj TaggedBox<Int32, String> [0x1b000007]..ctor [0x0a000014]
IL_001a: call Program.ReadText [0x0600000a]
IL_001f: dup
IL_0020: ldc.i4.s 50
IL_0022: stfld Box<Int32> [0x1b000005].Value [0x0a000015]
IL_0027: callvirt Box<Int32> [0x1b000005].Read [0x0a00000c]
IL_002c: call System.Console [0x01000007].WriteLine [0x0a00000d]
IL_0031: ldloc.0
IL_0032: ret
```

### String literals

- 0x70000001: `"number"`
- 0x7000000f: `"World"`
- 0x7000001b: `"Hello"`
