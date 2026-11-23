# Xui apps

App directories contain shared application source plus small host projects for
the supported environments. Follow the repository-root `AGENTS.md` first.

## Composition model

Most apps have three kinds of build file:

- `<App>.<Variant>.csproj` selects the SDK/target framework and imports the two
  target files.
- `Xui.targets` selects Core, middleware, and runtime references from the host OS,
  target framework, WebAssembly SDK, and `XuiDevEmulator`.
- `<App>.targets` supplies app identity, compiler settings, packages, and
  app-specific project references.

Shared files such as `Application.cs`, `MainWindow.cs`, `Program.cs`, and
`Platform.cs` compile into multiple variants. Platform-host resources live under
`Platforms`, `Properties`, `Resources`, or `wwwroot` and are conditionally
excluded by `Xui.targets`.

Current app families:

- `BlankApp`: Desktop, Emulator, Browser, and Mobile.
- `TestApp`: Desktop, Emulator, and Mobile.
- `LoadTestApp`: Desktop, Emulator, Browser, and Mobile.
- `XuiSDK`: Desktop, Emulator, and Mobile.
- `ClockApp`: Desktop only.

## Runtime selection

`Xui.targets` defines the symbols consumed by `Platform.cs`:

- Desktop selects `MACOS` or `WINDOWS` from the host OS.
- Emulator adds `EMULATOR` and wraps that desktop runtime in
  `EmulatorPlatform`.
- Mobile selects `IOS` or `ANDROID` from the target framework.
- Browser selects `BROWSER` from the WebAssembly SDK.
- All app families define `DEVTOOLS` for Debug desktop and emulator builds;
  their `Platform.cs` wraps the selected runtime in `DevToolsPlatform`.
  `--enable-stdio-jsonrpc` selects direct stdin/stdout control instead of the
  default named-pipe transport. See `Xui/Middleware/DevTools/README.md`.

Keep MSBuild conditions, project references, symbols, and the `Platform.cs`
branches synchronized. If a common `Xui.targets` behavior changes, inspect every
app's copy: these files are currently duplicated and may drift.

Do not put ordinary shared UI behavior behind platform conditionals. Keep native
host startup/resources under `Platforms`, and keep runtime selection centralized
in `Platform.cs`.

## App-specific roles

- BlankApp is the minimal playground and should remain a small example of normal
  application structure.
- TestApp is both a feature showcase and a test fixture. Component and
  integration projects reference its desktop project and source types.
- LoadTestApp exercises rendering throughput, scrolling, animation timing, and
  memory/performance behavior; avoid changes that invalidate its workload
  characteristics accidentally.
- ClockApp is a low-level window/render-loop example.
- XuiSDK explores the higher-level SDK/design experience.

Changes to TestApp IDs, class names, navigation order, text, geometry, examples,
or interaction states can affect component, integration, and E2E tests. Search
`Xui/Tests` for consumers before renaming or removing them.

## Adding or changing a variant

1. Inspect another app with the same variant.
2. Keep the variant project small; put shared settings in the app targets.
3. Add the correct runtime and middleware conditions to `Xui.targets`.
4. Add or update the matching `Platform.cs` branch.
5. Check platform resources and exclusion globs.
6. Add the project to `Xui.sln` only when it is intended as a maintained solution
   project.
7. Build the new variant and at least one existing sibling variant.

Mobile projects are MAUI single-project hosts targeting `net10.0-android` and
`net10.0-ios`; they require the corresponding workloads. Browser projects use
the WebAssembly SDK and may require `wasm-tools`, especially for AOT builds.

## Validation

Use the app that owns the change:

```sh
dotnet build Xui/Apps/BlankApp/BlankApp.Desktop.csproj
dotnet build Xui/Apps/BlankApp/BlankApp.Emulator.csproj
dotnet build Xui/Apps/TestApp/TestApp.Desktop.csproj
dotnet build Xui/Apps/TestApp/TestApp.Emulator.csproj
dotnet build Xui/Apps/LoadTestApp/LoadTestApp.Browser.csproj
dotnet build Xui/Apps/TestApp/TestApp.Mobile.csproj -f net10.0-android
dotnet build Xui/Apps/TestApp/TestApp.Mobile.csproj -f net10.0-ios
```

Run TestApp-related suites from `Xui/Tests/AGENTS.md` when shared TestApp content
or behavior changes. A successful compile of one variant does not validate the
conditional branches of its siblings.
