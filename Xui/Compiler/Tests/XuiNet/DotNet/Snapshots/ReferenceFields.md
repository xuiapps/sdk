# Assembly `XuiNet.ReferenceFields`

Source: [ReferenceFields.cs](Fixtures/Structs/ReferenceFields.cs)

- Version: `0.0.0.0`
- Culture: `neutral`
- Flags: `0`
- Hash algorithm: `Sha1`
- Public key: `(empty)`

## Module `XuiNet.ReferenceFields.dll`

- MVID: `<normalized>`
- Layout target: `WASM32`
- Entry point: `none`
- Declarations: `10 types, 13 fields, 1 methods`

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
- 0x01000007: `System.Object; scope: System.Private.CoreLib [0x23000001]`

### Member references

- 0x0a000001: `Method .ctor; parent: System.Runtime.CompilerServices.CompilationRelaxationsAttribute [0x01000001]; signature: 20 01 01 08`
- 0x0a000002: `Method .ctor; parent: System.Runtime.CompilerServices.RuntimeCompatibilityAttribute [0x01000002]; signature: 20 00 01`
- 0x0a000003: `Method .ctor; parent: System.Diagnostics.DebuggableAttribute [0x01000003]; signature: 20 01 01 11 11`
- 0x0a000004: `Method .ctor; parent: System.Runtime.CompilerServices.RefSafetyRulesAttribute [0x01000005]; signature: 20 01 01 08`
- 0x0a000005: `Method .ctor; parent: System.Object [0x01000007]; signature: 20 00 01`

### Module type `<Module>` [0x02000001]

- Namespace: `(empty)`
- Attributes: `NotPublic`
- Base type: `none`
- Is interface: `false`

### Struct `Samples.References.Label` [0x02000002]

- Namespace: `Samples.References`
- Attributes: `Public, SequentialLayout, Sealed, BeforeFieldInit`
- Base type: `System.ValueType [0x01000006]`
- Is interface: `false`
- Storage classification: `ContainsManagedReferences`
- Layout: `Unsupported: Cannot lay out Label.Text: Field storage type String is not supported.`

#### Field `X` [0x04000001]

- Declaring type: `Samples.References.Label [0x02000002]`
- Attributes: `Public`
- Signature: `06 08`

#### Field `Text` [0x04000002]

- Declaring type: `Samples.References.Label [0x02000002]`
- Attributes: `Public`
- Signature: `06 0E`

### Struct `Samples.References.NestedLabel` [0x02000003]

- Namespace: `Samples.References`
- Attributes: `Public, SequentialLayout, Sealed, BeforeFieldInit`
- Base type: `System.ValueType [0x01000006]`
- Is interface: `false`
- Storage classification: `ContainsManagedReferences`
- Layout: `Unsupported: Cannot lay out NestedLabel.Caption: Cannot lay out Label.Text: Field storage type String is not supported.`

#### Field `Caption` [0x04000003]

- Declaring type: `Samples.References.NestedLabel [0x02000003]`
- Attributes: `Public`
- Signature: `06 11 08`

#### Field `Y` [0x04000004]

- Declaring type: `Samples.References.NestedLabel [0x02000003]`
- Attributes: `Public`
- Signature: `06 08`

### Struct `Samples.References.ObjectField` [0x02000004]

- Namespace: `Samples.References`
- Attributes: `Public, SequentialLayout, Sealed, BeforeFieldInit`
- Base type: `System.ValueType [0x01000006]`
- Is interface: `false`
- Storage classification: `ContainsManagedReferences`
- Layout: `Unsupported: Cannot lay out ObjectField.Value: Field storage type Object is not supported.`

#### Field `Value` [0x04000005]

- Declaring type: `Samples.References.ObjectField [0x02000004]`
- Attributes: `Public`
- Signature: `06 1C`

### Struct `Samples.References.ArrayField` [0x02000005]

- Namespace: `Samples.References`
- Attributes: `Public, SequentialLayout, Sealed, BeforeFieldInit`
- Base type: `System.ValueType [0x01000006]`
- Is interface: `false`
- Storage classification: `ContainsManagedReferences`
- Layout: `Unsupported: Cannot lay out ArrayField.Values: Field storage type SZArray is not supported.`

#### Field `Values` [0x04000006]

- Declaring type: `Samples.References.ArrayField [0x02000005]`
- Attributes: `Public`
- Signature: `06 1D 08`

#### Field `Grid` [0x04000007]

- Declaring type: `Samples.References.ArrayField [0x02000005]`
- Attributes: `Public`
- Signature: `06 14 08 02 00 02 00 00`

#### Field `Rows` [0x04000008]

- Declaring type: `Samples.References.ArrayField [0x02000005]`
- Attributes: `Public`
- Signature: `06 1D 1D 08`

### Class `Samples.References.Node` [0x02000006]

- Namespace: `Samples.References`
- Attributes: `Public, BeforeFieldInit`
- Base type: `System.Object [0x01000007]`
- Is interface: `false`

#### Field `Next` [0x04000009]

- Declaring type: `Samples.References.Node [0x02000006]`
- Attributes: `Public`
- Signature: `06 12 18`

#### Method `.ctor` [0x06000001]

- Declaring type: `Samples.References.Node [0x02000006]`
- Attributes: `Public, HideBySig, SpecialName, RTSpecialName`
- Implementation: `IL`
- Signature: `20 00 01`
- RVA: `0x00002050`
- Max stack: `8`
- Init locals: `false`
- Local signature: `(empty)`

```il
IL_0000: ldarg.0
IL_0001: call System.Object [0x01000007]..ctor [0x0a000005]
IL_0006: ret
```

### Interface `Samples.References.INode` [0x02000007]

- Namespace: `Samples.References`
- Attributes: `Public, ClassSemanticsMask, Abstract, BeforeFieldInit`
- Base type: `none`
- Is interface: `true`

### Struct `Samples.References.ClassField` [0x02000008]

- Namespace: `Samples.References`
- Attributes: `Public, SequentialLayout, Sealed, BeforeFieldInit`
- Base type: `System.ValueType [0x01000006]`
- Is interface: `false`
- Storage classification: `ContainsManagedReferences`
- Layout: `Unsupported: Cannot lay out ClassField.Value: Reference-field storage is not supported.`

#### Field `Value` [0x0400000a]

- Declaring type: `Samples.References.ClassField [0x02000008]`
- Attributes: `Public`
- Signature: `06 12 18`

### Struct `Samples.References.InterfaceField` [0x02000009]

- Namespace: `Samples.References`
- Attributes: `Public, SequentialLayout, Sealed, BeforeFieldInit`
- Base type: `System.ValueType [0x01000006]`
- Is interface: `false`
- Storage classification: `ContainsManagedReferences`
- Layout: `Unsupported: Cannot lay out InterfaceField.Value: Reference-field storage is not supported.`

#### Field `Value` [0x0400000b]

- Declaring type: `Samples.References.InterfaceField [0x02000009]`
- Attributes: `Public`
- Signature: `06 12 1C`

### Struct `Samples.References.StaticOnly` [0x0200000a]

- Namespace: `Samples.References`
- Attributes: `Public, SequentialLayout, Sealed, BeforeFieldInit`
- Base type: `System.ValueType [0x01000006]`
- Is interface: `false`
- Storage classification: `LinearMemory`
- Alignment: `4`
- Size: `4`

#### Field `Shared` [0x0400000c]

- Declaring type: `Samples.References.StaticOnly [0x0200000a]`
- Attributes: `Public, Static`
- Signature: `06 0E`

#### Field `Value` [0x0400000d]

- Declaring type: `Samples.References.StaticOnly [0x0200000a]`
- Attributes: `Public`
- Signature: `06 08`
- Offset: `0`
- Size: `4`

### String literals

None.
