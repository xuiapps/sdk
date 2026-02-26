---
title: Getting Started
description: Create and run your first Xui application on Windows or macOS.
---

# Getting Started

## Prerequisites

- .NET 10 SDK
- Windows 11 (for the Windows target) or macOS 14+ (for the macOS target)
- Visual Studio 2022, Rider, or any editor with C# support

## Application structure

A Xui application has three parts: an `Application`, a `Window`, and at least one `View`.

### Application

```csharp
// Application.cs
using Xui.Core.Abstract;
using Xui.Core.DI;

public class Application : Xui.Core.Abstract.Application
{
    protected override void Start()
        => this.CreateAndShowMainWindowOnce<MainWindow>();
}
```

`Start()` is called once the runtime is ready. `CreateAndShowMainWindowOnce` creates a DI scope, resolves `MainWindow` from it, shows the window, and quits the app when it closes.

### Window

```csharp
// MainWindow.cs
using Xui.Core.Abstract;

public class MainWindow(IServiceProvider services) : Window(services)
{
    public MainWindow(IServiceProvider services) : base(services)
    {
        Title   = "Hello Xui";
        Content = new HelloView();
    }
}
```

`Content` is the root `View` for the window. Set it in the constructor.

### View

```csharp
// HelloView.cs
using Xui.Core.Canvas;
using Xui.Core.Math2D;
using Xui.Core.UI;

public class HelloView : View
{
    protected override void RenderCore(IContext context)
    {
        context.SetFill(Colors.White);
        context.FillRect(Frame);

        context.SetFill(Colors.Black);
        context.SetFont(new Font(24, ["Segoe UI"], FontWeight.Normal));
        context.TextBaseline = TextBaseline.Middle;
        context.FillText("Hello, Xui!", new Point(Frame.X + 20, Frame.MidY));
    }
}
```

### Entry point

```csharp
// Program.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xui.Core.DI;

return new HostBuilder()
    .UseRuntime()
    .ConfigureServices(services => services
        .AddScoped<MainWindow>()
        .AddScoped<Application>())
    .Build()
    .Run<Application>();
```

`UseRuntime()` registers the platform-specific runtime (Win32 on Windows, CoreGraphics on macOS, etc.). `Run<Application>()` starts the host, resolves your `Application`, and blocks until the app exits.

## Running

```
dotnet run
```

## Next steps

- [Architecture](architecture.md) — understand how the window connects to the platform.
- [Canvas API](canvas.md) — everything you can draw.
- [View System](views.md) — layout, lifecycle, and built-in views.
- [Service Resolution](services.md) — accessing platform and DI services from views.
- [Input Handling](input.md) — pointer events, keyboard, and scroll.
