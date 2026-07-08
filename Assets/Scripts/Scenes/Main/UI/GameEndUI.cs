using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

/// <summary>
/// 游戏结束结算面板。
/// 显示最终结果与结算详情，并提供返回主菜单按钮。
/// </summary>
public class GameEndUI : MonoBehaviour
{
    [Header("面板")]
    public GameObject panel;

    [Header("文本显示")]
    public TMP_Text titleText;
    public TMP_Text messageText;
    public TMP_Text detailText;

    [Header("按钮")]
    public Button returnToMainMenuButton;

    [Header("贴图")]
    public Sprite panelBackgroundSprite;
    public Sprite contentBackgroundSprite;
    public Sprite buttonBackgroundSprite;

    public bool IsOpen { get; private set; }

    void Awake()
    {
        if (panel == null)
        {
            CreateRuntimePanel();
        }
    }

    void Start()
    {
        if (panel != null)
            panel.SetActive(IsOpen);

        if (returnToMainMenuButton != null)
            returnToMainMenuButton.onClick.AddListener(OnReturnToMainMenu);
    }

    public void Open(string title, string message, string detail)
    {
        if (panel != null)
            panel.SetActive(true);

        if (titleText != null)
            titleText.text = "游戏结束";

        if (messageText != null)
            messageText.text = detail;

        if (detailText != null)
            detailText.text = string.Empty;

        IsOpen = true;
        Debug.Log("显示游戏结束结算面板");
    }

    public void Close()
    {
        if (panel != null)
            panel.SetActive(false);

        IsOpen = false;
        Debug.Log("关闭游戏结束结算面板");
    }

    void OnReturnToMainMenu()
    {
        Debug.Log("结算完成，返回主菜单");
        GameSession.ClearAll();
        SceneManager.LoadScene("MainMenuScene");
    }

    private void CreateRuntimePanel()
    {
        EnsureEventSystemExists();

        GameObject canvasObject = new GameObject("GameEndCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObject.AddComponent<GraphicRaycaster>();

        panel = new GameObject("GameEndPanel");
        panel.transform.SetParent(canvasObject.transform, false);
        Image panelImage = panel.AddComponent<Image>();
        if (panelBackgroundSprite != null)
        {
            panelImage.sprite = panelBackgroundSprite;
            panelImage.type = Image.Type.Sliced;
            panelImage.color = Color.white;
        }
        else
        {
            panelImage.color = new Color(0f, 0f, 0f, 0.8f);
        }
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // 内部大框
        GameObject content = new GameObject("Content");
        content.transform.SetParent(panel.transform, false);
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.pivot = new Vector2(0.5f, 0.5f);
        contentRect.sizeDelta = new Vector2(860f, 360f);

        Image contentImage = content.AddComponent<Image>();
        if (contentBackgroundSprite != null)
        {
            contentImage.sprite = contentBackgroundSprite;
            contentImage.type = Image.Type.Simple;
            contentImage.preserveAspect = true;
            contentImage.color = Color.white;
        }
        else
        {
            contentImage.color = new Color(0f, 0f, 0f, 0.7f);
        }

        // 标题和内容文本直接作为 Content 的子对象
        titleText = CreateText("TitleText", content.transform, 34, TextAlignmentOptions.Center);
        titleText.enableAutoSizing = true;
        titleText.fontSizeMin = 24;
        titleText.fontSizeMax = 36;
        titleText.text = "游戏结束";
        RectTransform titleRect = titleText.rectTransform;
        titleRect.anchorMin = new Vector2(0.08f, 0.70f);
        titleRect.anchorMax = new Vector2(0.92f, 0.84f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;

        messageText = CreateText("ContentText", content.transform, 26, TextAlignmentOptions.Center);
        messageText.enableAutoSizing = true;
        messageText.fontSizeMin = 16;
        messageText.fontSizeMax = 24;
        RectTransform messageRect = messageText.rectTransform;
        messageRect.anchorMin = new Vector2(0.08f, 0.16f);
        messageRect.anchorMax = new Vector2(0.92f, 0.70f);
        messageRect.offsetMin = Vector2.zero;
        messageRect.offsetMax = Vector2.zero;
        messageText.enableWordWrapping = true;
        messageText.alignment = TextAlignmentOptions.Center;

        detailText = CreateText("DetailText", content.transform, 22, TextAlignmentOptions.TopLeft);
        detailText.enableAutoSizing = true;
        detailText.fontSizeMin = 16;
        detailText.fontSizeMax = 22;
        RectTransform detailRect = detailText.rectTransform;
        detailRect.anchorMin = new Vector2(0.08f, 0.08f);
        detailRect.anchorMax = new Vector2(0.92f, 0.18f);
        detailRect.offsetMin = Vector2.zero;
        detailRect.offsetMax = Vector2.zero;
        detailText.enableWordWrapping = true;
        detailText.alignment = TextAlignmentOptions.TopLeft;
        detailText.text = string.Empty;

        // 右上角图标按钮
        GameObject iconButtonObject = new GameObject("CloseButton");
        iconButtonObject.transform.SetParent(panel.transform, false);
        Image iconButtonImage = iconButtonObject.AddComponent<Image>();
        if (buttonBackgroundSprite != null)
        {
            iconButtonImage.sprite = buttonBackgroundSprite;
            iconButtonImage.type = Image.Type.Simple;
            iconButtonImage.preserveAspect = true;
            iconButtonImage.color = Color.white;
            iconButtonImage.SetNativeSize();
        }
        else
        {
            iconButtonImage.color = new Color(0.8f, 0.2f, 0.2f, 1f);
        }
        returnToMainMenuButton = iconButtonObject.AddComponent<Button>();
        returnToMainMenuButton.onClick.AddListener(OnReturnToMainMenu);

        RectTransform iconRect = iconButtonObject.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(1f, 1f);
        iconRect.anchorMax = new Vector2(1f, 1f);
        iconRect.pivot = new Vector2(1f, 1f);
        iconRect.sizeDelta = new Vector2(48f, 48f);
        iconRect.anchoredPosition = new Vector2(-20f, -20f);

        if (returnToMainMenuButton != null)
            returnToMainMenuButton.onClick.AddListener(OnReturnToMainMenu);
    }

    private TMP_Text CreateText(string name, Transform parent, int fontSize, TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(0, 0);
        textObject.AddComponent<CanvasRenderer>();

        TMP_Text text = textObject.AddComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.enableWordWrapping = true;
        text.color = Color.white;
        text.text = string.Empty;
        text.rectTransform.sizeDelta = new Vector2(0, 0);
        return text;
    }

    private TMP_Text CreateText(string name, Transform parent, Vector2 anchor, int fontSize, TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.AddComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = anchor;
        rect.sizeDelta = new Vector2(0, 0);
        textObject.AddComponent<CanvasRenderer>();

        TMP_Text text = textObject.AddComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.enableWordWrapping = true;
        text.color = Color.white;
        text.text = string.Empty;
        text.rectTransform.sizeDelta = new Vector2(0, 0);
        return text;
    }

    private void EnsureEventSystemExists()
    {
        if (EventSystem.current != null) return;

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();
    }
}
