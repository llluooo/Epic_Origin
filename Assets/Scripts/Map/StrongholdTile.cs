using UnityEngine;

public enum StrongholdType
{
    Player,
    Enemy
}

public class StrongholdTile : Tile
{
    public StrongholdType strongholdType;

    public override void OnHeroEnter()
    {
        Debug.Log("进入据点！" + strongholdType);
    }
}
