---
title: Image Loading
description: IImage, IImagePipeline, DrawImage, and image pattern fill.
---

# Image Loading

Xui's image API mirrors `HTMLImageElement` in the browser:
acquire a handle, set the source, draw.

## Acquiring an image handle

Images are transient services — each call returns a new handle backed by the platform's cached pipeline:

```csharp
protected override void OnAttach(ref AttachEventRef e)
{
    image = this.GetService<IImage>();
    image?.Load("Assets/photo.png");
}

protected override void OnDetach(ref DetachEventRef e)
{
    image = null;
}
```

`GetService<IImage>()` walks up the view tree to the window's platform runtime, which vends a `DirectXImage` (on Windows) backed by `DirectXImageFactory`.

## Loading

```csharp
image.Load("Assets/photo.png");            // sync — instant if cached, blocks on first load
await image.LoadAsync("Assets/photo.png"); // async — first decode on background thread
```

Paths are resolved relative to `AppContext.BaseDirectory`. Absolute paths are also accepted.

The factory caches decoded images by URI — subsequent `Load` calls with the same path return instantly.

## Drawing

```csharp
protected override void RenderCore(IContext context)
{
    if (image is null || image.Size == Size.Empty) return;

    // stretch to fill a rect
    context.DrawImage(image, this.Frame);

    // with opacity
    context.DrawImage(image, destRect, opacity: 0.5f);

    // source sub-region
    context.DrawImage(image, sourceRect, destRect, opacity: 1f);
}
```

## Pattern fill

Use an image as a repeating fill for any path:

```csharp
context.SetFill(new ImagePattern(image));                          // tile in both axes
context.SetFill(new ImagePattern(image, PatternRepeat.RepeatX));   // tile horizontally only
context.SetFill(new ImagePattern(image, PatternRepeat.NoRepeat));  // single image, no tile

context.BeginPath();
// ... build any path ...
context.Fill();
```

`ImagePattern` is a `ref struct` — it is never boxed.

## Device-lost recovery

On Windows, if the D3D device is lost (GPU reset, driver update), the platform runtime calls `DirectXImageFactory.Rehydrate()`, which re-uploads all cached images to the new device. Existing `IImage` handles remain valid — their internal resource pointer is updated in-place.

Application code does not need to handle device-lost explicitly.

## Built-in ImageView

For simple image display, use `ImageView` instead of writing `RenderCore` yourself:

```csharp
var view = new ImageView { Source = "Assets/photo.png" };
```

`ImageView` acquires `IImage` via `GetService<IImage>()` in `OnActivate`, loads on `Source` set, and scales to fit while preserving aspect ratio.
