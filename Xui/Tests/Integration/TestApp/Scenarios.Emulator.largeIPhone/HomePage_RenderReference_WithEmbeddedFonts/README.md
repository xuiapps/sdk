# Integration Test Run

## Timeline

# TestApp home-screen rendering reference

Mobile TestApp on a 430×932 logical-point iPhone reference surface, including emulator frame, safe areas, and deterministic status bar (478×980 SVG). Initial home screen, before navigation or pointer input.

Rendered by the deterministic software SVG context with embedded Inter font subsets. The SVG records fills, strokes, text, transforms, and clipping for a future standalone drawing experiment; it is a visual reference, not a recording of native CGContext calls or an iOS timing measurement.

### 01. HomePage

<img src="./01.HomePage.Render.svg" alt="01. HomePage expected" />

## Moving top page

Push and pop a simple probe page. Only the moving top page gets a rounded clip; one stationary background covers the display, a translucent shade darkens the back page, and a native outline shadow follows the front page. Settled navigation draws only its active page. Full-screen fill counts exclude transparent surfaces and SVG shadow approximations. Opaque navigation keeps host clearing disabled throughout. The probe uses linear easing so the midpoint snapshots show the top page halfway across the display.

### 02. PushHalfway

<img src="./02.PushHalfway.Render.svg" alt="02. PushHalfway expected" />

### 03. Pushed

<img src="./03.Pushed.Render.svg" alt="03. Pushed expected" />

### 04. PopHalfway

<img src="./04.PopHalfway.Render.svg" alt="04. PopHalfway expected" />

### 05. Returned

<img src="./05.Returned.Render.svg" alt="05. Returned expected" />
