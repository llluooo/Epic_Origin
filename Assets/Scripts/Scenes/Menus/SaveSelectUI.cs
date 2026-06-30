using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 存档选择界面，使用卡牌式槽位视图，与战斗场景暗金风格统一。
/// 支持自动存档 + 3个手动存档槽，入场动画，悬停/选中反馈。
/// </summary>
public class SaveSelectUI : MonoBehaviour
{
    [Header("面板")]
    public Image panelBackground;
    public CanvasGroup panelCanvasGroup;

    [Header("标题")]
    public TMP_Text titleText;

    [Header("存档槽位视图")]
    public SaveSlotView autoSaveSlotView;
    public SaveSlotView[] manualSlotViews = new SaveSlotView[SaveSystem.MaxManualSaveSlots];

    [Header("底部按钮")]
    public Button backButton;
    public TMP_Text backButtonText;

    [Header("消息")]
    public TMP_Text messageText;

    [Header("动画")]
    public float panelFadeInDuration = 0.25f;

    private int selectedSlotIndex = -1;
    private bool selectedIsAuto = false;

    private void Awake()
    {
        if (panelCanvasGroup == null)
            panelCanvasGroup = GetComponent<CanvasGroup>();
        if (panelCanvasGroup == null)
            panelCanvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (autoSaveSlotView != null)
        {
            autoSaveSlotView.Setup(0, true);
            autoSaveSlotView.onSlotClicked = OnSlotClicked;
        }

        for (int i = 0; i < manualSlotViews.Length; i++)
        {
            if (manualSlotViews[i] != null)
            {
                int slot = i + 1;
                manualSlotViews[i].Setup(slot, false);
                manualSlotViews[i].onSlotClicked = OnSlotClicked;
            }
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(OnBackButton);
        }

        ApplyTheme();
    }

    private void Start()
    {
        RefreshAllSlots();
        if (messageText != null)
            messageText.gameObject.SetActive(false);
        StartCoroutine(PlayPanelEnterAnimation());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            OnBackButton();
    }

    void ApplyTheme()
    {
        if (panelBackground != null)
            panelBackground.color = UITheme.PanelBgDark;

        if (titleText != null)
        {
            titleText.color = UITheme.DarkGoldBright;
            titleText.text = "选择存档";
        }

        if (backButtonText != null)
            backButtonText.color = UITheme.TextPrimary;

        if (messageText != null)
            messageText.color = UITheme.TextSecondary;
    }

    void RefreshAllSlots()
    {
        if (autoSaveSlotView != null)
        {
            SaveSlotInfo autoInfo = SaveSystem.GetAutoSaveSlotInfo();
            autoSaveSlotView.Refresh(autoInfo);
            autoSaveSlotView.SetInteractable(autoInfo.hasSave);
        }

        for (int i = 0; i < manualSlotViews.Length; i++)
        {
            if (manualSlotViews[i] == null) continue;

            int slot = i + 1;
            SaveSlotInfo info = SaveSystem.GetManualSaveSlotInfo(slot);
            manualSlotViews[i].Refresh(info);
            manualSlotViews[i].SetInteractable(info.hasSave);
        }
    }

    void OnSlotClicked(int slotIndex, bool isAuto)
    {
        selectedSlotIndex = slotIndex;
        selectedIsAuto = isAuto;

        UpdateSelectionHighlight();

        GameRunState loadedState = isAuto
            ? SaveSystem.LoadAutoGame()
            : SaveSystem.LoadManualGame(slotIndex);

        if (loadedState == null)
        {
            ShowMessage(isAuto ? "自动档加载失败" : $"存档槽 {slotIndex} 加载失败");
            RefreshAllSlots();
            return;
        }

        GameSession.UpdateRunState(loadedState);
        GameSetupData.IsNewGame = false;
        SceneManager.LoadScene("MainScene");
    }

    void UpdateSelectionHighlight()
    {
        if (autoSaveSlotView != null)
            autoSaveSlotView.SetSelected(selectedIsAuto);

        for (int i = 0; i < manualSlotViews.Length; i++)
        {
            if (manualSlotViews[i] == null) continue;
            int slot = i + 1;
            manualSlotViews[i].SetSelected(!selectedIsAuto && slot == selectedSlotIndex);
        }
    }

    void OnBackButton()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    void ShowMessage(string msg)
    {
        if (messageText != null)
        {
            messageText.text = msg;
            messageText.color = UITheme.AccentRed;
            messageText.gameObject.SetActive(!string.IsNullOrEmpty(msg));
        }
    }

    IEnumerator PlayPanelEnterAnimation()
    {
        if (panelCanvasGroup != null)
            panelCanvasGroup.alpha = 0f;

        float elapsed = 0f;
        while (elapsed < panelFadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / panelFadeInDuration));
            if (panelCanvasGroup != null)
                panelCanvasGroup.alpha = t;
            yield return null;
        }

        if (panelCanvasGroup != null)
            panelCanvasGroup.alpha = 1f;

        float staggerDelay = UITheme.SlotEnterStaggerDelay;

        if (autoSaveSlotView != null)
            autoSaveSlotView.PlayEnterAnimation(0f);

        for (int i = 0; i < manualSlotViews.Length; i++)
        {
            if (manualSlotViews[i] != null)
                manualSlotViews[i].PlayEnterAnimation(staggerDelay * (i + 1));
        }
    }
}
