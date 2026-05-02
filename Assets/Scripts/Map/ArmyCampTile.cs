using UnityEngine;

public class ArmyCampTile : Tile
{
    public override void OnHeroEnter()
    {
        Debug.Log("进入战斗！（未来接Battle）");
    }
}