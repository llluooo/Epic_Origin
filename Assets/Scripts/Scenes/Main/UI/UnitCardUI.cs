using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 单张兵种卡牌UI组件
/// 包含：卡牌美术图、名称/等级、攻击、生命、消耗、已有数量、数量选择器、召唤按钮
/// </summary>
public class UnitCardUI : MonoBehaviour
{
    [Header("卡牌信息")]
    public Image cardArtImage;
    public TMP_Text nameLevelText;
    public TMP_Text statsText;
    public TMP_Text costText;
    public TMP_Text ownedText;

    [Header("数量选择器")]
    public Button minusButton;
    public TMP_InputField quantityInput;
    public Button plusButton;

    [Header("召唤")]
    public Button summonButton;
    public TMP_Text summonButtonText;

    [Header("状态")]
    public Image lockOverlay;
    public Image highlightBorder;

    // 数据
    private int unitIndex;
    private int quantity = 1;
    private Player playerCache;
    private bool isUnlocked;
    private float lastSummonTime;

    // 回调参数：兵种索引、召唤数量
    public System.Action<int, int> onSummonRequested;

    void Start()
    {
        minusButton.onClick.AddListener(Decrement);
        plusButton.onClick.AddListener(Increment);
        summonButton.onClick.AddListener(OnSummonClicked);
        quantityInput.onEndEdit.AddListener(OnQuantityInputChanged);
    }

    /// <summary>
    /// 初始化卡牌显示
    /// </summary>
    public void Setup(int unitIndex, Sprite cardArt, Player player)
    {
        this.unitIndex = unitIndex;
        this.playerCache = player;

        // 初始隐藏覆盖层
        if (lockOverlay != null)
            lockOverlay.gameObject.SetActive(false);
        if (highlightBorder != null)
            highlightBorder.gameObject.SetActive(false);

        // 卡牌美术
        if (cardArtImage != null)
        {
            cardArtImage.sprite = cardArt;
            cardArtImage.preserveAspect = true;
        }

        RefreshDisplay();
    }

    /// <summary>
    /// 刷新卡牌显示（资源、状态变化后调用）
    /// </summary>
    public void RefreshDisplay()
    {
        if (playerCache == null) return;

        int level = unitIndex + 1;
        isUnlocked = playerCache.strongholdLevel >= level;
        bool canAfford = playerCache.CanSummon(level);
        int ownedCount = playerCache.GetOwnedCount(unitIndex);
        ResourceData cost = playerCache.GetSummonCost(level);

        // 名称等级
        string unitName = GetUnitName();
        if (nameLevelText != null)
            nameLevelText.text = $"{unitName}  Lv{level}";

        // 数值（从Card数据取）
        if (statsText != null)
            statsText.text = GetUnitStats();

        // 消耗
        if (costText != null)
            costText.text = $"消耗:{cost.gold}金 + {cost.buildingMaterials}材";

        // 已有数量
        if (ownedText != null)
            ownedText.text = $"拥有:×{ownedCount}";

        // 状态控制
        if (!isUnlocked)
        {
            SetLocked(true);
        }
        else if (!canAfford)
        {
            SetLocked(false);
            SetUnaffordable(true);
        }
        else
        {
            SetLocked(false);
            SetUnaffordable(false);
        }

        // 数量上限
        int maxAffordable = playerCache.GetMaxAffordableCount(unitIndex);
        if (quantity > maxAffordable)
            quantity = Mathf.Max(1, maxAffordable);
        if (quantity < 1)
            quantity = 1;

        UpdateQuantityDisplay();
        UpdateSummonButton();
    }

    void SetLocked(bool locked)
    {
        if (lockOverlay != null)
            lockOverlay.gameObject.SetActive(locked);

        minusButton.interactable = !locked;
        plusButton.interactable = !locked;
        quantityInput.interactable = !locked;
        summonButton.interactable = false;

        if (locked && summonButtonText != null)
            summonButtonText.text = $"需要据点Lv{unitIndex + 1}";

        if (locked)
            cardArtImage.color = new Color(0.4f, 0.4f, 0.4f);
    }

    void SetUnaffordable(bool unaffordable)
    {
        if (unaffordable)
        {
            summonButton.interactable = false;
            cardArtImage.color = new Color(0.6f, 0.6f, 0.6f);
            if (costText != null)
                costText.color = Color.red;
        }
        else
        {
            cardArtImage.color = Color.white;
            if (costText != null)
                costText.color = Color.white;
        }
    }

    void UpdateQuantityDisplay()
    {
        if (quantityInput != null && quantityInput.text != quantity.ToString())
            quantityInput.text = quantity.ToString();

        int maxAffordable = isUnlocked ? playerCache.GetMaxAffordableCount(unitIndex) : 0;
        minusButton.interactable = isUnlocked && quantity > 1;
        plusButton.interactable = isUnlocked && quantity < maxAffordable;
    }

    void UpdateSummonButton()
    {
        if (summonButtonText == null || !isUnlocked) return;

        int level = unitIndex + 1;
        ResourceData singleCost = playerCache.GetSummonCost(level);
        summonButton.interactable = quantity > 0 && playerCache.CanSummon(level)
            && playerCache.resources.gold >= singleCost.gold * quantity
            && playerCache.resources.buildingMaterials >= singleCost.buildingMaterials * quantity;

        summonButtonText.text = $"召唤 ×{quantity}  ({singleCost.gold * quantity}金)";
    }

    void Decrement()
    {
        if (quantity > 1)
        {
            quantity--;
            UpdateQuantityDisplay();
            UpdateSummonButton();
        }
    }

    void Increment()
    {
        int maxAffordable = playerCache.GetMaxAffordableCount(unitIndex);
        if (quantity < maxAffordable)
        {
            quantity++;
            UpdateQuantityDisplay();
            UpdateSummonButton();
        }
    }

    void OnQuantityInputChanged(string value)
    {
        if (!int.TryParse(value, out int parsed) || parsed < 1)
        {
            quantity = 1;
        }
        else
        {
            int maxAffordable = playerCache.GetMaxAffordableCount(unitIndex);
            quantity = Mathf.Clamp(parsed, 1, maxAffordable);
        }
        UpdateQuantityDisplay();
        UpdateSummonButton();
    }

    void OnSummonClicked()
    {
        if (Time.time - lastSummonTime < 0.5f) return; // 防连点
        lastSummonTime = Time.time;

        if (quantity <= 0) return;

        int level = unitIndex + 1;
        ResourceData totalCost = new ResourceData(
            playerCache.GetSummonCost(level).gold * quantity,
            playerCache.GetSummonCost(level).buildingMaterials * quantity
        );

        if (!playerCache.resources.CanAfford(totalCost)) return;

        onSummonRequested?.Invoke(unitIndex, quantity);

        // 刷新显示
        RefreshDisplay();
    }

    /// <summary>
    /// 设置选中高亮状态
    /// </summary>
    public void SetHighlighted(bool highlighted)
    {
        if (highlightBorder != null)
            highlightBorder.gameObject.SetActive(highlighted);
    }

    string GetUnitName()
    {
        string[] humanNames = { "剑士", "重装步兵", "巫师", "骑士", "皇家近卫" };
        string[] heavenNames = { "天兵", "天穹法师", "独角兽", "巨人", "大天使" };
        string[] ghostNames = { "骷髅", "僵尸", "鬼火", "死亡骑士", "死神" };

        return playerCache.race switch
        {
            RaceType.Human => (unitIndex < humanNames.Length) ? humanNames[unitIndex] : "???",
            RaceType.Heaven => (unitIndex < heavenNames.Length) ? heavenNames[unitIndex] : "???",
            RaceType.Ghost => (unitIndex < ghostNames.Length) ? ghostNames[unitIndex] : "???",
            _ => "???"
        };
    }

    string GetUnitStats()
    {
        int[] atk = playerCache.race switch
        {
            RaceType.Human => new int[] { 4, 5, 10, 12, 20 },
            RaceType.Heaven => new int[] { 5, 9, 10, 8, 25 },
            RaceType.Ghost => new int[] { 3, 4, 10, 14, 23 },
            _ => new int[] { 1, 1, 1, 1, 1 }
        };

        int[] hp = playerCache.race switch
        {
            RaceType.Human => new int[] { 5, 8, 7, 12, 25 },
            RaceType.Heaven => new int[] { 6, 8, 16, 35, 28 },
            RaceType.Ghost => new int[] { 2, 6, 5, 8, 25 },
            _ => new int[] { 1, 1, 1, 1, 1 }
        };

        if (unitIndex >= 0 && unitIndex < atk.Length)
            return $"攻击:{atk[unitIndex]}  生命:{hp[unitIndex]}";
        return "攻击:? 生命:?";
    }
}
