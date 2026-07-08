// AudioEnums.cs
// 《史诗起点 Epic Origin》音频枚举
// 组员调用时使用这些枚举名，例如 AudioManager.Instance.PlaySFX(SFX.ButtonClick);

public enum BGM
{
    None,
    MainMenu,      // BGM1：主菜单 / 种族选择 / 存档选择 三个场景共用
    Map,           // BGM2：地图界面
    BattlePrep,    // BGM3：战前选卡组
    Battle,        // BGM4：进入战斗
}

public enum SFX
{
    // ---- 通用 UI ----
    ButtonClick,         // 所有界面通用的按钮点击

    // ---- 地图 ----
    Move,                // 英雄移动
    EnterCampTile,       // 走到兵营营地地块
    EnterStrongholdTile, // 走到敌方据点地块（敌方走到我方据点也用这个）
    GetGold,             // 获得金币（结算里"获得金币"也用这个）
    GetWood,             // 获得建材
    EnterEventTile,      // 走到随机事件地块

    // ---- 战斗 ----
    CardSelect,          // 出牌阶段：选中卡牌
    CardCancel,          // 出牌阶段：取消选择
    Attack,              // 攻击
    Defend,              // 防守 / 受击表现
    TakeDamage,          // 扣血

    // ---- 结算 ----
    Flee,                // 逃跑
    BattleVictory,       // 单场战斗胜利
    GetCard,             // 获得卡牌
    Defeat,              // 战败
    GameVictory,         // 游戏通关胜利（区别于单场战斗胜利）
}
