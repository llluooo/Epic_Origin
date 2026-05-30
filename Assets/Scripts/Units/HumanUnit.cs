/// <summary>
/// 人族单位定义 — Lv1=剑士, Lv2=重装步兵, Lv3=巫师, Lv4=骑士, Lv5=皇家近卫
/// </summary>
public class HumanUnit : Unit
{
    private static readonly (string name, int atk, int hp)[] UnitDefs =
    {
        ("剑士",     4,  5),
        ("重装步兵",  5,  8),
        ("巫师",    10,  7),
        ("骑士",    12, 12),
        ("皇家近卫", 20, 25),
    };

    public HumanUnit(int unitIndex)
    {
        var def = UnitDefs[unitIndex];
        unitName = def.name;
        baseAttack = def.atk;
        baseHP = def.hp;
        level = unitIndex + 1;
        race = RaceType.Human;
    }

    public static Card CreateCard(int unitIndex)
    {
        var def = UnitDefs[unitIndex];
        var card = new Card
        {
            unitIndex = unitIndex,
            level = unitIndex + 1,
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
