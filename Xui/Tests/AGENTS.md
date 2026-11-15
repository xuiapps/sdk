# Xui tests

Use the narrowest test layer that exercises the changed behavior. Follow the
repository-root `AGENTS.md` first.

## Choose the test layer

- Core unit tests: `Xui/Core/Core.Tests`. Use for math, curves, layout algorithms,
  event logic, GPU types/IR/backends, and other isolated framework behavior.
- DevKit unit tests: `Xui/DevKit/UI.Tests` and `Xui/DevKit/ThreeD.Tests`. Use for
  their corresponding higher-level libraries.
- Component tests: `Xui/Tests/Component`. Render an individual view or component
  deterministically, typically through `SvgDrawingContext`, and compare SVG or
  image output.
- Integration tests: `Xui/Tests/Integration`. Boot the real TestApp application
  through `TestSinglePageApp`, drive input/animation synchronously, and compare
  multi-step scenarios without an OS window.
- Design system suite: `Xui/Tests/Integration/DesignSystem`, within the Integration
  project. Owns palette math, retained-token
  and allocation checks, widget invalidation, and deterministic mobile design-system
  scenarios. SVG galleries and their README specifications live beside the tests.
- E2E tests: `Xui/Tests/E2E`. Launch a real Debug desktop app and control it
  through the DevTools named-pipe protocol. Use only when a real platform
  process/window or DevTools behavior matters.
- Docs generators: `Xui/Tests/Docs`. These xUnit methods generate committed
  documentation figures; they are not visual-regression assertions.

Prefer unit or component coverage over integration, and integration over E2E,
unless the lower layer cannot observe the behavior under test.

## Deterministic rendering

- Use `Xui.Runtime.Software.Actual.SvgDrawingContext` for display-independent 2D
  output.
- Pass `Xui.Core.Fonts.Inter.URIs` whenever text layout or metrics affect output.
- Dispose `SvgDrawingContext` before reading its stream; disposal flushes the
  closing SVG content. Use `keepOpen: true` when the stream must remain readable.
- Set the complete `LayoutGuide` pass and explicit size/alignment values; do not
  depend on a host display.
- Use `TestSinglePageApp` clocks, input methods, runtime variants, and emulator
  profiles rather than wall-clock sleeps or real input in integration tests.
- Keep culture-sensitive formatting invariant in snapshot-producing code.
- Integration and E2E test projects disable xUnit assembly/collection
  parallelism. Do not re-enable it without proving the shared process, pipe, and
  snapshot state are isolated.

## Snapshot workflow

Committed baselines are part of the test:

- Component baselines are under `Xui/Tests/Component/**/Snapshots`.
  A mismatch commonly writes `*.Actual.svg`.
- Integration scenarios are stored beside the test under `Scenarios/` or a
  device-specific `Scenarios.Emulator.*` directory. Each scenario contains
  numbered SVG baselines and a `README.md`.
- Integration mismatches produce `*.DIFF.svg` and `README.DIFF.md`.
- E2E diagnostics are written under `Xui/Tests/E2E/TestApp/TestResults`.
- Documentation figures are generated into `www/docs/img`.

Never accept or overwrite a baseline merely to make a test pass:

1. Read the code change and the textual SVG/image diff.
2. Inspect the rendered output when the visual meaning is not obvious.
3. Confirm that every changed device variant and scenario step is intentional.
4. Promote the new baseline and remove temporary Actual/DIFF artifacts only
   after that review.

Do not commit `*.Actual.svg`, `*.DIFF.svg`, or `README.DIFF.md` as baselines.
When adding a snapshot, use stable semantic names and keep its baseline beside
the test that owns it.

## Integration tests

- Instantiate `TestSinglePageApp<TApplication,TWindow>` with an explicit window
  size and dispose it so comparisons and reports are finalized.
- Use `IntegrationRuntimeVariants` for existing desktop and phone-form-factor
  coverage instead of duplicating device setup.
- Locate views through stable IDs or typed hierarchy queries. Prefer the harness
  overloads that target a view center for pointer input.
- Advance animations through `AnimationFrame`; avoid `Task.Delay`.
- A change to TestApp navigation, IDs, layout, safe areas, text metrics, focus,
  or input may legitimately affect multiple scenario sets. Review all of them.

## E2E tests

- `RealApp` runs `dotnet run --project <app> -c Debug`, waits for
  `DEVTOOLS_READY:<pipe>`, and captures stdout/stderr.
- Debug desktop app composition must include DevTools middleware or startup will
  time out.
- Await `WaitForWindowAsync` before inspection or input.
- Use inspected center coordinates for clicks/taps rather than guessed values.
- Always dispose `RealApp` and `TestLog`, including on failure.
- Treat E2E as platform-dependent: it requires process launch, local IPC, and a
  display/session. Consult the captured markdown/log before changing timeouts.

## Commands

Run focused suites from the repository root:

```sh
dotnet test Xui/Core/Core.Tests/Xui.Core.Tests.csproj
dotnet test Xui/DevKit/UI.Tests/Xui.DevKit.UI.Tests.csproj
dotnet test Xui/DevKit/ThreeD.Tests/Xui.DevKit.ThreeD.Tests.csproj
dotnet test Xui/Tests/Component/Xui.Tests.Component.csproj
dotnet test Xui/Tests/Integration/Xui.Tests.Integration.csproj
dotnet test Xui/Tests/E2E/Xui.Tests.E2E.csproj
dotnet test Xui/Tests/Docs/Xui.Tests.Docs.csproj
```

CI runs Core, component, and integration suites on Windows. E2E has a separate
manually dispatched Windows/macOS workflow. Match the CI command before
attributing a local-only failure to the code.
