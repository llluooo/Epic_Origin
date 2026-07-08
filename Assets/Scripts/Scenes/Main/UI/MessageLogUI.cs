using System.Collections.Generic;
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
            text = text,
            remainingTime = messageDuration
        };
        messages.Add(entry);

        if (messages.Count > maxMessages)
            messages.RemoveAt(0);

        RefreshDisplay();
    }

    void RefreshDisplay()
    {
        if (messageText == null) return;

        if (messages.Count == 0)
        {
            messageText.text = "";
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
