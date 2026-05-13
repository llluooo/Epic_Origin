using UnityEngine;

/// <summary>
/// 地图管理器（提供查询功能）
/// </summary>
public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

    public int width = 10;
    public int height = 10;
    public float tileSize = 1.2f;

    private Tile[,] map;

    private void Awake()
    {
        Instance = this;
    }

    public void SetMap(Tile[,] generatedMap)
    {
        map = generatedMap;
    }

    public Tile GetTileAt(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= width || pos.y < 0 || pos.y >= height)
            return null;

        return map[pos.x, pos.y];
    }

    public Vector3 GridToWorld(Vector2Int pos)
    {
        return new Vector3(pos.x * tileSize, pos.y * tileSize, 0);
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt(worldPos.x / tileSize);
        int y = Mathf.RoundToInt(worldPos.y / tileSize);

        return new Vector2Int(x, y);
    }
}