using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 简单的战斗AI决策：进攻优先高等级，防守优先克制/同级以上
/// </summary>
public static class BattleAI
{
    public struct SurrenderDecision
    {
        public bool canSurrender;
        public float chance;
        public int currentPower;
        public int initialPower;
        public int opponentPower;
        public string reason;
    }

    // 进攻选择：选择等级最高的存活卡牌，Tiebreaker：总攻击更高
    public static int ChooseAttackerIndex(List<BattleCard> own)
    {
        int best = -1;
        int bestLevel = int.MinValue;
        int bestAtk = int.MinValue;
        for (int i = 0; i < own.Count; i++)
        {
            var c = own[i];
            if (c == null || !c.IsAlive()) continue;
            int level = c.card.level;
            int atk = c.GetTotalAttack();
            if (level > bestLevel || (level == bestLevel && atk > bestAtk))
            {
                best = i;
                bestLevel = level;
                bestAtk = atk;
            }
        }
        return best >= 0 ? best : 0;
    }

    // 防守选择：优先选择种族克制（modifier>1）的卡牌，按实际伤害潜力排序
    public static int ChooseDefenderIndex(List<BattleCard> own, BattleCard incoming)
    {
        int best = -1;
        float bestScore = float.MinValue;

        // 1) 找到克制卡牌
        for (int i = 0; i < own.Count; i++)
        {
            var c = own[i];
            if (c == null || !c.IsAlive()) continue;
            float mod = BattleCalculator.GetRaceModifier(c.card.race, incoming.card.race);
            if (mod > 1.0f)
            {
                // 评价：攻击力 * modifier + 一点HP权重
                float score = c.GetTotalAttack() * mod + c.currentHP * 0.1f;
                if (score > bestScore)
                {
                    best = i;
                    bestScore = score;
                }
            }
        }

        if (best >= 0) return best;

        // 2) 没有克制卡，尝试找等级>=来袭等级的卡，按攻击优先
        for (int i = 0; i < own.Count; i++)
        {
            var c = own[i];
            if (c == null || !c.IsAlive()) continue;
            if (c.card.level >= incoming.card.level)
            {
                float score = c.GetTotalAttack() + c.currentHP * 0.1f;
                if (score > bestScore)
                {
                    best = i;
                    bestScore = score;
                }
            }
        }

        if (best >= 0) return best;

        // 3) 回退：选择HP最高的存活卡
        best = -1;
        int bestHP = int.MinValue;
        for (int i = 0; i < own.Count; i++)
        {
            var c = own[i];
            if (c == null || !c.IsAlive()) continue;
            if (c.currentHP > bestHP)
            {
                best = i;
                bestHP = c.currentHP;
            }
        }

        return best >= 0 ? best : 0;
    }

    public static int GetCurrentPower(List<BattleCard> cards)
    {
        int power = 0;

        foreach (var c in cards)
        {
            if (c == null) continue;
            power += c.card.GetAttack() * c.currentCount + c.currentHP;
        }

        return power;
    }

    public static int GetInitialPower(List<BattleCard> cards)
    {
        int power = 0;

        foreach (var c in cards)
        {
            if (c == null) continue;
            power += c.card.GetAttack() * c.initialCount + c.initialHP;
        }

        return power;
    }

    public static SurrenderDecision EvaluateSurrender(
        List<BattleCard> own,
        List<BattleCard> opponent,
        int currentRound,
        int minRound,
        float criticalPowerRatio,
        float opponentAdvantageRatio,
        float maxChance)
    {
        var decision = new SurrenderDecision
        {
            currentPower = GetCurrentPower(own),
            initialPower = GetInitialPower(own),
            opponentPower = GetCurrentPower(opponent),
            reason = "继续战斗"
        };

        if (decision.currentPower <= 0 || decision.initialPower <= 0)
        {
            decision.reason = "己方已无可战兵力";
            return decision;
        }

        if (currentRound < minRound)
        {
            decision.reason = $"回合{currentRound}未达到最小投降回合{minRound}";
            return decision;
        }

        float ownRemainingRatio = (float)decision.currentPower / decision.initialPower;
        if (ownRemainingRatio > criticalPowerRatio)
        {
            decision.reason = $"剩余战力比例{ownRemainingRatio:0.00}高于阈值{criticalPowerRatio:0.00}";
            return decision;
        }

        if (decision.opponentPower <= 0)
        {
            decision.reason = "对手已无战力";
            return decision;
        }

        float relativePower = (float)decision.currentPower / decision.opponentPower;
        if (relativePower > opponentAdvantageRatio)
        {
            decision.reason = $"相对对手战力{relativePower:0.00}高于阈值{opponentAdvantageRatio:0.00}";
            return decision;
        }

        float pressure = Mathf.Clamp01(1f - ownRemainingRatio / Mathf.Max(0.01f, criticalPowerRatio));
        float disadvantage = Mathf.Clamp01(1f - relativePower / Mathf.Max(0.01f, opponentAdvantageRatio));
        decision.chance = Mathf.Clamp01(maxChance * (0.35f + pressure * 0.4f + disadvantage * 0.25f));
        decision.canSurrender = true;
        decision.reason = $"剩余战力{ownRemainingRatio:0.00}，相对战力{relativePower:0.00}，投降概率{decision.chance:0.00}";
        return decision;
    }
}
