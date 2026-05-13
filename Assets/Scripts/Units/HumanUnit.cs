/// <summary>
/// 人族单位定义 — Lv1=剑士, Lv2=重装步兵, Lv3=巫师, Lv4=骑士, Lv5=皇家近卫
/// </summary>
public class HumanUnit : Unit
{
    private static readonly (string name, int atk, int hp)[] UnitDefs =
    {
        ("剑士",   8, 12),
        ("弓手",  10,  8),
        ("骑士",   7, 14),
        ("牧师",   4, 10),
        ("枪兵",   9,  9),
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
