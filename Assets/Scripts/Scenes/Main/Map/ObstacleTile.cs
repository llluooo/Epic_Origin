using UnityEngine;

public class ObstacleTile : Tile
{
    public override bool IsWalkable => false;

    public override void OnHeroEnter()
    {
        Debug.Log("障碍物阻挡移动: " + gridPosition);
    }
}
