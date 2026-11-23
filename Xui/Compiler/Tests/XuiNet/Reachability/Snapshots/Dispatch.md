# Dispatch reachability

Source: [Dispatch.cs](Fixtures/Reachability/Dispatch.cs)

## Retained requirements

| Node | Kind | Identity |
| --- | --- | --- |
| N0 | Root | `Method: Program.Main` |
| N1 | Method | `Program.Main` |
| N2 | Type | `Program` |
| N3 | Initialization | `Program` |
| N4 | Method | `Program.Create` |
| N5 | Field | `Box<Int32>.Value` |
| N6 | Method | `Program.Read` |
| N7 | Type | `System.Object` |
| N8 | Type | `Box<Int32>` |
| N9 | Layout | `Box<Int32>` |
| N10 | Method | `IntBox..ctor` |
| N11 | Instantiation | `IntBox` |
| N12 | VirtualCall | `Box<Int32>.Read` |
| N13 | Type | `IntBox` |
| N14 | Initialization | `IntBox` |
| N15 | Layout | `IntBox` |
| N16 | Method | `Box<Int32>..ctor` |
| N17 | Method | `Box<Int32>.Read` |
| N18 | Method | `IntBox.Read` |
| N19 | Initialization | `Box<Int32>` |
| N20 | Intrinsic | `System.Object..ctor` |

## Dependencies

| From | To | Reason | IL offset | Condition |
| --- | --- | --- | --- | --- |
| N0 | N1 | Root | — | — |
| N1 | N2 | DeclaringType | — | — |
| N1 | N3 | Initialization | — | — |
| N1 | N4 | Call | IL_0000 | — |
| N1 | N5 | FieldWrite | IL_0008 | — |
| N1 | N6 | Call | IL_000d | — |
| N2 | N7 | BaseType | — | — |
| N3 | N2 | Initialization | — | — |
| N4 | N2 | DeclaringType | — | — |
| N4 | N3 | Initialization | — | — |
| N4 | N8 | Signature | — | — |
| N4 | N9 | Layout | — | — |
| N4 | N10 | Call | IL_0000 | — |
| N4 | N11 | Allocation | IL_0000 | — |
| N5 | N8 | DeclaringType | — | — |
| N5 | N9 | Layout | — | — |
| N6 | N2 | DeclaringType | — | — |
| N6 | N3 | Initialization | — | — |
| N6 | N8 | Signature | — | — |
| N6 | N9 | Layout | — | — |
| N6 | N12 | VirtualCall | IL_0001 | — |
| N8 | N7 | BaseType | — | — |
| N9 | N8 | Layout | — | — |
| N9 | N5 | InstanceField | — | — |
| N10 | N13 | DeclaringType | — | — |
| N10 | N14 | Initialization | — | — |
| N10 | N15 | Layout | — | — |
| N10 | N16 | Call | IL_0001 | — |
| N11 | N15 | Allocation | — | — |
| N11 | N14 | Initialization | — | — |
| N12 | N17 | VirtualCall | — | — |
| N12 | N9 | Layout | — | — |
| N12 | N18 | VirtualImplementation | — | N11 |
| N13 | N8 | BaseType | — | — |
| N13 | N9 | Layout | — | — |
| N14 | N13 | Initialization | — | — |
| N15 | N13 | Layout | — | — |
| N15 | N9 | BaseType | — | — |
| N16 | N8 | DeclaringType | — | — |
| N16 | N19 | Initialization | — | — |
| N16 | N9 | Layout | — | — |
| N16 | N20 | Call | IL_0001 | — |
| N17 | N8 | DeclaringType | — | — |
| N17 | N19 | Initialization | — | — |
| N17 | N9 | Layout | — | — |
| N17 | N5 | FieldRead | IL_0001 | — |
| N18 | N13 | DeclaringType | — | — |
| N18 | N14 | Initialization | — | — |
| N18 | N15 | Layout | — | — |
| N18 | N5 | FieldRead | IL_0001 | — |
| N19 | N8 | Initialization | — | — |
