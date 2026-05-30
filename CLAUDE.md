# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Epic Origin** (史诗起点) — a turn-based 2D strategy/roguelike game built with Unity 2022.3.62f3c1. The project is in early development.

## Build & Run

- Open `Epic_Origin.sln` in JetBrains Rider or the project root in Unity Editor
- Open scene `Assets/Scenes/MainMenuScene.unity` and press Play — this is the entry point; or start from `Assets/Scenes/MainScene.unity` to skip menu flow
- No CI/CD or automated build pipeline is configured

## Architecture

### Scene Flow & Lifecycle

Three scenes form the game flow:

1. **`MainMenuScene.unity`** — Entry point. Simple menu with New Game / Load Game / Settings / Exit buttons. "New Game" navigates to RaceSelectScene.
2. **`RaceSelectScene.unity`** — Player picks a race (Human / Heaven / Ghost). Selection is written to the static `GameSetupData` bridge class, then loads MainScene.
3. **`MainScene.unity`** — The main game scene. Sets up:
   - A `MapGenerator` GameObject that creates the map, initializes hero position, and applies race-based hero appearance
   - A `MapManager` singleton that provides grid query APIs
   - A `GameManager` singleton controlling turn flow, player data, resource production, and win/loss
   - A `Hero` GameObject for the player unit
   - A `UIManager` for HUD (turn, state, resources)
   - A `StrongholdUI` panel for stronghold management (upgrade, summon)
   - A `CameraFollow` script on Main Camera to track the hero

**`GameSetupData`** (`Assets/Scripts/Core/GameSetupData.cs`) — Static bridge class that carries race selection across scenes. Fields: `PlayerRace`, `EnemyRace`, `IsNewGame`. `GameManager.StartGame()` reads from it to initialize both players with the correct races.

### Core Managers (Singleton Pattern)

All managers use the Unity `MonoBehaviour` singleton pattern with a public static `Instance` field set in `Awake()`.

- **`GameManager`** (`Assets/Scripts/Core/GameManager.cs`) — Central game controller. Manages:
  - **Turn flow**: `PlayerTurn → AITurn → PlayerTurn → ...` with 20-turn limit
  - **Player data**: Holds `player` and `aiPlayer` (`Player` instances) with resources, stronghold level, and deck. Races are determined by `GameSetupData` (defaults: Human for player, Ghost for AI).
  - **Action control**: `hasPlayerActed` enforces one main action per turn (move/upgrade/summon)
  - **Resource production**: End-of-turn automatic gold + building materials generation from both strongholds
  - **Summoning**: `SummonUnit()` creates cards from the player's race at a given level, deducts gold. `CreateInitialCard()` gives each player a Lv3 unit of their race at game start.
  - **Win/loss**: `OnHeroEnterEnemyStronghold()` compares deck combat power; `JudgeByScore()` decides winner at turn 20
  - AI turn is simulated with a 1.5-second delay (`Invoke`). AI does not make decisions yet.
- **`MapManager`** (`Assets/Scripts/Map/MapManager.cs`) — Holds a 2D array of `Tile` references. Provides `GetTileAt(Vector2Int)`, `WorldToGrid()`, and `GridToWorld()` for coordinate conversion. Configured with `width`, `height`, `tileSize`, and `worldOrigin` offset (set by MapGenerator at runtime).
- **`InputManager`** (`Assets/Scripts/Core/InputManager.cs`) — Translates mouse clicks into grid coordinates and delegates to `Hero.TryMove()`. References the `Hero` directly.
- **`UIManager`** (`Assets/Scripts/UI/UIManager.cs`) — Updates turn/state text each frame via `UpdateUI()`. Exposes `OnEndTurnButton()` bound to the end-turn button.

### Map System

- **`MapGenerator`** (`Assets/Scripts/Map/MapGenerator.cs`) — Spawns a 10×10 tile grid at startup with fixed counts matching the design spec:
  - 2 strongholds (player at (0,0), enemy at (9,9))
  - 8 resource tiles, 6 army camp tiles, 4 event tiles
  - Remaining 80 tiles are empty
  - Guarantees at least 1 resource + 1 army camp in the 3×3 area around each stronghold
  - Minimum spacing between special tiles (configurable via `minSpecialTileDistance`)
  - **Visual system**: Uses `MapVisualConfig` to assign per-race ground sprites, POI sprites, and stronghold sprites. Generates Voronoi-based neutral ground patches with noise for visual variety. Race ground areas surround each stronghold (radius 2).
  - Sets hero starting position to (0,0), calls `hero.SetRaceAppearance()` with the player's race from `GameSetupData`, and passes `StrongholdUI` reference to the player's stronghold tile
  - Supports `centerMapOnGenerator` option for aligned or centered map output
- **`Tile`** (abstract base, `Assets/Scripts/Map/Tile.cs`) — Holds `gridPosition` (Vector2Int) and a virtual `OnHeroEnter()` callback. All tile types inherit from this:
  - `EmptyTile` — passable, no effect
  - `ResourceTile` — placeholder for resource collection
  - `ArmyCampTile` — placeholder for battle encounters
  - `EventTile` — placeholder for random events
  - `StrongholdTile` — stronghold interactions. Has `strongholdType` (Player/Enemy) and `visualRace` for sprite selection. Player stronghold opens `StrongholdUI`; enemy stronghold triggers `GameManager.OnHeroEnterEnemyStronghold()`.
- Tile prefabs live in `Assets/Prefabs/Tile/` and each has a `TileVisual` component + tile script attached.

### Map Visual System

- **`MapVisualConfig`** (`Assets/Scripts/Map/MapVisualConfig.cs`) — `ScriptableObject` asset (`Assets/Map Visual Config.asset`) defining all tile graphics:
  - Stronghold sprites per race (Human/Heaven/Ghost)
  - Race ground sprites per race
  - Neutral ground sprite array (randomly assigned via patch generation)
  - POI sprites: resource, army camp, event
  - Overlay sprites: selected, reachable, target
  - Provides `GetStrongholdSprite(RaceType)`, `GetRaceGroundSprite(RaceType)`, `GetNeutralGroundSprite()` lookup methods
- **`TileVisual`** (`Assets/Scripts/Map/TileVisual.cs`) — Per-tile component managing three `SpriteRenderer` layers:
  - `groundRenderer` — background ground texture (sorting order 0)
  - `poiRenderer` — point-of-interest icon (sorting order 10)
  - `overlayRenderer` — selection/reachable/target highlight (sorting order 20)
  - Auto-binds renderers from child GameObjects named "Ground", "POI", "Overlay", with fallback to root `SpriteRenderer`
  - `FitRendererToWorldSize()` scales each sprite to a target world size based on configurable scale parameters in MapGenerator

### Unit System

- **`Hero`** (`Assets/Scripts/Units/Hero.cs`) — Player-controlled unit with grid-based movement and race-based appearance. Behavior:
  - Only during player turn (`GameManager.Instance.isPlayerTurn`)
  - Only once per turn (`GameManager.Instance.hasPlayerActed`)
  - Manhattan distance ≤ 3 from current position
  - Uses `Vector3.MoveTowards` for smooth interpolation at `moveSpeed`
  - **distance = 0** (clicking current tile): triggers `tile.OnHeroEnter()` **without consuming the action** — enables stronghold interaction without spending the turn
  - **distance > 0**: consumes the player's action via `GameManager.OnPlayerAction()`, then calls `tile.OnHeroEnter()` upon arrival
  - **`SetRaceAppearance(RaceType)`**: tints the `SpriteRenderer.color` based on race (Human=blue, Heaven=gold, Ghost=purple)
  - MapGenerator calls `SetRaceAppearance()` with the race from `GameSetupData.PlayerRace` during map generation
- **`Unit`** (abstract base, `Assets/Scripts/Units/Unit.cs`) — Non-MonoBehaviour data class. Defines `baseAttack`, `baseHP`, `level`, `race`, and level-multiplier scaling (Lv1=1.0, Lv2=1.5, Lv3=2.5, Lv4=4.0, Lv5=6.5). Also contains the `RaceType` enum (`Human`, `Heaven`, `Ghost`).
- **`HumanUnit` / `HeavenUnit` / `GhostUnit`** — Concrete subclasses, each defining 5 unit types with name + base stats. Provide static `CreateCard(int unitIndex, int level)` factory methods.
- **`Card`** (`Assets/Scripts/Units/Card.cs`) — Serializable data object wrapping a unit instance: `unitIndex`, `level`, `currentHP`, `baseAttack`, `baseHP`, `cardName`, `race`. Computes `GetAttack()`, `GetMaxHP()`, `GetCombatPower()` using the level multiplier.
- **`Deck`** (`Assets/Scripts/Units/Deck.cs`) — Serializable container of `Card` objects. Provides `AddCard()`, `RemoveCard()`, `GetTotalCombatPower()`.
- **`Player`** (`Assets/Scripts/Units/Player.cs`) — Serializable player data model:
  - Fields: `playerName`, `race`, `resources` (ResourceData), `strongholdLevel` (1-5), `deck`, `strongholdPos`
  - Initial state (from design spec): 100 gold, 100 building materials, stronghold Lv1, 1 Lv3 unit card
  - Upgrade cost: `80 × level^1.5` building materials
  - Summon cost: `30 × levelMultiplier` gold, requires `strongholdLevel >= cardLevel`
  - End-of-turn production: `10 gold × levelMult` + `5 buildingMaterials × levelMult` (multiplier scales with stronghold level: 1.0/1.3/1.6/2.0/2.5)

### Data Layer

- **`ResourceData`** (`Assets/Scripts/Data/ResourceData.cs`) — Serializable struct holding `gold` and `buildingMaterials`. Supports `Add()`, `Subtract()`, `CanAfford()`.

### UI System

- **`MainMenuUI`** (`Assets/Scripts/UI/MainMenuUI.cs`) — Main menu screen on `MainMenuScene`. Four buttons: New Game → loads `RaceSelectScene`, Load Game (placeholder), Settings (placeholder), Exit → `Application.Quit()`.
- **`RaceSelectUI`** (`Assets/Scripts/UI/RaceSelectUI.cs`) — Race selection screen on `RaceSelectScene`. Displays race options with highlight toggling. On confirm: writes player's chosen race to `GameSetupData.PlayerRace`, randomly picks one of the remaining races for `GameSetupData.EnemyRace`, sets `IsNewGame = true`, then loads `MainScene`.
- **`StrongholdUI`** (`Assets/Scripts/UI/StrongholdUI.cs`) — Panel shown when hero clicks on own stronghold. Three buttons:
  - **升级据点**: spends building materials to increase stronghold level (max 5), unlocking higher-level summons and better resource production
  - **召唤单位**: spends gold to summon a random unit of the player's race at current stronghold level, adds card to deck
  - **离开**: closes the panel
  - Buttons are only interactable when the player has not yet acted this turn and can afford the cost. All actions call `GameManager.OnPlayerAction()`.
- Uses `TMP_Text` (TextMeshPro) for all text. Requires a Chinese TMP font asset (see below).

### AI-Generated Art Assets

`Assets/AI_Generated/` contains AI-generated 2D sprites organized by purpose:

- **`Units/Heroes/`** — Single-frame hero portraits per race (`hero_human.png`, `hero_heaven.png`, `hero_ghost.png`)
- **`Units/Cards/`** — Card illustrations for all 15 units (5 per race × 3 races). Each file is named `{race}_{unit}_card.png`.
- **`Units/MapSprites/`** — 8-directional 4-frame walk cycle spritesheets per race (`hero_{race}_walk_8dir_4f.png`) for on-map hero animation.
- **`Units/Cards/_raw/`** — Raw AI outputs before card-frame compositing. Source material for future card frame redesigns.
- **`Map/Tiles/`** — Map tile sprites organized by layer:
  - **`Ground/`** — 5 ground textures: grass, dirt, stone, holy, corrupted, plus a parchment backdrop
  - **`POI/`** — POI icons: resource, army_camp, event_ruin, plus per-race strongholds (human/heaven/ghost)
  - **`Overlay/`** — Selection/reachable/target highlight sprites
- **`_chroma_sources/`** — Chroma-keyable character renders used as source for sprite generation. Contains hero portraits, walk spritesheets, and tile sprite sources with solid-color backgrounds for easy background removal.

Sprite import settings: TextureType=Sprite, PixelsPerUnit=100, FilterMode=Point (for pixel art style).

Note: The current hero system uses `SpriteRenderer.color` tinting for race differentiation (`SetRaceAppearance()`). The walk spritesheets and card images exist as assets but are not yet wired into the game logic — they represent the next integration step. Map tiles are now integrated via `MapVisualConfig` and `TileVisual`.

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
- Current active branch: `feature/MainScene_UI+Art` (map visual system + tile art integration)
- No CI checks or hooks are configured

## Design Documents

- `plans/` directory contains dated design documents for implemented features (e.g., `2026-05-11-race-selection-design.md`)
- `docs/` directory contains original game design documents in Chinese:
  - `《史诗起点》游戏设计需求文档（PRD）.docx` — Full PRD
  - `史诗起点_简化版策划案（终稿-目前需求）.docx` — Current simplified design spec (follow this for implementation)
  - `Game Logic.png`, `Work Flow.png` — Architecture diagrams
