using UnityEngine;

public class ResourceTile : Tile
{
    public override void OnHeroEnter()
    {
        bool giveGold = Random.value > 0.5f;
        int amount = Random.Range(40, 61);

        if (giveGold)
        {
            GameManager.Instance.player.AddResources(new ResourceData(amount, 0));
            string msg = $"获得资源: {amount}金币";
            Debug.Log(msg);
            MessageLogUI.Instance?.AddMessage(msg);
        }
        else
        {
            GameManager.Instance.player.AddResources(new ResourceData(0, amount));
            string msg = $"获得资源: {amount}建材";
            Debug.Log(msg);
            MessageLogUI.Instance?.AddMessage(msg);
        }
    }
}
