# The same assembly step is used locally and by the Pages workflow.
param([switch]$SkipDocs)
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
Push-Location $repo
function Invoke-Checked {
    param([string]$Command, [string[]]$Arguments)
    & $Command @Arguments
    if ($LASTEXITCODE -ne 0) { throw "$Command failed with exit code $LASTEXITCODE" }
}
try {
    if (-not $SkipDocs) {
        Invoke-Checked dotnet @('test', 'Xui/Tests/Docs/Xui.Tests.Docs.csproj', '--configuration', 'Release')
        Invoke-Checked docfx @('metadata', 'docfx.json')
        Invoke-Checked docfx @('build', 'docfx.json')
    }
    Invoke-Checked dotnet @('publish', 'Xui/Apps/TestApp/TestApp.Browser.csproj', '--configuration', 'Release',
        '-p:RunAOTCompilation=true', '-p:WasmStripILAfterAOT=true', '--output', 'artifacts/testapp-wasm')
    Invoke-Checked dotnet @('publish', 'Xui/Compiler/Playground/Xui.Compiler.Playground.csproj', '--configuration', 'Release',
        '--output', 'artifacts/playground')

    # Replace generated WASM output so obsolete fingerprinted files cannot survive.
    if (Test-Path '_site/wasm') { Remove-Item '_site/wasm' -Recurse -Force }
    New-Item -ItemType Directory -Force '_site/wasm/compiler' | Out-Null
    Copy-Item 'artifacts/testapp-wasm/wwwroot/*' '_site/wasm' -Recurse -Force
    Copy-Item 'artifacts/playground/wwwroot/*' '_site/wasm/compiler' -Recurse -Force
    # Preserve existing bookmarks to the compiler editor and the former TestApp URL.
    Set-Content '_site/wasm/playground.html' '<!doctype html><meta charset="utf-8"><meta http-equiv="refresh" content="0;url=compiler/playground.html"><title>Xui compiler playground</title><a href="compiler/playground.html">Open compiler playground</a>'
    Set-Content '_site/wasm/testapp.html' '<!doctype html><meta charset="utf-8"><meta http-equiv="refresh" content="0;url=./"><title>Xui TestApp</title><a href="./">Open TestApp</a>'
    New-Item -ItemType File -Force '_site/.nojekyll' | Out-Null
} finally { Pop-Location }
