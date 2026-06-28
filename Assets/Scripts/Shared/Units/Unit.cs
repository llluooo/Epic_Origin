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
/// 每个种族 5 级对应 5 种兵种，兵种索引 0 对应 Lv1，索引 1 对应 Lv2，以此类推。
/// </summary>
[Serializable]
public abstract class Unit
{
    public string unitName;
    public int baseAttack;
    public int baseHP;
    public int level;
    public RaceType race;
}
