using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class MapNavigationTests
{
    private GameObject managerObject;
    private MapManager mapManager;
    private readonly List<GameObject> createdTileObjects = new List<GameObject>();

    [SetUp]
    public void SetUp()
    {
        managerObject = new GameObject("MapManager Test Host");
        mapManager = managerObject.AddComponent<MapManager>();
    }

    [TearDown]
    public void TearDown()
    {
        MapManager.Instance = null;
        foreach (GameObject tileObject in createdTileObjects)
        {
            Object.DestroyImmediate(tileObject);
        }

        createdTileObjects.Clear();
        Object.DestroyImmediate(managerObject);
    }

    [Test]
    public void IsWalkable_returns_false_for_obstacles_and_out_of_bounds()
    {
        Tile[,] map = CreateMap(createdTileObjects, 3, 3, new Vector2Int(1, 1));
        mapManager.SetMap(map, new Vector2(-1.2f, -1.2f), 1.2f);

        Assert.IsTrue(mapManager.IsWalkable(new Vector2Int(0, 0)));
        Assert.IsFalse(mapManager.IsWalkable(new Vector2Int(1, 1)));
        Assert.IsFalse(mapManager.IsWalkable(new Vector2Int(-1, 0)));
        Assert.IsFalse(mapManager.IsWalkable(new Vector2Int(3, 0)));
    }

    [Test]
    public void CanReachWithinSteps_uses_cardinal_paths_and_does_not_cross_obstacles()
    {
        Tile[,] map = CreateMap(
            createdTileObjects,
            4,
            3,
            new Vector2Int(1, 0),
            new Vector2Int(1, 1),
            new Vector2Int(1, 2));

        mapManager.SetMap(map, Vector2.zero, 1.2f);

        Assert.IsFalse(mapManager.CanReachWithinSteps(new Vector2Int(0, 1), new Vector2Int(2, 1), 3));
        Assert.IsTrue(mapManager.CanReachWithinSteps(new Vector2Int(0, 1), new Vector2Int(0, 2), 1));
    }

    [Test]
    public void GetMapWorldRect_returns_playable_area_with_half_tile_padding()
    {
        Tile[,] map = CreateMap(createdTileObjects, 4, 2);
        mapManager.SetMap(map, new Vector2(10f, 20f), 2f);

        Rect bounds = mapManager.GetMapWorldRect();

        Assert.AreEqual(9f, bounds.xMin);
        Assert.AreEqual(19f, bounds.yMin);
        Assert.AreEqual(8f, bounds.width);
        Assert.AreEqual(4f, bounds.height);
    }

    [Test]
    public void CameraFollow_clamps_orthographic_camera_inside_bounds()
    {
        GameObject cameraObject = new GameObject("Camera");
        createdTileObjects.Add(cameraObject);
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 2f;
        camera.aspect = 1f;

        CameraFollow follow = cameraObject.AddComponent<CameraFollow>();
        follow.clampToMapBounds = true;
        follow.autoUseMapBounds = false;
        follow.SetBounds(new Rect(0f, 0f, 10f, 8f));

        Vector3 clamped = follow.ClampPositionToBounds(new Vector3(-50f, 50f, -10f));

        Assert.AreEqual(2f, clamped.x);
        Assert.AreEqual(6f, clamped.y);
        Assert.AreEqual(-10f, clamped.z);
    }

    private static Tile[,] CreateMap(List<GameObject> createdObjects, int width, int height, params Vector2Int[] obstacles)
    {
        Tile[,] map = new Tile[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                bool blocked = false;
                foreach (Vector2Int obstacle in obstacles)
                {
                    if (obstacle.x == x && obstacle.y == y)
                    {
                        blocked = true;
                        break;
                    }
                }

                GameObject tileObject = new GameObject(blocked ? "Obstacle" : "Empty");
                createdObjects.Add(tileObject);
                Tile tile = blocked
                    ? tileObject.AddComponent<ObstacleTile>()
                    : tileObject.AddComponent<EmptyTile>();

                tile.gridPosition = new Vector2Int(x, y);
                map[x, y] = tile;
            }
        }

        return map;
    }
}
