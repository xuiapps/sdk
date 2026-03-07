---
title: View — Layout
description: The Animate/Measure/Arrange/Render pipeline, Shell vs Core, LuminarFlow single-pass DFS, and the fork pattern.
---

# View — Layout

Every frame, the window asks its root view to prepare and render the frame. That single call propagates through the entire tree. Each view in the tree has one job: call `Update` on itself and, where appropriate, on its children. The goal is to get every view to the point where it can draw — with the minimum amount of work.

## Update, LuminarFlow, and the fork

```
Update
 ├─ (override)      ← single-pass DFS when all four stages fit in one traversal
 │
 OR, when a fork is required:
 ├─ Animate
 ├─ Measure         (optional — 0, 1, or many times)
 ├─ Arrange
 └─ Render          (called exactly once)
```

`Update` is the entry point — and it is `virtual`. When a view and all its children can be fully resolved in a single depth-first walk, the view overrides `Update` and handles all four stages itself. This is the **LuminarFlow** path: one call, one traversal, frame done.

The root view always starts a frame with a LuminarFlow call. Leaf views almost always remain in LuminarFlow — a label, an image, or an icon has no children to negotiate with. Even complex views like a virtualised grid can stay in LuminarFlow: the grid already knows each cell's size from its data model, so it can measure, arrange, and render each cell in a single pass without asking any cell to influence row height or column width.

### The fork

Some containers cannot stay in a single pass. Consider a **right-aligned `VerticalStack`** containing **left-aligned labels**. Before the stack can position any label, it must know the width of the widest one — but that width only emerges *after* measuring all of them. The stack must:

1. Measure every child (possibly multiple times with different constraints)
2. Decide on a final width from the results
3. Arrange every child using that width
4. Render

This split is called a **fork** — like the utensil: one handle going in, multiple tines coming out, rejoining at the other end (Render). A fork introduces disturbance in the update flow, replacing the single traversal with a sequence of separate passes. It is sometimes unavoidable. Measure may be called zero, one, or many times; Arrange follows once a size is settled; Render is called exactly once.

A view that forks calls the individual pass methods on its children directly from `MeasureCore` / `ArrangeCore`, and does **not** override `Update`:

```csharp
// inside a forking container's MeasureCore / ArrangeCore:
nfloat maxWidth = 0;
for (int i = 0; i < this.Count; i++)
    maxWidth = nfloat.Max(maxWidth, this[i].Measure(available, ctx).Width);

for (int i = 0; i < this.Count; i++)
    this[i].Arrange(new Rect(x, y, maxWidth, ...), ctx);
```

## The four passes

| Pass | Shell | Core override | Purpose |
|---|---|---|---|
| Animate | `AnimateShell` | `AnimateCore` | Advance time-based state, request next frame |
| Measure | `MeasureShell` | `MeasureCore` | Return desired border-edge size |
| Arrange | `ArrangeShell` | `ArrangeCore` | Position children within the allocated rect |
| Render | `RenderShell` | `RenderCore` | Draw content to the rendering context |

### Shell vs Core

Each pass has two layers:

- **Shell** (`protected`, non-virtual) — handles bookkeeping: clamping to `MinimumWidth`/`MaximumWidth`, computing alignment offsets, assigning `Frame`, logging. It calls the corresponding Core.
- **Core** (`protected virtual`) — the override point for subclasses. Only deals with the view's own content and children. The default implementation recurses into children.

Override Core, not Shell.

```csharp
protected override Size MeasureCore(Size available, IMeasureContext ctx)
{
    // measure my content, ask children, return desired size
}
```

## Update — the entry point

`Update(LayoutGuide)` is the central, virtual entry point. It examines the `LayoutPass` flags on the guide and routes each pass to its shell:

```csharp
public virtual LayoutGuide Update(LayoutGuide guide)
```

The public convenience methods build the guide automatically:

```csharp
Size desired = child.Measure(available, ctx);
Rect frame   = child.Arrange(rect, ctx);
child.Render(ctx);
child.Animate(prev, current);
```

Views may also be arranged and rendered directly, skipping Measure entirely, when their size is known from an external source (e.g. a virtualised grid cell whose dimensions come from the model).

## LuminarFlow — single-pass DFS

`Update` is virtual. Views that can handle all four stages in one depth-first traversal should override it and check `guide.IsLuminarFlow`:

```csharp
public override LayoutGuide Update(LayoutGuide guide)
{
    if (!guide.IsLuminarFlow)
        return base.Update(guide); // fall back to per-pass dispatch

    // Animate, then for each child in one DFS walk:
    this.AnimateShell(ref guide);
    for (int i = 0; i < this.Count; i++)
        this[i].Update(guide.WithChildConstraints(...));
    this.RenderShell(ref guide);
    return guide;
}
```

Prefer overriding `Update` over the individual Core virtuals whenever the layout algorithm allows a single traversal — it avoids redundant tree walks and keeps the hot path tight.

| | LuminarFlow | Fork |
|---|---|---|
| Tree walks | 1 | 2–3 |
| Backtracks | Never | Yes — must know all sizes before arranging any |
| Override | `Update` | nothing — call child pass methods from Core virtuals |
| Typical views | Root, leaf views, virtualised grids | Centred/right-aligned stacks, wrap panels, grids that negotiate size |

## LayoutGuide

`LayoutGuide` is a value type (`struct`) that carries all inputs and outputs for a pass:

| Field | Used in |
|---|---|
| `Pass` (`LayoutPass` flags) | All — controls which shells run |
| `AvailableSize`, `XSize`, `YSize` | Measure |
| `DesiredSize` | Measure output → Arrange input |
| `Anchor`, `XAlign`, `YAlign` | Arrange |
| `ArrangedRect` | Arrange output |
| `MeasureContext` | Measure + Arrange |
| `RenderContext` | Render |
| `PreviousTime`, `CurrentTime` | Animate |
| `Instruments` | All — zero-alloc logging |

`LayoutPass` flags: `Animate = 1`, `Measure = 2`, `Arrange = 4`, `Render = 8`, `LuminarFlow = 15`.
