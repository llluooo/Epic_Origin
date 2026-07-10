using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI管理器。负责显示回合信息、资源、按钮监听，以及Tab/Esc快捷键调度。
/// 面板打开时游戏后台完全暂停：禁用结束回合、存档按钮，仅允许面板操作。
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public TMP_Text turnText;
    public TMP_Text stateText;
    public TMP_Text resourceText;

    [Header("游戏按钮（面板打开时禁用）")]
    public Button endTurnButton;
    public Button saveButton;

    [Header("存档面板")]
    public SaveGameUI saveGameUI;

    [Header("英雄状态面板")]
    public HeroStatusUI heroStatusUI;

    [Header("游戏菜单")]
    public GameMenuUI gameMenuUI;

    [Header("据点面板")]
    public StrongholdUI strongholdUI;

    [Header("游戏结束结算面板")]
    public GameEndUI gameEndUI;

    [Header("地图操作反馈消息区域")]
    public MessageLogUI messageLogUI;

    /// <summary>
    /// 是否有任何面板处于打开状态。面板打开时游戏后台应完全暂停。
    /// </summary>
    public bool IsAnyPanelOpen
    {
        get
        {
            return (heroStatusUI != null && heroStatusUI.IsOpen)
                || (gameMenuUI != null && gameMenuUI.IsOpen)
                || (strongholdUI != null && strongholdUI.IsOpen)
                || (saveGameUI != null && saveGameUI.IsOpen)
                || (gameEndUI != null && gameEndUI.IsOpen);
        }
    }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        AudioManager.Instance?.PlayBGM(BGM.Map);

        if (endTurnButton != null)
            endTurnButton.onClick.AddListener(OnEndTurnButton);
        if (saveButton != null)
            saveButton.onClick.AddListener(OnSaveButton);
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
        if (gm.isBattleActive || gm.IsGameEnded) return;

        bool heroOpen = heroStatusUI != null && heroStatusUI.IsOpen;
        bool menuOpen = gameMenuUI != null && gameMenuUI.IsOpen;
        bool strongholdOpen = strongholdUI != null && strongholdUI.IsOpen;
        bool saveOpen = saveGameUI != null && saveGameUI.IsOpen;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (heroOpen)
            {
                heroStatusUI.Close();
            }
            else if (!strongholdOpen && !saveOpen)
            {
                if (menuOpen)
                    gameMenuUI.Close();
                heroStatusUI.Open();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 优先级：菜单子面板 > 存档面板 > 英雄状态 > 据点面板 > 游戏菜单 > 打开菜单
            if (menuOpen)
            {
                if (!gameMenuUI.HandleEsc())
                    gameMenuUI.Close();
            }
            else if (saveOpen)
            {
                saveGameUI.Close();
            }
            else if (heroOpen)
            {
                if (heroStatusUI.openedFromGameMenu)
                {
                    heroStatusUI.Close();
                    gameMenuUI.Open();
                }
                else
                {
                    heroStatusUI.Close();
                }
            }
            else if (strongholdOpen)
            {
                strongholdUI.Close();
            }
            else
            {
                gameMenuUI.Open();
            }
        }
    }

    void UpdateUI()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null) return;

        bool paused = IsAnyPanelOpen;

        // 回合信息
        turnText.text = $"回合: {gm.currentTurn}/{gm.maxTurn}";

        // 状态信息
        if (gm.currentState == GameManager.GameState.End)
        {
            stateText.text = "游戏结束";
        }
        else if (paused)
        {
            stateText.text = "已暂停";
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

        if (gm.IsGameEnded && gameEndUI != null && !gameEndUI.IsOpen)
        {
            if (gm.GameEndIsVictory)
                gameEndUI.ShowVictory(gm.GameEndDetail);
            else
                gameEndUI.ShowDefeat(gm.GameEndDetail);
        }

        // 面板打开时禁用游戏按钮，确保后台完全暂停
        if (endTurnButton != null)
            endTurnButton.interactable = gm.isPlayerTurn && !gm.IsGameEnded && !paused;
        if (saveButton != null)
            saveButton.interactable = gm.isPlayerTurn && !gm.IsGameEnded && !paused;
    }

    public void OnEndTurnButton()
    {
        GameManager.Instance.EndPlayerTurn();
    }

    void OnSaveButton()
    {
        if (saveGameUI != null)
            saveGameUI.Open();
    }

    public void ShowGameEndPanel(string detail, bool isVictory)
    {
        if (gameEndUI == null)
        {
            gameEndUI = FindObjectOfType<GameEndUI>();
            if (gameEndUI == null)
            {
                gameEndUI = CreateGameEndUI();
            }
        }

        if (gameEndUI != null)
        {
            if (isVictory)
                gameEndUI.ShowVictory(detail);
            else
                gameEndUI.ShowDefeat(detail);
        }
        else
        {
            Debug.LogWarning("未找到 GameEndUI，无法显示游戏结算面板。");
        }
    }

    private GameEndUI CreateGameEndUI()
    {
        GameObject uiObject = new GameObject("GameEndUI");
        GameEndUI endUI = uiObject.AddComponent<GameEndUI>();
        return endUI;
    }
}
