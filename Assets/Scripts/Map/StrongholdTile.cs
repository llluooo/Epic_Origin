using UnityEngine;

public class StrongholdTile : Tile
{
    public override void OnHeroEnter()
    {
        Debug.Log("进入据点！");
    }
}