# Collections reachability

Source: [Collections.cs](Fixtures/Reachability/Collections.cs)

## Retained requirements

| Node | Kind | Identity |
| --- | --- | --- |
| N0 | Root | `Assembly: HelloWorld (PublicApi)` |
| N1 | Root | `Type: Dictionary (PublicApi)` |
| N2 | Root | `Type: HashSet (PublicApi)` |
| N3 | Root | `Type: Program (PublicApi)` |
| N4 | Type | `Dictionary` |
| N5 | Layout | `Dictionary` |
| N6 | Method | `Dictionary.Read` |
| N7 | Instantiation | `Dictionary` |
| N8 | Method | `Dictionary.Unused` |
| N9 | Method | `Dictionary..ctor` |
| N10 | Field | `Dictionary.Value` |
| N11 | Type | `HashSet` |
| N12 | Layout | `HashSet` |
| N13 | Method | `HashSet.Count` |
| N14 | Instantiation | `HashSet` |
| N15 | Method | `HashSet..ctor` |
| N16 | Type | `Program` |
| N17 | Layout | `Program` |
| N18 | Method | `Program.Main` |
| N19 | Method | `Program.OtherEntry` |
| N20 | Type | `System.Object` |
| N21 | Intrinsic | `System.Object..ctor` |

## Dependencies

| From | To | Reason | IL offset | Condition |
| --- | --- | --- | --- | --- |
| N0 | N1 | Root | — | — |
| N0 | N2 | Root | — | — |
| N0 | N3 | Root | — | — |
| N1 | N4 | Root | — | — |
| N1 | N5 | Root | — | — |
| N1 | N6 | Root | — | — |
| N1 | N7 | Root | — | — |
| N1 | N8 | Root | — | — |
| N1 | N9 | Root | — | — |
| N1 | N10 | Root | — | — |
| N2 | N11 | Root | — | — |
| N2 | N12 | Root | — | — |
| N2 | N13 | Root | — | — |
| N2 | N14 | Root | — | — |
| N2 | N15 | Root | — | — |
| N3 | N16 | Root | — | — |
| N3 | N17 | Root | — | — |
| N3 | N18 | Root | — | — |
| N3 | N19 | Root | — | — |
| N4 | N20 | BaseType | — | — |
| N5 | N4 | Layout | — | — |
| N5 | N10 | InstanceField | — | — |
| N6 | N4 | DeclaringType | — | — |
| N6 | N5 | Layout | — | — |
| N6 | N10 | FieldRead | IL_0001 | — |
| N7 | N5 | Allocation | — | — |
| N8 | N4 | DeclaringType | — | — |
| N8 | N5 | Layout | — | — |
| N9 | N4 | DeclaringType | — | — |
| N9 | N5 | Layout | — | — |
| N9 | N21 | Call | IL_0001 | — |
| N10 | N4 | DeclaringType | — | — |
| N10 | N5 | Layout | — | — |
| N11 | N20 | BaseType | — | — |
| N12 | N11 | Layout | — | — |
| N13 | N11 | DeclaringType | — | — |
| N13 | N12 | Layout | — | — |
| N14 | N12 | Allocation | — | — |
| N15 | N11 | DeclaringType | — | — |
| N15 | N12 | Layout | — | — |
| N15 | N21 | Call | IL_0001 | — |
| N16 | N20 | BaseType | — | — |
| N17 | N16 | Layout | — | — |
| N18 | N16 | DeclaringType | — | — |
| N18 | N9 | Call | IL_0000 | — |
| N18 | N7 | Allocation | IL_0000 | — |
| N18 | N10 | FieldWrite | IL_0008 | — |
| N18 | N6 | Call | IL_000d | — |
| N19 | N16 | DeclaringType | — | — |
| N19 | N15 | Call | IL_0000 | — |
| N19 | N14 | Allocation | IL_0000 | — |
| N19 | N13 | Call | IL_0005 | — |
