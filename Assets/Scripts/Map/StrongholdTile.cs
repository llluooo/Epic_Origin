using UnityEngine;

public enum StrongholdType
{
    Player,
    Enemy
}

public class StrongholdTile : Tile
{
    public StrongholdType strongholdType;
    public StrongholdUI strongholdUI;

    void Start()
    {
        // 视觉区分：玩家据点绿色，敌方据点红色
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = strongholdType == StrongholdType.Player
                ? new Color(0.3f, 0.8f, 0.3f)   // 绿色
                : new Color(0.7f, 0.3f, 0.9f);  // 紫色
        }
    }

    public override void OnHeroEnter()
    {
        if (strongholdType == StrongholdType.Player)
        {
            // 进入己方据点 → 打开经营面板
            if (strongholdUI != null)
            {
                strongholdUI.Open();
            }
            else
            {
                Debug.Log("进入己方据点（StrongholdUI未绑定）");
            }
        }
        else
        {
            // 进入敌方据点 → 触发攻占判定
            GameManager gm = GameManager.Instance;
            gm.OnHeroEnterEnemyStronghold(gm.player, gm.aiPlayer);
        }
    }
}
