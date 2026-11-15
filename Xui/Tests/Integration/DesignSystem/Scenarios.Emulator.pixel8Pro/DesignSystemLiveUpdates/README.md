# Integration Test Run

## Timeline

# Design system — edit, preview, interact

The mobile editor uses vector choices and moving selection indicators. Scroll inertia is delivered through the page animation loop; Apple profiles also spring back at the edges. The palette and preview retain their scroll positions and share a single live design system. Images and fonts are embedded in the SVG so these snapshots remain self-contained.

### 01. Palette

<img src="./01.Palette.Render.svg" alt="01. Palette expected" />

Dragging the wheel updates the theme immediately, without replacing the design-system provider.

### 02. ChangedColor

<img src="./02.ChangedColor.Render.svg" alt="02. ChangedColor expected" />

### 03. ShapeAndDensity

<img src="./03.ShapeAndDensity.Render.svg" alt="03. ShapeAndDensity expected" />

### 04. MotionOptions

<img src="./04.MotionOptions.Render.svg" alt="04. MotionOptions expected" />

A sliding segment and eased page movement connect the editor to the live preview.

### 05. SwitchingToPreview

<img src="./05.SwitchingToPreview.Render.svg" alt="05. SwitchingToPreview expected" />

### 06. LivePreview

<img src="./06.LivePreview.Render.svg" alt="06. LivePreview expected" />

Cards, actions, toggles, and the expandable section use the updated tokens. Descriptions wrap inside their cards, and controls have a separate aligned column.

### 07. ControlsAndMotion

<img src="./07.ControlsAndMotion.Render.svg" alt="07. ControlsAndMotion expected" />
