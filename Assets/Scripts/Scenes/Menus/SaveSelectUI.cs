using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 存档槽选择场景逻辑，支持三个独立存档槽。
/// </summary>
public class SaveSelectUI : MonoBehaviour
{
    public Button autoSaveButton;
    public TMP_Text autoSaveLabel;
    public Button[] slotButtons = new Button[SaveSystem.MaxManualSaveSlots];
    public TMP_Text[] slotLabels = new TMP_Text[SaveSystem.MaxManualSaveSlots];
    public Button backButton;

    private void Awake()
    {
        EnsureAutoSaveControls();

        if (slotButtons.Length != SaveSystem.MaxManualSaveSlots || slotLabels.Length != SaveSystem.MaxManualSaveSlots)
        {
            Debug.LogWarning($"SaveSelectUI: 需要 {SaveSystem.MaxManualSaveSlots} 个手动存档槽按钮和文本。当前配置可能不完整。");
        }

        if (autoSaveButton != null)
        {
            autoSaveButton.onClick.RemoveAllListeners();
            autoSaveButton.onClick.AddListener(OnAutoSaveButton);
        }

        for (int i = 0; i < slotButtons.Length; i++)
        {
            int slot = i + 1;
            if (slotButtons[i] != null)
            {
                slotButtons[i].onClick.RemoveAllListeners();
                slotButtons[i].onClick.AddListener(() => OnSlotButton(slot));
            }
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(OnBackButton);
        }
    }

    private void Start()
    {
        RefreshSlotInfo();
    }

    public void RefreshSlotInfo()
    {
        SaveSlotInfo autoInfo = SaveSystem.GetAutoSaveSlotInfo();
        if (autoSaveLabel != null)
        {
            autoSaveLabel.text = autoInfo.hasSave
                ? $"{autoInfo.displayText}\n{autoInfo.lastModified:yyyy-MM-dd HH:mm:ss}"
                : autoInfo.displayText;
        }

        if (autoSaveButton != null)
        {
            autoSaveButton.interactable = autoInfo.hasSave;
        }

        for (int i = 0; i < SaveSystem.MaxManualSaveSlots; i++)
        {
            int slot = i + 1;
            SaveSlotInfo info = SaveSystem.GetManualSaveSlotInfo(slot);
            string label = info.hasSave
                ? $"{info.displayText}\n{info.lastModified:yyyy-MM-dd HH:mm:ss}"
                : info.displayText;

            if (slotLabels != null && i < slotLabels.Length && slotLabels[i] != null)
            {
                slotLabels[i].text = label;
            }

            if (slotButtons != null && i < slotButtons.Length && slotButtons[i] != null)
            {
                slotButtons[i].interactable = info.hasSave;
            }
        }
    }

    public void OnSlotButton(int slot)
    {
        GameRunState loadedState = SaveSystem.LoadManualGame(slot);
        if (loadedState == null)
        {
            Debug.LogWarning($"存档槽 {slot} 无法加载。请检查存档是否存在或存档文件是否损坏。");
            RefreshSlotInfo();
            return;
        }

        GameSession.UpdateRunState(loadedState);
        GameSetupData.IsNewGame = false;
        SceneManager.LoadScene("MainScene");
    }

    public void OnAutoSaveButton()
    {
        GameRunState loadedState = SaveSystem.LoadAutoGame();
        if (loadedState == null)
        {
            Debug.LogWarning("自动档无法加载。请检查存档是否存在或存档文件是否损坏。");
            RefreshSlotInfo();
            return;
        }

        GameSession.UpdateRunState(loadedState);
        GameSetupData.IsNewGame = false;
        SceneManager.LoadScene("MainScene");
    }

    public void OnBackButton()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    private void EnsureAutoSaveControls()
    {
        if (autoSaveButton != null || slotButtons == null || slotButtons.Length == 0 || slotButtons[0] == null)
        {
            return;
        }

        autoSaveButton = Instantiate(slotButtons[0], slotButtons[0].transform.parent);
        autoSaveButton.name = "AutoSaveButton";
        autoSaveButton.transform.SetSiblingIndex(slotButtons[0].transform.GetSiblingIndex());

        RectTransform sourceRect = slotButtons[0].GetComponent<RectTransform>();
        RectTransform autoRect = autoSaveButton.GetComponent<RectTransform>();
        if (sourceRect != null && autoRect != null)
        {
            autoRect.anchoredPosition = sourceRect.anchoredPosition + new Vector2(0f, 70f);
        }

        autoSaveLabel = autoSaveButton.GetComponentInChildren<TMP_Text>();
    }
}
