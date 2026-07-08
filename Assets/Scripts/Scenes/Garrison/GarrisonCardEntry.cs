using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 据点兵力管理场景的入口组件，负责选中和转移交互。
/// 每个入口可表示英雄卡槽或驻军卡牌。
/// </summary>
public class GarrisonCardEntry : MonoBehaviour
{
    public Button button;
    public TMP_Text nameText;
    public TMP_Text raceText;
    public TMP_Text statsText;
    public Image cardImage;
    public Image highlightBorder;

    public Card cardData;
    public int cardIndex;

    public System.Action<GarrisonCardEntry> onClicked;

    void Awake()
    {
        // 自动绑定：如果 Inspector 未赋值，按子节点名称查找
        if (button == null) button = GetComponent<Button>();
        if (nameText == null) nameText = FindChildText("NameText");
        if (raceText == null) raceText = FindChildText("RaceText");
        if (statsText == null) statsText = FindChildText("StatsText");
        if (cardImage == null) cardImage = FindChildImage("CardImage");
        if (highlightBorder == null) highlightBorder = FindChildImage("HighlightBorder");
    }

    void Start()
    {
        if (button != null)
            button.onClick.AddListener(() => onClicked?.Invoke(this));
    }

    private TMP_Text FindChildText(string childName)
    {
        Transform child = transform.Find(childName);
        return child != null ? child.GetComponent<TMP_Text>() : null;
    }

    private Image FindChildImage(string childName)
    {
        Transform child = transform.Find(childName);
        return child != null ? child.GetComponent<Image>() : null;
    }

    public void Setup(Card card, int index, bool isHeroSlot, Sprite cardSprite = null)
    {
        cardData = card;
        cardIndex = index;

        if (card == null)
        {
            if (nameText != null)
                nameText.text = isHeroSlot ? "空槽位" : "";
            if (raceText != null)
                raceText.text = "";
            if (statsText != null)
                statsText.text = "";
            if (cardImage != null)
                cardImage.gameObject.SetActive(false);
            return;
        }

        if (nameText != null)
            nameText.text = $"{card.cardName}  Lv{card.level}";
        if (raceText != null)
            raceText.text = card.race switch
            {
                RaceType.Human => "人族",
                RaceType.Heaven => "天族",
                RaceType.Ghost => "鬼族",
                _ => "???"
            };
        if (statsText != null)
            statsText.text = $"攻击:{card.GetAttack()}  生命:{card.GetMaxHP()}  ×{card.quantity}";
        if (cardImage != null && cardSprite != null)
        {
            cardImage.sprite = cardSprite;
            cardImage.gameObject.SetActive(true);
        }
    }

    public void SetHighlighted(bool highlighted)
    {
        if (highlightBorder != null)
            highlightBorder.gameObject.SetActive(highlighted);
    }
}
