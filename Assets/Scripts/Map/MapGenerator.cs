using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MapGenerator : MonoBehaviour
{
    public int width = 16;
    public int height = 10;
    public float tileSize = 1.2f;
    public bool centerMapOnGenerator = true;
    public bool renderTileGrounds = false;
    public float groundVisualScale = 1.0f;
    public float poiVisualScale = 0.72f;
    public float strongholdVisualScale = 0.92f;
    public float overlayVisualScale = 0.98f;
    public float backdropMarginTiles = 1.0f;
    public int neutralGroundPatchCount = 6;
    public float neutralGroundNoiseChance = 0.12f;
    public int minSpecialTileDistance = 2;
    public int resourceCount = 12;
    public int armyCampCount = 8;
    public int eventCount = 6;
    public int obstacleCount = 22;

    public GameObject emptyTilePrefab;
    public GameObject resourceTilePrefab;
    public GameObject armyCampTilePrefab;
    public GameObject eventTilePrefab;
    public GameObject strongholdTilePrefab;
    public GameObject obstacleTilePrefab;

    public MapVisualConfig visualConfig;
    public StrongholdUI strongholdUI;
    public Hero hero;

    private const int StrongholdGroundRadius = 2;

    private readonly Dictionary<Vector2Int, RaceType> raceGroundOverrides = new();
    private readonly Dictionary<Vector2Int, int> neutralGroundOverrides = new();
    private Tile[,] map;
    private Vector3 mapOrigin;
    private RaceType neutralGroundFallbackRace = RaceType.Human;

    private enum TileVisualRole
    {
        Empty,
        Resource,
        ArmyCamp,
        Event,
        Stronghold,
        Obstacle
    }

    void Start()
    {
        TryAutoAssignVisualConfigInEditor();
        CreateBackdrop();
        GenerateMap();
    }

    void GenerateMap()
    {
        map = new Tile[width, height];
        raceGroundOverrides.Clear();
        neutralGroundOverrides.Clear();
        mapOrigin = CalculateMapOrigin();

        if (visualConfig == null)
        {
            Debug.LogWarning("MapGenerator.visualConfig is not assigned. Ground and POI sprites will be empty until Map Visual Config is linked.");
        }

        RaceType playerRace = GameSetupData.IsNewGame ? GameSetupData.PlayerRace : RaceType.Human;
        RaceType enemyRace = GameSetupData.IsNewGame ? GameSetupData.EnemyRace : RaceType.Ghost;
        neutralGroundFallbackRace = playerRace;

        HashSet<Vector2Int> used = new HashSet<Vector2Int>();
        HashSet<Vector2Int> specialPositions = new HashSet<Vector2Int>();

        Vector2Int playerStronghold = new Vector2Int(0, 0);
        Vector2Int enemyStronghold = new Vector2Int(width - 1, height - 1);

        BuildGroundPlan(playerStronghold, playerRace, enemyStronghold, enemyRace);

        PlaceStronghold(playerStronghold, StrongholdType.Player, playerRace);
        used.Add(playerStronghold);
        specialPositions.Add(playerStronghold);
        PlaceStronghold(enemyStronghold, StrongholdType.Enemy, enemyRace);
        used.Add(enemyStronghold);
        specialPositions.Add(enemyStronghold);

        List<Vector2Int> free = new List<Vector2Int>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (!used.Contains(pos))
                {
                    free.Add(pos);
                }
            }
        }

        Shuffle(free);

        int remainingResource = Mathf.Max(0, resourceCount);
        int remainingArmyCamp = Mathf.Max(0, armyCampCount);
        int remainingEvent = Mathf.Max(0, eventCount);

        foreach (var strongholdPos in new[] { playerStronghold, enemyStronghold })
        {
            List<Vector2Int> area = GetFree3x3Area(strongholdPos, used);
            Shuffle(area);

            if (area.Count > 0 && remainingResource > 0)
            {
                PlaceTile(resourceTilePrefab, area[0], TileVisualRole.Resource);
                used.Add(area[0]);
                specialPositions.Add(area[0]);
                free.Remove(area[0]);
                remainingResource--;
            }

            if (area.Count > 1 && remainingArmyCamp > 0)
            {
                PlaceTile(armyCampTilePrefab, area[1], TileVisualRole.ArmyCamp);
                used.Add(area[1]);
                specialPositions.Add(area[1]);
                free.Remove(area[1]);
                remainingArmyCamp--;
            }
        }

        PlaceSpecialTiles(resourceTilePrefab, TileVisualRole.Resource, remainingResource, free, used, specialPositions);
        PlaceSpecialTiles(armyCampTilePrefab, TileVisualRole.ArmyCamp, remainingArmyCamp, free, used, specialPositions);
        PlaceSpecialTiles(eventTilePrefab, TileVisualRole.Event, remainingEvent, free, used, specialPositions);
        PlaceObstacleTiles(free, used, specialPositions, playerStronghold, enemyStronghold);

        foreach (Vector2Int pos in free)
        {
            PlaceTile(emptyTilePrefab, pos, TileVisualRole.Empty);
        }

        if (hero != null)
        {
            hero.currentGridPos = playerStronghold;
            hero.transform.position = GridToWorld(playerStronghold);
            hero.SetRaceAppearance(playerRace);
        }

        MapManager.Instance.SetMap(map, mapOrigin, tileSize);
        Debug.Log("Map generated.");
    }

    void PlaceStronghold(Vector2Int pos, StrongholdType type, RaceType race)
    {
        Vector3 worldPos = GridToWorld(pos);
        GameObject obj = Instantiate(strongholdTilePrefab, worldPos, Quaternion.identity, transform);
        StrongholdTile tile = obj.GetComponent<StrongholdTile>();
        tile.gridPosition = pos;
        tile.strongholdType = type;
        tile.visualRace = race;

        if (type == StrongholdType.Player)
        {
            tile.strongholdUI = strongholdUI;
        }

        ApplyTileVisual(obj, pos, GetStrongholdSprite(race), TileVisualRole.Stronghold);
        map[pos.x, pos.y] = tile;
    }

    void PlaceTile(GameObject prefab, Vector2Int pos, TileVisualRole visualRole)
    {
        Vector3 worldPos = GridToWorld(pos);
        GameObject obj = Instantiate(prefab, worldPos, Quaternion.identity, transform);
        Tile tile = obj.GetComponent<Tile>();
        tile.gridPosition = pos;

        ApplyTileVisual(obj, pos, GetPoiSprite(visualRole), visualRole);
        map[pos.x, pos.y] = tile;
    }

    void ApplyTileVisual(GameObject obj, Vector2Int pos, Sprite poiSprite, TileVisualRole visualRole)
    {
        Sprite groundSprite = renderTileGrounds ? GetGroundSprite(pos) : null;
        TileVisual tileVisual = obj.GetComponent<TileVisual>();
        if (tileVisual != null)
        {
            tileVisual.ConfigureForGeneratedTile();
            tileVisual.SetGround(groundSprite, tileSize * groundVisualScale);
            tileVisual.SetPoi(poiSprite, tileSize * GetPoiScale(visualRole));
            tileVisual.SetOverlay(null, false, tileSize * overlayVisualScale);
            return;
        }

        SpriteRenderer rootRenderer = obj.GetComponent<SpriteRenderer>();
        if (rootRenderer == null) return;

        Sprite fallbackSprite = poiSprite != null ? poiSprite : groundSprite;
        if (fallbackSprite != null)
        {
            rootRenderer.sprite = fallbackSprite;
        }

        rootRenderer.color = Color.white;
        FitRendererToWorldSize(rootRenderer, tileSize * GetPoiScale(visualRole));
    }

    Sprite GetGroundSprite(Vector2Int pos)
    {
        if (visualConfig == null) return null;

        if (raceGroundOverrides.TryGetValue(pos, out RaceType race))
        {
            return visualConfig.GetRaceGroundSprite(race);
        }

        if (neutralGroundOverrides.TryGetValue(pos, out int neutralGroundIndex))
        {
            return visualConfig.GetNeutralGroundSprite(neutralGroundIndex, neutralGroundFallbackRace);
        }

        return visualConfig.GetNeutralGroundSprite(0, neutralGroundFallbackRace);
    }

    Vector3 GridToWorld(Vector2Int pos)
    {
        return mapOrigin + new Vector3(pos.x * tileSize, pos.y * tileSize, 0);
    }

    Vector3 CalculateMapOrigin()
    {
        if (!centerMapOnGenerator)
        {
            return transform.position;
        }

        return transform.position - new Vector3((width - 1) * tileSize * 0.5f, (height - 1) * tileSize * 0.5f, 0);
    }

    void CreateBackdrop()
    {
        if (visualConfig == null || visualConfig.backdropSprite == null) return;

        GameObject backdropObj = new GameObject("Backdrop");
        backdropObj.transform.SetParent(transform);
        backdropObj.transform.position = CalculateMapOrigin()
            + new Vector3((width - 1) * tileSize * 0.5f, (height - 1) * tileSize * 0.5f, 0);

        SpriteRenderer renderer = backdropObj.AddComponent<SpriteRenderer>();
        renderer.sprite = visualConfig.backdropSprite;
        renderer.sortingOrder = -10;

        Vector2 spriteSize = visualConfig.backdropSprite.bounds.size;
        float mapWorldWidth = width * tileSize;
        float mapWorldHeight = height * tileSize;
        float margin = tileSize * Mathf.Max(0f, backdropMarginTiles);
        float scaleX = (mapWorldWidth + margin) / spriteSize.x;
        float scaleY = (mapWorldHeight + margin) / spriteSize.y;
        float scale = Mathf.Max(scaleX, scaleY);
        backdropObj.transform.localScale = new Vector3(scale, scale, 1);
    }

    float GetPoiScale(TileVisualRole visualRole)
    {
        return visualRole == TileVisualRole.Stronghold ? strongholdVisualScale : poiVisualScale;
    }

    static void FitRendererToWorldSize(SpriteRenderer renderer, float targetWorldSize)
    {
        if (renderer.sprite == null || targetWorldSize <= 0f) return;

        Vector2 spriteSize = renderer.sprite.bounds.size;
        float largestSide = Mathf.Max(spriteSize.x, spriteSize.y);
        if (largestSide <= 0f) return;

        float scale = targetWorldSize / largestSide;
        renderer.transform.localScale = new Vector3(scale, scale, renderer.transform.localScale.z);
    }

    Sprite GetStrongholdSprite(RaceType race)
    {
        return visualConfig != null ? visualConfig.GetStrongholdSprite(race) : null;
    }

    void TryAutoAssignVisualConfigInEditor()
    {
#if UNITY_EDITOR
        if (visualConfig != null)
        {
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:MapVisualConfig");
        if (guids.Length == 0)
        {
            return;
        }

        string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
        visualConfig = AssetDatabase.LoadAssetAtPath<MapVisualConfig>(assetPath);
#endif
    }

    Sprite GetPoiSprite(TileVisualRole visualRole)
    {
        if (visualConfig == null) return null;

        return visualRole switch
        {
            TileVisualRole.Resource => visualConfig.resourcePoi,
            TileVisualRole.ArmyCamp => visualConfig.armyCampPoi,
            TileVisualRole.Event => visualConfig.eventPoi,
            TileVisualRole.Obstacle => visualConfig.GetObstaclePoiSprite(),
            _ => null
        };
    }

    void BuildGroundPlan(Vector2Int playerStronghold, RaceType playerRace, Vector2Int enemyStronghold, RaceType enemyRace)
    {
        MarkRaceGroundArea(playerStronghold, playerRace);
        MarkRaceGroundArea(enemyStronghold, enemyRace);
        BuildNeutralGroundPatches();
    }

    void BuildNeutralGroundPatches()
    {
        int groundCount = visualConfig != null && visualConfig.neutralGrounds != null
            ? visualConfig.neutralGrounds.Length
            : 0;

        if (groundCount == 0)
        {
            return;
        }

        List<Vector2Int> seeds = new List<Vector2Int>();
        int patchCount = Mathf.Max(1, neutralGroundPatchCount);
        for (int i = 0; i < patchCount; i++)
        {
            seeds.Add(new Vector2Int(Random.Range(0, width), Random.Range(0, height)));
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (raceGroundOverrides.ContainsKey(pos))
                {
                    continue;
                }

                int nearestSeed = 0;
                int nearestDistance = int.MaxValue;
                for (int i = 0; i < seeds.Count; i++)
                {
                    int distance = Mathf.Abs(pos.x - seeds[i].x) + Mathf.Abs(pos.y - seeds[i].y);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestSeed = i;
                    }
                }

                int groundIndex = nearestSeed % groundCount;
                if (Random.value < neutralGroundNoiseChance)
                {
                    groundIndex = Random.Range(0, groundCount);
                }

                neutralGroundOverrides[pos] = groundIndex;
            }
        }
    }

    void PlaceSpecialTiles(
        GameObject prefab,
        TileVisualRole visualRole,
        int count,
        List<Vector2Int> free,
        HashSet<Vector2Int> used,
        HashSet<Vector2Int> specialPositions)
    {
        for (int i = 0; i < count && free.Count > 0; i++)
        {
            int candidateIndex = FindSpecialTileCandidate(free, specialPositions);
            Vector2Int pos = free[candidateIndex];
            free.RemoveAt(candidateIndex);

            PlaceTile(prefab, pos, visualRole);
            used.Add(pos);
            specialPositions.Add(pos);
        }
    }

    void PlaceObstacleTiles(
        List<Vector2Int> free,
        HashSet<Vector2Int> used,
        HashSet<Vector2Int> specialPositions,
        Vector2Int playerStronghold,
        Vector2Int enemyStronghold)
    {
        if (obstacleTilePrefab == null)
        {
            if (obstacleCount > 0)
            {
                Debug.LogWarning("MapGenerator.obstacleTilePrefab is not assigned. Obstacles will not be generated.");
            }

            return;
        }

        int placed = 0;
        int requestedCount = Mathf.Max(0, obstacleCount);
        HashSet<Vector2Int> blockedPositions = new HashSet<Vector2Int>();

        for (int i = 0; i < requestedCount && free.Count > 0; i++)
        {
            int candidateIndex = FindObstacleCandidate(
                free,
                blockedPositions,
                specialPositions,
                playerStronghold,
                enemyStronghold);

            if (candidateIndex < 0)
            {
                break;
            }

            Vector2Int pos = free[candidateIndex];
            free.RemoveAt(candidateIndex);

            PlaceTile(obstacleTilePrefab, pos, TileVisualRole.Obstacle);
            used.Add(pos);
            blockedPositions.Add(pos);
            placed++;
        }

        if (placed < requestedCount)
        {
            Debug.LogWarning($"Generated {placed}/{requestedCount} obstacles. Remaining candidates would block required paths.");
        }
    }

    int FindObstacleCandidate(
        List<Vector2Int> free,
        HashSet<Vector2Int> blockedPositions,
        HashSet<Vector2Int> specialPositions,
        Vector2Int playerStronghold,
        Vector2Int enemyStronghold)
    {
        for (int i = 0; i < free.Count; i++)
        {
            Vector2Int candidate = free[i];
            if (IsNearStronghold(candidate, playerStronghold) || IsNearStronghold(candidate, enemyStronghold))
            {
                continue;
            }

            HashSet<Vector2Int> testBlocked = new HashSet<Vector2Int>(blockedPositions)
            {
                candidate
            };

            if (CanReachAllRequiredTiles(playerStronghold, specialPositions, testBlocked))
            {
                return i;
            }
        }

        return -1;
    }

    bool CanReachAllRequiredTiles(
        Vector2Int start,
        HashSet<Vector2Int> requiredPositions,
        HashSet<Vector2Int> blockedPositions)
    {
        HashSet<Vector2Int> reachable = FloodFillWalkable(start, blockedPositions);
        foreach (Vector2Int required in requiredPositions)
        {
            if (!reachable.Contains(required))
            {
                return false;
            }
        }

        return true;
    }

    HashSet<Vector2Int> FloodFillWalkable(Vector2Int start, HashSet<Vector2Int> blockedPositions)
    {
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        Queue<Vector2Int> frontier = new Queue<Vector2Int>();

        if (!IsInsideMap(start) || blockedPositions.Contains(start))
        {
            return visited;
        }

        frontier.Enqueue(start);
        visited.Add(start);

        while (frontier.Count > 0)
        {
            Vector2Int current = frontier.Dequeue();
            foreach (Vector2Int next in GetCardinalNeighbors(current))
            {
                if (!IsInsideMap(next) || blockedPositions.Contains(next) || !visited.Add(next))
                {
                    continue;
                }

                frontier.Enqueue(next);
            }
        }

        return visited;
    }

    bool IsInsideMap(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }

    static bool IsNearStronghold(Vector2Int pos, Vector2Int stronghold)
    {
        return Mathf.Abs(pos.x - stronghold.x) <= 1 && Mathf.Abs(pos.y - stronghold.y) <= 1;
    }

    static IEnumerable<Vector2Int> GetCardinalNeighbors(Vector2Int pos)
    {
        yield return new Vector2Int(pos.x + 1, pos.y);
        yield return new Vector2Int(pos.x - 1, pos.y);
        yield return new Vector2Int(pos.x, pos.y + 1);
        yield return new Vector2Int(pos.x, pos.y - 1);
    }

    int FindSpecialTileCandidate(List<Vector2Int> free, HashSet<Vector2Int> specialPositions)
    {
        int requiredDistance = Mathf.Max(1, minSpecialTileDistance);

        for (int i = 0; i < free.Count; i++)
        {
            if (IsFarEnoughFromSpecialTiles(free[i], specialPositions, requiredDistance))
            {
                return i;
            }
        }

        return 0;
    }

    bool IsFarEnoughFromSpecialTiles(Vector2Int pos, HashSet<Vector2Int> specialPositions, int requiredDistance)
    {
        foreach (Vector2Int special in specialPositions)
        {
            int distance = Mathf.Abs(pos.x - special.x) + Mathf.Abs(pos.y - special.y);
            if (distance < requiredDistance)
            {
                return false;
            }
        }

        return true;
    }

    void MarkRaceGroundArea(Vector2Int center, RaceType race)
    {
        for (int dx = -StrongholdGroundRadius; dx <= StrongholdGroundRadius; dx++)
        {
            for (int dy = -StrongholdGroundRadius; dy <= StrongholdGroundRadius; dy++)
            {
                Vector2Int pos = new Vector2Int(center.x + dx, center.y + dy);
                if (pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height)
                {
                    raceGroundOverrides[pos] = race;
                }
            }
        }
    }

    List<Vector2Int> GetFree3x3Area(Vector2Int center, HashSet<Vector2Int> used)
    {
        List<Vector2Int> area = new List<Vector2Int>();
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                Vector2Int pos = new Vector2Int(center.x + dx, center.y + dy);
                if (pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height && !used.Contains(pos))
                {
                    area.Add(pos);
                }
            }
        }

        return area;
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int r = Random.Range(0, i + 1);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }
}
