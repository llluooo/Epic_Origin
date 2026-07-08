using UnityEngine;

/// <summary>
/// 存储每个种族 × 兵种等级(Lv1-5)对应的卡牌精灵图，供 Garrison 场景等查询使用。
/// </summary>
[CreateAssetMenu(menuName = "Epic Origin/Card Sprite Config")]
public class CardSpriteConfig : ScriptableObject
{
    [Header("人族 (Human)")]
    public Sprite[] humanSprites = new Sprite[5];

    [Header("天族 (Heaven)")]
    public Sprite[] heavenSprites = new Sprite[5];

    [Header("鬼族 (Ghost)")]
    public Sprite[] ghostSprites = new Sprite[5];

    public Sprite GetSprite(RaceType race, int unitIndex)
    {
        if (unitIndex < 0 || unitIndex >= 5)
        {
            Debug.LogWarning($"[CardSpriteConfig] unitIndex 越界: {unitIndex}");
            return null;
        }

        Sprite[] sprites = race switch
        {
            RaceType.Human => humanSprites,
            RaceType.Heaven => heavenSprites,
            RaceType.Ghost => ghostSprites,
            _ => null
        };

        if (sprites == null)
        {
            Debug.LogWarning($"[CardSpriteConfig] 未知种族: {race}");
            return null;
        }

        return sprites[unitIndex];
    }
}
