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

> **Important:** use `docfx build`, not `docfx` (without subcommand). The bare
> `docfx docfx.json --serve` also runs `metadata`, which overwrites the
> hand-maintained `www/api/toc.yml` with an auto-generated one.

To regenerate canvas SVG figures (only needed when `Xui/Tests/Docs/Canvas/Views/` changes):
```
dotnet test Xui/Tests/Docs/Xui.Tests.Docs.csproj
docfx build docfx.json --serve
```

### API metadata

To regenerate API metadata from C# source (only needed after public API changes):
```
docfx metadata docfx.json
git checkout -- www/api/toc.yml
```

The generated YAML lands in `www/api/` and is committed alongside the source.
After regenerating, commit the changed `.yml` files but not `toc.yml`.
The full site builds to `_site/` (gitignored) and is deployed via GitHub Actions on push to main.

See [`www/docs/PLAN.md`](www/docs/PLAN.md) for the documentation roadmap.
