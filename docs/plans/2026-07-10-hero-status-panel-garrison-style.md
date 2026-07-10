# HeroStatusPanel 改造 —— 驻军风格展示

## 目标

将 HeroStatusPanel 从"固定 5 兵种槽 + 数量"的僵化展示，改为 garrison 风格的动态卡牌列表。

## 改动范围

### 修改文件

| 文件 | 改动 |
|------|------|
| `Assets/Scripts/Scenes/Main/UI/HeroStatusUI.cs` | 核心改造：移除固定槽逻辑，改为动态列表 |

### 复用（不修改）

| 文件 | 用途 |
|------|------|
| `Assets/Scripts/Scenes/Garrison/GarrisonCardEntry.cs` | 卡牌条目组件，setup 后只读展示（不绑定点击） |
| `Assets/Scripts/Scenes/Garrison/CardSpriteConfig.cs` | ScriptableObject，提供精灵图查询 |

## 设计方案

### HeroStatusUI 改造

**移除：**
- `cardImages[]`、`cardNameTexts[]`、`cardQuantityTexts[]`（固定 5 槽 UI 引用）
- `humanCardSprites[]`、`heavenCardSprites[]`、`ghostCardSprites[]`（Inspector 精灵数组）
- `GetUnitNameForRace()`（硬编码名称）
- `GetCardNameFromDeck()`（不再需要单独查名称）
- `GetCardSpritesForRace()`（改用 CardSpriteConfig）

**新增：**
- `cardSpriteConfig` — `CardSpriteConfig` 引用
- `entryPrefab` — `GameObject`，卡牌条目预制体（挂 `GarrisonCardEntry`）
- `cardsContainer` — `Transform`，列表容器
- `cardCountText` — `TMP_Text`，显示"英雄兵力 (N/6)"

**保留不变：**
- `panel`、`goldText`、`materialsText`、`strongholdLevelText`、`deckPowerText`
- `inputManager` 引用
- `IsOpen`、`openedFromGameMenu`
- `Open()`、`Close()` 的整体流程
- `RefreshResourceDisplay()` 方法

### RefreshCardDisplay 新逻辑

```
清除 cardsContainer 下所有子对象
for deck 中每张卡牌:
    Instantiate(entryPrefab, cardsContainer)
    获取 GarrisonCardEntry 组件
    通过 cardSpriteConfig.GetSprite(card.race, card.unitIndex) 获取精灵图
    entry.Setup(card, index, isHeroSlot: false, sprite)
    entry.button.interactable = false  // 只读，不可点击
更新 cardCountText = "英雄兵力 (N/6)"
```

### 数据流

```
GameManager.Instance.player.deck
  → HeroStatusUI.RefreshCardDisplay()
    → 遍历 deck 卡牌
      → CardSpriteConfig.GetSprite(race, unitIndex)
      → GarrisonCardEntry.Setup(card, index, false, sprite)
      → 禁用按钮交互
```

## 边界情况

- **空 deck**：不生成任何条目，`cardCountText` 显示 "英雄兵力 (0/6)"
- **满 deck（6张）**：正常显示 6 条
- **同名卡牌堆叠**：deck 中同一 unitIndex 的卡会合并 quantity，GarrisonCardEntry 已支持显示 ×N
- **面板开关**：每次 Open() 时重建列表，Close() 时清理

## Unity 操作步骤

1. 选中 MainScene 中的 **HeroStatusPanel** GameObject
2. 在 HeroStatusUI 组件的 Inspector 中：
   - 移除旧的 cardImages/cardNameTexts/cardQuantityTexts 赋值
   - 移除旧的人类/天族/鬼族精灵数组赋值
   - 将 **Card Sprite Config** 拖入新增的 `cardSpriteConfig` 字段
   - 将 **Entry Prefab**（GarrisonEntryPrefab）拖入 `entryPrefab` 字段
   - 将列表容器 Transform 拖入 `cardsContainer` 字段
   - 将兵力数量 Text 拖入 `cardCountText` 字段
3. 如果 HeroStatusPanel 下还没有列表容器，创建一个空 GameObject 作为子节点，命名为 "CardsContainer"，添加 VerticalLayoutGroup 组件用于自动排列
