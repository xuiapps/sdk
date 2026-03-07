---
title: View — Hierarchy
description: Count, the indexer, and the protected helpers for managing child lifetime.
---

# View — Hierarchy

Every `View` can act as a container. The hierarchy API is intentionally minimal: views expose their children through a virtual `Count` / indexer pair, and containers manage child lifetime through protected helpers.

## Exposing children

```csharp
public virtual int Count { get; } = 0;
public virtual View this[int index] { get; }
```

Leaf views return `Count = 0` (the default). Container views override both. The rest of the framework — layout, event routing, lifecycle traversal — uses only this pair to iterate the tree.

## Managing children

Direct manipulation of `Parent` is not allowed. Containers use the protected helpers instead:

```csharp
protected void AddProtectedChild(View child)
protected void RemoveProtectedChild(View child)
protected void SetProtectedChild<T>(ref T? field, T? value)
```

`SetProtectedChild` is a convenience wrapper for single-child hosts (e.g. `ContentView`, `RootView`). It detaches the old child and attaches the new one atomically.

All three helpers:
- Set or clear `child.Parent`
- Run `AttachSubtree` / `DetachSubtree` if the parent is already attached
- Run `ActivateSubtree` / `DeactivateSubtree` if the parent is already active
- Call `InvalidateArrange()` and `InvalidateRender()` so the parent re-lays-out

See [Lifecycle](lifecycle.md) for the attach/activate sequence and [State](state.md) for invalidation.
