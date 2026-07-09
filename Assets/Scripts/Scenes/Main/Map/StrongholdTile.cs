using UnityEngine;

public enum StrongholdType
{
    Player,
    Enemy
}

public class StrongholdTile : Tile
{
    public StrongholdType strongholdType;
    public RaceType visualRace;
    public StrongholdUI strongholdUI;

    public override void OnHeroEnter()
    {
        if (strongholdType == StrongholdType.Player)
        {
            if (strongholdUI != null)
            {
                strongholdUI.Open();
            }
            else
            {
                string msg = "进入己方据点，但据点界面未绑定。";
                Debug.Log(msg);
                MessageLogUI.Instance?.AddMessage(msg);
            }

            return;
        }

        GameManager gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            Debug.LogError("无法进入敌方据点：游戏管理器未初始化。");
            return;
        }

        string msg2 = "进入敌方据点，切换到战斗场景。";
        Debug.Log(msg2);
        gameManager.StartEnemyStrongholdBattle(gridPosition);
    }
}
