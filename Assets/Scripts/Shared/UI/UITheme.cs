using UnityEngine;

/// <summary>
/// 全局UI视觉风格常量，统一战斗/存档/据点等界面的配色与动画参数。
/// </summary>
public static class UITheme
{
    // 暗金配色
    public static readonly Color DarkGold = new Color(0.76f, 0.62f, 0.35f);
    public static readonly Color DarkGoldDim = new Color(0.55f, 0.45f, 0.25f);
    public static readonly Color DarkGoldBright = new Color(0.92f, 0.78f, 0.42f);
    public static readonly Color PanelBgDark = new Color(0.12f, 0.10f, 0.08f, 0.92f);
    public static readonly Color PanelBgMedium = new Color(0.18f, 0.15f, 0.12f, 0.88f);
    public static readonly Color SlotEmptyBg = new Color(0.15f, 0.13f, 0.10f, 0.85f);
    public static readonly Color SlotHoverBg = new Color(0.22f, 0.18f, 0.13f, 0.95f);
    public static readonly Color TextPrimary = new Color(0.92f, 0.85f, 0.68f);
    public static readonly Color TextSecondary = new Color(0.68f, 0.60f, 0.42f);
    public static readonly Color TextDim = new Color(0.45f, 0.40f, 0.30f);
    public static readonly Color AccentGold = new Color(1f, 0.9f, 0.4f, 0.45f);
    public static readonly Color AccentRed = new Color(0.85f, 0.25f, 0.2f);
    public static readonly Color AccentGreen = new Color(0.3f, 0.75f, 0.35f);

    // 动画参数
    public const float HoverFadeDuration = 0.12f;
    public const float ClickScale = 0.95f;
    public const float ClickRestoreDuration = 0.1f;
    public const float SelectedFloatOffsetY = 8f;
    public const float SelectedFloatDuration = 0.15f;
    public const float PanelFadeInDuration = 0.25f;
    public const float PanelFadeOutDuration = 0.2f;
    public const float SlotEnterStaggerDelay = 0.08f;
    public const float SlotEnterDuration = 0.3f;

    public static readonly string FrameSpritePath = "Art/UI/Frames/ui_map_frame_dark_gold_9slice";
}
