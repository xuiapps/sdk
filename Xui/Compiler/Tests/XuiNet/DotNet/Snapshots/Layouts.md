# Assembly `XuiNet.Layouts`

Source: [Layouts.cs](Fixtures/Structs/Layouts.cs)

- Version: `0.0.0.0`
- Culture: `neutral`
- Flags: `0`
- Hash algorithm: `Sha1`
- Public key: `(empty)`

## Module `XuiNet.Layouts.dll`

- MVID: `<normalized>`
- Layout target: `WASM32`
- Entry point: `none`
- Declarations: `12 types, 24 fields, 0 methods`

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
- 0x01000007: `System.Enum; scope: System.Private.CoreLib [0x23000001]`
- 0x01000008: `System.Runtime.InteropServices.NFloat; scope: System.Private.CoreLib [0x23000001]`

### Member references

- 0x0a000001: `Method .ctor; parent: System.Runtime.CompilerServices.CompilationRelaxationsAttribute [0x01000001]; signature: 20 01 01 08`
- 0x0a000002: `Method .ctor; parent: System.Runtime.CompilerServices.RuntimeCompatibilityAttribute [0x01000002]; signature: 20 00 01`
- 0x0a000003: `Method .ctor; parent: System.Diagnostics.DebuggableAttribute [0x01000003]; signature: 20 01 01 11 11`
- 0x0a000004: `Method .ctor; parent: System.Runtime.CompilerServices.RefSafetyRulesAttribute [0x01000005]; signature: 20 01 01 08`

### Module type `<Module>` [0x02000001]

- Namespace: `(empty)`
- Attributes: `NotPublic`
- Base type: `none`
- Is interface: `false`

### Struct `Samples.Layout.Padded` [0x02000002]

- Namespace: `Samples.Layout`
- Attributes: `Public, SequentialLayout, Sealed, BeforeFieldInit`
- Base type: `System.ValueType [0x01000006]`
- Is interface: `false`
- Storage classification: `LinearMemory`
- Alignment: `8`
- Size: `24`

#### Field `Tag` [0x04000001]

- Declaring type: `Samples.Layout.Padded [0x02000002]`
- Attributes: `Public`
- Signature: `06 05`
- Offset: `0`
- Size: `1`

#### Field `Value` [0x04000002]

- Declaring type: `Samples.Layout.Padded [0x02000002]`
- Attributes: `Public`
- Signature: `06 0D`
- Offset: `8`
- Size: `8`

#### Field `Count` [0x04000003]

- Declaring type: `Samples.Layout.Padded [0x02000002]`
- Attributes: `Public`
- Signature: `06 06`
- Offset: `16`
- Size: `2`

### Struct `Samples.Layout.Packed` [0x02000003]

- Namespace: `Samples.Layout`
- Attributes: `Public, SequentialLayout, Sealed, BeforeFieldInit`
- Base type: `System.ValueType [0x01000006]`
- Is interface: `false`
- Storage classification: `LinearMemory`
- Packing: `2`
- Alignment: `2`
- Size: `8`

#### Field `Tag` [0x04000004]

- Declaring type: `Samples.Layout.Packed [0x02000003]`
- Attributes: `Public`
- Signature: `06 05`
- Offset: `0`
- Size: `1`

#### Field `Value` [0x04000005]

- Declaring type: `Samples.Layout.Packed [0x02000003]`
- Attributes: `Public`
- Signature: `06 08`
- Offset: `2`
- Size: `4`

#### Field `Code` [0x04000006]

- Declaring type: `Samples.Layout.Packed [0x02000003]`
- Attributes: `Public`
- Signature: `06 06`
- Offset: `6`
- Size: `2`

### Struct `Samples.Layout.Nested` [0x02000004]

- Namespace: `Samples.Layout`
- Attributes: `Public, SequentialLayout, Sealed, BeforeFieldInit`
- Base type: `System.ValueType [0x01000006]`
- Is interface: `false`
- Storage classification: `LinearMemory`
- Alignment: `2`
- Size: `12`

#### Field `Tag` [0x04000007]

- Declaring type: `Samples.Layout.Nested [0x02000004]`
- Attributes: `Public`
- Signature: `06 05`
- Offset: `0`
- Size: `1`

#### Field `Value` [0x04000008]

- Declaring type: `Samples.Layout.Nested [0x02000004]`
- Attributes: `Public`
- Signature: `06 11 0C`
- Offset: `2`
- Size: `8`

#### Field `End` [0x04000009]

- Declaring type: `Samples.Layout.Nested [0x02000004]`
- Attributes: `Public`
- Signature: `06 06`
- Offset: `10`
- Size: `2`

### Struct `Samples.Layout.Union` [0x02000005]

- Namespace: `Samples.Layout`
- Attributes: `Public, ExplicitLayout, Sealed, BeforeFieldInit`
- Base type: `System.ValueType [0x01000006]`
- Is interface: `false`
- Storage classification: `LinearMemory`
- Declared size: `16`
- Alignment: `4`
- Size: `16`

#### Field `Bits` [0x0400000a]

- Declaring type: `Samples.Layout.Union [0x02000005]`
- Attributes: `Public`
- Signature: `06 08`
- Offset: `0`
- Size: `4`

#### Field `Value` [0x0400000b]

- Declaring type: `Samples.Layout.Union [0x02000005]`
- Attributes: `Public`
- Signature: `06 0C`
- Offset: `0`
- Size: `4`

#### Field `Tail` [0x0400000c]

- Declaring type: `Samples.Layout.Union [0x02000005]`
- Attributes: `Public`
- Signature: `06 06`
- Offset: `12`
- Size: `2`

### Struct `Samples.Layout.Sized` [0x02000006]

- Namespace: `Samples.Layout`
- Attributes: `Public, SequentialLayout, Sealed, BeforeFieldInit`
- Base type: `System.ValueType [0x01000006]`
- Is interface: `false`
- Storage classification: `LinearMemory`
- Declared size: `11`
- Alignment: `4`
- Size: `11`

#### Field `Value` [0x0400000d]

- Declaring type: `Samples.Layout.Sized [0x02000006]`
- Attributes: `Public`
- Signature: `06 08`
- Offset: `0`
- Size: `4`

### Struct `Samples.Layout.MinimumSize` [0x02000007]

- Namespace: `Samples.Layout`
- Attributes: `Public, SequentialLayout, Sealed, BeforeFieldInit`
- Base type: `System.ValueType [0x01000006]`
- Is interface: `false`
- Storage classification: `LinearMemory`
- Declared size: `1`
- Alignment: `4`
- Size: `4`

#### Field `Value` [0x0400000e]

- Declaring type: `Samples.Layout.MinimumSize [0x02000007]`
- Attributes: `Public`
- Signature: `06 08`
- Offset: `0`
- Size: `4`

### Struct `Samples.Layout.Empty` [0x02000008]

- Namespace: `Samples.Layout`
- Attributes: `Public, SequentialLayout, Sealed, BeforeFieldInit`
- Base type: `System.ValueType [0x01000006]`
- Is interface: `false`
- Storage classification: `LinearMemory`
- Declared size: `1`
- Alignment: `1`
- Size: `1`

### Enum `Samples.Layout.Kind` [0x02000009]

- Namespace: `Samples.Layout`
- Attributes: `Public, Sealed`
- Base type: `System.Enum [0x01000007]`
- Is interface: `false`
- Storage classification: `LinearMemory`
- Alignment: `2`
- Size: `2`

#### Field `value__` [0x0400000f]

- Declaring type: `Samples.Layout.Kind [0x02000009]`
- Attributes: `Public, SpecialName, RTSpecialName`
- Signature: `06 07`
- Offset: `0`
- Size: `2`

#### Field `First` [0x04000010]

- Declaring type: `Samples.Layout.Kind [0x02000009]`
- Attributes: `Public, Static, Literal, HasDefault`
- Signature: `06 11 24`

#### Field `Second` [0x04000011]

- Declaring type: `Samples.Layout.Kind [0x02000009]`
- Attributes: `Public, Static, Literal, HasDefault`
- Signature: `06 11 24`

### Struct `Samples.Layout.SmallFields` [0x0200000a]

- Namespace: `Samples.Layout`
- Attributes: `Public, SequentialLayout, Sealed, BeforeFieldInit`
- Base type: `System.ValueType [0x01000006]`
- Is interface: `false`
- Storage classification: `LinearMemory`
- Alignment: `2`
- Size: `6`

#### Field `Enabled` [0x04000012]

- Declaring type: `Samples.Layout.SmallFields [0x0200000a]`
- Attributes: `Public`
- Signature: `06 02`
- Offset: `0`
- Size: `1`

#### Field `Character` [0x04000013]

- Declaring type: `Samples.Layout.SmallFields [0x0200000a]`
- Attributes: `Public`
- Signature: `06 03`
- Offset: `2`
- Size: `2`

#### Field `Kind` [0x04000014]

- Declaring type: `Samples.Layout.SmallFields [0x0200000a]`
- Attributes: `Public`
- Signature: `06 11 24`
- Offset: `4`
- Size: `2`

### Struct `Samples.Layout.NativeFields` [0x0200000b]

- Namespace: `Samples.Layout`
- Attributes: `Public, SequentialLayout, Sealed, BeforeFieldInit`
- Base type: `System.ValueType [0x01000006]`
- Is interface: `false`
- Storage classification: `LinearMemory`
- Alignment: `4`
- Size: `8`

#### Field `Address` [0x04000015]

- Declaring type: `Samples.Layout.NativeFields [0x0200000b]`
- Attributes: `Public`
- Signature: `06 18`
- Offset: `0`
- Size: `4`

#### Field `Value` [0x04000016]

- Declaring type: `Samples.Layout.NativeFields [0x0200000b]`
- Attributes: `Public`
- Signature: `06 11 21`
- Offset: `4`
- Size: `4`

### Struct `Samples.Layout.WithStatic` [0x0200000c]

- Namespace: `Samples.Layout`
- Attributes: `Public, SequentialLayout, Sealed, BeforeFieldInit`
- Base type: `System.ValueType [0x01000006]`
- Is interface: `false`
- Storage classification: `LinearMemory`
- Alignment: `4`
- Size: `4`

#### Field `Shared` [0x04000017]

- Declaring type: `Samples.Layout.WithStatic [0x0200000c]`
- Attributes: `Public, Static`
- Signature: `06 0A`

#### Field `Value` [0x04000018]

- Declaring type: `Samples.Layout.WithStatic [0x0200000c]`
- Attributes: `Public`
- Signature: `06 08`
- Offset: `0`
- Size: `4`

### String literals

None.
