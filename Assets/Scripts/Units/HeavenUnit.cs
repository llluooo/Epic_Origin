/// <summary>
/// 天族单位定义：Lv1=天兵, Lv2=天空法师, Lv3=独角兽, Lv4=巨人, Lv5=大天使
/// </summary>
public class HeavenUnit : Unit
{
    private static readonly (string name, int atk, int hp)[] UnitDefs =
    {
        ("天兵", 5, 6),
        ("天空法师", 9, 8),
        ("独角兽", 10, 16),
        ("巨人", 8, 35),
        ("大天使", 25, 28),
    };

    public HeavenUnit(int unitIndex)
    {
        var def = UnitDefs[unitIndex];
        unitName = def.name;
        baseAttack = def.atk;
        baseHP = def.hp;
        level = unitIndex + 1;
        race = RaceType.Heaven;
    }

    public static Card CreateCard(int unitIndex)
    {
        var def = UnitDefs[unitIndex];
        var card = new Card
        {
            unitIndex = unitIndex,
            level = unitIndex + 1,
            race = RaceType.Heaven,
            cardName = def.name,
            baseAttack = def.atk,
            baseHP = def.hp,
        };
        card.currentHP = card.GetMaxHP();
        return card;
    }

    public static int UnitCount => UnitDefs.Length;
}
