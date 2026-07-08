# Garrison 场景卡牌图片显示

Date: 2026-07-08

## 目标

Garrison 场景的卡槽/驻军列表当前只显示文字（名称、种族、属性），缺少卡牌美术图。利用已有的 `Assets/AI_Generated/Units/Cards/` 下的 15 张卡牌图，让每个卡槽显示对应兵种的卡牌插图。

## 方案

参照项目中 `MapVisualConfig` 的 ScriptableObject 模式，创建 `CardSpriteConfig` 存储每种种族+unitIndex 对应的 Sprite 引用。`GarrisonCardEntry` 新增 `cardImage` (Image) 字段来显示图片。

### 数据流

```
Card(race, unitIndex) → CardSpriteConfig.GetSprite(race, unitIndex) → GarrisonCardEntry.cardImage.sprite
```

### 1. 新文件：CardSpriteConfig.cs

`Assets/Scripts/Scenes/Garrison/CardSpriteConfig.cs`

- `[CreateAssetMenu]` ScriptableObject，菜单路径 `"Epic Origin/Card Sprite Config"`
- 按种族 × unitIndex(0-4) 组织 Sprite 数组：
  - `humanSprites[5]`
  - `heavenSprites[5]`
  - `ghostSprites[5]`
- `GetSprite(RaceType race, int unitIndex)` 查询方法

### 2. 修改：GarrisonCardEntry.cs

新增字段和自动绑定：
- `public Image cardImage` — 卡牌插图 Image 组件
- `Awake()` 中按子节点名 `"CardImage"` 自动绑定
- `Setup(Card, int, bool, Sprite)` — 新增第4个参数 `cardSprite`，非空时设置 `cardImage.sprite` 并启用 GameObject

### 3. 修改：GarrisonUI.cs

- 新增 `public CardSpriteConfig cardSpriteConfig` — Inspector 引用
- `SetupEntry(entry, card, index, isHeroSlot)` — 向 config 查询 sprite，调用 entry.Setup()

### 4. 兵种索引→文件名映射（供 Inspector 赋值参考）

| unitIndex | Human (人族) | Heaven (天族) | Ghost (鬼族) |
|-----------|-------------|--------------|-------------|
| 0 (Lv1) | human_swordsman_card | heaven_soldier_card | ghost_skeleton_card |
| 1 (Lv2) | human_heavy_infantry_card | heaven_sky_mage_card | ghost_zombie_card |
| 2 (Lv3) | human_wizard_card | heaven_unicorn_card | ghost_will_o_wisp_card |
| 3 (Lv4) | human_knight_card | heaven_giant_card | ghost_death_knight_card |
| 4 (Lv5) | human_royal_guard_card | heaven_archangel_card | ghost_reaper_card |

## Unity Editor 操作要点

1. 创建 `CardSpriteConfig.asset`，绑定 15 张卡牌 Sprite
2. 更新 3 个 prefab（HeroSlotPrefab / GarrisonEntryPrefab / CardEntryPrefab），添加名为 `"CardImage"` 的子 Image
3. 在 GarrisonController 的 Inspector 中把 CardSpriteConfig 拖入 `cardSpriteConfig` 字段
