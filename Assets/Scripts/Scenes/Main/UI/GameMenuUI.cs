using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 游戏暂停菜单（Esc打开）。
/// 包含继续游戏、英雄状态、存档、读档、设置（占位）、退出游戏。
/// </summary>
public class GameMenuUI : MonoBehaviour
{
    [Header("面板")]
    public GameObject panel;

    [Header("主菜单按钮")]
    public Button continueButton;
    public Button heroStatusButton;
    public Button saveButton;
    public Button loadButton;
    public Button settingsButton;
    public Button exitButton;

    [Header("读档槽位区")]
    public GameObject loadSlotContainer;
    public SaveSlotView[] loadSlotViews;
    public Button loadBackButton;

    [Header("确认退出")]
    public GameObject confirmExitPanel;
    public Button confirmExitYesButton;
    public Button confirmExitNoButton;

    [Header("设置占位")]
    public GameObject placeholderPanel;
    public TMP_Text placeholderText;

    [Header("地图输入")]
    public InputManager inputManager;

    [Header("引用外部面板")]
    public HeroStatusUI heroStatusUI;
    public SaveGameUI saveGameUI;

    private bool isOpen = false;
    private float placeholderTimer;

    void Start()
    {
        if (panel != null)
            panel.SetActive(false);

        if (continueButton != null)
            continueButton.onClick.AddListener(Close);
        if (heroStatusButton != null)
            heroStatusButton.onClick.AddListener(OnHeroStatus);
        if (saveButton != null)
            saveButton.onClick.AddListener(OnSave);
        if (loadButton != null)
            loadButton.onClick.AddListener(OnLoad);
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettings);
        if (exitButton != null)
            exitButton.onClick.AddListener(OnExit);
        if (loadBackButton != null)
            loadBackButton.onClick.AddListener(HideLoadSlots);
        if (confirmExitYesButton != null)
            confirmExitYesButton.onClick.AddListener(OnExitConfirmed);
        if (confirmExitNoButton != null)
            confirmExitNoButton.onClick.AddListener(HideConfirmExit);

        // 绑定读档槽位点击
        if (loadSlotViews != null)
        {
            for (int i = 0; i < loadSlotViews.Length; i++)
            {
                if (loadSlotViews[i] != null)
                {
                    int slot = i + 1;
                    loadSlotViews[i].Setup(slot, false, false);
                    loadSlotViews[i].onSlotClicked = (s, _) => OnLoadSlotClicked(s);
                }
            }
        }

        if (loadSlotContainer != null)
            loadSlotContainer.SetActive(false);
        if (confirmExitPanel != null)
            confirmExitPanel.SetActive(false);
        if (placeholderPanel != null)
            placeholderPanel.SetActive(false);
    }

    void Update()
    {
        if (!isOpen) return;

        if (placeholderTimer > 0)
        {
            placeholderTimer -= Time.deltaTime;
            if (placeholderTimer <= 0 && placeholderPanel != null)
                placeholderPanel.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (confirmExitPanel != null && confirmExitPanel.activeSelf)
                HideConfirmExit();
            else if (loadSlotContainer != null && loadSlotContainer.activeSelf)
                HideLoadSlots();
            else
                Close();
        }
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
        if (gm == null) return;

        if (panel != null)
            panel.SetActive(true);
        if (inputManager != null)
            inputManager.enabled = false;

        // 重置所有子面板状态
        HideLoadSlots();
        HideConfirmExit();
        if (placeholderPanel != null)
            placeholderPanel.SetActive(false);

        isOpen = true;
        Debug.Log("打开游戏菜单");
    }

    public void Close()
    {
        if (panel != null)
            panel.SetActive(false);
        if (inputManager != null)
            inputManager.enabled = true;

        HideLoadSlots();
        HideConfirmExit();
        if (placeholderPanel != null)
            placeholderPanel.SetActive(false);

        isOpen = false;
        Debug.Log("关闭游戏菜单");
    }

    void OnHeroStatus()
    {
        Close();
        if (heroStatusUI != null)
            heroStatusUI.Open();
    }

    void OnSave()
    {
        Close();
        if (saveGameUI != null)
            saveGameUI.Open();
    }

    void OnLoad()
    {
        ShowLoadSlots();
    }

    void ShowLoadSlots()
    {
        if (loadSlotContainer != null)
            loadSlotContainer.SetActive(true);

        if (loadSlotViews != null)
        {
            for (int i = 0; i < loadSlotViews.Length; i++)
            {
                if (loadSlotViews[i] == null) continue;
                int slot = i + 1;
                SaveSlotInfo info = SaveSystem.GetManualSaveSlotInfo(slot);
                loadSlotViews[i].Refresh(info);
                loadSlotViews[i].SetInteractable(info.hasSave);
            }
        }
    }

    void HideLoadSlots()
    {
        if (loadSlotContainer != null)
            loadSlotContainer.SetActive(false);
    }

    void OnLoadSlotClicked(int slotIndex)
    {
        GameRunState loadedState = SaveSystem.LoadManualGame(slotIndex);
        if (loadedState == null)
        {
            Debug.LogWarning($"读档失败：存档槽 {slotIndex} 不可用");
            return;
        }

        GameSession.UpdateRunState(loadedState);
        GameSetupData.IsNewGame = false;
        Debug.Log($"从存档槽 {slotIndex} 加载游戏，重新进入MainScene");
        SceneManager.LoadScene("MainScene");
    }

    void OnSettings()
    {
        if (placeholderPanel != null)
        {
            placeholderPanel.SetActive(true);
            if (placeholderText != null)
                placeholderText.text = "设置功能开发中...";
        }
        placeholderTimer = 3f;
    }

    void OnExit()
    {
        if (confirmExitPanel != null)
            confirmExitPanel.SetActive(true);
    }

    void HideConfirmExit()
    {
        if (confirmExitPanel != null)
            confirmExitPanel.SetActive(false);
    }

    void OnExitConfirmed()
    {
        Debug.Log("退出游戏，返回主菜单");
        GameSession.ClearAll();
        SceneManager.LoadScene("MainMenuScene");
    }
}
