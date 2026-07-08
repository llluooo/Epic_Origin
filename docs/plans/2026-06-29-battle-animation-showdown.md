# 战斗动画序列 — 抽牌对峙与同时翻开

Date: 2026-06-29

## Summary

在翻面聚合动画基础上扩展：隐藏非选中卡牌 → 我方卡牌移至出牌区 → 翻至背面 → 双方卡牌移至对峙区 → 同时翻到正面 → 结算。

## Changes

### 1. PlayEngageAnimation 扩展 (BattleUI.cs)

**完整动画序列：**
| 阶段 | 操作 | 时长 |
|------|------|------|
| ② | 所有敌方存活卡牌同时 FlipToBack | maxFlipDuration (0.5s) |
| ③ | 所有敌方卡牌 lerp → 聚合中心 | gatherDuration (0.6s) |
| ④ | SetActive(false) 非选中卡牌（敌我双方） | 瞬时 |
| ⑤ | 我方选中卡牌 MoveToPosition(placeAnchor) | arenaMoveDuration (0.35s) |
| ⑥ | 我方选中卡牌 FlipToBack | flipDuration (0.5s) |
| ⑦ | 敌方 → arenaLeftAnchor，我方 → arenaRightAnchor | arenaMoveDuration |
| ⑧ | 双方同时 FlipToFront | flipDuration |
| ⑨ | battleManager.ExecuteRound() + RefreshUI | 瞬时 |

**新增字段：**
- `public Transform playerPlaceAnchor` — 出牌区锚点（屏幕底侧中央）
- `public Transform arenaLeftAnchor` — 敌方对峙区锚点（屏幕中央偏左）
- `public Transform arenaRightAnchor` — 我方对峙区锚点（屏幕中央偏右）
- `public float arenaMoveDuration = 0.35f` — 移动到锚点的时长

### 2. CardView 新增方法 (CardView.cs)

- `HideCard()` — 隐藏卡牌 GameObject 并重置翻面状态

## Implementation Notes

- `attackerIndex` / `defenderIndex` 在动画前预选，动画期间数据不变
- `playerSlotViews[attackerIndex]` → 我方出战卡牌 View
- `enemySlotViews[defenderIndex]` → 敌方防守卡牌 View
- 锚点为 null 时跳过对应移动步骤（降级处理）
- 单张敌方卡牌时跳过聚合（已有 `aliveEnemyViews.Count > 1` 判断）
- 所有锚点由用户在 BattleScene 中手动放置空 GameObject 并拖入 Inspector
- 默认值已同步用户调整：flipDuration=0.5s, gatherDuration=0.6s
