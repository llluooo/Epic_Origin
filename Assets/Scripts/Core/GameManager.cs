using UnityEngine;

/// <summary>
/// 游戏管理器（核心控制类）
/// 负责：回合控制、玩家行为限制、状态管理、玩家数据、胜负判定
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // ================== 回合数据 ==================
    public int currentTurn = 1;
    public int maxTurn = 20;
    public bool isPlayerTurn = true;

    // ================== 玩家行为控制 ==================
    public bool hasPlayerActed = false;

    // ================== 游戏状态 ==================
    public enum GameState
    {
        PlayerTurn,
        AITurn,
        End
    }

    public GameState currentState;

    // ================== 玩家数据 ==================
    public Player player;
    public Player aiPlayer;

    private bool gameEnded = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        StartGame();
    }

    void StartGame()
    {
        currentTurn = 1;
        gameEnded = false;

        // 从GameSetupData获取种族，若未设置则使用默认
        RaceType playerRace = GameSetupData.IsNewGame ? GameSetupData.PlayerRace : RaceType.Human;
        RaceType aiRace = GameSetupData.IsNewGame ? GameSetupData.EnemyRace : RaceType.Ghost;

        // 初始化玩家（策划案5.5: 初始100金+100建材，1级据点，3张Lv1基础兵种卡）
        player = new Player
        {
            playerName = "玩家",
            race = playerRace,
            resources = new ResourceData(100, 100),
            strongholdLevel = 1,
            deck = new Deck(),
            strongholdPos = new Vector2Int(0, 0)
        };
        DeckInit(player);

        aiPlayer = new Player
        {
            playerName = "AI",
            race = aiRace,
            resources = new ResourceData(100, 100),
            strongholdLevel = 1,
            deck = new Deck(),
            strongholdPos = new Vector2Int(9, 9)
        };
        DeckInit(aiPlayer);

        StartPlayerTurn();
    }

    // ================== 玩家回合 ==================

    void StartPlayerTurn()
    {
        if (gameEnded) return;

        currentState = GameState.PlayerTurn;
        isPlayerTurn = true;
        hasPlayerActed = false;

        Debug.Log($"=== 第{currentTurn}回合 玩家回合开始 ===");
    }

    /// <summary>
    /// 玩家执行主操作（移动/升级/召唤）时调用
    /// </summary>
    public void OnPlayerAction()
    {
        hasPlayerActed = true;
    }

    /// <summary>
    /// 玩家结束回合
    /// </summary>
    public void EndPlayerTurn()
    {
        if (!hasPlayerActed)
        {
            Debug.Log("你还没有执行操作！");
            return;
        }

        Debug.Log("玩家回合结束");
        StartAITurn();
    }

    // ================== AI回合 ==================

    void StartAITurn()
    {
        if (gameEnded) return;

        currentState = GameState.AITurn;
        isPlayerTurn = false;

        Debug.Log("AI回合开始");

        Invoke(nameof(EndAITurn), 1.5f);
    }

    void EndAITurn()
    {
        Debug.Log("AI回合结束");
        NextTurn();
    }

    // ================== 回合推进 ==================

    void NextTurn()
    {
        if (gameEnded) return;

        // 回合结束时双方据点产出资源
        ProduceResources();

        currentTurn++;

        // 检查20回合上限
        if (currentTurn > maxTurn)
        {
            JudgeByScore();
            return;
        }

        StartPlayerTurn();
    }

    void ProduceResources()
    {
        ResourceData playerProd = player.GetTurnProduction();
        player.resources.Add(playerProd);
        Debug.Log($"玩家据点产出: {playerProd.gold}金币, {playerProd.buildingMaterials}建材");

        ResourceData aiProd = aiPlayer.GetTurnProduction();
        aiPlayer.resources.Add(aiProd);
        Debug.Log($"AI据点产出: {aiProd.gold}金币, {aiProd.buildingMaterials}建材");
    }

    // ================== 召唤单位 ==================

    /// <summary>
    /// 为指定玩家召唤单位
    /// </summary>
    public bool SummonUnit(Player owner, int unitIndex)
    {
        int level = unitIndex + 1;
        ResourceData cost = owner.GetSummonCost(level);
        if (!owner.CanSummon(level))
        {
            Debug.Log($"无法召唤！需要据点Lv{level}、{cost.gold}金币、{cost.buildingMaterials}建材");
            return false;
        }

        owner.resources.gold -= cost.gold;
        owner.resources.buildingMaterials -= cost.buildingMaterials;

        Card card = owner.race switch
        {
            RaceType.Human => HumanUnit.CreateCard(unitIndex),
            RaceType.Heaven => HeavenUnit.CreateCard(unitIndex),
            RaceType.Ghost => GhostUnit.CreateCard(unitIndex),
            _ => null
        };

        if (card != null)
        {
            owner.deck.AddCard(card);
            Debug.Log($"{owner.playerName} 召唤了 Lv{level} {card.cardName}！");
        }

        return true;
    }

    void DeckInit(Player p)
    {
        for (int i = 0; i < 3; i++)
            p.deck.AddCard(CreateBasicCard(p.race));
    }

    Card CreateBasicCard(RaceType race)
    {
        return race switch
        {
            RaceType.Human => HumanUnit.CreateCard(0),
            RaceType.Heaven => HeavenUnit.CreateCard(0),
            RaceType.Ghost => GhostUnit.CreateCard(0),
            _ => HumanUnit.CreateCard(0),
        };
    }

    // ================== 胜负判定 ==================

    /// <summary>
    /// 英雄到达敌方据点时调用
    /// </summary>
    public void OnHeroEnterEnemyStronghold(Player attacker, Player defender)
    {
        if (gameEnded) return;

        int atkPower = attacker.deck.GetTotalCombatPower();
        int defPower = defender.deck.GetTotalCombatPower();

        Debug.Log($"战斗！{attacker.playerName}战力:{atkPower} vs {defender.playerName}战力:{defPower}");

        if (atkPower > defPower)
        {
            WinGame(attacker);
        }
        else
        {
            Debug.Log("战力不足，无法攻占据点！");
        }
    }

    void WinGame(Player winner)
    {
        gameEnded = true;
        currentState = GameState.End;
        Debug.Log($"{winner.playerName} 胜利！游戏结束");
    }

    void JudgeByScore()
    {
        gameEnded = true;
        currentState = GameState.End;

        int playerScore = player.deck.GetTotalCombatPower() + player.resources.gold + player.resources.buildingMaterials;
        int aiScore = aiPlayer.deck.GetTotalCombatPower() + aiPlayer.resources.gold + aiPlayer.resources.buildingMaterials;

        Debug.Log($"20回合结束，按战力+资源判定:");
        Debug.Log($"玩家总分:{playerScore} vs AI总分:{aiScore}");

        if (playerScore > aiScore)
            WinGame(player);
        else if (aiScore > playerScore)
            WinGame(aiPlayer);
        else
            Debug.Log("平局！");
    }
}
