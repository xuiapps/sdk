# Assembly `XuiNet.Contracts`

Source: [Contracts.cs](Fixtures/Assemblies/Contracts.cs)

- Version: `1.2.3.4`
- Culture: `neutral`
- Flags: `0`
- Hash algorithm: `Sha1`
- Public key: `(empty)`

## Module `XuiNet.Contracts.dll`

- MVID: `<normalized>`
- Layout target: `WASM32`
- Entry point: `none`
- Declarations: `5 types, 3 fields, 5 methods`

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
- 0x01000006: `System.Runtime.CompilerServices.IsReadOnlyAttribute; scope: System.Private.CoreLib [0x23000001]`
- 0x01000007: `System.ValueType; scope: System.Private.CoreLib [0x23000001]`
- 0x01000008: `System.Object; scope: System.Private.CoreLib [0x23000001]`

### Member references

- 0x0a000001: `Method .ctor; parent: System.Runtime.CompilerServices.CompilationRelaxationsAttribute [0x01000001]; signature: 20 01 01 08`
- 0x0a000002: `Method .ctor; parent: System.Runtime.CompilerServices.RuntimeCompatibilityAttribute [0x01000002]; signature: 20 00 01`
- 0x0a000003: `Method .ctor; parent: System.Diagnostics.DebuggableAttribute [0x01000003]; signature: 20 01 01 11 11`
- 0x0a000004: `Method .ctor; parent: System.Runtime.CompilerServices.RefSafetyRulesAttribute [0x01000005]; signature: 20 01 01 08`
- 0x0a000005: `Method .ctor; parent: System.Runtime.CompilerServices.IsReadOnlyAttribute [0x01000006]; signature: 20 00 01`
- 0x0a000006: `Method .ctor; parent: System.Object [0x01000008]; signature: 20 00 01`

### Module type `<Module>` [0x02000001]

- Namespace: `(empty)`
- Attributes: `NotPublic`
- Base type: `none`
- Is interface: `false`

### Interface `Samples.Contracts.IGreeter` [0x02000002]

- Namespace: `Samples.Contracts`
- Attributes: `Public, ClassSemanticsMask, Abstract, BeforeFieldInit`
- Base type: `none`
- Is interface: `true`

#### Method `Greet` [0x06000001]

- Declaring type: `Samples.Contracts.IGreeter [0x02000002]`
- Attributes: `Public, Virtual, HideBySig, VtableLayoutMask, Abstract`
- Implementation: `IL`
- Signature: `20 00 0E`
- Body: `none`

### Struct `Samples.Contracts.Greeting` [0x02000003]

- Namespace: `Samples.Contracts`
- Attributes: `Public, SequentialLayout, Sealed, BeforeFieldInit`
- Base type: `System.ValueType [0x01000007]`
- Is interface: `false`
- Storage classification: `ContainsManagedReferences`
- Layout: `Unsupported: Cannot lay out Greeting.Text: Field storage type String is not supported.`

#### Field `Text` [0x04000001]

- Declaring type: `Samples.Contracts.Greeting [0x02000003]`
- Attributes: `Public, InitOnly`
- Signature: `06 0E`

#### Method `.ctor` [0x06000002]

- Declaring type: `Samples.Contracts.Greeting [0x02000003]`
- Attributes: `Public, HideBySig, SpecialName, RTSpecialName`
- Implementation: `IL`
- Signature: `20 01 01 0E`
- RVA: `0x00002050`
- Max stack: `8`
- Init locals: `false`
- Local signature: `(empty)`

```il
IL_0000: ldarg.0
IL_0001: ldarg.1
IL_0002: stfld Samples.Contracts.Greeting.Text [0x04000001]
IL_0007: ret
```

### Class `Samples.Contracts.Greeter` [0x02000004]

- Namespace: `Samples.Contracts`
- Attributes: `Public, Sealed, BeforeFieldInit`
- Base type: `System.Object [0x01000008]`
- Is interface: `false`
- Interface [0x09000001]: `Samples.Contracts.IGreeter [0x02000002]`

#### Field `greeting` [0x04000002]

- Declaring type: `Samples.Contracts.Greeter [0x02000004]`
- Attributes: `Private, InitOnly`
- Signature: `06 11 0C`

#### Method `.ctor` [0x06000003]

- Declaring type: `Samples.Contracts.Greeter [0x02000004]`
- Attributes: `Public, HideBySig, SpecialName, RTSpecialName`
- Implementation: `IL`
- Signature: `20 01 01 0E`
- RVA: `0x00002059`
- Max stack: `8`
- Init locals: `false`
- Local signature: `(empty)`

```il
IL_0000: ldarg.0
IL_0001: call System.Object [0x01000008]..ctor [0x0a000006]
IL_0006: ldarg.0
IL_0007: ldarg.1
IL_0008: newobj Samples.Contracts.Greeting..ctor [0x06000002]
IL_000d: stfld Samples.Contracts.Greeter.greeting [0x04000002]
IL_0012: ret
```

#### Method `Greet` [0x06000004]

- Declaring type: `Samples.Contracts.Greeter [0x02000004]`
- Attributes: `Public, Final, Virtual, HideBySig, VtableLayoutMask`
- Implementation: `IL`
- Signature: `20 00 0E`
- RVA: `0x0000206d`
- Max stack: `8`
- Init locals: `false`
- Local signature: `(empty)`

```il
IL_0000: ldarg.0
IL_0001: ldflda Samples.Contracts.Greeter.greeting [0x04000002]
IL_0006: ldfld Samples.Contracts.Greeting.Text [0x04000001]
IL_000b: ret
```

#### Class `Samples.Contracts.Greeter+Options` [0x02000005]

- Namespace: `(empty)`
- Attributes: `NestedPublic, Sealed, BeforeFieldInit`
- Base type: `System.Object [0x01000008]`
- Is interface: `false`

##### Field `Prefix` [0x04000003]

- Declaring type: `Samples.Contracts.Greeter+Options [0x02000005]`
- Attributes: `Public`
- Signature: `06 0E`

##### Method `.ctor` [0x06000005]

- Declaring type: `Samples.Contracts.Greeter+Options [0x02000005]`
- Attributes: `Public, HideBySig, SpecialName, RTSpecialName`
- Implementation: `IL`
- Signature: `20 00 01`
- RVA: `0x0000207a`
- Max stack: `8`
- Init locals: `false`
- Local signature: `(empty)`

```il
IL_0000: ldarg.0
IL_0001: ldstr "Hello | Здравей\nWorld" [0x70000001]
IL_0006: stfld Samples.Contracts.Greeter+Options.Prefix [0x04000003]
IL_000b: ldarg.0
IL_000c: call System.Object [0x01000008]..ctor [0x0a000006]
IL_0011: ret
```

### String literals

- 0x70000001: `"Hello | Здравей\nWorld"`
