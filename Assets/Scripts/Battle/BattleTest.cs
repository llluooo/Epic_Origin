using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 战斗测试入口脚本，用于在场景中触发控制台模拟战斗
/// </summary>
public class BattleTest : MonoBehaviour
{
    public bool autoStart = true;
    public int minCardQuantity = 3;
    public int maxCardQuantity = 8;

    private BattleManager battleManager;

    void Start()
    {
        battleManager = new BattleManager();

        if (autoStart)
            StartCoroutine(StartAutoBattle());
    }

    private System.Collections.IEnumerator StartAutoBattle()
    {
        List<Card> playerDeck = CreateRandomDeck(5);
        List<Card> enemyDeck = CreateRandomDeck(5);
        bool playerStarts = Random.value < 0.5f;

        battleManager.Initialize(playerDeck, enemyDeck, playerStartsAttacking: playerStarts);
        Debug.Log($"开始自动战斗测试，玩家先攻：{playerStarts}");
        Debug.Log($"玩家出战卡组：{GetDeckDescription(playerDeck)}");
        Debug.Log($"敌方出战卡组：{GetDeckDescription(enemyDeck)}");

        while (!battleManager.IsBattleOver())
        {
            if (battleManager.isPlayerAttacking)
            {
                int attackerIndex = BattleAI.ChooseAttackerIndex(battleManager.playerCards);
                int defenderIndex = BattleAI.ChooseDefenderIndex(battleManager.enemyCards, battleManager.playerCards[attackerIndex]);
                battleManager.ExecuteRound(attackerIndex, defenderIndex);
            }
            else
            {
                int attackerIndex = BattleAI.ChooseAttackerIndex(battleManager.enemyCards);
                int defenderIndex = BattleAI.ChooseDefenderIndex(battleManager.playerCards, battleManager.enemyCards[attackerIndex]);
                battleManager.ExecuteRound(attackerIndex, defenderIndex);
            }

            yield return null;
        }

        Debug.Log("战斗测试完成");
    }

    private List<Card> CreateRandomDeck(int count)
    {
        var deck = new List<Card>();
        for (int i = 0; i < count; i++)
        {
            deck.Add(CreateRandomCard());
        }
        return deck;
    }

    private Card CreateRandomCard()
    {
        int raceIndex = Random.Range(0, 3);
        int unitIndex = Random.Range(0, 5);

        Card card = raceIndex switch
        {
            0 => HumanUnit.CreateCard(unitIndex),
            1 => HeavenUnit.CreateCard(unitIndex),
            2 => GhostUnit.CreateCard(unitIndex),
            _ => HumanUnit.CreateCard(unitIndex),
        };

        int lower = Mathf.Max(1, minCardQuantity);
        int upperExclusive = Mathf.Max(lower + 1, maxCardQuantity + 1);
        card.quantity = Random.Range(lower, upperExclusive);
        return card;
    }

    private int GetRandomAliveIndex(List<BattleCard> list)
    {
        var aliveIndices = new List<int>();
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].IsAlive())
                aliveIndices.Add(i);
        }

        if (aliveIndices.Count == 0)
            return 0;

        return aliveIndices[Random.Range(0, aliveIndices.Count)];
    }

    private string GetDeckDescription(List<Card> deck)
    {
        var descriptions = new List<string>();
        foreach (var card in deck)
            descriptions.Add($"{card.cardName}({card.race} Lv{card.level}) x{card.quantity}");
        return string.Join(", ", descriptions);
    }
}

