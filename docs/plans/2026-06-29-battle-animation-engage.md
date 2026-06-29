# 战斗动画序列 — 敌方翻面聚合

Date: 2026-06-29

## Summary

将卡牌翻面能力集成到战斗流程中：玩家确认出战后，敌方卡牌同时翻到背面，然后全部滑动聚合到敌方区域中央。

## Changes

### 1. BattleUI 动画流程改造 (BattleUI.cs)

**流程变化：**
- 当前：`OnAttackConfirm()` → `ExecuteSingleRound()` → `RefreshUI()`
- 改造后：`OnAttackConfirm()` → 禁用按钮 → `StartCoroutine(PlayEngageAnimation())` → 动画完成 → `ExecuteSingleRound()` → `RefreshUI()`

**新增字段：**
- `public float cardFlipStaggerDelay = 0f` — 依次翻面的延迟（当前为 0，同时翻转）
- `public float gatherDuration = 0.4f` — 聚合移动时长
- `private bool isAnimating` — 动画进行中，阻塞重复操作

**新增/修改方法：**
- `OnAttackConfirm()` — 改为启动动画协程，攻击按钮设为不可交互
- `PlayEngageAnimation()` (协程) — 主持动画序列：
  1. 调用 `TryBuildRoundSelection()` 预选攻守双方，暂存 `attackerIndex` / `defenderIndex`
  2. 阶段①：遍历敌方存活卡牌，同时调用 `FlipToBack()`
  3. 等待所有翻面完成（最长的 flipDuration）
  4. 阶段②：计算敌方区域中心，所有卡牌 `GatherToPosition()` 滑动聚合
  5. 动画完成，执行 `battleManager.ExecuteRound(attackerIndex, defenderIndex)`
  6. 调用 `RefreshUI()`，清除 `isAnimating`
- `UpdateControlState()` — 增加 `isAnimating` 判断，动画期间攻击按钮不可交互

### 2. CardView 移动动画 (CardView.cs)

**新增方法：**
- `MoveToPosition(Vector3 targetWorldPos, float duration)` — 将卡牌从当前位置平滑移动到目标世界坐标

## Implementation Notes

- 聚合中心 = 所有存活敌方卡牌位置的几何平均值
- 翻面用 `FlipToBack()`，内部 `isFlipping` 互斥锁保证安全
- 动画期间 `isAnimating = true`，`UpdateControlState()` 检查此标志阻止重复点击
- 攻击按钮在动画开始时直接 `SetActive(false)`（用户要求消失）或 `interactable = false`
- `TryBuildRoundSelection()` 在动画前调用一次，暂存攻守索引，避免动画期间数据变化
- 翻面时长由 `CardView.flipDuration`（默认 0.3s）控制；聚合由 `gatherDuration`（默认 0.4s）控制
- 移动使用 `Vector3.Lerp` + `Mathf.SmoothStep` 缓动
