# Android composition

The renderer is a plain `net10.0` assembly referencing only Core. C# owns the Xui
view tree, `RetainedCanvasContext`, keyed `AndroidGraphicsNode` objects and the
command buffers. A small C ABI forwards batched updates through JNI to Java-owned
Android views. Drawing uses public `android.graphics.Canvas` APIs; there is no
reflection into `mNativeCanvasWrapper`, dependency on private Skia symbols, or
bitmap snapshot of a composition group.

```text
Xui views → RetainedCanvasContext → AndroidCompositionPresenter
                                      ↓ one frame packet
                                C ABI → JNI → Host
                                               ↓
                                       Surface ViewGroup
                                               ↓ onDraw when invalidated
                                         CanvasPlayer
                                               ↓
                                 Android hardware display list
```

Only backed groups create native views. Ordinary groups remain drawing commands.
Views are reused by stable node IDs. Transform-only updates preserve Android's
recorded drawing. Even when a caller requests updates, equal command bytes do not
invalidate the native display list. Hidden nodes detach but retain their content;
the existing bounded Core cache determines when their resources are released.

Each surface draws its content before its children. Child elevation is isolated in
separate canvas Z blocks to preserve declared sibling order. Clipping to rounded
bounds uses the native outline. Shadows use native elevation, so radius and offset
are an approximation of `OutlineShadow`, not an exact CSS shadow implementation.
Backdrop materials currently use a translucent approximation, not backdrop blur.

## Ownership and protocol

`Composition/WireBuffer` is reusable pooled managed storage. The JNI submission
channel is reusable C-owned memory exposed to Java as a direct ByteBuffer. Calls
consume that channel synchronously. A surface copies changed drawing bytes into
its own direct buffer before returning; it never holds a pointer into a pooled or
temporarily pinned managed array. Resource registration uses the same channel.

Protocol v1 is little-endian, with int32 identifiers/enums/lengths, float32 drawing
scalars, and length-prefixed UTF-16 text. It does not expose CLR struct layout or
native-sized `nfloat` fields. `DrawOp.cs` and `DrawOp.java` have explicit values and
a conformance test. Incompatible changes require a protocol version change.

A frame contains magic `0x58554931`, version, removed IDs, then ordered nodes. Each
node has a 136-byte header (identity, parent/order, bounds, transform, density,
clip/outline/material, border/shadow, content length), followed by drawing bytes.
Content length `-1` means reuse; `-2` declares a native GPU leaf without Canvas commands. Parent IDs precede children. Font/image IDs are
window-scoped; the managed resource list never crosses JNI as object pointers.

String text measurements use a window-scoped 128-entry LRU before encoding or
crossing JNI. Keys include string identity, the complete font, alignment and
baseline. Reused labels and string literals hit without character hashing;
distinct strings with equal contents remain distinct keys. Span measurements
bypass the cache without allocating strings. Cache storage is fixed, eviction
allocates nothing, and disposing the context releases its retained references.

All scene operations and callbacks run on the Android UI thread. JNI global
references belong to the host and are released at destruction. The C ABI catches
Java failures; managed callbacks catch exceptions before returning to JNI. The
platform Choreographer supplies frame times and display refresh rate. Pointer
packets preserve IDs and distinguish down/up for the changed pointer, moves for
other pointers, and cancellation for all contacts.

## Status-bar appearance

Resolve the optional window service `Xui.Core.Actual.IStatusBar` and set
`Appearance` to `StatusBarAppearance.Auto`, `Light`, or `Dark` on the UI thread.
Light/Dark describe the foreground icons and text, not the background. Auto
follows the system light/dark theme; it does not inspect canvas pixels.

Bridge operation 9 carries one int32 appearance (0 = Auto, 1 = Light, 2 = Dark).
Android uses `WindowInsetsController` on API 30+ and the decor view's light-status-bar
flag on API 29, preserving other system-bar flags. The host reapplies appearance
when attached, focused, or reconfigured. The API leaves safe areas and backgrounds
unchanged. The Java host configures edge-to-edge layout, a transparent status-bar
background, and disables the system status-bar contrast scrim, including on API 29.
iOS implements the same service through its root view controller;
the emulator renders the selected foreground and desktop hosts omit the service.

## Vulkan GPU surfaces

`GpuSurface` uses a Java `TextureView` only for placement and an Android presentation
surface. Its creation/resize/destruction callback lends an `ANativeWindow` to the
managed surface; the native Vulkan target acquires its own reference. Vulkan owns
the swapchain, depth attachment, pipelines, mapped upload buffers and submission
synchronization. C# calls the C Vulkan ABI directly for pipeline/buffer/draw/commit
operations. Java does not decode GPU commands, compile shaders or move GPU pixels.
There is no CPU readback/upload or bitmap snapshot in native presentation.

TextureView participates in ordinary Android clipping, transforms and sibling
ordering. Android composites its hardware buffers into the window, so this is
an embedded GPU surface, not a full-screen SurfaceView bypass of view composition.
The last submitted frame remains visible while navigation moves the node.
Only scene updates, surface recreation or resize request a fresh GPU submission.

The existing Roslyn shader generator produces HLSL from the application's C#
`[ShaderProgram]`. `Shaders/Android.Shaders.targets` runs the NDK's `glslc` and
`spirv-val` at build time and packages SPIR-V, keyed by source SHA-256 and stage.
Matrix packing matches Xui's row-major uniforms; vertex Y is inverted for Vulkan.
The runtime loads these assets and creates shader modules without a shader compiler
on the phone. Apps using GPU shaders import this targets file as TestApp does.
This build step currently requires Python 3 and NDK shader tools on macOS/Linux.

The first Vulkan implementation deliberately supports the existing cube contract:
Position(float3) and Color(float4), a uniform buffer at set 0/binding 0, triangle
lists, depth testing and culling, one draw per render pass. Unsupported layouts,
multiple draws and offscreen/readback targets fail explicitly. General vertex
layouts, textures, multiple draws, device-loss recovery and more advanced frame
pacing remain future work. Upload storage and command buffers are reused; a fence
protects their next use. Presentation semaphores are owned per swapchain image.
Resize/destruction drain the device before releasing surface resources.

## Text input

A Java `InputConnection` forwards commit, composition, selection, deletion and
return-key actions through the bridge to Xui's `TextEditingState`. The shared
editor remains authoritative for validation and grapheme boundaries. Keyboard
layout and Next/Done follow the focused field's `TextInputOptions`. Xui still draws
the caret/selection; native Android selection handles, magnifier, context menus
and autofill integration are not implemented by this adapter.

## Hosts and build

`Host/Android.Host.csproj` is optional. It packages Java sources without generated
bindings and builds `libxui_android.so` for ARM64 and x86-64. Its small Activity
adapter supplies the JavaVM and Activity handles to `AndroidWindow`. Existing
managed haptics/vibration services remain in this adapter, outside the renderer.
MAUI controls are not used by the renderer or native view hierarchy.

The first implementation requires Android 10 / API 29. Two-circle radial gradients
require API 31; concentric point-to-radius gradients work on API 29. The native
build script currently supports macOS and Linux NDK hosts.

On macOS, `sh tools/build-android.sh Debug` discovers JDK 21 and the installed
Android SDK/NDK and produces an APK with its assemblies embedded. Explicit paths:

```sh
dotnet build Xui/Apps/TestApp/TestApp.Mobile.csproj -f net10.0-android \
  -p:RuntimeIdentifier=android-arm64 \
  -p:JavaSdkDirectory=/path/to/jdk21 \
  -p:AndroidSdkDirectory=/path/to/android-sdk \
  -p:AndroidNdkDirectory=/path/to/android-ndk
```

The .NET 10 workload needs its matching SDK platform (currently API 36). The
native bridge uses API 29 and links for 16 KB pages. TestApp's Activity starts the
same application bootstrap as desktop; top-level desktop startup is excluded on
Android.

A standalone Mono host can use the same assembly, Java classes and C library:
bootstrap Mono, create `AndroidWindow(javaVm, activity)` on the UI thread, start
the application, and forward Activity resume/pause/destruction. This repository
change does **not** provide or validate a separate Mono APK bootstrap/packager.

## Validation

```sh
dotnet test Xui/Tests/Component/Xui.Tests.Component.csproj \
  --filter FullyQualifiedName~AndroidCompositionTest

# First build TestApp Debug for its generated Vulkan shader assets.
# Uses C#-generated fixtures and an attached ARM64 Android device/emulator:
sh Xui/Tests/Component/Android/Native/run.sh /path/to/sdk /path/to/ndk /path/to/jdk21
```

The component tests cover wire layout/opcode parity, cached node movement,
culling/reuse and zero managed allocation after warm-up. Native tests pass the
packets through the C/JNI bridge, poison the submission memory after return, check
rendered pixels (including combined image alpha and dash/paint Save/Restore), and
check that moving cached hardware views does not replay their drawing commands.
Vulkan tests check actual presented pixels, front-face culling, unchanged-frame reuse
and surface recreation after culling. Hardware pixel checks also verify that subtle
card shadows remain visible after Android theme opacity is applied. Activity resume
requests a fresh GPU frame even
when Android retains the same native surface. IME tests cover composition, validation and
grapheme-aware deletion.
These tests establish behavior, not a 120 FPS claim for the complete app.

## Remaining platform work

- True backdrop blur/glass and exact shadow offset/blur parity need platform work.
- Native precision selection, semantic accessibility and autofill bridging remain
  unimplemented. Basic keyboard/IME editing is supported as described above.
- Device information reports form factor, density-related geometry, font scale,
  appearance and animator preference; contrast preference is not wired yet.
- Images support packaged assets and raw BGRA pixels. Remote/asynchronous decode
  and image invalidation policy are separate work. Native images/fonts currently
  remain in the window catalogue until the window is disposed.
- Singular path transforms are explicitly unsupported. Complex-script shaping,
  RTL alignment and exact hanging/ideographic baseline metrics need broader text
  coverage; measurement and drawing use the same Android Paint/Typeface setup.
