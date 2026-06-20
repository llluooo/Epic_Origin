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
                Debug.Log("Entered player stronghold, but StrongholdUI is not assigned.");
            }
        }
        else
        {
            GameManager gm = GameManager.Instance;
            gm.OnHeroEnterEnemyStronghold(gm.player, gm.aiPlayer);
        }
    }
}
