using System;

/// <summary>
/// 资源数据结构（金币+建材）
/// </summary>
[Serializable]
public class ResourceData
{
    public int gold;
    public int buildingMaterials;

    public ResourceData(int gold = 0, int buildingMaterials = 0)
    {
        this.gold = gold;
        this.buildingMaterials = buildingMaterials;
    }

    public void Add(ResourceData other)
    {
        gold += other.gold;
        buildingMaterials += other.buildingMaterials;
    }

    public void Subtract(ResourceData other)
    {
        gold -= other.gold;
        buildingMaterials -= other.buildingMaterials;
    }

    public bool CanAfford(ResourceData cost)
    {
        return gold >= cost.gold && buildingMaterials >= cost.buildingMaterials;
    }
}
