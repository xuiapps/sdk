#!/bin/sh
set -eu
# Build a directly installable TestApp APK. Optional overrides use standard SDK variables.
root=$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd)
configuration=${1:-Debug}
if [ "$#" -gt 0 ]; then shift; fi
case $(uname -s) in
  Darwin)
    sdk=${ANDROID_SDK_ROOT:-"$HOME/Library/Android/sdk"}
    jdk=${XUI_JAVA_HOME:-$(/usr/libexec/java_home -v 21)}
    ;;
  *)
    sdk=${ANDROID_SDK_ROOT:?Set ANDROID_SDK_ROOT}
    jdk=${XUI_JAVA_HOME:-${JAVA_HOME:?Set JAVA_HOME to JDK 21}}
    ;;
esac
ndk=${ANDROID_NDK_HOME:-}
if [ -z "$ndk" ]; then
  for candidate in "$sdk"/ndk/*; do
    if [ -d "$candidate/toolchains/llvm" ]; then ndk=$candidate; fi
  done
fi
if [ -z "$ndk" ]; then echo 'Install an Android NDK or set ANDROID_NDK_HOME.' >&2; exit 1; fi
dotnet build "$root/Xui/Apps/TestApp/TestApp.Mobile.csproj" -f net10.0-android \
  -c "$configuration" -p:RuntimeIdentifier=android-arm64 -p:EmbedAssembliesIntoApk=true \
  "-p:JavaSdkDirectory=$jdk" "-p:AndroidSdkDirectory=$sdk" "-p:AndroidNdkDirectory=$ndk" \
  -m:1 -nr:false "$@"
