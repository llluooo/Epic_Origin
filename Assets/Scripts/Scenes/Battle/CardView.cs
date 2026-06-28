using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 图片卡牌视图：把卡牌数据映射为兵种图片，选中时上浮弹出。
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

    private Vector3 originalLocalPosition;
    private Coroutine floatCoroutine;

    private void Awake()
    {
        originalLocalPosition = transform.localPosition;
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

        target.sprite = GetSpriteForCard(card);
        target.enabled = true;
        target.preserveAspect = true;
    }

    public void SetCard(BattleCard battleCard)
    {
        SetCard(battleCard?.card);
    }

    public void ClearCard()
    {
        Image target = GetTargetImage();
        if (target != null)
        {
            target.sprite = null;
            target.enabled = false;
        }

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
