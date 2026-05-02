using UnityEngine;

/// <summary>
/// 地图生成器
/// 负责生成10x10地图
/// </summary>
public class MapGenerator : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public float tileSize = 1.2f;

    // Tile预制体
    public GameObject emptyTilePrefab;
    public GameObject resourceTilePrefab;
    public GameObject armyCampTilePrefab;
    public GameObject eventTilePrefab;
    public GameObject strongholdTilePrefab;

    private Tile[,] map;

    void Start()
    {
        GenerateMap();
    }

    void GenerateMap()
    {
        map = new Tile[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject prefab = GetRandomTile();

                Vector3 pos = new Vector3(x * tileSize, y * tileSize, 0);

                GameObject tileObj = Instantiate(prefab, pos, Quaternion.identity, transform);

                Tile tile = tileObj.GetComponent<Tile>();
                tile.gridPosition = new Vector2Int(x, y);

                map[x, y] = tile;
            }
        }
        MapManager.Instance.SetMap(map);
        Debug.Log("地图生成完成！");
    }

    GameObject GetRandomTile()
    {
        int rand = Random.Range(0, 100);

        if (rand < 70) return emptyTilePrefab;
        if (rand < 80) return resourceTilePrefab;
        if (rand < 90) return armyCampTilePrefab;
        if (rand < 95) return eventTilePrefab;

        return strongholdTilePrefab;
    }
}