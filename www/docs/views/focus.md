---
title: View — Focus
description: Focusable, IsFocused, Focus/Blur, and the IFocus service.
---

# View — Focus

Keyboard focus determines which view receives `OnKeyDown` and `OnChar` events. Focus is managed through the `IFocus` service, which is resolved via the parent chain and provided by `RootView`.

## Participating in focus

```csharp
public virtual bool Focusable => false;
```

Override and return `true` to opt in to Tab navigation. Only focusable views are visited during `IFocus.Next()` / `IFocus.Previous()`.

## Querying and changing focus

```csharp
public bool IsFocused { get; }   // true if this view holds focus
public bool Focus()              // request focus; returns false if no IFocus service
public void Blur()               // release focus from this view
```

All three resolve `IFocus` via `this.GetService<IFocus>()` (see [DI](di.md)). If the view is not in a tree rooted at a `RootView`, the service will be `null` and `Focus()` returns `false`.

## Focus notifications

```csharp
protected internal virtual void OnFocus() { }
protected internal virtual void OnBlur()  { }
```

These are called by `RootView` when `IFocus.FocusedView` changes. Override to react to focus changes — for example to start a caret blink animation in `OnFocus` and stop it in `OnBlur`.

## IFocus service

`RootView` implements `IFocus` and registers itself via `GetService`:

```csharp
View? FocusedView { get; set; }  // get/set the focused view directly
void Next()                      // move to the next focusable (Tab)
void Previous()                  // move to the previous focusable (Shift+Tab)
```

Tab navigation performs a depth-first walk of the tree, collecting all `Focusable` views in order.
