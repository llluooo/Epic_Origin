# 种族选择系统设计文档

**日期：** 2026-05-11
**状态：** 已确认

---

## 架构概览

### 场景变更

- **MainMenuScene** — 主菜单（新增）。四个按钮：开始新游戏（连 RaceSelectScene）、载入存档（占位）、设置（占位）、退出游戏（Application.Quit()）
- **RaceSelectScene** — 种族选择（新增）。三种族列表式选择，确认后写入 GameSetupData，跳转 MainScene
- **MainScene** — 保持不变。GameManager.Start() 从 GameSetupData 读取种族数据替代硬编码

### Build Settings 场景顺序

1. MainMenuScene（索引0，启动场景）
2. RaceSelectScene
3. MainScene

### 流程

```
启动 → MainMenuScene → "开始新游戏" → RaceSelectScene → 确认种族 → MainScene → 游戏开始
        ├─ "载入存档" (占位)
        ├─ "设置" (占位)
        └─ "退出游戏" → Application.Quit()
```

---

## 新增类

### GameSetupData — 静态数据桥接

```csharp
public static class GameSetupData
{
    public static RaceType PlayerRace;
    public static RaceType EnemyRace;
    public static bool IsNewGame;
}
```

纯 C# 静态类，不继承 MonoBehaviour，不需要挂载。RaceSelectUI 写入，GameManager.Start() 读取。
IsNewGame 用于后续区分"新游戏"和"读档"流程。

### MainMenuUI — 主菜单

挂载在 MainMenuScene Canvas 上。

- 四个公开 Button 字段（开始新游戏、载入存档、设置、退出游戏），在编辑器拖入
- 开始新游戏：`SceneManager.LoadScene("RaceSelectScene")`
- 载入存档：空方法（预留）
- 设置：空方法（预留）
- 退出游戏：`Application.Quit()`

### RaceSelectUI — 种族选择

挂载在 RaceSelectScene Canvas 上。

- 三个种族行 Button 字段 + 确认 Button 字段
- 内部维护当前选中种族状态（可为 null）
- 默认未选中，确认按钮置灰（interactable = false）
- 点击某行选中并高亮，确认按钮激活；再次点击同一行取消
- 确认逻辑：
  1. 防御检查是否有选中种族
  2. 从 RaceType 枚举中排除玩家所选，随机抽一个作为敌人种族
  3. 写入 GameSetupData（PlayerRace、EnemyRace、IsNewGame = true）
  4. `SceneManager.LoadScene("MainScene")`

---

## 现有类改动

### GameManager

`StartGame()` 不再硬编码种族：

```csharp
if (GameSetupData.IsNewGame)
{
    player.race = GameSetupData.PlayerRace;
    aiPlayer.race = GameSetupData.EnemyRace;
}
// else 回退到默认 Human vs Ghost（直接进 MainScene 时）
```

初始卡组也根据种族动态创建：

```csharp
Card playerCard = player.race switch
{
    RaceType.Human => HumanUnit.CreateCard(0, 3),
    RaceType.Heaven => HeavenUnit.CreateCard(0, 3),
    RaceType.Ghost => GhostUnit.CreateCard(0, 3),
    _ => HumanUnit.CreateCard(0, 3),
};
```

AI 卡组同理。

### Hero

新增 `SetRaceAppearance(RaceType)` 方法：

```csharp
private static readonly Dictionary<RaceType, Color> RaceColors = new()
{
    { RaceType.Human, new Color(0.3f, 0.5f, 1f) },
    { RaceType.Heaven, new Color(1f, 0.85f, 0.3f) },
    { RaceType.Ghost, new Color(0.6f, 0.3f, 0.8f) },
};

public void SetRaceAppearance(RaceType race)
{
    if (RaceColors.TryGetValue(race, out var color))
        GetComponent<SpriteRenderer>().color = color;
}
```

数据结构预留精灵字段，美术资源到位后把 Dictionary<RaceType, Color> 改为 Dictionary<RaceType, Sprite>。

### MapGenerator

放置英雄后调用 `hero.SetRaceAppearance(GameSetupData.PlayerRace)`：

---

## 边界情况处理

| 场景 | 处理 |
|------|------|
| 未选种族点确认 | 按钮置灰，点击无效 |
| 直接进 MainScene（跳过选择） | GameManager 检测 IsNewGame == false，回退默认 Human vs Ghost |
| 退回主菜单再进选择 | 重新初始化，GameSetupData 会被覆盖 |

---

## 编辑器挂载清单

### MainMenuScene
1. 创建 Canvas
2. 创建四个 Button 子物体（开始新游戏、载入存档、设置、退出游戏）
3. Canvas 挂载 `MainMenuUI.cs`
4. 把四个 Button 拖到脚本对应字段

### RaceSelectScene
1. 创建 Canvas
2. 搭建三行种族选项（色块 Image + 种族名 TMP + 描述 TMP），每行一个 Button
3. 创建一个确认 Button
4. Canvas 挂载 `RaceSelectUI.cs`
5. 把三行 Button 和确认 Button 拖到脚本对应字段
