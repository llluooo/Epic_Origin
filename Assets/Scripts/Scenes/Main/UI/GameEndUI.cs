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
            titleText.text = title;
        if (messageText != null)
            messageText.text = message;
        if (detailText != null)
            detailText.text = detail;

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
        panelImage.color = new Color(0f, 0f, 0f, 0.8f);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.1f, 0.2f);
        panelRect.anchorMax = new Vector2(0.9f, 0.8f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        // 使用垂直布局组织文本和按钮，保证在不同分辨率下居中且不变形
        GameObject content = new GameObject("Content");
        content.transform.SetParent(panel.transform, false);
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 0f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.offsetMin = new Vector2(20f, 20f);
        contentRect.offsetMax = new Vector2(-20f, -20f);

        VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.spacing = 12f;
        vlg.padding = new RectOffset(8, 8, 8, 8);
        vlg.childControlHeight = true;
        vlg.childControlWidth = true;

        ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

        // 标题
        titleText = CreateText("TitleText", content.transform, 36, TextAlignmentOptions.Center);
        titleText.enableAutoSizing = true;
        titleText.fontSizeMin = 24;
        titleText.fontSizeMax = 48;

        // 主消息
        messageText = CreateText("MessageText", content.transform, 28, TextAlignmentOptions.Center);
        messageText.enableAutoSizing = true;
        messageText.fontSizeMin = 18;
        messageText.fontSizeMax = 36;

        // 详情（多行）
        detailText = CreateText("DetailText", content.transform, 20, TextAlignmentOptions.Left);
        detailText.enableAutoSizing = true;
        detailText.fontSizeMin = 14;
        detailText.fontSizeMax = 24;
        detailText.alignment = TextAlignmentOptions.TopLeft;

        // 按钮（放在内容底部）
        GameObject buttonObject = new GameObject("ReturnButton");
        buttonObject.transform.SetParent(content.transform, false);
        Image buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.color = new Color(0.15f, 0.5f, 0.95f, 1f);
        returnToMainMenuButton = buttonObject.AddComponent<Button>();

        LayoutElement le = buttonObject.AddComponent<LayoutElement>();
        le.preferredHeight = 52f;
        le.minHeight = 40f;

        TMP_Text buttonText = CreateText("ReturnButtonText", buttonObject.transform, 26, TextAlignmentOptions.Center);
        buttonText.text = "返回主菜单";
        buttonText.color = Color.white;

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
