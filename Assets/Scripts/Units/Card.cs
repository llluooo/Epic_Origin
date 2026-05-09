using System;
using UnityEngine;

/// <summary>
/// 卡牌（包装一个单位实例，存储当前状态）
/// </summary>
[Serializable]
public class Card
{
    public int unitIndex;   // 单位类型索引（0-4，对应种族内5种单位）
    public int level;
    public int currentHP;
    public RaceType race;
    public int baseAttack;
    public int baseHP;
    public string cardName;

    public int GetAttack()
    {
        float mult = GetLevelMultiplier();
        return Mathf.RoundToInt(baseAttack * mult);
    }

    public int GetMaxHP()
    {
        float mult = GetLevelMultiplier();
        return Mathf.RoundToInt(baseHP * mult);
    }

    public int GetCombatPower()
    {
        return GetAttack() + GetMaxHP();
    }

    private float GetLevelMultiplier()
    {
        return level switch
        {
            1 => 1.0f,
            2 => 1.5f,
            3 => 2.5f,
            4 => 4.0f,
            5 => 6.5f,
            _ => 1.0f
        };
    }
}
