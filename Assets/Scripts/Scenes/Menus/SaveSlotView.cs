using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 存档槽位视图组件，卡牌式外观，与战斗场景暗金风格统一。
/// 支持悬停/选中动画、空槽/有存档/损坏三种状态显示。
/// </summary>
public class SaveSlotView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("视觉组件")]
    public Image backgroundImage;
    public Image frameImage;
    public Image iconImage;
    public Image selectionHighlight;

    [Header("文本")]
    public TMP_Text slotTitleText;
    public TMP_Text turnInfoText;
    public TMP_Text playerInfoText;
    public TMP_Text timestampText;
    public TMP_Text emptyText;

    [Header("状态覆盖")]
    public Image lockOverlay;
    public CanvasGroup canvasGroup;

    [Header("动画参数")]
    public float hoverAlpha = 0.95f;
    public float normalAlpha = 0.85f;
    public float hoverScale = 1.02f;
    public float normalScale = 1f;
    public float animDuration = 0.12f;
    public float selectedOffsetY = 8f;

    private int slotIndex;
    private bool isAutoSave;
    private bool hasSave;
    private bool allowSaveToEmpty;
    private SaveSlotInfo cachedInfo;
    private Vector3 originalLocalPos;
    private Coroutine animCoroutine;
    private Coroutine floatCoroutine;
    private bool isPointerInside;
    private bool isSelected;

    public System.Action<int, bool> onSlotClicked;

    private void Awake()
    {
        originalLocalPos = transform.localPosition;

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (selectionHighlight != null)
            selectionHighlight.enabled = false;
        if (lockOverlay != null)
            lockOverlay.gameObject.SetActive(false);
    }

    public void Setup(int index, bool isAuto, bool allowSave = false)
    {
        slotIndex = index;
        isAutoSave = isAuto;
        allowSaveToEmpty = allowSave;
    }

    public void Refresh(SaveSlotInfo info)
    {
        cachedInfo = info;
        hasSave = info != null && info.hasSave;

        if (slotTitleText != null)
            slotTitleText.text = isAutoSave ? "自动存档" : $"存档 {slotIndex}";

        if (emptyText != null)
            emptyText.gameObject.SetActive(!hasSave);

        if (!hasSave)
        {
            SetEmptyState();
            return;
        }

        SetFilledState(info);
    }

    void SetEmptyState()
    {
        if (turnInfoText != null) turnInfoText.gameObject.SetActive(false);
        if (playerInfoText != null) playerInfoText.gameObject.SetActive(false);
        if (timestampText != null) timestampText.gameObject.SetActive(false);
        if (iconImage != null) iconImage.enabled = false;

        if (backgroundImage != null)
            backgroundImage.color = UITheme.SlotEmptyBg;
    }

    void SetFilledState(SaveSlotInfo info)
    {
        if (turnInfoText != null)
        {
            turnInfoText.gameObject.SetActive(true);
            turnInfoText.text = info.displayText ?? "";
        }

        if (playerInfoText != null)
        {
            playerInfoText.gameObject.SetActive(true);
            playerInfoText.text = "";
        }

        if (timestampText != null)
        {
            timestampText.gameObject.SetActive(true);
            timestampText.text = info.lastModified != System.DateTime.MinValue
                ? info.lastModified.ToString("yyyy-MM-dd HH:mm")
                : "";
        }

        if (iconImage != null)
            iconImage.enabled = true;

        if (backgroundImage != null)
            backgroundImage.color = UITheme.SlotHoverBg;
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;

        if (selectionHighlight != null)
        {
            selectionHighlight.enabled = selected;
            if (selected)
                selectionHighlight.color = UITheme.AccentGold;
        }

        if (gameObject.activeInHierarchy)
        {
            if (floatCoroutine != null)
                StopCoroutine(floatCoroutine);

            Vector3 target = selected
                ? originalLocalPos + Vector3.up * selectedOffsetY
                : originalLocalPos;
            floatCoroutine = StartCoroutine(FloatTo(target));
        }
    }

    public void PlayEnterAnimation(float delay)
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        Vector3 targetPos = originalLocalPos;
        transform.localPosition = targetPos + Vector3.down * 30f;

        StartCoroutine(EnterRoutine(delay, targetPos));
    }

    public void SetInteractable(bool interactable)
    {
        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = interactable;

        if (lockOverlay != null)
            lockOverlay.gameObject.SetActive(!interactable);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerInside = true;
        AnimateTo(hoverAlpha, hoverScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerInside = false;
        AnimateTo(normalAlpha, normalScale);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!hasSave && !allowSaveToEmpty) return;

        AnimateTo(1f, UITheme.ClickScale);
        StartCoroutine(RestoreScaleAfterClick());

        onSlotClicked?.Invoke(slotIndex, isAutoSave);
    }

    void AnimateTo(float targetAlpha, float targetScale)
    {
        if (animCoroutine != null)
            StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(AnimateRoutine(targetAlpha, targetScale));
    }

    IEnumerator AnimateRoutine(float targetAlpha, float targetScale)
    {
        float startAlpha = canvasGroup != null ? canvasGroup.alpha : normalAlpha;
        float startScale = transform.localScale.x;
        float elapsed = 0f;

        while (elapsed < animDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / animDuration);
            t = 1f - Mathf.Pow(1f - t, 3f);

            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);

            float s = Mathf.Lerp(startScale, targetScale, t);
            transform.localScale = new Vector3(s, s, 1f);

            yield return null;
        }

        if (canvasGroup != null)
            canvasGroup.alpha = targetAlpha;

        transform.localScale = new Vector3(targetScale, targetScale, 1f);
        animCoroutine = null;
    }

    IEnumerator FloatTo(Vector3 target)
    {
        Vector3 start = transform.localPosition;
        float elapsed = 0f;
        float duration = UITheme.SelectedFloatDuration;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.localPosition = Vector3.Lerp(start, target, t);
            yield return null;
        }

        transform.localPosition = target;
        floatCoroutine = null;
    }

    IEnumerator EnterRoutine(float delay, Vector3 targetPos)
    {
        yield return new WaitForSecondsRealtime(delay);

        Vector3 startPos = transform.localPosition;
        float startAlpha = canvasGroup != null ? canvasGroup.alpha : 0f;
        float elapsed = 0f;
        float duration = UITheme.SlotEnterDuration;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));

            transform.localPosition = Vector3.Lerp(startPos, targetPos, t);

            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(startAlpha, normalAlpha, t);

            yield return null;
        }

        transform.localPosition = targetPos;
        if (canvasGroup != null)
            canvasGroup.alpha = normalAlpha;
    }

    IEnumerator RestoreScaleAfterClick()
    {
        yield return new WaitForSecondsRealtime(UITheme.ClickRestoreDuration);
        AnimateTo(isPointerInside ? hoverAlpha : normalAlpha, isPointerInside ? hoverScale : normalScale);
    }
}
