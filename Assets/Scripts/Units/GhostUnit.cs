/// <summary>
/// 鬼族单位定义 — Lv1=骷髅, Lv2=僵尸, Lv3=鬼火, Lv4=死亡骑士, Lv5=死神
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

    public GhostUnit(int unitIndex)
    {
        var def = UnitDefs[unitIndex];
        unitName = def.name;
        baseAttack = def.atk;
        baseHP = def.hp;
        level = unitIndex + 1;
        race = RaceType.Ghost;
    }

    public static Card CreateCard(int unitIndex)
    {
        var def = UnitDefs[unitIndex];
        var card = new Card
        {
            unitIndex = unitIndex,
            level = unitIndex + 1,
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
