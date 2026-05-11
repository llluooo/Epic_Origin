using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 英雄控制（玩家单位）
/// </summary>
public class Hero : MonoBehaviour
{
    public Vector2Int currentGridPos;
    public float moveSpeed = 5f;

    private bool isMoving = false;
    private Vector3 targetPos;

    private static readonly Dictionary<RaceType, Color> RaceColors = new()
    {
        { RaceType.Human, new Color(0.3f, 0.5f, 1f) },
        { RaceType.Heaven, new Color(1f, 0.85f, 0.3f) },
        { RaceType.Ghost, new Color(0.6f, 0.3f, 0.8f) },
    };

    public void SetRaceAppearance(RaceType race)
    {
        if (RaceColors.TryGetValue(race, out var color))
            GetComponent<SpriteRenderer>().color = color;
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
        if (!GameManager.Instance.isPlayerTurn)
        {
            Debug.Log("现在不是你的回合！");
            return;
        }

        if (isMoving) return;

        int distance = Mathf.Abs(targetGridPos.x - currentGridPos.x) +
                       Mathf.Abs(targetGridPos.y - currentGridPos.y);

        if (distance > 3)
        {
            Debug.Log("超出移动范围！");
            return;
        }

        // 站在当前格子上点自己 → 触发交互但不消耗行动
        if (distance == 0)
        {
            Tile tile = MapManager.Instance.GetTileAt(currentGridPos);
            tile.OnHeroEnter();
            return;
        }

        // 如果已经操作过，禁止移动
        if (GameManager.Instance.hasPlayerActed)
        {
            Debug.Log("本回合你已经行动过了！");
            return;
        }

        // 开始移动
        currentGridPos = targetGridPos;
        targetPos = MapManager.Instance.GridToWorld(targetGridPos);
        isMoving = true;

        GameManager.Instance.OnPlayerAction();
    }
}
