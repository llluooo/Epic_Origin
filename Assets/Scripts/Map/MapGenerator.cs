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
    public float resourcePoiScale = 0.55f;
    public float armyCampPoiScale = 0.55f;
    public float eventPoiScale = 0.55f;
    public float obstaclePoiScale = 0.55f;
    public float strongholdVisualScale = 0.75f;
    public float overlayVisualScale = 0.85f;
    public float backdropMarginTiles = 2.5f;
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
    public SpriteRenderer backdropRenderer;
    public StrongholdUI strongholdUI;
    public Hero hero;
    public GameObject aiHeroPrefab;

    private Tile[,] map;
    private Vector3 mapOrigin;

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

        if (GameSession.HasRunState && GameSession.RunState.mapState != null)
        {
            MapState state = GameSession.RunState.mapState;
            width = state.width;
            height = state.height;
            tileSize = state.tileSize;
            mapOrigin = state.worldOrigin;
            ConfigureBackdrop();
            RestoreMap(state);
            return;
        }

        ConfigureBackdrop();
        GenerateMap();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        TryAutoAssignVisualConfigInEditor();
        ConfigureBackdrop();
    }
#endif

    void GenerateMap()
    {
        map = new Tile[width, height];
        mapOrigin = CalculateMapOrigin();

        if (visualConfig == null)
        {
            Debug.LogWarning("地图生成器未绑定地图视觉配置。在关联配置前，兴趣点贴图会为空。");
        }

        RaceType playerRace = GameSetupData.IsNewGame ? GameSetupData.PlayerRace : RaceType.Human;
        RaceType enemyRace = GameSetupData.IsNewGame ? GameSetupData.EnemyRace : RaceType.Ghost;

        HashSet<Vector2Int> used = new HashSet<Vector2Int>();
        HashSet<Vector2Int> specialPositions = new HashSet<Vector2Int>();

        Vector2Int playerStronghold = new Vector2Int(0, 0);
        Vector2Int enemyStronghold = new Vector2Int(width - 1, height - 1);

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
            hero.transform.position = GridToWorld(playerStronghold) + hero.visualOffset;
            hero.SetRaceAppearance(playerRace);
        }

        SpawnAIHero(enemyStronghold, enemyRace);

        MapManager.Instance.SetMap(map, mapOrigin, tileSize);
        Debug.Log("地图已生成。");
    }

    public MapState CaptureMapState()
    {
        if (map == null)
        {
            Debug.LogError("地图生成器：地图尚未初始化，无法捕获地图状态。");
            return null;
        }

        return MapState.Capture(map, mapOrigin, tileSize);
    }

    void RestoreMap(MapState state)
    {
        map = new Tile[state.width, state.height];
        mapOrigin = state.worldOrigin;

        if (visualConfig == null)
        {
            Debug.LogWarning("地图生成器未绑定地图视觉配置。恢复地图时兴趣点贴图会为空，直到关联配置。");
        }

        foreach (TileState tileState in state.tiles)
        {
            if (tileState == null || !IsInsideState(state, tileState.position))
            {
                continue;
            }

            PlaceRestoredTile(tileState);
        }

        for (int x = 0; x < state.width; x++)
        {
            for (int y = 0; y < state.height; y++)
            {
                if (map[x, y] == null)
                {
                    PlaceTile(emptyTilePrefab, new Vector2Int(x, y), TileVisualRole.Empty);
                }
            }
        }

        RestoreHeroPosition();

        MapManager.Instance.SetMap(map, mapOrigin, tileSize);
        Debug.Log("已从运行会话恢复地图。");
    }

    void PlaceRestoredTile(TileState state)
    {
        if (state.kind == TileKind.Stronghold)
        {
            PlaceStronghold(state.position, state.strongholdType, state.visualRace);
            return;
        }

        PlaceTile(GetPrefabForTileKind(state.kind), state.position, GetVisualRoleForTileKind(state.kind));
    }

    GameObject GetPrefabForTileKind(TileKind kind)
    {
        GameObject prefab = kind switch
        {
            TileKind.Resource => resourceTilePrefab,
            TileKind.ArmyCamp => armyCampTilePrefab,
            TileKind.Event => eventTilePrefab,
            TileKind.Obstacle => obstacleTilePrefab,
            _ => emptyTilePrefab
        };

        return prefab != null ? prefab : emptyTilePrefab;
    }

    TileVisualRole GetVisualRoleForTileKind(TileKind kind)
    {
        return kind switch
        {
            TileKind.Resource => TileVisualRole.Resource,
            TileKind.ArmyCamp => TileVisualRole.ArmyCamp,
            TileKind.Event => TileVisualRole.Event,
            TileKind.Obstacle => TileVisualRole.Obstacle,
            _ => TileVisualRole.Empty
        };
    }

    void RestoreHeroPosition()
    {
        if (hero == null || !GameSession.HasRunState)
        {
            return;
        }

        Vector2Int heroPos = GameSession.RunState.heroGridPos;
        hero.currentGridPos = heroPos;
        hero.transform.position = GridToWorld(heroPos) + hero.visualOffset;

        RaceType playerRace = GameSession.RunState.player != null ? GameSession.RunState.player.race : RaceType.Human;
        hero.SetRaceAppearance(playerRace);

        RaceType aiRace = GameSession.RunState.aiPlayer != null ? GameSession.RunState.aiPlayer.race : RaceType.Ghost;
        Vector2Int aiPos = GameSession.RunState.aiPlayer != null ? GameSession.RunState.aiPlayer.strongholdPos : new Vector2Int(width - 1, height - 1);
        SpawnAIHero(aiPos, aiRace);
    }

    void SpawnAIHero(Vector2Int startPos, RaceType race)
    {
        if (aiHeroPrefab == null)
        {
            Debug.LogWarning("地图生成器未绑定 AI 英雄预制体，不会生成 AI 英雄。");
            return;
        }

        GameObject aiHeroObj = Instantiate(aiHeroPrefab);
        AIHero aiHeroComponent = aiHeroObj.GetComponent<AIHero>();

        if (aiHeroComponent == null)
        {
            Debug.LogError("AI 英雄预制体上没有 AIHero 组件！");
            Destroy(aiHeroObj);
            return;
        }

        aiHeroComponent.SetPosition(startPos, GridToWorld(startPos));
        aiHeroComponent.SetRaceAppearance(race);

        SpriteRenderer sr = aiHeroObj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = 30;
        }

        aiHeroObj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.aiHero = aiHeroComponent;
        }

        Debug.Log($"AI 英雄已生成在格子 {startPos}，种族 {race}");
    }

    static bool IsInsideState(MapState state, Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < state.width && pos.y >= 0 && pos.y < state.height;
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

        ApplyTileVisual(obj, GetStrongholdSprite(race), TileVisualRole.Stronghold);
        map[pos.x, pos.y] = tile;
    }

    void PlaceTile(GameObject prefab, Vector2Int pos, TileVisualRole visualRole)
    {
        Vector3 worldPos = GridToWorld(pos);
        GameObject obj = Instantiate(prefab, worldPos, Quaternion.identity, transform);
        Tile tile = obj.GetComponent<Tile>();
        tile.gridPosition = pos;

        ApplyTileVisual(obj, GetPoiSprite(visualRole), visualRole);
        map[pos.x, pos.y] = tile;
    }

    void ApplyTileVisual(GameObject obj, Sprite poiSprite, TileVisualRole visualRole)
    {
        TileVisual tileVisual = obj.GetComponent<TileVisual>();
        if (tileVisual != null)
        {
            tileVisual.ConfigureForGeneratedTile();
            tileVisual.SetGround(null, 0f);
            tileVisual.SetPoi(poiSprite, tileSize * GetPoiScale(visualRole));
            tileVisual.SetOverlay(null, false, tileSize * overlayVisualScale);
            return;
        }

        SpriteRenderer rootRenderer = obj.GetComponent<SpriteRenderer>();
        if (rootRenderer == null || poiSprite == null) return;

        rootRenderer.sprite = poiSprite;
        rootRenderer.color = Color.white;
        FitRendererToWorldSize(rootRenderer, tileSize * GetPoiScale(visualRole));
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

    void ConfigureBackdrop()
    {
        SpriteRenderer renderer = GetBackdropRenderer();
        if (visualConfig == null || visualConfig.backdropSprite == null || renderer == null)
        {
            return;
        }

        renderer.sprite = visualConfig.backdropSprite;
        renderer.sortingOrder = -10;
        renderer.drawMode = SpriteDrawMode.Simple;
        renderer.transform.position = CalculateMapOrigin()
            + new Vector3((width - 1) * tileSize * 0.5f, (height - 1) * tileSize * 0.5f, 0);

        Vector2 spriteSize = visualConfig.backdropSprite.bounds.size;
        if (spriteSize.x <= 0f || spriteSize.y <= 0f)
        {
            return;
        }

        float mapWorldWidth = width * tileSize;
        float mapWorldHeight = height * tileSize;
        float margin = tileSize * Mathf.Max(0f, backdropMarginTiles);
        float scaleX = (mapWorldWidth + margin) / spriteSize.x;
        float scaleY = (mapWorldHeight + margin) / spriteSize.y;
        float scale = Mathf.Max(scaleX, scaleY);
        renderer.transform.localScale = new Vector3(scale, scale, 1);
    }

    SpriteRenderer GetBackdropRenderer()
    {
        if (backdropRenderer != null)
        {
            return backdropRenderer;
        }

        Transform child = transform.Find("MapBackdrop");
        if (child != null)
        {
            backdropRenderer = child.GetComponent<SpriteRenderer>();
        }

        return backdropRenderer;
    }

    float GetPoiScale(TileVisualRole visualRole)
    {
        return visualRole switch
        {
            TileVisualRole.Resource => resourcePoiScale,
            TileVisualRole.ArmyCamp => armyCampPoiScale,
            TileVisualRole.Event => eventPoiScale,
            TileVisualRole.Obstacle => obstaclePoiScale,
            TileVisualRole.Stronghold => strongholdVisualScale,
            _ => resourcePoiScale
        };
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
                Debug.LogWarning("地图生成器未绑定障碍物格子预制体，不会生成障碍物。");
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
            Debug.LogWarning($"已生成 {placed}/{requestedCount} 个障碍物。剩余候选位置会阻断必要路径。");
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
