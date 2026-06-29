# 战斗动画序列 — 重置归位动画

Date: 2026-06-29

## Summary

回合结算后增加完整的重置动画序列：对峙停留 → 归位 → 存活/死亡分支 → 其余卡牌淡入 → 敌方翻回正面 → 新一轮。

## Changes

### 1. CardView 淡入淡出 (CardView.cs)

新增字段：
- `private CanvasGroup canvasGroup` — Alpha 控制组件

修改 `Awake()`：
- 自动获取或添加 CanvasGroup 组件

新增方法：
- `FadeOut(float duration)` — 协程 alpha 1→0
- `FadeIn(float duration)` — 协程 alpha 0→1
- `SetAlpha(float alpha)` — 瞬间设置 alpha

### 2. BattleUI 重置动画 (BattleUI.cs)

新增字段：
- `public float resetStayDuration = 1f` — 对峙停留
- `public float resetFadeDuration = 0.5f` — 淡入淡出
- `public float resetMoveDuration = 0.35f` — 归位移动

修改 `PlayEngageAnimation`：
- 回合结算后改为调用 `StartCoroutine(ResetSequence(...))`

新增 `ResetSequence` 协程：
- ⑩ 对峙停留 1s
- ⑪ 双方滑回区域中央
- ⑫ 存活→归位 / 死亡→淡出
- ⑬ 其余卡牌淡入
- ⑭ 敌方翻回正面
- ⑮ 重置状态 + RefreshUI

## Implementation Notes

- CanvasGroup 在 Awake 中自动添加，不做 Inspector 操作要求
- 存活判断使用 `battleManager.playerCards[attackerIndex].IsAlive()`
- 敌方聚合中心位置通过 CalculateEnemyCenter 重新计算（或使用阶段③的缓存值）
- 原始槽位位置来自 `CardView.originalLocalPosition`
