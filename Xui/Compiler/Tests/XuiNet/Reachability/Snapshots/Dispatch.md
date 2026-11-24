# Dispatch reachability

Source: [Dispatch.cs](Fixtures/Reachability/Dispatch.cs)

## Retained requirements

| Node | Kind | Identity |
| --- | --- | --- |
| N0 | Root | `Method: Program.Main` |
| N1 | Method | `Program.Main` |
| N2 | Type | `Program` |
| N3 | Method | `Program.Create` |
| N4 | Field | `Box<Int32>.Value` |
| N5 | Method | `Program.Read` |
| N6 | Type | `System.Object` |
| N7 | Type | `Box<Int32>` |
| N8 | Layout | `Box<Int32>` |
| N9 | Method | `IntBox..ctor` |
| N10 | Instantiation | `IntBox` |
| N11 | VirtualCall | `Box<Int32>.Read` |
| N12 | Type | `IntBox` |
| N13 | Layout | `IntBox` |
| N14 | Method | `Box<Int32>..ctor` |
| N15 | Method | `Box<Int32>.Read` |
| N16 | Method | `IntBox.Read` |
| N17 | Intrinsic | `System.Object..ctor` |

## Dependencies

| From | To | Reason | IL offset | Condition |
| --- | --- | --- | --- | --- |
| N0 | N1 | Root | — | — |
| N1 | N2 | DeclaringType | — | — |
| N1 | N3 | Call | IL_0000 | — |
| N1 | N4 | FieldWrite | IL_0008 | — |
| N1 | N5 | Call | IL_000d | — |
| N2 | N6 | BaseType | — | — |
| N3 | N2 | DeclaringType | — | — |
| N3 | N7 | Signature | — | — |
| N3 | N8 | Layout | — | — |
| N3 | N9 | Call | IL_0000 | — |
| N3 | N10 | Allocation | IL_0000 | — |
| N4 | N7 | DeclaringType | — | — |
| N4 | N8 | Layout | — | — |
| N5 | N2 | DeclaringType | — | — |
| N5 | N7 | Signature | — | — |
| N5 | N8 | Layout | — | — |
| N5 | N11 | VirtualCall | IL_0001 | — |
| N7 | N6 | BaseType | — | — |
| N8 | N7 | Layout | — | — |
| N8 | N4 | InstanceField | — | — |
| N9 | N12 | DeclaringType | — | — |
| N9 | N13 | Layout | — | — |
| N9 | N14 | Call | IL_0001 | — |
| N10 | N13 | Allocation | — | — |
| N11 | N15 | VirtualCall | — | — |
| N11 | N8 | Layout | — | — |
| N11 | N16 | VirtualImplementation | — | N10 |
| N12 | N7 | BaseType | — | — |
| N12 | N8 | Layout | — | — |
| N13 | N12 | Layout | — | — |
| N13 | N8 | BaseType | — | — |
| N14 | N7 | DeclaringType | — | — |
| N14 | N8 | Layout | — | — |
| N14 | N17 | Call | IL_0001 | — |
| N15 | N7 | DeclaringType | — | — |
| N15 | N8 | Layout | — | — |
| N15 | N4 | FieldRead | IL_0001 | — |
| N16 | N12 | DeclaringType | — | — |
| N16 | N13 | Layout | — | — |
| N16 | N4 | FieldRead | IL_0001 | — |
