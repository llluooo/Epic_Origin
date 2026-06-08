# AGENTS.md

This file provides guidance to Codex (Codex.ai/code) when working with code in this repository.

## Project Overview

**Epic Origin** (史诗起点) — a turn-based 2D strategy/roguelike game built with Unity 2022.3.62f3c1. The project is in early development. Gameplay loop: explore map → collect resources → upgrade stronghold & summon units → battle enemy camps → conquer enemy stronghold.

## Build & Run

- Open `Epic_Origin.sln` in JetBrains Rider or the project root in Unity Editor
- Open scene `Assets/Scenes/MainScene.unity` and press Play in the Editor
- No CI/CD or automated build pipeline is configured

## Architecture

### Entry Point & Lifecycle

`MainScene.unity` is the only scene. The scene sets up:
- A `MapGenerator` GameObject that creates the map and initializes hero position at startup
- A `MapManager` singleton that provides grid query APIs
- A `GameManager` singleton controlling turn flow, player data, resource production, and win/loss
- A `Hero` GameObject for the player unit
- A `UIManager` for HUD (turn, state, resources)
- A `StrongholdUI` panel for stronghold management (upgrade, summon)
- A `CameraFollow` script on Main Camera to track the hero

### Core Managers (Singleton Pattern)

All managers use the Unity `MonoBehaviour` singleton pattern with a public static `Instance` field set in `Awake()`.

- **`GameManager`** (`Assets/Scripts/Core/GameManager.cs`) — Central game controller. Manages:
  - **Turn flow**: `PlayerTurn → AITurn → PlayerTurn → ...` with 20-turn limit
  - **Player data**: Holds `player` and `aiPlayer` (`Player` instances) with resources, stronghold level, and deck
  - **Action control**: `hasPlayerActed` enforces one main action per turn (move/upgrade/summon)
  - **Resource production**: End-of-turn automatic gold + building materials generation from both strongholds
  - **Summoning**: `SummonUnit()` creates cards from the player's race at a given level, deducts gold
  - **Win/loss**: `OnHeroEnterEnemyStronghold()` compares deck combat power; `JudgeByScore()` decides winner at turn 20
  - AI turn is simulated with a 1.5-second delay (`Invoke`). AI does not make decisions yet.
- **`MapManager`** (`Assets/Scripts/Map/MapManager.cs`) — Holds a 2D array of `Tile` references. Provides `GetTileAt(Vector2Int)`, `WorldToGrid()`, and `GridToWorld()` for coordinate conversion. Configured with `width`, `height`, and `tileSize` (default: 10×10, 1.2 spacing).
- **`InputManager`** (`Assets/Scripts/Core/InputManager.cs`) — Translates mouse clicks into grid coordinates and delegates to `Hero.TryMove()`. References the `Hero` directly.
- **`UIManager`** (`Assets/Scripts/UI/UIManager.cs`) — Updates HUD text each frame: turn counter, game state, player resources (gold, building materials, stronghold level). Exposes `OnEndTurnButton()` bound to the end-turn button.
- **`CameraFollow`** (`Assets/Scripts/Core/CameraFollow.cs`) — Smoothly follows a target Transform (the Hero) in `LateUpdate` using `Vector3.Lerp`.

### Map System

- **`MapGenerator`** (`Assets/Scripts/Map/MapGenerator.cs`) — Spawns a 10×10 tile grid at startup with fixed counts matching the design spec:
  - 2 strongholds (player at (0,0), enemy at (9,9))
  - 8 resource tiles, 6 army camp tiles, 4 event tiles
  - Remaining 80 tiles are empty
  - Guarantees at least 1 resource + 1 army camp in the 3×3 area around each stronghold
  - Sets hero starting position to (0,0) and passes `StrongholdUI` reference to the player's stronghold tile
- **`Tile`** (abstract base, `Assets/Scripts/Map/Tile.cs`) — Holds `gridPosition` (Vector2Int) and a virtual `OnHeroEnter()` callback. All tile types inherit from this:
  - `EmptyTile` — passable, no effect
  - `ResourceTile` — grants random gold (3-8) or building materials (2-5) to the player
  - `ArmyCampTile` — compares player's total deck combat power against a random enemy (10-50); win grants gold, loss deducts gold
  - `EventTile` — random outcome: 40% bonus resources, 30% lose gold, 30% nothing
  - `StrongholdTile` — player stronghold opens `StrongholdUI` panel; enemy stronghold triggers `GameManager.OnHeroEnterEnemyStronghold()` for win/loss check. Colors itself at Start (player=green, enemy=purple) via SpriteRenderer.
- Tile prefabs live in `Assets/Prefabs/Tile/` and have the corresponding script component attached.

### Unit System

- **`Hero`** (`Assets/Scripts/Units/Hero.cs`) — Player-controlled unit with grid-based movement. Behavior:
  - Only during player turn (`GameManager.Instance.isPlayerTurn`)
  - Manhattan distance ≤ 3 from current position
  - Uses `Vector3.MoveTowards` for smooth interpolation at `moveSpeed`
  - **distance = 0** (clicking current tile): triggers `tile.OnHeroEnter()` **without consuming the action** — enables stronghold interaction without spending the turn
  - **distance > 0**: consumes the player's action via `GameManager.OnPlayerAction()`, then calls `tile.OnHeroEnter()` upon arrival
- **`Unit`** (abstract base, `Assets/Scripts/Units/Unit.cs`) — Non-MonoBehaviour data class. Defines `baseAttack`, `baseHP`, `level`, `race`, and level-multiplier scaling (Lv1=1.0, Lv2=1.5, Lv3=2.5, Lv4=4.0, Lv5=6.5). Also contains the `RaceType` enum (`Human`, `Heaven`, `Ghost`).
- **`HumanUnit` / `HeavenUnit` / `GhostUnit`** — Concrete subclasses, each defining 5 unit types with name + base stats. Provide static `CreateCard(int unitIndex, int level)` factory methods.
- **`Card`** (`Assets/Scripts/Units/Card.cs`) — Serializable data object wrapping a unit instance: `unitIndex`, `level`, `currentHP`, `baseAttack`, `baseHP`, `cardName`, `race`. Computes `GetAttack()`, `GetMaxHP()`, `GetCombatPower()` using the level multiplier.
- **`Deck`** (`Assets/Scripts/Units/Deck.cs`) — Serializable container of `Card` objects. Provides `AddCard()`, `RemoveCard()`, `GetTotalCombatPower()`.
- **`Player`** (`Assets/Scripts/Units/Player.cs`) — Serializable player data model:
  - Fields: `playerName`, `race`, `resources` (ResourceData), `strongholdLevel` (1-5), `deck`, `strongholdPos`
  - Initial state (from design spec): 100 gold, 100 building materials, stronghold Lv1, 1 Lv3 unit card
  - Upgrade cost: `80 × level^1.5` building materials
  - Summon cost: `30 × levelMultiplier` gold, requires `strongholdLevel >= cardLevel`
  - End-of-turn production: `10 gold × levelMult` + `5 buildingMaterials × levelMult`

### Data Layer

- **`ResourceData`** (`Assets/Scripts/Data/ResourceData.cs`) — Serializable struct holding `gold` and `buildingMaterials`. Supports `Add()`, `Subtract()`, `CanAfford()`.

### UI System

- **`StrongholdUI`** (`Assets/Scripts/UI/StrongholdUI.cs`) — Panel shown when hero clicks on own stronghold. Three buttons:
  - **升级据点**: spends building materials to increase stronghold level (max 5), unlocking higher-level summons and better resource production
  - **召唤单位**: spends gold to summon a random unit of the player's race at current stronghold level, adds card to deck
  - **离开**: closes the panel
  - Buttons are only interactable when the player has not yet acted this turn and can afford the cost. All actions call `GameManager.OnPlayerAction()`.
- Uses `TMP_Text` (TextMeshPro) for all text. Requires a Chinese TMP font asset (see below).

### Key Dependencies

- **TextMeshPro** (3.0.7) — UI text rendering. **Requires a Chinese CJK font asset** (e.g., Noto Sans SC via TMP Font Asset Creator or the `wy-luke/Unity-TextMeshPro-Chinese-Characters-Set` GitHub repo) to render Chinese characters.
- **UOS Launcher** (`cn.unity.uos.launcher`) — Chinese Unity Online Services framework, pulled from an external git URL
- **2D Feature Set** (`com.unity.feature.2d`) — Standard Unity 2D packages (tilemap, sprite shape, animation, etc.)
- **Unity Test Framework** (1.1.33) — installed but no tests exist yet

## Development Conventions

### Communication Language

- All Codex-facing communication for this project must be in Chinese.
- This includes clarifying questions, progress updates, implementation plans, proposed plans, review findings, test/verification summaries, and final responses.
- Keep code identifiers, file paths, commands, logs, API names, package names, and quoted source text in their original language when that is clearer or technically required.
- If the user explicitly asks for another language in a later message, follow that request for that interaction only.

### Code Style

- C# scripts follow Unity conventions: `MonoBehaviour` classes, public fields for Inspector-exposed properties, `[SerializeField]` not used (public fields instead)
- Data classes (`Player`, `Card`, `Deck`, `ResourceData`, `Unit`) are plain C# classes marked `[Serializable]` for future save/load support
- Chinese comments and debug log messages throughout
- No namespaces are used; all scripts are in the global namespace
- Scripts are organized by system in `Assets/Scripts/`: `Core/`, `Map/`, `Units/`, `UI/`, `Data/`

### Git Workflow

- **Branch naming**: `feature/<feature-name>` for feature branches
- **Base branch**: `main`
- Current active branch: `feature/Map_logic+Manage_logic` (map + hero movement + stronghold management loop)
- No CI checks or hooks are configured

## Design Documents

The `docs/` directory contains the game design documents (PRD, proposals, workflow diagrams) in Chinese. Key reference files:
- `《史诗起点》游戏设计需求文档（PRD）.docx` — Full PRD
- `史诗起点_简化版策划案（终稿-目前需求）.docx` — Current simplified design spec (follow this for implementation)
- `Game Logic.png`, `Work Flow.png` — Architecture diagrams
