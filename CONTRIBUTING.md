Xui Contributions
=================

Docs
----

Install docfx once:
```
dotnet tool install -g docfx
```

Serve locally with live-reload:
```
docfx build docfx.json --serve
```
Opens at http://localhost:8080. The landing page (`index.html`) is at the root;
docs are at `/docs/`, API reference at `/api/`.

> Use the explicit `docfx build` subcommand for a preview without regenerating API metadata.

To regenerate canvas SVG figures (only needed when `Xui/Tests/Docs/Canvas/Views/` changes):
```
dotnet test Xui/Tests/Docs/Xui.Tests.Docs.csproj
docfx build docfx.json --serve
```

### API metadata

To regenerate API metadata from C# source (only needed after public API changes):
```
docfx metadata docfx.json
```

The generated YAML and API navigation land in ignored `www/api/`; regenerate them
from source and do not commit generated metadata.
The full site builds to `_site/` (gitignored) and is deployed via GitHub Actions on push to main.

### Complete website, including WASM demos

Install the .NET 10 `wasm-tools` workload and DocFX, then run the same script as CI:

```sh
dotnet workload install wasm-tools
pwsh -NoProfile -File tools/build-web.ps1
python3 -m http.server 8080 --directory _site
```

This generates documentation figures and API metadata, builds DocFX, publishes
TestApp with .NET WebAssembly AOT and stripped compiled IL at `/wasm/`, and keeps
the custom compiler experiment at `/wasm/compiler/`. Old `/wasm/playground.html`
bookmarks redirect to the compiler editor. `-SkipDocs` refreshes just the WASM
portion of an existing site. `_site/` and `artifacts/` are generated and ignored.

The Web workflow builds and browser-tests both published apps on pull requests.
A push to `main` (including a merged PR) deploys the combined site to GitHub Pages;
PR builds never deploy. See [browser runtime verification](Xui/Runtime/Browser/README.md)
for local smoke tests and performance sampling.

See [`www/docs/PLAN.md`](www/docs/PLAN.md) for the documentation roadmap.

NuGet Packages
--------------

Build all distributable NuGet packages from the repo root:
```
dotnet pack Xui.Pack.slnf -c Release
```

`Xui.Pack.slnf` is a solution filter that lists only the packable library projects.
Using it (instead of `Xui.sln`) avoids restoring app projects that require optional
workloads (`wasm-tools`, etc.) not available on a stock CI runner.

Packages land in `packages/` at the repo root (gitignored). The output path is
configured globally in `Directory.Build.props` via `<PackageOutputPath>`, so you
get the same location regardless of which directory you invoke `dotnet pack` from.

To make a new library packable, do two things:

1. Add `<IsPackable>true</IsPackable>` to its `.csproj` `<PropertyGroup>`.
2. Add its path to the `projects` array in `Xui.Pack.slnf`.

All projects default to `<IsPackable>false</IsPackable>` (set globally in
`Directory.Build.props`), so neither step alone is sufficient.

The CI/CD pipeline (`CD.yml`) runs the same command and uploads the resulting
`*.nupkg` files as artifacts, then pushes them to NuGet.org on merges to `main`.
