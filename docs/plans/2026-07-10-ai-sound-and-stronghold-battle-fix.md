# AI 音效抑制 & 据点防守卡组修复

## 问题 1：敌方英雄触发音效

### 根因

`ResourceTile.OnHeroEnter()` 不管进入者是谁都播放 `SFX.GetGold`/`SFX.GetWood`。`GetEnteringPlayer()` 方法已能区分 AI 和玩家，但音效播放没有据此判断。

AI 的其他交互路径均不触发音效（ArmyCampTile 走 AIBattleSimulator、EventTile 内联逻辑、StrongholdTile 走 GameManager）。

### 修复

`ResourceTile.OnHeroEnter()` — 在播放音效前判断是否为 AI 进入，AI 进入时跳过音效。

## 问题 2：AI 攻击玩家据点使用错误卡组

### 根因

`GameManager.OnAIHeroEnterPlayerStronghold()` 第 819 行：
```csharp
StartBattle(BattleEncounterType.PlayerStronghold, player.deck.cards, player.strongholdPos);
```
第二个参数 `enemyDeck` 传了 `player.deck.cards`，导致双方都用玩家的卡组战斗。应该传 `aiPlayer.deck.cards`，让 AI 用自己的卡组攻击。

### 修复

将 `player.deck.cards` 改为 `aiPlayer.deck.cards`。

## 改动范围

| 文件 | 改动 |
|------|------|
| `Assets/Scripts/Scenes/Main/Map/ResourceTile.cs` | 播放音效前判断是否 AI 进入 |
| `Assets/Scripts/Scenes/Main/Core/GameManager.cs` | 修正 `OnAIHeroEnterPlayerStronghold` 的 enemyDeck 参数 |
