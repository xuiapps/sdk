# Collections reachability

Source: [Collections.cs](Fixtures/Reachability/Collections.cs)

## Retained requirements

| Node | Kind | Identity |
| --- | --- | --- |
| N0 | Root | `Type: Program (All)` |
| N1 | Type | `Program` |
| N2 | Layout | `Program` |
| N3 | Method | `Program.Main` |
| N4 | Method | `Program.OtherEntry` |
| N5 | Method | `Program.PrivateEntry` |
| N6 | Type | `System.Object` |
| N7 | Method | `Dictionary..ctor` |
| N8 | Instantiation | `Dictionary` |
| N9 | Field | `Dictionary.Value` |
| N10 | Method | `Dictionary.Read` |
| N11 | Method | `HashSet..ctor` |
| N12 | Instantiation | `HashSet` |
| N13 | Method | `HashSet.Count` |
| N14 | Method | `InternalCollection..ctor` |
| N15 | Instantiation | `InternalCollection` |
| N16 | Method | `InternalCollection.Count` |
| N17 | Type | `Dictionary` |
| N18 | Layout | `Dictionary` |
| N19 | Intrinsic | `System.Object..ctor` |
| N20 | Type | `HashSet` |
| N21 | Layout | `HashSet` |
| N22 | Type | `InternalCollection` |
| N23 | Layout | `InternalCollection` |

## Dependencies

| From | To | Reason | IL offset | Condition |
| --- | --- | --- | --- | --- |
| N0 | N1 | Root | — | — |
| N0 | N2 | Root | — | — |
| N0 | N3 | Root | — | — |
| N0 | N4 | Root | — | — |
| N0 | N5 | Root | — | — |
| N1 | N6 | BaseType | — | — |
| N2 | N1 | Layout | — | — |
| N3 | N1 | DeclaringType | — | — |
| N3 | N7 | Call | IL_0000 | — |
| N3 | N8 | Allocation | IL_0000 | — |
| N3 | N9 | FieldWrite | IL_0008 | — |
| N3 | N10 | Call | IL_000d | — |
| N4 | N1 | DeclaringType | — | — |
| N4 | N11 | Call | IL_0000 | — |
| N4 | N12 | Allocation | IL_0000 | — |
| N4 | N13 | Call | IL_0005 | — |
| N5 | N1 | DeclaringType | — | — |
| N5 | N14 | Call | IL_0000 | — |
| N5 | N15 | Allocation | IL_0000 | — |
| N5 | N16 | Call | IL_0005 | — |
| N7 | N17 | DeclaringType | — | — |
| N7 | N18 | Layout | — | — |
| N7 | N19 | Call | IL_0001 | — |
| N8 | N18 | Allocation | — | — |
| N9 | N17 | DeclaringType | — | — |
| N9 | N18 | Layout | — | — |
| N10 | N17 | DeclaringType | — | — |
| N10 | N18 | Layout | — | — |
| N10 | N9 | FieldRead | IL_0001 | — |
| N11 | N20 | DeclaringType | — | — |
| N11 | N21 | Layout | — | — |
| N11 | N19 | Call | IL_0001 | — |
| N12 | N21 | Allocation | — | — |
| N13 | N20 | DeclaringType | — | — |
| N13 | N21 | Layout | — | — |
| N14 | N22 | DeclaringType | — | — |
| N14 | N23 | Layout | — | — |
| N14 | N19 | Call | IL_0001 | — |
| N15 | N23 | Allocation | — | — |
| N16 | N22 | DeclaringType | — | — |
| N16 | N23 | Layout | — | — |
| N17 | N6 | BaseType | — | — |
| N18 | N17 | Layout | — | — |
| N18 | N9 | InstanceField | — | — |
| N20 | N6 | BaseType | — | — |
| N21 | N20 | Layout | — | — |
| N22 | N6 | BaseType | — | — |
| N23 | N22 | Layout | — | — |
