using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 英雄状态查看面板。
/// 以驻军风格的动态列表展示英雄携带的卡牌。
/// UIManager统一管理Tab/Esc输入，面板自身不再检测键盘开关。
/// </summary>
public class HeroStatusUI : MonoBehaviour
{
    [Header("面板")]
    public GameObject panel;

    [Header("卡牌列表")]
    public Transform cardsContainer;
    public GameObject entryPrefab;
    public CardSpriteConfig cardSpriteConfig;
    public TMP_Text cardCountText;

    [Header("英雄信息")]
    public TMP_Text goldText;
    public TMP_Text materialsText;
    public TMP_Text strongholdLevelText;
    public TMP_Text deckPowerText;

    [Header("地图输入")]
    public InputManager inputManager;

    public bool IsOpen { get; private set; }

    /// <summary>
    /// 标记是否从游戏菜单打开，用于Esc返回逻辑。
    /// </summary>
    public bool openedFromGameMenu;

    void Start()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    public void Open()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null || gm.player == null) return;

        if (panel != null)
            panel.SetActive(true);
        if (inputManager != null)
            inputManager.enabled = false;

        IsOpen = true;
        RefreshAll(gm.player);
        Debug.Log("打开英雄状态界面");
    }

    public void Close()
    {
        if (panel != null)
            panel.SetActive(false);
        if (inputManager != null)
            inputManager.enabled = true;

        ClearCardEntries();
        IsOpen = false;
        openedFromGameMenu = false;
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
        ClearCardEntries();

        if (cardsContainer == null || entryPrefab == null) return;

        Deck deck = p.deck;
        int cardCount = deck.CardCount;

        for (int i = 0; i < cardCount; i++)
        {
            Card card = deck[i];
            GameObject go = Instantiate(entryPrefab, cardsContainer);
            GarrisonCardEntry entry = go.GetComponent<GarrisonCardEntry>();
            if (entry == null) continue;

            Sprite sprite = cardSpriteConfig != null
                ? cardSpriteConfig.GetSprite(card.race, card.unitIndex)
                : null;

            entry.Setup(card, i, false, sprite);

            // 只读模式：禁用按钮交互
            if (entry.button != null)
                entry.button.interactable = false;
        }

        if (cardCountText != null)
            cardCountText.text = $"英雄兵力 ({cardCount}/{Deck.HeroSlotLimit})";
    }

    void ClearCardEntries()
    {
        if (cardsContainer == null) return;

        for (int i = cardsContainer.childCount - 1; i >= 0; i--)
            Destroy(cardsContainer.GetChild(i).gameObject);
    }
}
