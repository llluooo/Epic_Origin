# Garrison 返回时自动重开据点 UI

Date: 2026-07-09

## 问题

从据点 UI 进入 GarrisonScene（兵力管理），点击"返回"后，MainScene 恢复了地图探索状态，但据点 UI 没有自动重新打开。用户期望回到据点 UI。

## 根因

`GarrisonSceneBridge.ReturnToMainScene()` → `SceneManager.LoadScene("MainScene")` 触发完整场景重载。
`GameManager.RestoreGameFromSession()` 恢复了游戏状态，但没有信号告知 StrongholdUI 需要重新打开。

## 方案

利用现有的 `GameSession` 持久化机制，增加一个"返回后应重开据点 UI"标记。

### 修改文件

#### 1. `Assets/Scripts/Shared/Session/GameSession.cs`

新增：

```
private bool garrisonShouldReopenStronghold;

public static bool HasGarrisonShouldReopenStronghold
    => instance != null && instance.garrisonShouldReopenStronghold;

public static void SetGarrisonShouldReopenStronghold()
{
    GameSession session = EnsureExists();
    session.garrisonShouldReopenStronghold = true;
}

public static void ClearGarrisonShouldReopenStronghold()
{
    if (instance == null) return;
    instance.garrisonShouldReopenStronghold = false;
}
```

`ClearAll()` 中一并清理此标记。

#### 2. `Assets/Scripts/Scenes/Main/UI/StrongholdUI.cs`

- `OnGarrisonCheck()` 中，在 `LoadGarrisonScene()` 前调用 `GameSession.SetGarrisonShouldReopenStronghold()`
- `Start()` 中检查标记，若为 true 则通过协程延迟一帧后调用 `Open()`（确保 GameManager 已完成状态恢复）
- 新增 `using System.Collections;` 用于协程

### 数据流

```
StrongholdUI.OnGarrisonCheck()
  → GameSession.SetGarrisonShouldReopenStronghold()  // 设置标记
  → GarrisonSceneBridge.LoadGarrisonScene()
  → GarrisonScene
  → 用户点"返回"
  → ReturnToMainScene()
  → MainScene 重新加载
  → GameManager.Start() → RestoreGameFromSession() 恢复玩家卡组
  → StrongholdUI.Start() 检测标记 → 延迟一帧 → Open()
  → GameSession.ClearGarrisonShouldReopenStronghold()
```

### 不修改的内容

- GarrisonSceneBridge — 不需要改
- GameManager — 不需要改
- 不涉及 Unity 场景/prefab/.asset 修改
