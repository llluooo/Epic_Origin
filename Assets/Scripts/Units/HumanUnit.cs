/// <summary>
/// 人族单位定义
/// </summary>
public class HumanUnit : Unit
{
    // 5种人族单位: 剑士、弓手、骑士、牧师、枪兵
    private static readonly (string name, int atk, int hp)[] UnitDefs =
    {
        ("剑士",   8, 12),
        ("弓手",  10,  8),
        ("骑士",   7, 14),
        ("牧师",   4, 10),
        ("枪兵",   9,  9),
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
