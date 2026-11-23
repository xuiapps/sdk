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
| N7 | Initialization | `Program` |
| N8 | Method | `Dictionary..ctor` |
| N9 | Instantiation | `Dictionary` |
| N10 | Field | `Dictionary.Value` |
| N11 | Method | `Dictionary.Read` |
| N12 | Method | `HashSet..ctor` |
| N13 | Instantiation | `HashSet` |
| N14 | Method | `HashSet.Count` |
| N15 | Method | `InternalCollection..ctor` |
| N16 | Instantiation | `InternalCollection` |
| N17 | Method | `InternalCollection.Count` |
| N18 | Type | `Dictionary` |
| N19 | Initialization | `Dictionary` |
| N20 | Layout | `Dictionary` |
| N21 | Intrinsic | `System.Object..ctor` |
| N22 | Type | `HashSet` |
| N23 | Initialization | `HashSet` |
| N24 | Layout | `HashSet` |
| N25 | Type | `InternalCollection` |
| N26 | Initialization | `InternalCollection` |
| N27 | Layout | `InternalCollection` |

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
| N3 | N7 | Initialization | — | — |
| N3 | N8 | Call | IL_0000 | — |
| N3 | N9 | Allocation | IL_0000 | — |
| N3 | N10 | FieldWrite | IL_0008 | — |
| N3 | N11 | Call | IL_000d | — |
| N4 | N1 | DeclaringType | — | — |
| N4 | N7 | Initialization | — | — |
| N4 | N12 | Call | IL_0000 | — |
| N4 | N13 | Allocation | IL_0000 | — |
| N4 | N14 | Call | IL_0005 | — |
| N5 | N1 | DeclaringType | — | — |
| N5 | N7 | Initialization | — | — |
| N5 | N15 | Call | IL_0000 | — |
| N5 | N16 | Allocation | IL_0000 | — |
| N5 | N17 | Call | IL_0005 | — |
| N7 | N1 | Initialization | — | — |
| N8 | N18 | DeclaringType | — | — |
| N8 | N19 | Initialization | — | — |
| N8 | N20 | Layout | — | — |
| N8 | N21 | Call | IL_0001 | — |
| N9 | N20 | Allocation | — | — |
| N9 | N19 | Initialization | — | — |
| N10 | N18 | DeclaringType | — | — |
| N10 | N20 | Layout | — | — |
| N11 | N18 | DeclaringType | — | — |
| N11 | N19 | Initialization | — | — |
| N11 | N20 | Layout | — | — |
| N11 | N10 | FieldRead | IL_0001 | — |
| N12 | N22 | DeclaringType | — | — |
| N12 | N23 | Initialization | — | — |
| N12 | N24 | Layout | — | — |
| N12 | N21 | Call | IL_0001 | — |
| N13 | N24 | Allocation | — | — |
| N13 | N23 | Initialization | — | — |
| N14 | N22 | DeclaringType | — | — |
| N14 | N23 | Initialization | — | — |
| N14 | N24 | Layout | — | — |
| N15 | N25 | DeclaringType | — | — |
| N15 | N26 | Initialization | — | — |
| N15 | N27 | Layout | — | — |
| N15 | N21 | Call | IL_0001 | — |
| N16 | N27 | Allocation | — | — |
| N16 | N26 | Initialization | — | — |
| N17 | N25 | DeclaringType | — | — |
| N17 | N26 | Initialization | — | — |
| N17 | N27 | Layout | — | — |
| N18 | N6 | BaseType | — | — |
| N19 | N18 | Initialization | — | — |
| N20 | N18 | Layout | — | — |
| N20 | N10 | InstanceField | — | — |
| N22 | N6 | BaseType | — | — |
| N23 | N22 | Initialization | — | — |
| N24 | N22 | Layout | — | — |
| N25 | N6 | BaseType | — | — |
| N26 | N25 | Initialization | — | — |
| N27 | N25 | Layout | — | — |
