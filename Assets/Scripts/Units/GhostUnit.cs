/// <summary>
/// 鬼族单位定义
/// </summary>
public class GhostUnit : Unit
{
    private static readonly (string name, int atk, int hp)[] UnitDefs =
    {
        ("幽灵",   7, 11),
        ("骷髅",  10,  8),
        ("亡灵",   8, 13),
        ("鬼火",  11,  6),
        ("僵尸",   6, 14),
    };

    public GhostUnit(int index, int level)
    {
        var def = UnitDefs[index];
        unitName = def.name;
        baseAttack = def.atk;
        baseHP = def.hp;
        this.level = level;
        race = RaceType.Ghost;
    }

    public static Card CreateCard(int unitIndex, int level)
    {
        var def = UnitDefs[unitIndex];
        var card = new Card
        {
            unitIndex = unitIndex,
            level = level,
            race = RaceType.Ghost,
            cardName = def.name,
            baseAttack = def.atk,
            baseHP = def.hp,
        };
        card.currentHP = card.GetMaxHP();
        return card;
    }

    public static int UnitCount => UnitDefs.Length;
}
