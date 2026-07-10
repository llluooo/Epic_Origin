using UnityEngine;

public class EmptyTile : Tile
{
    public override void OnHeroEnter()
    {
        Debug.Log("空地");
        MessageLogUI.Instance?.AddMessage("空地");
    }
}
