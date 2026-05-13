using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 地图生成器
/// 按策划案精确数量生成10x10地图：
/// 据点2个(固定位置)、资源8个、兵营6个、事件4个、空地80个
/// </summary>
public class MapGenerator : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public float tileSize = 1.2f;

    public GameObject emptyTilePrefab;
    public GameObject resourceTilePrefab;
    public GameObject armyCampTilePrefab;
    public GameObject eventTilePrefab;
    public GameObject strongholdTilePrefab;

    public StrongholdUI strongholdUI;   // 据点经营面板
    public Hero hero;                   // 玩家英雄

    // 策划案精确数量
    private const int ResourceCount = 8;
    private const int ArmyCampCount = 6;
    private const int EventCount = 4;

    private Tile[,] map;

    void Start()
    {
        GenerateMap();
    }

    void GenerateMap()
    {
        map = new Tile[width, height];
        HashSet<Vector2Int> used = new HashSet<Vector2Int>();

        // 1. 固定据点位置
        Vector2Int playerStronghold = new Vector2Int(0, 0);
        Vector2Int enemyStronghold = new Vector2Int(width - 1, height - 1);

        PlaceStronghold(playerStronghold, StrongholdType.Player);
        used.Add(playerStronghold);
        PlaceStronghold(enemyStronghold, StrongholdType.Enemy);
        used.Add(enemyStronghold);

        // 2. 收集所有剩余位置并打乱
        List<Vector2Int> free = new List<Vector2Int>();
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (!used.Contains(pos))
                    free.Add(pos);
            }
        Shuffle(free);

        int remainingResource = ResourceCount;
        int remainingArmyCamp = ArmyCampCount;
        int remainingEvent = EventCount;

        // 3. 保证据点周围3x3区域至少有1资源+1兵营
        foreach (var strongholdPos in new[] { playerStronghold, enemyStronghold })
        {
            List<Vector2Int> area = GetFree3x3Area(strongholdPos, used);
            Shuffle(area);

            if (area.Count > 0 && remainingResource > 0)
            {
                PlaceTile(resourceTilePrefab, area[0]);
                used.Add(area[0]);
                free.Remove(area[0]);
                remainingResource--;
            }

            if (area.Count > 1 && remainingArmyCamp > 0)
            {
                PlaceTile(armyCampTilePrefab, area[1]);
                used.Add(area[1]);
                free.Remove(area[1]);
                remainingArmyCamp--;
            }
        }

        // 4. 按精确数量填充剩余格子
        int index = 0;

        for (int i = 0; i < remainingResource && index < free.Count; i++, index++)
            PlaceTile(resourceTilePrefab, free[index]);

        for (int i = 0; i < remainingArmyCamp && index < free.Count; i++, index++)
            PlaceTile(armyCampTilePrefab, free[index]);

        for (int i = 0; i < remainingEvent && index < free.Count; i++, index++)
            PlaceTile(eventTilePrefab, free[index]);

        for (; index < free.Count; index++)
            PlaceTile(emptyTilePrefab, free[index]);

        // 把英雄放到玩家据点位置
        if (hero != null)
        {
            hero.currentGridPos = playerStronghold;
            hero.transform.position = new Vector3(playerStronghold.x * tileSize, playerStronghold.y * tileSize, 0);
            hero.SetRaceAppearance(GameSetupData.PlayerRace);
        }

        MapManager.Instance.SetMap(map);
        Debug.Log("地图生成完成！");
    }

    void PlaceStronghold(Vector2Int pos, StrongholdType type)
    {
        Vector3 worldPos = new Vector3(pos.x * tileSize, pos.y * tileSize, 0);
        GameObject obj = Instantiate(strongholdTilePrefab, worldPos, Quaternion.identity, transform);
        StrongholdTile tile = obj.GetComponent<StrongholdTile>();
        tile.gridPosition = pos;
        tile.strongholdType = type;

        if (type == StrongholdType.Player)
        {
            tile.strongholdUI = strongholdUI;
        }

        map[pos.x, pos.y] = tile;
    }

    void PlaceTile(GameObject prefab, Vector2Int pos)
    {
        Vector3 worldPos = new Vector3(pos.x * tileSize, pos.y * tileSize, 0);
        GameObject obj = Instantiate(prefab, worldPos, Quaternion.identity, transform);
        Tile tile = obj.GetComponent<Tile>();
        tile.gridPosition = pos;
        map[pos.x, pos.y] = tile;
    }

    List<Vector2Int> GetFree3x3Area(Vector2Int center, HashSet<Vector2Int> used)
    {
        List<Vector2Int> area = new List<Vector2Int>();
        for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                Vector2Int pos = new Vector2Int(center.x + dx, center.y + dy);
                if (pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height && !used.Contains(pos))
                    area.Add(pos);
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
