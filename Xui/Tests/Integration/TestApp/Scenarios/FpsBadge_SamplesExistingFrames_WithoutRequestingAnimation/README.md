# Integration Test Run

## Timeline

# Passive FPS badge

A two-second startup spinner collects requested-frame timings before showing the number. Idle gaps are excluded; animation ticks remain 60 Hz even when retained content only redraws at 30 Hz. After warmup the badge requests no animation frames. The detailed overlay remains an explicit continuous-rendering mode.

### 01. PassiveBadge

<img src="./01.PassiveBadge.Render.svg" alt="01. PassiveBadge expected" />
