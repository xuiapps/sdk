# Application Model: Navigation, Templated Views, and Reactive Components

## Overview

This document establishes the architectural plan for Xui's application-level primitives: context providers (services that flow down the hierarchy and notify consumers of changes), templated views (lazy template realization analogous to custom web components with open shadow root), navigation primitives (stack navigation, tab view, navigation view), desktop and mobile application shells, and a reactive component model for highly polymorphic, data-driven UI.

---

## 1. Prior Art Analysis

### 1.1 React Context and Providers

React solves "services that can be queried up from the hierarchy and changes propagate down" via the **Context API**:

```jsx
// Define a context
const ThemeContext = React.createContext(defaultValue);

// Provide a value somewhere in the tree
<ThemeContext.Provider value={theme}>
  <DeepChild />
</ThemeContext.Provider>

// Consume anywhere below
function DeepChild() {
  const theme = useContext(ThemeContext);
  return <div style={{ color: theme.primary }} />;
}
```

**Key properties:**
- `createContext` registers a typed context slot.
- `<Context.Provider value={…}>` establishes a scope: all descendants read from the nearest provider.
- When the provider's `value` reference changes, **all consumers re-render**.
- Multiple providers of the same context can be nested; inner providers shadow outer ones.
- No global registry — resolution is purely tree-structural.

**Analogy to Xui:** Xui's existing DI chain (`View.GetService → Parent.GetService → … → Window.GetService`) already implements *querying up* the hierarchy. What is missing is:
1. **Type-safe context slots** that a specific ancestor declares and descendants subscribe to.
2. **Change notification** — when the service value changes, descendants are automatically re-invalidated.

### 1.2 Web Custom Components and Shadow DOM

The HTML Custom Elements + Shadow DOM spec provides "templated views with open shadow root":

```js
class MyCard extends HTMLElement {
  connectedCallback() {                // ≈ OnAttach
    const shadow = this.attachShadow({ mode: 'open' });
    shadow.innerHTML = `<slot></slot>`; // realize template
  }
}
```

**Key design decisions:**
- **Lazy realization:** The shadow root is not created until `connectedCallback` fires (equivalent to `OnAttach` in Xui). Before that, the element exists in the DOM tree but has no rendered content.
- **Slot projection:** The host element's light DOM children are *projected* into `<slot>` placeholders inside the shadow root. This allows composable component APIs.
- **Encapsulation:** Shadow root creates a style and event boundary, but "open" mode allows external traversal.

In Xui we will use `OnAttach` / `OnActivate` as the realization trigger.

### 1.3 Desktop Navigation Patterns

#### Windows Shell (Fluent Design)
- **NavigationView** — vertical sidebar with icons+labels on the left; collapses to icon-only rail on smaller windows; top tab bar on very wide windows.
- **Frame / ContentFrame** — single content slot that replaces its child on navigation; supports back/forward.
- **Breadcrumb** — hierarchical path display; each crumb is a navigation shortcut.
- **Tab View** (WinUI 3) — horizontally scrollable tab strip + content; supports tearing/reordering.
- **Dialog / Overlay** — modal or non-modal content on top of the shell.

#### macOS Shell (AppKit)
- **NSSplitViewController** — horizontal split: sidebar + content (+ optional inspector).
- **NSTabViewController** — tab strip with swappable content panels.
- **NSToolbar** — toolbar items docked above content.
- **Responder chain** — analogous to Xui's parent-chain DI: commands (e.g. "Save") walk the responder chain to find a handler.

#### Cross-Platform Desktop Pattern
```
┌─────────────────────────────────────────────────────┐
│  TitleBar (window chrome, menu, toolbar)            │
├──────────┬──────────────────────────────────────────┤
│          │                                          │
│ Sidebar  │         ContentFrame                     │
│ (Nav)    │   (active page / panel)                  │
│          │                                          │
├──────────┴──────────────────────────────────────────┤
│  StatusBar (optional)                               │
└─────────────────────────────────────────────────────┘
```

### 1.4 Mobile Navigation Patterns

#### iOS (UIKit / SwiftUI)
- **UINavigationController** — stack of full-screen pages; push/pop with slide animations; back gesture.
- **UITabBarController** — 2–5 tabs at the bottom; each tab owns its own navigation stack.
- **UISheetPresentationController** — bottom sheet with configurable detents.
- **Modal presentation** — full-screen, page sheet, form sheet.

#### Android (Jetpack Navigation)
- **NavGraph** + **NavController** — destination graph; `navigate(R.id.dest)` drives the back stack.
- **Bottom Navigation Bar** — ≈ UITabBarController.
- **Navigation Drawer** — slide-in sidebar.
- **Bottom Sheet** — partial-height surface from bottom.

#### SwiftUI / Compose Declarative Model
Both SwiftUI (`NavigationStack`, `NavigationSplitView`) and Jetpack Compose (`NavHost`) favor a **destination-based model**: instead of imperative push/pop, a route string (or enum) identifies the current page and the framework handles transitions. This is the recommended approach for Xui as well.

---

## 2. Core Concepts

### 2.1 Context Provider — Services That Flow Down

#### Problem
Xui's current DI walks *up* the tree (child → parent → … → Window). This is good for platform/infrastructure services. However, app-level services — like the currently signed-in user, a navigation controller, the active design theme, or a data store — require a **bidirectional** model:

- **Query direction (up):** a deep view asks its ancestor chain for the service value — the same pattern as `GetService`.
- **Notification direction (down):** when the service value changes, all registered consumers below the provider are automatically invalidated (measure + render).

The existing DI chain provides the *query* direction but has no *notification* direction. App-level services therefore additionally need to:

1. Be **injected at a subtree root** (e.g. a page), not only at the Window level.
2. **Notify all consumers** when the service state changes, triggering layout/render invalidation.

#### Design: `ContextProvider<T>` and `IContext<T>`

```csharp
/// <summary>
/// Marks a view as a provider of a typed context value.
/// Views below this node may call GetContext<T>() to receive the value.
/// When the value changes, all attached consumers are invalidated.
/// Implementing RegisterConsumer on the interface ensures that any
/// custom provider (not only ContextProvider<T>) can participate in
/// the change-notification protocol.
/// </summary>
public interface IContextProvider<T>
{
    T ContextValue { get; }

    /// <summary>
    /// Registers a consumer view so it is invalidated when ContextValue changes.
    /// Implementations should hold weak references to avoid keeping consumers alive.
    /// </summary>
    void RegisterConsumer(View consumer);
}
```

```csharp
/// <summary>
/// A view that injects a typed context value into the subtree it contains.
/// Analogous to React's <Context.Provider value={…}>.
/// </summary>
public class ContextProvider<T> : View, IContextProvider<T>
{
    private T value;
    private readonly List<WeakReference<View>> consumers = new();

    public T Value
    {
        get => value;
        set
        {
            if (EqualityComparer<T>.Default.Equals(this.value, value)) return;
            this.value = value;
            NotifyConsumers();
        }
    }

    // IContextProvider<T>
    T IContextProvider<T>.ContextValue => Value;

    public override object? GetService(Type serviceType)
    {
        if (serviceType == typeof(IContextProvider<T>))
            return this;
        return base.GetService(serviceType);
    }

    public void RegisterConsumer(View consumer)
    {
        consumers.Add(new WeakReference<View>(consumer));
    }

    private void NotifyConsumers()
    {
        foreach (var weakRef in consumers)
        {
            if (weakRef.TryGetTarget(out var consumer))
            {
                consumer.InvalidateRender();
                consumer.InvalidateMeasure();
            }
        }
        // Prune dead references
        consumers.RemoveAll(r => !r.TryGetTarget(out _));
    }
}
```

**Resolution Extension:**

```csharp
public static class ViewContextExtensions
{
    /// <summary>
    /// Resolves the nearest ContextProvider<T> ancestor and returns its value.
    /// Registers this view as a consumer so it is invalidated on value changes.
    /// Returns defaultValue if no provider is found.
    /// </summary>
    public static T GetContext<T>(this View view, T defaultValue = default!)
    {
        var provider = view.GetService<IContextProvider<T>>();
        if (provider is not null)
        {
            provider.RegisterConsumer(view);
            return provider.ContextValue;
        }
        return defaultValue;
    }
}
```

**Usage pattern:**

```csharp
// App root wires the theme context
var shell = new DesktopShell
{
    Content = new ContextProvider<ITheme>
    {
        Value = lightTheme,
        Content = new MyPage()
    }
};

// A deep view consumes it
protected override void RenderCore(IContext ctx)
{
    var theme = this.GetContext<ITheme>();
    ctx.SetFill(theme.Surface);
    // …
}
```

#### Relationship to Existing DI

| Mechanism | Direction | Change notification | Scope |
|-----------|-----------|---------------------|-------|
| `View.GetService` (existing) | Up (child → parent → Window) | None | Window/App |
| `ContextProvider<T>` (new) | Up (query) + Down (notify) | Yes | Subtree |

The two mechanisms are complementary. Platform and infrastructure services remain in the DI chain. Application-level reactive state uses context providers.

---

### 2.2 Reactive Observable State

#### Design: `Observable<T>` — A Lightweight Reactive Cell

```csharp
/// <summary>
/// A boxed value cell that notifies subscribers when its value changes.
/// Equivalent to MobX observable or Dart's ValueNotifier.
/// </summary>
public sealed class Observable<T>
{
    private T current;

    public Observable(T initial) => current = initial;

    public T Value
    {
        get => current;
        set
        {
            if (EqualityComparer<T>.Default.Equals(current, value)) return;
            current = value;
            Changed?.Invoke(value);
        }
    }

    public event Action<T>? Changed;
}
```

#### Design: `ReactiveView` — Auto-invalidating Views

```csharp
/// <summary>
/// A View that subscribes to a set of Observable<T> cells and
/// automatically invalidates when any of them changes.
/// Subscriptions are established in OnActivate and torn down in OnDeactivate.
/// </summary>
public abstract class ReactiveView : View
{
    private readonly List<Action> unsubscribeActions = new();

    /// <summary>
    /// Subscribes to an observable. When it changes, this view is invalidated
    /// (measure + render). Subscription is active only while the view is active.
    /// </summary>
    protected void Watch<T>(Observable<T> observable)
    {
        void OnChanged(T _)
        {
            this.InvalidateMeasure();
            this.InvalidateRender();
        }

        observable.Changed += OnChanged;
        unsubscribeActions.Add(() => observable.Changed -= OnChanged);
    }

    protected override void OnActivate()
    {
        base.OnActivate();
        ConfigureBindings();
    }

    protected override void OnDeactivate()
    {
        base.OnDeactivate();
        foreach (var unsub in unsubscribeActions) unsub();
        unsubscribeActions.Clear();
    }

    /// <summary>
    /// Override to call Watch(…) on the observables this view depends on.
    /// Called each time the view activates.
    /// </summary>
    protected virtual void ConfigureBindings() { }
}
```

**Usage:**

```csharp
public class UserBadge : ReactiveView
{
    private readonly Observable<User> currentUser;

    public UserBadge(Observable<User> currentUser)
    {
        this.currentUser = currentUser;
    }

    protected override void ConfigureBindings()
    {
        Watch(currentUser);
    }

    protected override void RenderCore(IContext ctx)
    {
        var user = currentUser.Value;
        ctx.FillText(user.DisplayName, …);
    }
}
```

#### Design: `ObservableList<T>` — Reactive Collection

```csharp
/// <summary>
/// A list that fires change events when items are added, removed, or replaced.
/// Suitable for powering virtualizing lists or reactive data grids.
/// </summary>
public sealed class ObservableList<T> : IReadOnlyList<T>
{
    private readonly List<T> inner = new();

    public event Action? Changed;

    public int Count => inner.Count;
    public T this[int index] => inner[index];

    public void Add(T item)      { inner.Add(item); Changed?.Invoke(); }
    public void Remove(T item)   { inner.Remove(item); Changed?.Invoke(); }
    public void Insert(int i, T item) { inner.Insert(i, item); Changed?.Invoke(); }
    public void RemoveAt(int i)  { inner.RemoveAt(i); Changed?.Invoke(); }
    public void Clear()          { inner.Clear(); Changed?.Invoke(); }

    public IEnumerator<T> GetEnumerator() => inner.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => inner.GetEnumerator();
}
```

---

### 2.3 Templated View — Lazy Realization

#### Motivation

A templated view separates *declaration* of a component (its template) from *realization* (constructing child views). This mirrors:

- **Web custom elements** with `connectedCallback` — the shadow root is only created once the element is connected to the document.
- **WPF ContentControl + DataTemplate** — a template produces content when the control loads.
- **SwiftUI `@ViewBuilder`** — content is described lazily as a function that produces views.

#### Design: `TemplatedView`

```csharp
/// <summary>
/// A view that realizes its content lazily on attach or activation.
/// Subclasses override BuildContent() to produce the view tree.
/// The produced tree is not constructed until OnAttach fires, ensuring
/// platform services (IImagePipeline, ITextMeasureContext, etc.) are available.
/// </summary>
public abstract class TemplatedView : View
{
    private View? content;

    /// <summary>
    /// Gets the realized content view, or null if not yet attached.
    /// </summary>
    public View? Content => content;

    /// <inheritdoc/>
    public override int Count => content is null ? 0 : 1;

    /// <inheritdoc/>
    public override View this[int index] =>
        index == 0 && content is not null ? content : throw new ArgumentOutOfRangeException(nameof(index));

    /// <summary>
    /// Called once when the view is first attached to the visual tree.
    /// Override BuildContent() — do not call base unless you intend to
    /// re-trigger the default realization.
    /// </summary>
    protected override void OnAttach(ref AttachEventRef e)
    {
        base.OnAttach(ref e);
        if (content is null)
            Realize();
    }

    /// <summary>
    /// Destroys the current content and rebuilds it from BuildContent().
    /// Useful for hot-reload or explicit template refresh.
    /// </summary>
    public void Realize()
    {
        if (content is not null)
            this.SetProtectedChild(ref content, null);

        var built = BuildContent();
        this.SetProtectedChild(ref content, built);
    }

    /// <summary>
    /// Constructs and returns the view tree for this component.
    /// Called once on attach (or on each Realize() call).
    /// At this point, platform services are available via GetService().
    /// </summary>
    protected abstract View BuildContent();

    protected override Size MeasureCore(Size available, IMeasureContext ctx) =>
        content?.Measure(available, ctx) ?? Size.Zero;

    protected override void ArrangeCore(Rect rect, IMeasureContext ctx) =>
        content?.Arrange(rect, ctx);
}
```

#### Slot Projection (Content Projection)

To support composable templated components (analogous to `<slot>` in web components), a templated view can accept *projected content* from its host:

```csharp
/// <summary>
/// A TemplatedView that accepts a host-supplied content slot.
/// The BuildContent() method may place the SlotContent view anywhere
/// inside the realized template.
/// </summary>
public abstract class SlottedView : TemplatedView
{
    private View? slotContent;

    /// <summary>
    /// The host-supplied content to project into a slot inside the template.
    /// Must be set before the view is attached, or Realize() must be called after setting.
    /// </summary>
    public View? SlotContent
    {
        get => slotContent;
        set
        {
            slotContent = value;
            // If already attached, re-realize so the new slot content is projected.
            if ((this.Flags & ViewFlags.Attached) != 0)
                Realize();
        }
    }
}
```

**Usage example:**

```csharp
public class CardView : SlottedView
{
    public string Title { get; set; } = "";

    protected override View BuildContent() =>
        new Border
        {
            CornerRadius = 8,
            Content = new VerticalStack
            {
                Content = [
                    new Label { Text = Title },
                    SlotContent ?? new View() // projected slot
                ]
            }
        };
}

// At call site:
new CardView
{
    Title = "Profile",
    SlotContent = new UserBadge(currentUser)
}
```

---

## 3. Navigation Primitives

### 3.1 Navigation Model — Destination-Based Routing

Rather than imperative push/pop APIs tied to concrete view types, Xui navigation uses a **destination model**: navigation routes are described as values (strings, discriminated unions, or record types), and a router maps them to realized views. This mirrors SwiftUI's `NavigationStack(path:)` and Jetpack Compose's `NavHost`.

#### Core Types

```csharp
/// <summary>
/// Represents a navigation destination — a value that identifies a page.
/// Can be a string, enum, or any equatable record.
/// </summary>
public interface IDestination { }

/// <summary>
/// Resolves a destination to its View. Registered by the app.
/// </summary>
public interface INavigationRouter
{
    View Resolve(IDestination destination);
}

/// <summary>
/// Service provided by NavigationStack (and TabView) to descendants.
/// Allows deep views to navigate without holding a direct reference to the stack.
/// </summary>
public interface INavigationController
{
    /// <summary>Current navigation path (read-only snapshot).</summary>
    IReadOnlyList<IDestination> Path { get; }

    /// <summary>Push a new page onto the stack.</summary>
    void Push(IDestination destination);

    /// <summary>Pop the top page. No-op if at root.</summary>
    void Pop();

    /// <summary>Pop to root.</summary>
    void PopToRoot();

    /// <summary>Replace the entire stack.</summary>
    void Replace(IReadOnlyList<IDestination> path);

    /// <summary>Fired when the navigation path changes.</summary>
    event Action? PathChanged;
}
```

**Resolution via DI chain:**  
`INavigationController` is provided as a view service by `NavigationStack`, making it available to all descendants via the existing parent-chain resolution:

```csharp
// Deep view navigates without holding a reference:
protected override void OnMouseDown(ref MouseDownEventRef e)
{
    var nav = this.GetService<INavigationController>();
    nav?.Push(new ProductDetailDestination(productId));
}
```

---

### 3.2 NavigationStack

A vertical stack of full-screen pages with push/pop transitions (slide, fade). Serves as the mobile "drill-down" and desktop back-button navigation primitive.

#### Interface

```csharp
/// <summary>
/// A view that manages a stack of pages, each identified by an IDestination.
/// Provides INavigationController to all descendants.
/// Applies transition animations on push/pop.
/// </summary>
public class NavigationStack : TemplatedView, INavigationController, IServiceProvider
{
    private readonly INavigationRouter router;
    private readonly List<IDestination> path = new();
    private readonly List<View> pageViews = new();

    // Currently visible page (top of stack)
    private View? activeView;

    // Outgoing page during transition
    private View? outgoingView;

    // Transition state
    private NavigationTransition? activeTransition;

    public event Action? PathChanged;

    public IReadOnlyList<IDestination> Path => path.AsReadOnly();

    public NavigationStack(INavigationRouter router, IDestination root)
    {
        this.router = router;
        path.Add(root);
    }

    /// <summary>
    /// Push a new page onto the stack. Fires PathChanged and starts the
    /// configured push transition. No-op if destination equals the current top.
    /// </summary>
    public void Push(IDestination destination) { /* … */ }

    /// <summary>
    /// Pop the top page. Fires PathChanged and starts the configured pop transition.
    /// No-op if the stack has only the root page.
    /// </summary>
    public void Pop() { /* … */ }

    /// <summary>Pop all pages back to the root. No-op if already at root.</summary>
    public void PopToRoot() { /* … */ }

    /// <summary>
    /// Replace the entire path. The router resolves each destination in order;
    /// the last item becomes the active page. Fires PathChanged once.
    /// </summary>
    public void Replace(IReadOnlyList<IDestination> newPath) { /* … */ }

    // Provide INavigationController to descendants
    public override object? GetService(Type serviceType)
    {
        if (serviceType == typeof(INavigationController))
            return this;
        return base.GetService(serviceType);
    }

    protected override View BuildContent() => /* navigation chrome */ ;
}
```

#### Transition System

```csharp
/// <summary>
/// Describes a visual transition between two pages.
/// </summary>
public abstract class NavigationTransition
{
    public abstract void Animate(
        View? outgoing,
        View incoming,
        TimeSpan elapsed,
        TimeSpan duration,
        out bool complete);
}

/// <summary>Built-in: slide incoming from right, slide outgoing to left (push).</summary>
public sealed class SlideTransition : NavigationTransition { /* … */ }

/// <summary>Built-in: cross-fade.</summary>
public sealed class FadeTransition : NavigationTransition { /* … */ }

/// <summary>No transition — immediate swap.</summary>
public sealed class NoneTransition : NavigationTransition { /* … */ }
```

#### Navigation Bar

```csharp
/// <summary>
/// A view that renders a navigation bar: title + back button.
/// Reads INavigationController from the parent chain.
/// </summary>
public class NavigationBar : ReactiveView
{
    protected override void ConfigureBindings()
    {
        var nav = this.GetService<INavigationController>();
        if (nav is not null)
        {
            nav.PathChanged += () => { InvalidateRender(); InvalidateMeasure(); };
        }
    }

    protected override void RenderCore(IContext ctx)
    {
        var nav = this.GetService<INavigationController>();
        bool canGoBack = nav?.Path.Count > 1;
        // Render back button (if canGoBack), title text, etc.
    }
}
```

---

### 3.3 TabView

A container that presents multiple pages (tabs), each with an icon and label. Only one tab is active at a time. Each tab owns its own navigation stack.

```csharp
/// <summary>
/// A view that holds multiple tabs, each identified by a TabItem descriptor.
/// The active tab's page is fully attached and active; others are deactivated
/// (but remain attached if tab persistence is enabled).
/// </summary>
public class TabView : View, IServiceProvider
{
    public IReadOnlyList<TabItem> Tabs { get; }

    /// <summary>
    /// Gets or sets the index of the active tab.
    /// Setting an out-of-range value throws ArgumentOutOfRangeException.
    /// </summary>
    public int SelectedIndex { get; set; }
    public TabBarPosition BarPosition { get; set; } = TabBarPosition.Bottom; // Bottom for mobile, Top for desktop

    // Provides ITabController to descendants
    public override object? GetService(Type serviceType) { … }
}

public record TabItem(
    string Title,
    View Icon,
    View Page,
    bool PersistWhenInactive = true  // keep attached, just deactivate
);

public enum TabBarPosition { Top, Bottom, Left, Hidden }

/// <summary>
/// Service provided by TabView to descendants.
/// </summary>
public interface ITabController
{
    int SelectedIndex { get; set; }
    IReadOnlyList<TabItem> Tabs { get; }
    event Action? SelectionChanged;
}
```

**Lifecycle semantics:**
- All tab pages are **attached** to the visual tree immediately (platform resources available).
- Only the **active** tab is **activated** (receives events, renders, animates).
- On tab switch: `DeactivateSubtree(outgoing)` → `ActivateSubtree(incoming)`.
- If `PersistWhenInactive = false`, the inactive tab page is **fully detached** (both deactivated and removed from the visual tree), freeing all platform resources (images, font caches). It will be re-attached and re-activated when the tab is selected again.

---

### 3.4 NavigationView (Desktop Sidebar)

A two-pane layout: a collapsible sidebar (navigation rail or pane) on the left and a content frame on the right. Adapts to window width.

```csharp
/// <summary>
/// Desktop navigation view. Renders a sidebar with navigation items and a content area.
/// Automatically collapses the sidebar into an icon-only rail when narrow,
/// and into a hamburger-menu overlay when very narrow.
/// </summary>
public class NavigationView : View, IServiceProvider
{
    public IReadOnlyList<NavigationItem> Items { get; set; } = [];
    public NavigationItem? Selected { get; set; }
    public NavigationPaneDisplayMode DisplayMode { get; set; } = NavigationPaneDisplayMode.Auto;
    public View? Header { get; set; }
    public View? Footer { get; set; }

    // The content frame — replaced when navigation changes
    public View? Content { get; set; }

    // Provides INavigationController service
    public override object? GetService(Type serviceType) { … }
}

public record NavigationItem(
    string Title,
    View Icon,
    IDestination? Destination = null,
    IReadOnlyList<NavigationItem>? Children = null
);

public enum NavigationPaneDisplayMode
{
    /// Automatically chooses based on window width
    Auto,
    /// Always expanded with icon + text
    Left,
    /// Always icon-only rail
    LeftCompact,
    /// Overlay triggered by menu button
    LeftMinimal,
    /// Horizontal tabs at top
    Top
}
```

**Width thresholds (defaults, configurable):**

| Window width | Display mode |
|---|---|
| > 1008 dp | Left (full) |
| 641–1008 dp | LeftCompact (icon rail) |
| < 641 dp | LeftMinimal (overlay) |

---

### 3.5 DesktopShell

A complete desktop application shell: title bar region, optional menu bar, optional toolbar, navigation view, status bar.

```csharp
/// <summary>
/// A top-level window shell for desktop applications.
/// Composes: TitleBar, optional MenuBar, optional Toolbar, NavigationView, optional StatusBar.
/// </summary>
public class DesktopShell : View
{
    public View? TitleBar { get; set; }       // Custom title bar (null = platform chrome)
    public View? MenuBar { get; set; }        // Application menu bar
    public View? Toolbar { get; set; }        // Toolbar strip below menu bar
    public NavigationView Navigation { get; } // Sidebar + content frame
    public View? StatusBar { get; set; }      // Bottom status bar
}
```

**Layout:**

```
┌─────────────────────────────────────────────────────┐
│  TitleBar  (optional custom chrome)                 │  ← Windows: extends into title bar area
├─────────────────────────────────────────────────────┤
│  MenuBar   (optional, macOS: uses system menu)      │
├─────────────────────────────────────────────────────┤
│  Toolbar   (optional)                               │
├──────────────────────────────────────────────────── ┤
│  NavigationView                                     │
│  ┌────────┬──────────────────────────────────────┐  │
│  │        │                                      │  │
│  │  Nav   │      ContentFrame                    │  │
│  │  Pane  │   (active page / panel)              │  │
│  │        │                                      │  │
│  └────────┴──────────────────────────────────────┘  │
├─────────────────────────────────────────────────────┤
│  StatusBar (optional)                               │
└─────────────────────────────────────────────────────┘
```

**macOS specifics:** The title bar is provided by the platform (`NSWindow`). The shell sets the window's `Title` and optionally provides a custom toolbar. Menu bar is managed by `NSApplication` (not within the window hierarchy).

**Windows specifics:** Support for extending content into the title bar area (like WinUI `AppWindow.TitleBar`). The `TitleBar` slot is sized to the non-client area and the window's chrome is transparent.

---

### 3.6 MobileShell

A mobile application shell with optional header/footer strips and a content area:

```csharp
/// <summary>
/// A full-screen mobile application shell.
/// Manages system UI insets (safe areas, notches, IME keyboard).
/// </summary>
public class MobileShell : View
{
    /// <summary>
    /// The main content — typically a NavigationStack or TabView.
    /// </summary>
    public View? Content { get; set; }

    /// <summary>
    /// Optional header pinned above safe area (e.g. a custom status bar).
    /// </summary>
    public View? Header { get; set; }

    /// <summary>
    /// Optional footer pinned below safe area (e.g. a tab bar).
    /// When Content is a TabView, the TabView renders its own bar and this is null.
    /// </summary>
    public View? Footer { get; set; }

    /// <summary>
    /// When true, content extends under the system status bar (edge-to-edge).
    /// The content is responsible for insets via GetService<ISafeArea>().
    /// </summary>
    public bool EdgeToEdge { get; set; } = true;
}
```

---

## 4. Reactive Component Model

### 4.1 Problem Statement

Navigation introduces polymorphism: the content frame's view tree changes as the user navigates. Service changes (e.g. user signs out, theme switches) must propagate to all active pages. The reactive component model addresses both.

### 4.2 Page Lifecycle

A "page" is a `TemplatedView` whose content is constructed lazily and whose data subscriptions are established in `OnActivate` / torn down in `OnDeactivate`. This ensures:

- Platform resources (images, fonts) are available before content is built.
- Observable subscriptions don't accumulate on deactivated pages.
- Navigating back to a page re-activates it without rebuilding the tree.

```csharp
/// <summary>
/// Base class for navigation destination pages.
/// Combines TemplatedView (lazy build) with ReactiveView (observable subscriptions).
/// </summary>
public abstract class Page : TemplatedView
{
    private readonly List<Action> unsubscribeActions = new();

    /// <summary>
    /// Subscribe to an observable. Automatically unsubscribed on deactivate.
    /// </summary>
    protected void Watch<T>(Observable<T> observable, Action<T>? handler = null)
    {
        void OnChanged(T v)
        {
            handler?.Invoke(v);
            this.InvalidateMeasure();
            this.InvalidateRender();
        }
        observable.Changed += OnChanged;
        unsubscribeActions.Add(() => observable.Changed -= OnChanged);
    }

    protected override void OnActivate()
    {
        base.OnActivate();
        ConfigureBindings();
    }

    protected override void OnDeactivate()
    {
        base.OnDeactivate();
        foreach (var a in unsubscribeActions) a();
        unsubscribeActions.Clear();
    }

    /// <summary>
    /// Override to call Watch(…) on the observables this page depends on.
    /// Called each time the page is activated (not just the first time).
    /// </summary>
    protected virtual void ConfigureBindings() { }
}
```

### 4.3 Service Change Propagation

When an application-level service changes (e.g. the current user logs out), the pattern is:

1. The service exposes `Observable<T>` fields for its mutable state.
2. Views/pages call `Watch(service.CurrentUser)` in `ConfigureBindings`.
3. When the user signs out, the service sets `CurrentUser.Value = null`.
4. All watching views are automatically invalidated.

**Example:**

```csharp
// Application service
public class AuthService
{
    public Observable<User?> CurrentUser { get; } = new(null);

    public void SignIn(User user) => CurrentUser.Value = user;
    public void SignOut()         => CurrentUser.Value = null;
}

// Page that reacts to auth state
public class ProfilePage : Page
{
    private AuthService auth = null!;

    protected override void OnAttach(ref AttachEventRef e)
    {
        base.OnAttach(ref e);
        auth = this.GetService<AuthService>()!;
    }

    protected override void ConfigureBindings()
    {
        Watch(auth.CurrentUser);
    }

    protected override View BuildContent() => new Label { Text = auth.CurrentUser.Value?.DisplayName ?? "Guest" };
    // Note: BuildContent() is called once on attach. It reads auth.CurrentUser.Value
    // at that point. Subsequent changes are handled by the Watch() subscription in
    // ConfigureBindings(), which triggers InvalidateRender(). If the AuthService
    // *itself* is replaced in the DI container (not just its CurrentUser value),
    // the page will not automatically update — the page should be re-attached or
    // the application should navigate away and back to rebuild the page.
}
```

### 4.4 Dynamic Content Replacement

Sometimes a reactive view must completely replace its subtree (not just re-render). Example: a page shows a login form when logged out and a dashboard when logged in.

```csharp
/// <summary>
/// A view that re-evaluates and re-realizes its content whenever its
/// observable dependencies change.
/// </summary>
public abstract class DynamicView : Page
{
    protected sealed override void ConfigureBindings()
    {
        base.ConfigureBindings();
        WatchDependencies();
    }

    /// <summary>
    /// Override to call Watch(…) for dependencies that should trigger
    /// content replacement (not just re-render).
    /// </summary>
    protected abstract void WatchDependencies();

    protected void WatchAndRebuild<T>(Observable<T> observable)
    {
        Watch(observable, _ => Realize()); // re-invoke BuildContent on change
    }
}
```

**Usage:**

```csharp
public class AuthGatedView : DynamicView
{
    private AuthService auth = null!;

    protected override void OnAttach(ref AttachEventRef e)
    {
        base.OnAttach(ref e);
        auth = this.GetService<AuthService>()!;
    }

    protected override void WatchDependencies() =>
        WatchAndRebuild(auth.CurrentUser);

    protected override View BuildContent() =>
        auth.CurrentUser.Value is null
            ? new LoginPage()
            : new DashboardPage();
}
```

---

## 5. History and Deep Linking

### 5.1 NavigationHistory

The navigation path is a `List<IDestination>`, which is inherently serializable if destinations are records:

```csharp
public record HomeDestination() : IDestination;
public record ProductListDestination(string CategoryId) : IDestination;
public record ProductDetailDestination(string ProductId) : IDestination;
```

**State restoration:**

```csharp
// Persist — navigationStack is the NavigationStack instance
var serialized = navigationStack.Path.Select(SerializeDestination).ToList();
prefs.Set("nav_path", serialized);

// Restore
var restored = prefs.Get("nav_path").Select(DeserializeDestination).ToList();
navigationStack.Replace(restored);
```

### 5.2 Browser-Style Back/Forward History

On desktop, windows have a back/forward queue beyond a simple stack:

```csharp
public interface IWindowNavigationHistory
{
    bool CanGoBack    { get; }
    bool CanGoForward { get; }
    void GoBack();
    void GoForward();
    IReadOnlyList<IDestination> BackStack    { get; }
    IReadOnlyList<IDestination> ForwardStack { get; }
}
```

`NavigationStack` implements this and publishes `IWindowNavigationHistory` as a window-level service so keyboard shortcuts (`Alt+Left`, `Backspace`, `⌘[`) and toolbar buttons can trigger it.

---

## 6. Implementation Roadmap

### Phase 1 — Reactive Foundations (Xui.Core)
- [ ] `Observable<T>` and `ObservableList<T>` value cells with `Changed` event
- [ ] `ContextProvider<T>` view + `IContextProvider<T>` interface
- [ ] `ViewContextExtensions.GetContext<T>()` with consumer registration
- [ ] `ReactiveView` base class with `Watch<T>` + `ConfigureBindings()`
- [ ] `TemplatedView` base class with lazy `BuildContent()` on `OnAttach`
- [ ] `SlottedView` with `SlotContent` projection
- [ ] `Page` base class (combines `TemplatedView` + reactive subscriptions)
- [ ] `DynamicView` base class for content replacement on observable change

### Phase 2 — Navigation Primitives (Xui.Core.UI.Navigation)
- [ ] `IDestination` marker interface
- [ ] `INavigationRouter` — maps destination → View
- [ ] `INavigationController` interface + service resolution
- [ ] `ITabController` interface + service resolution
- [ ] `NavigationTransition` abstract + `SlideTransition`, `FadeTransition`, `NoneTransition`
- [ ] `NavigationStack` with push/pop/replace, transitions, `INavigationController` service
- [ ] `NavigationBar` reactive view (title + back button)
- [ ] `TabView` with `TabItem`, `TabBarPosition`, activate/deactivate semantics
- [ ] `IWindowNavigationHistory` + back/forward queue

### Phase 3 — Shell Components (Xui.Core.UI.Shell)
- [ ] `NavigationItem` record
- [ ] `NavigationView` with sidebar, icon rail, overlay, and top-tab display modes
- [ ] Width-adaptive layout with `NavigationPaneDisplayMode.Auto`
- [ ] `DesktopShell` with title bar slot, menu bar, toolbar, navigation view, status bar
- [ ] `MobileShell` with safe area inset management, edge-to-edge support
- [ ] `ISafeArea` service (provided by `MobileShell`, consumed by pages)

### Phase 4 — Design System Integration
- [ ] `ITheme` context provider consumed by shell and all navigation chrome
- [ ] `NavigationView` visual polish: animation on pane expand/collapse, keyboard focus ring
- [ ] Transition animations driven by `Easing.SmoothDamp` (already available in `Xui.Core.Animation`)
- [ ] Accessibility: focus traversal across navigation boundaries, landmark roles

### Phase 5 — Platform Adapters
- [ ] **macOS:** `DesktopShell` integrates with `NSSplitViewController`-style layout; `NavigationView` respects macOS sidebar conventions.
- [ ] **Windows:** `DesktopShell` extends content into title bar; `NavigationView` follows WinUI 3 `NavigationView` visual spec.
- [ ] **iOS/Android:** `MobileShell` + `TabView` maps to `UITabBarController` / bottom nav bar; `NavigationStack` maps to `UINavigationController`.
- [ ] **Browser:** All primitives work purely in software; URL hash reflects navigation path.

---

## 7. Key Design Decisions

### Decision 1: Destination-Based vs Imperative Navigation
**Chosen: Destination-based.** Imperative push(`new MyPage()`) couples the caller to the concrete page class and makes state restoration impossible. A destination value (`record MyPageDestination(int Id)`) is serializable, testable, and decoupled from the UI.

### Decision 2: Lazy Template Realization (OnAttach vs OnActivate)
**Chosen: OnAttach.** `OnAttach` fires when the view is connected to a window, making platform services available. `OnActivate` fires when the view is live (rendering). Building the subtree in `OnAttach` means it is layout-ready before activation, and platform services (fonts, image pipelines) are accessible during `BuildContent`. Observable subscriptions, however, are set up in `OnActivate` to avoid receiving updates for off-screen pages.

### Decision 3: Tab Persistence
**Chosen: Persist by default (deactivate, do not detach).** Tabs remain attached (keeping loaded images, measured text) but are deactivated (no rendering, no event handling) when not selected. This matches iOS `UITabBarController` semantics. An opt-out (`PersistWhenInactive = false`) allows memory-constrained scenarios to detach.

### Decision 4: Observable<T> vs INotifyPropertyChanged
**Chosen: `Observable<T>`.** `INotifyPropertyChanged` is property-name-string-based and requires reflection or source generators. `Observable<T>` is a typed, allocation-light value cell. Views call `Watch(service.Name)` — strongly typed, IDE-navigable, no reflection.

### Decision 5: Context Provider vs Global Service Locator
**Chosen: Context provider (tree-scoped).** A global service locator breaks component reuse (a component assumes a global `AuthService` always exists). A `ContextProvider<AuthService>` placed at the navigation root makes the service available exactly where it is needed, allows testing with mocks, and multiple instances are naturally isolated.

---

## 8. Open Questions

1. **Animation interruption:** When the user taps back while a push animation is in progress, should the in-flight animation reverse, snap to final state, or complete? Current recommendation: snap to final state, then pop.

2. **NavigationView + NavigationStack composition:** On desktop, the navigation view's sidebar items each navigate the content frame. Should the content frame *be* a `NavigationStack`, or is it a simple single-child slot? Recommendation: the frame *contains* a `NavigationStack`; selecting a sidebar item replaces the *root* of that stack.

3. **Multi-window:** On macOS and iPad, multiple windows can show the same navigation graph. Should `INavigationController` be per-window (yes, default) or shared (opt-in via a shared `Observable<List<IDestination>>`)?

4. **Back gesture (iOS/Android):** The swipe-back gesture requires the incoming page to be partially rendered behind the current page during the gesture. This requires a "peek" rendering mode where the previous page in the stack is kept attached but clipped.

5. **Keyboard navigation (desktop):** `Alt+Left` / `Alt+Right` should trigger `IWindowNavigationHistory.GoBack()` / `GoForward()`. This requires the window to expose a keyboard shortcut registration mechanism.
