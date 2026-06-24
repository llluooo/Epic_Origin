using System;

/// <summary>
/// 战斗中的卡牌包装类，保存整队兵力和累计生命值。
/// </summary>
public class BattleCard
{
    public Card card;
    public int currentCount;
    public int initialCount;
    public int currentHP;
    public int initialHP;
    public bool hasActed;

    public BattleCard(Card source, int count = 1)
    {
        card = source;
        initialCount = Math.Max(1, count);
        currentCount = initialCount;
        initialHP = card.GetMaxHP() * initialCount;
        currentHP = initialHP;
        hasActed = false;
    }

    public bool IsAlive()
    {
        return currentCount > 0;
    }

    public void ResetRound()
    {
        hasActed = false;
    }

    public int GetTotalAttack()
    {
        return card.GetAttack() * currentCount;
    }

    public int GetTotalHP()
    {
        return currentHP;
    }

    public int GetMaxTotalHP()
    {
        return initialHP;
    }

    public void AddUnits(int count)
    {
        int addedCount = Math.Max(0, count);
        int addedHP = card.GetMaxHP() * addedCount;

        initialCount += addedCount;
        currentCount += addedCount;
        initialHP += addedHP;
        currentHP += addedHP;
    }

    public int TakeDamage(int damage)
    {
        if (damage <= 0 || !IsAlive())
            return 0;

        int oldCount = currentCount;
        int singleHP = Math.Max(1, card.GetMaxHP());

        currentHP = Math.Max(0, currentHP - damage);
        currentCount = currentHP == 0 ? 0 : (currentHP + singleHP - 1) / singleHP;

        return oldCount - currentCount;
    }

    public override string ToString()
    {
        return $"{card.cardName}({card.race} Lv{card.level}) 数量:{currentCount}/{initialCount} HP:{currentHP}/{initialHP} ATK:{card.GetAttack()}";
    }
}
