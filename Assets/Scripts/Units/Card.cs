using System;

/// <summary>
/// 卡牌数据。unitIndex 决定等级，quantity 表示该兵种拥有的数量。
/// baseAttack/baseHP 是单个单位的实际数值。
/// </summary>
[Serializable]
public class Card
{
    public int unitIndex;   // 0-4，决定等级 (Lv = unitIndex + 1)
    public int level;       // = unitIndex + 1
    public int quantity = 1;
    public int currentHP;
    public RaceType race;
    public int baseAttack;
    public int baseHP;
    public string cardName;

    public int GetAttack()
    {
        return baseAttack;
    }

    public int GetMaxHP()
    {
        return baseHP;
    }

    public int GetCombatPower()
    {
        return (baseAttack + baseHP) * quantity;
    }

    public Card Clone()
    {
        return new Card
        {
            unitIndex = unitIndex,
            level = level,
            quantity = quantity,
            currentHP = currentHP,
            race = race,
            baseAttack = baseAttack,
            baseHP = baseHP,
            cardName = cardName
        };
    }
}
