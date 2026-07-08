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

    [Header("转移按钮")]
    public Button transferToGarrisonBtn;
    public Button transferToHeroBtn;

    [Header("返回")]
    public Button returnBtn;

    [Header("消息")]
    public TMP_Text messageText;

    private Deck heroDeck;
    private Deck garrisonDeck;

    private GarrisonCardEntry selectedHeroEntry;
    private GarrisonCardEntry selectedGarrisonEntry;

    private List<GarrisonCardEntry> heroEntries = new List<GarrisonCardEntry>();
    private List<GarrisonCardEntry> garrisonEntries = new List<GarrisonCardEntry>();

    private float messageTimer;

    void Start()
    {
        // 从 GameSession 读取兵力数据
        heroDeck = GameSession.GetGarrisonHeroDeck();
        garrisonDeck = GameSession.GetGarrisonGarrisonDeck();

        if (heroDeck == null || garrisonDeck == null)
        {
            Debug.LogError("GarrisonUI: 未能从 GameSession 读取兵力数据，返回主场景。");
            GarrisonSceneBridge.ReturnToMainScene(new Deck(), new Deck());
            return;
        }

        transferToGarrisonBtn.onClick.AddListener(OnTransferToGarrison);
        transferToHeroBtn.onClick.AddListener(OnTransferToHero);
        returnBtn.onClick.AddListener(OnReturn);

        RebuildHeroSlots();
        RebuildGarrisonList();
        UpdateButtonStates();
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
        ClearContainer(heroSlotsContainer);
        heroEntries.Clear();

        int slotCount = Mathf.Max(Deck.HeroSlotLimit, heroDeck.CardCount);
        for (int i = 0; i < slotCount; i++)
        {
            GameObject go = Instantiate(heroSlotPrefab, heroSlotsContainer);
            GarrisonCardEntry entry = go.GetComponent<GarrisonCardEntry>();
            if (entry == null)
            {
                Debug.LogWarning("GarrisonUI: heroSlotPrefab 缺少 GarrisonCardEntry 组件");
                continue;
            }

            if (i < heroDeck.CardCount)
            {
                entry.Setup(heroDeck[i], i, true);
            }
            else
            {
                // 空槽位
                entry.Setup(null, i, true);
                if (entry.nameText != null) entry.nameText.text = "空";
                if (entry.raceText != null) entry.raceText.text = "";
                if (entry.statsText != null) entry.statsText.text = "";
            }

            entry.onClicked = OnHeroSlotClicked;
            heroEntries.Add(entry);
        }

        if (heroSlotCountText != null)
            heroSlotCountText.text = $"英雄兵力 ({heroDeck.CardCount}/{Deck.HeroSlotLimit})";
    }

    void RebuildGarrisonList()
    {
        ClearContainer(garrisonListContainer);
        garrisonEntries.Clear();

        for (int i = 0; i < garrisonDeck.CardCount; i++)
        {
            GameObject go = Instantiate(garrisonEntryPrefab, garrisonListContainer);
            GarrisonCardEntry entry = go.GetComponent<GarrisonCardEntry>();
            if (entry == null)
            {
                Debug.LogWarning("GarrisonUI: garrisonEntryPrefab 缺少 GarrisonCardEntry 组件");
                continue;
            }

            entry.Setup(garrisonDeck[i], i, false);
            entry.onClicked = OnGarrisonEntryClicked;
            garrisonEntries.Add(entry);
        }

        if (garrisonCountText != null)
            garrisonCountText.text = $"据点驻军 ({garrisonDeck.CardCount})";
    }

    void ClearContainer(Transform container)
    {
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
        if (entry.cardData == null) return; // 空槽位不可选中

        if (selectedHeroEntry == entry)
        {
            // 取消选中
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
}
