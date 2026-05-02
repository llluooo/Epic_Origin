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
        if (isMoving) return;

        // 限制：最多3格（曼哈顿距离）
        int distance = Mathf.Abs(targetGridPos.x - currentGridPos.x) +
                       Mathf.Abs(targetGridPos.y - currentGridPos.y);

        if (distance > 3)
        {
            Debug.Log("超出移动范围！");
            return;
        }

        // 设置目标位置
        currentGridPos = targetGridPos;
        targetPos = MapManager.Instance.GridToWorld(targetGridPos);
        isMoving = true;
    }
}