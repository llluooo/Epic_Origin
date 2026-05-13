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

    // ================== 数值表（设计方案精确值）==================

    // 升级消耗: [level] = (金币, 建材) — 1→2用索引1, 2→3用索引2, ...
    private static readonly ResourceData[] BaseUpgradeCosts =
    {
        new ResourceData(0, 0),          // 占位
        new ResourceData(50, 100),       // 1→2
        new ResourceData(150, 300),      // 2→3
        new ResourceData(400, 800),      // 3→4
        new ResourceData(1000, 2000),    // 4→5
    };

    // 召唤消耗: [level] = (金币, 建材)
    private static readonly ResourceData[] BaseSummonCosts =
    {
        new ResourceData(0, 0),          // 占位
        new ResourceData(20, 10),        // Lv1
        new ResourceData(60, 30),        // Lv2
        new ResourceData(150, 80),       // Lv3
        new ResourceData(400, 200),      // Lv4
        new ResourceData(1000, 500),     // Lv5
    };

    // 回合末被动产出: [据点等级] = (金币, 建材)
    private static readonly ResourceData[] BaseProduction =
    {
        new ResourceData(0, 0),          // 占位
        new ResourceData(20, 20),        // Lv1
        new ResourceData(35, 35),        // Lv2
        new ResourceData(55, 55),        // Lv3
        new ResourceData(80, 80),        // Lv4
        new ResourceData(120, 120),      // Lv5
    };

    // ================== 种族成本修正 ==================

    public float GetRaceCostMultiplier()
    {
        return race switch
        {
            RaceType.Human => 1.0f,
            RaceType.Heaven => 1.3f,
            RaceType.Ghost => 0.8f,
            _ => 1.0f
        };
    }

    // ================== 升级 ==================

    public ResourceData GetUpgradeCost()
    {
        if (strongholdLevel >= 5) return new ResourceData(int.MaxValue, int.MaxValue);
        ResourceData baseCost = BaseUpgradeCosts[strongholdLevel];
        float mult = GetRaceCostMultiplier();
        return new ResourceData(
            Mathf.RoundToInt(baseCost.gold * mult),
            Mathf.RoundToInt(baseCost.buildingMaterials * mult)
        );
    }

    public bool CanUpgrade()
    {
        if (strongholdLevel >= 5) return false;
        ResourceData cost = GetUpgradeCost();
        return resources.gold >= cost.gold && resources.buildingMaterials >= cost.buildingMaterials;
    }

    public void UpgradeStronghold()
    {
        if (!CanUpgrade()) return;
        ResourceData cost = GetUpgradeCost();
        resources.gold -= cost.gold;
        resources.buildingMaterials -= cost.buildingMaterials;
        strongholdLevel++;
        Debug.Log($"{playerName} 据点升级到 Lv{strongholdLevel}！");
    }

    // ================== 召唤 ==================

    public ResourceData GetSummonCost(int cardLevel)
    {
        if (cardLevel < 1 || cardLevel > 5) return new ResourceData(int.MaxValue, int.MaxValue);
        ResourceData baseCost = BaseSummonCosts[cardLevel];
        float mult = GetRaceCostMultiplier();
        return new ResourceData(
            Mathf.RoundToInt(baseCost.gold * mult),
            Mathf.RoundToInt(baseCost.buildingMaterials * mult)
        );
    }

    public bool CanSummon(int cardLevel)
    {
        if (cardLevel < 1 || cardLevel > 5) return false;
        if (strongholdLevel < cardLevel) return false;
        ResourceData cost = GetSummonCost(cardLevel);
        return resources.gold >= cost.gold && resources.buildingMaterials >= cost.buildingMaterials;
    }

    // ================== 资源 ==================

    public void AddResources(ResourceData amount)
    {
        resources.Add(amount);
        Debug.Log($"{playerName} 获得 {amount.gold}金币, {amount.buildingMaterials}建材");
    }

    // ================== 回合末资源产出 ==================

    public ResourceData GetTurnProduction()
    {
        if (strongholdLevel < 1 || strongholdLevel > 5)
            return new ResourceData(0, 0);
        return BaseProduction[strongholdLevel];
    }
}
