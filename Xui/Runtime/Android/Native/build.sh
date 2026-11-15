#!/bin/sh
set -eu
# No runtime hosting dependency: the same library works with .NET Android or Mono.
ndk=$1
out=$2
arch=$3
host=$(uname -s | tr '[:upper:]' '[:lower:]')
case "$host" in darwin) host=darwin-x86_64;; linux) host=linux-x86_64;; *) echo 'Use the NDK clang toolchain on macOS or Linux' >&2; exit 1;; esac
case "$arch" in arm64-v8a) target=aarch64-linux-android29;; x86_64) target=x86_64-linux-android29;; *) echo "Unsupported Android ABI: $arch" >&2; exit 1;; esac
mkdir -p "$out/$arch"
"$ndk/toolchains/llvm/prebuilt/$host/bin/clang" --target="$target" -shared -fPIC -O2 -Wall -Wextra -Werror \
  -Wl,-z,max-page-size=16384 -Wl,-soname,libxui_android.so "$(dirname "$0")/xui_android.c" "$(dirname "$0")/xui_vulkan.c" -lvulkan -landroid -o "$out/$arch/libxui_android.so"
