using System;

/// <summary>
/// 卡牌数据。兵种索引决定等级，数量表示该兵种拥有的数量。
/// 基础攻击和基础生命是单个单位的实际数值。
/// </summary>
[Serializable]
public class Card
{
    public int unitIndex;   // 0-4，决定等级（Lv = unitIndex + 1）
    public int level;       // 等于 unitIndex + 1
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
