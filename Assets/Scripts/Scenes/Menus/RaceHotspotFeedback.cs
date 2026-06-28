using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 种族地图热点的轻量悬停与点击反馈。
/// </summary>
public class RaceHotspotFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("视觉")]
    public Graphic highlightGraphic;
    public RectTransform animatedTarget;

    [Header("透明度")]
    [Range(0f, 1f)] public float hiddenAlpha = 0f;
    [Range(0f, 1f)] public float hoverAlpha = 0.28f;
    [Range(0f, 1f)] public float pressedAlpha = 0.45f;
    [Range(0f, 1f)] public float clickAlpha = 0.65f;

    [Header("缩放")]
    public float normalScale = 1f;
    public float hoverScale = 1.015f;
    public float pressedScale = 0.985f;
    public float clickScale = 1.035f;

    [Header("时间")]
    public float fadeDuration = 0.12f;
    public float clickDuration = 0.22f;

    private Coroutine animateRoutine;
    private bool isPointerInside;
    private bool isPressed;

    public Graphic HighlightGraphic
    {
        get
        {
            ResolveReferences();
            return highlightGraphic;
        }
    }

    void Awake()
    {
        ResolveReferences();

        SetVisual(hiddenAlpha, normalScale);
    }

    void OnValidate()
    {
        ResolveReferences();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerInside = true;
        AnimateTo(hoverAlpha, hoverScale, fadeDuration);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerInside = false;

        if (!isPressed)
            AnimateTo(hiddenAlpha, normalScale, fadeDuration);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        AnimateTo(pressedAlpha, pressedScale, fadeDuration);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;

        float targetAlpha = isPointerInside ? hoverAlpha : hiddenAlpha;
        float targetScale = isPointerInside ? hoverScale : normalScale;
        AnimateTo(targetAlpha, targetScale, fadeDuration);
    }

    public float PlayClickFeedback()
    {
        AnimateTo(clickAlpha, clickScale, clickDuration * 0.5f);
        return clickDuration;
    }

    private void AnimateTo(float targetAlpha, float targetScale, float duration)
    {
        if (animateRoutine != null)
            StopCoroutine(animateRoutine);

        animateRoutine = StartCoroutine(AnimateRoutine(targetAlpha, targetScale, Mathf.Max(0.01f, duration)));
    }

    private IEnumerator AnimateRoutine(float targetAlpha, float targetScale, float duration)
    {
        float startAlpha = GetCurrentAlpha();
        float startScale = animatedTarget != null ? animatedTarget.localScale.x : normalScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = 1f - Mathf.Pow(1f - t, 3f);

            SetVisual(Mathf.Lerp(startAlpha, targetAlpha, t), Mathf.Lerp(startScale, targetScale, t));
            yield return null;
        }

        SetVisual(targetAlpha, targetScale);
        animateRoutine = null;
    }

    private float GetCurrentAlpha()
    {
        return highlightGraphic != null ? highlightGraphic.color.a : hiddenAlpha;
    }

    private void SetVisual(float alpha, float scale)
    {
        if (highlightGraphic != null)
        {
            if (!highlightGraphic.gameObject.activeSelf)
                highlightGraphic.gameObject.SetActive(true);

            highlightGraphic.raycastTarget = false;
            Color color = highlightGraphic.color;
            color.a = alpha;
            highlightGraphic.color = color;
        }

        if (animatedTarget != null)
            animatedTarget.localScale = Vector3.one * scale;
    }

    private void ResolveReferences()
    {
        if (highlightGraphic == null)
        {
            Graphic[] childGraphics = GetComponentsInChildren<Graphic>(true);
            for (int i = 0; i < childGraphics.Length; i++)
            {
                if (childGraphics[i].gameObject != gameObject)
                {
                    highlightGraphic = childGraphics[i];
                    break;
                }
            }
        }

        if (animatedTarget == null)
        {
            animatedTarget = highlightGraphic != null
                ? highlightGraphic.rectTransform
                : transform as RectTransform;
        }
    }
}
