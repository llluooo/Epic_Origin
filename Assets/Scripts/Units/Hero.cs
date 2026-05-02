using UnityEngine;

/// <summary>
/// 英雄控制（玩家单位）
/// </summary>
public class Hero : MonoBehaviour
{
    public Vector2Int currentGridPos;   // 当前格子坐标
    public float moveSpeed = 5f;

    private bool isMoving = false;
    private Vector3 targetPos;

    void Update()
    {
        // 平滑移动
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPos) < 0.01f)
            {
                transform.position = targetPos;
                isMoving = false;

                // 到达后触发格子事件
                Tile tile = MapManager.Instance.GetTileAt(currentGridPos);
                tile.OnHeroEnter();
            }
        }
    }

    /// <summary>
    /// 尝试移动到目标格子
    /// </summary>
    public void TryMove(Vector2Int targetGridPos)
    {
        // ❌ 如果不是玩家回合，禁止操作
        if (!GameManager.Instance.isPlayerTurn)
        {
            Debug.Log("现在不是你的回合！");
            return;
        }

        // ❌ 如果已经操作过，禁止再次操作
        if (GameManager.Instance.hasPlayerActed)
        {
            Debug.Log("本回合你已经行动过了！");
            return;
        }

        if (isMoving) return;

        // 曼哈顿距离限制（最多3格）
        int distance = Mathf.Abs(targetGridPos.x - currentGridPos.x) +
                       Mathf.Abs(targetGridPos.y - currentGridPos.y);

        if (distance > 3)
        {
            Debug.Log("超出移动范围！");
            return;
        }

        // 开始移动
        currentGridPos = targetGridPos;
        targetPos = MapManager.Instance.GridToWorld(targetGridPos);
        isMoving = true;

        // ⭐ 关键：记录“已经行动”
        GameManager.Instance.OnPlayerAction();
    }
}