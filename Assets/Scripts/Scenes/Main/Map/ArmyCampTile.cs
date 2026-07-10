using System.Collections.Generic;
using UnityEngine;
public class ArmyCampTile : Tile
{
    public override void OnHeroEnter()
    {
        if (Cleared) return;

        GameManager gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            Debug.LogError("兵营格子：游戏管理器未初始化。");
            return;
        }

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(SFX.EnterCampTile);

        List<Card> enemyDeck = CreateEnemyEncounterDeck();
        Debug.Log("进入兵营，切换到战斗场景。");
        MessageLogUI.Instance?.AddMessage("进入兵营");
        gameManager.StartArmyCampBattle(enemyDeck, gridPosition);
    }
    private List<Card> CreateEnemyEncounterDeck()
    {
        int cardCount = Random.Range(3, 6);
        var deck = new List<Card>(cardCount);
        for (int i = 0; i < cardCount; i++)
        {
            RaceType race = (RaceType)Random.Range(0, 3);
            int unitIndex = Random.Range(0, 3);
            deck.Add(CreateRandomCard(race, unitIndex));
        }
        return deck;
    }
    private Card CreateRandomCard(RaceType race, int unitIndex)
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