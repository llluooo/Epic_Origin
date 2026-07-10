using UnityEngine;
using Action = System.Action;

/// <summary>
/// AI 英雄控制器，由 AIController 驱动移动，在地图上提供 AI 的视觉表现。
/// </summary>
public class AIHero : MonoBehaviour
{
    public Vector2Int currentGridPos;
    public float moveSpeed = 1.5f;
    public Vector3 visualOffset = new Vector3(0, -0.35f, 0);

    private bool isMoving = false;
    private Vector3 targetPos;
    private HeroWalkAnimator walkAnimator;

    /// <summary>
    /// 移动完成后的回调，通知 GameManager AI 英雄已到达目标。
    /// </summary>
    public event Action OnMoveComplete;

    /// <summary>
    /// AI 英雄是否正在移动中。
    /// </summary>
    public bool IsMoving => isMoving;

    private void Awake()
    {
        walkAnimator = GetComponent<HeroWalkAnimator>();
    }

    public void SetRaceAppearance(RaceType race)
    {
        if (walkAnimator == null)
        {
            walkAnimator = GetComponent<HeroWalkAnimator>();
        }

        if (walkAnimator != null)
        {
            walkAnimator.SetRace(race);
        }
    }

    void Update()
    {
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPos) < 0.01f)
            {
                transform.position = targetPos;
                isMoving = false;

                if (walkAnimator != null)
                {
                    walkAnimator.StopWalking();
                }

                Tile tile = MapManager.Instance.GetTileAt(currentGridPos);
                if (tile != null)
                {
                    OnAIEnterTile(tile);
                }

                OnMoveComplete?.Invoke();
            }
        }
    }

    /// <summary>
    /// AI 英雄移动到指定格子坐标。由 AIController 调用。
    /// </summary>
    public void MoveTo(Vector2Int targetGridPos)
    {
        if (isMoving) return;

        if (targetGridPos == currentGridPos) return;

        Vector2Int movementDelta = targetGridPos - currentGridPos;
        currentGridPos = targetGridPos;
        targetPos = MapManager.Instance.GridToWorld(targetGridPos) + visualOffset;
        isMoving = true;

        if (walkAnimator != null)
        {
            walkAnimator.StartWalking(new Vector2(movementDelta.x, movementDelta.y));
        }

        Debug.Log($"[AIHero] 移动到格子 {targetGridPos}");
    }

    /// <summary>
    /// 直接设置位置（不播放移动动画），用于初始化或存档恢复。
    /// </summary>
    public void SetPosition(Vector2Int gridPos, Vector3 worldPos)
    {
        currentGridPos = gridPos;
        transform.position = worldPos + visualOffset;
        isMoving = false;

        if (walkAnimator != null)
        {
            walkAnimator.StopWalking();
        }
    }

    /// <summary>
    /// AI 英雄进入格子时的交互逻辑。
    /// </summary>
    private void OnAIEnterTile(Tile tile)
    {
        if (tile.Cleared) return;

        if (tile is ResourceTile resourceTile)
        {
            // AI 和玩家使用完全相同的资源获取逻辑
            resourceTile.OnHeroEnter();
        }
        else if (tile is ArmyCampTile armyCamp)
        {
            bool won = AIBattleSimulator.SimulateArmyCampBattle(
                GameManager.Instance.aiPlayer,
                GameManager.Instance.currentTurn
            );
            Debug.Log($"[AIHero] 兵营战斗{(won ? "胜利" : "失败")}");
            if (won)
                tile.MarkCleared();
        }
        else if (tile is EventTile eventTile)
        {
            float roll = Random.value;
            if (roll < 0.4f)
            {
                GameManager.Instance.aiPlayer.resources.gold += Random.Range(3, 9);
                Debug.Log("[AIHero] 事件：获得资源");
            }
            else if (roll < 0.7f)
            {
                GameManager.Instance.aiPlayer.resources.gold -= Random.Range(2, 6);
                Debug.Log("[AIHero] 事件：损失金币");
            }
            tile.MarkCleared();
        }
        else if (tile is StrongholdTile stronghold)
        {
            if (stronghold.strongholdType == StrongholdType.Player)
            {
                Debug.Log("[AIHero] 进入玩家据点，发起进攻！");
                GameManager.Instance.OnAIHeroEnterPlayerStronghold();
            }
        }
    }
}
