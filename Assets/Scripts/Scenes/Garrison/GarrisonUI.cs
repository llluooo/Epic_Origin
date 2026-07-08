using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 据点兵力管理场景UI控制器。
/// 管理英雄6卡槽展示、驻军列表展示、兵力转移、返回主场景。
/// </summary>
public class GarrisonUI : MonoBehaviour
{
    [Header("英雄区")]
    public Transform heroSlotsContainer;
    public GameObject heroSlotPrefab;
    public TMP_Text heroSlotCountText;

    [Header("驻军区")]
    public Transform garrisonListContainer;
    public GameObject garrisonEntryPrefab;
    public TMP_Text garrisonCountText;

    [Header("卡牌图鉴")]
    public CardSpriteConfig cardSpriteConfig;

    [Header("转移按钮")]
    public Button transferToGarrisonBtn;
    public Button transferToHeroBtn;

    [Header("返回")]
    public Button returnBtn;

    [Header("消息")]
    public TMP_Text messageText;

    [Header("种族背景")]
    public Image backgroundImage;
    public Sprite humanTrainingBg;
    public Sprite heavenTrainingBg;
    public Sprite ghostTrainingBg;

    private Deck heroDeck;
    private Deck garrisonDeck;

    private GarrisonCardEntry selectedHeroEntry;
    private GarrisonCardEntry selectedGarrisonEntry;

    private List<GarrisonCardEntry> heroEntries = new List<GarrisonCardEntry>();
    private List<GarrisonCardEntry> garrisonEntries = new List<GarrisonCardEntry>();

    private float messageTimer;

    void Start()
    {
        Debug.Log("[GarrisonUI] Start 开始，检查 GameSession 数据...");

        // 检查 Inspector 引用
        if (heroSlotPrefab == null)
            Debug.LogError("[GarrisonUI] heroSlotPrefab 未在 Inspector 中赋值！");
        if (heroSlotsContainer == null)
            Debug.LogError("[GarrisonUI] heroSlotsContainer 未在 Inspector 中赋值！");
        if (garrisonEntryPrefab == null)
            Debug.LogError("[GarrisonUI] garrisonEntryPrefab 未在 Inspector 中赋值！");
        if (garrisonListContainer == null)
            Debug.LogError("[GarrisonUI] garrisonListContainer 未在 Inspector 中赋值！");

        // 从 GameSession 读取兵力数据
        heroDeck = GameSession.GetGarrisonHeroDeck();
        garrisonDeck = GameSession.GetGarrisonGarrisonDeck();

        Debug.Log($"[GarrisonUI] heroDeck={(heroDeck != null ? heroDeck.CardCount + "张" : "null")}, garrisonDeck={(garrisonDeck != null ? garrisonDeck.CardCount + "张" : "null")}");

        if (heroDeck == null || garrisonDeck == null)
        {
            Debug.LogError("[GarrisonUI] GameSession 兵力数据为空，返回主场景。");
            GarrisonSceneBridge.ReturnToMainScene(new Deck(), new Deck());
            return;
        }

        transferToGarrisonBtn.onClick.AddListener(OnTransferToGarrison);
        transferToHeroBtn.onClick.AddListener(OnTransferToHero);
        returnBtn.onClick.AddListener(OnReturn);

        RaceType playerRace = GameSession.GetGarrisonPlayerRace();
        SetBackgroundForRace(playerRace);

        RebuildHeroSlots();
        RebuildGarrisonList();
        UpdateButtonStates();

        Debug.Log($"[GarrisonUI] Start 完成，heroEntries={heroEntries.Count}, garrisonEntries={garrisonEntries.Count}");
    }

    void Update()
    {
        if (messageTimer > 0)
        {
            messageTimer -= Time.deltaTime;
            if (messageTimer <= 0 && messageText != null)
                messageText.text = "";
        }
    }

    void RebuildHeroSlots()
    {
        if (heroSlotsContainer == null || heroSlotPrefab == null)
        {
            Debug.LogError("[GarrisonUI] RebuildHeroSlots 跳过：container 或 prefab 为空");
            return;
        }

        ClearContainer(heroSlotsContainer);
        heroEntries.Clear();

        int slotCount = Mathf.Max(Deck.HeroSlotLimit, heroDeck.CardCount);
        Debug.Log($"[GarrisonUI] 创建 {slotCount} 个英雄卡槽 (deck有{heroDeck.CardCount}张)");

        for (int i = 0; i < slotCount; i++)
        {
            GameObject go = Instantiate(heroSlotPrefab, heroSlotsContainer);
            if (go == null)
            {
                Debug.LogError($"[GarrisonUI] Instantiate(heroSlotPrefab) 返回 null，卡槽索引={i}");
                continue;
            }

            GarrisonCardEntry entry = go.GetComponent<GarrisonCardEntry>();
            if (entry == null)
            {
                Debug.LogWarning($"[GarrisonUI] heroSlotPrefab 缺少 GarrisonCardEntry 组件，卡槽索引={i}");
                continue;
            }

            Card card = i < heroDeck.CardCount ? heroDeck[i] : null;
            Sprite sprite = card != null && cardSpriteConfig != null ? cardSpriteConfig.GetSprite(card.race, card.unitIndex) : null;
            entry.Setup(card, i, true, sprite);
            entry.onClicked = OnHeroSlotClicked;
            heroEntries.Add(entry);
        }

        if (heroSlotCountText != null)
            heroSlotCountText.text = $"英雄兵力 ({heroDeck.CardCount}/{Deck.HeroSlotLimit})";
    }

    void RebuildGarrisonList()
    {
        if (garrisonListContainer == null || garrisonEntryPrefab == null)
        {
            Debug.LogError("[GarrisonUI] RebuildGarrisonList 跳过：container 或 prefab 为空");
            return;
        }

        ClearContainer(garrisonListContainer);
        garrisonEntries.Clear();

        Debug.Log($"[GarrisonUI] 创建 {garrisonDeck.CardCount} 个驻军入口");

        for (int i = 0; i < garrisonDeck.CardCount; i++)
        {
            GameObject go = Instantiate(garrisonEntryPrefab, garrisonListContainer);
            if (go == null)
            {
                Debug.LogError($"[GarrisonUI] Instantiate(garrisonEntryPrefab) 返回 null，索引={i}");
                continue;
            }

            GarrisonCardEntry entry = go.GetComponent<GarrisonCardEntry>();
            if (entry == null)
            {
                Debug.LogWarning($"[GarrisonUI] garrisonEntryPrefab 缺少 GarrisonCardEntry 组件，索引={i}");
                continue;
            }

            Card card = garrisonDeck[i];
            Sprite sprite = cardSpriteConfig != null ? cardSpriteConfig.GetSprite(card.race, card.unitIndex) : null;
            entry.Setup(card, i, false, sprite);
            entry.onClicked = OnGarrisonEntryClicked;
            garrisonEntries.Add(entry);
        }

        if (garrisonCountText != null)
            garrisonCountText.text = $"据点驻军 ({garrisonDeck.CardCount})";
    }

    void ClearContainer(Transform container)
    {
        if (container == null) return;

        for (int i = container.childCount - 1; i >= 0; i--)
            Destroy(container.GetChild(i).gameObject);
    }

    void ClearSelections()
    {
        if (selectedHeroEntry != null)
        {
            selectedHeroEntry.SetHighlighted(false);
            selectedHeroEntry = null;
        }
        if (selectedGarrisonEntry != null)
        {
            selectedGarrisonEntry.SetHighlighted(false);
            selectedGarrisonEntry = null;
        }
    }

    void OnHeroSlotClicked(GarrisonCardEntry entry)
    {
        if (entry.cardData == null) return;

        if (selectedHeroEntry == entry)
        {
            entry.SetHighlighted(false);
            selectedHeroEntry = null;
        }
        else
        {
            if (selectedHeroEntry != null)
                selectedHeroEntry.SetHighlighted(false);
            if (selectedGarrisonEntry != null)
            {
                selectedGarrisonEntry.SetHighlighted(false);
                selectedGarrisonEntry = null;
            }

            entry.SetHighlighted(true);
            selectedHeroEntry = entry;
        }
        UpdateButtonStates();
    }

    void OnGarrisonEntryClicked(GarrisonCardEntry entry)
    {
        if (selectedGarrisonEntry == entry)
        {
            entry.SetHighlighted(false);
            selectedGarrisonEntry = null;
        }
        else
        {
            if (selectedGarrisonEntry != null)
                selectedGarrisonEntry.SetHighlighted(false);
            if (selectedHeroEntry != null)
            {
                selectedHeroEntry.SetHighlighted(false);
                selectedHeroEntry = null;
            }

            entry.SetHighlighted(true);
            selectedGarrisonEntry = entry;
        }
        UpdateButtonStates();
    }

    void OnTransferToGarrison()
    {
        if (selectedHeroEntry == null)
        {
            ShowMessage("请先在英雄兵力中选择一个兵种。");
            return;
        }

        int index = selectedHeroEntry.cardIndex;
        if (index < 0 || index >= heroDeck.CardCount) return;

        Card card = heroDeck[index];
        garrisonDeck.AddCard(card);
        heroDeck.RemoveCard(card);

        Debug.Log($"[Garrison] {card.cardName} Lv{card.level} 移入据点驻军");
        ShowMessage($"{card.cardName} 已移入据点");

        ClearSelections();
        RebuildHeroSlots();
        RebuildGarrisonList();
        UpdateButtonStates();
    }

    void OnTransferToHero()
    {
        if (selectedGarrisonEntry == null)
        {
            ShowMessage("请先在据点驻军中选择一个兵种。");
            return;
        }

        int index = selectedGarrisonEntry.cardIndex;
        if (index < 0 || index >= garrisonDeck.CardCount) return;

        Card card = garrisonDeck[index];
        if (!heroDeck.CanAddCard(card))
        {
            ShowMessage("英雄兵力已满！请先移出一些兵种。");
            return;
        }

        heroDeck.AddCard(card);
        garrisonDeck.RemoveCard(card);

        Debug.Log($"[Garrison] {card.cardName} Lv{card.level} 移入英雄携带");
        ShowMessage($"{card.cardName} 已带回英雄");

        ClearSelections();
        RebuildHeroSlots();
        RebuildGarrisonList();
        UpdateButtonStates();
    }

    void UpdateButtonStates()
    {
        if (transferToGarrisonBtn != null)
            transferToGarrisonBtn.interactable = selectedHeroEntry != null;
        if (transferToHeroBtn != null)
            transferToHeroBtn.interactable = selectedGarrisonEntry != null && heroDeck.CardCount < Deck.HeroSlotLimit;
    }

    void OnReturn()
    {
        GarrisonSceneBridge.ReturnToMainScene(heroDeck, garrisonDeck);
    }

    void ShowMessage(string msg)
    {
        if (messageText != null)
            messageText.text = msg;
        messageTimer = 3f;
    }

    void SetBackgroundForRace(RaceType race)
    {
        if (backgroundImage == null) return;

        Sprite bg = null;
        switch (race)
        {
            case RaceType.Human:
                bg = humanTrainingBg;
                break;
            case RaceType.Heaven:
                bg = heavenTrainingBg;
                break;
            case RaceType.Ghost:
                bg = ghostTrainingBg;
                break;
        }

        if (bg != null)
        {
            backgroundImage.sprite = bg;
            Debug.Log($"[GarrisonUI] 设置种族背景：{race}");
        }
    }
}
