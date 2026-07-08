# Garrison 美术资源集成

Date: 2026-07-08

## Summary

将新增的 Garrison 美术资源集成到 Garrison 驻军场景中：卡槽框替换预制体占位图、箭头按钮替换文字转移按钮、训练背景根据玩家种族动态切换。

## Changes

### 1. 玩家种族存储 (GameSession.cs)

- 新增 `private RaceType garrisonPlayerRace` 字段，存储进入 Garrison 场景时的玩家种族
- `StoreGarrisonState()` 中从 `nextRunState.player.race` 读取并保存到 `garrisonPlayerRace`
- 新增 `public static RaceType GetGarrisonPlayerRace()` 静态方法供 `GarrisonUI` 查询
- 新增 `public static bool HasGarrisonPlayerRace` 属性供空值判断

### 2. 种族训练背景切换 (GarrisonUI.cs)

- 新增字段：
  - `public Image backgroundImage` — 场景背景 Image 组件引用
  - `public Sprite humanTrainingBg` — 人族训练背景
  - `public Sprite heavenTrainingBg` — 天族训练背景
  - `public Sprite ghostTrainingBg` — 鬼族训练背景
- 新增 `GetBackgroundForRace(RaceType race)` 辅助方法，switch 返回对应 Sprite
- `Start()` 中：通过 `GameSession.GetGarrisonPlayerRace()` 获取种族，调用 `GetBackgroundForRace()` 设置 `backgroundImage.sprite`
- `backgroundImage` 为 null 时静默跳过，与项目现有 nullable 检查风格一致

### 3. 卡槽框 & 箭头按钮（Unity Editor 手动操作，不改代码）

- HeroSlotPrefab / GarrisonEntryPrefab：将 CardImage 的白色占位 Source Image 替换为 `garrison_card_slot_9slice.png`
- GarrisonScene 转移按钮：将 `transferToGarrisonBtn` 和 `transferToHeroBtn` 的 Image Source 分别设为左/右箭头按钮图
- 如果按钮原为纯文字按钮，改为 Image 按钮或将箭头图设为按钮子对象的 Image

## Implementation Notes

- 背景图字段参照 `StrongholdUI.cs` 中 `2026-07-01-据点种族背景图.md` 的设计模式：同款 switch 结构、nullable 静默跳过
- 不需要修改 `GarrisonSceneBridge.cs`、`GarrisonCardEntry.cs` 或其他文件
- 卡槽框和箭头按钮的 Unity 操作不需要改代码，仅涉及预制体和场景的 Inspector 赋值
