# 卡牌翻面功能

Date: 2026-06-29

## Summary

为战斗场景中的 CardView 组件新增翻面能力，通过 X 轴缩放动画模拟 Y 轴 3D 旋转效果，所有卡牌具备正面（兵种贴图）和背面（统一卡背贴图）两种显示状态。

## Changes

### 1. CardView 翻面核心 (CardView.cs)

新增字段：
- `public Sprite cardBackSprite = null` — 卡背贴图，由 BattleUI 在初始化时统一赋值
- `private Sprite currentFrontSprite = null` — 记录当前正面精灵，翻面过程中保留引用
- `private bool isShowingFront = true` — 当前是否显示正面
- `private bool isFlipping = false` — 翻面动画进行中时阻塞重复调用
- `public float flipDuration = 0.3f` — 翻面动画总时长（秒）

新增方法：
- `FlipToFront()` — 翻到正面（背面→正面），协程动画
- `FlipToBack()` — 翻到背面（正面→背面），协程动画
- `SetFaceInstant(bool showFront)` — 无动画直接切换正反面
- `IsShowingFront()` — 查询当前朝向

修改逻辑：
- `SetCard(Card)` / `SetCard(BattleCard)` — 设置正面精灵时同时存入 `currentFrontSprite`；如果当前是正面则更新 `cardImage.sprite`，否则仅存储
- `ClearCard()` — 重置 `isShowingFront` 和 `currentFrontSprite`

动画协程 `FlipRoutine(bool toFront)`：
1. 检查 `isFlipping`，正在翻转中则忽略
2. 检查目标面是否与当前面相同，相同则跳过
3. 前半段：`scale.x` 从 `1` 缓动到 `0`（flipDuration/2 秒）
4. 中点：交换 `cardImage.sprite`（正面精灵 ↔ 卡背精灵）
5. 后半段：`scale.x` 从 `0` 缓动到 `1`（flipDuration/2 秒）
6. 更新 `isShowingFront`，清除 `isFlipping` 标志

现有 `SetSelected()` 浮动动画和 `ClearCard()` 逻辑不变。

### 2. BattleUI 集成 (BattleUI.cs)

新增方法：
- `FlipEnemyCardsToBack()` — 遍历 `enemySlotViews`，对每个存活的 CardView 调用 `FlipToBack()`
- `FlipEnemyCardsToFront()` — 遍历 `enemySlotViews`，对每个存活的 CardView 调用 `FlipToFront()`
- `GetEnemySlotView(int index)` — 返回敌方卡牌视图引用
- `GetPlayerSlotView(int index)` — 返回玩家卡牌视图引用

修改逻辑：
- `Start()` / `BindControls()` — 初始化时通过 `Resources.Load<Sprite>("Art/Battle/battle_card_back")` 加载卡背贴图，遍历 `playerSlotViews` 和 `enemySlotViews` 赋值 `cardBackSprite`
- `RefreshCardViews()` — 不改变翻面朝向，仅更新精灵和可见性

## Implementation Notes

- 卡背贴图需放置于 `Assets/Resources/Art/Battle/battle_card_back.png`，确保 Resources.Load 可加载
- 翻面动画中 `isFlipping` 标记会阻塞重复调用，避免快速连点导致视觉错乱
- `SetCard()` 在卡牌为背面时只存储 `currentFrontSprite` 不立即设置 `cardImage.sprite`，翻回正面时自动恢复正确精灵
- `ClearCard()` 会重置翻面状态，确保卡牌在重新分配时从正面开始
- 新增的 BattleUI 公开方法为后续"集中成堆→抽牌→同时翻开"动画序列提供接口
- 动画使用 `EaseInOut` 缓动曲线，模拟物理翻牌的加速-减速感
