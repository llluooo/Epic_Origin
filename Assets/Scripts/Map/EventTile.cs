using UnityEngine;

public class EventTile : Tile
{
    public override void OnHeroEnter()
    {
        Debug.Log("触发随机事件！");
    }
}