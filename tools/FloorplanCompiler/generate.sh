#!/bin/sh
set -eu
cd "$(dirname "$0")/../.."
dotnet run --project tools/FloorplanCompiler/FloorplanCompiler.csproj -c Release -- "$@"
