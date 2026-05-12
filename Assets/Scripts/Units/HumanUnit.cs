/// <summary>
/// 人族单位定义
/// </summary>
public class HumanUnit : Unit
{
    // 5种人族单位: 剑士、重装步兵、巫师、骑士、皇家近卫
    private static readonly (string name, int atk, int hp)[] UnitDefs =
    {
        ("剑士",     4,  5),
        ("重装步兵",  5,  8),
        ("巫师",    10,  7),
        ("骑士",    12, 12),
        ("皇家近卫", 20, 25),
    };

    public HumanUnit(int index, int level)
    {
        var def = UnitDefs[index];
        unitName = def.name;
        baseAttack = def.atk;
        baseHP = def.hp;
        this.level = level;
        race = RaceType.Human;
    }

    public static Card CreateCard(int unitIndex, int level)
    {
        var def = UnitDefs[unitIndex];
        var card = new Card
        {
            unitIndex = unitIndex,
            level = level,
            race = RaceType.Human,
            cardName = def.name,
            baseAttack = def.atk,
            baseHP = def.hp,
        };
        card.currentHP = card.GetMaxHP();
        return card;
    }

    public static int UnitCount => UnitDefs.Length;
}
