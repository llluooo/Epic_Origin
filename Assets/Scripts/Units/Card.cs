using System;
using UnityEngine;

/// <summary>
/// 卡牌（包装一个单位实例，存储当前状态）
/// level 由 unitIndex 决定: Lv = unitIndex + 1
/// 不做等级倍率缩放 — baseAttack/baseHP 即为该兵种在该等级的实际数值
/// </summary>
[Serializable]
public class Card
{
    public int unitIndex;   // 0-4，决定等级 (Lv = unitIndex + 1)
    public int level;       // = unitIndex + 1
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
        return baseAttack + baseHP;
    }
}
