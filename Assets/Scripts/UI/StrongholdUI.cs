using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 据点经营面板
/// 在场景中挂载到据点UI面板的GameObject上
/// </summary>
public class StrongholdUI : MonoBehaviour
{
    public GameObject panel;
    public TMP_Text titleText;
    public TMP_Text upgradeCostText;
    public TMP_Text summonCostText;
    public TMP_Text messageText;
    public Button upgradeButton;
    public Button summonButton;
    public Button leaveButton;

    private bool isOpen = false;

    void Start()
    {
        panel.SetActive(false);

        upgradeButton.onClick.AddListener(OnUpgrade);
        summonButton.onClick.AddListener(OnSummon);
        leaveButton.onClick.AddListener(OnLeave);
    }

    void Update()
    {
        if (!isOpen) return;

        GameManager gm = GameManager.Instance;
        if (gm == null || gm.player == null) return;

        Player p = gm.player;

        // 更新标题
        titleText.text = $"你的据点 Lv{p.strongholdLevel}";

        // 升级按钮
        int upgradeCost = p.GetUpgradeCost();
        upgradeCostText.text = $"升级费用: {upgradeCost}建材";
        upgradeButton.interactable = p.CanUpgrade() && !gm.hasPlayerActed && p.strongholdLevel < 5;

        // 召唤按钮（召唤当前据点等级的单位）
        int summonCost = p.GetSummonCost(p.strongholdLevel);
        summonCostText.text = $"召唤 Lv{p.strongholdLevel} 单位: {summonCost}金币";
        summonButton.interactable = p.CanSummon(p.strongholdLevel) && !gm.hasPlayerActed;
    }

    public void Open()
    {
        panel.SetActive(true);
        isOpen = true;
        messageText.text = "";
    }

    public void Close()
    {
        panel.SetActive(false);
        isOpen = false;
    }

    void OnUpgrade()
    {
        GameManager gm = GameManager.Instance;
        Player p = gm.player;

        if (!p.CanUpgrade())
        {
            messageText.text = "资源不足或已达到最高等级！";
            return;
        }

        int oldLevel = p.strongholdLevel;
        p.UpgradeStronghold();
        gm.OnPlayerAction();
        messageText.text = $"据点升级: Lv{oldLevel} → Lv{p.strongholdLevel}！";
    }

    void OnSummon()
    {
        GameManager gm = GameManager.Instance;
        Player p = gm.player;

        int level = p.strongholdLevel;
        if (!p.CanSummon(level))
        {
            messageText.text = "金币不足！";
            return;
        }

        // 随机召唤一个该种族的单位
        int unitCount = p.race switch
        {
            RaceType.Human => HumanUnit.UnitCount,
            RaceType.Heaven => HeavenUnit.UnitCount,
            RaceType.Ghost => GhostUnit.UnitCount,
            _ => 5
        };
        int unitIndex = Random.Range(0, unitCount);

        bool success = gm.SummonUnit(p, unitIndex, level);
        if (success)
        {
            gm.OnPlayerAction();
            Card lastCard = p.deck[p.deck.CardCount - 1];
            messageText.text = $"召唤成功: {lastCard.cardName} Lv{lastCard.level} (ATK:{lastCard.GetAttack()} HP:{lastCard.GetMaxHP()})";
        }
        else
        {
            messageText.text = "召唤失败！";
        }
    }

    void OnLeave()
    {
        Close();
    }
}
