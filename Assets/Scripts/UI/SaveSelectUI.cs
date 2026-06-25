using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 存档槽选择场景逻辑，支持三个独立存档槽。
/// </summary>
public class SaveSelectUI : MonoBehaviour
{
    public Button[] slotButtons = new Button[SaveSystem.MaxSaveSlots];
    public TMP_Text[] slotLabels = new TMP_Text[SaveSystem.MaxSaveSlots];
    public Button backButton;

    private void Awake()
    {
        if (slotButtons.Length != SaveSystem.MaxSaveSlots || slotLabels.Length != SaveSystem.MaxSaveSlots)
        {
            Debug.LogWarning($"SaveSelectUI: 需要 {SaveSystem.MaxSaveSlots} 个存档槽按钮和文本。当前配置可能不完整。");
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
        for (int i = 0; i < SaveSystem.MaxSaveSlots; i++)
        {
            int slot = i + 1;
            SaveSlotInfo info = SaveSystem.GetSaveSlotInfo(slot);
            string label = info.hasSave
                ? $"槽{slot}: {info.displayText}\n{info.lastModified:yyyy-MM-dd HH:mm:ss}"
                : $"槽{slot}:  空";

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
        GameRunState loadedState = SaveSystem.LoadGame(slot);
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

    public void OnBackButton()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}
