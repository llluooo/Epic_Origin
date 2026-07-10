using System;
using System.Collections.Generic;
using UnityEngine;

public enum TileKind
{
    Empty,
    Resource,
    ArmyCamp,
    Event,
    Stronghold,
    Obstacle
}

[Serializable]
public class GameRunState
{
    public int currentTurn;
    public int maxTurn;
    public bool isPlayerTurn;
    public bool hasPlayerActed;
    public bool isBattleActive;
    public bool gameEnded;
    public GameManager.GameState currentState;
    public Player player;
    public Player aiPlayer;
    public Vector2Int heroGridPos;
    public Vector2Int aiHeroGridPos;
    public bool hasAIHeroGridPos;
    public MapState mapState;

    public static GameRunState Capture(GameManager gameManager, Hero hero, MapState mapState)
    {
        return Capture(gameManager, hero != null ? hero.currentGridPos : Vector2Int.zero, mapState);
    }

    public static GameRunState Capture(GameManager gameManager, Vector2Int heroGridPos, MapState mapState)
    {
        Vector2Int aiHeroGridPos = gameManager.aiHero != null
            ? gameManager.aiHero.currentGridPos
            : gameManager.aiPlayer != null
                ? gameManager.aiPlayer.strongholdPos
                : Vector2Int.zero;
        return Capture(gameManager, heroGridPos, aiHeroGridPos, mapState);
    }

    public static GameRunState Capture(GameManager gameManager, Vector2Int heroGridPos, Vector2Int aiHeroGridPos, MapState mapState)
    {
        return new GameRunState
        {
            currentTurn = gameManager.currentTurn,
            maxTurn = gameManager.maxTurn,
            isPlayerTurn = gameManager.isPlayerTurn,
            hasPlayerActed = gameManager.hasPlayerActed,
            isBattleActive = false,
            gameEnded = gameManager.IsGameEnded,
            currentState = gameManager.currentState,
            player = ClonePlayer(gameManager.player),
            aiPlayer = ClonePlayer(gameManager.aiPlayer),
            heroGridPos = heroGridPos,
            aiHeroGridPos = aiHeroGridPos,
            hasAIHeroGridPos = true,
            mapState = mapState?.Clone()
        };
    }

    public GameRunState Clone()
    {
        return new GameRunState
        {
            currentTurn = currentTurn,
            maxTurn = maxTurn,
            isPlayerTurn = isPlayerTurn,
            hasPlayerActed = hasPlayerActed,
            isBattleActive = isBattleActive,
            gameEnded = gameEnded,
            currentState = currentState,
            player = ClonePlayer(player),
            aiPlayer = ClonePlayer(aiPlayer),
            heroGridPos = heroGridPos,
            aiHeroGridPos = aiHeroGridPos,
            hasAIHeroGridPos = hasAIHeroGridPos,
            mapState = mapState?.Clone()
        };
    }

    public static Player ClonePlayer(Player source)
    {
        if (source == null)
        {
            return null;
        }

        return new Player
        {
            playerName = source.playerName,
            race = source.race,
            resources = new ResourceData(source.resources?.gold ?? 0, source.resources?.buildingMaterials ?? 0),
            strongholdLevel = source.strongholdLevel,
            deck = CloneDeck(source.deck),
            garrisonDeck = CloneDeck(source.garrisonDeck),
            strongholdPos = source.strongholdPos,
            capturedResourceCount = source.capturedResourceCount
        };
    }

    public static Deck CloneDeck(Deck source)
    {
        Deck clone = new Deck();
        if (source == null)
        {
            return clone;
        }

        clone.hasSlotLimit = source.hasSlotLimit;
        foreach (Card card in source.cards)
        {
            clone.AddCard(card?.Clone());
        }

        return clone;
    }

    public static List<Card> CloneCardList(List<Card> source)
    {
        if (source == null)
        {
            return null;
        }

        List<Card> clone = new List<Card>(source.Count);
        foreach (Card card in source)
        {
            clone.Add(card?.Clone());
        }

        return clone;
    }
}

[Serializable]
public class MapState
{
    public int width;
    public int height;
    public float tileSize;
    public Vector2 worldOrigin;
    public List<TileState> tiles = new List<TileState>();

    public static MapState Capture(Tile[,] map, Vector2 worldOrigin, float tileSize)
    {
        MapState state = new MapState
        {
            width = map.GetLength(0),
            height = map.GetLength(1),
            worldOrigin = worldOrigin,
            tileSize = tileSize
        };

        for (int x = 0; x < state.width; x++)
        {
            for (int y = 0; y < state.height; y++)
            {
                Tile tile = map[x, y];
                if (tile != null)
                {
                    state.tiles.Add(TileState.FromTile(tile));
                }
            }
        }

        return state;
    }

    public TileState GetTile(Vector2Int position)
    {
        foreach (TileState tile in tiles)
        {
            if (tile.position == position)
            {
                return tile;
            }
        }

        return null;
    }

    public MapState Clone()
    {
        MapState clone = new MapState
        {
            width = width,
            height = height,
            tileSize = tileSize,
            worldOrigin = worldOrigin,
            tiles = new List<TileState>(tiles.Count)
        };

        foreach (TileState tile in tiles)
        {
            clone.tiles.Add(tile?.Clone());
        }

        return clone;
    }
}

[Serializable]
public class TileState
{
    public Vector2Int position;
    public TileKind kind;
    public StrongholdType strongholdType;
    public RaceType visualRace;
    public bool cleared;
    public List<Card> armyCampDefenders;

    public static TileState FromTile(Tile tile)
    {
        TileState state = new TileState
        {
            position = tile.gridPosition,
            kind = GetKind(tile),
            cleared = tile.Cleared
        };

        if (tile is StrongholdTile stronghold)
        {
            state.strongholdType = stronghold.strongholdType;
            state.visualRace = stronghold.visualRace;
        }

        return state;
    }

    public TileState Clone()
    {
        return new TileState
        {
            position = position,
            kind = kind,
            strongholdType = strongholdType,
            visualRace = visualRace,
            cleared = cleared,
            armyCampDefenders = GameRunState.CloneCardList(armyCampDefenders)
        };
    }

    private static TileKind GetKind(Tile tile)
    {
        if (tile is StrongholdTile) return TileKind.Stronghold;
        if (tile is ResourceTile) return TileKind.Resource;
        if (tile is ArmyCampTile) return TileKind.ArmyCamp;
        if (tile is EventTile) return TileKind.Event;
        if (tile is ObstacleTile) return TileKind.Obstacle;
        return TileKind.Empty;
    }
}

[Serializable]
public class PendingBattleState
{
    public BattleEncounterType encounterType;
    public Vector2Int sourceTilePos;
    public bool playerStartsAttacking = true;
    public List<Card> playerDeck;
    public List<Card> enemyDeck;

    public PendingBattleState Clone()
    {
        return new PendingBattleState
        {
            encounterType = encounterType,
            sourceTilePos = sourceTilePos,
            playerStartsAttacking = playerStartsAttacking,
            playerDeck = GameRunState.CloneCardList(playerDeck),
            enemyDeck = GameRunState.CloneCardList(enemyDeck)
        };
    }
}

[Serializable]
public class PendingBattleResult
{
    public BattleEncounterType encounterType;
    public Vector2Int sourceTilePos;
    public BattleOutcome outcome;

    public PendingBattleResult Clone()
    {
        return new PendingBattleResult
        {
            encounterType = encounterType,
            sourceTilePos = sourceTilePos,
            outcome = outcome
        };
    }
}
