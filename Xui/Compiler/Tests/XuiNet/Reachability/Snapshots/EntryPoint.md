# Collections reachability

Source: [Collections.cs](Fixtures/Reachability/Collections.cs)

## Retained requirements

| Node | Kind | Identity |
| --- | --- | --- |
| N0 | Root | `Method: Program.Main` |
| N1 | Method | `Program.Main` |
| N2 | Type | `Program` |
| N3 | Initialization | `Program` |
| N4 | Method | `Dictionary..ctor` |
| N5 | Instantiation | `Dictionary` |
| N6 | Field | `Dictionary.Value` |
| N7 | Method | `Dictionary.Read` |
| N8 | Type | `System.Object` |
| N9 | Type | `Dictionary` |
| N10 | Initialization | `Dictionary` |
| N11 | Layout | `Dictionary` |
| N12 | Intrinsic | `System.Object..ctor` |

## Dependencies

| From | To | Reason | IL offset | Condition |
| --- | --- | --- | --- | --- |
| N0 | N1 | Root | — | — |
| N1 | N2 | DeclaringType | — | — |
| N1 | N3 | Initialization | — | — |
| N1 | N4 | Call | IL_0000 | — |
| N1 | N5 | Allocation | IL_0000 | — |
| N1 | N6 | FieldWrite | IL_0008 | — |
| N1 | N7 | Call | IL_000d | — |
| N2 | N8 | BaseType | — | — |
| N3 | N2 | Initialization | — | — |
| N4 | N9 | DeclaringType | — | — |
| N4 | N10 | Initialization | — | — |
| N4 | N11 | Layout | — | — |
| N4 | N12 | Call | IL_0001 | — |
| N5 | N11 | Allocation | — | — |
| N5 | N10 | Initialization | — | — |
| N6 | N9 | DeclaringType | — | — |
| N6 | N11 | Layout | — | — |
| N7 | N9 | DeclaringType | — | — |
| N7 | N10 | Initialization | — | — |
| N7 | N11 | Layout | — | — |
| N7 | N6 | FieldRead | IL_0001 | — |
| N9 | N8 | BaseType | — | — |
| N10 | N9 | Initialization | — | — |
| N11 | N9 | Layout | — | — |
| N11 | N6 | InstanceField | — | — |
