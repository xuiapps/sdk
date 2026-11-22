# XuiNet DotNet unit tests

The assembly and instruction unit tests live here. `AssemblySnapshotTests` compiles
shared assembly and struct-layout fixtures plus two console demos from
`Tests/Fixtures/Assemblies` and `Tests/Fixtures/Structs` with Roslyn, opens each PE through XuiNet, and compares a depth-first Markdown dump
against a checked-in snapshot. Each of the three original assembly library fixtures has four declared types,
including a class, a struct, and an interface.

| Fixture | Coverage | Snapshot |
| --- | --- | --- |
| [Contracts](Fixtures/Assemblies/Contracts.cs) | Interfaces, readonly fields, constructors, nested classes, Unicode strings | [Assembly dump](Snapshots/Contracts.md) |
| [ControlFlow](Fixtures/Assemblies/ControlFlow.cs) | Switch targets, loops, catch/finally regions, numeric operands | [Assembly dump](Snapshots/ControlFlow.md) |
| [Inheritance](Fixtures/Assemblies/Inheritance.cs) | Abstract and virtual methods, overrides, structs, ref/out signatures | [Assembly dump](Snapshots/Inheritance.md) |
| [HelloWorldMain](Fixtures/Assemblies/HelloWorldMain.cs) | Integer entry point, assembly references, and the `Console.WriteLine(string)` member reference | [Assembly dump](Snapshots/HelloWorldMain.md) |
| [PointVector](Fixtures/Structs/PointVector.cs) | The shared Point/Vector REPL: nfloat fields, constructors, addition operator, NFloat conversions, and field-access IL | [Assembly dump](Snapshots/PointVector.md) |
| [Layouts](Fixtures/Structs/Layouts.cs) | Sequential padding, packing, nested structs, explicit overlaps, declared sizes, enums, and target-native storage | [Assembly dump](Snapshots/Layouts.md) |

The visitor uses XuiNet's public views and typed instruction readers. It visits
assembly → module → declared type → fields/methods → nested types in metadata order.
Reference edges are printed as names and tokens rather than recursively following
links back into already visited declarations. Every type must be visited once.

The dump includes all information currently exposed by these views: identities,
references, flags, signature bytes, method bodies and locals, IL operands, exception
regions, and decoded string literals. Signatures remain lossless hexadecimal blobs;
properties, events, custom attributes, and generic parameter declarations do not yet
have dedicated XuiNet views. The MVID is checked for a nonempty value and printed as
`<normalized>` so compiler/runtime identity differences do not cause snapshot churn.
Line endings and numeric formatting are normalized as well.

Structs and enums also show computed alignment and size; their instance fields
show byte offsets and sizes. These use XuiNet's `LayoutResolver` with the explicit
WASM32 target, including four-byte NFloat storage even when the tests run on a
64-bit host. Unsupported layouts display the reason without aborting the metadata
dump. The layout tests additionally compare supported scalar layouts with CLR
sizes and blittable field offsets.

Run from the repository root:

```sh
dotnet test Xui/Compiler/Tests/Xui.Compiler.Tests.csproj -c Release --filter FullyQualifiedName~XuiNet.DotNet
```

A mismatch fails the test and writes a sibling `*.received.md` for review. Missing
snapshots fail too. To intentionally regenerate the expected files:

```sh
XUI_UPDATE_SNAPSHOTS=1 dotnet test Xui/Compiler/Tests/Xui.Compiler.Tests.csproj -c Release --filter FullyQualifiedName~AssemblySnapshotTests
```

Review the Markdown diff, then rerun without `XUI_UPDATE_SNAPSHOTS`. Test output also
contains the full Markdown dump.

[Boxes](Snapshots/Boxes.md) shows a generic class declaration, its indexed parameter, constructed `TypeSpec` signatures, and `MemberRef` uses. `GenericTypeTests` also resolve those references with concrete contexts and verify that repeated decoding preserves type equality and stack-state interning.
