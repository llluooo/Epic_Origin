using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 英雄状态查看面板，按Tab切换。
/// 显示卡组中5种兵种的名称、数量及卡牌图片。
/// </summary>
public class HeroStatusUI : MonoBehaviour
{
    [Header("面板")]
    public GameObject panel;

    [Header("兵种卡牌信息（5个，按Lv1-Lv5排列）")]
    public Image[] cardImages;
    public TMP_Text[] cardNameTexts;
    public TMP_Text[] cardQuantityTexts;

    [Header("卡牌美术（每种族5张）")]
    public Sprite[] humanCardSprites;
    public Sprite[] heavenCardSprites;
    public Sprite[] ghostCardSprites;

    [Header("英雄信息")]
    public TMP_Text goldText;
    public TMP_Text materialsText;
    public TMP_Text strongholdLevelText;
    public TMP_Text deckPowerText;

    [Header("地图输入")]
    public InputManager inputManager;

    private bool isOpen = false;

    void Start()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    void Update()
    {
        if (!isOpen) return;

        if (Input.GetKeyDown(KeyCode.Tab))
            Close();
    }

    public void Toggle()
    {
        if (isOpen)
            Close();
        else
            Open();
    }

    public void Open()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null || gm.player == null) return;

        if (panel != null)
            panel.SetActive(true);
        if (inputManager != null)
            inputManager.enabled = false;

        isOpen = true;
        RefreshAll(gm.player);
        Debug.Log("打开英雄状态界面");
    }

    public void Close()
    {
        if (panel != null)
            panel.SetActive(false);
        if (inputManager != null)
            inputManager.enabled = true;

        isOpen = false;
        Debug.Log("关闭英雄状态界面");
    }

    void RefreshAll(Player p)
    {
        RefreshResourceDisplay(p);
        RefreshCardDisplay(p);
    }

    void RefreshResourceDisplay(Player p)
    {
        if (goldText != null)
            goldText.text = $"金币: {p.resources.gold}";
        if (materialsText != null)
            materialsText.text = $"建材: {p.resources.buildingMaterials}";
        if (strongholdLevelText != null)
            strongholdLevelText.text = $"据点 Lv{p.strongholdLevel}";
        if (deckPowerText != null)
            deckPowerText.text = $"总战力: {p.deck.GetTotalCombatPower()}";
    }

    void RefreshCardDisplay(Player p)
    {
        Sprite[] sprites = GetCardSpritesForRace(p.race);

        for (int i = 0; i < 5; i++)
        {
            int count = p.GetOwnedCount(i);
            string cardName = GetCardNameFromDeck(p, i);

            if (cardNameTexts != null && i < cardNameTexts.Length && cardNameTexts[i] != null)
                cardNameTexts[i].text = count > 0 ? cardName : GetUnitNameForRace(p.race, i);

            if (cardQuantityTexts != null && i < cardQuantityTexts.Length && cardQuantityTexts[i] != null)
            {
                cardQuantityTexts[i].text = $"×{count}";
                cardQuantityTexts[i].color = count > 0 ? Color.white : Color.gray;
            }

            if (cardImages != null && i < cardImages.Length && cardImages[i] != null)
            {
                cardImages[i].sprite = (sprites != null && i < sprites.Length) ? sprites[i] : null;
                cardImages[i].color = count > 0 ? Color.white : new Color(0.3f, 0.3f, 0.3f, 1f);
            }
        }
    }

    string GetCardNameFromDeck(Player p, int unitIndex)
    {
        for (int i = 0; i < p.deck.CardCount; i++)
        {
            Card card = p.deck[i];
            if (card.unitIndex == unitIndex && !string.IsNullOrEmpty(card.cardName))
                return card.cardName;
        }
        return "";
    }

    string GetUnitNameForRace(RaceType race, int unitIndex)
    {
        string[][] names = {
            new[] { "剑士", "重装步兵", "巫师", "骑士", "皇家守卫" },
            new[] { "天族士兵", "天空法师", "独角兽", "巨人", "大天使" },
            new[] { "骷髅兵", "僵尸", "鬼火", "死亡骑士", "死神" }
        };
        int raceIdx = race == RaceType.Heaven ? 1 : (race == RaceType.Ghost ? 2 : 0);
        int unitIdx = Mathf.Clamp(unitIndex, 0, 4);
        return names[raceIdx][unitIdx];
    }

    Sprite[] GetCardSpritesForRace(RaceType race)
    {
        return race switch
        {
            RaceType.Heaven => heavenCardSprites,
            RaceType.Ghost => ghostCardSprites,
            _ => humanCardSprites
        };
    }
}
