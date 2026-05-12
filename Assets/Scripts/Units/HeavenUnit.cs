/// <summary>
/// 天族单位定义
/// </summary>
public class HeavenUnit : Unit
{
    private static readonly (string name, int atk, int hp)[] UnitDefs =
    {
        ("天兵",     5,  6),
        ("天穹法师",  9,  8),
        ("独角兽",   10, 16),
        ("巨人",      8, 35),
        ("大天使",   25, 28),
    };

    public HeavenUnit(int index, int level)
    {
        var def = UnitDefs[index];
        unitName = def.name;
        baseAttack = def.atk;
        baseHP = def.hp;
        this.level = level;
        race = RaceType.Heaven;
    }

    public static Card CreateCard(int unitIndex, int level)
    {
        var def = UnitDefs[unitIndex];
        var card = new Card
        {
            unitIndex = unitIndex,
            level = level,
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
