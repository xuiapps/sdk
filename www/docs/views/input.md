---
title: View — Input
description: Pointer events, scroll wheel, and keyboard input.
---

# View — Input

Input events are dispatched by `RootView`'s `EventRouter` and delivered to views through virtual methods. Override the relevant methods in a subclass to handle input.

## Pointer events

```csharp
public virtual void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
```

Called for mouse and touch pointer events during a specific phase of dispatch (e.g. bubble, tunnel). Override to handle press, move, and release.

For pointer capture (receiving events outside the view's bounds) see `CapturePointer` / `ReleasePointer` in [State](state.md).

## Scroll wheel

```csharp
public virtual void OnScrollWheel(ref ScrollWheelEventRef e)
```

Called when a scroll wheel or trackpad scroll is dispatched to this view. Set `e.Handled = true` to stop propagation.

## Keyboard

Keyboard events are delivered only to the view that currently holds focus (see [Focus](focus.md)).

```csharp
public virtual void OnKeyDown(ref KeyEventRef e)
public virtual void OnChar(ref KeyEventRef e)
```

`OnKeyDown` receives raw key codes (including modifier keys). `OnChar` receives character input after platform key composition, and is the right place to handle text entry.
