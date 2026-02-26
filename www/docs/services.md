---
title: Service Resolution
description: How views acquire platform services through the IServiceProvider chain.
---

# Service Resolution

Every `View` implements `IServiceProvider`. Calling `GetService(typeof(T))` walks up the parent chain until a provider that knows about `T` responds.

**Source:** `Xui/Core/Core/UI/View.DI.cs`, `Xui/Core/Core/Abstract/Window.cs`.

## Resolution chain

```
View → … → RootView → Window.GetService()
                           ├─ Context  (DI scope / IServiceProvider injected at construction)
                           └─ Actual.GetService()  (platform runtime services)
```

Each `View` delegates upward to its `Parent`:

```csharp
// View.DI.cs
public virtual object? GetService(Type serviceType) =>
    this.Parent?.GetService(serviceType);
```

`RootView` delegates to its `Window`. `Window.GetService()` checks two sources in order:

```csharp
// Window.cs
public virtual object? GetService(Type serviceType) =>
    this.Context.GetService(serviceType) ?? this.Actual.GetService(serviceType);
```

1. **`Context`** — the `IServiceProvider` injected when the window was constructed (e.g., a DI scope from `Microsoft.Extensions.DependencyInjection`).
2. **`Actual.GetService()`** — the underlying platform window, which exposes platform services such as `IImagePipeline` and `ITextMeasureContext`.

## Calling from a view

Use the generic extension method available on any `View`:

```csharp
// Inside any View subclass:
var image  = this.GetService<IImage>();
var fonts  = this.GetRequiredService<ITextMeasureContext>();
```

`GetService<T>()` returns `null` if the service is not registered.
`GetRequiredService<T>()` throws `InvalidOperationException` if missing.

Both methods are defined in `Xui.Core.UI` (no extra `using` required).

## Platform services

These are provided by the platform runtime via `Actual.GetService()` — no DI registration needed:

| Service | Description |
|---|---|
| `IImage` | Self-loading image handle — one instance per logical image |
| `IImagePipeline` | Factory that loads and caches image data |
| `ITextMeasureContext` | Font shaping and text metrics |

See [Image Loading](images.md) for how to use `IImage`.

## DI integration (`Xui.Core.DI`)

When you use the `HostBuilder` path, each `Window` receives a scoped `IServiceProvider` as its `Context`. Services registered in `ConfigureServices` are then available to every view in that window:

```csharp
// Program.cs
new HostBuilder()
    .UseRuntime()
    .ConfigureServices(services =>
    {
        services.AddScoped<MainWindow>();
        services.AddScoped<Application>();
        services.AddSingleton<IMyDataService, MyDataService>();
    })
    .Build()
    .Run<Application>();
```

```csharp
// Inside any view in the window:
var data = this.GetService<IMyDataService>();
```

The window scope is created when the window opens and disposed when it closes. See [Getting Started](getting-started.md) for the full `HostBuilder` setup.

## Without DI

If you construct a `Window` without the host, pass `IServiceProvider.Empty` (or a minimal provider) as the context. Platform services are still available via `Actual.GetService()`:

```csharp
var window = new MainWindow(IServiceProvider.Empty);
// IImage still resolves — it comes from Actual.GetService()
```

## Direction rule

Service resolution flows **abstract → actual** only. The platform runtime never calls back into abstract code through `GetService`. This keeps `Xui.Core` free of all platform dependencies.
