using UnityEngine;

public class ResourceTile : Tile
{
    public override void OnHeroEnter()
    {
        // 随机给金币或建材
        bool giveGold = Random.value > 0.5f;
        int amount;

        if (giveGold)
        {
            amount = Random.Range(3, 9);
            var reward = new ResourceData(amount, 0);
            GameManager.Instance.player.AddResources(reward);
            Debug.Log($"获得资源: {amount}金币");
        }
        else
        {
            amount = Random.Range(2, 6);
            var reward = new ResourceData(0, amount);
            GameManager.Instance.player.AddResources(reward);
            Debug.Log($"获得资源: {amount}建材");
        }
    }
}
