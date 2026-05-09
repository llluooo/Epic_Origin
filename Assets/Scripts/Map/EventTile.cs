using UnityEngine;

public class EventTile : Tile
{
    public override void OnHeroEnter()
    {
        float roll = Random.value;
        Player player = GameManager.Instance.player;

        if (roll < 0.4f)
        {
            // 40% 获得额外资源
            int gold = Random.Range(10, 21);
            int mat = Random.Range(5, 11);
            player.AddResources(new ResourceData(gold, mat));
            Debug.Log($"触发事件: 发现宝藏！获得 {gold}金币, {mat}建材");
        }
        else if (roll < 0.7f)
        {
            // 30% 失去少量资源
            int loseGold = Random.Range(5, 11);
            int actualLose = Mathf.Min(loseGold, player.resources.gold);
            player.resources.gold -= actualLose;
            Debug.Log($"触发事件: 遭遇强盗！损失 {actualLose}金币");
        }
        else
        {
            // 30% 无事发生
            Debug.Log("触发事件: 什么也没发生...");
        }
    }
}
