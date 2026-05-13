using UnityEngine;

public class ArmyCampTile : Tile
{
    public override void OnHeroEnter()
    {
        Player player = GameManager.Instance.player;
        int playerPower = player.deck.GetTotalCombatPower();

        int enemyPower = Random.Range(10, 51);

        Debug.Log($"遭遇兵营敌人！你的战力:{playerPower} vs 敌方战力:{enemyPower}");

        if (playerPower >= enemyPower)
        {
            int goldReward = Random.Range(30, 51);
            player.resources.gold += goldReward;

            // 随机获得1张随机种族的 Lv1~Lv2 卡牌
            RaceType randomRace = (RaceType)Random.Range(0, 3);
            int randomUnitIndex = Random.Range(0, 2);  // 0=Lv1, 1=Lv2
            Card rewardCard = CreateRandomCard(randomRace, randomUnitIndex);
            player.deck.AddCard(rewardCard);

            Debug.Log($"战胜兵营！获得 {goldReward}金币, {rewardCard.cardName} Lv{rewardCard.level}");
        }
        else
        {
            int loseGold = Random.Range(10, 31);
            int actualLose = Mathf.Min(loseGold, player.resources.gold);
            player.resources.gold -= actualLose;
            Debug.Log($"兵营战斗失败！损失 {actualLose}金币");
        }
    }

    Card CreateRandomCard(RaceType race, int unitIndex)
    {
        return race switch
        {
            RaceType.Human => HumanUnit.CreateCard(unitIndex),
            RaceType.Heaven => HeavenUnit.CreateCard(unitIndex),
            RaceType.Ghost => GhostUnit.CreateCard(unitIndex),
            _ => HumanUnit.CreateCard(0),
        };
    }
}
