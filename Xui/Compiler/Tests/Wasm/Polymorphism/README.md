# Class polymorphism

[Units](Snapshots/Units.md) compiles `Unit → Scout → Veteran`, calls virtual methods through a `Unit` parameter, reads inherited inline `Point` fields, and tests `is`/`as` against `Scout` and the sibling `Medic`. The snapshot includes source, output, exit code, runtime type/field/slot metadata, and WAT. The [assembly snapshot](../../XuiNet/DotNet/Snapshots/Units.md) shows the corresponding PE metadata and MSIL. Integer coordinates isolate storage and dispatch from external `NFloat` operators.

Tests compare CLR and Node execution for Debug and Release compilations. Cases cover abstract methods, inherited implementations, overloads, `new virtual` slots, base calls, dispatch during construction, nominal identity for identical sibling layouts, nulls, casts, conditional reference joins, argument widths, and GC struct argument copies. Invalid casts and null virtual receivers trap in WASM; catchable .NET exception objects are not implemented.

Interfaces, generic types, external class hierarchies, boxing, general `object` storage, explicit MethodImpl mappings, and covariant return overrides remain outside this first implementation. `Runtime.Type` is compiler metadata; a managed `System.Type` API will build on the emitted descriptors later.
