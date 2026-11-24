# Assembly `XuiNet.Buffers`

Source: [Buffers.cs](Fixtures/InlineArrays/Buffers.cs)

- Version: `0.0.0.0`
- Culture: `neutral`
- Flags: `0`
- Hash algorithm: `Sha1`
- Public key: `(empty)`

## Module `XuiNet.Buffers.dll`

- MVID: `<normalized>`
- Layout target: `WASM32`
- Entry point: `none`
- Declarations: `5 types, 7 fields, 0 methods`

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
- 0x01000006: `System.ValueType; scope: System.Private.CoreLib [0x23000001]`
- 0x01000007: `System.Runtime.CompilerServices.InlineArrayAttribute; scope: System.Private.CoreLib [0x23000001]`

### Member references

- 0x0a000001: `Method .ctor; parent: System.Runtime.CompilerServices.CompilationRelaxationsAttribute [0x01000001]; signature: 20 01 01 08`
- 0x0a000002: `Method .ctor; parent: System.Runtime.CompilerServices.RuntimeCompatibilityAttribute [0x01000002]; signature: 20 00 01`
- 0x0a000003: `Method .ctor; parent: System.Diagnostics.DebuggableAttribute [0x01000003]; signature: 20 01 01 11 11`
- 0x0a000004: `Method .ctor; parent: System.Runtime.CompilerServices.RefSafetyRulesAttribute [0x01000005]; signature: 20 01 01 08`
- 0x0a000005: `Method .ctor; parent: System.Runtime.CompilerServices.InlineArrayAttribute [0x01000007]; signature: 20 01 01 08`

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
- Signature: `06 06`
- Offset: `0`
- Size: `2`

#### Field `Y` [0x04000002]

- Declaring type: `Point [0x02000002]`
- Attributes: `Public`
- Signature: `06 08`
- Offset: `4`
- Size: `4`

### Struct `Points` [0x02000003]

- Namespace: `(empty)`
- Attributes: `Public, SequentialLayout, Sealed, BeforeFieldInit`
- Base type: `System.ValueType [0x01000006]`
- Is interface: `false`
- Inline array length: `3`
- Storage classification: `LinearMemory`
- Alignment: `4`
- Size: `24`

#### Field `first` [0x04000003]

- Declaring type: `Points [0x02000003]`
- Attributes: `Private`
- Signature: `06 11 08`
- Offset: `0`
- Size: `8`

### Struct `Labels` [0x02000004]

- Namespace: `(empty)`
- Attributes: `Public, SequentialLayout, Sealed, BeforeFieldInit`
- Base type: `System.ValueType [0x01000006]`
- Is interface: `false`
- Inline array length: `4`
- Storage classification: `ContainsManagedReferences`
- Layout: `Unsupported: Field storage type String is not supported.`

#### Field `first` [0x04000004]

- Declaring type: `Labels [0x02000004]`
- Attributes: `Private`
- Signature: `06 0E`

### Struct `Holder` [0x02000005]

- Namespace: `(empty)`
- Attributes: `Public, SequentialLayout, Sealed, BeforeFieldInit`
- Base type: `System.ValueType [0x01000006]`
- Is interface: `false`
- Storage classification: `LinearMemory`
- Alignment: `4`
- Size: `32`

#### Field `Head` [0x04000005]

- Declaring type: `Holder [0x02000005]`
- Attributes: `Public`
- Signature: `06 05`
- Offset: `0`
- Size: `1`

#### Field `Values` [0x04000006]

- Declaring type: `Holder [0x02000005]`
- Attributes: `Public`
- Signature: `06 11 0C`
- Offset: `4`
- Size: `24`

#### Field `Tail` [0x04000007]

- Declaring type: `Holder [0x02000005]`
- Attributes: `Public`
- Signature: `06 05`
- Offset: `28`
- Size: `1`

### String literals

None.
