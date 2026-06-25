using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int currentTurn = 1;
    public int maxTurn = 20;
    public bool isPlayerTurn = true;
    public bool hasPlayerActed = false;
    public bool isBattleActive = false;

    public enum GameState
    {
        PlayerTurn,
        AITurn,
        End
    }

    public GameState currentState;

    public Player player;
    public Player aiPlayer;

    public bool IsGameEnded => gameEnded;

    private bool gameEnded = false;
    private BattleEncounterType pendingBattleType = BattleEncounterType.None;

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
        if (GameSession.HasRunState)
        {
            RestoreGameFromSession();
            return;
        }

        StartGame();
    }

    void StartGame()
    {
        currentTurn = 1;
        gameEnded = false;
        isBattleActive = false;
        pendingBattleType = BattleEncounterType.None;

        RaceType playerRace = GameSetupData.IsNewGame ? GameSetupData.PlayerRace : RaceType.Human;
        RaceType aiRace = GameSetupData.IsNewGame ? GameSetupData.EnemyRace : RaceType.Ghost;

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
            playerName = "电脑",
            race = aiRace,
            resources = new ResourceData(100, 100),
            strongholdLevel = 1,
            deck = new Deck(),
            strongholdPos = new Vector2Int(9, 9)
        };
        DeckInit(aiPlayer);

        StartPlayerTurn();
    }

    private void RestoreGameFromSession()
    {
        GameRunState state = GameSession.RunState;
        if (state == null)
        {
            StartGame();
            return;
        }

        currentTurn = state.currentTurn;
        maxTurn = state.maxTurn;
        isPlayerTurn = state.isPlayerTurn;
        hasPlayerActed = state.hasPlayerActed;
        isBattleActive = false;
        gameEnded = state.gameEnded;
        currentState = state.currentState;
        player = GameRunState.ClonePlayer(state.player);
        aiPlayer = GameRunState.ClonePlayer(state.aiPlayer);
        pendingBattleType = BattleEncounterType.None;

        if (GameSession.HasPendingBattleResult)
        {
            PendingBattleResult result = GameSession.PendingBattleResult.Clone();
            pendingBattleType = result.encounterType;
            ResolveBattleResult(result.encounterType, result.outcome);
            GameSession.ClearPendingBattleAfterResolution();
            GameSession.UpdateRunState(GameRunState.Capture(this, state.heroGridPos, state.mapState));
        }

        Debug.Log("游戏管理器：已从运行会话恢复主地图运行状态。");
    }

    void StartPlayerTurn()
    {
        if (gameEnded)
        {
            return;
        }

        currentState = GameState.PlayerTurn;
        isPlayerTurn = true;
        hasPlayerActed = false;

        Debug.Log($"=== 第{currentTurn}回合 玩家回合开始 ===");
    }

    public void OnPlayerAction()
    {
        hasPlayerActed = true;
    }

    public void EndPlayerTurn()
    {
        if (!hasPlayerActed)
        {
            Debug.Log("你还没有执行操作。");
            return;
        }

        Debug.Log("玩家回合结束");
        StartAITurn();
    }

    void StartAITurn()
    {
        if (gameEnded)
        {
            return;
        }

        currentState = GameState.AITurn;
        isPlayerTurn = false;

        Debug.Log("电脑回合开始");

        Invoke(nameof(EndAITurn), 1.5f);
    }

    void EndAITurn()
    {
        Debug.Log("电脑回合结束");
        NextTurn();
    }

    void NextTurn()
    {
        if (gameEnded)
        {
            return;
        }

        ProduceResources();

        currentTurn++;

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
        Debug.Log($"电脑据点产出: {aiProd.gold}金币, {aiProd.buildingMaterials}建材");
    }

    public bool SummonUnit(Player owner, int unitIndex)
    {
        return SummonUnits(owner, unitIndex, 1) > 0;
    }

    public int SummonUnits(Player owner, int unitIndex, int count)
    {
        if (count <= 0)
        {
            return 0;
        }

        int level = unitIndex + 1;
        ResourceData singleCost = owner.GetSummonCost(level);

        if (!owner.CanSummon(level))
        {
            Debug.Log($"无法召唤：需要据点Lv{level}、{singleCost.gold}金币、{singleCost.buildingMaterials}建材。");
            return 0;
        }

        int maxByGold = owner.resources.gold / singleCost.gold;
        int maxByMat = owner.resources.buildingMaterials / singleCost.buildingMaterials;
        int actualCount = Mathf.Min(count, maxByGold, maxByMat);
        if (actualCount <= 0)
        {
            return 0;
        }

        owner.resources.gold -= singleCost.gold * actualCount;
        owner.resources.buildingMaterials -= singleCost.buildingMaterials * actualCount;

        for (int i = 0; i < actualCount; i++)
        {
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
            }
        }

        Debug.Log($"{owner.playerName} 批量召唤了 {actualCount} 张 Lv{level} 卡牌。");
        return actualCount;
    }

    public void StartArmyCampBattle(List<Card> enemyDeck)
    {
        StartArmyCampBattle(enemyDeck, Vector2Int.zero);
    }

    public void StartArmyCampBattle(List<Card> enemyDeck, Vector2Int sourceTilePos)
    {
        StartBattle(BattleEncounterType.ArmyCamp, enemyDeck, sourceTilePos);
    }

    public void StartEnemyStrongholdBattle()
    {
        StartEnemyStrongholdBattle(aiPlayer.strongholdPos);
    }

    public void StartEnemyStrongholdBattle(Vector2Int sourceTilePos)
    {
        StartBattle(BattleEncounterType.EnemyStronghold, aiPlayer.deck.cards, sourceTilePos);
    }

    public void ResolveBattleResult(BattleEncounterType encounterType, BattleOutcome outcome)
    {
        isBattleActive = false;

        if (encounterType != pendingBattleType)
        {
            Debug.LogWarning($"战斗类型不一致：待结算 {pendingBattleType}，收到 {encounterType}。");
        }

        pendingBattleType = BattleEncounterType.None;

        switch (encounterType)
        {
            case BattleEncounterType.ArmyCamp:
                ResolveArmyCampBattle(outcome);
                break;
            case BattleEncounterType.EnemyStronghold:
                ResolveEnemyStrongholdBattle(outcome);
                break;
            default:
                Debug.LogWarning($"未知战斗类型：{encounterType}");
                break;
        }
    }

    private void StartBattle(BattleEncounterType encounterType, List<Card> enemyDeck, Vector2Int sourceTilePos)
    {
        if (gameEnded)
        {
            return;
        }

        if (isBattleActive)
        {
            Debug.Log("已有战斗正在进行，无法重复进入战斗场景。");
            return;
        }

        if (player == null || player.deck == null || enemyDeck == null)
        {
            Debug.LogError("无法开始战斗：玩家或敌方卡组数据缺失。");
            return;
        }

        Hero hero = FindObjectOfType<Hero>();
        MapGenerator mapGenerator = FindObjectOfType<MapGenerator>();
        if (hero == null || mapGenerator == null)
        {
            Debug.LogError("无法开始战斗：缺少英雄或地图生成器，不能保存主地图状态。");
            return;
        }

        MapState mapState = mapGenerator.CaptureMapState();
        if (mapState == null)
        {
            Debug.LogError("无法开始战斗：主地图状态保存失败。");
            return;
        }

        GameRunState runState = GameRunState.Capture(this, hero, mapState);
        PendingBattleState battleState = new PendingBattleState
        {
            encounterType = encounterType,
            sourceTilePos = sourceTilePos,
            playerStartsAttacking = true,
            playerDeck = GameRunState.CloneCardList(player.deck.cards),
            enemyDeck = GameRunState.CloneCardList(enemyDeck)
        };

        isBattleActive = true;
        pendingBattleType = encounterType;
        BattleSceneBridge.LoadBattleScene(runState, battleState);
    }

    private void ResolveArmyCampBattle(BattleOutcome outcome)
    {
        if (IsPlayerBattleWin(outcome))
        {
            int goldReward = Random.Range(30, 51);
            player.resources.gold += goldReward;

            RaceType randomRace = (RaceType)Random.Range(0, 3);
            int randomUnitIndex = Random.Range(0, 2);
            Card rewardCard = CreateCard(randomRace, randomUnitIndex);
            player.deck.AddCard(rewardCard);

            Debug.Log($"战胜兵营！获得 {goldReward} 金币，{rewardCard.cardName} Lv{rewardCard.level}。");
            return;
        }

        int loseGold = Random.Range(10, 31);
        int actualLose = Mathf.Min(loseGold, player.resources.gold);
        player.resources.gold -= actualLose;
        Debug.Log($"兵营战斗失败，损失 {actualLose} 金币。");
    }

    private void ResolveEnemyStrongholdBattle(BattleOutcome outcome)
    {
        if (IsPlayerBattleWin(outcome))
        {
            WinGame(player);
            return;
        }

        Debug.Log("攻打敌方据点失败，返回主地图继续游戏。");
    }

    private static bool IsPlayerBattleWin(BattleOutcome outcome)
    {
        return outcome == BattleOutcome.PlayerVictory || outcome == BattleOutcome.EnemySurrender;
    }

    void DeckInit(Player p)
    {
        for (int i = 0; i < 3; i++)
        {
            p.deck.AddCard(CreateBasicCard(p.race));
        }
    }

    Card CreateBasicCard(RaceType race)
    {
        return CreateCard(race, 0);
    }

    Card CreateCard(RaceType race, int unitIndex)
    {
        return race switch
        {
            RaceType.Human => HumanUnit.CreateCard(unitIndex),
            RaceType.Heaven => HeavenUnit.CreateCard(unitIndex),
            RaceType.Ghost => GhostUnit.CreateCard(unitIndex),
            _ => HumanUnit.CreateCard(unitIndex),
        };
    }

    public void OnHeroEnterEnemyStronghold(Player attacker, Player defender)
    {
        StartEnemyStrongholdBattle();
    }

    void WinGame(Player winner)
    {
        gameEnded = true;
        isBattleActive = false;
        currentState = GameState.End;
        Debug.Log($"{winner.playerName} 胜利！游戏结束。");
    }

    void JudgeByScore()
    {
        gameEnded = true;
        currentState = GameState.End;

        int playerScore = player.deck.GetTotalCombatPower() + player.resources.gold + player.resources.buildingMaterials;
        int aiScore = aiPlayer.deck.GetTotalCombatPower() + aiPlayer.resources.gold + aiPlayer.resources.buildingMaterials;

        Debug.Log("20回合结束，按战力+资源判定。");
        Debug.Log($"玩家总分:{playerScore}，电脑总分:{aiScore}");

        if (playerScore > aiScore)
        {
            WinGame(player);
        }
        else if (aiScore > playerScore)
        {
            WinGame(aiPlayer);
        }
        else
        {
            Debug.Log("平局。");
        }
    }
}
