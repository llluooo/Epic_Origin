using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 设置面板：音量调节和 AI 难度切换。
/// </summary>
public class SettingsUI : MonoBehaviour
{
    [Header("面板")]
    public GameObject panel;
    public Image panelBackground;
    public CanvasGroup panelCanvasGroup;

    [Header("音量")]
    public Slider volumeSlider;
    public TMP_Text volumeValueText;

    [Header("AI难度")]
    public Button difficultyButton;
    public TMP_Text difficultyLabel;

    [Header("按钮")]
    public Button backButton;
    public TMP_Text backButtonText;

    private bool isInitializing;
    private bool isInitialized;
    private bool skipStartClose;

    private static readonly string[] difficultyNames = { "简单", "困难" };
    private static readonly AIDifficulty[] difficultyValues = { AIDifficulty.Easy, AIDifficulty.Hard };

    void Start()
    {
        bool shouldCloseOnStart = !isInitialized && !skipStartClose;
        Initialize();

        if (shouldCloseOnStart && panel != null)
            panel.SetActive(false);
    }

    public static SettingsUI CreateRuntimeSettingsUI(Transform parent)
    {
        GameObject panelObject = new GameObject("SettingsPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
        panelObject.transform.SetParent(parent, false);

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image background = panelObject.GetComponent<Image>();
        background.color = new Color(0.12f, 0.1f, 0.08f, 0.88f);

        SettingsUI settingsUI = panelObject.AddComponent<SettingsUI>();
        settingsUI.skipStartClose = true;
        settingsUI.panel = panelObject;
        settingsUI.panelBackground = background;
        settingsUI.panelCanvasGroup = panelObject.GetComponent<CanvasGroup>();

        TMP_Text titleText = CreateLabel(panelRect, "设置", new Vector2(0f, 220f), new Vector2(320f, 64f), 40);
        titleText.fontStyle = FontStyles.Bold;

        CreateLabel(panelRect, "音量", new Vector2(-300f, 110f), new Vector2(160f, 48f), 30);
        settingsUI.volumeSlider = CreateSlider(panelRect, new Vector2(0f, 110f), new Vector2(430f, 30f));
        settingsUI.volumeValueText = CreateLabel(panelRect, "100%", new Vector2(300f, 110f), new Vector2(140f, 48f), 26);

        settingsUI.difficultyButton = CreateButton(panelRect, "AI难度: 简单", new Vector2(0f, 0f), new Vector2(300f, 74f), out TMP_Text difficultyText);
        settingsUI.difficultyLabel = difficultyText;

        settingsUI.backButton = CreateButton(panelRect, "返回", new Vector2(0f, -130f), new Vector2(260f, 70f), out TMP_Text backText);
        settingsUI.backButtonText = backText;

        settingsUI.Initialize();
        settingsUI.Close();
        return settingsUI;
    }

    public void Open()
    {
        Initialize();
        EnsurePanel();

        if (panel != null)
            panel.SetActive(true);

        isInitializing = true;

        AudioManager audioManager = AudioManager.Instance;
        if (audioManager != null && volumeSlider != null)
            volumeSlider.value = audioManager.GetMasterVolume();

        UpdateVolumeLabel();
        UpdateDifficultyLabel();
        isInitializing = false;
    }

    public void Close()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    private void Initialize()
    {
        if (isInitialized)
            return;

        EnsurePanel();

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(Close);
        }

        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        if (difficultyButton != null)
        {
            difficultyButton.onClick.RemoveAllListeners();
            difficultyButton.onClick.AddListener(OnDifficultyClicked);
        }

        isInitialized = true;
    }

    private void EnsurePanel()
    {
        if (panel == null)
            panel = gameObject;
    }

    private void OnVolumeChanged(float value)
    {
        if (!isInitializing && AudioManager.Instance != null)
            AudioManager.Instance.SetMasterVolume(value);

        UpdateVolumeLabel();
    }

    private void OnDifficultyClicked()
    {
        if (isInitializing)
            return;

        AIDifficulty current = AIDifficultySelector.CurrentDifficulty;
        int nextIndex = 0;
        for (int i = 0; i < difficultyValues.Length; i++)
        {
            if (difficultyValues[i] == current)
            {
                nextIndex = (i + 1) % difficultyValues.Length;
                break;
            }
        }

        AIDifficultySelector.SetDifficulty(difficultyValues[nextIndex]);
        UpdateDifficultyLabel();
    }

    private void UpdateVolumeLabel()
    {
        if (volumeValueText != null && volumeSlider != null)
            volumeValueText.text = Mathf.RoundToInt(volumeSlider.value * 100f) + "%";
    }

    private void UpdateDifficultyLabel()
    {
        if (difficultyLabel == null)
            return;

        AIDifficulty current = AIDifficultySelector.CurrentDifficulty;
        int index = 0;
        for (int i = 0; i < difficultyValues.Length; i++)
        {
            if (difficultyValues[i] == current)
            {
                index = i;
                break;
            }
        }

        difficultyLabel.text = "AI难度: " + difficultyNames[index];
    }

    private static TMP_Text CreateLabel(RectTransform parent, string text, Vector2 anchoredPosition, Vector2 size, int fontSize)
    {
        GameObject textObject = new GameObject(text, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        TMP_Text label = textObject.GetComponent<TMP_Text>();
        label.text = text;
        label.fontSize = fontSize;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        return label;
    }

    private static Button CreateButton(RectTransform parent, string text, Vector2 anchoredPosition, Vector2 size, out TMP_Text label)
    {
        GameObject buttonObject = new GameObject(text, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.72f, 0.52f, 0.24f, 1f);

        label = CreateLabel(rect, text, Vector2.zero, size, 28);
        label.raycastTarget = false;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        return button;
    }

    private static Slider CreateSlider(RectTransform parent, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject sliderObject = new GameObject("VolumeSlider", typeof(RectTransform), typeof(Slider));
        sliderObject.transform.SetParent(parent, false);

        RectTransform sliderRect = sliderObject.GetComponent<RectTransform>();
        sliderRect.anchoredPosition = anchoredPosition;
        sliderRect.sizeDelta = size;

        RectTransform backgroundRect = CreateImage(sliderRect, "Background", new Color(0.16f, 0.13f, 0.09f, 1f), Vector2.zero, size).rectTransform;
        RectTransform fillArea = CreateRect(sliderRect, "Fill Area", Vector2.zero, size - new Vector2(20f, 0f));
        RectTransform fillRect = CreateImage(fillArea, "Fill", new Color(0.92f, 0.72f, 0.42f, 1f), Vector2.zero, size - new Vector2(20f, 10f)).rectTransform;
        RectTransform handleArea = CreateRect(sliderRect, "Handle Slide Area", Vector2.zero, size);
        RectTransform handleRect = CreateImage(handleArea, "Handle", Color.white, Vector2.zero, new Vector2(22f, 42f)).rectTransform;

        Slider slider = sliderObject.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        slider.targetGraphic = handleRect.GetComponent<Image>();
        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.direction = Slider.Direction.LeftToRight;

        backgroundRect.SetAsFirstSibling();
        return slider;
    }

    private static RectTransform CreateRect(RectTransform parent, string name, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject rectObject = new GameObject(name, typeof(RectTransform));
        rectObject.transform.SetParent(parent, false);

        RectTransform rect = rectObject.GetComponent<RectTransform>();
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        return rect;
    }

    private static Image CreateImage(RectTransform parent, string name, Color color, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(parent, false);

        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        return image;
    }
}
