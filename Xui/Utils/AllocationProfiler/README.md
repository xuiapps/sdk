# Exact allocation-stack profiler

This opt-in native CLR profiler records individual managed allocations inside
DevTools input, animation and render scopes. It is a diagnostic tool, not a
performance benchmark: enabling allocation callbacks, disabling ReadyToRun/tiered
compilation, and preserving callers by disabling inlining changes execution cost.
The normal Debug stdio mode does not load this library.

Build on macOS/Linux with a C++17 compiler:

```sh
Xui/Utils/AllocationProfiler/build.sh
```

Or use CMake (including Windows with Visual Studio's C++ tools):

```sh
cmake -S Xui/Utils/AllocationProfiler -B Xui/Utils/AllocationProfiler/bin/native
cmake --build Xui/Utils/AllocationProfiler/bin/native --config Release
```

The library is `libXuiAllocationProfiler.dylib` on macOS,
`libXuiAllocationProfiler.so` on Linux, or `Release/XuiAllocationProfiler.dll`
with the Windows multi-configuration generator. Only 64-bit runtimes are supported.

Launch a built Debug app through `StdioApp.StartAsync(appPath,
AllocationStacks.EnvironmentFor(libraryPath))`. This sets the profiler's CLSID and
library path **before** process startup. Navigate and warm the app, call
`ResetProfileAsync`, perform the input/frame operations to measure, then call
`ReadAllocationLogAsync`. The log is empty if the enabled capture observed no
allocations. Missing profiler initialization fails startup instead of returning an
apparently empty capture.

The collector implements `ICorProfilerCallback2.ObjectAllocated` and synchronously
calls `ICorProfilerInfo2.DoStackSnapshot` on the allocating thread. Native storage
keeps collection outside the app's managed heap. Begin/end scopes are thread-local;
transport, dispatcher setup, screenshots and response formatting are outside them.
Allocation count/size are exact within those scopes; background threads, unmanaged
allocations and renderer work outside the app callbacks are excluded.

The client resolves metadata tokens and IL offsets using the matching assembly and
portable PDB (adjacent or embedded, identity checked by `PEReader`). Reports contain
`/path/File.cs(line,column): Type.Method [IL_offset]`, suitable for IDE navigation.
Locations are PDB sequence points; multiple expressions on one statement may share
one location. Missing PDBs, dynamic methods, unmapped frames and truncated stacks
are explicitly labeled. This implementation resolves local symbols; it does not
download Source Link documents or symbol-server PDBs.

Captures are bounded to 4,096 allocations and 64 managed frames per allocation.
Overflow and stack-walk failures produce nonempty diagnostics, so an empty snapshot
cannot pass by silently dropping captured allocations. Reset discards old records;
it does not collect the GC heap or clear app caches. Read does not clear records.
Native callback interface declarations derive from .NET 10's MIT-licensed
`corprof.idl`; see `ProfilerAbi.h` and `LICENSE.txt`.

Run the exact stack and source-location tests on macOS:

```sh
XUI_TEST_ALLOCATION_PROFILER="$PWD/Xui/Utils/AllocationProfiler/bin/libXuiAllocationProfiler.dylib" \
  dotnet test Xui/Tests/Memory/Xui.Tests.Memory.csproj -c Debug \
  --filter 'FullyQualifiedName~AllocationStackTest'
```
