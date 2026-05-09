using System;

/// <summary>
/// 种族类型
/// </summary>
public enum RaceType
{
    Human,   // 人族
    Heaven,  // 天族
    Ghost    // 鬼族
}

/// <summary>
/// 单位抽象基类（数据模型，非 MonoBehaviour）
/// </summary>
[Serializable]
public abstract class Unit
{
    public string unitName;
    public int baseAttack;
    public int baseHP;
    public int level;
    public RaceType race;

    public int GetAttack()
    {
        return UnityEngine.Mathf.RoundToInt(baseAttack * GetLevelMultiplier());
    }

    public int GetHP()
    {
        return UnityEngine.Mathf.RoundToInt(baseHP * GetLevelMultiplier());
    }

    public float GetLevelMultiplier()
    {
        // Lv1=1.0, Lv2=1.5, Lv3=2.5, Lv4=4.0, Lv5=6.5
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
