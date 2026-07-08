using System;
using System.Collections.Generic;

/// <summary>
/// 卡组
/// </summary>
[Serializable]
public class Deck
{
    public List<Card> cards = new List<Card>();

    public void AddCard(Card card)
    {
        cards.Add(card);
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

    public Card this[int index] => cards[index];
}
