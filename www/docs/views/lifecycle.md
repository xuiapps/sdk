---
title: View — Lifecycle
description: OnAttach, OnActivate, the frame loop, OnDeactivate, and OnDetach.
---

# View — Lifecycle

A view moves through a well-defined lifecycle as it enters and leaves the visual tree. The sequence is:

```
OnAttach
  OnActivate
    AnimateCore  ←─┐
    MeasureCore    │  repeating per frame
    ArrangeCore    │  (see Layout)
    RenderCore   ──┘
  OnDeactivate
OnDetach
```

## Attach / Detach

These events fire when the view is connected to (or disconnected from) a live platform window.

```csharp
protected virtual void OnAttach(ref AttachEventRef e) { }
protected virtual void OnDetach(ref DetachEventRef e) { }
```

`OnAttach` is the right place to acquire resources that require platform context — e.g. loading a bitmap via `IBitmapFactory` or caching a font metric. `OnDetach` should release them.

Subtree traversal is **top-down** for attach (parent before children) and **bottom-up** for detach (children before parent).

## Activate / Deactivate

These events fire when the view becomes live (will receive events, animate, and render) or dormant.

```csharp
protected virtual void OnActivate()   { }
protected virtual void OnDeactivate() { }
```

A view can be in the tree but inactive — for example, a view held in a virtualising panel's recycle pool. `OnActivate` is the right place to start animations or subscribe to data sources. `OnDeactivate` should stop them.

Subtree traversal is **top-down** for activate and **bottom-up** for deactivate, matching the attach/detach convention.

## Flags

The `Attached` and `Active` bits on `ViewFlags` reflect the current state and are set/cleared automatically by the subtree helpers:

| Flag | Set by | Cleared by |
|---|---|---|
| `ViewFlags.Attached` | `AttachSubtree` | `DetachSubtree` |
| `ViewFlags.Active` | `ActivateSubtree` | `DeactivateSubtree` |

See [State](state.md) for the full flag reference.

## The frame loop

While active, a view participates in the repeating frame loop via the layout pipeline. See [Layout](layout.md) for how `AnimateCore`, `MeasureCore`, `ArrangeCore`, and `RenderCore` fit into that loop.
