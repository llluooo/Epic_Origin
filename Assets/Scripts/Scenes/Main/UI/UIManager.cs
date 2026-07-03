using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI管理器
/// 负责显示回合信息、资源、按钮监听
/// </summary>
public class UIManager : MonoBehaviour
{
    public TMP_Text turnText;
    public TMP_Text stateText;
    public TMP_Text resourceText;

    [Header("存档按钮")]
    public Button saveButton;

    [Header("存档面板")]
    public SaveGameUI saveGameUI;

    [Header("英雄状态面板")]
    public HeroStatusUI heroStatusUI;

    [Header("游戏菜单")]
    public GameMenuUI gameMenuUI;

    void Start()
    {
        if (saveButton != null)
        {
            saveButton.onClick.RemoveAllListeners();
            saveButton.onClick.AddListener(OnSaveButton);
        }
    }

    void Update()
    {
        UpdateUI();
        HandleKeyboardInput();
    }

    void HandleKeyboardInput()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null) return;

        // 战斗进行中或游戏结束时不响应快捷键
        if (gm.isBattleActive || gm.IsGameEnded) return;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            bool menuOpen = gameMenuUI != null && gameMenuUI.panel != null && gameMenuUI.panel.activeSelf;
            if (!menuOpen && heroStatusUI != null)
                heroStatusUI.Toggle();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            bool heroOpen = heroStatusUI != null && heroStatusUI.panel != null && heroStatusUI.panel.activeSelf;
            if (!heroOpen && gameMenuUI != null)
                gameMenuUI.Toggle();
        }
    }

    void UpdateUI()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null) return;

        // 回合信息
        turnText.text = $"回合: {gm.currentTurn}/{gm.maxTurn}";

        // 状态信息
        if (gm.currentState == GameManager.GameState.End)
        {
            stateText.text = "游戏结束";
        }
        else if (gm.isPlayerTurn)
        {
            stateText.text = "你的回合";
            if (gm.hasPlayerActed)
                stateText.text += " [已行动]";
        }
        else
        {
            stateText.text = "AI回合";
        }

        // 资源信息
        if (resourceText != null && gm.player != null)
        {
            Player p = gm.player;
            resourceText.text = $"金币:{p.resources.gold}  建材:{p.resources.buildingMaterials}  据点Lv{p.strongholdLevel}";
        }

        // 存档按钮仅在玩家回合可用
        if (saveButton != null)
        {
            saveButton.interactable = gm.isPlayerTurn && !gm.IsGameEnded;
        }
    }

    /// <summary>
    /// 结束回合按钮监听
    /// </summary>
    public void OnEndTurnButton()
    {
        GameManager.Instance.EndPlayerTurn();
    }

    void OnSaveButton()
    {
        if (saveGameUI != null)
        {
            saveGameUI.Open();
        }
    }
}
