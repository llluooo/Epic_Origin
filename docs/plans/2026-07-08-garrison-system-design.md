# 据点兵力管理系统（Garrison System）

Date: 2026-07-08

## Summary

Hero 携带卡牌上限从无限制改为 6 个兵种卡槽，超出卡牌存入据点驻军。新建 GarrisonScene 独立场景管理兵力分配，支持英雄与据点间转移兵种。战斗奖励超出上限时自动归入据点。

## Changes

### 1. Deck 卡槽限制 (Assets/Scripts/Shared/Units/Deck.cs)

- `public bool hasSlotLimit = false` — 是否启用卡槽上限
- `public const int HeroSlotLimit = 6` — 英雄卡槽上限
- `CanAddCard(Card)` → 检查同 unitIndex + 同 race 是否可堆叠，或还有空槽
- `AddCard(Card)` → 逻辑改为同种堆叠 quantity（已存在时）；否则占用新槽（有上限时检查）
- `GetSlotKey(Card)` → 返回 `"race_unitIndex"` 作为槽标识，同种兵种共享一槽

### 2. Player 据点驻军 & 转移方法 (Assets/Scripts/Shared/Units/Player.cs)

- `public Deck garrisonDeck = new Deck()` — 据点驻军卡组，无槽上限，hasSlotLimit = false
- `TryAddToHeroDeck(Card)` → 尝试加入 hero deck，成功返回 true，满槽则 false
- `AddToGarrison(Card)` → 直接加入 garrisonDeck
- `TransferToGarrison(int slotIndex)` → 将 hero deck 中指定槽的卡牌(按列表索引)移入 garrison
- `TransferToHero(int garrisonCardIndex)` → 从 garrison 移入 hero deck，先检查 CanAddCard

### 3. GameManager 初始化适配 (Assets/Scripts/Scenes/Main/Core/GameManager.cs)

- `StartGame()` → 创建 player 时 `player.deck.hasSlotLimit = true`，确保 garrisonDeck 初始化
- `DeckInit()` → 初始 3 张卡直接进 hero deck（还有 3 空槽）
- `ResolveArmyCampBattle()` 战胜奖励 → `TryAddToHeroDeck()`，false 则 `AddToGarrison()` + log
- `RestoreGameFromSession()` → 恢复 garrisonDeck
- `SummonUnits()` → 召唤后检查 `deck` 是否超过 6 槽，超出则移入 garrison

### 4. StrongholdUI 兵力检查按钮 (Assets/Scripts/Scenes/Main/UI/StrongholdUI.cs)

- 新增 `public Button garrisonButton` — "兵力检查"按钮
- `OnGarrisonCheck()` → 保存 run state → `GarrisonSceneBridge.LoadGarrisonScene(runState)`

### 5. GarrisonSceneBridge 场景桥接 (新文件: Assets/Scripts/Shared/Session/GarrisonSceneBridge.cs)

- `public const string GarrisonSceneName = "GarrisonScene"` — 场景名常量
- `LoadGarrisonScene(GameRunState)` → 存储状态 → 切换场景
- `ReturnToMainScene(Deck heroDeck, Deck garrisonDeck)` → 存回 GameSession → 切回 MainScene
- 参考 `BattleSceneBridge` 模式

### 6. GarrisonScene UI 控制器 (新文件: Assets/Scripts/Scenes/Garrison/GarrisonUI.cs)

- **HeroDeck 展示区**：6 槽网格，每槽显示卡牌图标/名称/种族/等级/数量，可点击选中
- **Garrison 驻军列表**：可滚动列表，显示所有驻军卡牌
- **转移按钮**：
  - "→ 移入驻军"：选中 hero slot → transfer
  - "← 带回英雄"：选中 garrison card → transfer（hero 满槽时置灰）
- **返回按钮**：`GarrisonSceneBridge.ReturnToMainScene(heroDeck, garrisonDeck)`
- 进入时从 `GameSession` 读取 heroDeck 和 garrisonDeck 初始化 UI
- 纯 UI 操作，不涉及新资源创建，通过 Card 数据展示

### 7. GameRunState 序列化适配 (Assets/Scripts/Shared/Session/GameRunState.cs)

- `ClonePlayer()` → 增加 `garrisonDeck = CloneDeck(source.garrisonDeck)`
- `Capture()` → garrisonDeck 随 Player 数据自动序列化（Deck 已 `[Serializable]`）

### 8. GameSession 适配 (Assets/Scripts/Shared/Session/GameSession.cs)

- 新增字段/属性：存储 garrison scene 返回时的 heroDeck + garrisonDeck
- 新增方法：`HasGarrisonReturnState` / `StoreGarrisonReturn` / `ClearGarrisonReturn`
- 参考现有 `PendingBattle` / `PendingBattleResult` 机制

## Implementation Notes

- **同种堆叠逻辑**：同 unitIndex + 同 race 的卡牌在同一槽内通过 quantity 堆叠，不占新槽
- **BattleSceneBridge 不变**：战斗切回 MainScene → `RestoreGameFromSession` 恢复 garrison
- **存档系统**：`SaveSystem` 序列化 Player 时自动包含 garrisonDeck（Deck 已 `[Serializable]`）
- **AI Player**：AI 的 deck 是否也设限暂不管，AI 只需 garrison 数据完整即可
- **回 MainScene 恢复**：MainScene 的 `GameManager.RestoreGameFromSession()` 需要检查 garrison 返回状态并恢复
