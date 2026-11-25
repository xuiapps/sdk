# Collections reachability

Source: [Collections.cs](Fixtures/Reachability/Collections.cs)

## Retained requirements

| Node | Kind | Identity |
| --- | --- | --- |
| N0 | Root | `Method: Program.Main` |
| N1 | Method | `Program.Main` |
| N2 | Type | `Program` |
| N3 | Method | `Dictionary..ctor` |
| N4 | Instantiation | `Dictionary` |
| N5 | Field | `Dictionary.Value` |
| N6 | Method | `Dictionary.Read` |
| N7 | Type | `System.Object` |
| N8 | Type | `Dictionary` |
| N9 | Layout | `Dictionary` |
| N10 | Intrinsic | `System.Object..ctor` |

## Dependencies

| From | To | Reason | IL offset | Condition |
| --- | --- | --- | --- | --- |
| N0 | N1 | Root | — | — |
| N1 | N2 | DeclaringType | — | — |
| N1 | N3 | Call | IL_0000 | — |
| N1 | N4 | Allocation | IL_0000 | — |
| N1 | N5 | FieldWrite | IL_0008 | — |
| N1 | N6 | Call | IL_000d | — |
| N2 | N7 | BaseType | — | — |
| N3 | N8 | DeclaringType | — | — |
| N3 | N9 | Layout | — | — |
| N3 | N10 | Call | IL_0001 | — |
| N4 | N9 | Allocation | — | — |
| N5 | N8 | DeclaringType | — | — |
| N5 | N9 | Layout | — | — |
| N6 | N8 | DeclaringType | — | — |
| N6 | N9 | Layout | — | — |
| N6 | N5 | FieldRead | IL_0001 | — |
| N8 | N7 | BaseType | — | — |
| N9 | N8 | Layout | — | — |
| N9 | N5 | InstanceField | — | — |
