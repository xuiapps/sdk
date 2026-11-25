# Command-line control

All repository desktop and emulator apps include DevTools in Debug builds. Launch
with `--enable-stdio-jsonrpc` to use stdin/stdout directly, without MCP or pipe
connection setup. Without the switch, the existing named-pipe transport remains
available. Release, mobile and browser app builds do not enable this switch.

Build first, then launch the built app. Do not mix `dotnet run` build output into
the protocol stream:

```sh
dotnet build Xui/Apps/TestApp/TestApp.Desktop.csproj -c Debug
dotnet Xui/Apps/TestApp/bin/Debug/net10.0/TestApp.Desktop.dll --enable-stdio-jsonrpc
```

Send one JSON-RPC 2.0 object per line. Stdout contains responses only; application
`Console` output goes to stderr. Requests execute sequentially. Input responses
acknowledge UI-thread delivery, and frame responses acknowledge completed app
rendering. Request IDs can be numbers or strings. Notifications have no response.
Malformed lines return errors without disconnecting. Legacy pipe request envelopes
without `jsonrpc` are still accepted. Batch arrays are not supported.

```json
{"jsonrpc":"2.0","id":1,"method":"ui.inspect"}
{"jsonrpc":"2.0","id":2,"method":"app.frame","params":{"elapsedMilliseconds":16}}
{"jsonrpc":"2.0","id":3,"method":"profile.reset"}
{"jsonrpc":"2.0","id":4,"method":"app.frame","params":{"elapsedMilliseconds":16}}
{"jsonrpc":"2.0","id":5,"method":"profile.read"}
{"jsonrpc":"2.0","id":6,"method":"app.quit"}
```

| Method | Parameters / result |
| --- | --- |
| `ui.inspect` | View tree with IDs and window-coordinate centers; `no-window` until startup finishes |
| `input.click`, `input.tap`, `input.mousemove` | Required `x`, `y` |
| `input.pointer` | Required `phase` (`start`, `move`, `end`, `cancel`), `x`, `y`; optional `index` |
| `app.frame` | Optional `elapsedMilliseconds` (0–60,000, default 16); returns `frame`, `animationBytes`, `renderBytes` |
| `profile.reset` | Clears counters; does not force GC, reset animation time, or clear app caches |
| `profile.read` | Cumulative `frames`, `animationBytes`, `renderBytes`, `inputBytes` since reset |
| `ui.screenshot` | SVG capture; excluded from frame measurements |
| `app.invalidate` | Schedules a repaint without advancing the controlled clock |
| `app.identify` | Required `label`; visual pointer/identity overlays are disabled in stdio mode |
| `app.quit` | Flushes the response then exits the run loop; closing stdin also requests exit |

In stdio mode, app animation time advances only through `app.frame`. OS paints
can still occur, but only explicitly requested frames enter the allocation totals.
A frame runs the app animation callback on the UI thread, invalidates its window,
and waits for the next real render callback. Use successive frames to finish
navigation and warm drawing caches before resetting counters. Inspect the destination
before measuring; input delivery alone does not prove successful navigation.

The counters bracket the application's animation, input and render callbacks.
They exclude JSON parsing, dispatcher closures, response construction, DevTools
overlays, and screenshots. They include managed allocations in drawing APIs called
by those callbacks. They do **not** cover native renderer setup/teardown outside the
callbacks, emulator chrome outside the app window, other threads, native memory,
retained heap size. The default counters do not include object types or call stacks;
the optional native collector described below adds them. The counters use
[`GC.GetAllocatedBytesForCurrentThread`](https://learn.microsoft.com/en-us/dotnet/api/system.gc.getallocatedbytesforcurrentthread?view=net-10.0),
so even a small allocation contributes to the byte total.

`Xui.Middleware.DevTools.Client.StdioApp` launches a built executable or DLL with
`ProcessStartInfo`, redirects all three streams, waits for a window, bounds request
time, captures recent stderr, and cleans up the process on failure or disposal.
Its `environment` argument applies child-only environment overrides before launch.
For exact allocation stacks use
`AllocationStacks.EnvironmentFor(nativeProfilerPath)` from the client and build
[the native test profiler](../../Utils/AllocationProfiler/README.md). After warmup,
`profile.reset` arms its scoped capture. `profile.read` additionally includes an
`allocationTrace` payload when that profiler is enabled; the client method
`ReadAllocationLogAsync` resolves it to text with portable-PDB file, line and column
locations. The log is empty when the capture contains no allocations.

The native collector uses
[`ObjectAllocated` and synchronous stack snapshots](https://learn.microsoft.com/en-us/dotnet/framework/unmanaged-api/profiling/icorprofilerinfo2-dostacksnapshot-method),
not sampled allocation events. Missing symbols and capture truncation are reported
explicitly. Loading the collector changes JIT/profiling behavior, so use the default
counter mode for lightweight frame budgets and opt into stacks for diagnostics.

Current window selection follows the existing DevTools behavior: commands target
the most recently created app window. Native frame completion requires a live,
renderable window; client timeouts report a failure rather than inventing a frame.
