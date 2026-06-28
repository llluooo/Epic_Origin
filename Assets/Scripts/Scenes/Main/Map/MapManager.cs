using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 地图查询辅助组件，保存生成后的格子数组并提供坐标换算。
/// </summary>
public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

    public int width = 10;
    public int height = 10;
    public float tileSize = 1.2f;
    public Vector2 worldOrigin = Vector2.zero;

    private Tile[,] map;
    public bool HasMap => map != null;

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
        if (map == null)
            return null;

        if (pos.x < 0 || pos.x >= width || pos.y < 0 || pos.y >= height)
            return null;

        return map[pos.x, pos.y];
    }

    public bool IsInBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }

    public bool IsWalkable(Vector2Int pos)
    {
        Tile tile = GetTileAt(pos);
        return tile != null && tile.IsWalkable;
    }

    public bool CanReachWithinSteps(Vector2Int start, Vector2Int target, int maxSteps)
    {
        if (maxSteps < 0 || !IsWalkable(start) || !IsWalkable(target))
        {
            return false;
        }

        if (start == target)
        {
            return true;
        }

        Queue<(Vector2Int pos, int steps)> frontier = new Queue<(Vector2Int pos, int steps)>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        frontier.Enqueue((start, 0));
        visited.Add(start);

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();
            if (current.steps >= maxSteps)
            {
                continue;
            }

            foreach (Vector2Int next in GetCardinalNeighbors(current.pos))
            {
                if (!visited.Add(next) || !IsWalkable(next))
                {
                    continue;
                }

                if (next == target)
                {
                    return true;
                }

                frontier.Enqueue((next, current.steps + 1));
            }
        }

        return false;
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

    public Rect GetMapWorldRect()
    {
        float halfTile = tileSize * 0.5f;
        return new Rect(
            worldOrigin.x - halfTile,
            worldOrigin.y - halfTile,
            width * tileSize,
            height * tileSize);
    }

    private static IEnumerable<Vector2Int> GetCardinalNeighbors(Vector2Int pos)
    {
        yield return new Vector2Int(pos.x + 1, pos.y);
        yield return new Vector2Int(pos.x - 1, pos.y);
        yield return new Vector2Int(pos.x, pos.y + 1);
        yield return new Vector2Int(pos.x, pos.y - 1);
    }
}
