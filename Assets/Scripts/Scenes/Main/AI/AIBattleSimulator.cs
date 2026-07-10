using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AI 兵营后台战斗模拟器，使用完整 BattleManager + BattleAI 流程，
/// 但不进入战斗场景，纯数据模拟。
/// </summary>
public static class AIBattleSimulator
{
    /// <summary>
    /// 模拟 AI 与兵营的完整战斗，直接修改 AI 的卡组数据。
    /// </summary>
    /// <returns>战斗是否胜利</returns>
    public static bool SimulateArmyCampBattle(Player aiPlayer, int currentTurn)
    {
        List<Card> aiDeck = CloneDeck(aiPlayer.deck.cards);
        List<Card> enemyDeck = GenerateArmyCampDeck(currentTurn);

        if (aiDeck.Count == 0)
        {
            Debug.Log("[AIBattleSimulator] AI 没有可用的卡牌，战斗跳过。");
            return false;
        }

        Debug.Log($"[AIBattleSimulator] 开始后台战斗模拟（回合{currentTurn}）");
        Debug.Log($"[AIBattleSimulator] AI 出战：{DeckToString(aiDeck)}");
        Debug.Log($"[AIBattleSimulator] 兵营敌方：{DeckToString(enemyDeck)}");

        BattleManager battle = new BattleManager();
        battle.Initialize(aiDeck, enemyDeck, playerStartsAttacking: true);

        while (!battle.IsBattleOver())
        {
            List<BattleCard> attackerList = battle.isPlayerAttacking ? battle.playerCards : battle.enemyCards;
            List<BattleCard> defenderList = battle.isPlayerAttacking ? battle.enemyCards : battle.playerCards;

            int attackerIndex = BattleAI.ChooseAttackerIndex(attackerList);
            BattleCard attacker = attackerList[attackerIndex];
            int defenderIndex = BattleAI.ChooseDefenderIndex(defenderList, attacker);

            battle.ExecuteRound(attackerIndex, defenderIndex);
        }

        bool aiWon = battle.outcome == BattleOutcome.PlayerVictory;
        Debug.Log($"[AIBattleSimulator] 战斗结束：{(aiWon ? "AI 胜利" : "AI 失败")}");

        ApplyBattleResult(aiPlayer, battle.playerCards, aiWon ? enemyDeck : null);
        return aiWon;
    }

    /// <summary>
    /// 根据回合数生成兵营敌方卡组
    /// </summary>
    private static List<Card> GenerateArmyCampDeck(int turn)
    {
        RaceType race = (RaceType)Random.Range(0, 3);
        int cardCount = Mathf.Min(2 + turn / 5, 6);
        int maxLevel = Mathf.Min(5, 1 + turn / 4);

        List<Card> deck = new List<Card>();
        HashSet<int> usedIndices = new HashSet<int>();

        for (int i = 0; i < cardCount; i++)
        {
            int unitIndex;
            int attempts = 0;
            do
            {
                unitIndex = Random.Range(0, maxLevel);
                attempts++;
            } while (usedIndices.Contains(unitIndex) && attempts < 10);

            usedIndices.Add(unitIndex);

            Card card = CreateCardForRace(race, unitIndex);
            card.quantity = Random.Range(1, 4);
            deck.Add(card);
        }

        return deck;
    }

    private static Card CreateCardForRace(RaceType race, int unitIndex)
    {
        return race switch
        {
            RaceType.Human => HumanUnit.CreateCard(unitIndex),
            RaceType.Heaven => HeavenUnit.CreateCard(unitIndex),
            RaceType.Ghost => GhostUnit.CreateCard(unitIndex),
            _ => HumanUnit.CreateCard(unitIndex),
        };
    }

    private static List<Card> CloneDeck(List<Card> source)
    {
        List<Card> clone = new List<Card>();
        foreach (Card card in source)
        {
            if (card != null && card.quantity > 0)
                clone.Add(card.Clone());
        }
        return clone;
    }

    /// <summary>
    /// 将战斗结果写回 AI 的原始卡组
    /// </summary>
    private static void ApplyBattleResult(Player aiPlayer, List<BattleCard> survivingCards, List<Card> enemyDeck)
    {
        Deck originalDeck = aiPlayer.deck;

        // 根据 BattleCard 的剩余数量更新原始卡组
        foreach (BattleCard bc in survivingCards)
        {
            if (bc == null || bc.card == null) continue;

            Card originalCard = FindMatchingCard(originalDeck, bc.card.race, bc.card.unitIndex);
            if (originalCard != null)
            {
                int lost = Mathf.Max(0, bc.initialCount - bc.currentCount);
                originalCard.quantity -= lost;
                if (originalCard.quantity <= 0)
                {
                    originalDeck.RemoveCard(originalCard);
                    Debug.Log($"[AIBattleSimulator] AI 损失：{bc.card.cardName} 全部阵亡");
                }
                else
                {
                    originalCard.currentHP = originalCard.GetMaxHP();
                    Debug.Log($"[AIBattleSimulator] AI 卡牌 {bc.card.cardName} 剩余 {originalCard.quantity} 个");
                }
            }
        }

        // 胜利奖励：获得敌方随机卡牌
        if (enemyDeck != null && enemyDeck.Count > 0)
        {
            int rewardCount = Mathf.Min(Random.Range(1, 3), enemyDeck.Count);
            List<int> indices = new List<int>();
            for (int i = 0; i < enemyDeck.Count; i++) indices.Add(i);
            Shuffle(indices);

            for (int i = 0; i < rewardCount; i++)
            {
                Card reward = enemyDeck[indices[i]].Clone();
                reward.quantity = 1;
                reward.currentHP = reward.GetMaxHP();

                if (aiPlayer.TryAddToHeroDeck(reward))
                {
                    Debug.Log($"[AIBattleSimulator] AI 获得 {reward.cardName} Lv{reward.level}");
                }
                else
                {
                    aiPlayer.AddToGarrison(reward);
                    Debug.Log($"[AIBattleSimulator] AI 获得 {reward.cardName} Lv{reward.level}（移入驻军）");
                }
            }
        }
    }

    private static Card FindMatchingCard(Deck deck, RaceType race, int unitIndex)
    {
        for (int i = 0; i < deck.CardCount; i++)
        {
            Card card = deck[i];
            if (card.race == race && card.unitIndex == unitIndex)
                return card;
        }
        return null;
    }

    private static void Shuffle(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int r = Random.Range(0, i + 1);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }

    private static string DeckToString(List<Card> deck)
    {
        List<string> parts = new List<string>();
        foreach (Card c in deck)
            parts.Add($"{c.cardName} Lv{c.level} x{c.quantity}");
        return string.Join(" | ", parts);
    }
}
