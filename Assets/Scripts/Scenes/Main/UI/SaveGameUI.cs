using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 游戏内手动存档面板，与战斗场景暗金风格统一。
/// 显示3个存档槽位（含已有存档信息），支持覆盖保存。
/// </summary>
public class SaveGameUI : MonoBehaviour
{
    [Header("面板")]
    public GameObject panel;
    public Image panelBackground;
    public CanvasGroup panelCanvasGroup;

    [Header("标题")]
    public TMP_Text titleText;

    [Header("存档槽位视图")]
    public SaveSlotView[] slotViews = new SaveSlotView[SaveSystem.MaxManualSaveSlots];

    [Header("底部按钮")]
    public Button backButton;
    public TMP_Text backButtonText;

    [Header("消息")]
    public TMP_Text messageText;

    [Header("动画")]
    public float panelFadeInDuration = 0.25f;
    public float panelFadeOutDuration = 0.2f;

    [Header("地图输入")]
    public InputManager inputManager;

    private bool isOpen = false;
    private float messageTimer;

    private void Awake()
    {
        if (panelCanvasGroup == null)
            panelCanvasGroup = GetComponent<CanvasGroup>();
        if (panelCanvasGroup == null)
            panelCanvasGroup = gameObject.AddComponent<CanvasGroup>();

        for (int i = 0; i < slotViews.Length; i++)
        {
            if (slotViews[i] != null)
            {
                int slot = i + 1;
                slotViews[i].Setup(slot, false, true);
                slotViews[i].onSlotClicked = OnSlotClicked;
            }
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(Close);
        }

        ApplyTheme();
    }

    private void Start()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    void Update()
    {
        if (!isOpen) return;

        if (messageTimer > 0)
        {
            messageTimer -= Time.deltaTime;
            if (messageTimer <= 0 && messageText != null)
                messageText.text = "";
        }

        if (Input.GetKeyDown(KeyCode.Escape))
            Close();
    }

    void ApplyTheme()
    {
        if (panelBackground != null)
            panelBackground.color = UITheme.PanelBgDark;

        if (titleText != null)
        {
            titleText.color = UITheme.DarkGoldBright;
            titleText.text = "保存游戏";
        }

        if (backButtonText != null)
            backButtonText.color = UITheme.TextPrimary;

        if (messageText != null)
            messageText.color = UITheme.TextSecondary;
    }

    public void Open()
    {
        if (panel != null)
            panel.SetActive(true);

        if (inputManager != null)
            inputManager.enabled = false;

        isOpen = true;
        RefreshAllSlots();
        StartCoroutine(PlayPanelEnterAnimation());
    }

    public void Close()
    {
        isOpen = false;

        if (inputManager != null)
            inputManager.enabled = true;

        StartCoroutine(PlayPanelExitAnimation());
    }

    void RefreshAllSlots()
    {
        for (int i = 0; i < slotViews.Length; i++)
        {
            if (slotViews[i] == null) continue;

            int slot = i + 1;
            SaveSlotInfo info = SaveSystem.GetManualSaveSlotInfo(slot);
            slotViews[i].Refresh(info);
            slotViews[i].SetInteractable(true);
        }
    }

    void OnSlotClicked(int slotIndex, bool isAuto)
    {
        GameManager gm = GameManager.Instance;
        if (gm == null) return;

        bool success = gm.SaveToManualSlot(slotIndex);
        if (success)
        {
            ShowMessage($"已保存到存档 {slotIndex}");
            RefreshAllSlots();
        }
        else
        {
            ShowMessage($"存档 {slotIndex} 保存失败");
        }
    }

    void ShowMessage(string msg)
    {
        if (messageText != null)
        {
            messageText.text = msg;
            messageText.color = UITheme.AccentGreen;
        }
        messageTimer = 3f;
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
        for (int i = 0; i < slotViews.Length; i++)
        {
            if (slotViews[i] != null)
                slotViews[i].PlayEnterAnimation(staggerDelay * i);
        }
    }

    IEnumerator PlayPanelExitAnimation()
    {
        if (panelCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < panelFadeOutDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / panelFadeOutDuration));
                panelCanvasGroup.alpha = 1f - t;
                yield return null;
            }
        }

        if (panel != null)
            panel.SetActive(false);

        if (panelCanvasGroup != null)
            panelCanvasGroup.alpha = 1f;
    }
}
