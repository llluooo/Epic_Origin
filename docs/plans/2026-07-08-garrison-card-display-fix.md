# Garrison 场景卡牌不显示修复

Date: 2026-07-08

## 根因

`GarrisonCardEntry.Setup(Card card, ...)` 未对 `card` 参数做空检查，空槽位调用 `Setup(null, ...)` 时访问 `card.cardName` 抛出 NullReferenceException，`RebuildHeroSlots()` 方法中断，导致所有卡牌入口都无法渲染。

## 修复内容

### 1. GarrisonCardEntry.Setup() 空检查 (Assets/Scripts/Scenes/Garrison/GarrisonCardEntry.cs)

- Setup 方法开头增加 `card == null` 判断
- 空卡牌时显示空状态文字："空槽位"

### 2. Player.deck 默认初始化 (Assets/Scripts/Shared/Units/Player.cs)

- `public Deck deck` → `public Deck deck = new Deck()`，与 garrisonDeck 保持一致
