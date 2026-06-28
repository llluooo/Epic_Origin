using UnityEngine;

/// <summary>
/// 战斗伤害与种族克制计算器。
/// </summary>
public static class BattleCalculator
{
    public static float GetRaceModifier(RaceType attacker, RaceType defender)
    {
        if (attacker == defender)
        {
            return 1f;
        }

        if (attacker == RaceType.Human && defender == RaceType.Heaven)
        {
            return 1.2f;
        }

        if (attacker == RaceType.Heaven && defender == RaceType.Ghost)
        {
            return 1.2f;
        }

        if (attacker == RaceType.Ghost && defender == RaceType.Human)
        {
            return 1.2f;
        }

        return 0.8f;
    }

    public static int CalculateAttackDamage(BattleCard attacker, BattleCard defender)
    {
        float attackValue = attacker.GetTotalAttack();
        float attackModifier = GetRaceModifier(attacker.card.race, defender.card.race);
        return Mathf.FloorToInt(attackValue * attackModifier);
    }
}
