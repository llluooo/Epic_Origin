using UnityEngine;

/// <summary>
/// Map query helper for generated grid tiles.
/// </summary>
public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

    public int width = 10;
    public int height = 10;
    public float tileSize = 1.2f;
    public Vector2 worldOrigin = Vector2.zero;

    private Tile[,] map;

    private void Awake()
    {
        Instance = this;
    }

    public void SetMap(Tile[,] generatedMap)
    {
        SetMap(generatedMap, worldOrigin, tileSize);
    }

    public void SetMap(Tile[,] generatedMap, Vector2 generatedWorldOrigin, float generatedTileSize)
    {
        map = generatedMap;
        width = generatedMap.GetLength(0);
        height = generatedMap.GetLength(1);
        worldOrigin = generatedWorldOrigin;
        tileSize = generatedTileSize;
    }

    public Tile GetTileAt(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= width || pos.y < 0 || pos.y >= height)
            return null;

        return map[pos.x, pos.y];
    }

    public Vector3 GridToWorld(Vector2Int pos)
    {
        return new Vector3(worldOrigin.x + pos.x * tileSize, worldOrigin.y + pos.y * tileSize, 0);
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt((worldPos.x - worldOrigin.x) / tileSize);
        int y = Mathf.RoundToInt((worldPos.y - worldOrigin.y) / tileSize);

        return new Vector2Int(x, y);
    }
}
