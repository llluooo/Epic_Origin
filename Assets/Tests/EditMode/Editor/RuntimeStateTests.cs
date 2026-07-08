using System.Collections.Generic;
using System.IO;
using System;
using NUnit.Framework;
using UnityEngine;

public class RuntimeStateTests
{
    private readonly List<GameObject> createdObjects = new List<GameObject>();

    [TearDown]
    public void TearDown()
    {
        GameSession.ResetForTests();
        GameManager.Instance = null;

        foreach (GameObject createdObject in createdObjects)
        {
            UnityEngine.Object.DestroyImmediate(createdObject);
        }

        createdObjects.Clear();
    }

    [Test]
    public void ClonePlayer_copies_runtime_data_without_reusing_deck_or_cards()
    {
        Player original = new Player
        {
            playerName = "Player",
            race = RaceType.Heaven,
            resources = new ResourceData(77, 88),
            strongholdLevel = 3,
            strongholdPos = new Vector2Int(2, 4),
            deck = new Deck()
        };
        Card originalCard = HeavenUnit.CreateCard(1);
        originalCard.quantity = 6;
        originalCard.currentHP = 12;
        original.deck.AddCard(originalCard);

        Player clone = GameRunState.ClonePlayer(original);

        Assert.AreNotSame(original, clone);
        Assert.AreEqual(original.playerName, clone.playerName);
        Assert.AreEqual(original.race, clone.race);
        Assert.AreEqual(original.resources.gold, clone.resources.gold);
        Assert.AreEqual(original.resources.buildingMaterials, clone.resources.buildingMaterials);
        Assert.AreEqual(original.strongholdLevel, clone.strongholdLevel);
        Assert.AreEqual(original.strongholdPos, clone.strongholdPos);
        Assert.AreNotSame(original.deck, clone.deck);
        Assert.AreEqual(1, clone.deck.CardCount);
        Assert.AreNotSame(original.deck[0], clone.deck[0]);
        Assert.AreEqual(original.deck[0].quantity, clone.deck[0].quantity);
        Assert.AreEqual(original.deck[0].currentHP, clone.deck[0].currentHP);
    }

    [Test]
    public void CloneDeck_preserves_slot_limit_setting()
    {
        Deck original = new Deck { hasSlotLimit = true };
        original.AddCard(HumanUnit.CreateCard(0));

        Deck clone = GameRunState.CloneDeck(original);

        Assert.IsTrue(clone.hasSlotLimit);
        Assert.AreEqual(original.CardCount, clone.CardCount);
    }

    [Test]
    public void MapState_capture_records_tile_kind_position_and_stronghold_metadata()
    {
        Tile[,] map = new Tile[2, 2];
        map[0, 0] = CreateTile<EmptyTile>(0, 0);
        map[1, 0] = CreateTile<ObstacleTile>(1, 0);
        map[0, 1] = CreateTile<ArmyCampTile>(0, 1);

        StrongholdTile stronghold = CreateTile<StrongholdTile>(1, 1);
        stronghold.strongholdType = StrongholdType.Enemy;
        stronghold.visualRace = RaceType.Ghost;
        map[1, 1] = stronghold;

        MapState state = MapState.Capture(map, new Vector2(-2f, 3f), 1.5f);

        Assert.AreEqual(2, state.width);
        Assert.AreEqual(2, state.height);
        Assert.AreEqual(new Vector2(-2f, 3f), state.worldOrigin);
        Assert.AreEqual(1.5f, state.tileSize);
        Assert.AreEqual(TileKind.Empty, state.GetTile(new Vector2Int(0, 0)).kind);
        Assert.AreEqual(TileKind.Obstacle, state.GetTile(new Vector2Int(1, 0)).kind);
        Assert.AreEqual(TileKind.ArmyCamp, state.GetTile(new Vector2Int(0, 1)).kind);

        TileState strongholdState = state.GetTile(new Vector2Int(1, 1));
        Assert.AreEqual(TileKind.Stronghold, strongholdState.kind);
        Assert.AreEqual(StrongholdType.Enemy, strongholdState.strongholdType);
        Assert.AreEqual(RaceType.Ghost, strongholdState.visualRace);
    }

    [Test]
    public void GameSession_keeps_pending_battle_and_result_without_scene_objects()
    {
        GameRunState runState = new GameRunState
        {
            currentTurn = 4,
            hasPlayerActed = true,
            heroGridPos = new Vector2Int(3, 2)
        };
        PendingBattleState battleState = new PendingBattleState
        {
            encounterType = BattleEncounterType.ArmyCamp,
            sourceTilePos = new Vector2Int(5, 6),
            playerStartsAttacking = true,
            playerDeck = new List<Card> { HumanUnit.CreateCard(0) },
            enemyDeck = new List<Card> { GhostUnit.CreateCard(0) }
        };

        GameSession.StoreBattleLaunchState(runState, battleState);
        GameSession.SetPendingBattleResult(BattleOutcome.PlayerVictory);

        Assert.IsTrue(GameSession.HasRunState);
        Assert.IsTrue(GameSession.HasPendingBattle);
        Assert.IsTrue(GameSession.HasPendingBattleResult);
        Assert.AreEqual(4, GameSession.RunState.currentTurn);
        Assert.AreEqual(BattleEncounterType.ArmyCamp, GameSession.PendingBattle.encounterType);
        Assert.AreEqual(BattleOutcome.PlayerVictory, GameSession.PendingBattleResult.outcome);
        Assert.AreNotSame(battleState.playerDeck[0], GameSession.PendingBattle.playerDeck[0]);
    }

    [Test]
    public void GameRunState_capture_and_clone_preserve_ai_hero_position()
    {
        GameObject managerObject = new GameObject("GameManager");
        createdObjects.Add(managerObject);
        GameManager gameManager = managerObject.AddComponent<GameManager>();
        gameManager.player = CreatePlayer("Player", RaceType.Human, new Vector2Int(0, 0));
        gameManager.aiPlayer = CreatePlayer("AI", RaceType.Ghost, new Vector2Int(9, 9));

        GameObject heroObject = new GameObject("Hero");
        createdObjects.Add(heroObject);
        Hero hero = heroObject.AddComponent<Hero>();
        hero.currentGridPos = new Vector2Int(2, 3);

        GameObject aiHeroObject = new GameObject("AIHero");
        createdObjects.Add(aiHeroObject);
        AIHero aiHero = aiHeroObject.AddComponent<AIHero>();
        aiHero.currentGridPos = new Vector2Int(6, 7);
        gameManager.aiHero = aiHero;

        GameRunState state = GameRunState.Capture(gameManager, hero, new MapState());
        GameRunState clone = state.Clone();

        Assert.AreEqual(new Vector2Int(2, 3), state.heroGridPos);
        Assert.AreEqual(new Vector2Int(6, 7), state.aiHeroGridPos);
        Assert.AreEqual(new Vector2Int(6, 7), clone.aiHeroGridPos);
    }

    [Test]
    public void SaveSystem_keeps_auto_save_separate_from_manual_slots()
    {
        string saveDirectory = Path.Combine(Path.GetTempPath(), $"EpicOriginSaveSystemTests_{Guid.NewGuid():N}");
        SaveSystem.SaveDirectoryOverride = saveDirectory;
        try
        {
            GameRunState manualState = new GameRunState { currentTurn = 3 };
            GameRunState autoState = new GameRunState { currentTurn = 9 };

            Assert.IsTrue(SaveSystem.SaveManualGame(manualState, 1));
            Assert.IsTrue(SaveSystem.SaveAutoGame(autoState));

            Assert.IsTrue(File.Exists(SaveSystem.GetManualSaveFilePath(1)));
            Assert.IsTrue(File.Exists(SaveSystem.GetAutoSaveFilePath()));
            Assert.AreNotEqual(SaveSystem.GetManualSaveFilePath(1), SaveSystem.GetAutoSaveFilePath());
            Assert.AreEqual(3, SaveSystem.LoadManualGame(1).currentTurn);
            Assert.AreEqual(9, SaveSystem.LoadAutoGame().currentTurn);
        }
        finally
        {
            SaveSystem.SaveDirectoryOverride = null;
            if (Directory.Exists(saveDirectory))
            {
                Directory.Delete(saveDirectory, true);
            }
        }
    }

    [Test]
    public void AIController_actions_use_player_cost_rules()
    {
        GameObject managerObject = new GameObject("GameManager");
        createdObjects.Add(managerObject);
        GameManager gameManager = managerObject.AddComponent<GameManager>();
        Player aiPlayer = CreatePlayer("AI", RaceType.Heaven, new Vector2Int(9, 9));
        aiPlayer.resources = new ResourceData(65, 130);
        gameManager.aiPlayer = aiPlayer;

        EasyAI easyAI = new EasyAI(gameManager, aiPlayer);

        Assert.IsTrue(easyAI.TryUpgradeStronghold());
        Assert.AreEqual(2, aiPlayer.strongholdLevel);
        Assert.AreEqual(0, aiPlayer.resources.gold);
        Assert.AreEqual(0, aiPlayer.resources.buildingMaterials);

        aiPlayer.resources = new ResourceData(78, 39);
        Assert.IsTrue(easyAI.TrySummonUnit(1));
        Assert.AreEqual(0, aiPlayer.resources.gold);
        Assert.AreEqual(0, aiPlayer.resources.buildingMaterials);
        Assert.AreEqual(1, aiPlayer.deck.CardCount);
        Assert.AreEqual(1, aiPlayer.deck[0].unitIndex);
    }

    [Test]
    public void RemoveCardsBySnapshot_matches_level_and_quantity()
    {
        Player player = CreatePlayer("Player", RaceType.Human, new Vector2Int(0, 0));
        Card levelOne = HumanUnit.CreateCard(0);
        levelOne.quantity = 3;
        Card sameUnitDifferentLevel = HumanUnit.CreateCard(0);
        sameUnitDifferentLevel.level = 2;
        sameUnitDifferentLevel.quantity = 5;
        Card levelTwoSnapshot = HumanUnit.CreateCard(0);
        levelTwoSnapshot.level = 2;
        levelTwoSnapshot.quantity = 2;

        player.deck.AddCard(levelOne);
        player.deck.AddCard(sameUnitDifferentLevel);

        GameManager.RemoveCardsByBattleSnapshot(player.deck, new List<Card> { levelTwoSnapshot });

        Assert.AreEqual(2, player.deck.CardCount);
        Assert.AreEqual(3, player.deck[0].quantity);
        Assert.AreEqual(3, player.deck[1].quantity);
        Assert.AreEqual(2, player.deck[1].level);
    }

    [Test]
    public void CalculateScaleToMatchRendererSize_preserves_aspect_and_matches_largest_side()
    {
        Vector3 scale = MapGenerator.CalculateScaleToMatchRendererSize(
            new Vector3(0.5f, 0.5f, 0.5f),
            new Vector2(2f, 4f),
            new Vector2(1f, 1f));

        Assert.AreEqual(0.125f, scale.x);
        Assert.AreEqual(0.125f, scale.y);
        Assert.AreEqual(0.5f, scale.z);
    }

    private T CreateTile<T>(int x, int y) where T : Tile
    {
        GameObject tileObject = new GameObject(typeof(T).Name);
        createdObjects.Add(tileObject);
        T tile = tileObject.AddComponent<T>();
        tile.gridPosition = new Vector2Int(x, y);
        return tile;
    }

    private static Player CreatePlayer(string playerName, RaceType race, Vector2Int strongholdPos)
    {
        return new Player
        {
            playerName = playerName,
            race = race,
            resources = new ResourceData(100, 100),
            strongholdLevel = 1,
            strongholdPos = strongholdPos,
            deck = new Deck()
        };
    }
}
