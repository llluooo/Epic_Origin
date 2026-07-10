using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using TMPro;

/// <summary>
/// 大地图HUD消息反馈区域。显示英雄在地图上操作后的反馈文本，
/// 支持多条消息滚动显示，旧消息自动淡出消失。
/// </summary>
public class MessageLogUI : MonoBehaviour
{
    public static MessageLogUI Instance;

    [Header("消息显示")]
    public TMP_Text messageText;
    public int maxMessages = 5;
    public float messageDuration = 4f;

    [Header("布局宽度")]
    [SerializeField] private Vector2 containerAnchorMin = new Vector2(0f, 0.80f);
    [SerializeField] private Vector2 containerAnchorMax = new Vector2(1f, 0.98f);
    [SerializeField] private Vector2 containerOffsetMin = new Vector2(40f, 8f);
    [SerializeField] private Vector2 containerOffsetMax = new Vector2(-40f, -8f);

    private readonly List<MessageEntry> messages = new List<MessageEntry>();

    private struct MessageEntry
    {
        public string text;
        public float remainingTime;
    }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        ApplyLayout();
    }

    private void OnEnable()
    {
        // 初始时隐藏文本对象，避免编辑器默认文本（如 "New Text"）闪现
        if (messageText != null)
            messageText.gameObject.SetActive(false);
    }

    private void OnValidate()
    {
        ApplyLayout();
    }

    private void ApplyLayout()
    {
        if (messageText == null) return;

        RectTransform rect = messageText.rectTransform;
        rect.anchorMin = containerAnchorMin;
        rect.anchorMax = containerAnchorMax;
        rect.offsetMin = containerOffsetMin;
        rect.offsetMax = containerOffsetMax;
        rect.pivot = new Vector2(0.5f, 1f);

        // 强制设置宽度为屏幕宽度减去左右内边距，避免被父容器限制得过窄
        float horizontalPadding = Mathf.Abs(containerOffsetMin.x) + Mathf.Abs(containerOffsetMax.x);
        float targetWidth = Screen.width - horizontalPadding;
        if (targetWidth > 0)
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
    }

    void Update()
    {
        if (messages.Count == 0) return;

        float dt = Time.deltaTime;
        for (int i = messages.Count - 1; i >= 0; i--)
        {
            var entry = messages[i];
            entry.remainingTime -= dt;
            messages[i] = entry;

            if (entry.remainingTime <= 0f)
                messages.RemoveAt(i);
        }

        RefreshDisplay();
    }

    /// <summary>
    /// 添加一条反馈消息到HUD。
    /// </summary>
    public void AddMessage(string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        var entry = new MessageEntry
        {
            text = FormatMessage(text),
            remainingTime = messageDuration
        };
        messages.Add(entry);

        if (messages.Count > maxMessages)
            messages.RemoveAt(0);

        // 确保消息框在有消息时可见
        if (messageText != null && !messageText.gameObject.activeSelf)
            messageText.gameObject.SetActive(true);

        RefreshDisplay();
    }

    private string FormatMessage(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        // 如果包含冒号（中英文），在冒号处换行：标签在上，数值/详情在下
        var m = Regex.Match(text, "^(.+?[:：])\\s*(.+)$");
        if (m.Success) return m.Groups[1].Value + "\n" + m.Groups[2].Value;

        // 如果没有冒号，尝试在第一个数字之前换行（例如："获得资源 51金币"）
        m = Regex.Match(text, "^(.+?)\\s+([+\\-]?\\d[\\d,]*.*)$");
        if (m.Success) return m.Groups[1].Value + "\n" + m.Groups[2].Value;

        return text;
    }

    void RefreshDisplay()
    {
        if (messageText == null) return;

        if (messages.Count == 0)
        {
            messageText.text = "";
            // 没有消息时隐藏文本对象，防止编辑器或默认文案显示
            if (messageText.gameObject.activeSelf)
                messageText.gameObject.SetActive(false);
            return;
        }

        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < messages.Count; i++)
        {
            if (i > 0) sb.AppendLine();
            sb.Append(messages[i].text);
        }
        messageText.text = sb.ToString();
    }
}
