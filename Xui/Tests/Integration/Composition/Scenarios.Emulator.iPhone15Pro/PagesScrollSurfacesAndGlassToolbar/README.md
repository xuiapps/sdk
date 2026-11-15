# Integration Test Run

## Timeline

# Retained page composition

Pages, cards, and scrolling viewports are separate keyed surfaces. Card borders and shadows follow rounded outlines, and culled cards can reuse their retained drawings. The toolbar uses native Liquid Glass with a translucent selection. Headers blur the content scrolling beneath them, including the top safe area. SVG approximates the materials while keeping labels crisp.

### 01. Home

<img src="./01.Home.Render.svg" alt="01. Home expected" />

### 02. PageTransition

<img src="./02.PageTransition.Render.svg" alt="02. PageTransition expected" />

### 03. GlassToolbar

<img src="./03.GlassToolbar.Render.svg" alt="03. GlassToolbar expected" />

### 04. BlurredHeader

<img src="./04.BlurredHeader.Render.svg" alt="04. BlurredHeader expected" />
