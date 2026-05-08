# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Epic Origin** (史诗起点) — a turn-based 2D strategy/roguelike game built with Unity 2022.3.62f3c1. The project is in early development.

## Build & Run

- Open `Epic_Origin.sln` in JetBrains Rider or the project root in Unity Editor
- Open scene `Assets/Scenes/MainScene.unity` and press Play in the Editor
- No CI/CD or automated build pipeline is configured

## Architecture

### Entry Point & Lifecycle

`MainScene.unity` is the only scene. The scene sets up:
- A `MapGenerator` GameObject that creates the map at startup
- A `MapManager` singleton that provides grid query APIs
- A `GameManager` singleton controlling turn flow
- A `Hero` GameObject for the player unit
- A `UIManager` for HUD

### Core Managers (Singleton Pattern)

All managers use the Unity `MonoBehaviour` singleton pattern with a public static `Instance` field set in `Awake()`.

- **`GameManager`** (`Assets/Scripts/Core/GameManager.cs`) — Turn-based game loop: `PlayerTurn → AITurn → PlayerTurn → ...`. Tracks `currentTurn`, `isPlayerTurn`, and `hasPlayerActed` (one action per turn). AI turn is simulated with a 1.5-second delay (`Invoke`). Players must act before ending their turn.
- **`MapManager`** (`Assets/Scripts/Map/MapManager.cs`) — Holds a 2D array of `Tile` references. Provides `GetTileAt(Vector2Int)`, `WorldToGrid()`, and `GridToWorld()` for coordinate conversion. Configured with `width`, `height`, and `tileSize` (default: 10×10, 1.2 spacing).
- **`InputManager`** (`Assets/Scripts/Core/InputManager.cs`) — Translates mouse clicks into grid coordinates and delegates to `Hero.TryMove()`. References the `Hero` directly.
- **`UIManager`** (`Assets/Scripts/UI/UIManager.cs`) — Updates turn/state text each frame via `UpdateUI()`. Exposes `OnEndTurnButton()` bound to the end-turn button.

### Map System

- **`MapGenerator`** (`Assets/Scripts/Map/MapGenerator.cs`) — Spawns tiles in a 10×10 grid at startup, selecting prefabs randomly (70% empty, 10% resource, 10% army camp, 5% event, 5% stronghold). Pushes the tile array to `MapManager.SetMap()`.
- **`Tile`** (abstract base, `Assets/Scripts/Map/Tile.cs`) — Holds `gridPosition` (Vector2Int) and a virtual `OnHeroEnter()` callback. All tile types inherit from this:
  - `EmptyTile` — passable, no effect
  - `ResourceTile` — placeholder for resource collection
  - `ArmyCampTile` — placeholder for battle encounters
  - `EventTile` — placeholder for random events
  - `StrongholdTile` — placeholder for stronghold interactions
- Tile prefabs live in `Assets/Prefabs/Tile/` and have the corresponding script component attached.

### Unit System

- **`Hero`** (`Assets/Scripts/Units/Hero.cs`) — Player-controlled unit with grid-based movement. Movement constraints:
  - Only during player turn (`GameManager.Instance.isPlayerTurn`)
  - Only once per turn (`GameManager.Instance.hasPlayerActed`)
  - Manhattan distance ≤ 3 from current position
  - Uses `Vector3.MoveTowards` for smooth interpolation at `moveSpeed`
  - Calls `tile.OnHeroEnter()` when arrival completes, after which the tile subclass triggers its specific logic

### Key Dependencies

- **TextMeshPro** (3.0.7) — UI text rendering (turn counter, state display)
- **UOS Launcher** (`cn.unity.uos.launcher`) — Chinese Unity Online Services framework, pulled from an external git URL
- **2D Feature Set** (`com.unity.feature.2d`) — Standard Unity 2D packages (tilemap, sprite shape, animation, etc.)
- **Unity Test Framework** (1.1.33) — installed but no tests exist yet

## Development Conventions

### Code Style

- C# scripts follow Unity conventions: `MonoBehaviour` classes, public fields for Inspector-exposed properties, `[SerializeField]` not used (public fields instead)
- Chinese comments and debug log messages throughout
- No namespaces are used; all scripts are in the global namespace
- Scripts are organized by system in `Assets/Scripts/`: `Core/`, `Map/`, `Units/`, `UI/`

### Git Workflow

- **Branch naming**: `feature/<feature-name>` for feature branches, `feature-<name>` for older branches
- **Base branch**: `main`
- Current active branches: `feature/Map_logic` (map + hero movement), `feature/game-manager`, `feature-ui-mainmenu`
- No CI checks or hooks are configured

## Design Documents

The `docs/` directory contains the game design documents (PRD, proposals, workflow diagrams) in Chinese. Key reference files:
- `《史诗起点》游戏设计需求文档（PRD）.docx` — Full PRD
- `史诗起点_简化版策划案（终稿-目前需求）.docx` — Current simplified design spec
- `Game Logic.png`, `Work Flow.png` — Architecture diagrams
