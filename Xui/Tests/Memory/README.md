# Memory tests

Run the dedicated suite separately from functional and snapshot tests:

```sh
dotnet test Xui/Tests/Memory/Xui.Tests.Memory.csproj -c Debug
```

`Regression/` contains the allocation tests moved out of Core, Component and
Integration, preserving their warmups and budgets. Shared partial fixtures are
source-linked from those projects. These low-level regression tests still execute
in the test process. They are serialized and are intentionally kept separate from
functional/snapshot suites; allocation failures belong here.

`StdioProcessTest` launches a separate managed probe process through the production
DevTools stdio transport. It checks known small allocations, zero-allocation
callbacks, reset/read boundaries, ordered input, pointer cancellation, child
environment settings, logging isolation, protocol errors, and process cleanup.
The probe has a queued UI thread and software surface and needs no desktop window.
It does not establish a native-renderer allocation baseline.

For a real desktop or emulator app, build that Debug variant first and set
`XUI_MEMORY_APP` to its executable or DLL. For example on macOS:

```sh
dotnet build Xui/Apps/TestApp/TestApp.Emulator.csproj -c Debug
XUI_MEMORY_APP="$PWD/Xui/Apps/TestApp/bin/Debug/net10.0/TestApp.Emulator.dll" \
  dotnet test Xui/Tests/Memory/Xui.Tests.Memory.csproj --no-build \
  --filter Category=NativeMemory --logger 'console;verbosity=detailed'
```

The native test warms 60 frames, resets counters, measures ten frames, and writes
per-frame and aggregate JSON to the test output. Set `XUI_MEMORY_TARGET` to a
visible view ID to click before warmup. Set `XUI_MEMORY_EXPECT_ID` to a destination
view ID when navigating, so a missed click cannot silently profile the home page.
For longer scenarios use `StdioApp` directly and inspected view centers for input.

Set `XUI_MEMORY_MAX_FRAME_BYTES` to enforce a per-frame animation-plus-render budget.
Without a budget the native test captures a baseline and checks protocol accounting;
it does not assume all platforms and pages allocate zero bytes. Native tests are
skipped unless an app path is supplied, and need an interactive display session.
Use separate budgets for each runtime/page. Avoid collecting coverage or screenshots
while establishing an allocation baseline.

For allocation snapshots, build [the native profiler](../../Utils/AllocationProfiler/README.md)
and launch with `AllocationStacks.EnvironmentFor(libraryPath)`. Tests can keep an
empty `.verified.txt` baseline and use:

```csharp
await using var app = await StdioApp.StartAsync(appPath,
    AllocationStacks.EnvironmentFor(nativeProfilerPath));
// Navigate, verify the destination, and warm frames here.
await app.ResetProfileAsync();
await app.FrameAsync();
await AllocationSnapshot.AssertMatchesAsync(app, "Snapshots/MyFrame.verified.txt");
```

A mismatch writes `MyFrame.received.txt` beside the baseline and fails with the
received path. The report groups allocation count/bytes by type, phase and stack;
source locations use `File.cs(line,column)` for IDE navigation. Passing removes any
stale received file. Baselines are never created or updated automatically.
`AllocationStackTest` verifies both an empty warmed frame and a deliberate allocation
whose reported source location must match its allocation statement.

Set `XUI_TEST_ALLOCATION_PROFILER` to the built native library to run stack tests;
without it they are explicitly skipped. The manually dispatched Memory workflow
builds the collector before running these tests. Native UI captures also use this collector when it is set. Supply
`XUI_MEMORY_SNAPSHOT` with a `.verified.txt` path to turn the native frame test into
an allocation snapshot assertion; an empty baseline enforces zero captured
allocations. Without a snapshot path, native tests print the stack log.

See [the stdio protocol](../../Middleware/DevTools/README.md) for command schemas
and measured boundaries. `profile.reset` clears measurements, not the GC heap.
PDB locations identify sequence points; missing symbols are reported rather than
guessed. Native storage, profiler overhead and background-thread allocations are
outside the app UI-thread managed-byte measurements.
