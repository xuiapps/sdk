---
title: View
description: The View base class — properties, hit testing, and an overview of the partial files.
---

# View

`View` is the base class for every element in the Xui visual tree. A view participates in layout, rendering, input hit-testing, and may contain child views.

The class is split across several partial files, each covering one concern:

| Article | Covers |
|---|---|
| [Hierarchy](hierarchy.md) | `Count`, indexer, add/remove child helpers |
| [Lifecycle](lifecycle.md) | `OnAttach`, `OnActivate`, `OnDeactivate`, `OnDetach` |
| [Layout](layout.md) | `Update`, Measure/Arrange/Render pipeline, LuminarFlow vs FORK |
| [State](state.md) | `ViewFlags`, invalidation, validation, change propagation |
| [Input](input.md) | Pointer, scroll wheel, keyboard events |
| [Focus](focus.md) | `Focusable`, `IsFocused`, `Focus()`, `Blur()` |
| [DI](di.md) | `IServiceProvider`, `GetService`, parent-chain resolution |

## Properties

| Property | Description |
|---|---|
| `Id` | Optional identifier for lookup via `ViewExtensions.FindViewById` |
| `ClassName` | Class names for lookup via `ViewExtensions.FindViewsByClass` |
| `Parent` | Parent in the visual hierarchy (set automatically) |
| `Frame` | Border-edge rectangle in window-relative coordinates (set during Arrange) |
| `Margin` | External spacing; participates in collapsed margin logic |
| `HorizontalAlignment` | How the view fills its horizontal slot (default: `Stretch`) |
| `VerticalAlignment` | How the view fills its vertical slot (default: `Stretch`) |
| `Direction` | Block/inline flow direction (inherited if `Inherit`) |
| `WritingMode` | Horizontal or vertical text flow |
| `Flow` | How the layout system treats children |
| `MinimumWidth` / `MinimumHeight` | Size floor for the border-edge box |
| `MaximumWidth` / `MaximumHeight` | Size ceiling for the border-edge box |

## Hit testing

```csharp
public virtual bool HitTest(Point point)
```

Returns `true` if `point` (in window coordinates) falls within this view or any child. The default implementation iterates children in reverse order (top-most first) and falls back to `Frame.Contains(point)`.
