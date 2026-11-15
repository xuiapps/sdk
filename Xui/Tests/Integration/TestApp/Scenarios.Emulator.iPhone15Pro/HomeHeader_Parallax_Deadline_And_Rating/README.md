# Integration Test Run

## Timeline

# Home header

The header is an overlay inside the scroll gesture route. Pulling moves it at 60% of the content's displacement. From offset 75 to 225 it collapses into a toolbar; the 0.7-second deadline starts a 0.56-second eased finish. Returning keeps the toolbar pinned until offset 175, then expands fully by offset 25, with the same deadline and easing. Expanded elements can move above the viewport. Heart, FPS and settings remain tappable in either layout.

### 01. Expanded

<img src="./01.Expanded.Render.svg" alt="01. Expanded expected" />

### 02. Pulled

<img src="./02.Pulled.Render.svg" alt="02. Pulled expected" />

### 03. PartlyCollapsed

<img src="./03.PartlyCollapsed.Render.svg" alt="03. PartlyCollapsed expected" />

### 04. Toolbar

<img src="./04.Toolbar.Render.svg" alt="04. Toolbar expected" />

### 05. Rating

<img src="./05.Rating.Render.svg" alt="05. Rating expected" />

### 06. PartlyExpanded

<img src="./06.PartlyExpanded.Render.svg" alt="06. PartlyExpanded expected" />

### 07. Returned

<img src="./07.Returned.Render.svg" alt="07. Returned expected" />
