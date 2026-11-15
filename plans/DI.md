# Dependency Injection (DI) Architecture

## Overview

This document describes the Dependency Injection (DI) architecture in Xui, focusing on service resolution flow, middleware integration, and best practices for maintaining unidirectional dependency flow.

## Core Principles

### 1. Unidirectional Flow: Abstract → Actual

The fundamental design principle is that dependencies flow **from abstract to actual** only:
- `Xui.Core` (abstract) has **zero** platform dependencies
- Platform runtimes (actual) implement platform-specific interfaces
- Abstract code **never** calls back into actual code through DI

### 2. Parent-Chain Resolution

Services are resolved by walking up the view hierarchy:
- No global service locator pattern
- DI flows from the root (Window) down to leaves (Views)
- Each component delegates to its parent until a provider responds

### 3. Middleware Transparency

Middleware components (Emulator, DevTools) sit between abstract and actual layers:
- They implement both `Abstract.IWindow` and `Actual.IWindow`
- They intercept and transform events/rendering without breaking the DI chain
- During construction, they must handle the case where Platform is not yet wired

## Service Resolution Chain

```
┌──────────────────────────────────────────────────────────┐
│ View                                                     │
│   GetService<T>() → Parent?.GetService<T>()             │
└────────────────────┬─────────────────────────────────────┘
                     ↓
┌──────────────────────────────────────────────────────────┐
│ Parent View (recursively)                                │
│   GetService<T>() → Parent?.GetService<T>()             │
└────────────────────┬─────────────────────────────────────┘
                     ↓
┌──────────────────────────────────────────────────────────┐
│ RootView                                                 │
│   GetService<T>():                                       │
│     - Returns `this` if T == IFocus                      │
│     - Otherwise → Window.GetService<T>()                 │
└────────────────────┬─────────────────────────────────────┘
                     ↓
┌──────────────────────────────────────────────────────────┐
│ Abstract Window                                          │
│   GetService<T>():                                       │
│     1. Check Context (DI scope) → T?                    │
│     2. Fall back to Actual.GetService<T>()              │
└────────────────────┬─────────────────────────────────────┘
                     ↓
┌──────────────────────────────────────────────────────────┐
│ Middleware Window (if present: Emulator, DevTools)       │
│   GetService<T>():                                       │
│     - If Platform == null → Abstract.GetService<T>()    │
│     - Otherwise → Platform.GetService<T>()              │
└────────────────────┬─────────────────────────────────────┘
                     ↓
┌──────────────────────────────────────────────────────────┐
│ Actual Window (Win32, macOS, iOS, Browser)              │
│   GetService<T>():                                       │
│     - Returns platform services (IImage, ITextMeasure)  │
│     - May query Application for app-level services      │
└────────────────────┬─────────────────────────────────────┘
                     ↓
┌──────────────────────────────────────────────────────────┐
│ Application                                              │
│   GetService<T>() → Context.GetService<T>()             │
└────────────────────┬─────────────────────────────────────┘
                     ↓
┌──────────────────────────────────────────────────────────┐
│ Host / IServiceProvider                                  │
│   - Application-level services (IInstruments, etc.)     │
│   - User services (IMyDataService, etc.)                │
└──────────────────────────────────────────────────────────┘
```

## Component Responsibilities

### View (Xui.Core.UI)

**File:** `Xui/Core/Core/UI/View.DI.cs`

```csharp
public virtual object? GetService(Type serviceType) =>
    this.Parent?.GetService(serviceType);
```

- Default implementation: delegate to parent
- Can be overridden to provide subtree-scoped services
- Example: `RootView` overrides to provide `IFocus`

### RootView (Xui.Core.UI)

**File:** `Xui/Core/Core/UI/RootView.cs`

```csharp
public override object? GetService(Type serviceType)
{
    if (serviceType == typeof(IFocus))
        return this;
    return this.window.GetService(serviceType);
}
```

- Provides `IFocus` service (focus management)
- Delegates all other services to Window

### Window (Xui.Core.Abstract)

**File:** `Xui/Core/Core/Abstract/Window.cs`

```csharp
public virtual object? GetService(Type serviceType) =>
    this.Context.GetService(serviceType) ?? this.Actual.GetService(serviceType);
```

- **Two-tier resolution:**
  1. **Context** (DI scope): User services registered via `HostBuilder`
  2. **Actual** (platform): Platform services like `IImage`, `ITextMeasureContext`
- This is the **root resolution point** for the view hierarchy

### Middleware Window (Emulator, DevTools)

**Files:**
- `Xui/Middleware/EmulatorLegacy/Actual/EmulatorWindow.cs`
- `Xui/Middleware/DevTools/Actual/DevToolsWindow.cs`

```csharp
public object? GetService(Type serviceType)
{
    // During construction, Platform is not yet wired — fall back to the abstract window's services.
    if (Platform == null)
        return (Abstract as IServiceProvider)?.GetService(serviceType);
    return Platform.GetService(serviceType);
}
```

**Critical Design Pattern:**
- Middleware implements both `Abstract.IWindow` and `Actual.IWindow`
- During initialization, `Platform` may be null
- **Fallback to Abstract**: Ensures DI works during construction phase
- Once wired, delegates to the underlying platform window

**Why This Pattern Is Necessary:**

1. **Timing Issue**: When `EmulatorWindow` is constructed:
   - `EmulatorPlatform.CreateWindow(abstractWindow)` creates the middleware
   - `EmulatorWindow.Abstract = abstractWindow` is set
   - `EmulatorWindow.Platform` is still `null` (assigned later)
   
2. **Construction-Time DI**: Some components need services during setup:
   - View initialization may call `GetService<IImage>()`
   - Platform resources may not exist yet
   - Must resolve from abstract window's DI scope

3. **Graceful Degradation**: The fallback ensures:
   - User DI services remain available
   - Platform services fail gracefully (return null)
   - No crashes during window setup

### Actual Window (Platform-Specific)

**Files:**
- `Xui/Runtime/Windows/Actual/Win32Window.cs`
- `Xui/Runtime/MacOS/Actual/MacOSWindow.cs`
- `Xui/Runtime/IOS/Actual/IOSWindow.cs`
- `Xui/Runtime/Browser/Actual/BrowserWindow.cs`

**Responsibilities:**
- Provide platform-specific services (`IImage`, `ITextMeasureContext`)
- Manage native window resources (HWND, NSWindow, UIWindow)
- Create platform-specific drawing contexts
- **Never** call back into abstract code via DI

**Example (Win32):**
```csharp
// In Win32Window constructor
var instruments = (@abstract as IServiceProvider)?.GetService(typeof(IInstruments)) as IInstruments;
```
**Note:** This is acceptable because:
- Constructor queries abstract's DI scope directly
- Does not create circular dependency
- Respects abstract → actual direction

### Application (Xui.Core.Abstract)

**File:** `Xui/Core/Core/Abstract/Application.cs`

```csharp
public object? GetService(Type serviceType) =>
    this.Context.GetService(serviceType);
```

- Delegates to the host's DI container
- Provides application-level services (`IInstruments`, user services)

## Service Types

### Platform Services (Actual → Abstract)

These services are provided by the platform runtime and are available without DI registration:

| Service | Provider | Description |
|---------|----------|-------------|
| `IImage` | Window's `IImagePipeline` | Self-loading image handle |
| `IImagePipeline` | Actual window | Factory for creating `IImage` instances |
| `ITextMeasureContext` | Actual window | Font shaping and text metrics |
| `IContext` (during render) | Render loop | Drawing context for current frame |

**Resolution:** `Window.Actual.GetService()`

### User Services (Host → Abstract)

These services are registered via `HostBuilder` and injected into windows:

| Service | Registration | Description |
|---------|--------------|-------------|
| `IInstruments` | `AddSingleton<IInstruments>` | Trace logging and diagnostics |
| `IMyDataService` | User-defined | Application-specific services |

**Resolution:** `Window.Context.GetService()`

### View Services (Local Scope)

These services are provided by specific views in the hierarchy:

| Service | Provider | Description |
|---------|----------|-------------|
| `IFocus` | `RootView` | Focus management |

**Resolution:** Walk up parent chain

## DI Integration (Xui.Core.DI)

When using `HostBuilder`, each window receives a scoped `IServiceProvider`:

```csharp
// Program.cs
new HostBuilder()
    .UseRuntime()
    .ConfigureServices(services =>
    {
        services.AddScoped<MainWindow>();
        services.AddScoped<Application>();
        services.AddSingleton<IInstruments>(_ => Instruments.Console);
        services.AddSingleton<IMyDataService, MyDataService>();
    })
    .Build()
    .Run<Application>();
```

**Scoping:**
- **Application scope**: Created at startup, disposed at shutdown
- **Window scope**: Created when window opens, disposed when it closes
- Services can be `Singleton`, `Scoped`, or `Transient`

## Common Patterns

### 1. Acquiring Platform Services from Views

```csharp
protected override void OnAttach(ref AttachEventRef e)
{
    var image = this.GetService<IImage>();
    image?.Load("Assets/photo.png");
}
```

- Walks up to Window
- Window checks Context (null) → Actual
- Actual window returns new `IImage` instance

### 2. Acquiring User Services from Views

```csharp
protected override void OnAttach(ref AttachEventRef e)
{
    var data = this.GetService<IMyDataService>();
    data?.Load();
}
```

- Walks up to Window
- Window checks Context (found!) → returns service
- Never reaches Actual

### 3. Middleware Transparent Pass-Through

```csharp
// In EmulatorWindow
public object? GetService(Type serviceType)
{
    if (Platform == null)
        return (Abstract as IServiceProvider)?.GetService(serviceType);
    return Platform.GetService(serviceType);
}
```

- During construction: queries abstract's DI scope
- After wiring: queries platform's services
- Transparent to the view hierarchy

## Known Issues and Solutions

### Issue 1: Middleware "Swimming Upstream"

**Problem:** Middleware queries Abstract window before Platform is set, creating apparent upstream dependency.

**Current Status:** ✅ **RESOLVED**

**Solution:** Both `EmulatorWindow` and `DevToolsWindow` implement fallback pattern:
```csharp
if (Platform == null)
    return (Abstract as IServiceProvider)?.GetService(serviceType);
return Platform.GetService(serviceType);
```

This is **not** a violation of unidirectional flow because:
- It's a **construction-time fallback**, not a runtime callback
- Abstract window's services (DI scope) are legitimately accessible
- Once Platform is wired, normal flow resumes

### Issue 2: Win32Window Constructor Queries Abstract

**Location:** `Xui/Runtime/Windows/Actual/Win32Window.cs:78`

```csharp
var instruments = (@abstract as IServiceProvider)?.GetService(typeof(IInstruments)) as IInstruments;
```

**Status:** ✅ **ACCEPTABLE**

**Rationale:**
- Constructor-time initialization, not runtime query
- Queries abstract's DI scope directly (not via GetService chain)
- Does not create circular dependency
- Respects abstract → actual direction (abstract provides services, actual consumes)

### Issue 3: Application-Level Services

**Pattern:** Some actual windows may need application-level services (like `IInstruments`).

**Current Approach:**
- Actual window queries abstract window's DI scope during construction
- Stores reference for lifetime of window

**Better Approach:**
- Pass required services as constructor parameters
- Avoids any appearance of upstream query
- More explicit dependency graph

**Recommendation:**
```csharp
// Current (acceptable but implicit)
var instruments = (@abstract as IServiceProvider)?.GetService(typeof(IInstruments));

// Better (explicit and clear)
internal Win32Window(IWindow @abstract, IInstruments? instruments)
{
    this.instruments = instruments;
}
```

## Future Improvements

### 1. Explicit Service Injection for Actual Windows

Instead of querying abstract window for services, pass them as constructor parameters:

```csharp
// Platform creates window with services from abstract's DI scope
internal Win32Window CreateWindow(IWindow @abstract)
{
    var instruments = (@abstract as IServiceProvider)?.GetService<IInstruments>();
    return new Win32Window(@abstract, instruments);
}
```

**Benefits:**
- More explicit dependency graph
- Easier to test (can mock services)
- No appearance of upstream query

### 2. Service Provider Interfaces

Define explicit interfaces for service providers at each layer:

```csharp
public interface IPlatformServices
{
    IImage CreateImage();
    ITextMeasureContext GetTextMeasure();
}

public interface IApplicationServices
{
    IInstruments GetInstruments();
}
```

**Benefits:**
- Clear contract for each layer
- Better discoverability
- Type-safe service resolution

### 3. Middleware Service Interception

Allow middleware to intercept and augment services:

```csharp
public object? GetService(Type serviceType)
{
    // Intercept IContext to wrap with debugging context
    if (serviceType == typeof(IContext) && screenshotPending)
        return new SplicingContext(Platform!.GetService<IContext>());
    
    // Normal pass-through
    return Platform?.GetService(serviceType);
}
```

**Current Status:** ✅ DevToolsWindow already implements this pattern

## Testing Strategy

### Unit Tests

1. **View Resolution Chain:**
   - Test parent-chain walking
   - Test RootView provides IFocus
   - Test Window delegates to Context and Actual

2. **Middleware Fallback:**
   - Test GetService with Platform == null
   - Test GetService with Platform != null
   - Test service resolution during construction

3. **Service Scoping:**
   - Test singleton vs. scoped services
   - Test window scope disposal
   - Test service isolation between windows

### Integration Tests

1. **Full Stack:**
   - Create host with services
   - Create window with middleware
   - Verify view can resolve services
   - Verify platform services work

2. **Device Recovery (Windows):**
   - Simulate device-lost
   - Verify service resolution still works
   - Verify image resources re-hydrate

## Summary

The Xui DI architecture maintains strict unidirectional flow (abstract → actual) while allowing flexible middleware integration. Key principles:

1. **Views resolve services by walking up to Window**
2. **Window checks DI scope first, then platform services**
3. **Middleware transparently passes through, with construction-time fallback**
4. **Actual windows never call back into abstract code via DI**

The current implementation correctly handles the middleware initialization timing issue without violating architectural principles. Future improvements can make dependencies more explicit without changing the fundamental flow.