using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 战斗主循环管理器，仅负责逻辑运算和流程控制。
/// </summary>
public class BattleManager
{
    public List<BattleCard> playerCards = new List<BattleCard>();
    public List<BattleCard> enemyCards = new List<BattleCard>();
    public int currentRound;
    public bool isPlayerAttacking;
    public bool battleEnded;
    public string battleResult;
    public BattleOutcome outcome;

    public void Initialize(List<Card> playerDeck, List<Card> enemyDeck, bool playerStartsAttacking = true)
    {
        playerCards.Clear();
        enemyCards.Clear();
        battleEnded = false;
        battleResult = string.Empty;
        outcome = BattleOutcome.Draw;
        currentRound = 1;
        isPlayerAttacking = playerStartsAttacking;

        BuildBattleCards(playerDeck, playerCards);
        BuildBattleCards(enemyDeck, enemyCards);

        Debug.Log($"战斗初始化完成，玩家先攻：{playerStartsAttacking}");
        Debug.Log($"玩家出战：{string.Join(" | ", playerCards)}");
        Debug.Log($"敌方出战：{string.Join(" | ", enemyCards)}");

        CheckBattleEnd();
    }

    public void ResetRoundStates()
    {
        foreach (BattleCard card in playerCards)
        {
            card.ResetRound();
        }

        foreach (BattleCard card in enemyCards)
        {
            card.ResetRound();
        }
    }

    public bool IsBattleOver()
    {
        return battleEnded;
    }

    public List<int> GetAlivePlayerIndices()
    {
        return GetAliveIndices(playerCards);
    }

    public List<int> GetAliveEnemyIndices()
    {
        return GetAliveIndices(enemyCards);
    }

    public string GetBattleStatus()
    {
        return $"回合{currentRound}，当前攻方：{(isPlayerAttacking ? "玩家" : "敌方")}";
    }

    public BattleState GetBattleState()
    {
        var state = new BattleState
        {
            currentRound = currentRound,
            isPlayerAttacking = isPlayerAttacking
        };

        state.playerCardStates.Clear();
        state.enemyCardStates.Clear();

        foreach (BattleCard card in playerCards)
        {
            state.playerCardStates.Add(card == null ? "空" : card.ToString());
        }

        foreach (BattleCard card in enemyCards)
        {
            state.enemyCardStates.Add(card == null ? "空" : card.ToString());
        }

        return state;
    }

    public string GetDetailedStatus()
    {
        return GetBattleState().ToString();
    }

    public void ExecuteRound(int attackerIndex, int defenderIndex)
    {
        if (battleEnded)
        {
            return;
        }

        List<BattleCard> attackerList = isPlayerAttacking ? playerCards : enemyCards;
        List<BattleCard> defenderList = isPlayerAttacking ? enemyCards : playerCards;
        string attackerName = isPlayerAttacking ? "玩家" : "敌方";
        string defenderName = isPlayerAttacking ? "敌方" : "玩家";

        if (!IsValidSelection(attackerList, attackerIndex) || !IsValidSelection(defenderList, defenderIndex))
        {
            Debug.LogWarning("无效的出牌选择，回合跳过。");
            return;
        }

        BattleCard attacker = attackerList[attackerIndex];
        BattleCard defender = defenderList[defenderIndex];

        attacker.hasActed = true;

        float attackModifier = BattleCalculator.GetRaceModifier(attacker.card.race, defender.card.race);
        float counterModifier = BattleCalculator.GetRaceModifier(defender.card.race, attacker.card.race);
        int attackDamage = BattleCalculator.CalculateAttackDamage(attacker, defender);
        int counterDamage = BattleCalculator.CalculateCounterDamage(defender, attacker);

        int defenderHPBefore = defender.currentHP;
        int attackerHPBefore = attacker.currentHP;
        int defenderCountBefore = defender.currentCount;
        int attackerCountBefore = attacker.currentCount;
        defender.TakeDamage(attackDamage);
        attacker.TakeDamage(counterDamage);

        Debug.Log($"[{GetBattleStatus()}] {attackerName} {attacker.card.cardName} 出牌，{defenderName} {defender.card.cardName} 应战。");
        Debug.Log($"伤害：主动 {attacker.GetTotalAttack()}x{attackModifier:0.0#}={attackDamage}，反击 {defender.GetTotalAttack()}x0.5x{counterModifier:0.0#}={counterDamage}。");
        Debug.Log($"结果：{defender.card.cardName} 生命 {defenderHPBefore}->{defender.currentHP} 数量 {defenderCountBefore}->{defender.currentCount}；{attacker.card.cardName} 生命 {attackerHPBefore}->{attacker.currentHP} 数量 {attackerCountBefore}->{attacker.currentCount}。");

        if (!defender.IsAlive())
        {
            Debug.Log($"{defender.card.cardName} 已阵亡。");
        }

        if (!attacker.IsAlive())
        {
            Debug.Log($"{attacker.card.cardName} 已阵亡。");
        }

        CheckBattleEnd();

        if (!battleEnded)
        {
            isPlayerAttacking = !isPlayerAttacking;
            currentRound++;
            ResetRoundStates();
        }
    }

    public void Surrender(bool playerSide)
    {
        if (battleEnded)
        {
            return;
        }

        battleEnded = true;
        outcome = playerSide ? BattleOutcome.PlayerSurrender : BattleOutcome.EnemySurrender;
        battleResult = playerSide ? "玩家投降" : "敌方投降";
        Debug.Log($"战斗结束：{battleResult}");
    }

    private bool IsValidSelection(List<BattleCard> list, int index)
    {
        return index >= 0 && index < list.Count && list[index] != null && list[index].IsAlive();
    }

    private void BuildBattleCards(List<Card> sourceDeck, List<BattleCard> target)
    {
        var cardsByKey = new Dictionary<string, BattleCard>();

        if (sourceDeck == null)
        {
            return;
        }

        foreach (Card card in sourceDeck)
        {
            if (card == null)
            {
                continue;
            }

            string key = $"{card.race}_{card.unitIndex}";
            int quantity = Mathf.Max(1, card.quantity);

            if (cardsByKey.TryGetValue(key, out BattleCard battleCard))
            {
                battleCard.AddUnits(quantity);
            }
            else if (target.Count < 5)
            {
                battleCard = new BattleCard(card, quantity);
                cardsByKey.Add(key, battleCard);
                target.Add(battleCard);
            }
        }
    }

    private List<int> GetAliveIndices(List<BattleCard> list)
    {
        var alive = new List<int>();
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].IsAlive())
            {
                alive.Add(i);
            }
        }

        return alive;
    }

    private void CheckBattleEnd()
    {
        bool playerAlive = playerCards.Exists(card => card.IsAlive());
        bool enemyAlive = enemyCards.Exists(card => card.IsAlive());

        if (playerAlive && enemyAlive)
        {
            return;
        }

        battleEnded = true;
        if (playerAlive && !enemyAlive)
        {
            outcome = BattleOutcome.PlayerVictory;
            battleResult = "玩家胜利";
        }
        else if (!playerAlive && enemyAlive)
        {
            outcome = BattleOutcome.EnemyVictory;
            battleResult = "敌方胜利";
        }
        else
        {
            outcome = BattleOutcome.Draw;
            battleResult = "双方同归于尽";
        }

        Debug.Log($"战斗结束：{battleResult}");
    }
}
