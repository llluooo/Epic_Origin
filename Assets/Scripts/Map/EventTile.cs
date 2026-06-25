using System.Collections.Generic;
using UnityEngine;

public class EventTile : Tile
{
    public override void OnHeroEnter()
    {
        int roll = Random.Range(0, 6);
        switch (roll)
        {
            case 0: AncientTreasure(); break;
            case 1: WanderingMerchant(); break;
            case 2: HeroBlessing(); break;
            case 3: BanditRaid(); break;
            case 4: Sandstorm(); break;
            case 5: Plague(); break;
        }
    }

    // 好事件: 古代宝藏 — 获得200金币
    void AncientTreasure()
    {
        Player p = GameManager.Instance.player;
        p.resources.gold += 200;
        Debug.Log("触发事件【古代宝藏】: 获得 200 金币！");
    }

    // 好事件: 流浪商人 — 获得150建材
    void WanderingMerchant()
    {
        Player p = GameManager.Instance.player;
        p.resources.buildingMaterials += 150;
        Debug.Log("触发事件【流浪商人】: 获得 150 建材！");
    }

    // 好事件: 英雄遇仙 — 获得2张Lv1本种族基础卡牌
    void HeroBlessing()
    {
        Player p = GameManager.Instance.player;
        for (int i = 0; i < 2; i++)
        {
            Card card = CreateCard(p.race, 0);  // 兵种索引 0 对应 Lv1
            p.deck.AddCard(card);
            Debug.Log($"触发事件【英雄遇仙】: 获得 {card.cardName} Lv1！");
        }
    }

    // 坏事件: 强盗袭击 — 损失100金币
    void BanditRaid()
    {
        Player p = GameManager.Instance.player;
        int lose = Mathf.Min(100, p.resources.gold);
        p.resources.gold -= lose;
        Debug.Log($"触发事件【强盗袭击】: 损失 {lose} 金币！");
    }

    // 坏事件: 沙尘暴 — 本回合消耗主操作（不能再行动）
    void Sandstorm()
    {
        GameManager.Instance.OnPlayerAction();
        Debug.Log("触发事件【沙尘暴】: 本回合无法再进行主操作！");
    }

    // 坏事件: 瘟疫 — 每种兵种卡牌 -1（最少保留1张）
    void Plague()
    {
        Player p = GameManager.Instance.player;
        Deck deck = p.deck;

        // 按兵种索引分组
        var groups = new Dictionary<int, List<Card>>();
        for (int i = deck.CardCount - 1; i >= 0; i--)
        {
            Card c = deck[i];
            if (!groups.ContainsKey(c.unitIndex))
                groups[c.unitIndex] = new List<Card>();
            groups[c.unitIndex].Add(c);
        }

        int removed = 0;
        foreach (var kv in groups)
        {
            if (kv.Value.Count <= 1) continue;

            deck.cards.Remove(kv.Value[0]);
            removed++;
        }

        Debug.Log(removed > 0
            ? $"触发事件【瘟疫】: 损失 {removed} 张卡牌（每种至少保留1张）！"
            : "触发事件【瘟疫】: 每种兵种仅剩1张，无法再减少！");
    }

    Card CreateCard(RaceType race, int unitIndex)
    {
        return race switch
        {
            RaceType.Human => HumanUnit.CreateCard(unitIndex),
            RaceType.Heaven => HeavenUnit.CreateCard(unitIndex),
            RaceType.Ghost => GhostUnit.CreateCard(unitIndex),
            _ => HumanUnit.CreateCard(0),
        };
    }
}
