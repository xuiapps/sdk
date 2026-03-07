---
title: View — DI
description: IServiceProvider implementation, GetService, and the parent-chain resolution hierarchy.
---

# View — DI (Service Resolution)

`View` implements `IServiceProvider`. Services are resolved by walking up the parent chain until a provider returns a non-null result.

## GetService

```csharp
public virtual object? GetService(Type serviceType) =>
    this.Parent?.GetService(serviceType);
```

The default implementation delegates to the parent. `RootView` overrides to return `this` for `IFocus` and then delegates to `Window` for everything else. `Window` in turn can delegate to a platform-provided or DI-scope-provided `IServiceProvider`.

The generic extension (from `Xui.Core.DI`) is the preferred call site:

```csharp
using Xui.Core.DI;

var focus   = this.GetService<IFocus>();
var bitmaps = this.GetService<IBitmapFactory>();
```

## Resolution chain

```
View.GetService
  → Parent.GetService
      → ... (ancestor chain)
          → RootView.GetService  (returns IFocus)
              → Window.GetService
                  → platform services / DI scope
```

## Registering services

Override `GetService` in a custom container or host view to inject services scoped to a subtree:

```csharp
public override object? GetService(Type serviceType)
{
    if (serviceType == typeof(IMyService)) return _myService;
    return base.GetService(serviceType);
}
```

This is how `RootView` provides `IFocus` without any external DI container in `Xui.Core`.
