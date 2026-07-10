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
/// AI 具备与玩家 Hero 对等的能力：升级据点、召唤单位、驻军管理、多步移动。
/// </summary>
public abstract class AIController
{
    protected GameManager gameManager;
    protected Player aiPlayer;
    protected MapManager mapManager;

    // 免费行动保底金储备
    protected const int GoldReserve = 30;

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

    // ================== 移动系统 ==================

    /// <summary>
    /// 获取 AI 当前在地图上的格子位置
    /// </summary>
    protected Vector2Int GetAIPosition()
    {
        return aiHero != null ? aiHero.currentGridPos : aiPlayer.strongholdPos;
    }

    /// <summary>
    /// 计算两点之间的曼哈顿距离
    /// </summary>
    protected int ManhattanDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    /// <summary>
    /// BFS 寻路：返回从 from 到 to 的完整路径（不含起点），无法到达则返回空列表
    /// </summary>
    protected List<Vector2Int> FindPathToward(Vector2Int from, Vector2Int to)
    {
        List<Vector2Int> emptyPath = new List<Vector2Int>();
        if (from == to) return emptyPath;

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> parent = new Dictionary<Vector2Int, Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        Vector2Int[] dirs = {
            new Vector2Int(1, 0), new Vector2Int(-1, 0),
            new Vector2Int(0, 1), new Vector2Int(0, -1)
        };

        queue.Enqueue(from);
        visited.Add(from);
        parent[from] = from;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            foreach (Vector2Int dir in dirs)
            {
                Vector2Int neighbor = current + dir;
                if (visited.Contains(neighbor)) continue;
                if (!mapManager.IsInBounds(neighbor)) continue;

                // 目标格子即使不可行走也允许到达（如资源点等特殊格子）
                if (neighbor != to && !mapManager.IsWalkable(neighbor)) continue;

                visited.Add(neighbor);
                parent[neighbor] = current;

                if (neighbor == to)
                {
                    // 回溯构建完整路径
                    List<Vector2Int> path = new List<Vector2Int>();
                    Vector2Int step = to;
                    while (true)
                    {
                        Vector2Int p = parent[step];
                        if (p == from) break;
                        path.Add(step);
                        step = p;
                    }
                    path.Add(step); // 第一步
                    path.Reverse();
                    return path;
                }

                queue.Enqueue(neighbor);
            }
        }

        return emptyPath;
    }

    /// <summary>
    /// 让 AI 英雄向目标移动最多 maxSteps 步（BFS 寻路，可绕过障碍），返回是否成功发起移动。
    /// 如果目标距离 ≤ maxSteps，走到目标即停；否则走满 maxSteps。
    /// </summary>
    protected bool MoveAIHeroToward(Vector2Int target, int maxSteps = 3)
    {
        if (aiHero == null || aiHero.IsMoving) return false;

        Vector2Int currentPos = aiHero.currentGridPos;
        if (currentPos == target) return false;

        List<Vector2Int> path = FindPathToward(currentPos, target);
        if (path.Count == 0) return false;

        int steps = Mathf.Min(maxSteps, path.Count);
        Vector2Int destination = path[steps - 1];

        aiHero.MoveTo(destination);
        return true;
    }

    /// <summary>
    /// 寻找距离 AI 最近的未清除的指定类型格子
    /// </summary>
    protected Vector2Int? FindNearestTarget<T>() where T : Tile
    {
        T[] tiles = Object.FindObjectsOfType<T>();
        Vector2Int aiPos = GetAIPosition();
        float minDist = float.MaxValue;
        Vector2Int? nearest = null;

        foreach (T tile in tiles)
        {
            if (tile.Cleared) continue;
            float dist = Vector2Int.Distance(tile.gridPosition, aiPos);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = tile.gridPosition;
            }
        }

        return nearest;
    }

    // ================== 据点行动 ==================

    /// <summary>
    /// 检查 AI 英雄当前是否在己方据点上
    /// </summary>
    protected bool IsAtOwnStronghold()
    {
        return GetAIPosition() == aiPlayer.strongholdPos;
    }

    public bool TryUpgradeStronghold()
    {
        if (aiPlayer == null || !aiPlayer.CanUpgrade())
            return false;

        aiPlayer.UpgradeStronghold();
        return true;
    }

    /// <summary>
    /// 智能召唤：多样性优先 → 高等级优先，保留金储备
    /// </summary>
    protected bool TrySmartSummon()
    {
        if (gameManager == null || aiPlayer == null) return false;

        int maxLevel = Mathf.Min(aiPlayer.strongholdLevel, 5);
        if (maxLevel < 1) return false;

        // 第一轮：优先召唤卡组中还没有的兵种（多样性），高等级优先
        for (int level = maxLevel; level >= 1; level--)
        {
            int unitIndex = level - 1;
            if (aiPlayer.GetOwnedCount(unitIndex) == 0)
            {
                if (TrySummonIfAffordable(unitIndex))
                    return true;
            }
        }

        // 第二轮：召唤已有的兵种（堆叠），高等级优先
        for (int level = maxLevel; level >= 1; level--)
        {
            int unitIndex = level - 1;
            if (aiPlayer.GetOwnedCount(unitIndex) > 0)
            {
                if (TrySummonIfAffordable(unitIndex))
                    return true;
            }
        }

        return false;
    }

    private bool TrySummonIfAffordable(int unitIndex)
    {
        int level = unitIndex + 1;
        if (!aiPlayer.CanSummon(level)) return false;

        ResourceData cost = aiPlayer.GetSummonCost(level);
        // 保留金储备，除非据点等级已经很高
        int reserve = aiPlayer.strongholdLevel >= 3 ? GoldReserve / 2 : GoldReserve;
        if (aiPlayer.resources.gold - cost.gold < reserve) return false;

        // 召唤前检查英雄卡槽，满了先腾位
        if (!aiPlayer.deck.CanAddCard(new Card { race = aiPlayer.race, unitIndex = unitIndex, quantity = 1 }))
        {
            TryManageGarrison();
        }

        return gameManager.SummonUnit(aiPlayer, unitIndex);
    }

    /// <summary>
    /// 驻军管理：将英雄卡组中攻击力最低的重复卡牌移入驻军，腾出空槽
    /// </summary>
    protected void TryManageGarrison()
    {
        Deck deck = aiPlayer.deck;
        if (!deck.hasSlotLimit || deck.SlotCount < Deck.HeroSlotLimit) return;

        // 找到攻击力最低的卡牌（优先移重复的、低等级的）
        int lowestIndex = -1;
        int lowestAttack = int.MaxValue;

        for (int i = 0; i < deck.CardCount; i++)
        {
            Card card = deck[i];
            if (card == null) continue;

            // 优先移 quantity > 1 的（有重复），或攻击力最低的
            int score = card.baseAttack * 100 + (card.quantity > 1 ? 0 : 1000);
            if (score < lowestAttack)
            {
                lowestAttack = score;
                lowestIndex = i;
            }
        }

        if (lowestIndex >= 0)
        {
            Card moved = deck[lowestIndex];
            aiPlayer.TransferToGarrison(lowestIndex);
            Debug.Log($"[AI] {moved.cardName} Lv{moved.level} 移入驻军腾出英雄卡槽");
        }
    }

    /// <summary>
    /// 维护据点驻兵：确保至少有 2 张驻兵卡牌
    /// </summary>
    protected void MaintainGarrison()
    {
        if (aiPlayer == null || gameManager == null) return;

        int garrisonCount = aiPlayer.garrisonDeck.CardCount;
        if (garrisonCount >= 2) return;

        // 尝试召唤直接加入驻兵
        int maxLevel = Mathf.Min(aiPlayer.strongholdLevel, 5);
        for (int level = maxLevel; level >= 1; level--)
        {
            int unitIndex = level - 1;
            if (!aiPlayer.CanSummon(level)) continue;

            ResourceData cost = aiPlayer.GetSummonCost(level);
            if (aiPlayer.resources.gold - cost.gold < GoldReserve) continue;

            aiPlayer.resources.gold -= cost.gold;
            aiPlayer.resources.buildingMaterials -= cost.buildingMaterials;
            Card card = gameManager.CreateCardForRace(aiPlayer.race, unitIndex, level);
            aiPlayer.AddToGarrison(card);
            Debug.Log($"[AI] 驻兵不足，召唤 {card.cardName} Lv{card.level} 加入驻兵（当前驻兵：{aiPlayer.garrisonDeck.CardCount}张）");
            return;
        }

        // 资源不足召唤时，从英雄卡组转移最低攻击力的卡牌到驻兵
        if (aiPlayer.deck.CardCount > 1)
        {
            int lowestIdx = -1;
            int lowestAtk = int.MaxValue;
            for (int i = 0; i < aiPlayer.deck.CardCount; i++)
            {
                Card card = aiPlayer.deck[i];
                if (card == null) continue;
                int atk = card.baseAttack * card.quantity;
                if (atk < lowestAtk)
                {
                    lowestAtk = atk;
                    lowestIdx = i;
                }
            }

            if (lowestIdx >= 0)
            {
                Card moved = aiPlayer.deck[lowestIdx];
                aiPlayer.TransferToGarrison(lowestIdx);
                Debug.Log($"[AI] 驻兵不足，从英雄卡组转移 {moved.cardName} Lv{moved.level} 到驻兵（当前驻兵：{aiPlayer.garrisonDeck.CardCount}张）");
            }
        }
    }

    /// <summary>
    /// 检查当前目标是否已失效（被清除）
    /// </summary>
    protected bool IsTargetValid(Vector2Int? targetPos)
    {
        if (!targetPos.HasValue) return false;
        Tile tile = mapManager.GetTileAt(targetPos.Value);
        return tile != null && !tile.Cleared;
    }

    /// <summary>
    /// 获取 AI 卡组的总攻击力
    /// </summary>
    protected int GetTotalAttack()
    {
        return aiPlayer.GetTotalAttack();
    }
}

/// <summary>
/// 简单 AI：收集资源 → 发展兵力 → 总攻击力 ≥ 90 时进攻玩家据点。
/// 效率较低，进攻门槛较低。
/// </summary>
public class EasyAI : AIController
{
    public EasyAI(GameManager gm, Player ai) : base(gm, ai) { }

    public override bool ExecuteTurn()
    {
        // 免费行动：只在己方据点时执行
        if (IsAtOwnStronghold())
        {
            // 升级据点（最高Lv3）
            if (aiPlayer.strongholdLevel < 3)
            {
                if (TryUpgradeStronghold())
                    Debug.Log("[EasyAI] 升级据点");
            }

            // 智能召唤
            if (TrySmartSummon())
                Debug.Log("[EasyAI] 智能召唤");

            // 维护驻兵
            MaintainGarrison();
        }

        if (aiHero == null) return false;

        int totalAttack = GetTotalAttack();

        // 进攻判断：总攻击力 ≥ 90
        if (totalAttack >= 90)
        {
            Vector2Int playerStrongholdPos = gameManager.player.strongholdPos;
            if (MoveAIHeroToward(playerStrongholdPos))
            {
                Debug.Log($"[EasyAI] 进攻模式：向玩家据点推进（总攻击力:{totalAttack}）");
                return true;
            }
        }

        // 收集资源
        Vector2Int? target = FindNearestTarget<ResourceTile>()
                         ?? FindNearestTarget<ArmyCampTile>()
                         ?? FindNearestTarget<EventTile>();
        if (target.HasValue && MoveAIHeroToward(target.Value))
        {
            Debug.Log($"[EasyAI] 向目标 {target.Value} 移动");
            return true;
        }

        // 无目标，向玩家据点推进（用尽移动力）
        Vector2Int playerPos = gameManager.player.strongholdPos;
        if (MoveAIHeroToward(playerPos))
        {
            Debug.Log("[EasyAI] 无目标，向玩家据点移动");
            return true;
        }

        return false;
    }
}

/// <summary>
/// 困难 AI：收集资源 → 积攒兵力 → 总攻击力 ≥ 150 时全力进攻玩家据点。
/// 效率高，准备充分才进攻，有目标记忆。
/// </summary>
public class HardAI : AIController
{
    private Vector2Int? currentTarget = null;
    private bool isAttacking = false;

    public HardAI(GameManager gm, Player ai) : base(gm, ai) { }

    public override bool ExecuteTurn()
    {
        // 免费行动：只在己方据点时执行
        if (IsAtOwnStronghold())
        {
            // 升级据点（最高Lv5）
            if (aiPlayer.strongholdLevel < 5)
            {
                if (TryUpgradeStronghold())
                    Debug.Log($"[HardAI] 升级据点到等级 {aiPlayer.strongholdLevel}");
            }

            // 智能召唤（尝试2次）
            for (int attempt = 0; attempt < 2; attempt++)
            {
                if (!TrySmartSummon()) break;
                Debug.Log($"[HardAI] 智能召唤（第{attempt + 1}次）");
            }

            // 维护驻兵
            MaintainGarrison();
        }

        if (aiHero == null) return false;

        Vector2Int aiPos = GetAIPosition();
        int totalAttack = GetTotalAttack();

        // 检查当前目标是否失效
        if (currentTarget.HasValue && !IsTargetValid(currentTarget))
        {
            Debug.Log($"[HardAI] 目标 {currentTarget} 已失效，重新搜索");
            currentTarget = null;
        }

        // 到达目标则清除
        if (currentTarget.HasValue && ManhattanDistance(aiPos, currentTarget.Value) == 0)
        {
            currentTarget = null;
        }

        // 进攻判断：总攻击力 ≥ 150
        if (totalAttack >= 150)
        {
            isAttacking = true;
            currentTarget = gameManager.player.strongholdPos;
        }

        // 进攻模式
        if (isAttacking)
        {
            Debug.Log($"[HardAI] 进攻模式：向玩家据点推进（总攻击力:{totalAttack}）");
            if (MoveAIHeroToward(gameManager.player.strongholdPos))
                return true;
        }

        // 收集资源模式
        if (!currentTarget.HasValue)
        {
            currentTarget = FindNearestTarget<ResourceTile>()
                         ?? FindNearestTarget<ArmyCampTile>()
                         ?? FindNearestTarget<EventTile>();
        }

        if (currentTarget.HasValue && ManhattanDistance(aiPos, currentTarget.Value) > 0)
        {
            Debug.Log($"[HardAI] 向 {currentTarget} 移动");
            if (MoveAIHeroToward(currentTarget.Value))
                return true;

            // 不可达，切换目标
            currentTarget = FindNearestTarget<ArmyCampTile>();
            if (currentTarget.HasValue && MoveAIHeroToward(currentTarget.Value))
            {
                Debug.Log($"[HardAI] 切换目标到兵营 {currentTarget}");
                return true;
            }
            currentTarget = FindNearestTarget<EventTile>();
            if (currentTarget.HasValue && MoveAIHeroToward(currentTarget.Value))
            {
                Debug.Log($"[HardAI] 切换目标到事件 {currentTarget}");
                return true;
            }
            currentTarget = null;
        }

        // 无目标，向玩家据点推进（用尽移动力）
        Vector2Int playerPos = gameManager.player.strongholdPos;
        if (MoveAIHeroToward(playerPos))
        {
            Debug.Log("[HardAI] 无目标，向玩家据点移动");
            return true;
        }

        return false;
    }
}
