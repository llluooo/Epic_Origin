using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 卡牌 UI 组件：将卡牌数据渲染成带边框、头像、种族、攻击力、生命值的卡牌面板。
/// </summary>
public class CardView : MonoBehaviour
{
    [Header("卡牌边框")]
    public Image cardFrame;
    public Sprite cardFrameSprite;
    public Image iconImage;
    public Image attackIconImage;
    public Image hpIconImage;
    public Image selectionOverlay;
    public Color selectedColor = new Color(1f, 0.9f, 0.4f, 0.5f);

    [Header("卡牌文本")]
    public TMP_Text titleText;
    public TMP_Text raceText;
    public TMP_Text levelText;
    public TMP_Text attackText;
    public TMP_Text hpText;
    public TMP_Text quantityText;

    [Header("卡牌贴图")]
    public Sprite[] humanCardSprites;
    public Sprite[] heavenCardSprites;
    public Sprite[] ghostCardSprites;

    [Header("可选种族颜色")]
    public Color humanFrameColor = new Color(0.5f, 0.7f, 1f);
    public Color heavenFrameColor = new Color(1f, 0.9f, 0.5f);
    public Color ghostFrameColor = new Color(0.8f, 0.6f, 1f);

    public void SetCard(Card card)
    {
        if (card == null)
        {
            ClearCard();
            return;
        }

        if (iconImage != null)
            iconImage.sprite = GetSpriteForCard(card);

        if (titleText != null)
            titleText.text = string.IsNullOrEmpty(card.cardName) ? $"Lv{card.level} 卡牌" : card.cardName;

        if (raceText != null)
            raceText.text = GetRaceDisplayName(card.race);

        if (levelText != null)
            levelText.text = $"Lv{card.level}";

        if (attackText != null)
            attackText.text = card.GetAttack().ToString();

        if (hpText != null)
            hpText.text = card.GetMaxHP().ToString();

        if (quantityText != null)
            quantityText.text = card.quantity > 1 ? $"x{card.quantity}" : string.Empty;

        if (cardFrame != null && cardFrameSprite != null)
            cardFrame.sprite = cardFrameSprite;

        SetFrameColor(card.race);
    }

    public void SetCard(BattleCard battleCard)
    {
        if (battleCard == null || battleCard.card == null)
        {
            ClearCard();
            return;
        }

        SetCard(battleCard.card);

        if (quantityText != null)
            quantityText.text = battleCard.currentCount > 1 ? $"x{battleCard.currentCount}" : string.Empty;
    }

    public void ClearCard()
    {
        if (iconImage != null)
            iconImage.sprite = null;

        if (titleText != null)
            titleText.text = "(空)";

        if (raceText != null)
            raceText.text = string.Empty;

        if (levelText != null)
            levelText.text = string.Empty;

        if (attackText != null)
            attackText.text = string.Empty;

        if (hpText != null)
            hpText.text = string.Empty;

        if (quantityText != null)
            quantityText.text = string.Empty;

        if (attackIconImage != null)
            attackIconImage.enabled = false;

        if (hpIconImage != null)
            hpIconImage.enabled = false;

        if (cardFrame != null)
        {
            cardFrame.color = Color.white;
            if (cardFrameSprite != null)
                cardFrame.sprite = cardFrameSprite;
        }

        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        if (selectionOverlay == null)
            return;

        selectionOverlay.enabled = selected;
        if (selected)
            selectionOverlay.color = selectedColor;
    }

    private void SetFrameColor(RaceType race)
    {
        if (cardFrame == null)
            return;

        cardFrame.color = race switch
        {
            RaceType.Human => humanFrameColor,
            RaceType.Heaven => heavenFrameColor,
            RaceType.Ghost => ghostFrameColor,
            _ => Color.white,
        };
    }

    private string GetRaceDisplayName(RaceType race)
    {
        return race switch
        {
            RaceType.Human => "人族",
            RaceType.Heaven => "天族",
            RaceType.Ghost => "鬼族",
            _ => race.ToString(),
        };
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
            return null;

        return sprites[card.unitIndex];
    }
}
