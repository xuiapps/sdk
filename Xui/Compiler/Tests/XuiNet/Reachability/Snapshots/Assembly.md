# Collections reachability

Source: [Collections.cs](Fixtures/Reachability/Collections.cs)

## Retained requirements

| Node | Kind | Identity |
| --- | --- | --- |
| N0 | Root | `Assembly: HelloWorld (All)` |
| N1 | Root | `Type: Dictionary (All)` |
| N2 | Root | `Type: HashSet (All)` |
| N3 | Root | `Type: InternalCollection (All)` |
| N4 | Root | `Type: Program (All)` |
| N5 | Type | `Dictionary` |
| N6 | Layout | `Dictionary` |
| N7 | Method | `Dictionary.Read` |
| N8 | Instantiation | `Dictionary` |
| N9 | Method | `Dictionary.Unused` |
| N10 | Method | `Dictionary..ctor` |
| N11 | Field | `Dictionary.Value` |
| N12 | Type | `HashSet` |
| N13 | Layout | `HashSet` |
| N14 | Method | `HashSet.Count` |
| N15 | Instantiation | `HashSet` |
| N16 | Method | `HashSet..ctor` |
| N17 | Type | `InternalCollection` |
| N18 | Layout | `InternalCollection` |
| N19 | Method | `InternalCollection.Count` |
| N20 | Instantiation | `InternalCollection` |
| N21 | Method | `InternalCollection..ctor` |
| N22 | Type | `Program` |
| N23 | Layout | `Program` |
| N24 | Method | `Program.Main` |
| N25 | Method | `Program.OtherEntry` |
| N26 | Method | `Program.PrivateEntry` |
| N27 | Type | `System.Object` |
| N28 | Intrinsic | `System.Object..ctor` |

## Dependencies

| From | To | Reason | IL offset | Condition |
| --- | --- | --- | --- | --- |
| N0 | N1 | Root | — | — |
| N0 | N2 | Root | — | — |
| N0 | N3 | Root | — | — |
| N0 | N4 | Root | — | — |
| N1 | N5 | Root | — | — |
| N1 | N6 | Root | — | — |
| N1 | N7 | Root | — | — |
| N1 | N8 | Root | — | — |
| N1 | N9 | Root | — | — |
| N1 | N10 | Root | — | — |
| N1 | N11 | Root | — | — |
| N2 | N12 | Root | — | — |
| N2 | N13 | Root | — | — |
| N2 | N14 | Root | — | — |
| N2 | N15 | Root | — | — |
| N2 | N16 | Root | — | — |
| N3 | N17 | Root | — | — |
| N3 | N18 | Root | — | — |
| N3 | N19 | Root | — | — |
| N3 | N20 | Root | — | — |
| N3 | N21 | Root | — | — |
| N4 | N22 | Root | — | — |
| N4 | N23 | Root | — | — |
| N4 | N24 | Root | — | — |
| N4 | N25 | Root | — | — |
| N4 | N26 | Root | — | — |
| N5 | N27 | BaseType | — | — |
| N6 | N5 | Layout | — | — |
| N6 | N11 | InstanceField | — | — |
| N7 | N5 | DeclaringType | — | — |
| N7 | N6 | Layout | — | — |
| N7 | N11 | FieldRead | IL_0001 | — |
| N8 | N6 | Allocation | — | — |
| N9 | N5 | DeclaringType | — | — |
| N9 | N6 | Layout | — | — |
| N10 | N5 | DeclaringType | — | — |
| N10 | N6 | Layout | — | — |
| N10 | N28 | Call | IL_0001 | — |
| N11 | N5 | DeclaringType | — | — |
| N11 | N6 | Layout | — | — |
| N12 | N27 | BaseType | — | — |
| N13 | N12 | Layout | — | — |
| N14 | N12 | DeclaringType | — | — |
| N14 | N13 | Layout | — | — |
| N15 | N13 | Allocation | — | — |
| N16 | N12 | DeclaringType | — | — |
| N16 | N13 | Layout | — | — |
| N16 | N28 | Call | IL_0001 | — |
| N17 | N27 | BaseType | — | — |
| N18 | N17 | Layout | — | — |
| N19 | N17 | DeclaringType | — | — |
| N19 | N18 | Layout | — | — |
| N20 | N18 | Allocation | — | — |
| N21 | N17 | DeclaringType | — | — |
| N21 | N18 | Layout | — | — |
| N21 | N28 | Call | IL_0001 | — |
| N22 | N27 | BaseType | — | — |
| N23 | N22 | Layout | — | — |
| N24 | N22 | DeclaringType | — | — |
| N24 | N10 | Call | IL_0000 | — |
| N24 | N8 | Allocation | IL_0000 | — |
| N24 | N11 | FieldWrite | IL_0008 | — |
| N24 | N7 | Call | IL_000d | — |
| N25 | N22 | DeclaringType | — | — |
| N25 | N16 | Call | IL_0000 | — |
| N25 | N15 | Allocation | IL_0000 | — |
| N25 | N14 | Call | IL_0005 | — |
| N26 | N22 | DeclaringType | — | — |
| N26 | N21 | Call | IL_0000 | — |
| N26 | N20 | Allocation | IL_0000 | — |
| N26 | N19 | Call | IL_0005 | — |
