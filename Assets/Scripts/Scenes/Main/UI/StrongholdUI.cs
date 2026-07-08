using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 全屏据点管理面板，使用全屏覆盖方式切换显隐。
/// 进入据点时全屏覆盖，展示据点信息、升级、5张兵种卡牌（含数量选择器）、卡组信息
/// 据点内所有操作不消耗回合行动
/// </summary>
public class StrongholdUI : MonoBehaviour
{
    [Header("面板根节点")]
    public GameObject panel;

    [Header("主相机（进入据点时关闭，离开时恢复）")]
    public Camera mainCamera;

    [Header("据点背景相机（纯色背景，无音频监听器）")]
    public Camera strongholdCamera;

    [Header("顶部信息")]
    public TMP_Text strongholdLevelText;
    public TMP_Text goldText;
    public TMP_Text materialsText;

    [Header("升级区")]
    public TMP_Text upgradeCostText;
    public Button upgradeButton;
    public TMP_Text upgradeButtonText;

    [Header("兵种卡牌（5张，按Lv1-Lv5排列）")]
    public UnitCardUI[] unitCards;

    [Header("卡牌美术（每种族5张）")]
    public Sprite[] humanCardSprites;
    public Sprite[] heavenCardSprites;
    public Sprite[] ghostCardSprites;

    [Header("据点背景图（每种族1张）")]
    public Image backgroundImage;
    public Sprite humanStrongholdBg;
    public Sprite heavenStrongholdBg;
    public Sprite ghostStrongholdBg;

    [Header("离开")]
    public Button leaveButton;

    [Header("消息")]
    public TMP_Text messageText;

    [Header("地图输入")]
    public InputManager inputManager;

    public bool IsOpen { get; private set; }
    private float messageTimer;

    void Start()
    {
        // 初始隐藏面板和据点相机
        if (panel != null)
            panel.SetActive(false);
        if (strongholdCamera != null)
            strongholdCamera.enabled = false;

        upgradeButton.onClick.AddListener(OnUpgrade);
        leaveButton.onClick.AddListener(Close);

        // 绑定每张卡牌的召唤回调
        if (unitCards != null)
        {
            for (int i = 0; i < unitCards.Length; i++)
            {
                int unitIndex = i;
                unitCards[i].onSummonRequested = (idx, qty) => OnSummonUnit(idx, qty);
            }
        }
    }

    void Update()
    {
        if (!IsOpen) return;

        GameManager gm = GameManager.Instance;
        if (gm == null || gm.player == null) return;

        RefreshResourceDisplay(gm.player);

        if (messageTimer > 0)
        {
            messageTimer -= Time.deltaTime;
            if (messageTimer <= 0 && messageText != null)
                messageText.text = "";
        }
    }

    public void Open()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null || gm.player == null) return;

        Player p = gm.player;

        // 显示面板 + 切相机 + 禁用地图输入
        if (panel != null)
            panel.SetActive(true);
        if (mainCamera != null)
            mainCamera.enabled = false;
        if (strongholdCamera != null)
            strongholdCamera.enabled = true;
        if (inputManager != null)
            inputManager.enabled = false;

        IsOpen = true;

        // 设置种族据点背景图
        if (backgroundImage != null)
            backgroundImage.sprite = GetBackgroundForRace(p.race);

        // 初始化5张卡牌
        Sprite[] cardSprites = GetCardSpritesForRace(p.race);
        for (int i = 0; i < unitCards.Length; i++)
        {
            Sprite sprite = (cardSprites != null && i < cardSprites.Length) ? cardSprites[i] : null;
            unitCards[i].Setup(i, sprite, p);
        }

        RefreshAll(p);
        ShowMessage("");
        Debug.Log("进入据点管理界面");
    }

    public void Close()
    {
        if (panel != null)
            panel.SetActive(false);
        if (mainCamera != null)
            mainCamera.enabled = true;
        if (strongholdCamera != null)
            strongholdCamera.enabled = false;
        if (inputManager != null)
            inputManager.enabled = true;

        IsOpen = false;
        Debug.Log("离开据点管理界面");
    }

    void RefreshAll(Player p)
    {
        RefreshResourceDisplay(p);
        RefreshUpgradeSection(p);

        for (int i = 0; i < unitCards.Length; i++)
            unitCards[i].RefreshDisplay();
    }

    void RefreshResourceDisplay(Player p)
    {
        if (strongholdLevelText != null)
            strongholdLevelText.text = $"据点 Lv{p.strongholdLevel}";
        if (goldText != null)
            goldText.text = $"金币: {p.resources.gold}";
        if (materialsText != null)
            materialsText.text = $"建材: {p.resources.buildingMaterials}";
    }

    void RefreshUpgradeSection(Player p)
    {
        if (p.strongholdLevel >= 5)
        {
            upgradeButton.interactable = false;
            if (upgradeCostText != null)
                upgradeCostText.text = "据点已满级 (Lv5)";
            if (upgradeButtonText != null)
                upgradeButtonText.text = "已满级";
            return;
        }

        ResourceData cost = p.GetUpgradeCost();
        bool canAfford = p.CanUpgrade();

        if (upgradeCostText != null)
            upgradeCostText.text = $"升级到 Lv{p.strongholdLevel + 1} → 消耗: {cost.gold}金 + {cost.buildingMaterials}材";

        upgradeButton.interactable = canAfford;

        if (upgradeButtonText != null)
            upgradeButtonText.text = canAfford ? "升级据点" : "资源不足";
    }

    void OnUpgrade()
    {
        GameManager gm = GameManager.Instance;
        Player p = gm.player;

        if (!p.CanUpgrade())
        {
            ShowMessage("资源不足或已满级！");
            return;
        }

        int oldLevel = p.strongholdLevel;
        p.UpgradeStronghold();

        RefreshAll(p);
        ShowMessage($"据点升级: Lv{oldLevel} → Lv{p.strongholdLevel}！");

        Debug.Log($"据点升级: Lv{oldLevel} → Lv{p.strongholdLevel}");
    }

    void OnSummonUnit(int unitIndex, int quantity)
    {
        GameManager gm = GameManager.Instance;
        Player p = gm.player;

        int summoned = gm.SummonUnits(p, unitIndex, quantity);
        if (summoned > 0)
        {
            RefreshAll(p);
            string unitName = unitCards[unitIndex] != null ? unitCards[unitIndex].nameLevelText.text : $"Lv{unitIndex + 1}兵种";
            ShowMessage($"成功召唤 {summoned} 张 {unitName}！");
        }
        else
        {
            ShowMessage("召唤失败，请检查资源！");
        }
    }

    void ShowMessage(string msg)
    {
        if (messageText != null)
            messageText.text = msg;
        messageTimer = 3f;
    }

    Sprite[] GetCardSpritesForRace(RaceType race)
    {
        return race switch
        {
            RaceType.Human => humanCardSprites,
            RaceType.Heaven => heavenCardSprites,
            RaceType.Ghost => ghostCardSprites,
            _ => humanCardSprites
        };
    }

    Sprite GetBackgroundForRace(RaceType race)
    {
        return race switch
        {
            RaceType.Human => humanStrongholdBg,
            RaceType.Heaven => heavenStrongholdBg,
            RaceType.Ghost => ghostStrongholdBg,
            _ => humanStrongholdBg
        };
    }
}
