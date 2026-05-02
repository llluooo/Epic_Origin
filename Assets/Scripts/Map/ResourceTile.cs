using UnityEngine;

public class ResourceTile : Tile
{
    public override void OnHeroEnter()
    {
        Debug.Log("获得资源！");
    }
}