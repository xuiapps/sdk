#!/bin/sh
set -eu
cd "$(dirname "$0")"
mkdir -p bin
case "$(uname -s)" in
  Darwin)
    /usr/bin/clang++ -std=c++17 -O2 -dynamiclib -fvisibility=hidden AllocationProfiler.cpp -o bin/libXuiAllocationProfiler.dylib
    ;;
  Linux)
    c++ -std=c++17 -O2 -shared -fPIC -fvisibility=hidden AllocationProfiler.cpp -o bin/libXuiAllocationProfiler.so
    ;;
  *) echo 'Use CMake with a C++ toolchain on this platform.' >&2; exit 1 ;;
esac
