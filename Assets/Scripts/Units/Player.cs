using System;
using UnityEngine;

/// <summary>
/// 玩家数据模型（种族、资源、据点等级、卡组）
/// </summary>
[Serializable]
public class Player
{
    public string playerName;
    public RaceType race;
    public ResourceData resources;
    public int strongholdLevel;
    public Deck deck;

    // 据点位置
    public Vector2Int strongholdPos;

    // 升级费用: 80 × level^1.5
    public int GetUpgradeCost()
    {
        return Mathf.RoundToInt(80 * Mathf.Pow(strongholdLevel, 1.5f));
    }

    public bool CanUpgrade()
    {
        return strongholdLevel < 5 && resources.buildingMaterials >= GetUpgradeCost();
    }

    public void UpgradeStronghold()
    {
        if (!CanUpgrade()) return;
        resources.buildingMaterials -= GetUpgradeCost();
        strongholdLevel++;
        Debug.Log($"{playerName} 据点升级到 Lv{strongholdLevel}！");
    }

    // 召唤费用: 基础费用 × 等级系数
    public int GetSummonCost(int cardLevel)
    {
        float levelMult = cardLevel switch
        {
            1 => 1.0f,
            2 => 1.5f,
            3 => 2.5f,
            4 => 4.0f,
            5 => 6.5f,
            _ => 1.0f
        };
        return Mathf.RoundToInt(30 * levelMult);
    }

    public bool CanSummon(int cardLevel)
    {
        return strongholdLevel >= cardLevel && resources.gold >= GetSummonCost(cardLevel);
    }

    public void AddResources(ResourceData amount)
    {
        resources.Add(amount);
        Debug.Log($"{playerName} 获得 {amount.gold}金币, {amount.buildingMaterials}建材");
    }

    // 回合末资源产出
    public ResourceData GetTurnProduction()
    {
        // 基础产出 × 等级系数
        float mult = strongholdLevel switch
        {
            1 => 1.0f,
            2 => 1.3f,
            3 => 1.6f,
            4 => 2.0f,
            5 => 2.5f,
            _ => 1.0f
        };
        return new ResourceData(
            Mathf.RoundToInt(10 * mult),
            Mathf.RoundToInt(5 * mult)
        );
    }
}
