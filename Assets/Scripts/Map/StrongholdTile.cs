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
                Debug.Log("进入己方据点，但 StrongholdUI 未绑定。");
            }

            return;
        }

        GameManager gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            Debug.LogError("无法进入敌方据点：GameManager 未初始化。");
            return;
        }

        gameManager.StartEnemyStrongholdBattle();
    }
}
