#!/bin/sh
set -eu
# Run component tests first to generate AndroidFixtures. Requires an ARM64 device/emulator.
# Usage: sh run.sh SDK NDK JDK [fixtures-directory]
sdk=$1
ndk=$2
jdk=$3
here=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
root=$(CDPATH= cd -- "$here/../../../../.." && pwd)
runtime="$root/Xui/Runtime/Android"
fixtures=${4:-"$root/Xui/Tests/Component/bin/Debug/net10.0/AndroidFixtures"}
out="$root/Xui/Tests/Component/obj/android-native-tests"
buildtools="$sdk/build-tools/35.0.0"
mkdir -p "$out/classes" "$out/apk/lib/arm64-v8a" "$out/apk/assets"
"$jdk/bin/javac" --release 17 -cp "$sdk/platforms/android-35/android.jar" -d "$out/classes" "$runtime"/Java/xui/runtime/*.java "$here/CompositionSmoke.java"
"$jdk/bin/jar" cf "$out/classes.jar" -C "$out/classes" .
JAVA_HOME="$jdk" "$buildtools/d8" --min-api 29 --lib "$sdk/platforms/android-35/android.jar" --output "$out/apk" "$out/classes.jar"
case $(uname -s) in Darwin) host=darwin-x86_64;; Linux) host=linux-x86_64;; *) exit 1;; esac
"$ndk/toolchains/llvm/prebuilt/$host/bin/clang" --target=aarch64-linux-android29 -shared -fPIC -O2 -Wall -Wextra -Werror \
  -Wl,-z,max-page-size=16384 -I "$runtime/Native" "$runtime/Native/xui_android.c" "$runtime/Native/xui_vulkan.c" "$here/bridge_test.c" -landroid -lvulkan -o "$out/apk/lib/arm64-v8a/libxui_android_test.so"
cp "$fixtures"/*.bin "$out/apk/assets/"
# The application build compiles and validates the C# generator's actual cube shader.
spirv="$root/Xui/Apps/TestApp/obj/Debug/net10.0-android/android-arm64/xui-spirv"
cp "$spirv"/*.VSMain.spv "$out/apk/assets/vertex.spv"
cp "$spirv"/*.PSMain.spv "$out/apk/assets/fragment.spv"
"$buildtools/aapt2" link -I "$sdk/platforms/android-35/android.jar" --manifest "$here/AndroidManifest.xml" -o "$out/test.apk" -A "$out/apk/assets"
(cd "$out/apk" && zip -q "$out/test.apk" classes.dex lib/arm64-v8a/libxui_android_test.so)
if [ ! -f "$out/test.keystore" ]; then
  "$jdk/bin/keytool" -genkeypair -keystore "$out/test.keystore" -storepass android -keypass android -alias android -dname CN=XuiTest -keyalg RSA -validity 30 >/dev/null 2>&1
fi
JAVA_HOME="$jdk" "$buildtools/apksigner" sign --ks "$out/test.keystore" --ks-pass pass:android "$out/test.apk"
"$sdk/platform-tools/adb" install -r "$out/test.apk"
"$sdk/platform-tools/adb" shell am force-stop xui.runtime.compositiontest
"$sdk/platform-tools/adb" logcat -c
"$sdk/platform-tools/adb" shell am start -W -n xui.runtime.compositiontest/xui.runtime.CompositionSmoke
# Bounded polling: failures include the complete Java stack in the log.
for attempt in 1 2 3 4 5 6 7 8 9 10; do
  "$sdk/platform-tools/adb" logcat -d -s XuiCompositionTest:I AndroidRuntime:E > "$out/result.log"
  if grep -q 'PASS:' "$out/result.log"; then cat "$out/result.log"; exit 0; fi
  if grep -q 'FAIL\|FATAL EXCEPTION' "$out/result.log"; then cat "$out/result.log"; exit 1; fi
  sleep 1
done
cat "$out/result.log"
exit 1
