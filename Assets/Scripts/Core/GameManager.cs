using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 游戏管理器：控制回合、玩家数据、资源产出、召唤升级和战斗结算。
/// </summary>
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

        Debug.Log("AI回合开始");

        Invoke(nameof(EndAITurn), 1.5f);
    }

    void EndAITurn()
    {
        Debug.Log("AI回合结束");
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
        Debug.Log($"AI据点产出: {aiProd.gold}金币, {aiProd.buildingMaterials}建材");
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
        StartBattle(BattleEncounterType.ArmyCamp, enemyDeck);
    }

    public void StartEnemyStrongholdBattle()
    {
        StartBattle(BattleEncounterType.EnemyStronghold, aiPlayer.deck.cards);
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

    private void StartBattle(BattleEncounterType encounterType, List<Card> enemyDeck)
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

        isBattleActive = true;
        pendingBattleType = encounterType;
        BattleSceneBridge.LoadBattleScene(player.deck.cards, enemyDeck, encounterType, playerStartsAttacking: true);
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

        Debug.Log("20回合结束，按战力+资源判定：");
        Debug.Log($"玩家总分:{playerScore} vs AI总分:{aiScore}");

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
