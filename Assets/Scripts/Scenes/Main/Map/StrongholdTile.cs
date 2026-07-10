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
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(SFX.EnterStrongholdTile);

        if (strongholdType == StrongholdType.Player)
        {
            if (strongholdUI != null)
            {
                // 显示己方驻兵信息
                GameManager gm = GameManager.Instance;
                int garrisonCount = gm != null && gm.player != null ? gm.player.garrisonDeck.CardCount : 0;
                int heroCount = gm != null && gm.player != null ? gm.player.deck.CardCount : 0;
                MessageLogUI.Instance?.AddMessage($"进入己方据点（英雄{heroCount}张 + 驻兵{garrisonCount}张）");
                strongholdUI.Open();
            }
            else
            {
                Debug.Log("进入己方据点，但据点界面未绑定。");
            }
            return;
        }
        GameManager gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            Debug.LogError("无法进入敌方据点：游戏管理器未初始化。");
            return;
        }

        // 显示敌方驻兵信息
        int aiHeroCount = gameManager.aiPlayer?.deck?.CardCount ?? 0;
        int aiGarrisonCount = gameManager.aiPlayer?.garrisonDeck?.CardCount ?? 0;
        MessageLogUI.Instance?.AddMessage($"进入敌方据点（英雄{aiHeroCount}张 + 驻兵{aiGarrisonCount}张）");
        gameManager.StartEnemyStrongholdBattle(gridPosition);
    }
}