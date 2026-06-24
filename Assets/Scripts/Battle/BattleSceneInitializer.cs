using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 创建 BattleManager、初始化卡组并将实例分配给 BattleUI。
/// </summary>
public class BattleSceneInitializer : MonoBehaviour
{
    public BattleUI battleUI;
    public bool playerStarts = true;
    public int cardTypeCount = 5;
    public int minUnitsPerCard = 3;
    public int maxUnitsPerCard = 8;
    public float battleCameraDepth = 50f;

    void Start()
    {
        ConfigureBattleSceneCamera();
        EnsureUsableEventSystem();

        if (battleUI == null)
        {
            battleUI = FindObjectOfType<BattleUI>();
            if (battleUI == null)
            {
                Debug.LogError("未找到 BattleUI，请在场景中创建并绑定 BattleUI。初始化终止。");
                return;
            }
        }

        var manager = new BattleManager();

        List<Card> playerDeck;
        List<Card> enemyDeck;
        bool playerStartsAttacking = playerStarts;

        if (BattleSceneBridge.HasBattleData)
        {
            playerDeck = BattleSceneBridge.PlayerDeck;
            enemyDeck = BattleSceneBridge.EnemyDeck;
            playerStartsAttacking = BattleSceneBridge.PlayerStartsAttacking;
            Debug.Log("BattleSceneInitializer: 使用地图桥接数据初始化战斗。");
        }
        else
        {
            playerDeck = CreateRandomTestDeck("玩家");
            enemyDeck = CreateRandomTestDeck("敌方");
            Debug.Log("BattleSceneInitializer: 使用随机测试卡组初始化战斗。");
        }

        manager.Initialize(playerDeck, enemyDeck, playerStartsAttacking);

        battleUI.battleManager = manager;
        battleUI.RefreshUI();
    }

    private void ConfigureBattleSceneCamera()
    {
        Camera[] cameras = FindObjectsOfType<Camera>(true);
        foreach (Camera camera in cameras)
        {
            if (camera.gameObject.scene != gameObject.scene)
            {
                continue;
            }

            camera.tag = "Untagged";
            camera.depth = Mathf.Max(camera.depth, battleCameraDepth);
            camera.clearFlags = CameraClearFlags.SolidColor;
        }
    }

    private void EnsureUsableEventSystem()
    {
        EventSystem[] eventSystems = FindObjectsOfType<EventSystem>(true);
        EventSystem localEventSystem = null;
        bool hasActiveEventSystemOutsideBattleScene = false;

        foreach (EventSystem eventSystem in eventSystems)
        {
            if (eventSystem.gameObject.scene == gameObject.scene)
            {
                localEventSystem = eventSystem;
            }
            else if (eventSystem.gameObject.activeInHierarchy)
            {
                hasActiveEventSystemOutsideBattleScene = true;
            }
        }

        if (hasActiveEventSystemOutsideBattleScene)
        {
            if (localEventSystem != null)
            {
                localEventSystem.gameObject.SetActive(false);
            }

            return;
        }

        if (localEventSystem != null)
        {
            localEventSystem.gameObject.SetActive(true);
            return;
        }

        if (eventSystems.Length == 0)
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }
    }

    private List<Card> CreateRandomTestDeck(string ownerName)
    {
        int count = Mathf.Clamp(cardTypeCount, 1, 5);
        var deck = new List<Card>();
        var usedKeys = new HashSet<string>();

        while (deck.Count < count && usedKeys.Count < 15)
        {
            RaceType race = (RaceType)Random.Range(0, 3);
            int unitIndex = Random.Range(0, 5);
            string key = $"{race}_{unitIndex}";
            if (!usedKeys.Add(key))
            {
                continue;
            }

            Card card = CreateCard(race, unitIndex);
            card.quantity = Random.Range(GetMinUnitsPerCard(), GetMaxUnitsPerCard() + 1);
            deck.Add(card);
        }

        Debug.Log($"{ownerName}随机测试卡组：{GetDeckDescription(deck)}");
        return deck;
    }

    private Card CreateCard(RaceType race, int unitIndex)
    {
        return race switch
        {
            RaceType.Human => HumanUnit.CreateCard(unitIndex),
            RaceType.Heaven => HeavenUnit.CreateCard(unitIndex),
            RaceType.Ghost => GhostUnit.CreateCard(unitIndex),
            _ => HumanUnit.CreateCard(unitIndex),
        };
    }

    private string GetDeckDescription(List<Card> deck)
    {
        var descriptions = new List<string>();
        foreach (Card card in deck)
        {
            descriptions.Add($"{card.cardName}({card.race} Lv{card.level}) x{card.quantity}");
        }

        return string.Join(" | ", descriptions);
    }

    private int GetMinUnitsPerCard()
    {
        return Mathf.Max(1, minUnitsPerCard);
    }

    private int GetMaxUnitsPerCard()
    {
        return Mathf.Max(GetMinUnitsPerCard(), maxUnitsPerCard);
    }
}
