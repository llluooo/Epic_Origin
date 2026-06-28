using System.Collections.Generic;

/// <summary>
/// 简单的战斗出牌决策。
/// </summary>
public static class BattleAI
{
    public static int ChooseAttackerIndex(List<BattleCard> own)
    {
        int best = -1;
        int bestLevel = int.MinValue;
        int bestAttack = int.MinValue;

        for (int i = 0; i < own.Count; i++)
        {
            BattleCard card = own[i];
            if (card == null || !card.IsAlive())
            {
                continue;
            }

            int level = card.card.level;
            int attack = card.GetTotalAttack();
            if (level > bestLevel || (level == bestLevel && attack > bestAttack))
            {
                best = i;
                bestLevel = level;
                bestAttack = attack;
            }
        }

        return best >= 0 ? best : 0;
    }

    public static int ChooseDefenderIndex(List<BattleCard> own, BattleCard incoming)
    {
        int counterIndex = FindBestCounterIndex(own, incoming);
        if (counterIndex >= 0)
        {
            return counterIndex;
        }

        int levelMatchIndex = FindBestLevelMatchIndex(own, incoming);
        if (levelMatchIndex >= 0)
        {
            return levelMatchIndex;
        }

        return FindHighestHpIndex(own);
    }

    private static int FindBestCounterIndex(List<BattleCard> own, BattleCard incoming)
    {
        int best = -1;
        float bestScore = float.MinValue;

        for (int i = 0; i < own.Count; i++)
        {
            BattleCard card = own[i];
            if (card == null || !card.IsAlive())
            {
                continue;
            }

            float modifier = BattleCalculator.GetRaceModifier(card.card.race, incoming.card.race);
            if (modifier <= 1f)
            {
                continue;
            }

            float score = card.GetTotalAttack() * modifier + card.currentHP * 0.1f;
            if (score > bestScore)
            {
                best = i;
                bestScore = score;
            }
        }

        return best;
    }

    private static int FindBestLevelMatchIndex(List<BattleCard> own, BattleCard incoming)
    {
        int best = -1;
        float bestScore = float.MinValue;

        for (int i = 0; i < own.Count; i++)
        {
            BattleCard card = own[i];
            if (card == null || !card.IsAlive() || card.card.level < incoming.card.level)
            {
                continue;
            }

            float score = card.GetTotalAttack() + card.currentHP * 0.1f;
            if (score > bestScore)
            {
                best = i;
                bestScore = score;
            }
        }

        return best;
    }

    private static int FindHighestHpIndex(List<BattleCard> own)
    {
        int best = -1;
        int bestHp = int.MinValue;

        for (int i = 0; i < own.Count; i++)
        {
            BattleCard card = own[i];
            if (card == null || !card.IsAlive())
            {
                continue;
            }

            if (card.currentHP > bestHp)
            {
                best = i;
                bestHp = card.currentHP;
            }
        }

        return best >= 0 ? best : 0;
    }
}
