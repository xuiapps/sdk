---
title: View — State
description: ViewFlags, the invalidate/validate dirty-flag system, animation requests, and pointer capture.
---

# View — State

`View.State.cs` owns the dirty-flag system that lets the framework skip work it does not need to redo.

## ViewFlags

`ViewFlags` is a bit-field on every view. Flags come in pairs: one for the view itself and one for "any descendant has this flag".

| Flag | Meaning |
|---|---|
| `Animated` | This view has time-dependent state; needs `AnimateCore` next frame |
| `DescendantAnimated` | A descendant requested an animation frame |
| `MeasureChanged` | This view's desired size may have changed |
| `ArrangeChanged` | This view's child layout may have changed |
| `DescendantArrangeChanged` | A descendant needs re-arranging |
| `RenderChanged` | This view needs to be redrawn |
| `DescendantRenderChanged` | A descendant needs to be redrawn |
| `HitTestChanged` | This view's hit-test result may have changed |
| `DescendantHitTestChanged` | A descendant's hit-test result may have changed |
| `Active` | View is live — receives events, animates, renders |
| `Attached` | View is connected to a platform window |

## Invalidation

Views signal that something has changed by calling an `Invalidate*` method. Each one:
1. Sets the matching flag on `this`
2. Calls the corresponding `OnChild*Changed` on the parent, which propagates the `Descendant*` flag up the tree

```csharp
protected void InvalidateMeasure()
protected void InvalidateArrange()
protected void InvalidateRender()
protected void InvalidateHitTest()
```

Propagation stops as soon as the ancestor already carries the `Descendant*` flag — no redundant notifications travel further up.

## Validation

Layout shells clear flags after processing a view:

```csharp
public void ValidateMeasure()
public void ValidateArrange()   // also clears DescendantArrangeChanged
public void ValidateRender()    // also clears DescendantRenderChanged
public void ValidateHitTest()   // also clears DescendantHitTestChanged
```

A parent can call `ValidateMeasure()` on a child to acknowledge a measure change it has decided to absorb (e.g. a grid that keeps column widths stable regardless of cell content changes).

## Animation

```csharp
protected void RequestAnimationFrame()
```

Call this inside `AnimateCore` when the animation should continue on the next frame. Sets `ViewFlags.Animated` and notifies ancestors via `OnChildRequestedAnimationFrame`.

## Pointer capture

```csharp
public void CapturePointer(int pointerId)
public void ReleasePointer(int pointerId)
```

Captures or releases a pointer at the `EventRouter` level (resolved via the parent chain to `RootView`). While captured, pointer events are delivered to this view regardless of hit testing.
