using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 图片卡牌视图：把卡牌数据映射为兵种图片，选中时上浮弹出，支持翻面动画。
/// </summary>
public class CardView : MonoBehaviour
{
    [Header("卡牌图片")]
    public Image cardImage;

    [Header("可选选中遮罩")]
    public Image selectionOverlay;
    public Color selectedColor = new Color(1f, 0.9f, 0.4f, 0.45f);

    [Header("选中上浮动画")]
    public float selectedOffsetY = 25f;
    public float floatDuration = 0.15f;

    [Header("兵种图片")]
    public Sprite[] humanCardSprites;
    public Sprite[] heavenCardSprites;
    public Sprite[] ghostCardSprites;

    [Header("翻面")]
    public Sprite cardBackSprite;
    public float flipDuration = 0.5f;

    private Vector3 originalLocalPosition;
    private Vector3 originalLocalScale;
    private Quaternion originalLocalRotation;
    private Coroutine floatCoroutine;
    private Coroutine flipCoroutine;
    private Coroutine moveCoroutine;
    private CanvasGroup canvasGroup;

    private Sprite currentFrontSprite;
    private bool isShowingFront = true;
    private bool isFlipping;

    public Vector3 OriginalLocalPosition => originalLocalPosition;
    public Vector3 OriginalLocalScale => originalLocalScale;
    public Quaternion OriginalLocalRotation => originalLocalRotation;

    /// <summary>
    /// 更新卡牌"家"位置（居中汇集后调用）
    /// </summary>
    public void UpdateOriginalPosition(Vector3 newPosition)
    {
        originalLocalPosition = newPosition;
    }

    private void Awake()
    {
        originalLocalPosition = transform.localPosition;
        originalLocalScale = transform.localScale;
        originalLocalRotation = transform.localRotation;
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void SetCard(Card card)
    {
        if (card == null)
        {
            ClearCard();
            return;
        }

        Image target = GetTargetImage();
        if (target == null)
        {
            return;
        }

        Sprite frontSprite = GetSpriteForCard(card);
        currentFrontSprite = frontSprite;

        if (isShowingFront)
        {
            target.sprite = frontSprite;
        }

        target.enabled = true;
        target.preserveAspect = true;
    }

    public void SetCard(BattleCard battleCard)
    {
        SetCard(battleCard?.card);
    }

    public void ClearCard()
    {
        StopFlipCoroutine();
        isShowingFront = true;
        currentFrontSprite = null;

        Image target = GetTargetImage();
        if (target != null)
        {
            target.sprite = null;
            target.enabled = false;
        }

        Vector3 scale = transform.localScale;
        scale.x = 1f;
        transform.localScale = scale;

        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        if (floatCoroutine != null)
        {
            StopCoroutine(floatCoroutine);
            floatCoroutine = null;
        }

        Vector3 targetPosition = selected
            ? originalLocalPosition + Vector3.up * selectedOffsetY
            : originalLocalPosition;

        if (gameObject.activeInHierarchy)
        {
            floatCoroutine = StartCoroutine(FloatToPosition(targetPosition));
        }
        else
        {
            transform.localPosition = targetPosition;
        }

        if (selectionOverlay != null)
        {
            selectionOverlay.enabled = selected;
            if (selected)
            {
                selectionOverlay.color = selectedColor;
            }
        }
    }

    /// <summary>
    /// 翻到正面（卡背 → 兵种图片）
    /// </summary>
    public void FlipToFront()
    {
        if (isFlipping || isShowingFront) return;
        StopFlipCoroutine();
        if (gameObject.activeInHierarchy)
        {
            flipCoroutine = StartCoroutine(FlipRoutine(true));
        }
        else
        {
            SetFaceInstant(true);
        }
    }

    /// <summary>
    /// 翻到背面（兵种图片 → 卡背）
    /// </summary>
    public void FlipToBack()
    {
        if (isFlipping || !isShowingFront) return;
        StopFlipCoroutine();
        if (gameObject.activeInHierarchy)
        {
            flipCoroutine = StartCoroutine(FlipRoutine(false));
        }
        else
        {
            SetFaceInstant(false);
        }
    }

    /// <summary>
    /// 无动画直接切换正反面
    /// </summary>
    public void SetFaceInstant(bool showFront)
    {
        StopFlipCoroutine();
        isShowingFront = showFront;

        Vector3 scale = transform.localScale;
        scale.x = 1f;
        transform.localScale = scale;

        Image target = GetTargetImage();
        if (target != null)
        {
            target.sprite = showFront ? currentFrontSprite : cardBackSprite;
        }
    }

    /// <summary>
    /// 当前是否显示正面
    /// </summary>
    public bool IsShowingFront()
    {
        return isShowingFront;
    }

    public bool IsFlipping => isFlipping;

    /// <summary>
    /// 隐藏卡牌并停止所有动画
    /// </summary>
    public void HideCard()
    {
        StopFlipCoroutine();
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 设置透明度（0=全透明，1=不透明）
    /// </summary>
    public void SetAlpha(float alpha)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = alpha;
        }
    }

    /// <summary>
    /// 平滑移动到目标世界坐标
    /// </summary>
    public void MoveToWorldPosition(Vector3 targetPos, float duration)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }
        if (gameObject.activeInHierarchy)
        {
            moveCoroutine = StartCoroutine(MoveRoutine(targetPos, duration));
        }
        else
        {
            transform.position = targetPos;
        }
    }

    /// <summary>
    /// 平滑移动到目标世界坐标，同时插值缩放和旋转（用于出击动画）
    /// </summary>
    public IEnumerator MoveWithScaleAndRotation(Vector3 targetWorldPos, Vector3 targetLocalScale, Quaternion targetLocalRotation, float duration)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;
        Quaternion startRotation = transform.localRotation;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            transform.position = Vector3.Lerp(startPos, targetWorldPos, t);
            transform.localScale = Vector3.Lerp(startScale, targetLocalScale, t);
            transform.localRotation = Quaternion.Slerp(startRotation, targetLocalRotation, t);
            yield return null;
        }
        transform.position = targetWorldPos;
        transform.localScale = targetLocalScale;
        transform.localRotation = targetLocalRotation;
        moveCoroutine = null;
    }

    /// <summary>
    /// 平滑移动到目标本地坐标，同时插值缩放和旋转（用于返回卡槽动画）
    /// </summary>
    public IEnumerator MoveLocalWithScaleAndRotation(Vector3 targetLocalPos, Vector3 targetLocalScale, Quaternion targetLocalRotation, float duration)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        Vector3 startPos = transform.localPosition;
        Vector3 startScale = transform.localScale;
        Quaternion startRotation = transform.localRotation;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            transform.localPosition = Vector3.Lerp(startPos, targetLocalPos, t);
            transform.localScale = Vector3.Lerp(startScale, targetLocalScale, t);
            transform.localRotation = Quaternion.Slerp(startRotation, targetLocalRotation, t);
            yield return null;
        }
        transform.localPosition = targetLocalPos;
        transform.localScale = targetLocalScale;
        transform.localRotation = targetLocalRotation;
        moveCoroutine = null;
    }

    private IEnumerator MoveRoutine(Vector3 target, float duration)
    {
        Vector3 start = transform.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            transform.position = Vector3.Lerp(start, target, t);
            yield return null;
        }
        transform.position = target;
        moveCoroutine = null;
    }

    private IEnumerator FlipRoutine(bool toFront)
    {
        isFlipping = true;
        float halfDuration = flipDuration * 0.5f;

        // 前半段：scale.x 从 1 缩小到 0（卡牌侧身）
        float elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / halfDuration);
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Lerp(1f, 0f, t);
            transform.localScale = scale;
            yield return null;
        }

        // 中点：交换精灵
        Image target = GetTargetImage();
        if (target != null)
        {
            target.sprite = toFront ? currentFrontSprite : cardBackSprite;
        }
        isShowingFront = toFront;

        // 后半段：scale.x 从 0 恢复到 1（卡牌转正）
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / halfDuration);
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Lerp(0f, 1f, t);
            transform.localScale = scale;
            yield return null;
        }

        Vector3 finalScale = transform.localScale;
        finalScale.x = 1f;
        transform.localScale = finalScale;

        isFlipping = false;
        flipCoroutine = null;
    }

    private void StopFlipCoroutine()
    {
        if (flipCoroutine != null)
        {
            StopCoroutine(flipCoroutine);
            flipCoroutine = null;
        }
        isFlipping = false;
    }

    private IEnumerator FloatToPosition(Vector3 target)
    {
        Vector3 start = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < floatDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / floatDuration;
            transform.localPosition = Vector3.Lerp(start, target, t);
            yield return null;
        }

        transform.localPosition = target;
        floatCoroutine = null;
    }

    private Image GetTargetImage()
    {
        return cardImage != null ? cardImage : GetComponent<Image>();
    }

    private Sprite GetSpriteForCard(Card card)
    {
        Sprite[] sprites = card.race switch
        {
            RaceType.Human => humanCardSprites,
            RaceType.Heaven => heavenCardSprites,
            RaceType.Ghost => ghostCardSprites,
            _ => null,
        };

        if (sprites == null || card.unitIndex < 0 || card.unitIndex >= sprites.Length)
        {
            return null;
        }

        return sprites[card.unitIndex];
    }
}
