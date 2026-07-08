using UnityEngine.SceneManagement;

/// <summary>
/// 据点兵力管理场景的桥接器，负责进出 GarrisonScene 时的状态存取。
/// 参考 BattleSceneBridge 模式。
/// </summary>
public static class GarrisonSceneBridge
{
    public const string GarrisonSceneName = "GarrisonScene";
    public const string MainSceneName = "MainScene";

    /// <summary>
    /// 从 MainScene 进入 GarrisonScene，存储当前 runState 和玩家兵力数据
    /// </summary>
    public static void LoadGarrisonScene(GameRunState runState)
    {
        GameSession.StoreGarrisonState(runState);
        SceneManager.LoadScene(GarrisonSceneName);
    }

    /// <summary>
    /// 从 GarrisonScene 返回 MainScene，携带调整后的英雄卡组和驻军卡组
    /// </summary>
    public static void ReturnToMainScene(Deck heroDeck, Deck garrisonDeck)
    {
        GameSession.StoreGarrisonReturnData(heroDeck, garrisonDeck);
        SceneManager.LoadScene(MainSceneName);
    }
}
