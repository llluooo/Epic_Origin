using UnityEngine;

public class ArmyCampTile : Tile
{
    public override void OnHeroEnter()
    {
        Player player = GameManager.Instance.player;
        int playerPower = player.deck.GetTotalCombatPower();

        // 敌方兵营战力随机 10-50
        int enemyPower = Random.Range(10, 51);

        Debug.Log($"遭遇兵营敌人！你的战力:{playerPower} vs 敌方战力:{enemyPower}");

        if (playerPower >= enemyPower)
        {
            // 胜利：获得金币奖励
            int goldReward = Random.Range(8, 21);
            player.resources.gold += goldReward;
            Debug.Log($"战胜兵营！获得 {goldReward}金币");
        }
        else
        {
            // 失败：损失一些金币
            int loseGold = Random.Range(5, 16);
            int actualLose = Mathf.Min(loseGold, player.resources.gold);
            player.resources.gold -= actualLose;
            Debug.Log($"兵营战斗失败！损失 {actualLose}金币");
        }
    }
}
