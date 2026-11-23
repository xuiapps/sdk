# Assembly `XuiNet.Inheritance`

Source: [Inheritance.cs](Fixtures/Assemblies/Inheritance.cs)

- Version: `3.0.1.0`
- Culture: `neutral`
- Flags: `0`
- Hash algorithm: `Sha1`
- Public key: `(empty)`

## Module `XuiNet.Inheritance.dll`

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

### Type references

- 0x01000001: `System.Runtime.CompilerServices.CompilationRelaxationsAttribute; scope: System.Private.CoreLib [0x23000001]`
- 0x01000002: `System.Runtime.CompilerServices.RuntimeCompatibilityAttribute; scope: System.Private.CoreLib [0x23000001]`
- 0x01000003: `System.Diagnostics.DebuggableAttribute; scope: System.Private.CoreLib [0x23000001]`
- 0x01000004: `DebuggingModes; scope: System.Diagnostics.DebuggableAttribute [0x01000003]`
- 0x01000005: `System.Runtime.CompilerServices.RefSafetyRulesAttribute; scope: System.Private.CoreLib [0x23000001]`
- 0x01000006: `System.Object; scope: System.Private.CoreLib [0x23000001]`
- 0x01000007: `System.Runtime.CompilerServices.IsReadOnlyAttribute; scope: System.Private.CoreLib [0x23000001]`
- 0x01000008: `System.ValueType; scope: System.Private.CoreLib [0x23000001]`

### Member references

- 0x0a000001: `Method .ctor; parent: System.Runtime.CompilerServices.CompilationRelaxationsAttribute [0x01000001]; signature: 20 01 01 08`
- 0x0a000002: `Method .ctor; parent: System.Runtime.CompilerServices.RuntimeCompatibilityAttribute [0x01000002]; signature: 20 00 01`
- 0x0a000003: `Method .ctor; parent: System.Diagnostics.DebuggableAttribute [0x01000003]; signature: 20 01 01 11 11`
- 0x0a000004: `Method .ctor; parent: System.Runtime.CompilerServices.RefSafetyRulesAttribute [0x01000005]; signature: 20 01 01 08`
- 0x0a000005: `Method .ctor; parent: System.Runtime.CompilerServices.IsReadOnlyAttribute [0x01000007]; signature: 20 00 01`
- 0x0a000006: `Method .ctor; parent: System.Object [0x01000006]; signature: 20 00 01`

### Module type `<Module>` [0x02000001]

- Namespace: `(empty)`
- Attributes: `NotPublic`
- Base type: `none`
- Is interface: `false`

### Interface `Samples.Inheritance.IValue` [0x02000002]

- Namespace: `Samples.Inheritance`
- Attributes: `Public, ClassSemanticsMask, Abstract, BeforeFieldInit`
- Base type: `none`
- Is interface: `true`

#### Method `GetValue` [0x06000001]

- Declaring type: `Samples.Inheritance.IValue [0x02000002]`
- Attributes: `Public, Virtual, HideBySig, VtableLayoutMask, Abstract`
- Implementation: `IL`
- Signature: `20 00 08`
- Body: `none`

### Class `Samples.Inheritance.ValueSource` [0x02000003]

- Namespace: `Samples.Inheritance`
- Attributes: `Public, Abstract, BeforeFieldInit`
- Base type: `System.Object [0x01000006]`
- Is interface: `false`
- Interface [0x09000001]: `Samples.Inheritance.IValue [0x02000002]`

#### Method `GetValue` [0x06000002]

- Declaring type: `Samples.Inheritance.ValueSource [0x02000003]`
- Attributes: `Public, Virtual, HideBySig, VtableLayoutMask, Abstract`
- Implementation: `IL`
- Signature: `20 00 08`
- Body: `none`

#### Method `Describe` [0x06000003]

- Declaring type: `Samples.Inheritance.ValueSource [0x02000003]`
- Attributes: `Public, Virtual, HideBySig, VtableLayoutMask`
- Implementation: `IL`
- Signature: `20 00 0E`
- RVA: `0x00002050`
- Max stack: `8`
- Init locals: `false`
- Local signature: `(empty)`

```il
IL_0000: ldstr "value" [0x70000001]
IL_0005: ret
```

#### Method `.ctor` [0x06000004]

- Declaring type: `Samples.Inheritance.ValueSource [0x02000003]`
- Attributes: `Family, HideBySig, SpecialName, RTSpecialName`
- Implementation: `IL`
- Signature: `20 00 01`
- RVA: `0x00002057`
- Max stack: `8`
- Init locals: `false`
- Local signature: `(empty)`

```il
IL_0000: ldarg.0
IL_0001: call System.Object [0x01000006]..ctor [0x0a000006]
IL_0006: ret
```

### Struct `Samples.Inheritance.Result` [0x02000004]

- Namespace: `Samples.Inheritance`
- Attributes: `Public, SequentialLayout, Sealed, BeforeFieldInit`
- Base type: `System.ValueType [0x01000008]`
- Is interface: `false`
- Storage classification: `LinearMemory`
- Alignment: `4`
- Size: `4`

#### Field `Value` [0x04000001]

- Declaring type: `Samples.Inheritance.Result [0x02000004]`
- Attributes: `Public, InitOnly`
- Signature: `06 08`
- Offset: `0`
- Size: `4`

#### Method `.ctor` [0x06000005]

- Declaring type: `Samples.Inheritance.Result [0x02000004]`
- Attributes: `Public, HideBySig, SpecialName, RTSpecialName`
- Implementation: `IL`
- Signature: `20 01 01 08`
- RVA: `0x0000205f`
- Max stack: `8`
- Init locals: `false`
- Local signature: `(empty)`

```il
IL_0000: ldarg.0
IL_0001: ldarg.1
IL_0002: stfld Samples.Inheritance.Result.Value [0x04000001]
IL_0007: ret
```

### Class `Samples.Inheritance.FixedValue` [0x02000005]

- Namespace: `Samples.Inheritance`
- Attributes: `Public, Sealed, BeforeFieldInit`
- Base type: `Samples.Inheritance.ValueSource [0x02000003]`
- Is interface: `false`

#### Field `value` [0x04000002]

- Declaring type: `Samples.Inheritance.FixedValue [0x02000005]`
- Attributes: `Private, InitOnly`
- Signature: `06 08`

#### Method `.ctor` [0x06000006]

- Declaring type: `Samples.Inheritance.FixedValue [0x02000005]`
- Attributes: `Public, HideBySig, SpecialName, RTSpecialName`
- Implementation: `IL`
- Signature: `20 01 01 08`
- RVA: `0x00002068`
- Max stack: `8`
- Init locals: `false`
- Local signature: `(empty)`

```il
IL_0000: ldarg.0
IL_0001: call Samples.Inheritance.ValueSource..ctor [0x06000004]
IL_0006: ldarg.0
IL_0007: ldarg.1
IL_0008: stfld Samples.Inheritance.FixedValue.value [0x04000002]
IL_000d: ret
```

#### Method `GetValue` [0x06000007]

- Declaring type: `Samples.Inheritance.FixedValue [0x02000005]`
- Attributes: `Public, Virtual, HideBySig`
- Implementation: `IL`
- Signature: `20 00 08`
- RVA: `0x00002077`
- Max stack: `8`
- Init locals: `false`
- Local signature: `(empty)`

```il
IL_0000: ldarg.0
IL_0001: ldfld Samples.Inheritance.FixedValue.value [0x04000002]
IL_0006: ret
```

#### Method `Describe` [0x06000008]

- Declaring type: `Samples.Inheritance.FixedValue [0x02000005]`
- Attributes: `Public, Virtual, HideBySig`
- Implementation: `IL`
- Signature: `20 00 0E`
- RVA: `0x0000207f`
- Max stack: `8`
- Init locals: `false`
- Local signature: `(empty)`

```il
IL_0000: ldstr "fixed" [0x7000000d]
IL_0005: ret
```

#### Method `Create` [0x06000009]

- Declaring type: `Samples.Inheritance.FixedValue [0x02000005]`
- Attributes: `Public, Static, HideBySig`
- Implementation: `IL`
- Signature: `00 01 11 10 08`
- RVA: `0x00002086`
- Max stack: `8`
- Init locals: `false`
- Local signature: `(empty)`

```il
IL_0000: ldarg.0
IL_0001: newobj Samples.Inheritance.Result..ctor [0x06000005]
IL_0006: ret
```

#### Method `Copy` [0x0600000a]

- Declaring type: `Samples.Inheritance.FixedValue [0x02000005]`
- Attributes: `Public, Static, HideBySig`
- Implementation: `IL`
- Signature: `00 02 01 10 11 10 10 11 10`
- RVA: `0x0000208e`
- Max stack: `8`
- Init locals: `false`
- Local signature: `(empty)`

```il
IL_0000: ldarg.1
IL_0001: ldarg.0
IL_0002: ldobj Samples.Inheritance.Result [0x02000004]
IL_0007: stobj Samples.Inheritance.Result [0x02000004]
IL_000c: ret
```

### String literals

- 0x70000001: `"value"`
- 0x7000000d: `"fixed"`
