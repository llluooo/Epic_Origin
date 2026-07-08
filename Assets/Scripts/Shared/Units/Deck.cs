using System;
using System.Collections.Generic;

/// <summary>
/// 卡组。支持可选的卡槽上限，同一种兵种（同race+同unitIndex）堆叠 quantity 共享一槽。
/// </summary>
[Serializable]
public class Deck
{
    public const int HeroSlotLimit = 6;

    public List<Card> cards = new List<Card>();
    public bool hasSlotLimit = false;

    /// <summary>
    /// 检查是否可以添加（同种堆叠时不算新槽）
    /// </summary>
    public bool CanAddCard(Card card)
    {
        if (card == null) return false;
        if (!hasSlotLimit) return true;

        int existingIndex = FindSlotIndex(card.race, card.unitIndex);
        if (existingIndex >= 0) return true; // 同种堆叠，不占新槽
        return cards.Count < HeroSlotLimit;
    }

    /// <summary>
    /// 添加卡牌。同种兵种堆叠 quantity；启用上限时新槽需有空位。
    /// </summary>
    public void AddCard(Card card)
    {
        if (card == null) return;

        int existingIndex = FindSlotIndex(card.race, card.unitIndex);
        if (existingIndex >= 0)
        {
            cards[existingIndex].quantity += card.quantity;
            return;
        }

        if (hasSlotLimit && cards.Count >= HeroSlotLimit)
        {
            return; // 满槽，丢弃（调用方应提前 CanAddCard 判断）
        }

        cards.Add(card);
    }

    /// <summary>
    /// 查找同种兵种的槽位索引，-1 表示不存在
    /// </summary>
    public int FindSlotIndex(RaceType race, int unitIndex)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i].race == race && cards[i].unitIndex == unitIndex)
                return i;
        }
        return -1;
    }

    public void RemoveCard(Card card)
    {
        cards.Remove(card);
    }

    public int GetTotalCombatPower()
    {
        int total = 0;
        foreach (var card in cards)
            total += card.GetCombatPower();
        return total;
    }

    public int CardCount => cards.Count;

    /// <summary>
    /// 当前已用槽位数
    /// </summary>
    public int SlotCount => cards.Count;

    public Card this[int index] => cards[index];
}
