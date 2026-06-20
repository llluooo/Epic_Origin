using UnityEngine;

public class ObstacleTile : Tile
{
    public override bool IsWalkable => false;

    public override void OnHeroEnter()
    {
        Debug.Log("Obstacle blocks movement: " + gridPosition);
    }
}
