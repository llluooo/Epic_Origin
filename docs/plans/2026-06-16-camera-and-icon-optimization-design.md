# Camera & Icon Optimization Design

Date: 2026-06-16

## Summary

Two problems:
1. Camera too far — map and icons appear too small
2. Hero movement feels too fast, especially jarring with a zoomed-in camera

Also: Hero overlaps with POI icons visually; different POI types need independent size controls.

## Changes

### 1. Camera Zoom (CameraFollow.cs)

- Add `public float targetOrthographicSize = 3f` field
- `Start()` sets Camera.orthographicSize to this value automatically
- ~5 tiles visible vertically, ~9 tiles horizontally at 16:9
- Existing bounds clamping logic works without changes

### 2. Hero Visual Offset + Speed (Hero.cs)

- Add `public Vector3 visualOffset = new Vector3(0, -0.35f, 0)` field
- Target position = `GridToWorld(gridPos) + visualOffset` — Hero stands slightly below tile center, POI icon above
- Reduce `moveSpeed` default from `5` to `2.5` — one tile takes ~0.5s, feels natural at close range

### 3. Per-POI Scale Controls (MapGenerator.cs)

Replace single `poiVisualScale` with three separate fields:
- `resourcePoiScale = 0.55f`
- `armyCampPoiScale = 0.55f`
- `eventPoiScale = 0.55f`

Update `GetPoiScale(TileVisualRole)` to return the matching value. `strongholdVisualScale` and `overlayVisualScale` remain single-parameter, adjusted down slightly (0.75, 0.85).
