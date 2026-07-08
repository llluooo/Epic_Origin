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
    public Image highlightBorder;

    public Card cardData;
    public int cardIndex;

    public System.Action<GarrisonCardEntry> onClicked;

    void Start()
    {
        if (button != null)
            button.onClick.AddListener(() => onClicked?.Invoke(this));
    }

    public void Setup(Card card, int index, bool isHeroSlot)
    {
        cardData = card;
        cardIndex = index;

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
    }

    public void SetHighlighted(bool highlighted)
    {
        if (highlightBorder != null)
            highlightBorder.gameObject.SetActive(highlighted);
    }
}
