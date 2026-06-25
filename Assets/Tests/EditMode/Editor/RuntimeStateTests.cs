using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class RuntimeStateTests
{
    private readonly List<GameObject> createdObjects = new List<GameObject>();

    [TearDown]
    public void TearDown()
    {
        GameSession.ResetForTests();

        foreach (GameObject createdObject in createdObjects)
        {
            Object.DestroyImmediate(createdObject);
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

    private T CreateTile<T>(int x, int y) where T : Tile
    {
        GameObject tileObject = new GameObject(typeof(T).Name);
        createdObjects.Add(tileObject);
        T tile = tileObject.AddComponent<T>();
        tile.gridPosition = new Vector2Int(x, y);
        return tile;
    }
}
