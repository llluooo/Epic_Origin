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
    public string GameEndTitle => gameEndTitle;
    public string GameEndMessage => gameEndMessage;
    public string GameEndDetail => gameEndDetail;
    public bool GameEndIsVictory => gameEndIsVictory;

    private bool gameEnded = false;
    private bool gameEndIsVictory = false;
    private string gameEndTitle = "";
    private string gameEndMessage = "";
    private string gameEndDetail = "";
    private BattleEncounterType pendingBattleType = BattleEncounterType.None;
    private AIController aiController;
    public AIHero aiHero;
    public bool showDeveloperAILog = false;
    private CameraFollow cameraFollow;

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

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F10))
        {
            TriggerGameEndVictoryTest();
        }

        if (Input.GetKeyDown(KeyCode.F11))
        {
            TriggerGameEndDefeatTest();
        }
    }
#endif

    private void OnEnable()
    {
        AIDifficultySelector.DifficultyChanged += OnAIDifficultyChanged;
    }

    private void OnDisable()
    {
        AIDifficultySelector.DifficultyChanged -= OnAIDifficultyChanged;
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
            deck = new Deck { hasSlotLimit = true },
            garrisonDeck = new Deck(),
            strongholdPos = new Vector2Int(0, 0)
        };
        DeckInit(player);

        aiPlayer = new Player
        {
            playerName = "电脑",
            race = aiRace,
            resources = new ResourceData(100, 100),
            strongholdLevel = 1,
            deck = new Deck { hasSlotLimit = true },
            garrisonDeck = new Deck(),
            strongholdPos = new Vector2Int(9, 9)
        };
        DeckInit(aiPlayer);

        InitializeAIController();
        StartPlayerTurn();
    }

    private void InitializeAIController()
    {
        AIDifficulty difficulty = AIDifficultySelector.CurrentDifficulty;

        if (difficulty == AIDifficulty.Easy)
        {
            aiController = new EasyAI(this, aiPlayer);
            Debug.Log("AI 难度: 简单 (Easy)");
        }
        else
        {
            aiController = new HardAI(this, aiPlayer);
            Debug.Log("AI 难度: 困难 (Hard)");
        }
    }

    private void OnAIDifficultyChanged(AIDifficulty difficulty)
    {
        if (aiPlayer == null || gameEnded)
        {
            return;
        }

        InitializeAIController();
        ShowAILog($"AI 难度切换为 {difficulty}");
    }

    private void RestoreGameFromSession()
    {
        GameRunState state = GameSession.RunState;
        if (state == null)
        {
            StartGame();
            return;
        }

        // 如果是据点兵力场景返回，恢复兵力数据
        if (GameSession.HasGarrisonReturnState)
        {
            var savedState = GameRunState.ClonePlayer(state.player);
            savedState.deck = GameSession.GetGarrisonHeroDeck() ?? savedState.deck;
            savedState.garrisonDeck = GameSession.GetGarrisonGarrisonDeck() ?? savedState.garrisonDeck;
            state.player = savedState;
            GameSession.ClearGarrisonReturnData();
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

        InitializeAIController();

        if (GameSession.HasPendingBattleResult)
        {
            PendingBattleResult result = GameSession.PendingBattleResult.Clone();
            pendingBattleType = result.encounterType;
            ResolveBattleResult(result.encounterType, result.outcome, result.sourceTilePos);
            GameSession.ClearPendingBattleAfterResolution();
            Vector2Int restoredAIHeroPos = state.hasAIHeroGridPos ? state.aiHeroGridPos : aiPlayer.strongholdPos;
            GameSession.UpdateRunState(GameRunState.Capture(this, state.heroGridPos, restoredAIHeroPos, state.mapState));
        }

        Debug.Log("游戏管理器：已从运行会话恢复主地图运行状态。");
    }

    public bool SaveCurrentRunState()
    {
        Hero hero = FindObjectOfType<Hero>();
        MapGenerator mapGenerator = FindObjectOfType<MapGenerator>();
        if (hero == null || mapGenerator == null)
        {
            Debug.LogError("无法保存存档：缺少英雄或地图生成器。");
            return false;
        }

        MapState mapState = mapGenerator.CaptureMapState();
        if (mapState == null)
        {
            Debug.LogError("无法保存存档：地图状态捕获失败。");
            return false;
        }

        GameRunState runState = GameRunState.Capture(this, hero, mapState);
        return SaveSystem.SaveAutoGame(runState);
    }

    public bool SaveToManualSlot(int slot)
    {
        Hero hero = FindObjectOfType<Hero>();
        MapGenerator mapGenerator = FindObjectOfType<MapGenerator>();
        if (hero == null || mapGenerator == null)
        {
            Debug.LogError("无法保存存档：缺少英雄或地图生成器。");
            return false;
        }

        MapState mapState = mapGenerator.CaptureMapState();
        if (mapState == null)
        {
            Debug.LogError("无法保存存档：地图状态捕获失败。");
            return false;
        }

        GameRunState runState = GameRunState.Capture(this, hero, mapState);
        return SaveSystem.SaveManualGame(runState, slot);
    }

    public int GetNextManualSaveSlot()
    {
        return SaveSystem.GetNextSaveSlot();
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

        SwitchCameraToPlayer();

        Debug.Log($"=== 第{currentTurn}回合 玩家回合开始 ===");
    }

    public void OnPlayerAction()
    {
        hasPlayerActed = true;
    }

    public void EndPlayerTurn()
    {
        if (currentState != GameState.PlayerTurn)
        {
            Debug.Log("当前不是玩家回合，无法结束回合。");
            return;
        }

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

        if (currentState == GameState.AITurn)
        {
            return;
        }

        currentState = GameState.AITurn;
        isPlayerTurn = false;

        SwitchCameraToAI();

        Debug.Log("电脑回合开始");

        if (aiController != null)
        {
            bool actedSuccessfully = aiController.ExecuteTurn();
            if (actedSuccessfully)
            {
                Debug.Log("AI 执行了一个动作");
            }
            else
            {
                Debug.Log("AI 本回合没有进行动作");
            }
        }

        if (aiHero != null && aiHero.IsMoving)
        {
            aiHero.OnMoveComplete += OnAIHeroArrived;
        }
        else
        {
            Invoke(nameof(EndAITurn), 2.5f);
        }
    }

    private void OnAIHeroArrived()
    {
        if (aiHero != null)
        {
            aiHero.OnMoveComplete -= OnAIHeroArrived;
        }
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
        SaveCurrentRunState();
    }

    void ProduceResources()
    {
        ResourceData playerProd = player.GetTurnProduction();
        player.resources.Add(playerProd);
        Debug.Log($"玩家据点产出: {playerProd.gold}金币, {playerProd.buildingMaterials}建材");
        MessageLogUI.Instance?.AddMessage($"据点产出: +{playerProd.gold}金币, +{playerProd.buildingMaterials}建材");

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
                if (owner.TryAddToHeroDeck(card))
                {
                    // 成功加入英雄卡组
                }
                else
                {
                    owner.AddToGarrison(card);
                    Debug.Log($"英雄兵力已满，{card.cardName} Lv{card.level} 自动移入据点。");
                }
            }
        }

        Debug.Log($"{owner.playerName} 批量召唤了 {actualCount} 张 Lv{level} 卡牌。");
        return actualCount;
    }

    /// <summary>
    /// 为指定种族创建卡牌（供 AI 控制器调用）
    /// 如果指定了 level，会覆盖默认的 unitIndex + 1 等级
    /// </summary>
    public Card CreateCardForRace(RaceType race, int unitIndex, int targetLevel = -1)
    {
        Card card = race switch
        {
            RaceType.Human => HumanUnit.CreateCard(unitIndex),
            RaceType.Heaven => HeavenUnit.CreateCard(unitIndex),
            RaceType.Ghost => GhostUnit.CreateCard(unitIndex),
            _ => null
        };

        // 如果指定了目标等级，更新卡牌的等级
        if (card != null && targetLevel > 0 && targetLevel != card.level)
        {
            card.level = targetLevel;
            card.currentHP = card.GetMaxHP();
            Debug.Log($"调整卡牌 {card.cardName} 等级为 {targetLevel}");
        }

        return card;
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

    public void ResolveBattleResult(BattleEncounterType encounterType, BattleOutcome outcome, Vector2Int sourceTilePos)
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
                ResolveArmyCampBattle(outcome, sourceTilePos);
                break;
            case BattleEncounterType.EnemyStronghold:
                ResolveEnemyStrongholdBattle(outcome);
                break;
            case BattleEncounterType.PlayerStronghold:
                ResolvePlayerStrongholdBattle(outcome);
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

    private void ResolveArmyCampBattle(BattleOutcome outcome, Vector2Int sourceTilePos)
    {
        if (outcome == BattleOutcome.PlayerSurrender)
        {
            ApplySurrenderPenalty();
            return;
        }

        if (outcome == BattleOutcome.PlayerFled)
        {
            Debug.Log("玩家从兵营战斗中逃跑成功。");
            MessageLogUI.Instance?.AddMessage("玩家从兵营战斗中逃跑成功。");
            return;
        }

        if (IsPlayerBattleWin(outcome))
        {
            List<Card> enemyDeck = GetPendingBattleEnemyDeck();
            int rewardCount = Mathf.Min(enemyDeck != null ? enemyDeck.Count : 0, Random.Range(2, 4));

            if (enemyDeck != null && rewardCount > 0)
            {
                List<int> indices = new List<int>();
                for (int i = 0; i < enemyDeck.Count; i++) indices.Add(i);
                ShuffleList(indices);

                for (int i = 0; i < rewardCount; i++)
                {
                    Card source = enemyDeck[indices[i]];
                    if (source == null)
                    {
                        continue;
                    }

                    Card rewardCard = source.Clone();
                    rewardCard.quantity = 1;
                    rewardCard.currentHP = rewardCard.GetMaxHP();

                    if (player.TryAddToHeroDeck(rewardCard))
                    {
                        Debug.Log($"战胜兵营！获得 {rewardCard.cardName} Lv{rewardCard.level}。");
                        MessageLogUI.Instance?.AddMessage($"战胜兵营！获得 {rewardCard.cardName} Lv{rewardCard.level}。");
                    }
                    else
                    {
                        player.AddToGarrison(rewardCard);
                        Debug.Log($"战胜兵营！获得 {rewardCard.cardName} Lv{rewardCard.level}，英雄兵力已满，自动移入据点。");
                        MessageLogUI.Instance?.AddMessage($"战胜兵营！获得 {rewardCard.cardName} Lv{rewardCard.level}。");
                    }
                }
            }
            else
            {
                int goldReward = Random.Range(30, 51);
                player.resources.gold += goldReward;
                Debug.Log($"战胜兵营！获得 {goldReward} 金币。");
                MessageLogUI.Instance?.AddMessage($"战胜兵营！获得 {goldReward} 金币。");
            }

            // 战胜后清除兵营格子
            if (MapManager.Instance != null)
            {
                MapManager.Instance.ClearTileAt(sourceTilePos);
            }

            return;
        }

        RemovePlayerBattleCards();
        Debug.Log("兵营战斗失败，参战卡牌已损失。");
        MessageLogUI.Instance?.AddMessage("兵营战斗失败，参战卡牌已损失。");
    }

    private void ResolveEnemyStrongholdBattle(BattleOutcome outcome)
    {
        if (outcome == BattleOutcome.PlayerSurrender)
        {
            ApplySurrenderPenalty();
            return;
        }

        if (outcome == BattleOutcome.PlayerFled)
        {
            Debug.Log("玩家从敌方据点战斗中逃跑成功。");
            MessageLogUI.Instance?.AddMessage("玩家从敌方据点战斗中逃跑成功。");
            return;
        }

        if (IsPlayerBattleWin(outcome))
        {
            WinGame(player);
            return;
        }

        Debug.Log("攻打敌方据点失败，返回主地图继续游戏。");
        MessageLogUI.Instance?.AddMessage("攻打敌方据点失败，返回主地图继续游戏。");
    }

    private void ResolvePlayerStrongholdBattle(BattleOutcome outcome)
    {
        if (outcome == BattleOutcome.PlayerSurrender)
        {
            // 玩家投降 → AI 胜利
            WinGame(aiPlayer);
            return;
        }

        if (outcome == BattleOutcome.PlayerFled)
        {
            Debug.Log("玩家从己方据点战斗中逃跑成功。");
            MessageLogUI.Instance?.AddMessage("玩家从己方据点战斗中逃跑成功。");
            return;
        }

        if (IsPlayerBattleWin(outcome))
        {
            // 玩家成功防守，击退 AI 进攻
            Debug.Log("成功防守己方据点，击退 AI 进攻！");
            MessageLogUI.Instance?.AddMessage("成功防守己方据点！");
            return;
        }

        // AI 攻破玩家据点 → AI 胜利
        WinGame(aiPlayer);
    }

    private void ApplySurrenderPenalty()
    {
        int goldLost = Mathf.RoundToInt(player.resources.gold * 0.2f);
        int matLost = Mathf.RoundToInt(player.resources.buildingMaterials * 0.2f);
        player.resources.gold -= goldLost;
        player.resources.buildingMaterials -= matLost;

        int totalCards = player.deck.CardCount;
        int cardsToLose = Mathf.RoundToInt(totalCards * 0.15f);
        for (int i = 0; i < cardsToLose && player.deck.CardCount > 0; i++)
        {
            int idx = Random.Range(0, player.deck.CardCount);
            Card removed = player.deck[idx];
            player.deck.cards.RemoveAt(idx);
            Debug.Log($"逃跑损失卡牌：{removed.cardName} Lv{removed.level}");
        }

        Debug.Log($"逃跑惩罚：损失 {goldLost} 金币、{matLost} 建材、{cardsToLose} 张卡牌。");
        MessageLogUI.Instance?.AddMessage($"逃跑惩罚：损失 {goldLost} 金币、{matLost} 建材、{cardsToLose} 张卡牌");
    }

    private void RemovePlayerBattleCards()
    {
        List<Card> battleCards = GetPendingBattlePlayerDeck();
        if (battleCards == null || battleCards.Count == 0) return;

        RemoveCardsByBattleSnapshot(player.deck, battleCards);
    }

    public static void RemoveCardsByBattleSnapshot(Deck deck, List<Card> battleCards)
    {
        if (deck == null || battleCards == null)
        {
            return;
        }

        foreach (Card battleCard in battleCards)
        {
            if (battleCard == null)
            {
                continue;
            }

            int remaining = Mathf.Max(1, battleCard.quantity);
            for (int i = deck.CardCount - 1; i >= 0 && remaining > 0; i--)
            {
                Card deckCard = deck[i];
                if (deckCard.race == battleCard.race
                    && deckCard.unitIndex == battleCard.unitIndex
                    && deckCard.level == battleCard.level)
                {
                    int removedQuantity = Mathf.Min(deckCard.quantity, remaining);
                    deckCard.quantity -= removedQuantity;
                    remaining -= removedQuantity;
                    Debug.Log($"损失参战卡牌：{deckCard.cardName} Lv{deckCard.level} x{removedQuantity}");

                    if (deckCard.quantity <= 0)
                    {
                        deck.cards.RemoveAt(i);
                    }
                }
            }
        }
    }

    private List<Card> GetPendingBattlePlayerDeck()
    {
        if (GameSession.HasPendingBattle && GameSession.PendingBattle != null)
        {
            return GameSession.PendingBattle.playerDeck;
        }
        return null;
    }

    private static bool IsPlayerBattleWin(BattleOutcome outcome)
    {
        return outcome == BattleOutcome.PlayerVictory || outcome == BattleOutcome.EnemySurrender;
    }

    private List<Card> GetPendingBattleEnemyDeck()
    {
        if (GameSession.HasPendingBattle && GameSession.PendingBattle != null)
        {
            return GameSession.PendingBattle.enemyDeck;
        }
        return null;
    }

    private static void ShuffleList(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int r = Random.Range(0, i + 1);
            (list[i], list[r]) = (list[r], list[i]);
        }
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

    /// <summary>
    /// AI 英雄进入玩家据点时调用，AI 进攻玩家据点
    /// </summary>
    public void OnAIHeroEnterPlayerStronghold()
    {
        if (gameEnded || isBattleActive) return;
        Debug.Log("AI 进攻玩家据点！");
        MessageLogUI.Instance?.AddMessage("AI 进攻我方据点！");
        StartBattle(BattleEncounterType.PlayerStronghold, aiPlayer.deck.cards, player.strongholdPos);
    }

    void WinGame(Player winner)
    {
        gameEnded = true;
        isBattleActive = false;
        currentState = GameState.End;
        SetGameEndSummary(winner);
        NotifyGameEndUI();
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

        if (playerScore >= aiScore)
        {
            WinGame(player);
        }
        else
        {
            WinGame(aiPlayer);
        }
    }

    public void TriggerGameEndVictoryTest()
    {
        if (gameEnded)
            return;

        Debug.Log("[Debug] 触发游戏结束结算测试（胜利）");
        gameEnded = true;
        isBattleActive = false;
        currentState = GameState.End;
        SetGameEndSummary(player);
        NotifyGameEndUI();
    }

    public void TriggerGameEndDefeatTest()
    {
        if (gameEnded)
            return;

        Debug.Log("[Debug] 触发游戏结束结算测试（失败）");
        gameEnded = true;
        isBattleActive = false;
        currentState = GameState.End;
        SetGameEndSummary(aiPlayer);
        NotifyGameEndUI();
    }

    private void SetGameEndSummary(Player winner)
    {
        gameEndIsVictory = (winner == player);
        gameEndTitle = gameEndIsVictory ? "胜利！" : "失败！";
        gameEndMessage = gameEndIsVictory ? "你获得了胜利！" : "电脑获胜，游戏结束。";
        gameEndDetail = BuildGameEndDetail();
    }

    private string BuildGameEndDetail()
    {
        int playerScore = player.deck.GetTotalCombatPower() + player.resources.gold + player.resources.buildingMaterials;
        int aiScore = aiPlayer.deck.GetTotalCombatPower() + aiPlayer.resources.gold + aiPlayer.resources.buildingMaterials;

        string finalResult = gameEndMessage;

        return $"回合数: {Mathf.Min(currentTurn, maxTurn)}/{maxTurn}\n"
             + $"玩家据点等级: Lv{player.strongholdLevel}\n"
             + $"玩家资源: {player.resources.gold} 金币, {player.resources.buildingMaterials} 建材\n"
             + $"结局: {finalResult}";
    }

    private void NotifyGameEndUI()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameEndPanel(GameEndDetail, GameEndIsVictory);
            return;
        }

        UIManager ui = FindObjectOfType<UIManager>();
        if (ui != null)
        {
            ui.ShowGameEndPanel(GameEndDetail, GameEndIsVictory);
            return;
        }

        GameEndUI endUI = FindObjectOfType<GameEndUI>();
        if (endUI == null)
        {
            GameObject uiObject = new GameObject("GameEndUI");
            endUI = uiObject.AddComponent<GameEndUI>();
        }

        if (GameEndIsVictory)
            endUI.ShowVictory(GameEndDetail);
        else
            endUI.ShowDefeat(GameEndDetail);
    }

    private CameraFollow GetCameraFollow()
    {
        if (cameraFollow == null)
        {
            cameraFollow = FindObjectOfType<CameraFollow>();
        }
        return cameraFollow;
    }

    private void SwitchCameraToAI()
    {
        CameraFollow cf = GetCameraFollow();
        if (cf != null && aiHero != null)
        {
            cf.target = aiHero.transform;
        }
    }

    private void SwitchCameraToPlayer()
    {
        CameraFollow cf = GetCameraFollow();
        if (cf != null)
        {
            Hero playerHero = FindObjectOfType<Hero>();
            if (playerHero != null)
            {
                cf.target = playerHero.transform;
            }
        }
    }

    public void ShowAILog(string message)
    {
        aiActionLog = message;
        aiLogTimer = 2.5f;
    }

    private string aiActionLog = "";
    private float aiLogTimer = 0f;

    void OnGUI()
    {
        if (showDeveloperAILog && aiLogTimer > 0)
        {
            aiLogTimer -= Time.deltaTime;
            GUIStyle logStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, 0.85f, 0.3f) }
            };
            Rect logRect = new Rect(Screen.width / 2 - 200, Screen.height - 80, 400, 50);
            GUI.Label(logRect, aiActionLog, logStyle);
        }
    }
}
