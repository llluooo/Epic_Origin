using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 战斗主循环管理器，只负责逻辑运算和流程控制。
/// </summary>
public class BattleManager
{
    public const int MaxBattleCardTypes = 6;

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

    public bool IsBattleOver()
    {
        return battleEnded;
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
            Debug.LogWarning("无效的出牌选择，回合未执行。");
            return;
        }

        BattleCard attacker = attackerList[attackerIndex];
        BattleCard defender = defenderList[defenderIndex];

        if (isPlayerAttacking && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(SFX.Attack);

        float attackModifier = BattleCalculator.GetRaceModifier(attacker.card.race, defender.card.race);
        int attackDamage = BattleCalculator.CalculateAttackDamage(attacker, defender);

        int defenderHPBefore = defender.currentHP;
        int defenderCountBefore = defender.currentCount;

        if (!isPlayerAttacking && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(SFX.Defend);

        defender.TakeDamage(attackDamage);

        Debug.Log($"[回合{currentRound} 当前攻方:{attackerName}] {attackerName} {attacker.card.cardName} 出牌，{defenderName} {defender.card.cardName} 应战。");
        Debug.Log($"伤害：{attacker.GetTotalAttack()}x{attackModifier:0.0#}={attackDamage}。");
        Debug.Log($"结果：{defender.card.cardName} 生命 {defenderHPBefore}->{defender.currentHP} 数量 {defenderCountBefore}->{defender.currentCount}。");

        if (!defender.IsAlive())
        {
            Debug.Log($"{defender.card.cardName} 已阵亡。");
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(SFX.UnitDeath);
        }

        CheckBattleEnd();

        if (!battleEnded)
        {
            isPlayerAttacking = !isPlayerAttacking;
            currentRound++;
        }
    }

    public void Surrender(bool playerSide)
    {
        if (battleEnded)
        {
            return;
        }

        battleEnded = true;

        if (playerSide && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(SFX.Flee);

        AudioManager.Instance?.StopBGM();

        outcome = playerSide ? BattleOutcome.PlayerSurrender : BattleOutcome.EnemySurrender;
        battleResult = playerSide ? "玩家投降" : "敌方投降";
        Debug.Log($"战斗结束：{battleResult}");
    }

    /// <summary>
    /// 玩家逃跑，50%概率成功脱战，失败则所有己方卡牌HP减半
    /// </summary>
    public bool AttemptFlee()
    {
        if (battleEnded)
        {
            return false;
        }

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(SFX.Flee);

        bool success = Random.value < 0.5f;

        if (success)
        {
            battleEnded = true;
            outcome = BattleOutcome.PlayerFled;
            battleResult = "玩家逃跑成功";
            AudioManager.Instance?.StopBGM();
            Debug.Log("玩家逃跑成功！");
            return true;
        }

        // 逃跑失败：所有玩家卡牌HP减半（至少保留1）
        foreach (BattleCard card in playerCards)
        {
            if (card != null && card.IsAlive())
            {
                int newHP = Mathf.Max(1, card.currentHP / 2);
                int hpLost = card.currentHP - newHP;
                card.TakeDamage(hpLost);
            }
        }

        // 回合交给敌方
        isPlayerAttacking = false;
        Debug.Log("玩家逃跑失败，所有己方卡牌HP减半！");
        return false;
    }

    private bool IsValidSelection(List<BattleCard> list, int index)
    {
        return list != null &&
               index >= 0 &&
               index < list.Count &&
               list[index] != null &&
               list[index].IsAlive();
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
            else if (target.Count < MaxBattleCardTypes)
            {
                battleCard = new BattleCard(card, quantity);
                cardsByKey.Add(key, battleCard);
                target.Add(battleCard);
            }
        }
    }

    private void CheckBattleEnd()
    {
        bool playerAlive = playerCards.Exists(card => card != null && card.IsAlive());
        bool enemyAlive = enemyCards.Exists(card => card != null && card.IsAlive());

        if (playerAlive && enemyAlive)
        {
            return;
        }

        battleEnded = true;
        if (playerAlive && !enemyAlive)
        {
            outcome = BattleOutcome.PlayerVictory;
            battleResult = "玩家胜利";

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StopBGM();
                AudioManager.Instance.PlaySFX(SFX.BattleVictory);
            }
        }
        else if (!playerAlive && enemyAlive)
        {
            outcome = BattleOutcome.EnemyVictory;
            battleResult = "敌方胜利";

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StopBGM();
                AudioManager.Instance.PlaySFX(SFX.Defeat);
            }
        }
        else
        {
            outcome = BattleOutcome.Draw;
            battleResult = "双方同归于尽";
        }

        Debug.Log($"战斗结束：{battleResult}");
    }
}