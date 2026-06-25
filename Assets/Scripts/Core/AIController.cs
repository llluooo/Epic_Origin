using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// AI 难度枚举
/// </summary>
public enum AIDifficulty
{
    Easy,
    Hard
}

/// <summary>
/// AI 控制器基类，驱动 AIHero 在地图上移动和行动。
/// </summary>
public abstract class AIController
{
    protected GameManager gameManager;
    protected Player aiPlayer;
    protected MapManager mapManager;

    public AIController(GameManager gm, Player ai)
    {
        gameManager = gm;
        aiPlayer = ai;
        mapManager = MapManager.Instance;
    }

    protected AIHero aiHero => gameManager.aiHero;

    /// <summary>
    /// 执行 AI 回合，返回该回合是否采取了行动
    /// </summary>
    public abstract bool ExecuteTurn();

    /// <summary>
    /// 获取 AI 当前在地图上的格子位置
    /// </summary>
    protected Vector2Int GetAIPosition()
    {
        return aiHero != null ? aiHero.currentGridPos : aiPlayer.strongholdPos;
    }

    /// <summary>
    /// 寻找距离 AI 最近的指定类型格子
    /// </summary>
    protected Vector2Int? FindNearestTarget<T>() where T : Tile
    {
        T[] tiles = Object.FindObjectsOfType<T>();
        Vector2Int aiPos = GetAIPosition();
        float minDist = float.MaxValue;
        Vector2Int? nearest = null;

        foreach (T tile in tiles)
        {
            float dist = Vector2Int.Distance(tile.gridPosition, aiPos);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = tile.gridPosition;
            }
        }

        return nearest;
    }

    /// <summary>
    /// 计算两点之间的曼哈顿距离
    /// </summary>
    protected int ManhattanDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    /// <summary>
    /// 简单寻路：向目标移动一步，返回下一步坐标
    /// </summary>
    protected Vector2Int MoveTowardTarget(Vector2Int from, Vector2Int target)
    {
        Vector2Int move = from;

        if (from.x < target.x) move.x++;
        else if (from.x > target.x) move.x--;

        if (from.y < target.y) move.y++;
        else if (from.y > target.y) move.y--;

        return move;
    }

    /// <summary>
    /// 让 AI 英雄向目标移动一步，返回是否成功发起移动
    /// </summary>
    protected bool MoveAIHeroToward(Vector2Int target)
    {
        if (aiHero == null || aiHero.IsMoving) return false;

        Vector2Int currentPos = aiHero.currentGridPos;
        if (currentPos == target) return false;

        Vector2Int nextPos = MoveTowardTarget(currentPos, target);

        if (!mapManager.IsWalkable(nextPos))
        {
            Vector2Int altPos = TryAlternativeStep(currentPos, target);
            if (altPos != currentPos)
            {
                nextPos = altPos;
            }
            else
            {
                return false;
            }
        }

        aiHero.MoveTo(nextPos);
        return true;
    }

    /// <summary>
    /// 当直线路径被阻挡时，尝试只走 x 或 y 方向的一步
    /// </summary>
    private Vector2Int TryAlternativeStep(Vector2Int from, Vector2Int target)
    {
        if (from.x != target.x)
        {
            Vector2Int xStep = new Vector2Int(from.x + (target.x > from.x ? 1 : -1), from.y);
            if (mapManager.IsWalkable(xStep)) return xStep;
        }

        if (from.y != target.y)
        {
            Vector2Int yStep = new Vector2Int(from.x, from.y + (target.y > from.y ? 1 : -1));
            if (mapManager.IsWalkable(yStep)) return yStep;
        }

        return from;
    }
}

/// <summary>
/// 简单 AI：偶尔移动，优先升级和召唤
/// </summary>
public class EasyAI : AIController
{
    public EasyAI(GameManager gm, Player ai) : base(gm, ai) { }

    public override bool ExecuteTurn()
    {
        // 50% 概率尝试随机移动一步
        if (Random.value < 0.5f && aiHero != null)
        {
            Vector2Int current = aiHero.currentGridPos;
            Vector2Int[] dirs = {
                new Vector2Int(1, 0), new Vector2Int(-1, 0),
                new Vector2Int(0, 1), new Vector2Int(0, -1)
            };
            Shuffle(dirs);
            foreach (Vector2Int dir in dirs)
            {
                Vector2Int candidate = current + dir;
                if (mapManager.IsInBounds(candidate) && mapManager.IsWalkable(candidate))
                {
                    aiHero.MoveTo(candidate);
                    Debug.Log("[EasyAI] 随机移动");
                    gameManager.ShowAILog("AI 随机移动");
                    return true;
                }
            }
        }

        if (aiPlayer.resources.gold < 100)
        {
            Debug.Log("[EasyAI] 资源不足，等待");
            return false;
        }

        if (aiPlayer.strongholdLevel < 3)
        {
            if (CanUpgradeStronghold())
            {
                UpgradeStronghold();
                Debug.Log("[EasyAI] 升级据点");
                gameManager.ShowAILog($"AI 升级据点到 Lv{aiPlayer.strongholdLevel}");
                return true;
            }
        }

        if (CanSummonUnit())
        {
            SummonUnit();
            Debug.Log("[EasyAI] 召唤单位");
            gameManager.ShowAILog("AI 召唤了单位");
            return true;
        }

        return false;
    }

    private bool CanUpgradeStronghold()
    {
        int upgradeCost = (int)(80 * Mathf.Pow(aiPlayer.strongholdLevel + 1, 1.5f));
        return aiPlayer.resources.buildingMaterials >= upgradeCost && aiPlayer.strongholdLevel < 5;
    }

    private void UpgradeStronghold()
    {
        int upgradeCost = (int)(80 * Mathf.Pow(aiPlayer.strongholdLevel + 1, 1.5f));
        aiPlayer.resources.Subtract(new ResourceData(0, upgradeCost));
        aiPlayer.strongholdLevel++;
    }

    private bool CanSummonUnit()
    {
        int unitLevel = Mathf.Min(aiPlayer.strongholdLevel, 3);
        int summonCost = (int)(30 * GetLevelMultiplier(unitLevel));
        return aiPlayer.resources.gold >= summonCost;
    }

    private void SummonUnit()
    {
        int unitLevel = Mathf.Min(aiPlayer.strongholdLevel, 5);
        int unitIndex = Mathf.Min(unitLevel - 1, 4);
        Card card = gameManager.CreateCardForRace(aiPlayer.race, unitIndex, unitLevel);
        if (card != null)
        {
            aiPlayer.deck.AddCard(card);
            int summonCost = (int)(30 * GetLevelMultiplier(unitLevel));
            aiPlayer.resources.Subtract(new ResourceData(summonCost, 0));
        }
    }

    private float GetLevelMultiplier(int level)
    {
        return level switch
        {
            1 => 1.0f,
            2 => 1.5f,
            3 => 2.5f,
            4 => 4.0f,
            5 => 6.5f,
            _ => 1.0f
        };
    }

    private static void Shuffle(Vector2Int[] arr)
    {
        for (int i = arr.Length - 1; i > 0; i--)
        {
            int r = Random.Range(0, i + 1);
            (arr[i], arr[r]) = (arr[r], arr[i]);
        }
    }
}

/// <summary>
/// 困难 AI：主动寻路，策略性行动
/// </summary>
public class HardAI : AIController
{
    private Vector2Int? currentTarget = null;

    public HardAI(GameManager gm, Player ai) : base(gm, ai) { }

    public override bool ExecuteTurn()
    {
        Vector2Int aiPos = GetAIPosition();

        // 检查是否到达目标
        if (currentTarget.HasValue && ManhattanDistance(aiPos, currentTarget.Value) == 0)
        {
            currentTarget = null;
        }

        // 优先升级据点
        if (aiPlayer.strongholdLevel < 5)
        {
            int upgradeCost = (int)(80 * Mathf.Pow(aiPlayer.strongholdLevel + 1, 1.5f));
            if (aiPlayer.resources.buildingMaterials >= upgradeCost)
            {
                aiPlayer.resources.Subtract(new ResourceData(0, upgradeCost));
                aiPlayer.strongholdLevel++;
                Debug.Log($"[HardAI] 升级据点到等级 {aiPlayer.strongholdLevel}");
                gameManager.ShowAILog($"AI 升级据点到 Lv{aiPlayer.strongholdLevel}");
                return true;
            }
        }

        // 积极召唤单位
        int maxUnitLevel = Mathf.Min(aiPlayer.strongholdLevel, 5);
        for (int level = maxUnitLevel; level >= 1; level--)
        {
            int summonCost = (int)(30 * GetLevelMultiplier(level));
            if (aiPlayer.resources.gold >= summonCost && Random.value < 0.7f)
            {
                int unitIndex = Mathf.Min(level - 1, 4);
                Card card = gameManager.CreateCardForRace(aiPlayer.race, unitIndex, level);
                if (card != null)
                {
                    aiPlayer.deck.AddCard(card);
                    aiPlayer.resources.Subtract(new ResourceData(summonCost, 0));
                    Debug.Log($"[HardAI] 召唤等级 {level} 单位");
                    gameManager.ShowAILog($"AI 召唤了 Lv{level} 单位");
                    return true;
                }
            }
        }

        // 寻找最近的资源点作为目标
        if (!currentTarget.HasValue)
        {
            currentTarget = FindNearestTarget<ResourceTile>();
        }

        // 向目标移动
        if (currentTarget.HasValue && ManhattanDistance(aiPos, currentTarget.Value) > 0)
        {
            Debug.Log($"[HardAI] 向 {currentTarget} 移动");
            if (MoveAIHeroToward(currentTarget.Value))
            {
                gameManager.ShowAILog("AI 向资源点移动");
                return true;
            }
        }

        return false;
    }

    private float GetLevelMultiplier(int level)
    {
        return level switch
        {
            1 => 1.0f,
            2 => 1.5f,
            3 => 2.5f,
            4 => 4.0f,
            5 => 6.5f,
            _ => 1.0f
        };
    }
}
